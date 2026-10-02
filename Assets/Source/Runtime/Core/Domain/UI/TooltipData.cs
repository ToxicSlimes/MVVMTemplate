
namespace Core.Domain.UI
{
    public struct TooltipData : IPopupData
    {
        public string Title;
        public string Description;

        public TooltipData(string title, string description = null)
        {
            Title = title;
            Description = description;
        }

        public static TooltipData Simple(string text) => new(text);
    }
}
