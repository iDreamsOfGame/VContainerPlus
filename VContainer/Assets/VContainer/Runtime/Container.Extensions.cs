using System.Collections.Concurrent;

// ReSharper disable ConvertToAutoProperty

namespace VContainer
{
    public partial interface IObjectResolver
    {
        ConcurrentDictionary<Registration, object> SharedInstances { get; }

        bool ThrowIfUnresolved { get; set; }
    }

    public sealed partial class ScopedContainer
    {
        public ConcurrentDictionary<Registration, object> SharedInstances => sharedInstances;

        public bool ThrowIfUnresolved { get; set; } = true;
    }

    public sealed partial class Container
    {
        public ConcurrentDictionary<Registration, object> SharedInstances => sharedInstances;
        
        public bool ThrowIfUnresolved { get; set; } = true;
        
        public IScopedObjectResolver RootScope => rootScope;
    }
}