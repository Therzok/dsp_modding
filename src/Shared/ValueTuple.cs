using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System.Runtime.CompilerServices
{
    [CLSCompliant(false)]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Event)]
    sealed class TupleElementNamesAttribute : Attribute
    {
        private readonly string[] _transformNames;

        public TupleElementNamesAttribute(string[] transformNames)
        {
            if (transformNames == null)
            {
                throw new ArgumentNullException(nameof(transformNames));
            }

            _transformNames = transformNames;
        }

        /// <summary>
        /// Specifies, in a pre-order depth-first traversal of a type's
        /// construction, which <see cref="System.ValueTuple"/> elements are
        /// meant to carry element names.
        /// </summary>
        public IList<string> TransformNames => _transformNames;
    }
}
namespace System
{
    interface IStructuralEquatable
    {
        bool Equals(object other, IEqualityComparer comparer);
        int GetHashCode(IEqualityComparer comparer);
    }

    interface IStructuralComparable
    {
        int CompareTo(object other, IComparer comparer);
    }

    interface ITuple
    {
        int Length { get; }

        object this[int index] { get; }
    }

    internal interface IValueTupleInternal : ITuple
    {
        int GetHashCode(IEqualityComparer comparer);
        string ToStringEnd();
    }

    [Serializable]
    struct ValueTuple
        : IEquatable<ValueTuple>, IStructuralEquatable, IStructuralComparable, IComparable, IComparable<ValueTuple>, IValueTupleInternal, ITuple
    {
        public override bool Equals(object obj)
        {
            return obj is ValueTuple;
        }

        public bool Equals(ValueTuple other)
        {
            return true;
        }

        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
        {
            return other is ValueTuple;
        }

        int IComparable.CompareTo(object other)
        {
            if (other is null) return 1;

            if (!(other is ValueTuple))
            {
                throw new ArgumentException("Incorrect tuple type", nameof(other));
            }

            return 0;
        }

        public int CompareTo(ValueTuple other)
        {
            return 0;
        }

        int IStructuralComparable.CompareTo(object other, IComparer comparer)
        {
            if (other is null) return 1;

            if (!(other is ValueTuple))
            {
                throw new ArgumentException("Incorrect tuple type", nameof(other));
            }

            return 0;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            return 0;
        }

        int IValueTupleInternal.GetHashCode(IEqualityComparer comparer)
        {
            return 0;
        }

        public override string ToString()
        {
            return "()";
        }

        string IValueTupleInternal.ToStringEnd()
        {
            return ")";
        }

        int ITuple.Length => 0;

        object ITuple.this[int index] => throw new IndexOutOfRangeException();

        public static ValueTuple Create() =>
            default;

        public static ValueTuple<T1> Create<T1>(T1 item1) =>
            new ValueTuple<T1>(item1);

        public static ValueTuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2) =>
            new ValueTuple<T1, T2>(item1, item2);
    }

    [Serializable]
    struct ValueTuple<T1>
        : IEquatable<ValueTuple<T1>>, IStructuralEquatable, IStructuralComparable, IComparable, IComparable<ValueTuple<T1>>, IValueTupleInternal, ITuple
    {
        public T1 Item1;

        public ValueTuple(T1 item1)
        {
            Item1 = item1;
        }

        public override bool Equals(object obj)
        {
            return obj is ValueTuple<T1> tuple && Equals(tuple);
        }

        public bool Equals(ValueTuple<T1> other)
        {
            return EqualityComparer<T1>.Default.Equals(Item1, other.Item1);
        }

        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer) =>
            other is ValueTuple<T1> vt &&
            comparer.Equals(Item1, vt.Item1);

        int IComparable.CompareTo(object other)
        {
            if (other != null)
            {
                if (other is ValueTuple<T1> objTuple)
                {
                    return Comparer<T1>.Default.Compare(Item1, objTuple.Item1);
                }

                throw new ArgumentException("Incorrect tuple type", nameof(other));
            }

            return 1;
        }

        public int CompareTo(ValueTuple<T1> other)
        {
            return Comparer<T1>.Default.Compare(Item1, other.Item1);
        }

        int IStructuralComparable.CompareTo(object other, IComparer comparer)
        {
            if (other != null)
            {
                if (other is ValueTuple<T1> objTuple)
                {
                    return comparer.Compare(Item1, objTuple.Item1);
                }

                throw new ArgumentException("Incorrect tuple type", nameof(other));
            }

            return 1;
        }

        public override int GetHashCode()
        {
            return Item1?.GetHashCode() ?? 0;
        }

        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            return comparer.GetHashCode(Item1);
        }

        int IValueTupleInternal.GetHashCode(IEqualityComparer comparer)
        {
            return comparer.GetHashCode(Item1);
        }

        public override string ToString()
        {
            return "(" + Item1?.ToString() + ")";
        }

        string IValueTupleInternal.ToStringEnd()
        {
            return Item1?.ToString() + ")";
        }

        int ITuple.Length => 1;

        object ITuple.this[int index] {
            get {
                if (index != 0)
                {
                    throw new IndexOutOfRangeException();
                }
                return Item1;
            }
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Auto)]
    struct ValueTuple<T1, T2>
        : IEquatable<ValueTuple<T1, T2>>, IStructuralEquatable, IStructuralComparable, IComparable, IComparable<ValueTuple<T1, T2>>, IValueTupleInternal, ITuple
    {
        public T1 Item1;

        public T2 Item2;

        public ValueTuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }

        public override bool Equals(object obj)
        {
            return obj is ValueTuple<T1, T2> tuple && Equals(tuple);
        }

        public bool Equals(ValueTuple<T1, T2> other)
        {
            return EqualityComparer<T1>.Default.Equals(Item1, other.Item1)
                && EqualityComparer<T2>.Default.Equals(Item2, other.Item2);
        }

        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer) =>
            other is ValueTuple<T1, T2> vt &&
            comparer.Equals(Item1, vt.Item1) &&
            comparer.Equals(Item2, vt.Item2);

        int IComparable.CompareTo(object other)
        {
            if (other != null)
            {
                if (other is ValueTuple<T1, T2> objTuple)
                {
                    return CompareTo(objTuple);
                }

                throw new ArgumentException("Incorrect tuple type", nameof(other));
            }

            return 1;
        }

        public int CompareTo(ValueTuple<T1, T2> other)
        {
            int c = Comparer<T1>.Default.Compare(Item1, other.Item1);
            if (c != 0) return c;

            return Comparer<T2>.Default.Compare(Item2, other.Item2);
        }

        int IStructuralComparable.CompareTo(object other, IComparer comparer)
        {
            if (other != null)
            {
                if (other is ValueTuple<T1, T2> objTuple)
                {
                    int c = comparer.Compare(Item1, objTuple.Item1);
                    if (c != 0) return c;

                    return comparer.Compare(Item2, objTuple.Item2);
                }

                throw new ArgumentException("Incorrect tuple type", nameof(other));
            }

            return 1;
        }

        public override int GetHashCode()
        {
            // FIXME: 
            return 0;
//            return HashCode.Combine(Item1?.GetHashCode() ?? 0,
//                                    Item2?.GetHashCode() ?? 0);
        }

        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            return GetHashCodeCore(comparer);
        }

        private int GetHashCodeCore(IEqualityComparer comparer)
        {
            // FIXME:
            return 0;
            //return HashCode.Combine(comparer.GetHashCode(Item1),
                                    //comparer.GetHashCode(Item2));
        }

        int IValueTupleInternal.GetHashCode(IEqualityComparer comparer)
        {
            return GetHashCodeCore(comparer);
        }

        public override string ToString()
        {
            return "(" + Item1?.ToString() + ", " + Item2?.ToString() + ")";
        }

        string IValueTupleInternal.ToStringEnd()
        {
            return Item1?.ToString() + ", " + Item2?.ToString() + ")";
        }

        int ITuple.Length => 2;

        object ITuple.this[int index] {
            get {
                switch (index)
                {
                    case 0:
                        return Item1;
                    case 1:
                        return Item2;
                    default:
                        throw new IndexOutOfRangeException();

                }
            }
        }
    }
}