using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Helpers;
using DCCPanelController.Model;
using DCCPanelController.Model.Tracks;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Services;
using DCCPanelController.Services.NavigationService;
using DCCPanelController.Tracks;
using DCCPanelController.Tracks.StyleManager;

namespace DCCPanelController.ViewModel;

public partial class PanelEditorViewModel : BaseViewModel {
    [ObservableProperty] private Panel _panel;
    [ObservableProperty] private bool _hasSelectedTracks;
    [ObservableProperty] private bool _canUsePropertyPage;
    
    private readonly NavigationService _navigationService = MauiProgram.ServiceHelper.GetService<NavigationService>();
    public EditState EditState = EditState.None;
    public ObservableCollection<ITrackSymbol> TrackSymbols { get; init; } = [];

    public event Action<Panel?>? OnSaveCompleted;
    public event EventHandler? CloseRequested;

    public PanelEditorViewModel(Panel panel) {
        Panel = panel;
        Panel.PropertyChanged += OnPanelPropertyChanged;
        PropertyChanged += OnPropertyChanged;
        TrackSymbols = BuildTrackSymbols(Panel);
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        Console.WriteLine($"PanelEditorView: PropertyChanged: {sender} - {e.PropertyName}");
    }

    private void OnPanelPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        Console.WriteLine($"PanelEditorView: PanelPropertyChanged: {sender} - {e.PropertyName}");
    }

    [RelayCommand]
    private async Task SaveAsync() {
        Save();
    }

    public void Save() {
        OnSaveCompleted?.Invoke(Panel);
        CloseRequested?.Invoke(Panel, EventArgs.Empty);
    }

    [RelayCommand]
    private async Task CancelAsync() {
        Cancel();
    }

    public void Cancel() {
        //if (EditState == EditState.Changed) {
        //  Console.WriteLine("Panel was changed.");
        // var answer = _navigationService.DisplayAlertAsync("Save Changed?", "You have unsaved Changes. Do you want to save?", "Yes", "No").GetAwaiter().GetResult();
        // if (answer) { Save(); return; }
        //}
        OnSaveCompleted?.Invoke(null);
        CloseRequested?.Invoke(null, EventArgs.Empty);
    }

    public bool TracksOutsideBounds => Panel.Tracks.Any(track => track.X < 0 || track.X >= Panel.Cols || track.Y < 0 || track.Y >= Panel.Rows);
    public void Validate() {
        // Make sure that all the Coordinates for the Track Pieces are valid and 
        // if not, make sure they are within the bounds of the Panel. 
        if (!Panel.Tracks.Any()) return;
        for (var idx = Panel.Tracks.Count - 1; idx >= 0; idx--) {
            var track = Panel.Tracks[idx];
            if (track.X < 0 || track.X >= Panel.Cols || track.Y < 0 || track.Y >= Panel.Rows) {
                Panel.Tracks.Remove(track);
            }
        }
    }

    private static ObservableCollection<ITrackSymbol> BuildTrackSymbols(Panel parent) {
        return [
            new TrackButton(parent),
            new TrackLabelCircle(parent),
            new TrackText(parent),
            new TrackImage(parent),
            new TrackStraight(parent),
            new TrackStraight(parent) {TrackRotation = 90},
            new TrackStraightContinuation(parent),
            new TrackCorner(parent),
            new TrackCorner(parent) {TrackRotation = 180},
            new TrackCornerContinuation(parent),
            new TrackLeftTurnout(parent),
            new TrackLeftTurnout(parent) {TrackRotation = 180},
            new TrackRightTurnout(parent),
            new TrackRightTurnout(parent) {TrackRotation = 180},
            new TrackCrossing(parent),
            new TrackTerminator(parent),
            
            new TrackStraight(parent, TrackStyleType.Branchline),
            new TrackStraight(parent, TrackStyleType.Branchline) {TrackRotation = 90},
            new TrackStraightContinuation(parent, TrackStyleType.Branchline),
            new TrackStraightContinuation(parent, TrackStyleType.Branchline) {TrackRotation = 90},
            new TrackCorner(parent, TrackStyleType.Branchline),
            new TrackCornerContinuation(parent, TrackStyleType.Branchline),
            new TrackLeftTurnout(parent, TrackStyleType.Branchline),
            new TrackRightTurnout(parent, TrackStyleType.Branchline),
            new TrackCrossing(parent, TrackStyleType.Branchline),
            new TrackTerminator(parent, TrackStyleType.Branchline),
            
        ];
    }
}

public enum EditState {
    None,
    Saved,
    Changed
}
