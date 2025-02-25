using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.View.EditProperties.Attributes;
using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.Model.Tracks;

public partial class TrackText(Panel? parent = null) : TrackBase(parent), ITrackSymbol, ITrack {

    [ObservableProperty]
    private string _name = "Text Block";

    [ObservableProperty] [property: EditableString(Name = "Text", Description = "Text to Display")]
    private string _text = "";
    
    [ObservableProperty] [property: EditableInt(Name = "Font Size", Description = "Font Size", Group = "Attributes")]
    private int _fontSize = 12;

    [ObservableProperty] [property: EditableBool(Name = "Bold", Description = "Bold Text", Group = "Attributes")]
    private bool _fontBold;

    [property: EditableInt(Name = "Width", Description = "Width of the Image", Group = "Attributes")]
    public new int Width {
        get => base.Width;
        set => base.Width = value;
    }

    [ObservableProperty] [property: EditableEnum(Name = "Horizontal", Description = "Horizontal Justification of the Text", Group = "Attributes")]
    private TextAlignment _horizontalJustification = TextAlignment.Center;

    [ObservableProperty] [property: EditableEnum(Name = "Vertical", Description = "Vertical Justification of the Text", Group = "Attributes")]
    private TextAlignment _verticalJustification = TextAlignment.Center;
    
    [ObservableProperty] [property: EditableColor(Name = "Font Color", Description = "Font Color", Group = "Colors")]
    private Color _textColor = Colors.Black;

    [ObservableProperty] [property: EditableColor(Name = "Background", Description = "Background Color", Group = "Colors")]
    private Color _backgroundColor = Colors.Transparent;

    [ObservableProperty] [property: EditableInt(Name = "Border Width", Description = "Border With", Group = "Border")]
    private int _borderWidth = 0;

    [ObservableProperty] [property: EditableInt(Name = "Border Radius", Description = "Rounder Corners on the Border", Group = "Border")]
    private int _borderRadius = 0;

    [ObservableProperty] [property: EditableColor(Name = "Border Color", Description = "Border Color", Group = "Border")]
    private Color _borderColor = Colors.Transparent;

    public TrackText() : this(null) { }

    private int MaxGridWidth => Parent is not null ? CalculateMaxGridWidth(Parent.Rows, Parent.Cols, Width, X, Y, TrackRotation) : Width;

    public ITrack Clone(Panel parent) {
        return Clone<TrackText>(parent);
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
            FontFamily = !FontBold ? "OpenSansRegular" : "OpenSansSemibold",
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

        return BorderWidth <= 0 ? label : new Border() {
            Content = label,
            InputTransparent = passthrough,
            RotationX = TrackRotation,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Fill,
            WidthRequest = gridSize * MaxGridWidth,
            HeightRequest = gridSize,
            StrokeThickness = BorderWidth,
            Stroke = BorderColor,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(BorderRadius) }
        };
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