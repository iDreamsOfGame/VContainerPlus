using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using VContainer.Internal;

namespace VContainer
{
    public partial interface IObjectResolver : IDisposable
    {
        object ApplicationOrigin { get; }
        
        /// <summary>
        /// Resolve from type with or without key
        /// </summary>
        /// <remarks>
        /// This version of resolve looks for all of scopes
        /// </remarks>
        object Resolve(Type type, object key = null);

        /// <summary>
        /// Try resolve from type with or without key
        /// </summary>
        /// <remarks>
        /// This version of resolve looks for all of scopes
        /// </remarks>
        /// <returns>Successfully resolved</returns>
        bool TryResolve(Type type, out object resolved, object key = null);

        /// <summary>
        /// Resolve from meta with registration
        /// </summary>
        /// <remarks>
        /// This version of resolve will look for instances from only the registration information already founds.
        /// </remarks>
        object Resolve(Registration registration);

        IScopedObjectResolver CreateScope(Action<IContainerBuilder> installation = null);

        void Inject(object instance);
        bool TryGetRegistration(Type type, out Registration registration, object key = null);
    }

    public interface IScopedObjectResolver : IObjectResolver
    {
        IObjectResolver Root { get; }
        IScopedObjectResolver Parent { get; set; }
    }

    public enum Lifetime
    {
        Transient,
        Singleton,
        Scoped
    }

    public sealed partial class ScopedContainer : IScopedObjectResolver
    {
        public IObjectResolver Root { get; }
        public IScopedObjectResolver Parent { get; set; }
        public object ApplicationOrigin { get; }

        readonly Registry registry;
        readonly Dictionary<Registration, object> sharedInstances;

        internal ScopedContainer(
            Registry registry,
            IObjectResolver root,
            IScopedObjectResolver parent = null,
            object applicationOrigin = null,
            Dictionary<Registration, object> sharedInstances = null)
        {
            Root = root;
            Parent = parent;
            ApplicationOrigin = applicationOrigin;
            this.registry = registry;
            this.sharedInstances = sharedInstances ?? new Dictionary<Registration, object>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Resolve(Type type, object key = null)
        {
            if (TryFindRegistration(type, key, out var registration))
            {
                return Resolve(registration);
            }
            
            if (ThrowIfUnresolved)
                throw new VContainerException(type, $"No such registration of type: {type}{(key == null ? string.Empty : $" with Key: {key}")}");

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryResolve(Type type, out object resolved, object key = null)
        {
            if (TryFindRegistration(type, key, out var registration))
            {
                resolved = Resolve(registration);
                return true;
            }

            resolved = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Resolve(Registration registration) => ResolveCore(registration);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IScopedObjectResolver CreateScope(Action<IContainerBuilder> installation = null)
        {
            var containerBuilder = new ScopedContainerBuilder(Root, this)
            {
                ApplicationOrigin = ApplicationOrigin
            };
            installation?.Invoke(containerBuilder);
            return containerBuilder.BuildScope();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Inject(object instance)
        {
            var injector = InjectorCache.GetOrBuild(instance.GetType());
            injector.Inject(instance, this, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetRegistration(Type type, out Registration registration, object key = null)
            => registry.TryGet(type, key, out registration);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            sharedInstances.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        object ResolveCore(Registration registration)
        {
            switch (registration.Lifetime)
            {
                case Lifetime.Singleton:
                    if (Parent is null)
                        return Root.Resolve(registration);

                    if (!registry.Exists(registration.ImplementationType, registration.Key))
                        return Parent.Resolve(registration);

                    return CreateTrackedInstance(registration);

                case Lifetime.Scoped:
                    return CreateTrackedInstance(registration);

                default:
                    return registration.SpawnInstance(this);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        object CreateTrackedInstance(Registration registration)
        {
            if (!sharedInstances.TryGetValue(registration, out var instance))
            {
                instance = registration.SpawnInstance(this);
                sharedInstances.TryAdd(registration, instance);
            }

            return instance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool TryFindRegistration(Type type, object key, out Registration registration)
        {
            IScopedObjectResolver scope = this;
            while (scope != null)
            {
                if (scope.TryGetRegistration(type, out registration, key))
                {
                    return true;
                }
                scope = scope.Parent;
            }

            registration = default;
            return false;
        }
    }

    public sealed partial class Container : IObjectResolver
    {
        public object ApplicationOrigin { get; }

        readonly Registry registry;
        readonly IScopedObjectResolver rootScope;

        readonly Dictionary<Registration, object> sharedInstances;

        internal Container(Registry registry, object applicationOrigin = null, Dictionary<Registration, object> sharedInstances = null)
        {
            this.registry = registry;
            this.sharedInstances = sharedInstances ?? new Dictionary<Registration, object>();
            rootScope = new ScopedContainer(registry, this, applicationOrigin: applicationOrigin);
            ApplicationOrigin = applicationOrigin;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Resolve(Type type, object key = null)
        {
            if (TryGetRegistration(type, out var registration, key))
            {
                return Resolve(registration);
            }
            
            if (ThrowIfUnresolved)
                throw new VContainerException(type, $"No such registration of type: {type}{(key == null ? string.Empty : $" with Key: {key}")}");

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryResolve(Type type, out object resolved, object key = null)
        {
            if (TryGetRegistration(type, out var registration, key))
            {
                resolved = Resolve(registration);
                return true;
            }

            resolved = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Resolve(Registration registration) => ResolveCore(registration);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IScopedObjectResolver CreateScope(Action<IContainerBuilder> installation = null)
            => rootScope.CreateScope(installation);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Inject(object instance)
        {
            var injector = InjectorCache.GetOrBuild(instance.GetType());
            injector.Inject(instance, this, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetRegistration(Type type, out Registration registration, object key = null)
            => registry.TryGet(type, key, out registration);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            rootScope.Dispose();
            sharedInstances.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        object ResolveCore(Registration registration)
        {
            switch (registration.Lifetime)
            {
                case Lifetime.Singleton:
                    if (!sharedInstances.TryGetValue(registration, out var singleton))
                    {
                        singleton = registration.SpawnInstance(this);
                        sharedInstances.TryAdd(registration, singleton);
                    }
                    
                    return singleton;

                case Lifetime.Scoped:
                    return rootScope.Resolve(registration);

                default:
                    return registration.SpawnInstance(this);
            }
        }
    }
}

