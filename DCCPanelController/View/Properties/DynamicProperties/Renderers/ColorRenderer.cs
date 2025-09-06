namespace DCCPanelController.View.Properties.DynamicProperties;

internal sealed class ColorRenderer : IPropertyRenderer {
    private static readonly Color[] Palette = new[] { Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow, Colors.Orange, Colors.Purple, Colors.White, Colors.Black };
    public bool CanRender(PropertyContext ctx) => ctx.EditorKind == "color";

    public object CreateView(PropertyContext ctx) {
        var row = ctx.Row;
        var btn = new Button { Text = row.HasMixedValues ? "— mixed —" : "Select", BackgroundColor = ToColor(row.OriginalValue) };
        btn.Clicked += (s, e) => {
            var next = NextColor(btn.BackgroundColor);
            btn.BackgroundColor = next;
            RenderBinding.SetValue(row, next);
            btn.Text = "Select";
        };
        btn.IsEnabled = !(ctx.Mode == AppMode.Run && row.Field.Meta.IsReadOnlyInRunMode);
        return PropertyRenderers.WrapWithLabel(row, btn, 150);
        static Color ToColor(object? o) => o is Color c ? c : Colors.Transparent;

        static Color NextColor(Color current) {
            var idx = Array.IndexOf(Palette, current);
            idx = (idx + 1 + Palette.Length) % Palette.Length;
            return Palette[idx];
        }
    }
}
