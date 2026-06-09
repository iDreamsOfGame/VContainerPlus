using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace VContainer.Tests
{
    [TestFixture]
    public class ScopedContainerTest
    {
        [Test]
        public void CreateScopeWithResolveSingleton()
        {
            var builder = new ContainerBuilder();
            builder.Register<DisposableServiceA>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();

            var container = builder.Build();
            var scopedContainer = container.CreateScope(childBuilder =>
            {
                childBuilder.Register<DisposableServiceB>(Lifetime.Singleton)
                    .AsImplementedInterfaces()
                    .AsSelf();
            });

            var singleton = container.Resolve<DisposableServiceA>();
            var singleton2 = scopedContainer.Resolve<DisposableServiceA>();
            Assert.That(singleton, Is.InstanceOf<DisposableServiceA>());
            Assert.That(singleton2, Is.InstanceOf<DisposableServiceA>());
            Assert.That(singleton, Is.EqualTo(singleton2));
        }

        [Test]
        public void CreateScopeWithRegisterOnlyInterfaces()
        {
            var builder = new ContainerBuilder();
            var container = builder.Build();
            var child = container.CreateScope(childBuilder =>
            {
                childBuilder.Register<NoDependencyServiceA>(Lifetime.Singleton).AsImplementedInterfaces();
                childBuilder.Register<ServiceA>(Lifetime.Singleton).AsImplementedInterfaces();
            });

            var singletonAsInterface = child.Resolve<I4>();
            Assert.That(singletonAsInterface, Is.InstanceOf<ServiceA>());
        }

        [Test]
        public void ResolveFromParent()
        {
            var builder = new ContainerBuilder();
            builder.Register<I2, NoDependencyServiceA>(Lifetime.Singleton);
            builder.Register<I3, NoDependencyServiceB>(Lifetime.Singleton);
            var parentContainer = builder.Build();

            var childContainer = parentContainer.CreateScope(childBuilder =>
            {
                // childBuilder.Register<ServiceA>(Lifetime.Singleton);
                childBuilder.Register<ServiceB>(Lifetime.Scoped);
            });

            //var singletonService = childContainer.Resolve<ServiceA>();
            var scopedService = childContainer.Resolve<ServiceB>();
            //Assert.That(singletonService, Is.InstanceOf<ServiceA>());
            Assert.That(scopedService, Is.InstanceOf<ServiceB>());
        }

        [Test]
        public void ResolveCollectionFromParent()
        {
            var builder = new ContainerBuilder();
            builder.Register<I1, MultipleInterfaceServiceA>(Lifetime.Scoped);
            builder.Register<I1, MultipleInterfaceServiceB>(Lifetime.Scoped);
            var parentContainer = builder.Build();

            var childContainer = parentContainer.CreateScope(childBuilder =>
            {
                childBuilder.Register<I1CollectionService>(Lifetime.Scoped);
            });

            var scopedService = childContainer.Resolve<I1CollectionService>();
            Assert.That(scopedService.enumerable.Count(), Is.EqualTo(2));
        }

        [Test]
        public void ResolveCollectionFromParentByContinuous()
        {
            var builder = new ContainerBuilder();
            builder.Register<I1, MultipleInterfaceServiceA>(Lifetime.Singleton);
            builder.Register<I1, MultipleInterfaceServiceB>(Lifetime.Singleton);
            var parentContainer = builder.Build();

            var childContainer = parentContainer.CreateScope(childBuilder =>
            {
                childBuilder.Register<I1, MultipleInterfaceServiceC>(Lifetime.Singleton);
                childBuilder.Register<I1, MultipleInterfaceServiceD>(Lifetime.Singleton);
            });

            var scopedService = childContainer.Resolve<IReadOnlyList<I1>>();
            var moreScopedService = childContainer.Resolve<IReadOnlyList<I1>>();
            Assert.That(moreScopedService.Count(), Is.EqualTo(4));
        }

        [Test]
        public void ResolveCollection_ElementRepeatedly()
        {
            HasInstanceId.ResetId();

            var parentBuilder = new ContainerBuilder();
            parentBuilder.Register<HasInstanceId>(Lifetime.Singleton);

            var parentContainer = parentBuilder.Build();

            var childContainer = parentContainer.CreateScope(childBuilder =>
            {
                childBuilder.Register<HasInstanceId>(Lifetime.Singleton);
            });

            var a1 = parentContainer.Resolve<HasInstanceId>();
            Assert.That(a1.Id, Is.EqualTo(1));

            var collection = childContainer.Resolve<IReadOnlyList<HasInstanceId>>();
            Assert.That(collection.Count, Is.EqualTo(2));
            Assert.That(collection.Any(x => x.Id == 1), Is.True);
        }

        [Test]
        public void ResolveCollectionByLazyInstance()
        {
            var builder = new ContainerBuilder();
            builder.Register<I1, MultipleInterfaceServiceA>(Lifetime.Scoped);
            builder.Register<I1, MultipleInterfaceServiceB>(Lifetime.Scoped);
            builder.Register<I1CollectionService>(Lifetime.Scoped);

            var parentContainer = builder.Build();

            var childContainer = parentContainer.CreateScope(childBuilder =>
            {
            });

            var scopedService = childContainer.Resolve<I1CollectionService>();
            Assert.That(scopedService.enumerable.Count(), Is.EqualTo(2));
        }

        public class I1CollectionService
        {
            public readonly IReadOnlyList<I1> enumerable;

            public I1CollectionService(IReadOnlyList<I1> enumerable)
            {
                this.enumerable = enumerable;
            }
        }

        [Test]
        public void Inject()
        {
            var builder = new ContainerBuilder();
            var container = builder.Build();
            var childContainer = container.CreateScope(childBuilder =>
            {
                childBuilder.Register<I2, NoDependencyServiceA>(Lifetime.Singleton);
            });

            var methodInjectable = new HasMethodInjection();
            Assert.That(methodInjectable.Service2, Is.Null);

            childContainer.Inject(methodInjectable);
            Assert.That(methodInjectable.Service2, Is.InstanceOf<I2>());

            var noInjectable = new NoDependencyServiceA();
            Assert.DoesNotThrow(() => childContainer.Inject(noInjectable));

            var ctorInjectable = new ServiceA(new NoDependencyServiceA());
            Assert.DoesNotThrow(() => childContainer.Inject(ctorInjectable));
        }


        [Test]
        public void InstanceRegistrationDoesNotDisposal()
        {
            var instance1 = new DisposableServiceA();

            var builder = new ContainerBuilder();
            builder.RegisterInstance(instance1);

            var container = builder.Build();
            var childContainer = container.CreateScope();

            var resolveFromParent = container.Resolve<DisposableServiceA>();
            var resolveFromChild = childContainer.Resolve<DisposableServiceA>();

            Assert.That(resolveFromParent, Is.InstanceOf<DisposableServiceA>());
            Assert.That(resolveFromChild, Is.InstanceOf<DisposableServiceA>());
            Assert.That(resolveFromParent, Is.EqualTo(resolveFromChild));

            childContainer.Dispose();
            Assert.That(resolveFromParent.Disposed, Is.False);

            container.Dispose();
            Assert.That(resolveFromParent.Disposed, Is.False);
        }
    }
}
