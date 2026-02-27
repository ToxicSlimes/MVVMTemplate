using System;

namespace Core.Abstractions.MVVM.Models.Counter
{
    /// <summary>
    /// TEMPLATE EXAMPLE: Model interface — defines the data contract.
    /// Models hold state and business logic. ViewModels consume models.
    /// </summary>
    public interface ICounterModel : IDisposable
    {
        int Value { get; }
        int TotalIncrements { get; }
        event Action<int, int> ValueChanged; // oldValue, newValue

        void Increment(int amount);
        void Reset();
    }
}
