namespace DCCPanelController.Components.Tracks.SVGManager;

public interface ISvgModifier {

    void SetElementOccupied(Color? color, int? opacity = null);
    void SetElementFree(Color? color, int? opacity = null);
    void SetElementRoute(Color? color, int? opacity = null);

    void SetButtonColor(Color? color, int? opacity = null);
    void SetTrackColor(Color? color, int? opacity = null);
    void SetBorderColor(Color? color, int? opacity = null);
    void SetDivergingColor(Color? color, int? opacity = null);
    void SetTerminatorColor(Color? color, int? opacity = null);
    void SetContinuationColor(Color? color, int? opacity = null);

}