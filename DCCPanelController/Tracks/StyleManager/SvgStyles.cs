using DCCPanelController.Model;
using DCCPanelController.Tracks.ImageManager;

namespace DCCPanelController.Tracks.StyleManager;

public static class SvgStyles {

    private static readonly SvgStyleBuilder MainlineDefault = new SvgStyleBuilder().AddElement(e => e.WithName(SvgElementEnum.Border).WithColor(Colors.Black).Visible()).AddElement(e => e.WithName(SvgElementEnum.Track).WithColor(Colors.ForestGreen).Visible()).AddElement(e => e.WithName(SvgElementEnum.TrackDiverging).WithColor(Colors.ForestGreen).Visible()).AddElement(e => e.WithName(SvgElementEnum.Terminator).WithColor(Colors.Black).Visible()).AddElement(e => e.WithName(SvgElementEnum.Continuation).WithColor(Colors.Black).Visible()).AddElement(e => e.WithName(SvgElementEnum.Occupied).Hidden()).AddElement(e => e.WithName(SvgElementEnum.Dashline).Hidden());
    private static readonly SvgStyleBuilder BranchLineDefault = new SvgStyleBuilder().AddElement(e => e.WithName(SvgElementEnum.Border).Hidden()).AddElement(e => e.WithName(SvgElementEnum.Track).WithColor(Colors.DarkGray).Visible()).AddElement(e => e.WithName(SvgElementEnum.TrackDiverging).WithColor(Colors.DarkGray).Visible()).AddElement(e => e.WithName(SvgElementEnum.Terminator).WithColor(Colors.DarkGray).Visible()).AddElement(e => e.WithName(SvgElementEnum.Continuation).WithColor(Colors.DarkGray).Visible()).AddElement(e => e.WithName(SvgElementEnum.Occupied).Hidden()).AddElement(e => e.WithName(SvgElementEnum.Dashline).Hidden());
    private static readonly SvgStyleBuilder ButtonDefault = new SvgStyleBuilder().AddElement(e => e.WithName(SvgElementEnum.Border).Visible().WithColor(Colors.Gray)).AddElement(e => e.WithName(SvgElementEnum.ButtonOutline).Visible().WithColor(Colors.Black));

    private static Color _mainLineColor = Colors.ForestGreen;
    private static Color _branchLineColor = Colors.DarkGray;
    private static Color _divergingColor = Colors.DarkGray;
    private static Color _buttonOnColor = Colors.Lime;
    private static Color _buttonOffColor = Colors.Crimson;
    private static Color _occupiedColor = Colors.Crimson;
    private static Color _hiddenColor = Colors.White;
    private static Color _terminatorColor = Colors.Black;

    public static SvgStyle GetStyle(TrackStyleType styleType, TrackStyleImage styleImage, TrackStyleAttribute attribute, Panel? parent = null) {
        return ApplyStyleAttributes(GetStyle(styleType, styleImage, parent), attribute);
    }
    
