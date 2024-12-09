namespace DCCPanelController.Model.Tracks.Interfaces;

public interface ITrackSymbol {
    string Name { get; }
    ImageSource? Symbol { get; }
    Panel? Parent { get; }
}