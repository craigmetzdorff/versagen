using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Versagen.Utils
{
    /// <inheritdoc cref="IUnionType" />
    /// <summary>
    /// A C# version of F#'s union type implementation. It may only be ONE or NONE of these types at a given time.
    /// NOTE: Due to the nature of ValueTypes, which ARE allowed in this struct, if a value type is used it cannot return null.
    /// BUG: When copying to another variable that has the same types but in a different order the inner object is always lost and there seem to be little to do about it.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public struct UnionType<T1, T2> : IUnionType, IDynamicMetaObjectProvider
    {
        public object Obj { get; }

        public Type CurrentType => Obj.GetType();

        public Type[] GetTypes() => GetUnionTypesFromArray(typeof(T1), typeof(T2));

        internal static Type[] GetUnionTypesFromArray(params Type[] types)
        {
            var typList = new List<Type>();
            var utype = typeof(IUnionType);
            void HandleRecursion(Type t)
            {
                var ut = (IUnionType)Activator.CreateInstance(t);
                var typeshold = ut.GetTypes();
                typList.AddRange(typeshold);
            }

            foreach (var type in types)
            {
                if (type.IsSubclassOf(utype))
                    HandleRecursion(type);
                else typList.Add(type);
                if (type.IsSubclassOf(utype))
                    HandleRecursion(type);
                else typList.Add(type);
            }
            return typList.Distinct().ToArray();
        }

        public bool Is<T>(out T item)
        {
            switch (Obj)
            {
                case T outItem:
                    item = outItem;
                    return true;
                case IUnionType iu:
                    return iu.Is(out item);
                default:
                    item = default;
                    return false;
            }
        }

        public bool Is<T>() => Obj is T || Obj is IUnionType obj && obj.Is<T>();

        // ReSharper disable once MethodOverloadWithOptionalParameter
        public T As<T>(T defaultItem, bool throwIfInvalidCast = false) where T : struct
        {
            switch (Obj)
            {
                case T @as:
                    return @as;
                case IUnionType ut:
                    return ut.As(defaultItem, throwIfInvalidCast);
            }
            if (throwIfInvalidCast)
                throw new InvalidCastException("UnionType does not contain a value expressible as this type.");
            return defaultItem;
        }

        public T As<T>() where T : class => Obj as T ?? (Obj is IUnionType obj ? obj.As<T>() : null);

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case T1 _:
                case T2 _:
                    return Obj.Equals(obj);
                case IUnionType typ2:
                    return Obj.Equals(typ2.Obj);
                default:
                    return false;
            }
        }

        public override int GetHashCode() => Obj?.GetHashCode() ?? base.GetHashCode();

        public override string ToString() => Obj?.ToString() ?? base.ToString();
        public DynamicMetaObject GetMetaObject(Expression parameter) => DynamicMetaObject.Create(Obj, parameter);

        static UnionType()
        {
            var t1 = typeof(T1);
            var t2 = typeof(T2);
            if (t1 == t2)
                throw new InvalidOperationException("You should not use a UnionType with two type arguments that are the same. There is no benefit compared to simply using the same class.");
            if (t1.IsSubclassOf(t2) || t2.IsSubclassOf(t1))
                throw new InvalidOperationException(
                    "Union types are not appropriate for when one object inherits the other. Consider using just the types by themselves instead. In this case, "
                    + (t1.IsAssignableFrom(t2) ? $"T1, or {t1.Name}," : $"T2, or {t2.Name},")
                    + "appears to be the base class here.");
        }

        /// <summary>
        /// Unpacks an object that's been nested in to a bunch of these things.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal static object RecurseToBase(object obj)
        {
            while (true)
            {
                if (!(obj is IUnionType uobj)) return obj;
                obj = uobj.Obj;
            }
        }

        public UnionType(T1 isThisType)
        {
            Obj = RecurseToBase(isThisType);
        }
        public UnionType(T2 isThisType)
        {
            Obj = RecurseToBase(isThisType);
        }

        public void Deconstruct(out T1 item1)
        {
            item1 = Obj is T1 obj ? obj : default;
        }
        public void Deconstruct(out T2 item2)
        {
            item2 = Obj is T2 obj ? obj : default;
        }

        /// <summary>
        /// NOTE: THIS WILL ONLY EVER RETURN ONE OF THESE VARIABLES, NOT BOTH. KEEP THIS IN MIND WHEN TESTING IN SWITCH CASES.
        /// </summary>
        /// <param name="item1"></param>
        /// <param name="item2"></param>
        public void Deconstruct(out T1 item1, out T2 item2)
        {
            switch (Obj)
            {
                case T1 out1:
                    item1 = out1;
                    item2 = default;
                    return;
                case T2 out2:
                    item1 = default;
                    item2 = out2;
                    return;
                default:
                    item1 = default;
                    item2 = default;
                    return;
            }
        }

        public static implicit operator UnionType<T2, T1>(UnionType<T1, T2> swapped)
        {
            switch (swapped.Obj)
            {
                case T1 item1:
                    return new UnionType<T2, T1>(item1);
                case T2 item2:
                    return new UnionType<T2, T1>(item2);
                case IUnionType union:
                    if (union.Is<T1>(out var itemu1))
                        return new UnionType<T2, T1>(itemu1);
                    else if (union.Is<T2>(out var itemu2))
                        return new UnionType<T2, T1>(itemu2);
                    else goto default;
                default:
                    return default;
            }
        }

        public static UnionType<T1, T2> operator |(UnionType<T1, T2> us,
            UnionType<T1, T2> them)
        {
            if (us.Obj != null)
            {
                switch (us.Obj)
                {
                    case T1 wonobj:
                        return new UnionType<T1, T2>(wonobj);
                    case T2 tooobj:
                        return new UnionType<T1, T2>(tooobj);
                }
            }

            switch (them.Obj)
            {
                case T1 wonobj:
                    return new UnionType<T1, T2>(wonobj);
                case T2 tooobj:
                    return new UnionType<T1, T2>(tooobj);
            }

            return new UnionType<T1, T2>();
        }

        public static implicit operator UnionType<T1, T2>(T1 input) => new UnionType<T1, T2>(input);
        public static implicit operator UnionType<T1, T2>(T2 input) => new UnionType<T1, T2>(input);
        public static explicit operator T1(UnionType<T1, T2> union) => (union.Obj is T1 obj ? obj : default);
        public static explicit operator T2(UnionType<T1, T2> union) => (union.Obj is T2 obj ? obj : default);
    }
}
