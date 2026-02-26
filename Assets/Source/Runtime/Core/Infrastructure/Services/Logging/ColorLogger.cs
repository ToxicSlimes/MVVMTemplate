using System;
using Core.Abstractions.Services;
using UnityEngine;
using ILogger = Core.Abstractions.Services.ILogger;

namespace Core.Infrastructure.Services.Logging
{
    public class ColorLogger : ILogger
    {
        private readonly ILogFormatter _formatter;
        private readonly bool _includeStackTrace;

        public ColorLogger(ILogFormatter formatter, ILogInterceptor interceptor, bool includeStackTrace = false)
        {
            _formatter = formatter;
            _includeStackTrace = includeStackTrace;
            interceptor.Ensure();
        }

        public LogLevel MinimumLevel { get; set; } = LogLevel.Debug;

        [HideInCallstack]
        public void Log(LogLevel level, string messageTemplate, params object[] args)
        {
            Write(level, messageTemplate, args);
        }

        [HideInCallstack]
        public void Debug(string messageTemplate, params object[] args)
        {
            Write(LogLevel.Debug, messageTemplate, args);
        }

        [HideInCallstack]
        public void Info(string messageTemplate, params object[] args)
        {
            Write(LogLevel.Info, messageTemplate, args);
        }

        [HideInCallstack]
        public void Warn(string messageTemplate, params object[] args)
        {
            Write(LogLevel.Warning, messageTemplate, args);
        }

        [HideInCallstack]
        public void Error(string messageTemplate, params object[] args)
        {
            Write(LogLevel.Error, messageTemplate, args);
        }

        [HideInCallstack]
        public void Error(Exception exception, string messageTemplate = "", params object[] args)
        {
            var prefix = string.IsNullOrWhiteSpace(messageTemplate) ? string.Empty : _formatter.Render(messageTemplate, args);
            var exceptionText = _includeStackTrace ? exception.ToString() : $"{exception.GetType().Name}: {exception.Message}";
            var composed = string.IsNullOrWhiteSpace(prefix) ? exceptionText : $"{prefix}\n{exceptionText}";
            Write(LogLevel.Error, "{0}", composed);
        }

        [HideInCallstack]
        private void Write(LogLevel level, string messageTemplate, params object[] args)
        {
            if (level < MinimumLevel || level == LogLevel.None)
                return;

            var rendered = _formatter.Format(level, messageTemplate ?? string.Empty, args ?? Array.Empty<object>());
            var output = ColorLogFormatter.Mark(rendered);

            switch (level)
            {
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(output);
                    break;
                case LogLevel.Error:
                    UnityEngine.Debug.LogError(output);
                    break;
                case LogLevel.Debug:
                case LogLevel.Info:
                default:
                    UnityEngine.Debug.Log(output);
                    break;
            }
        }
    }
}
