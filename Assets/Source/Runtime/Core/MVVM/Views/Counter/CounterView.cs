using Core.Abstractions.MVVM.Elements;
using Core.Abstractions.MVVM.Views;
using Core.Abstractions.Popup;
using Core.Data.Domain.Models.Elements;
using Core.Data.Domain.Models.Elements.Samples;
using Core.Data.UI;
using Core.MVVM.ViewModels.Counter;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.MVVM.Views.Counter
{
    /// <summary>
    /// TEMPLATE EXAMPLE: View with two modes — Counter Demo and Element Gallery.
    /// 
    /// Demonstrates:
    /// 1. ReactiveProperty binding (ViewModel → View)
    /// 2. ReactiveCommand binding (View → ViewModel)
    /// 3. PopupTrigger (Hover tooltips + Click confirm)
    /// 4. BindableElement usage with sample data (via IBindableElement interface)
    /// 5. Panel switching driven by ViewModel state
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
        [SerializeField] private Button _toggleGalleryButton;
        [SerializeField] private TMP_Text _toggleButtonLabel;

        [Header("Panels")]
        [SerializeField] private GameObject _counterPanel;
        [SerializeField] private GameObject _galleryPanel;

        [Header("Tooltips (Hover)")]
        [SerializeField] private PopupTrigger _incrementTooltip;
        [SerializeField] private PopupTrigger _decrementTooltip;

        [Header("Confirm Popup (Click)")]
        [SerializeField] private PopupTrigger _resetConfirm;

        [Header("Gallery Elements")]
        [SerializeField] private GameObject _sampleCard;
        [SerializeField] private GameObject _sampleStatBar;
        [SerializeField] private GameObject _sampleActionItem;

        protected override void OnBind(ref DisposableBag disposables)
        {
            // === COUNTER MODE ===

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

            if (_resetButton != null)
            {
                ViewModel.CounterValue
                    .Subscribe(_resetButton, static (v, btn) => btn.interactable = v != 0)
                    .AddTo(ref disposables);
            }

            // === MODE TOGGLE ===

            if (_toggleGalleryButton != null)
            {
                _toggleGalleryButton.OnClickAsObservable()
                    .Subscribe(ViewModel.ToggleGalleryCommand, static (_, cmd) => cmd.Execute(Unit.Default))
                    .AddTo(ref disposables);
            }

            if (_toggleButtonLabel != null)
            {
                ViewModel.ToggleButtonText
                    .Subscribe(_toggleButtonLabel, static (text, tmp) => tmp.text = text)
                    .AddTo(ref disposables);
            }

            ViewModel.IsGalleryMode
                .Subscribe(this, static (isGallery, self) =>
                {
                    if (self._counterPanel != null) self._counterPanel.SetActive(!isGallery);
                    if (self._galleryPanel != null) self._galleryPanel.SetActive(isGallery);
                })
                .AddTo(ref disposables);

            SetupPopups();
            SetupGalleryElements();
        }

        private void SetupPopups()
        {
            if (_incrementTooltip != null)
                _incrementTooltip.SetTooltipData("Increment", "Add 1 to the counter");

            if (_decrementTooltip != null)
                _decrementTooltip.SetTooltipData("Decrement", "Subtract 1 from the counter");

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

        private void SetupGalleryElements()
        {
            // TEMPLATE EXAMPLE: Bind sample elements via IBindableElement interface
            // View uses GameObject refs + GetComponent — no direct Presentation dependency

            BindElement(_sampleCard, new TextCardData
            {
                Id = "sample-card",
                Title = "TextCard Element",
                Description = "A card with title, description, and optional icon."
            });

            BindElement(_sampleStatBar, new StatBarData
            {
                Id = "sample-stat",
                Label = "Health",
                CurrentValue = 75f,
                MaxValue = 100f
            });

            BindElement(_sampleActionItem, new ActionListItemData
            {
                Id = "sample-action",
                Title = "Action Item",
                Subtitle = "Tap the button to trigger an action",
                ActionLabel = "Execute",
                IsActionEnabled = true
            });
        }

        private static void BindElement<TData>(GameObject go, TData data) where TData : struct, IElementData
        {
            if (go == null) return;
            var element = go.GetComponent<IBindableElement<TData>>();
            element?.Bind(data);
        }
    }
}
