namespace DCCPanelController.Model.Tracks.Interfaces;

public interface ITrackSymbol {
    string Name { get; }
    bool ShowAboveSymbol { get; }
    bool ShowBelowSymbol { get; }
    int ImageRotation { get; }
    ImageSource? DisplaySymbol { get; }
    Panel? Parent { get; }
}