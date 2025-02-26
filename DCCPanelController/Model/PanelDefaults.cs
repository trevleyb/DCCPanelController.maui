using CommunityToolkit.Mvvm.ComponentModel;

namespace DCCPanelController.Model;

/// <summary>
///     Represents a Panel or Schematic that we can display on the app to control
/// </summary>
public partial class Panel : ObservableObject {

    [ObservableProperty] private Color _backgroundColor = null!;
    [ObservableProperty] private Color _borderColor = null!;
    [ObservableProperty] private Color _mainLineColor = null!;
    [ObservableProperty] private Color _branchLineColor = null!;
    [ObservableProperty] private Color _divergingColor = null!;

    [ObservableProperty] private Color _buttonBorder = null!;
    [ObservableProperty] private Color _buttonColor = null!;
    [ObservableProperty] private Color _buttonOffBorder = null!;
    [ObservableProperty] private Color _buttonOffColor = null!;
    [ObservableProperty] private Color _buttonOnBorder = null!;
    [ObservableProperty] private Color _buttonOnColor = null!;

    [ObservableProperty] private Color _continuationColor = null!;
    [ObservableProperty] private Color _terminatorColor = null!;
    [ObservableProperty] private Color _hiddenColor = null!;
    [ObservableProperty] private Color _occupiedColor = null!;

    public void ResetToDefaults() {
        BackgroundColor = Colors.White;
        BorderColor = Colors.LightSlateGray;
        MainLineColor = Colors.Black;
        BranchLineColor = Colors.LightGray;
        DivergingColor = Colors.LightGray;

        ButtonBorder = Colors.Grey;
        ButtonColor = Colors.Grey;
        ButtonOffBorder = Colors.Red;
        ButtonOffColor = Colors.Red;
        ButtonOnBorder = Colors.Green;
        ButtonOnColor = Colors.Green;

        ContinuationColor = Colors.DimGray;
        TerminatorColor = Colors.DimGrey;
        HiddenColor = Colors.WhiteSmoke;
        OccupiedColor = Colors.Red;
    }
}