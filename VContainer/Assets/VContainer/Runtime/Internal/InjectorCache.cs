using System;
using System.Collections.Generic;
using System.Reflection;

namespace VContainer.Internal
{
    public static class InjectorCache
    {
        static readonly Dictionary<Type, IInjector> Injectors = new();

        public static IInjector GetOrBuild(Type type)
        {
            return Injectors.GetOrAdd(type, key =>
            {
                // SourceGenerator
                var generatedType = key.Assembly.GetType($"{key.FullName}GeneratedInjector", false);
                if (generatedType != null)
                {
                    return (IInjector)Activator.CreateInstance(generatedType);
                }

                // IL weaving (Deprecated)
                var getter = key.GetMethod("__GetGeneratedInjector", BindingFlags.Static | BindingFlags.Public);
                if (getter != null)
                {
                    return (IInjector)getter.Invoke(null, null);
                }
                return ReflectionInjector.Build(key);
            });
        }
    }
}
