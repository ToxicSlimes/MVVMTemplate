namespace Core.Abstractions.Services.Platform
{
    public interface IClipboardService
    {
        void SetText(string text);
        string GetText();
    }
}
