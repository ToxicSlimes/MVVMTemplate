using Cysharp.Threading.Tasks;

namespace Core.Mediator.Abstractions
{
    /// <summary>
    /// Готовый результат для хендлеров без возвращаемого значения.
    /// </summary>
    /// <remarks>
    /// Кэш лежит здесь, а не в самом <see cref="Nothing"/>, намеренно. Статическое поле типа
    /// <c>UniTask&lt;Nothing&gt;</c> внутри <c>Nothing</c> делало определение рекурсивным:
    /// загрузка структуры требовала загрузки обобщения, параметризованного ею же. Среда
    /// разрешала этот цикл в зависимости от порядка загрузки типов, поэтому падение
    /// <c>TypeLoadException</c> выглядело плавающим.
    /// </remarks>
    public static class NothingTask
    {
        public static UniTask<Nothing> Completed => UniTask.FromResult(Nothing.Value);
    }
}
