namespace Core.Infrastructure.Mediator.Pipeline
{
    using System.Threading;
    using Cysharp.Threading.Tasks;

    /// <summary>
    /// Executes after handler.
    /// </summary>
    public interface IRequestPostProcessor<in TRequest, in TResponse>
        where TRequest : notnull
    {
        UniTask Process(TRequest request, TResponse response, CancellationToken cancellationToken);
    }
}
