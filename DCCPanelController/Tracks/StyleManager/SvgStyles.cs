using System.Diagnostics;
using DCCPanelController.Model;
using DCCPanelController.Tracks.ImageManager;

namespace DCCPanelController.Tracks.StyleManager;

public static class SvgStyles {
    public static SvgStyle GetStyle(TrackStyleTypeEnum styleTypeEnum,
                                    TrackStyleImageEnum styleImageEnum,
                                    TrackStyleAttributeEnum attributeEnum, Panel panel) {
        return ApplyStyleAttributes(GetStyle(styleTypeEnum, styleImageEnum, panel), attributeEnum, panel);
    }

    // Get a Style based on the Track Type and the DisplayImage Type
    public static SvgStyle GetStyle(TrackStyleTypeEnum styleTypeEnum, TrackStyleImageEnum styleImageEnum, Panel? panel) {
        if (panel == null) {
            Console.WriteLine("GetStyle: Panel is null");
            return new SvgStyle();
        }

        var style = new SvgStyleBuilder();

        // Unknown, Normal and Default all return the Default Style
        // --------------------------------------------------------
        switch (styleTypeEnum) {
        case TrackStyleTypeEnum.Mainline:
            style = new SvgStyleBuilder()
                   .AddElement(e => e.WithName(SvgElementEnum.Border).WithColor(panel.BorderColor).Visible())
                   .AddElement(e => e.WithName(SvgElementEnum.Track).WithColor(panel.MainLineColor).Visible())
                   .AddElement(e => e.WithName(SvgElementEnum.TrackDiverging).WithColor(panel.DivergingColor).Visible())
                   .AddElement(e => e.WithName(SvgElementEnum.Terminator).WithColor(panel.TerminatorColor).Visible())
                   .AddElement(e => e.WithName(SvgElementEnum.Continuation).WithColor(panel.ContinuationColor).Visible())
                   .AddElement(e => e.WithName(SvgElementEnum.Occupied).Hidden())
                   .AddElement(e => e.WithName(SvgElementEnum.Dashline).Hidden());

            break;

        case TrackStyleTypeEnum.Branchline:
            style = new SvgStyleBuilder()
                   .AddElement(e => e.WithName(SvgElementEnum.Border).Hidden())
                   .AddElement(e => e.WithName(SvgElementEnum.Track).WithColor(panel.BranchLineColor).Visible())
                   .AddElement(e => e.WithName(SvgElementEnum.TrackDiverging).WithColor(panel.DivergingColor).Visible())
                   .AddElement(e => e.WithName(SvgElementEnum.Terminator).WithColor(panel.TerminatorColor).Visible())
                   .AddElement(e => e.WithName(SvgElementEnum.Continuation).WithColor(panel.ContinuationColor).Visible())
                   .AddElement(e => e.WithName(SvgElementEnum.Occupied).Hidden())
                   .AddElement(e => e.WithName(SvgElementEnum.Dashline).Hidden());

            break;

        case TrackStyleTypeEnum.Button or
            TrackStyleTypeEnum.Text:
            style = new SvgStyleBuilder()
                   .AddElement(e => e.WithName(SvgElementEnum.Button).Visible().WithColor(panel.ButtonColor))
                   .AddElement(e => e.WithName(SvgElementEnum.Border).Visible().WithColor(panel.ButtonBorder))
                   .AddElement(e => e.WithName(SvgElementEnum.ButtonOutline).Visible().WithColor(Colors.White));

            if (styleImageEnum == TrackStyleImageEnum.Active) {
                style.AddElement(e => e.WithName(SvgElementEnum.Button).Visible().WithColor(panel.ButtonOnColor))
                     .AddElement(e => e.WithName(SvgElementEnum.Border).Visible().WithColor(panel.ButtonOnBorder));
            } else if (styleImageEnum == TrackStyleImageEnum.InActive) {
                style.AddElement(e => e.WithName(SvgElementEnum.Button).Visible().WithColor(panel.ButtonOffColor))
                     .AddElement(e => e.WithName(SvgElementEnum.Border).Visible().WithColor(panel.ButtonOffBorder));
            }

            break;

        default:
            Trace.TraceWarning($"Unknown Track Style Type: {styleTypeEnum}");
            break;
        }

        return style.Build();
    }

    /// <summary>
    ///     Apply an additional attributeEnum to a Style such as marking it
    ///     hidden, or Occupied.
    /// </summary>
    /// <param name="style">The style to add elements to</param>
    /// <param name="attributeEnum">The attributeEnum to apply</param>
    /// <param name="panel">Reference to the Panel that owns this track piece panel</param>
    /// <returns></returns>
    public static SvgStyle ApplyStyleAttributes(SvgStyle style, TrackStyleAttributeEnum attributeEnum, Panel? panel) {
        if (panel == null) {
            Console.WriteLine("ApplyStyleAttributes: Panel is null");
            return style;
        }

        return attributeEnum switch {
            TrackStyleAttributeEnum.Occupied => new SvgStyleBuilder()
                                               .AddExistingStyle(style)
                                               .AddElement(e => e.WithName(SvgElementEnum.Occupied).Visible().WithColor(panel.OccupiedColor))
                                               .Build(),
            TrackStyleAttributeEnum.Hidden => new SvgStyleBuilder()
                                             .AddExistingStyle(style)
                                             .AddElement(e => e.WithName(SvgElementEnum.Dashline).Visible().WithColor(panel.HiddenColor))
                                             .Build(),
            TrackStyleAttributeEnum.Normal => new SvgStyleBuilder()
                                             .AddExistingStyle(style)
                                             .AddElement(e => e.WithName(SvgElementEnum.Dashline).Hidden().WithName(SvgElementEnum.Occupied).Hidden())
                                             .Build(),
            _ => style
        };
    }

    public static SvgStyle AddTextToStyle(SvgStyle style, string text) {
        return new SvgStyleBuilder().AddExistingStyle(style).AddElement(e => e.WithName(SvgElementEnum.Text).Text(text)).Build();
    }

    public static SvgStyle SetTextToColor(SvgStyle style, Color color) {
        return new SvgStyleBuilder().AddExistingStyle(style).AddElement(e => e.WithName(SvgElementEnum.Text).WithTextColor(color)).Build();
    }

    public static SvgStyle SetButtonColor(SvgStyle style, Color color) {
        return new SvgStyleBuilder().AddExistingStyle(style).AddElement(e => e.WithName(SvgElementEnum.Button).WithColor(color)).Build();
    }

    public static SvgStyle SetButtonOutlineColor(SvgStyle style, Color color) {
        return new SvgStyleBuilder().AddExistingStyle(style).AddElement(e => e.WithName(SvgElementEnum.Border).WithColor(color)).Build();
    }
}