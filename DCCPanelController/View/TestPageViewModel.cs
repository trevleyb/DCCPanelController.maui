using Common.Logging.Factory;
using CommunityToolkit.Mvvm.ComponentModel;
using Font = Microsoft.Maui.Graphics.Font;

namespace DCCPanelController.View;

public struct SomeTestItem {
    public required string Name { get; set; }
    public required string Id { get; set; }
}

public partial class TestPageViewModel : ObservableObject {

    [ObservableProperty] private string? _selectedFontAlias = "OpenSansRegular";
    [ObservableProperty] private string? _selectedFontStyle;
    [ObservableProperty] private string? _selectedFontFamily;
    [ObservableProperty] private int     _fontSize = 12;

    public Microsoft.Maui.Controls.View TestFonts() {

        var fontsToTest = new[]
        {
            "OpenSansRegular",       // your alias (from <MauiFont ... Alias="OpenSansRegular" />)
            "OpenSansSemibold",      // another alias
            "OpenSans-Semibold",     // another alias
            "OpenSansSemibold.ttf",  // another alias
            "OpenSans-Semibold.ttf", // another alias
            "OrbitrongBold",         // another alias
            "Orbitrong-Bold",        // another alias
            "OrbitrongBold.ttf",    // another alias
            "Orbitrong-Bold.ttf",    // another alias
            "Helvetica"              // system font (iOS/macOS); use "sans-serif" on Android
        };

        return new GraphicsView
        {
            HeightRequest = 260,
            WidthRequest  = 600,
            Drawable      = new FontProbeDrawable(fontsToTest),
            BackgroundColor = Colors.White
        };
    }
    
    private sealed class FontProbeDrawable : IDrawable
    {
        private readonly string[] _fonts;
        public FontProbeDrawable(string[] fonts) => _fonts = fonts;

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.SaveState();
            canvas.Antialias = true;

            // Background
            canvas.FillColor = Colors.White;
            canvas.FillRectangle(dirtyRect);

            // Layout
            float rowH = dirtyRect.Height / Math.Max(1, _fonts.Length);
            float pad  = 6f;

            // Title
            canvas.FontColor = Colors.Gray;
            canvas.FontSize  = 12;
            canvas.DrawString("Font probe: verifying canvas draws with the specified face",
                new RectF(dirtyRect.X, dirtyRect.Y, dirtyRect.Width, 18),
                HorizontalAlignment.Center, VerticalAlignment.Center);

            for (int i = 0; i < _fonts.Length; i++)
            {
                var rowY   = dirtyRect.Y + i * rowH;
                var row    = new RectF(dirtyRect.X, rowY, dirtyRect.Width, rowH);

                // Row frame (visual aid)
                canvas.StrokeColor = Colors.LightGray;
                canvas.StrokeSize  = 1;
                canvas.DrawRectangle(row);

                // Try the font
                var name = _fonts[i];
                canvas.Font      = new Font(name);
                canvas.FontColor = Colors.Black;

                // Two sizes to make differences obvious
                float bigSize   = 28f;
                float smallSize = 16f;

                // Top line: big
                canvas.FontSize = bigSize;
                canvas.DrawString($"{name} (big)",
                    new RectF(row.X + pad, row.Y + pad, row.Width - pad * 2, rowH / 2 - pad),
                    HorizontalAlignment.Center, VerticalAlignment.Center);

                // Bottom line: small
                canvas.FontSize = smallSize;
                canvas.DrawString($"{name} (small)",
                    new RectF(row.X + pad, row.Y + rowH / 2, row.Width - pad * 2, rowH / 2 - pad),
                    HorizontalAlignment.Center, VerticalAlignment.Center);
            }

            canvas.RestoreState();
        }
    }

}