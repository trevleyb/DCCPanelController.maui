using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model;
using DCCPanelController.Model.Tracks;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Services;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.ViewModel;

public partial class PanelEditorViewModel : BaseViewModel {

    private readonly NavigationService _navigationService = MauiProgram.ServiceHelper.GetService<NavigationService>();
    [ObservableProperty] private Panel _panel;

    public PanelEditorViewModel(Panel panel, Action<bool> onCompleted) {
        Panel = panel;
        TrackSymbols = BuildTrackSymbols(Panel);
        OnPropertyChanged(nameof(HasSelectedTracks));
        OnPropertyChanged(nameof(CanUsePropertyPage));

        SaveCommand = new Command(() => {
            onCompleted(true); // Indicates successful save
        });

        CancelCommand = new Command(() => {
            onCompleted(false); // Indicates cancel
        });
    }

    public ObservableCollection<ITrackSymbol> TrackSymbols { get; init; } = [];

    public bool HasSelectedTracks => Panel.SelectedTracks.Count > 0;
    public bool CanUsePropertyPage => Panel.SelectedTracks.Count <= 1;

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    
    public void TrackPieceChanged() {
        OnPropertyChanged(nameof(HasSelectedTracks));
        OnPropertyChanged(nameof(CanUsePropertyPage));
    }

    private static ObservableCollection<ITrackSymbol> BuildTrackSymbols(Panel parent) {
        return [
            new TrackButton(parent),
            new TrackLabelCircle(parent),
            new TrackText(parent),
            new TrackImage(parent),
            new TrackStraight(parent),
            new TrackStraight(parent) { TrackRotation = 90 },
            new TrackStraightContinuation(parent),
            new TrackCorner(parent),
            new TrackCorner(parent) { TrackRotation = 180 },
            new TrackCornerContinuation(parent),
            new TrackLeftTurnout(parent),
            new TrackLeftTurnout(parent) { TrackRotation = 180 },
            new TrackRightTurnout(parent),
            new TrackRightTurnout(parent) { TrackRotation = 180 },
            new TrackCrossing(parent),
            new TrackTerminator(parent),
            // TODO: Add a Circle Track Type
            // TODO: Add a Rectangle Track Type
            // TODO: Add a Line Track Type
            new TrackStraight(parent, TrackStyleTypeEnum.Branchline),
            new TrackStraight(parent, TrackStyleTypeEnum.Branchline) { TrackRotation = 90 },
            new TrackStraightContinuation(parent, TrackStyleTypeEnum.Branchline),
            new TrackStraightContinuation(parent, TrackStyleTypeEnum.Branchline) { TrackRotation = 90 },
            new TrackCorner(parent, TrackStyleTypeEnum.Branchline),
            new TrackCornerContinuation(parent, TrackStyleTypeEnum.Branchline),
            new TrackLeftTurnout(parent, TrackStyleTypeEnum.Branchline),
            new TrackRightTurnout(parent, TrackStyleTypeEnum.Branchline),
            new TrackCrossing(parent, TrackStyleTypeEnum.Branchline),
            new TrackTerminator(parent, TrackStyleTypeEnum.Branchline)
        ];
    }
}