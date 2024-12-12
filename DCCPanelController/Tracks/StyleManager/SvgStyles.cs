using System.Diagnostics;
using DCCPanelController.Model;
using DCCPanelController.Tracks.ImageManager;

namespace DCCPanelController.Tracks.StyleManager;

public static class SvgStyles {
    
    public static SvgStyle GetStyle(TrackStyleType styleType, TrackStyleImage styleImage, TrackStyleAttribute attribute, PanelDefaults? defaults) {
        defaults ??= new PanelDefaults();
        return ApplyStyleAttributes(GetStyle(styleType, styleImage, defaults), attribute, defaults);
    }

    /// <summary>
    ///     Get a Style based on the Track Type and the DisplayImage Type
    /// </summary>
    /// <param name="styleType">Mainline, Branchline or Button</param>
    /// <param name="styleImage">Normal, Straight, Diverging</param>
    /// <param name="defaults">Reference to the Parent or Owner Panel defaults</param>
    /// <returns></returns>
    public static SvgStyle GetStyle(TrackStyleType styleType, TrackStyleImage styleImage, PanelDefaults? defaults) {
        defaults ??= new PanelDefaults();
        var style = new SvgStyleBuilder();
        
        // Unknown, Normal and Default all return the Default Style
        // --------------------------------------------------------
        switch (styleType) {
        case TrackStyleType.Mainline:
            style = new SvgStyleBuilder()
              .AddElement(e => e.WithName(SvgElementEnum.Border).WithColor(defaults.BorderColor).Visible())
              .AddElement(e => e.WithName(SvgElementEnum.Track).WithColor(defaults.MainLineColor).Visible())
              .AddElement(e => e.WithName(SvgElementEnum.TrackDiverging).WithColor(defaults.DivergingColor).Visible())
              .AddElement(e => e.WithName(SvgElementEnum.Terminator).WithColor(defaults.TerminatorColor).Visible())
              .AddElement(e => e.WithName(SvgElementEnum.Continuation).WithColor(defaults.ContinuationColor).Visible())
              .AddElement(e => e.WithName(SvgElementEnum.Occupied).Hidden())
              .AddElement(e => e.WithName(SvgElementEnum.Dashline).Hidden());
            break;

        case TrackStyleType.Branchline:
            style = new SvgStyleBuilder()
                .AddElement(e => e.WithName(SvgElementEnum.Border).Hidden())
                .AddElement(e => e.WithName(SvgElementEnum.Track).WithColor(defaults.BranchLineColor).Visible())
                .AddElement(e => e.WithName(SvgElementEnum.TrackDiverging).WithColor(defaults.DivergingColor).Visible())
                .AddElement(e => e.WithName(SvgElementEnum.Terminator).WithColor(defaults.TerminatorColor).Visible())
                .AddElement(e => e.WithName(SvgElementEnum.Continuation).WithColor(defaults.ContinuationColor).Visible())
                .AddElement(e => e.WithName(SvgElementEnum.Occupied).Hidden())
                .AddElement(e => e.WithName(SvgElementEnum.Dashline).Hidden());
            break;
        
        case TrackStyleType.Button or 
            TrackStyleType.Text:
            style = new SvgStyleBuilder()
                    .AddElement(e => e.WithName(SvgElementEnum.Button).Visible().WithColor(defaults.ButtonColor))
                    .AddElement(e => e.WithName(SvgElementEnum.Border).Visible().WithColor(defaults.ButtonBorder))
                    .AddElement(e => e.WithName(SvgElementEnum.ButtonOutline).Visible().WithColor(Colors.White));

            if (styleImage == TrackStyleImage.Active) {
                style.AddElement(e => e.WithName(SvgElementEnum.Button).Visible().WithColor(defaults.ButtonOnColor))
                     .AddElement(e => e.WithName(SvgElementEnum.Border).Visible().WithColor(defaults.ButtonOnBorder));
                
            } else if (styleImage == TrackStyleImage.InActive) {
                style.AddElement(e => e.WithName(SvgElementEnum.Button).Visible().WithColor(defaults.ButtonOffColor))
                     .AddElement(e => e.WithName(SvgElementEnum.Border).Visible().WithColor(defaults.ButtonOffBorder));
            }
            break;

        default:
            Trace.TraceWarning($"Unknown Track Style Type: {styleType}");
            break;
        }
        return style.Build();
    }

    /// <summary>
    ///     Apply an additional attribute to a Style such as marking it
    ///     hidden, or Occupied.
    /// </summary>
    /// <param name="style">The style to add elements to</param>
    /// <param name="attribute">The attribute to apply</param>
    /// <param name="defaults">Reference to the Panel that owns this track piece defaults</param>
    /// <returns></returns>
    public static SvgStyle ApplyStyleAttributes(SvgStyle style, TrackStyleAttribute attribute, PanelDefaults? defaults) {
        defaults ??= new PanelDefaults();
        return attribute switch {
            TrackStyleAttribute.Occupied => new SvgStyleBuilder()
                .AddExistingStyle(style).AddElement(e => e.WithName(SvgElementEnum.Occupied).Visible().WithColor(defaults.OccupiedColor)).Build(),
            TrackStyleAttribute.Hidden   => new SvgStyleBuilder()
                .AddExistingStyle(style).AddElement(e => e.WithName(SvgElementEnum.Dashline).Visible().WithColor(defaults.HiddenColor)).Build(),
            TrackStyleAttribute.Normal   => new SvgStyleBuilder()
                .AddExistingStyle(style).AddElement(e => e.WithName(SvgElementEnum.Dashline).Hidden().WithName(SvgElementEnum.Occupied).Hidden()).Build(),
            _ => style
        };
    }

    public static SvgStyle AddTextToStyle(SvgStyle style, string text) {
        return new SvgStyleBuilder().AddExistingStyle(style).AddElement(e => e.WithName(SvgElementEnum.Text).Text(text)).Build();
    }
}
