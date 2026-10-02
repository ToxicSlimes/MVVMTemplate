namespace Core.Mediator.Abstractions
{
    /// <summary>
    /// Main mediator interface combining ISender and IPublisher.
    /// </summary>
    public interface IMediator : ISender, IPublisher
    {
    }
}
