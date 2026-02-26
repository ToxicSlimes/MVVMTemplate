using System;
using System.Collections.Generic;
using Core.Abstractions.MVVM.Models;
using Core.Data.Domain.Models.Character;
using Core.Data.Domain.Models.Chat;
using Core.Data.Domain.ValueObjects;

namespace Core.Abstractions.MVVM.Models.PerformanceTest
{
    public interface IPerformanceTestModel : IModel
    {
        event Action<List<CharacterModel>> AvailableCharactersChanged;
        event Action<int> SelectedCharacterIndexChanged;
        event Action<PerformanceMetrics> MetricAdded;
        event Action<List<PerformanceMetrics>> MetricsChanged;
        event Action<PerformanceStatistics> StatisticsChanged;
        event Action<bool> IsRunningChanged;
        event Action<string> StatusTextChanged;
        event Action<string> GreetingMessageChanged;
        event Action<string> ResponseMessageChanged;
        event Action<List<ChatMessage>> ChatMessagesChanged;

        List<CharacterModel> AvailableCharacters { get; set; }
        int SelectedCharacterIndex { get; set; }
        List<PerformanceMetrics> Metrics { get; }
        PerformanceStatistics Statistics { get; }
        bool IsRunning { get; set; }
        string StatusText { get; set; }
        string GreetingMessage { get; set; }
        string ResponseMessage { get; set; }
        List<ChatMessage> ChatMessages { get; }
        string CurrentChatId { get; set; }

        void AddChatMessage(ChatMessage message);
        void ClearChat();

        void AddMetric(PerformanceMetrics metric);
        void ClearMetrics();
        CharacterModel GetSelectedCharacter();
    }
}
