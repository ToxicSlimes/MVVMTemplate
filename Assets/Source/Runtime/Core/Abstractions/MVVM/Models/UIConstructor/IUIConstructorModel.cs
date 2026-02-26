using System;
using System.Collections.Generic;
using Core.Abstractions.MVVM.Models;
using Core.Data.Domain.Models.ImageGen;
using Core.Data.Domain.Models.UIGeneration;

namespace Core.Abstractions.MVVM.Models.UIConstructor
{
    public interface IUIConstructorModel : IModel
    {
        UIElementType SelectedElementType { get; }
        List<UIPromptBlock> Blocks { get; }
        UIStylePreset SelectedStylePreset { get; }
        ImageGenSettings ImageSettings { get; }
        bool RemoveBackground { get; set; }
        bool IsGenerating { get; }
        string StatusText { get; }
        IImageHandle PreviewImage { get; }
        IImageHandle ProcessedImage { get; }
        
        event Action ElementTypeChanged;
        event Action BlocksChanged;
        event Action StylePresetChanged;
        event Action ImageSettingsChanged;
        event Action IsGeneratingChanged;
        event Action StatusTextChanged;
        event Action PreviewImageChanged;
        
        void SetElementType(UIElementType type);
        void ApplyStylePreset(UIStylePreset preset);
        void UpdateBlock(string blockName, UIPromptBlock block);
        void UpdateField(string blockName, string fieldPath, object value);
        void ToggleBlockIncluded(string blockName, bool included);
        void AddCustomField(string blockName, UIPromptField field);
        void RemoveField(string blockName, string fieldPath);
        string GetPreviewJson();
        UIGenerationRequest BuildRequest();
        void SetGenerating(bool value);
        void SetStatus(string text);
        void SetPreviewImage(IImageHandle image);
        void SetProcessedImage(IImageHandle image);
    }
}
