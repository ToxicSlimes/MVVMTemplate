namespace Core.Abstractions.UI
{
    public interface ITooltipTarget
    {
        void SetTooltipData(string title, string description = null);
    }
}
