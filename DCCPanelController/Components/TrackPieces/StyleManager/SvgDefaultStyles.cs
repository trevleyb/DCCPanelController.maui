using DCCPanelController.Components.TrackPieces.ImageManager;

namespace DCCPanelController.Components.TrackPieces.StyleManager;

public class SvgDefaultStyles {

    public static void AddDefaultStyles(Dictionary<string, SvgStyle> styles) {
        AddStyle("Mainline", styles);
        AddStyle("Branchline", styles);
        AddStyle("Mainline-Hidden", styles);
        AddStyle("Branchline-Hidden", styles);
        AddStyle("Occupied", styles);
    }

    public static void AddStyle(string styleName, Dictionary<string, SvgStyle> styles) {
        if (styles.ContainsKey(styleName)) return;
        styles.Add(styleName, new SvgStyle(styleName));
    }
    
    public static SvgStyle? GetStyle(string styleName) {
        switch (styleName.ToLowerInvariant()) {
        case "default":
            break;
        case "mainline":
            return new SvgStyleBuilder(styleName)
                .AddElement(e => e.WithName(SvgElementEnum.Border).WithColor(Colors.Black).Visible())
                .AddElement(e => e.WithName(SvgElementEnum.Track).WithColor(Colors.DarkGreen).Visible())
                .AddElement(e => e.WithName(SvgElementEnum.TrackDiverging).WithColor(Colors.DarkGreen).Visible())
                .AddElement(e => e.WithName(SvgElementEnum.Terminator).WithColor(Colors.Black).Visible())
                .AddElement(e => e.WithName(SvgElementEnum.Continuation).WithColor(Colors.Black).Visible())
                .AddElement(e => e.WithName(SvgElementEnum.Occupied).Hidden())
                .AddElement(e => e.WithName(SvgElementEnum.Dashline).Hidden())
                .Build();

        case "branchline":
            return new SvgStyleBuilder(styleName)
                .AddElement(e => e.WithName(SvgElementEnum.Border).Hidden())
                .AddElement(e => e.WithName(SvgElementEnum.Track).WithColor(Colors.DarkGray).Visible())
                .AddElement(e => e.WithName(SvgElementEnum.TrackDiverging).WithColor(Colors.DarkGray).Visible())
                .AddElement(e => e.WithName(SvgElementEnum.Terminator).WithColor(Colors.DarkGray).Visible())
                .AddElement(e => e.WithName(SvgElementEnum.Continuation).WithColor(Colors.DarkGray).Visible())
                .AddElement(e => e.WithName(SvgElementEnum.Occupied).Hidden())
                .AddElement(e => e.WithName(SvgElementEnum.Dashline).Hidden())
                .Build();
        
        case "mainlinedashed" or "mainlinehidden" or "mainline-hidden":
            return new SvgStyleBuilder(styleName)
                .AddExistingStyle(SvgStyles.GetStyle("Mainline"))
                .AddElement(e => e.WithName(SvgElementEnum.Dashline).Visible().WithColor(Colors.White))
                .Build();
            
        case "branchlinedashed" or "branchlinehidden" or "branchline-hidden":
            return new SvgStyleBuilder(styleName)
                .AddExistingStyle(SvgStyles.GetStyle("Branchline"))
                .AddElement(e => e.WithName(SvgElementEnum.Dashline).Visible().WithColor(Colors.White))
                .Build();

        case "track-occupied":
            return new SvgStyleBuilder(styleName)
                .AddElement(e => e.WithName(SvgElementEnum.Occupied).Visible().WithColor(Colors.Red))
                .Build();

        case "track-free":
            return new SvgStyleBuilder(styleName)
                .AddElement(e => e.WithName(SvgElementEnum.Occupied).Hidden())
                .Build();
        
        case "button-active":
            return new SvgStyleBuilder(styleName)
                .AddElement(e => e.WithName(SvgElementEnum.Button).Visible().WithColor(Colors.Green))
                .AddElement(e => e.WithName(SvgElementEnum.ButtonOutline).Visible().WithColor(Colors.Black))
                .Build();

        case "button-inactive":
            return new SvgStyleBuilder(styleName)
                .AddElement(e => e.WithName(SvgElementEnum.Button).Visible().WithColor(Colors.Red))
                .AddElement(e => e.WithName(SvgElementEnum.ButtonOutline).Visible().WithColor(Colors.Red))
                .Build();
        }

        return null;
    } 
}