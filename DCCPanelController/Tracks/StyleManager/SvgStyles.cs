using System.Text.Json;
using DCCPanelController.Tracks.ImageManager;

namespace DCCPanelController.Tracks.StyleManager;

public static class SvgStyles {

    private static readonly SvgStyle MainlineDefault = new SvgStyleBuilder().AddElement(e => e.WithName(SvgElementEnum.Border).WithColor(Colors.Black).Visible()).AddElement(e => e.WithName(SvgElementEnum.Track).WithColor(Colors.ForestGreen).Visible()).AddElement(e => e.WithName(SvgElementEnum.TrackDiverging).WithColor(Colors.ForestGreen).Visible()).AddElement(e => e.WithName(SvgElementEnum.Terminator).WithColor(Colors.Black).Visible()).AddElement(e => e.WithName(SvgElementEnum.Continuation).WithColor(Colors.Black).Visible()).AddElement(e => e.WithName(SvgElementEnum.Occupied).Hidden()).AddElement(e => e.WithName(SvgElementEnum.Dashline).Hidden()).Build();
    private static readonly SvgStyle BranchlineDefault = new SvgStyleBuilder().AddElement(e => e.WithName(SvgElementEnum.Border).Hidden()).AddElement(e => e.WithName(SvgElementEnum.Track).WithColor(Colors.DarkGray).Visible()).AddElement(e => e.WithName(SvgElementEnum.TrackDiverging).WithColor(Colors.DarkGray).Visible()).AddElement(e => e.WithName(SvgElementEnum.Terminator).WithColor(Colors.DarkGray).Visible()).AddElement(e => e.WithName(SvgElementEnum.Continuation).WithColor(Colors.DarkGray).Visible()).AddElement(e => e.WithName(SvgElementEnum.Occupied).Hidden()).AddElement(e => e.WithName(SvgElementEnum.Dashline).Hidden()).Build();
    private static readonly SvgStyle ButtonDefault = new SvgStyleBuilder().AddElement(e => e.WithName(SvgElementEnum.Border).Visible().WithColor(Colors.Gray)).AddElement(e => e.WithName(SvgElementEnum.ButtonOutline).Visible().WithColor(Colors.Black)).Build();

    public static SvgStyle GetStyle(TrackStyleType styleType, TrackStyleImage styleImage, TrackStyleAttribute attribute) {
        return ApplyStyleAttributes(GetStyle(styleType, styleImage), attribute);    
    }

    /// <summary>
    /// Apply an additional attribute to a Style such as marking it
    /// hidden, or Occupied. 
    /// </summary>
    /// <param name="style">The style to add elements to</param>
    /// <param name="attribute">The attribute to apply</param>
    /// <returns></returns>
    public static SvgStyle ApplyStyleAttributes(SvgStyle style, TrackStyleAttribute attribute) {
        return attribute switch {
            TrackStyleAttribute.Occupied => new SvgStyleBuilder().AddExistingStyle(style).AddElement(e => e.WithName(SvgElementEnum.Occupied).Visible().WithColor(Colors.Red)).Build(),
            TrackStyleAttribute.Hidden   => new SvgStyleBuilder().AddExistingStyle(style).AddElement(e => e.WithName(SvgElementEnum.Dashline).Visible().WithColor(Colors.White)).Build(),
            TrackStyleAttribute.Normal   => new SvgStyleBuilder().AddExistingStyle(style).AddElement(e => e.WithName(SvgElementEnum.Dashline).Hidden().WithName(SvgElementEnum.Occupied).Hidden()).Build(),
            _                            => style
        };
    }

