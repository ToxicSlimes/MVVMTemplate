using System;
using System.Text.RegularExpressions;
using Core.Abstractions.Services;
using UnityEngine;

namespace Core.Infrastructure.Services.Logging
{
    public class ColorLogFormatter : ILogFormatter
    {
        private readonly bool _useColors;
        private readonly bool _showTimestamp;
        private readonly string _tag;
        private readonly Color _valueColor;
        private readonly Color _timestampColor;
        private readonly Color _tagColor;
        private readonly Color _debugColor;
        private readonly Color _infoColor;
        private readonly Color _warningColor;
        private readonly Color _errorColor;

        public ColorLogFormatter(bool useColors = true, bool showTimestamp = true, string tag = "[Log]")
        {
            _useColors = useColors;
            _showTimestamp = showTimestamp;
            _tag = tag;
            _valueColor = new Color(1f, 0.9f, 0.6f, 1f);
            _timestampColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            _tagColor = new Color(0.6f, 0.6f, 0.6f, 1f);
            _debugColor = new Color(0.6f, 0.6f, 0.6f, 1f);
            _infoColor = new Color(0.2f, 0.8f, 1f, 1f);
            _warningColor = new Color(1f, 0.85f, 0.35f, 1f);
            _errorColor = new Color(1f, 0.35f, 0.35f, 1f);
        }

        public string Format(LogLevel level, string template, params object[] args)
        {
            var body = Render(template ?? string.Empty, args);
            var timestamp = _showTimestamp ? BuildTimestamp() : string.Empty;
            var levelLabel = BuildLevelLabel(level);
            var tag = BuildTag();
            return string.Concat(timestamp, tag, levelLabel, " ", body);
        }

        public string Render(string template, params object[] args)
        {
            if (string.IsNullOrEmpty(template))
                return string.Empty;
            return args == null || args.Length == 0 ? template : ReplacePlaceholders(template, args);
        }

        public string ColorizeValue(object value)
        {
            var content = value?.ToString() ?? "null";
            if (!_useColors)
                return content;
            return $"<color=#{ToHex(_valueColor)}>{content}</color>";
        }

        private string BuildTimestamp()
        {
            var text = DateTime.Now.ToString("HH:mm:ss.fff");
            if (!_useColors)
                return $"[{text}] ";
            return $"<color=#{ToHex(_timestampColor)}>[{text}]</color> ";
        }

        private string BuildTag()
        {
            if (string.IsNullOrEmpty(_tag))
                return string.Empty;
            if (!_useColors)
                return $"{_tag} ";
            return $"<color=#{ToHex(_tagColor)}>{_tag}</color> ";
        }

        private string BuildLevelLabel(LogLevel level)
        {
            var text = $"[{level}]";
            if (!_useColors)
                return text;
            return level switch
            {
                LogLevel.Debug => $"<color=#{ToHex(_debugColor)}>{text}</color>",
                LogLevel.Info => $"<color=#{ToHex(_infoColor)}>{text}</color>",
                LogLevel.Warning => $"<color=#{ToHex(_warningColor)}>{text}</color>",
                LogLevel.Error => $"<color=#{ToHex(_errorColor)}>{text}</color>",
                _ => text
            };
        }

        private string ReplacePlaceholders(string template, object[] args)
        {
            var regex = new Regex(@"\{(@?[\w\d_]+)(?::([^}]+))?\}");
            var index = 0;
            return regex.Replace(template, match =>
            {
                if (index >= args.Length)
                    return match.Value;
                var value = args[index++];
                var destructure = match.Groups[1].Value.StartsWith("@");
                var format = match.Groups[2].Success ? match.Groups[2].Value : null;
                var rendered = RenderValue(value, format, destructure);
                return ColorizeValue(rendered);
            });
        }

        private string RenderValue(object value, string format, bool destructure)
        {
            if (value == null)
                return "null";
            var source = value;
            if (destructure)
            {
                try
                {
                    var json = JsonUtility.ToJson(value, false);
                    if (!string.IsNullOrEmpty(json))
                        source = json;
                }
                catch
                {
                    source = value;
                }
            }
            if (string.IsNullOrEmpty(format))
                return source.ToString();
            try
            {
                return string.Format($"{{0:{format}}}", source);
            }
            catch
            {
                return source.ToString();
            }
        }

        private string ToHex(Color color)
        {
            return ColorUtility.ToHtmlStringRGBA(color);
        }

        private const string Marker = "[[clog]]";

        public static string Mark(string text)
        {
            return string.Concat(Marker, text ?? string.Empty);
        }

        public static bool HasMarker(string text)
        {
            return !string.IsNullOrEmpty(text) && text.StartsWith(Marker, StringComparison.Ordinal);
        }

        public static string StripMarker(string text)
        {
            return HasMarker(text) ? text.Substring(Marker.Length) : text;
        }
    }
}
