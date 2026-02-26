using System;
using System.Collections.Generic;
using Core.Abstractions.MVVM.Models;
using Core.Data.Domain.Models.Character;
using Core.Data.Domain.Models.Chat;
using Core.Data.Domain.Models.Chat.Extensions;

namespace Core.Abstractions.MVVM.Models.Chat
{
    public interface IChatModel : IModel
    {
        event Action<string> StatusTextChanged;
        event Action<string> ResponseTextChanged;
        event Action<bool> IsLoadingChanged;
        event Action<CharacterModel> CharacterChanged;
        event Action<string> CurrentChatIdChanged;
        event Action<UiExtensionsModel> UiExtensionsChanged;
        event Action<List<ChatMessage>> ChatMessagesChanged;

        string StatusText { get; set; }
        string ResponseText { get; set; }
        bool IsLoading { get; set; }
        CharacterModel Character { get; set; }
        string CurrentChatId { get; set; }
        UiExtensionsModel UiExtensions { get; set; }
        List<ChatMessage> ChatMessages { get; set; }

        void AddChatMessage(ChatMessage message);
        void ClearChat();
    }
}
