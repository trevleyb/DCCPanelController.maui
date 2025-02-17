using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model;

/// <summary>
///     Represents a Panel or Schematic that we can display on the app to control
/// </summary>
public partial class PanelDefaults : ObservableObject {

    [ObservableProperty] private Color _backgroundColor = Colors.White;
    [ObservableProperty] private Color _borderColor = Colors.LightSlateGray;
    [ObservableProperty] private Color _branchLineColor = Colors.LightGray;
    [ObservableProperty] private Color _buttonBorder = Colors.Grey;
    [ObservableProperty] private Color _buttonColor = Colors.Grey;
    [ObservableProperty] private Color _buttonOffBorder = Colors.Red;
    [ObservableProperty] private Color _buttonOffColor = Colors.Red;
    [ObservableProperty] private Color _buttonOnBorder = Colors.Green;

    [ObservableProperty] private Color _buttonOnColor = Colors.Green;
    [ObservableProperty] private Color _continuationColor = Colors.Black;
    [ObservableProperty] private Color _divergingColor = Colors.Grey;
    [ObservableProperty] private Color _hiddenColor = Colors.White;
    [ObservableProperty] private Color _mainLineColor = Colors.Black;
    [ObservableProperty] private Color _occupiedColor = Colors.Red;
    [ObservableProperty] private Color _terminatorColor = Colors.Black;

    public PanelDefaults() {
        ResetToDefaults();
    }

    public void ResetToDefaults() {
        BackgroundColor = Colors.White;
        MainLineColor = Colors.Green;
        BranchLineColor = Colors.LightGray;
        BorderColor = Colors.Black;
        DivergingColor = Colors.Grey;
        OccupiedColor = Colors.Crimson;
        HiddenColor = Colors.White;
        TerminatorColor = Colors.Black;
        ContinuationColor = Colors.Black;

        ButtonOnColor = Colors.LightGreen;
        ButtonOnBorder = Colors.DarkGreen;
        ButtonOffColor = Colors.LightCoral;
        ButtonOffBorder = Colors.Crimson;
        ButtonColor = Colors.LightGray;
        ButtonBorder = Colors.DarkGrey;
    }

    public PanelDefaults Clone() {
        return (PanelDefaults)MemberwiseClone();
    }
}