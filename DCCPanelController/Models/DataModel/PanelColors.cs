using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.Attributes;
using DCCPanelController.Models.DataModel.Helpers;
using AppleCrayonColors = DCCPanelController.Helpers.AppleCrayonColors;
using AppleCrayonColorsEnum = DCCPanelController.Helpers.AppleCrayonColors.AppleCrayonColorsEnum;

namespace DCCPanelController.Models.DataModel;

/// <summary>
///     Represents a Panel or Schematic that we can display on the app to control
/// </summary>
public partial class Panel {
    [ObservableProperty] [Copyable("Colors", DisplayName = "BranchLine Track", Category = "Track", CategorySortOrder = 5, ItemSortOrder = 3)]
    private Color _branchLineColor = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Button Border (Default)", Category = "Action Buttons", CategorySortOrder = 20, ItemSortOrder = 2)]
    private Color _buttonBorder = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Button Color (Default)", Category = "Action Buttons", CategorySortOrder = 20, ItemSortOrder = 1)]
    private Color _buttonColor = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Button Border (Off)", Category = "Action Buttons", CategorySortOrder = 20, ItemSortOrder = 4)]
    private Color _buttonOffBorder = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Button Color (Off)", Category = "Action Buttons", CategorySortOrder = 20, ItemSortOrder = 3)]
    private Color _buttonOffColor = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Button Border (On)", Category = "Action Buttons", CategorySortOrder = 20, ItemSortOrder = 5)]
    private Color _buttonOnBorder = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Button Color (On)", Category = "Action Buttons", CategorySortOrder = 20, ItemSortOrder = 6)]
    private Color _buttonOnColor = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Display Background", Category = "Panel", CategorySortOrder = 1, ItemSortOrder = 1)]
    private Color _displayBackgroundColor = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Diverging Track", Category = "Track", CategorySortOrder = 5, ItemSortOrder = 5)]
    private Color _divergingColor = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Hidden Track", Category = "Track", CategorySortOrder = 5, ItemSortOrder = 6)]
    private Color _hiddenColor = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Mainline Border", Category = "Track", CategorySortOrder = 5, ItemSortOrder = 2)]
    private Color _mainlineBorderColor = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Mainline Track", Category = "Track", CategorySortOrder = 5, ItemSortOrder = 1)]
    private Color _mainLineColor = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Occupied Color", Category = "Paths", CategorySortOrder = 10, ItemSortOrder = 1)]
    private Color _occupiedColor = null!;

    [ObservableProperty] [Copyable("Settings")]
    private double _opacityAttribute = 0.35;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Panel Background", Category = "Panel", CategorySortOrder = 1, ItemSortOrder = 1)]
    private Color _panelBackgroundColor = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Show Path Color", Category = "Paths", CategorySortOrder = 10, ItemSortOrder = 2)]
    private Color _showPathColor = null!;

    [ObservableProperty] [Copyable("Colors", DisplayName = "Decorators", Category = "Track", CategorySortOrder = 5, ItemSortOrder = 7)]
    private Color _terminatorColor = null!;

    public void CopyColorsTo(Panel target) => AttributeMapper.CopyTo(this, target, "Colors");

    public void CopyAllTo(Panel target) => AttributeMapper.CopyTo(this, target);

    public void CopySettingsTo(Panel target) => AttributeMapper.CopyTo(this, target, "Settings");

    public ObservableCollection<ColorItemGroup> InitializeGroupedColorSettings() => AttributeMapper.GenerateGroupedColorItems(this, "Colors");

    public ObservableCollection<PanelColorItem> InitializeColorSettings() => AttributeMapper.GenerateColorItems(this, "Colors");

    public void ResetColorsToDefaults() {
        DisplayBackgroundColor = AppleCrayonColors.EnumToColor(AppleCrayonColorsEnum.Snow);
        PanelBackgroundColor = AppleCrayonColors.EnumToColor(AppleCrayonColorsEnum.Snow);

        MainLineColor = AppleCrayonColors.EnumToColor(AppleCrayonColorsEnum.Aqua);
        MainlineBorderColor = AppleCrayonColors.EnumToColor(AppleCrayonColorsEnum.Lead);
        BranchLineColor = AppleCrayonColors.EnumToColor(AppleCrayonColorsEnum.Licorice);
        TerminatorColor = AppleCrayonColors.EnumToColor(AppleCrayonColorsEnum.Iron);
        HiddenColor = AppleCrayonColors.EnumToColor(AppleCrayonColorsEnum.Snow);
        DivergingColor = AppleCrayonColors.EnumToColor(AppleCrayonColorsEnum.Silver);

        ButtonColor = AppleCrayonColors.EnumToColor(AppleCrayonColorsEnum.Aluminium);
        ButtonBorder = AppleCrayonColors.EnumToColor(AppleCrayonColorsEnum.Steel);
        ButtonOnColor = AppleCrayonColors.EnumToColor(AppleCrayonColorsEnum.Fern);
        ButtonOnBorder = AppleCrayonColors.EnumToColor(AppleCrayonColorsEnum.Steel);
        ButtonOffColor = AppleCrayonColors.EnumToColor(AppleCrayonColorsEnum.Maraschino);
        ButtonOffBorder = AppleCrayonColors.EnumToColor(AppleCrayonColorsEnum.Steel);

        OccupiedColor = AppleCrayonColors.EnumToColor(AppleCrayonColorsEnum.Maraschino);
        ShowPathColor = AppleCrayonColors.EnumToColor(AppleCrayonColorsEnum.Tangerine);

        OpacityAttribute = 0.35;
    }
}