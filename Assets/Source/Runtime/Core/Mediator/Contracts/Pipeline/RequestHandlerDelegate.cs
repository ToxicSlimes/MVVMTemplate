namespace Core.Mediator.Pipeline
{
    using Cysharp.Threading.Tasks;

    /// <summary>
    /// Delegate representing the next action in the pipeline.
    /// </summary>
    public delegate UniTask<TResponse> RequestHandlerDelegate<TResponse>();
}