    /// <summary>
    /// Get a Style based on the Track Type and the Image Type
    /// </summary>
    /// <param name="styleType">Mainline, Branchline or Button</param>
    /// <param name="styleImage">Normal, Straight, Diverging</param>
    /// <returns></returns>
    public static SvgStyle GetStyle(TrackStyleType styleType, TrackStyleImage styleImage) {

        // Unknown, Normal and Default all return the Default Style
        // --------------------------------------------------------
        switch (styleType) {
        case TrackStyleType.Mainline:
            return styleImage switch {
                TrackStyleImage.Straight  => new SvgStyleBuilder().AddExistingStyle(MainlineDefault).AddElement(e => e.WithName(SvgElementEnum.Track).WithColor(Colors.ForestGreen).Visible()).AddElement(e => e.WithName(SvgElementEnum.TrackDiverging).WithColor(Colors.DarkGray).Visible()).Build(),
                TrackStyleImage.Diverging => new SvgStyleBuilder().AddExistingStyle(MainlineDefault).AddElement(e => e.WithName(SvgElementEnum.Track).WithColor(Colors.ForestGreen).Visible()).AddElement(e => e.WithName(SvgElementEnum.TrackDiverging).WithColor(Colors.DarkGray).Visible()).Build(),
                _                         => MainlineDefault,
            };
        case TrackStyleType.Branchline:
            return styleImage switch {
                TrackStyleImage.Straight  => new SvgStyleBuilder().AddExistingStyle(BranchlineDefault).AddElement(e => e.WithName(SvgElementEnum.Track).WithColor(Colors.ForestGreen).Visible()).AddElement(e => e.WithName(SvgElementEnum.TrackDiverging).WithColor(Colors.DarkGray).Visible()).Build(),
                TrackStyleImage.Diverging => new SvgStyleBuilder().AddExistingStyle(BranchlineDefault).AddElement(e => e.WithName(SvgElementEnum.Track).WithColor(Colors.ForestGreen).Visible()).AddElement(e => e.WithName(SvgElementEnum.TrackDiverging).WithColor(Colors.DarkGray).Visible()).Build(),
                _                         => BranchlineDefault,
            };
        case TrackStyleType.Button:
            return styleImage switch {
                TrackStyleImage.Active   => new SvgStyleBuilder().AddElement(e => e.WithName(SvgElementEnum.Button).Visible().WithColor(Colors.Lime)).AddElement(e => e.WithName(SvgElementEnum.ButtonOutline).Visible().WithColor(Colors.Black)).Build(),
                TrackStyleImage.InActive => new SvgStyleBuilder().AddElement(e => e.WithName(SvgElementEnum.Button).Visible().WithColor(Colors.Red)).AddElement(e => e.WithName(SvgElementEnum.ButtonOutline).Visible().WithColor(Colors.Black)).Build(),
                _                        => ButtonDefault,
            };
        }

        return MainlineDefault;
    }
}




