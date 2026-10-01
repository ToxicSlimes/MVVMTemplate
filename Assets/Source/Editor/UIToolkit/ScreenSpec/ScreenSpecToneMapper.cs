namespace Core.Editor.UIToolkit.ScreenSpec
{
    public static class ScreenSpecToneMapper
    {
        public static string ToButtonClasses(string tone, string extraClass = null)
        {
            var btn = tone switch
            {
                "green" => "btn btn-primary",
                "neutral" => "btn btn-secondary",
                "cyan" => "btn btn-outline",
                "red" => "btn btn-danger",
                "ghost" => "btn btn-ghost",
                _ => "btn btn-outline"
            };

            if (!string.IsNullOrWhiteSpace(extraClass))
                btn += " " + extraClass.Trim();

            return btn;
        }
    }
}
