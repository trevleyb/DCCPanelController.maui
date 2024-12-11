namespace DCCPanelController.Model.Tracks.Interfaces;

public interface ITrackSymbol {
    string Name { get; }
    ImageSource? DisplaySymbol { get; }
    Panel? Parent { get; }
}