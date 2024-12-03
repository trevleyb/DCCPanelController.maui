// TODO: REMOVE THIS

// using DCCPanelController.Tracks.ImageManager;
//
// namespace DCCPanelController.Tracks.StyleManager;
//
// public class SvgDefaultStyles {
//     
//     public static SvgStyle? GetStyle(TrackStyleType type, _trackStyleSubType trackSubType) {
//         var styleName = $"{type}-{trackSubType}";
//         return GetStyle(styleName);
//     }
//
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
// }
