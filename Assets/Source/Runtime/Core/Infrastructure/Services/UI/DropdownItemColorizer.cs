using UnityEngine;
using UnityEngine.UI;

namespace Core.Presentation.UI
{
    public class DropdownItemColorizer : MonoBehaviour
    {
        private readonly static Color[] AlternatingColors = new Color[]
        {
            new Color(0.15f, 0.15f, 0.15f, 1f),
            new Color(0.2f, 0.18f, 0.18f, 1f),
            new Color(0.18f, 0.2f, 0.18f, 1f),
            new Color(0.18f, 0.18f, 0.2f, 1f)
        };

        private Image _backgroundImage;

        public void SetBackgroundImage(Image image)
        {
            _backgroundImage = image;
        }

        private void Start()
        {
            if (_backgroundImage == null)
                return;
            
            int siblingIndex = transform.GetSiblingIndex();
            int colorIndex = siblingIndex % AlternatingColors.Length;
            _backgroundImage.color = AlternatingColors[colorIndex];
        }
    }
}
