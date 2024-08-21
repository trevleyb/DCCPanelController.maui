using System.Text.Json;

namespace DCCPanelController.Components.TrackImages;

public static class TrackStyles {
    
    public static Dictionary<string, TrackStyle> Styles = [];

    // Export of Import our collection of Styles
    // ----------------------------------------------------------------------------------------
    public static string Export() => JsonSerializer.Serialize(Styles, new JsonSerializerOptions { WriteIndented = false });
    public static void Import(string jsonString) {
        if (!string.IsNullOrWhiteSpace(jsonString)) {
            var styles = JsonSerializer.Deserialize<Dictionary<string, TrackStyle>>(jsonString) ?? BuildDefaultStyles();
            Styles = styles;
        }
    }

    // Apply Styles to a Track Piece
    // --------------------------------------------------------------------------------------
    public static void ApplyStyle(string styleName, TrackImage trackImage) {
        if (Styles.Count == 0) Styles = BuildDefaultStyles();
        if (Styles.ContainsKey(styleName)) {
            Styles[styleName].ApplyStyle(trackImage);
        }
    }

    // Build up a list of default styles for when we have no style set
    // ---------------------------------------------------------------------------------------
    public static Dictionary<string, TrackStyle> BuildDefaultStyles() {
        var styles = new Dictionary<string, TrackStyle>();
        var mainlineBuilder = new TrackStylesBuilder("Mainline");
        mainlineBuilder
            .AddElement("Border").Color(Colors.Black).Visible().Done()
            .AddElement("Mainline").Color(Colors.Green).Visible().Done()
            .AddElement("Branchline").Hidden().Dashed(false).Done()
            .AddElement("Terminator").Color(Colors.Black).Visible().Done()
            .AddElement("Continuation").Color(Colors.Black).Visible().Done();
        styles.Add(mainlineBuilder.Name, mainlineBuilder.Build());

        var branchlineBuilder = new TrackStylesBuilder("Branchline");
        branchlineBuilder
            .AddElement("Border").Hidden().Done()
            .AddElement("Mainline").Color(Colors.Black).Visible().Done()
            .AddElement("Branchline").Hidden().Dashed(false).Done()
            .AddElement("Terminator").Color(Colors.Black).Visible().Done()
            .AddElement("Continuation").Color(Colors.Black).Visible().Done();
        styles.Add(branchlineBuilder.Name, branchlineBuilder.Build());
        
        var mainlineDashedBuilder = new TrackStylesBuilder("MainlineDashed");
        mainlineDashedBuilder
            .AddElement("Border").Color(Colors.Black).Visible().Done()
            .AddElement("Mainline").Color(Colors.Green).Visible().Done()
            .AddElement("Branchline").Color(Colors.White).Visible().Dashed().Done()
            .AddElement("Terminator").Color(Colors.Black).Visible().Done()
            .AddElement("Continuation").Color(Colors.Black).Visible().Done();
        styles.Add(mainlineDashedBuilder.Name, mainlineDashedBuilder.Build());
        
        var branchlineDashedBuilder = new TrackStylesBuilder("BranchlineDashed");
        branchlineDashedBuilder
            .AddElement("Border").Hidden().Done()
            .AddElement("Mainline").Color(Colors.Black).Visible().Done()
            .AddElement("Branchline").Color(Colors.White).Visible().Dashed().Done()
            .AddElement("Terminator").Color(Colors.Black).Visible().Done()
            .AddElement("Continuation").Color(Colors.Black).Visible().Done();
        styles.Add(branchlineDashedBuilder.Name, branchlineDashedBuilder.Build());

        return styles;
    } 
}