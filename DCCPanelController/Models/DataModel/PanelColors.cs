using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Helpers.Attributes;
using DCCPanelController.Models.DataModel.Helpers;
using Microsoft.Maui.Graphics;

namespace DCCPanelController.Models.DataModel;

/// <summary>
///     Represents a Panel or Schematic that we can display on the app to control
/// </summary>
public partial class Panel : ObservableObject {
    [ObservableProperty] [Copyable("Colors", DisplayName = "Branchline Track", Category = "Track", CategorySortOrder = 5, ItemSortOrder = 3)]
    private Color _branchLineColor = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Bridge", Category = "Construction", CategorySortOrder = 5, ItemSortOrder = 7)]
    private Color _bridgeColor = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Button Border", Category = "Action Buttons", CategorySortOrder = 20, ItemSortOrder = 2)]
    private Color _buttonBorder = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Button Color", Category = "Action Buttons", CategorySortOrder = 20, ItemSortOrder = 1)]
    private Color _buttonColor = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Button Off Border", Category = "Action Buttons", CategorySortOrder = 20, ItemSortOrder = 4)]
    private Color _buttonOffBorder = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Button Off", Category = "Action Buttons", CategorySortOrder = 20, ItemSortOrder = 3)]
    private Color _buttonOffColor = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Button On Border", Category = "Action Buttons", CategorySortOrder = 20, ItemSortOrder = 5)]
    private Color _buttonOnBorder = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Button On", Category = "Action Buttons", CategorySortOrder = 20, ItemSortOrder = 6)]
    private Color _buttonOnColor = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Continuation Marker", Category = "Track", CategorySortOrder = 5, ItemSortOrder = 4)]
    private Color _continuationColor = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Display Background", Category = "Panel", CategorySortOrder = 1, ItemSortOrder = 1)]
    private Color _displayBackgroundColor = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Diverging Track", Category = "Track", CategorySortOrder = 5, ItemSortOrder = 5)]
    private Color _divergingColor = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Hidden Track", Category = "Track", CategorySortOrder = 5, ItemSortOrder = 6)]
    private Color _hiddenColor = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Light Switch Off Border", Category = "Light Switch", CategorySortOrder = 20, ItemSortOrder = 13)]
    private Color _lightOffBorderColor = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Light Switch Off", Category = "Light Switch", CategorySortOrder = 20, ItemSortOrder = 12)]
    private Color _lightOffColor = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Light Switch On Border", Category = "Light Switch", CategorySortOrder = 20, ItemSortOrder = 11)]
    private Color _lightOnBorderColor = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Light Switch On", Category = "Light Switch", CategorySortOrder = 20, ItemSortOrder = 10)]
    private Color _lightOnColor = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Mainline Border", Category = "Track", CategorySortOrder = 5, ItemSortOrder = 2)]
    private Color _mainlineBorderColor = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Mainline Track", Category = "Track", CategorySortOrder = 5, ItemSortOrder = 1)]
    private Color _mainLineColor = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Occupied", Category = "Paths", CategorySortOrder = 10, ItemSortOrder = 1)]
    private Color _occupiedColor = null!;

    [ObservableProperty] [Copyable("Settings")]
    private double _opacityAttribute = 0.35;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Panel Background", Category = "Panel", CategorySortOrder = 1, ItemSortOrder = 1)]
    private Color _panelBackgroundColor = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Platform", Category = "Construction", CategorySortOrder = 5, ItemSortOrder = 7)]
    private Color _platformColor = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Show Path", Category = "Paths", CategorySortOrder = 10, ItemSortOrder = 2)]
    private Color _showPathColor = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Terminator", Category = "Track", CategorySortOrder = 5, ItemSortOrder = 7)]
    private Color _terminatorColor = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Tunnel", Category = "Construction", CategorySortOrder = 5, ItemSortOrder = 7)]
    private Color _tunnelColor = null!;

    public void CopyColorsTo(Panel target) {
        AttributeMapper.CopyTo(this, target, "Colors");
    }

    public void CopyAllTo(Panel target) {
        AttributeMapper.CopyTo(this, target);
    }

    public void CopySettingsTo(Panel target) {
        AttributeMapper.CopyTo(this, target, "Settings");
    }

    public ObservableCollection<ColorItemGroup> InitializeGroupedColorSettings() {
        return AttributeMapper.GenerateGroupedColorItems(this, "Colors");
    }

    public ObservableCollection<PanelColorItem> InitializeColorSettings() {
        return AttributeMapper.GenerateColorItems(this, "Colors");
    }

    public void ResetColorsToDefaults() {
        DisplayBackgroundColor = AppleCrayonColors.Value("Snow");
        PanelBackgroundColor = AppleCrayonColors.Value("Snow");

        MainLineColor = AppleCrayonColors.Value("Aqua");
        MainlineBorderColor = AppleCrayonColors.Value("Lead");
        BranchLineColor = AppleCrayonColors.Value("Licorice");
        ContinuationColor = AppleCrayonColors.Value("Iron");
        TerminatorColor = AppleCrayonColors.Value("Iron");
        HiddenColor = AppleCrayonColors.Value("Snow");
        DivergingColor = AppleCrayonColors.Value("Silver");

        ButtonColor = AppleCrayonColors.Value("Aluminum");
        ButtonBorder = AppleCrayonColors.Value("Ocean");
        ButtonOnColor = AppleCrayonColors.Value("Fern");
        ButtonOnBorder = AppleCrayonColors.Value("Ocean");
        ButtonOffColor = AppleCrayonColors.Value("Maraschino");
        ButtonOffBorder = AppleCrayonColors.Value("Ocean");

        LightOnColor = AppleCrayonColors.Value("Fern");
        LightOnBorderColor = AppleCrayonColors.Value("Ocean");
        LightOffColor = AppleCrayonColors.Value("Mercury");
        LightOffBorderColor = AppleCrayonColors.Value("Tin");

        BridgeColor = AppleCrayonColors.Value("Tin");
        PlatformColor = AppleCrayonColors.Value("Tin");
        TunnelColor = AppleCrayonColors.Value("Tin");

        OccupiedColor = AppleCrayonColors.Value("Maraschino");
        ShowPathColor = AppleCrayonColors.Value("Tangerine");

        OpacityAttribute = 0.35;
    }
}