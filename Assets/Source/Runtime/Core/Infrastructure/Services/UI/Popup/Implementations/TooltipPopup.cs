using LitMotion;
using Core.Data.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Infrastructure.Popup.Implementations
{
    public class TooltipPopup : PopupBase<TooltipData>
    {
        [Header("Tooltip Elements")]
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private GameObject _descriptionContainer;

        protected override void OnDataSet(TooltipData data)
        {
            if (_titleText != null)
                _titleText.text = data.Title;

            var hasDescription = !string.IsNullOrEmpty(data.Description);
            
            if (_descriptionContainer != null)
                _descriptionContainer.SetActive(hasDescription);

            if (_descriptionText != null)
                _descriptionText.text = hasDescription ? data.Description : string.Empty;

            RebuildLayout();
        }

        private void RebuildLayout()
        {
            if (_descriptionText != null)
                _descriptionText.ForceMeshUpdate();
            
            if (_titleText != null)
                _titleText.ForceMeshUpdate();

            Canvas.ForceUpdateCanvases();
            
            if (_contentRoot != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(_contentRoot);
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(RectTransform);
        }

        protected override void OnDataCleared()
        {
            if (_titleText != null)
                _titleText.text = string.Empty;

            if (_descriptionText != null)
                _descriptionText.text = string.Empty;

            if (_descriptionContainer != null)
                _descriptionContainer.SetActive(false);

            RebuildLayout();
        }

        protected override PopupAnimator GetShowAnimation()
        {
            return Animate()
                .FadeIn(0.15f, Ease.OutQuad)
                .Scale(0.85f, 1f, 0.15f, Ease.OutBack);
        }

        protected override PopupAnimator GetHideAnimation()
        {
            return Animate()
                .FadeOut(0.1f, Ease.InQuad);
        }
    }
}
