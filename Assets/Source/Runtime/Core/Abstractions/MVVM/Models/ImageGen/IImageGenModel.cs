using System;
using System.Collections.Generic;
using Core.Abstractions.MVVM.Models;
using Core.Data.Domain.Models.ImageGen;

namespace Core.Abstractions.MVVM.Models.ImageGen
{
    public interface IImageGenModel : IModel
    {
        List<ImageGenMessage> Messages { get; }

        ImageGenSettings SelectedSettings { get; set; }

        bool IsGenerating { get; set; }

        string StatusText { get; set; }

        List<IImageHandle> PendingAttachments { get; set; }

        event Action<List<ImageGenMessage>> MessagesChanged;

        event Action<ImageGenSettings> SelectedSettingsChanged;

        event Action<bool> IsGeneratingChanged;

        event Action<string> StatusTextChanged;

        event Action<List<IImageHandle>> PendingAttachmentsChanged;

        void AddMessage(ImageGenMessage message);

        void UpdateMessage(string id, ImageGenMessage updated);

        ImageGenMessage FindMessage(string id);

        void RemoveMessage(string id);

        void ClearMessages();

        ImageGenMessage GetLastMessage();

        ImageGenMessage GetLastAssistantMessage();

        void AddAttachment(IImageHandle image);

        void ClearAttachments();

        void RemoveAttachment(int index);

        void RemoveAttachment(IImageHandle image);
    }
}
