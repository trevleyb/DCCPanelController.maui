using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;

namespace DCCPanelController.Model;

/// <summary>
///     Represents a Panel or Schematic that we can display on the app to control
/// </summary>
public partial class Panel : ObservableObject {
    [ObservableProperty] private Color _backgroundColor = null!;
    [ObservableProperty] private Color _borderColor = null!;
    [ObservableProperty] private Color _branchLineColor = null!;

    [ObservableProperty] private Color _buttonBorder = null!;
    [ObservableProperty] private Color _buttonColor = null!;
    [ObservableProperty] private Color _buttonOffBorder = null!;
    [ObservableProperty] private Color _buttonOffColor = null!;
    [ObservableProperty] private Color _buttonOnBorder = null!;
    [ObservableProperty] private Color _buttonOnColor = null!;

    [ObservableProperty] private Color _continuationColor = null!;
    [ObservableProperty] private Color _divergingColor = null!;
    [ObservableProperty] private Color _hiddenColor = null!;
    [ObservableProperty] private Color _mainLineColor = null!;
    [ObservableProperty] private Color _occupiedColor = null!;
    [ObservableProperty] private Color _showPathColor = null!;
    [ObservableProperty] private Color _terminatorColor = null!;

    public void CopyTo(Panel target) {
        target.BackgroundColor = BackgroundColor;
        target.BorderColor = BorderColor;
        target.MainLineColor = MainLineColor;
        target.BranchLineColor = BranchLineColor;
        target.DivergingColor = DivergingColor;

        target.ButtonBorder = ButtonBorder;
        target.ButtonColor = ButtonColor;
        target.ButtonOffBorder = ButtonOffBorder;
        target.ButtonOffColor = ButtonOffColor;
        target.ButtonOnBorder = ButtonOnBorder;
        target.ButtonOnColor = ButtonOnColor;

        target.ContinuationColor = ContinuationColor;
        target.TerminatorColor = TerminatorColor;
        target.HiddenColor = HiddenColor;
        target.OccupiedColor = OccupiedColor;
        target.ShowPathColor = ShowPathColor;
    }

    public void ResetToDefaults() {
        BackgroundColor = AppleCrayonColors.Value("Snow");
        BorderColor = AppleCrayonColors.Value("Midnight");
        MainLineColor = AppleCrayonColors.Value("Ocean");
        BranchLineColor = AppleCrayonColors.Value("Aqua");
        DivergingColor = AppleCrayonColors.Value("Sky");

        ButtonBorder = AppleCrayonColors.Value("Steel");
        ButtonColor = AppleCrayonColors.Value("Aluminum");
        ButtonOffBorder = AppleCrayonColors.Value("Cayenne");
        ButtonOffColor = AppleCrayonColors.Value("Maraschino");
        ButtonOnBorder = AppleCrayonColors.Value("Clover");
        ButtonOnColor = AppleCrayonColors.Value("Fern");

        ContinuationColor = AppleCrayonColors.Value("Iron");
        TerminatorColor = AppleCrayonColors.Value("Iron");
        ;
        HiddenColor = AppleCrayonColors.Value("Snow");
        OccupiedColor = AppleCrayonColors.Value("Cayenne");
        ShowPathColor = AppleCrayonColors.Value("Lemon");
    }
}