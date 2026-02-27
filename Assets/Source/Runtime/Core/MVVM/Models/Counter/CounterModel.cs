using System;
using Core.Abstractions.MVVM.Models.Counter;

namespace Core.MVVM.Models.Counter
{
    /// <summary>
    /// TEMPLATE EXAMPLE: Concrete model implementation.
    /// Pure C# — no Unity dependencies. Easy to unit test.
    /// </summary>
    public class CounterModel : ICounterModel
    {
        public int Value { get; private set; }
        public int TotalIncrements { get; private set; }
        public event Action<int, int> ValueChanged;

        public void Increment(int amount)
        {
            var old = Value;
            Value += amount;
            TotalIncrements++;
            ValueChanged?.Invoke(old, Value);
        }

        public void Reset()
        {
            var old = Value;
            Value = 0;
            TotalIncrements = 0;
            if (old != 0) ValueChanged?.Invoke(old, 0);
        }

        public void Dispose()
        {
            ValueChanged = null;
        }
    }
}
