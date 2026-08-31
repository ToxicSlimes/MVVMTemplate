namespace Core.Network.Abstractions.Input
{
    /// <summary>
    /// Собирает структуру ввода локального игрока. Вызывается каждый сетевой тик, аллокации недопустимы.
    /// </summary>
    public interface INetworkInputCollector<TInput> where TInput : struct
    {
        bool TryCollect(out TInput input);
    }
}
