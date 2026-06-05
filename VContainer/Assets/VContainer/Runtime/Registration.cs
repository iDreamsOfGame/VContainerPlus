using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VContainer
{
    public sealed class Registration : IEquatable<Registration>
    {
        public readonly Type ImplementationType;
        public readonly IReadOnlyList<Type> InterfaceTypes;
        public readonly Lifetime Lifetime;
        public readonly IInstanceProvider Provider;
        public readonly object Key;

        public Registration(
            Type implementationType,
            Lifetime lifetime,
            IReadOnlyList<Type> interfaceTypes,
            IInstanceProvider provider,
            object key = null)
        {
            ImplementationType = implementationType;
            InterfaceTypes = interfaceTypes;
            Lifetime = lifetime;
            Provider = provider;
            Key = key;
        }

        public override string ToString()
        {
            var contractTypes = InterfaceTypes != null ? string.Join(", ", InterfaceTypes) : "";
            var keyStr = Key == null ? "" : $" (Key: {Key})";
            return $"Registration {ImplementationType.Name}{keyStr} ContractTypes=[{contractTypes}] {Lifetime} {Provider}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object SpawnInstance(IObjectResolver resolver) => Provider.SpawnInstance(resolver);
        
        public bool Equals(Registration other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return ImplementationType == other.ImplementationType &&
                   Lifetime == other.Lifetime &&
                   Equals(Key, other.Key) &&
                   AreInterfaceTypesEqual(InterfaceTypes, other.InterfaceTypes);
        }
        
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            return Equals((Registration)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ImplementationType?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (int)Lifetime;
                hashCode = (hashCode * 397) ^ (Key?.GetHashCode() ?? 0);
            
                if (InterfaceTypes != null)
                {
                    foreach (var type in InterfaceTypes)
                    {
                        hashCode = (hashCode * 397) ^ (type?.GetHashCode() ?? 0);
                    }
                }
            
                return hashCode;
            }
        }

        private static bool AreInterfaceTypesEqual(IReadOnlyList<Type> a, IReadOnlyList<Type> b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;
            if (a.Count != b.Count) return false;

            for (var i = 0; i < a.Count; i++)
            {
                if (a[i] != b[i]) return false;
            }
        
            return true;
        }
    }
}
