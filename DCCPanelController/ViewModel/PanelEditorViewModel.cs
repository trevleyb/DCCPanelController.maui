using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model;
using DCCPanelController.Tracks;
using DCCPanelController.Tracks.Interfaces;

namespace DCCPanelController.ViewModel;

public partial class PanelEditorViewModel : BaseViewModel {

    [ObservableProperty] private ObservableCollection<ITrackSymbol> _trackSymbols = [];
    [ObservableProperty] private Panel _panel;

    public PanelEditorViewModel(Panel panel) {
        _panel = panel;
        foreach (var symbol in TrackPieceFactory.TrackPieces) {
            if (symbol is ITrackSymbol trackSymbol) TrackSymbols.Add(trackSymbol);
        }
    }
}