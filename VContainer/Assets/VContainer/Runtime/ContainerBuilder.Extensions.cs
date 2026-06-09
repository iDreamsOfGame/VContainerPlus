using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VContainer
{
    public partial interface IContainerBuilder
    {
        bool IsDirty { get; }
    }
    
    public sealed partial class ScopedContainerBuilder
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override IObjectResolver Build(Dictionary<Registration, object> sharedInstances) => BuildScope(sharedInstances);
        
        public IScopedObjectResolver BuildScope(Dictionary<Registration, object> sharedInstances)
        {
            var registry = BuildRegistry();
            var container = new ScopedContainer(registry, root, parent, ApplicationOrigin, sharedInstances);
            EmitCallbacks(container);
            return container;
        }
    }
    
    public partial class ContainerBuilder
    {
        public bool IsDirty { get; protected set; } = true;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IObjectResolver Build(Dictionary<Registration, object> sharedInstances)
        {
            var registry = BuildRegistry();
            var container = new Container(registry, ApplicationOrigin, sharedInstances);
            EmitCallbacks(container);
            return container;
        }
    }
}