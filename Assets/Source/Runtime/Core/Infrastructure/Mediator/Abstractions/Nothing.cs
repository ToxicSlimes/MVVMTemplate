namespace Core.Infrastructure.Mediator.Abstractions
{
    using System;
    using Cysharp.Threading.Tasks;

    public readonly struct Nothing : IEquatable<Nothing>, IComparable<Nothing>
    {
        public static readonly Nothing Value = new();
        public static readonly UniTask<Nothing> Task = UniTask.FromResult(Value);

        public override int GetHashCode() => 0;

        public override bool Equals(object obj) => obj is Nothing;

        public bool Equals(Nothing other) => true;

        public int CompareTo(Nothing other) => 0;

        public override string ToString() => "()";

        public static bool operator ==(Nothing left, Nothing right) => true;

        public static bool operator !=(Nothing left, Nothing right) => false;
    }
}
