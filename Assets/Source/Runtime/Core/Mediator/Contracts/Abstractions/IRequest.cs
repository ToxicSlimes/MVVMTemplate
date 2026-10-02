namespace Core.Mediator.Abstractions
{
    /// <summary>
    /// Marker interface for requests with response.
    /// </summary>
    public interface IRequest<out TResponse>
    {
    }

    /// <summary>
    /// Marker interface for requests without response (commands).
    /// </summary>
    public interface IRequest : IRequest<Nothing>
    {
    }
}
