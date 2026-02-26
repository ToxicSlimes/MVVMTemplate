using System;
using Core.Abstractions.Services;
using UnityEngine;

namespace Core.Infrastructure.Services.Logging
{
    public class UnityLogInterceptor : ILogHandler, ILogInterceptor
    {
        private static UnityLogInterceptor _instance;
        private readonly ILogHandler _inner;
        private readonly ILogFormatter _formatter;

        public UnityLogInterceptor(ILogFormatter formatter)
        {
            _formatter = formatter;
            _inner = Debug.unityLogger.logHandler;
        }

        [HideInCallstack]
        public void Ensure()
        {
            if (_instance != null)
                return;
            _instance = this;
            Debug.unityLogger.logHandler = this;
        }

        [HideInCallstack]
        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
            var text = args != null && args.Length > 0 ? string.Format(format, args) : format;
            if (ColorLogFormatter.HasMarker(text))
            {
                var stripped = ColorLogFormatter.StripMarker(text);
                _inner?.LogFormat(logType, context, "{0}", stripped);
                return;
            }
            var level = Map(logType);
            var formatted = _formatter.Format(level, text, Array.Empty<object>());
            _inner?.LogFormat(logType, context, "{0}", formatted);
        }

        [HideInCallstack]
        public void LogException(Exception exception, UnityEngine.Object context)
        {
            var formatted = _formatter.Format(LogLevel.Error, exception.ToString(), Array.Empty<object>());
            _inner?.LogFormat(LogType.Exception, context, "{0}", formatted);
        }

        private LogLevel Map(LogType logType)
        {
            return logType switch
            {
                LogType.Error => LogLevel.Error,
                LogType.Assert => LogLevel.Error,
                LogType.Exception => LogLevel.Error,
                LogType.Warning => LogLevel.Warning,
                _ => LogLevel.Info
            };
        }
    }
}
