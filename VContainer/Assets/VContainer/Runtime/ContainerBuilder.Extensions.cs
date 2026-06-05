using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace VContainer
{
    public sealed partial class ScopedContainerBuilder
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override IObjectResolver Build(ConcurrentDictionary<Registration, object> sharedInstances) => BuildScope(sharedInstances);
        
        public IScopedObjectResolver BuildScope(ConcurrentDictionary<Registration, object> sharedInstances)
        {
            var registry = BuildRegistry();
            var container = new ScopedContainer(registry, root, parent, ApplicationOrigin, sharedInstances);
            container.Diagnostics = Diagnostics;
            EmitCallbacks(container);
            return container;
        }
    }
    
    public partial class ContainerBuilder
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual IObjectResolver Build(ConcurrentDictionary<Registration, object> sharedInstances)
        {
            var registry = BuildRegistry();
            var container = new Container(registry, ApplicationOrigin, sharedInstances);
            container.Diagnostics = Diagnostics;
            EmitCallbacks(container);
            return container;
        }
    }
}