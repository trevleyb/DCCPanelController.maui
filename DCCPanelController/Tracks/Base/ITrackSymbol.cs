namespace DCCPanelController.Tracks.Interfaces;

public interface ITrackSymbol {
    string Name { get; }
    ImageSource? SymbolImage { get; }
}