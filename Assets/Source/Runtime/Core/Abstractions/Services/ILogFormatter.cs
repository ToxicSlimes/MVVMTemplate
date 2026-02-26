namespace Core.Abstractions.Services
{
    public interface ILogFormatter
    {
        string Format(LogLevel level, string template, params object[] args);
        string Render(string template, params object[] args);
        string ColorizeValue(object value);
    }
}
