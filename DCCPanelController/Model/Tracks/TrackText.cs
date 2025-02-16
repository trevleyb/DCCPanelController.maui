using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.EditableProperties;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.Model.Tracks;

public partial class TrackText(Panel? parent = null) : TrackBase(parent), ITrackSymbol, ITrackPiece {

    [ObservableProperty] [property: EditableColorProperty(Name = "Background", Description = "Background Color", Group = "Colors")]
    private Color _backgroundColor = Colors.Transparent;

    [ObservableProperty] [property: EditableBoolProperty(Name = "Bold", Description = "Bold Text", Group = "Attributes")]
    private bool _bold;

    [ObservableProperty] [property: EditableIntProperty(Name = "Font Size", Description = "Font Size", Group = "Attributes")]
    private int _fontSize = 12;

    [ObservableProperty] [property: EditableEnumProperty(Name = "Horizontal", Description = "Horizontal Justification of the Text", Group = "Attributes")]
    private TextAlignment _horizontalJustification = TextAlignment.Center;

    [ObservableProperty] [property: EditableStringProperty(Name = "Text", Description = "Text to Display")]
    private string _text = "";

    [ObservableProperty] [property: EditableColorProperty(Name = "Font Color", Description = "Font Color", Group = "Colors")]
    private Color _textColor = Colors.Black;

    [ObservableProperty] [property: EditableIntProperty(Name = "Width", Description = "Text Grid Width", Group = "Attributes")]
    private int _textWidth = 2;

    [ObservableProperty] [property: EditableEnumProperty(Name = "Vertical", Description = "Vertical Justification of the Text", Group = "Attributes")]
    private TextAlignment _verticalJustification = TextAlignment.Center;

    public TrackText() : this(null) { }

    private int MaxGridWidth => Parent is not null ? CalculateMaxGridWidth(Parent.Rows, Parent.Cols, TextWidth, X, Y, TrackRotation) : TextWidth;

    public ITrackPiece Clone(Panel parent) {
        return new TrackText(parent);
    }

    protected override void Setup() {
        Layer = 2;
        Name = "Text";
        RotationIncrement = 90;
        AddImageSourceAndRotation(TrackStyleImageEnum.Symbol, "Text", (0, 0), (90, 90), (180, 180), (270, 270));
        AddImageSourceAndRotation(TrackStyleImageEnum.Normal, "Text", (0, 0), (90, 90), (180, 180), (270, 270));
    }

    protected override ImageSource GetViewForSymbol(double gridSize) {
        return CreateImageView(TrackStyleImageEnum.Symbol, TrackRotation, gridSize).Image;
    }

    protected override IView GetViewForTrack(double gridSize, bool passthrough = false) {
        if (string.IsNullOrEmpty(Text)) {
            var image = CreateImageView(TrackStyleImageEnum.Normal, TrackRotation, gridSize, passthrough);
            return CreateViewFromImage(image.Image, image.Rotation, gridSize, passthrough);
        }

        var label = new Label {
            Text = Text,
            FontSize = FontSize,
            FontAttributes = Bold ? FontAttributes.Bold : FontAttributes.None,
            HorizontalTextAlignment = HorizontalJustification,
            VerticalTextAlignment = VerticalJustification,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Fill,
            TextColor = TextColor,
            BackgroundColor = BackgroundColor,
            ZIndex = Layer,
            RotationX = TrackRotation,
            LineBreakMode = LineBreakMode.TailTruncation,
            InputTransparent = passthrough,

            WidthRequest = gridSize * MaxGridWidth,
            HeightRequest = gridSize

            //WidthRequest = TextWidthRequest(gridSize),
            //HeightRequest = TextHeightRequest(gridSize)
        };

        return label;
    }

    private double TextWidthRequest(double gridSize) {
        return TrackRotation % 360 == 0 || TrackRotation % 360 == 180 ? gridSize * MaxGridWidth : gridSize;
    }

    private double TextHeightRequest(double gridSize) {
        return TrackRotation % 360 == 90 || TrackRotation % 360 == 270 ? gridSize * MaxGridWidth : gridSize;
    }

    protected (ImageSource Image, int Rotation) CreateImageView(TrackStyleImageEnum trackStyle, int rotation, double gridSize, bool passthrough = false) {
        // Find the appropriate image reference for the details we have
        // ---------------------------------------------------------------------------------------------------
        var trackInfo = StyleTrackImages.GetTrackImageSourceAndRotation(trackStyle, rotation);
        var imageInfo = SvgImages.GetImage(trackInfo.ImageSource);
        ImageRotation = trackInfo.ImageRotation;
        TrackRotation = trackInfo.TrackRotation;
        var style = SvgStyles.GetStyle(TrackStyleTypeEnum.Text, TrackStyleImageEnum.Normal, Parent?.Defaults);
        ActiveImage = imageInfo.ApplyStyle(style);
        return (ActiveImage.Image, trackInfo.ImageRotation);
    }

    private int CalculateMaxGridWidth(int rows, int cols, int textWidth, int x, int y, int rotation) {
        var maxWidth = textWidth;
        maxWidth = (rotation % 360) switch {
            // Normalize rotation (handle values >= 360 or < 0)
            0   => Math.Min(maxWidth, cols - x), // Horizontal, to the right
            90  => Math.Min(maxWidth, rows - y), // Vertical, downward
            180 => Math.Min(maxWidth, x + 1),    // Horizontal, to the left
            270 => Math.Min(maxWidth, y + 1),    // Vertical, upward
            _   => throw new ArgumentException("Rotation not supported. Must be 0, 90, 180, or 270 degrees.")
        };

        if (cols - x < 0 || rows - y < 0) {
            Console.WriteLine("Text too wide for grid");
        }

        return maxWidth;
    }
}