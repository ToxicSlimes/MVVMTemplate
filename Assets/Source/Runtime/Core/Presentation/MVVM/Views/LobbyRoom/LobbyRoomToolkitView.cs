using Core.Domain.Models.Lobby;
using Core.Presentation.MVVM.ViewModels.LobbyRoom;
using Core.UI.Toolkit;
using R3;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.Presentation.MVVM.Views.LobbyRoom
{
    public class LobbyRoomToolkitView : ToolkitPanelView<LobbyRoomViewModel>
    {
        private const string HostRole = "HOST";
        private const string PlayerRole = "PLAYER";
        private const int SlotCount = 4;

        private Label _roomCodeLabel;
        private Button _copyButton;
        private Toggle _readyToggle;
        private Button _addBotButton;
        private Button _startButton;
        private Button _leaveButton;
        private readonly VisualElement[] _slotRoots = new VisualElement[SlotCount];
        private readonly Label[] _slotNames = new Label[SlotCount];
        private readonly Label[] _slotRoles = new Label[SlotCount];
        private readonly Label[] _slotReady = new Label[SlotCount];

        protected override void BindPanel(ref DisposableBag disposables)
        {
            QueryElements();
            Bind(_roomCodeLabel, ViewModel.RoomCode, ref disposables);
            Bind(_copyButton, ViewModel.CopyCodeCommand, ref disposables);
            BindReadyToggle(ref disposables);
            Bind(_addBotButton, ViewModel.AddBotCommand, ref disposables);
            BindDisplay(_addBotButton, ViewModel.IsHost, ref disposables);
            Bind(_startButton, ViewModel.StartCommand, ref disposables);
            BindEnabled(_startButton, ViewModel.CanStart, ref disposables);
            BindDisplay(_startButton, ViewModel.IsHost, ref disposables);
            Bind(_leaveButton, ViewModel.LeaveCommand, ref disposables);
            ViewModel.RosterChanged
                .Subscribe(this, static (_, self) => self.RefreshSlots())
                .AddTo(ref disposables);
            ViewModel.StartButtonLabel
                .Subscribe(_startButton, static (label, button) => button.text = label)
                .AddTo(ref disposables);
            RefreshSlots();
        }

        private void QueryElements()
        {
            if (Root == null) return;

            _roomCodeLabel = Root.Q<Label>("room-code-label");
            _copyButton = Root.Q<Button>("copy-button");
            _readyToggle = Root.Q<Toggle>("ready-toggle");
            _addBotButton = Root.Q<Button>("add-bot-button");
            _startButton = Root.Q<Button>("start-button");
            _leaveButton = Root.Q<Button>("leave-button");

            for (int i = 0; i < SlotCount; i++)
            {
                _slotRoots[i] = Root.Q<VisualElement>($"slot-{i}");
                _slotNames[i] = Root.Q<Label>($"slot-{i}-name");
                _slotRoles[i] = Root.Q<Label>($"slot-{i}-role");
                _slotReady[i] = Root.Q<Label>($"slot-{i}-ready");
            }
        }

        private void BindReadyToggle(ref DisposableBag disposables)
        {
            if (_readyToggle == null) return;

            ViewModel.IsLocalReady
                .Subscribe(_readyToggle, static (isReady, toggle) => toggle.SetValueWithoutNotify(isReady))
                .AddTo(ref disposables);

            void OnChanged(ChangeEvent<bool> evt)
            {
                if (evt.newValue != ViewModel.IsLocalReady.Value)
                    ViewModel.ToggleReadyCommand.Execute(Unit.Default);
            }

            _readyToggle.RegisterValueChangedCallback(OnChanged);
            disposables.Add(Disposable.Create(() => _readyToggle.UnregisterValueChangedCallback(OnChanged)));
        }

        private void RefreshSlots()
        {
            var roster = ViewModel.Roster;

            for (var i = 0; i < SlotCount; i++)
            {
                var info = roster != null && i < roster.Count ? roster[i] : LobbyPlayerInfo.Empty;
                var root = _slotRoots[i];
                if (root == null) continue;

                root.style.display = info.IsOccupied ? DisplayStyle.Flex : DisplayStyle.None;
                if (!info.IsOccupied) continue;

                if (_slotNames[i] != null) _slotNames[i].text = info.Nickname;
                if (_slotRoles[i] != null) _slotRoles[i].text = info.IsHost ? HostRole : PlayerRole;
                if (_slotReady[i] != null)
                    _slotReady[i].style.display = info.IsReady ? DisplayStyle.Flex : DisplayStyle.None;

                root.style.borderLeftColor = PlayerPalette.Resolve(info.ColorIndex);
                root.style.borderLeftWidth = 4;
            }
        }
    }
}
