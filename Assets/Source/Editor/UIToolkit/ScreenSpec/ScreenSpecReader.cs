using System.IO;
using Newtonsoft.Json;

namespace Core.Editor.UIToolkit.ScreenSpec
{
    public static class ScreenSpecReader
    {
        public static ScreenSpecDocument Load(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return null;

            var json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json))
                return null;

            return JsonConvert.DeserializeObject<ScreenSpecDocument>(json);
        }
    }
}