//     public static SvgStyle? GetStyle(string styleName) {
//         switch (styleName) {
//         case "default":
//             return new SvgStyleBuilder(styleName).AddElement(e => e.WithName(SvgElementEnum.Border).WithColor(Colors.Black).Visible()).AddElement(e => e.WithName(SvgElementEnum.Track).WithColor(Colors.ForestGreen).Visible()).AddElement(e => e.WithName(SvgElementEnum.TrackDiverging).WithColor(Colors.ForestGreen).Visible()).AddElement(e => e.WithName(SvgElementEnum.Terminator).WithColor(Colors.Black).Visible()).AddElement(e => e.WithName(SvgElementEnum.Continuation).WithColor(Colors.Black).Visible()).AddElement(e => e.WithName(SvgElementEnum.Occupied).Hidden()).AddElement(e => e.WithName(SvgElementEnum.Dashline).Hidden()).Build();
//
//         case "Mainline-Normal":
//             return new SvgStyleBuilder(styleName).AddElement(e => e.WithName(SvgElementEnum.Border).WithColor(Colors.Black).Visible()).AddElement(e => e.WithName(SvgElementEnum.Track).WithColor(Colors.ForestGreen).Visible()).AddElement(e => e.WithName(SvgElementEnum.TrackDiverging).WithColor(Colors.ForestGreen).Visible()).AddElement(e => e.WithName(SvgElementEnum.Terminator).WithColor(Colors.Black).Visible()).AddElement(e => e.WithName(SvgElementEnum.Continuation).WithColor(Colors.Black).Visible()).AddElement(e => e.WithName(SvgElementEnum.Occupied).Hidden()).AddElement(e => e.WithName(SvgElementEnum.Dashline).Hidden()).Build();
//
//         case "Branchline-Normal":
//             return new SvgStyleBuilder(styleName).AddElement(e => e.WithName(SvgElementEnum.Border).Hidden()).AddElement(e => e.WithName(SvgElementEnum.Track).WithColor(Colors.DarkGray).Visible()).AddElement(e => e.WithName(SvgElementEnum.TrackDiverging).WithColor(Colors.DarkGray).Visible()).AddElement(e => e.WithName(SvgElementEnum.Terminator).WithColor(Colors.DarkGray).Visible()).AddElement(e => e.WithName(SvgElementEnum.Continuation).WithColor(Colors.DarkGray).Visible()).AddElement(e => e.WithName(SvgElementEnum.Occupied).Hidden()).AddElement(e => e.WithName(SvgElementEnum.Dashline).Hidden()).Build();
//
//         case "Mainline-Hidden":
//             return new SvgStyleBuilder(styleName).AddExistingStyle(SvgStyles.GetStyle("Mainline")).AddElement(e => e.WithName(SvgElementEnum.Dashline).Visible().WithColor(Colors.White)).Build();
//
//         case "Branchline-Hidden":
//             return new SvgStyleBuilder(styleName).AddExistingStyle(SvgStyles.GetStyle("Branchline")).AddElement(e => e.WithName(SvgElementEnum.Dashline).Visible().WithColor(Colors.White)).Build();
//
//         case "Mainline-Occupied":
//             return new SvgStyleBuilder(styleName).AddElement(e => e.WithName(SvgElementEnum.Occupied).Visible().WithColor(Colors.Red)).Build();
//
//         case "Branchline-Occupied":
//             return new SvgStyleBuilder(styleName).AddElement(e => e.WithName(SvgElementEnum.Occupied).Visible().WithColor(Colors.Red)).Build();
//
//         case "track-free":
//             return new SvgStyleBuilder(styleName).AddElement(e => e.WithName(SvgElementEnum.Occupied).Hidden()).Build();
//
//         case "Button-Unknown":
//             return new SvgStyleBuilder(styleName).AddElement(e => e.WithName(SvgElementEnum.Button).Visible().WithColor(Colors.Yellow)).AddElement(e => e.WithName(SvgElementEnum.ButtonOutline).Visible().WithColor(Colors.Black)).Build();
//
//         case "Button-Active":
//             return new SvgStyleBuilder(styleName).AddElement(e => e.WithName(SvgElementEnum.Button).Visible().WithColor(Colors.LawnGreen)).AddElement(e => e.WithName(SvgElementEnum.ButtonOutline).Visible().WithColor(Colors.Black)).Build();
//
//         case "Button-Inactive":
//             return new SvgStyleBuilder(styleName).AddElement(e => e.WithName(SvgElementEnum.Button).Visible().WithColor(Colors.Red)).AddElement(e => e.WithName(SvgElementEnum.ButtonOutline).Visible().WithColor(Colors.Red)).Build();
//
//         case "Mainline-Straight":
//             return new SvgStyleBuilder(styleName).AddExistingStyle("Mainline-Normal").AddElement(e => e.WithName(SvgElementEnum.Track).WithColor(Colors.ForestGreen).Visible()).AddElement(e => e.WithName(SvgElementEnum.TrackDiverging).WithColor(Colors.DarkGray).Visible()).Build();
//
//         case "Mainline-Diverging":
//             return new SvgStyleBuilder(styleName).AddExistingStyle("Mainline-Normal").AddElement(e => e.WithName(SvgElementEnum.Track).WithColor(Colors.ForestGreen).Visible()).AddElement(e => e.WithName(SvgElementEnum.TrackDiverging).WithColor(Colors.DarkGray).Visible()).Build();
//
//         case "Mainline-Unknown":
//             return new SvgStyleBuilder(styleName).AddExistingStyle("Mainline-Normal").AddElement(e => e.WithName(SvgElementEnum.Track).WithColor(Colors.ForestGreen).Visible()).AddElement(e => e.WithName(SvgElementEnum.TrackDiverging).WithColor(Colors.DarkGray).Visible()).Build();
//
//         case "Branchline-Straight":
//             return new SvgStyleBuilder(styleName).AddExistingStyle("Branchline-Normal").AddElement(e => e.WithName(SvgElementEnum.Track).WithColor(Colors.ForestGreen).Visible()).AddElement(e => e.WithName(SvgElementEnum.TrackDiverging).WithColor(Colors.DarkGray).Visible()).Build();
//
//         case "Branchline-Diverging":
//             return new SvgStyleBuilder(styleName).AddExistingStyle("Branchline-Normal").AddElement(e => e.WithName(SvgElementEnum.Track).WithColor(Colors.ForestGreen).Visible()).AddElement(e => e.WithName(SvgElementEnum.TrackDiverging).WithColor(Colors.DarkGray).Visible()).Build();
//
//         case "Branchline-Unknown":
//             return new SvgStyleBuilder(styleName).AddExistingStyle("Branchline-Normal").AddElement(e => e.WithName(SvgElementEnum.Track).WithColor(Colors.ForestGreen).Visible()).AddElement(e => e.WithName(SvgElementEnum.TrackDiverging).WithColor(Colors.DarkGray).Visible()).Build();
//         }
//
//         return null;
//     }
