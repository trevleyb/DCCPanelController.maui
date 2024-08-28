using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Model;
using DCCPanelController.Tracks;

namespace DCCPanelController.ViewModel;

public partial class ControlPanelPageViewModel : BaseViewModel {

    [ObservableProperty] private bool _showGrid;
    public ObservableCollection<ControlPanelViewModel> Panels { get; set; }
    public ControlPanelViewModel? SelectedPanel { get; set; }

    public ControlPanelPageViewModel() {
        Panels = new ObservableCollection<ControlPanelViewModel>();
        Panels.Add(new ControlPanelViewModel(CreateTestPanel("Page 1")));
        Panels.Add(new ControlPanelViewModel(CreateTestPanel("Page 2")));
        Panels.Add(new ControlPanelViewModel(CreateTestPanel("Page 3")));
        this.PropertyChanged += OnPropertyChanged;
    }
    
    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        switch (e.PropertyName) {
        case nameof(ShowGrid):
            if (SelectedPanel is not null) SelectedPanel.ShowGrid = ShowGrid;
            break;
        }
    }

    private Panel CreateTestPanel(string name) {
        var panel = new Panel();
        panel.Id = "01";
        panel.Name = name;
        panel.Cols = 18;
        panel.Rows = 12;
        panel.Tracks = [];
        panel.Tracks.Add(new TrackStraight() { X = 1, Y = 1, ImageRotation = 0 });
        panel.Tracks.Add(new TrackStraight() { X = 2, Y = 2, ImageRotation = 90 });
        
        panel.Tracks.Add(new TrackCorner()   { X = 2, Y = 3, ImageRotation = 180 });
        panel.Tracks.Add(new TrackStraight() { X = 3, Y = 3, ImageRotation = 180 });
        panel.Tracks.Add(new TrackStraight() { X = 4, Y = 3, ImageRotation = 180 });
        panel.Tracks.Add(new TrackStraight() { X = 5, Y = 3, ImageRotation = 180 });
        panel.Tracks.Add(new TrackStraight() { X = 6, Y = 3, ImageRotation = 180 });
        panel.Tracks.Add(new TrackStraight() { X = 7, Y = 3, ImageRotation = 180 });
        panel.Tracks.Add(new TrackStraight() { X = 8, Y = 3, ImageRotation = 180 });
        panel.Tracks.Add(new TrackCorner()   { X = 9, Y = 3, ImageRotation = 0 });
        
        panel.Tracks.Add(new TrackStraight() { X = 4, Y = 4, ImageRotation = 270 });
        panel.Tracks.Add(new TrackStraight() { X = 5, Y = 5, ImageRotation = 0 });
        
        panel.Tracks.Add(new TrackButton()      { X = 10, Y = 8, ImageRotation = 0 });
        panel.Tracks.Add(new TrackCrossing()    { X = 11, Y = 9, ImageRotation = 0 });
        panel.Tracks.Add(new TrackTerminator()  { X = 12, Y = 10, ImageRotation = 0 });
        panel.Tracks.Add(new TrackThreeway()    { X = 13, Y = 11, ImageRotation = 0 });
        panel.Tracks.Add(new TrackLeftTurnout() { X = 14, Y = 12, ImageRotation = 0 });
        panel.Tracks.Add(new TrackRightTurnout(){ X = 15, Y = 8, ImageRotation = 0 });





        
        return panel;
    }
    
    
}