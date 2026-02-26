using System.IO;

namespace Core.Editor.ViewMappingGenerator
{
    public static class ViewMappingGeneratorConfig
    {
        public const string MappingScriptsFolder = "Assets/Source/CodeGen/Views";
        public const string MappingAssetsFolder = "Assets/Source/00. Configs/ViewsMapping";
        public const string ViewConfigPath = "Assets/Source/00. Configs/ViewConfig.asset";

        public static string GetMappingScriptPath(string viewName)
        {
            return Path.Combine(MappingScriptsFolder, viewName, viewName + "ViewMapping.cs");
        }

        public static string GetMappingAssetPath(string viewName)
        {
            return Path.Combine(MappingAssetsFolder, viewName + "ViewMapping.asset");
        }

        public static string GetMappingTypeName(string viewName)
        {
            return viewName + "ViewMapping";
        }

        public static void EnsureDirectoriesExist(string viewName)
        {
            var scriptDir = Path.GetDirectoryName(GetMappingScriptPath(viewName));
            if (!Directory.Exists(scriptDir))
            {
                Directory.CreateDirectory(scriptDir);
            }

            if (!Directory.Exists(MappingAssetsFolder))
            {
                Directory.CreateDirectory(MappingAssetsFolder);
            }
        }
    }
}

