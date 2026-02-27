using Core.Abstractions.MVVM.Views;
using Core.Abstractions.Popup;
using Core.Data.UI;
using Core.MVVM.ViewModels.Counter;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.MVVM.Views.Counter
{
    /// <summary>
    /// TEMPLATE EXAMPLE: View — the UI layer.
    /// 
    /// Key patterns demonstrated:
    /// 1. BindableView&lt;TViewModel&gt; — typed binding to a specific ViewModel
    /// 2. OnBind — subscribe to ViewModel properties and wire up buttons
    /// 3. PopupTrigger (Hover) — tooltip on hover for +/- buttons
    /// 4. PopupTrigger (Click) — confirm popup on Reset button click
    /// 5. No business logic here — only UI binding
    /// </summary>
    public class CounterView : BindableView<CounterViewModel>
    {
        [Header("Display")]
        [SerializeField] private TMP_Text _counterText;
        [SerializeField] private TMP_Text _incrementsText;

        [Header("Buttons")]
        [SerializeField] private Button _incrementButton;
        [SerializeField] private Button _decrementButton;
        [SerializeField] private Button _resetButton;

        [Header("Tooltips (Hover)")]
        [SerializeField] private PopupTrigger _incrementTooltip;
        [SerializeField] private PopupTrigger _decrementTooltip;

        [Header("Confirm Popup (Click)")]
        [SerializeField] private PopupTrigger _resetConfirm;

        protected override void OnBind(ref DisposableBag disposables)
        {
            // ViewModel → View: reactive property changes update UI
            if (_counterText != null)
            {
                ViewModel.DisplayText
                    .Subscribe(_counterText, static (text, tmp) => tmp.text = text)
                    .AddTo(ref disposables);
            }

            if (_incrementsText != null)
            {
                ViewModel.TotalIncrements
                    .Subscribe(_incrementsText, static (count, tmp) => tmp.text = $"Total actions: {count}")
                    .AddTo(ref disposables);
            }

            // View → ViewModel: button clicks invoke commands
            if (_incrementButton != null)
            {
                _incrementButton.OnClickAsObservable()
                    .Subscribe(ViewModel.IncrementCommand, static (_, cmd) => cmd.Execute(Unit.Default))
                    .AddTo(ref disposables);
            }

            if (_decrementButton != null)
            {
                _decrementButton.OnClickAsObservable()
                    .Subscribe(ViewModel.DecrementCommand, static (_, cmd) => cmd.Execute(Unit.Default))
                    .AddTo(ref disposables);
            }

            // Reset button — NO direct command execution!
            // PopupTrigger (Click) shows ConfirmPopup → OnConfirm callback executes command
            if (_resetButton != null)
            {
                ViewModel.CounterValue
                    .Subscribe(_resetButton, static (v, btn) => btn.interactable = v != 0)
                    .AddTo(ref disposables);
            }

            SetupPopups();
        }

        private void SetupPopups()
        {
            // TEMPLATE EXAMPLE: Hover tooltips — simple text
            if (_incrementTooltip != null)
                _incrementTooltip.SetTooltipData("Increment", "Add 1 to the counter");

            if (_decrementTooltip != null)
                _decrementTooltip.SetTooltipData("Decrement", "Subtract 1 from the counter");

            // TEMPLATE EXAMPLE: Click confirm popup — with callback
            if (_resetConfirm != null)
            {
                var vm = ViewModel;
                _resetConfirm.SetDataProvider(() => new ConfirmPopupData
                {
                    Title = "Reset Counter?",
                    Message = $"Current value: {vm.CounterValue.Value}. This will reset to zero.",
                    ConfirmText = "Reset",
                    CancelText = "Keep",
                    OnConfirm = () => vm.ResetCommand.Execute(Unit.Default)
                });
            }
        }
    }
}
