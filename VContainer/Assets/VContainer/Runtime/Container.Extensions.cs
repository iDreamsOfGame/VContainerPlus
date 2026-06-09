using System.Collections.Generic;

// ReSharper disable ConvertToAutoProperty

namespace VContainer
{
    public partial interface IObjectResolver
    {
        Dictionary<Registration, object> SharedInstances { get; }

        bool ThrowIfUnresolved { get; set; }
    }

    public sealed partial class ScopedContainer
    {
        public Dictionary<Registration, object> SharedInstances => sharedInstances;

        public bool ThrowIfUnresolved { get; set; } = true;
    }

    public sealed partial class Container
    {
        public Dictionary<Registration, object> SharedInstances => sharedInstances;
        
        public bool ThrowIfUnresolved { get; set; } = true;
        
        public IScopedObjectResolver RootScope => rootScope;
    }
}