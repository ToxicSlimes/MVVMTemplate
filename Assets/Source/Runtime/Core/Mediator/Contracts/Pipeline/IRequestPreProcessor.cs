namespace Core.Mediator.Pipeline
{
    using System.Threading;
    using Cysharp.Threading.Tasks;

    /// <summary>
    /// Executes before handler.
    /// </summary>
    public interface IRequestPreProcessor<in TRequest>
        where TRequest : notnull
    {
        UniTask Process(TRequest request, CancellationToken cancellationToken);
    }
}
