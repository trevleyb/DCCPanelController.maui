namespace DCCPanelController.Models.ViewModel.StyleManager;

public static class SvgElementTypes {
    public static string GetElement(SvgElementType elementType) {
        return elementType switch {
            SvgElementType.Track           => "Track",
            SvgElementType.TrackDiverging  => "TrackDiverging",
            SvgElementType.Dashline        => "Dashline",
            SvgElementType.Border          => "Border",
            SvgElementType.BorderDiverging => "BorderDiverging",
            SvgElementType.Terminator      => "Terminator",
            SvgElementType.Continuation    => "Continuation",
            SvgElementType.Button          => "Button",
            SvgElementType.ButtonOutline   => "ButtonOutline",
            SvgElementType.Text            => "Text",
            SvgElementType.Highlight       => "Highlight",
            SvgElementType.Indicator       => "Indicator",
            SvgElementType.Bridge          => "Bridge",
            SvgElementType.Platform        => "Platform",
            SvgElementType.Tunnel          => "Tunnel",
            _                              => ""
        };
    }
}