using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VContainer.Internal
{
    static class RuntimeTypeCache
    {
        private static Dictionary<Type, Type> OpenGenericTypes;

        private static Dictionary<Type, Type[]> GenericTypeParameters;

        private static Dictionary<Type, Type> ArrayTypes;

        private static Dictionary<Type, Type> EnumerableTypes;

        private static Dictionary<Type, Type> ReadOnlyListTypes;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type OpenGenericTypeOf(Type closedGenericType)
        {
            OpenGenericTypes ??= new Dictionary<Type, Type>();
            return OpenGenericTypes.GetOrAdd(closedGenericType, key => key.GetGenericTypeDefinition());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type[] GenericTypeParametersOf(Type closedGenericType)
        {
            GenericTypeParameters ??= new Dictionary<Type, Type[]>();
            return GenericTypeParameters.GetOrAdd(closedGenericType, key => key.GetGenericArguments());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type ArrayTypeOf(Type elementType)
        {
            ArrayTypes ??= new Dictionary<Type, Type>();
            return ArrayTypes.GetOrAdd(elementType, key => key.MakeArrayType());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type EnumerableTypeOf(Type elementType)
        {
            EnumerableTypes ??= new Dictionary<Type, Type>();
            return EnumerableTypes.GetOrAdd(elementType, key => typeof(IEnumerable<>).MakeGenericType(key));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type ReadOnlyListTypeOf(Type elementType)
        {
            ReadOnlyListTypes ??= new Dictionary<Type, Type>();
            return ReadOnlyListTypes.GetOrAdd(elementType, key => typeof(IReadOnlyList<>).MakeGenericType(key));
        }
    }
}