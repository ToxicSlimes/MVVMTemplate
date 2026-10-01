using System;
using System.Collections.Generic;

namespace Core.Editor.UIToolkit.ScreenSpec
{
    [Serializable]
    public class ScreenSpecDocument
    {
        public int schemaVersion;
        public string screenId;
        public string viewType;
        public string viewModelType;
        public string viewNamespace;
        public ScreenSpecOutput output;
        public ScreenSpecTheme theme;
        public ScreenSpecLocalization localization;
        public List<ScreenSpecNode> nodes = new();
        public List<ScreenSpecBinding> bindings = new();
    }

    [Serializable]
    public class ScreenSpecOutput
    {
        public string folder;
        public string uxmlFile;
        public string uxmlFileName => uxmlFile;
        public string ussFile;
        public bool includeToolkitScreen;
        public bool includePixelControls;
    }

    [Serializable]
    public class ScreenSpecTheme
    {
        public List<string> styles = new();
    }

    [Serializable]
    public class ScreenSpecLocalization
    {
        public string table;
        public string prefix;
        public List<ScreenSpecLocalizationKey> keys = new();
    }

    [Serializable]
    public class ScreenSpecLocalizationKey
    {
        public string key;
        public string en;
        public string ru;
    }

    [Serializable]
    public class ScreenSpecNode
    {
        public string type;
        public string name;
        public string text;
        public string @class;
        public string tone;
        public string label;
        public string valueName;
        public bool rawClass;
        public float low;
        public float high;
        public List<ScreenSpecTab> tabs = new();
        public List<ScreenSpecNode> children = new();
    }

    [Serializable]
    public class ScreenSpecTab
    {
        public string id;
        public string label;
        public string tabButtonName;
        public List<ScreenSpecNode> children = new();
    }

    [Serializable]
    public class ScreenSpecBinding
    {
        public string name;
        public string kind;
        public string target;
        public string valueLabel;
        public string localizationKey;
        public string format;
        public string elementType;
        public bool queryByClass;
        public string className;
    }
}