    /// <summary>
    ///     Get a Style based on the Track Type and the Image Type
    /// </summary>
    /// <param name="styleType">Mainline, Branchline or Button</param>
    /// <param name="styleImage">Normal, Straight, Diverging</param>
    /// <returns></returns>
    public static SvgStyle GetStyle(TrackStyleType styleType, TrackStyleImage styleImage, Panel? parent = null) {
        SetColorsFromParent(parent);
        var style = MainlineDefault;
        
        // Unknown, Normal and Default all return the Default Style
        // --------------------------------------------------------
        switch (styleType) {
        case TrackStyleType.Mainline:
            style = styleImage switch {
                TrackStyleImage.Straight  => new SvgStyleBuilder().AddExistingStyle(MainlineDefault).AddElement(e => e.WithName(SvgElementEnum.Track).WithColor(_mainLineColor).Visible()).AddElement(e => e.WithName(SvgElementEnum.TrackDiverging).WithColor(_divergingColor).Visible()),
                TrackStyleImage.Diverging => new SvgStyleBuilder().AddExistingStyle(MainlineDefault).AddElement(e => e.WithName(SvgElementEnum.Track).WithColor(_mainLineColor).Visible()).AddElement(e => e.WithName(SvgElementEnum.TrackDiverging).WithColor(_divergingColor).Visible()),
                _                         => MainlineDefault
            };
            break;

        case TrackStyleType.Branchline:
            style = styleImage switch {
                TrackStyleImage.Straight  => new SvgStyleBuilder().AddExistingStyle(BranchLineDefault).AddElement(e => e.WithName(SvgElementEnum.Track).WithColor(_branchLineColor).Visible()).AddElement(e => e.WithName(SvgElementEnum.TrackDiverging).WithColor(_divergingColor).Visible()),
                TrackStyleImage.Diverging => new SvgStyleBuilder().AddExistingStyle(BranchLineDefault).AddElement(e => e.WithName(SvgElementEnum.Track).WithColor(_branchLineColor).Visible()).AddElement(e => e.WithName(SvgElementEnum.TrackDiverging).WithColor(_divergingColor).Visible()),
                _                         => BranchLineDefault
            };
            break;
        
        case TrackStyleType.Button:
            style = styleImage switch {
                TrackStyleImage.Active   => new SvgStyleBuilder().AddElement(e => e.WithName(SvgElementEnum.Button).Visible().WithColor(_buttonOnColor)).AddElement(e => e.WithName(SvgElementEnum.ButtonOutline).Visible().WithColor(Colors.Black)),
                TrackStyleImage.InActive => new SvgStyleBuilder().AddElement(e => e.WithName(SvgElementEnum.Button).Visible().WithColor(_buttonOffColor)).AddElement(e => e.WithName(SvgElementEnum.ButtonOutline).Visible().WithColor(Colors.Black)),
                _                        => ButtonDefault
            };
            break;
        }
        style.AddElement(e => e.WithName(SvgElementEnum.Occupied).Hidden());
        style.AddElement(e => e.WithName(SvgElementEnum.Dashline).Hidden());
        return style.Build();
    }
    
    /// <summary>
    ///     Apply an additional attribute to a Style such as marking it
    ///     hidden, or Occupied.
    /// </summary>
    /// <param name="style">The style to add elements to</param>
    /// <param name="attribute">The attribute to apply</param>
    /// <returns></returns>
    public static SvgStyle ApplyStyleAttributes(SvgStyle style, TrackStyleAttribute attribute) {
        return attribute switch {
            TrackStyleAttribute.Occupied => new SvgStyleBuilder().AddExistingStyle(style).AddElement(e => e.WithName(SvgElementEnum.Occupied).Visible().WithColor(_occupiedColor)).Build(),
            TrackStyleAttribute.Hidden   => new SvgStyleBuilder().AddExistingStyle(style).AddElement(e => e.WithName(SvgElementEnum.Dashline).Visible().WithColor(_hiddenColor)).Build(),
            TrackStyleAttribute.Normal   => new SvgStyleBuilder().AddExistingStyle(style).AddElement(e => e.WithName(SvgElementEnum.Dashline).Hidden().WithName(SvgElementEnum.Occupied).Hidden()).Build(),
            _                            => style
        };
    }

    public static SvgStyle AddTextToStyle(SvgStyle style, string text) {
        return new SvgStyleBuilder().AddExistingStyle(style).AddElement(e => e.WithName(SvgElementEnum.Text).Text(text)).Build();
    }

    private static void SetColorsFromParent(Panel? parent = null) {
        if (parent is null) return;
        _mainLineColor = parent.MainLineColor;
        _branchLineColor = parent.BranchLineColor;
        _divergingColor = parent.DivergingColor;
        _buttonOnColor = parent.ButtonOnColor;
        _buttonOffColor = parent.ButtonOffColor;
        _occupiedColor = parent.OccupiedColor;
        _hiddenColor = parent.HiddenColor;
        _terminatorColor = parent.TerminatorColor;
    }
}
