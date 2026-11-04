namespace Core.Abstractions.Services.Platform
{
    public interface IPlatformService
    {
        GamePlatform Platform { get; }

        DeviceKind Device { get; }

        string Language { get; }

        void MarkReady();

        void SetGameplayActive(bool active);
    }
}
