namespace DCCPanelController.Model.Tracks.Interfaces;

public interface ITrackSymbol {
    string Name { get; }
    int ImageRotation { get; }
    ImageSource SymbolView { get; }
}