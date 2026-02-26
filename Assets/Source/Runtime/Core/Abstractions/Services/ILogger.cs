using System;

namespace Core.Abstractions.Services
{
    public interface ILogger
    {
        LogLevel MinimumLevel { get; set; }
        void Log(LogLevel level, string messageTemplate, params object[] args);
        void Debug(string messageTemplate, params object[] args);
        void Info(string messageTemplate, params object[] args);
        void Warn(string messageTemplate, params object[] args);
        void Error(string messageTemplate, params object[] args);
        void Error(Exception exception, string messageTemplate = "", params object[] args);
    }
}
