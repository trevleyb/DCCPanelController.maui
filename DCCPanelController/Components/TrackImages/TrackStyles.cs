namespace DCCPanelController.Components.TrackImages;

public class TrackStyles {

    private static Dictionary<string, TrackStyle> _styles = [];

    public static void ApplyStyle(string styleName, TrackImage trackImage) {
        if (_styles.Count == 0) BuildDefaultStyles();
        if (_styles.ContainsKey(styleName)) {
            _styles[styleName].ApplyStyle(trackImage);
        }
    }

    public static void BuildDefaultStyles() {
        
        _styles = new Dictionary<string, TrackStyle>();
        
        var mainlineBuilder = new TrackStyleBuilder("Mainline");
        mainlineBuilder
            .AddElement("Border").Color(Colors.Black.ToHex()).Visible().Done()
            .AddElement("MainlineTrack").Color(Colors.Green.ToHex()).Visible().Done()
            .AddElement("BranchlineTrack").Hidden().Done()
            .AddElement("Terminator").Color(Colors.Black.ToHex()).Visible().Done()
            .AddElement("Continuation").Color(Colors.Black.ToHex()).Visible().Done();
        _styles.Add(mainlineBuilder.Name, mainlineBuilder.Build());

        var branchlineBuilder = new TrackStyleBuilder("Branchline");
        branchlineBuilder
            .AddElement("Border").Hidden().Done()
            .AddElement("MainlineTrack").Hidden().Done()
            .AddElement("BranchlineTrack").Color(Colors.Green.ToHex()).Visible().Done()
            .AddElement("Terminator").Color(Colors.Black.ToHex()).Visible().Done()
            .AddElement("Continuation").Color(Colors.Black.ToHex()).Visible().Done();
        _styles.Add(branchlineBuilder.Name, branchlineBuilder.Build());
        
        var mainlineDashedBuilder = new TrackStyleBuilder("MainlineDashed");
        mainlineDashedBuilder
            .AddElement("Border").Color(Colors.Black.ToHex()).Visible().Done()
            .AddElement("MainlineTrack").Color(Colors.Green.ToHex()).Visible().Done()
            .AddElement("BranchlineTrack").Visible().Dashed().Done()
            .AddElement("Terminator").Color(Colors.Black.ToHex()).Visible().Done()
            .AddElement("Continuation").Color(Colors.Black.ToHex()).Visible().Done();
        _styles.Add(mainlineDashedBuilder.Name, mainlineDashedBuilder.Build());
        
        var branchlineDashedBuilder = new TrackStyleBuilder("BranchlineDashed");
        branchlineBuilder
            .AddElement("Border").Hidden().Done()
            .AddElement("MainlineTrack").Hidden().Done()
            .AddElement("BranchlineTrack").Color(Colors.Green.ToHex()).Visible().Dashed().Done()
            .AddElement("Terminator").Color(Colors.Black.ToHex()).Visible().Done()
            .AddElement("Continuation").Color(Colors.Black.ToHex()).Visible().Done();
        _styles.Add(branchlineBuilder.Name, branchlineBuilder.Build());

    } 
}