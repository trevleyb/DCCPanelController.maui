using System.Collections.ObjectModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Helpers.Attributes;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.View.Base;

namespace DCCPanelController.View.Properties.PanelProperties;

public partial class PanelPropertyViewModel : BaseViewModel {
    [ObservableProperty] private int    _colorGridSpan = 2; // Default to 2 columns
    [ObservableProperty] private Panel  _panel;
    [ObservableProperty] private Panel  _original;
    [ObservableProperty] private string _title;

    [ObservableProperty] private int _minCols;
    [ObservableProperty] private int _minRows;

    public ObservableCollection<ColorItemGroup> GroupedColorSettings { get; set; }
    public ObservableCollection<PanelColorItem> ColorSettings { get; }
    
    public PanelPropertyViewModel(Panel panel, Panel original) {
        Original = original;
        Panel = panel;
        Title = $"{panel.Id} Properties" ?? "Panel Properties";

        ColorSettings = Panel.InitializeColorSettings();
        GroupedColorSettings = Panel.InitializeGroupedColorSettings();
        var (maxCol, maxRow) = GetMaxColAndRow();
        MinCols = maxCol;
        MinRows = maxRow;
    }

    public Task ApplyChangesAsync() => Task.CompletedTask;

    public ContentView CreatePropertiesView() {
        var propPage = new PanelPropertyPage(this);
        return propPage;
    }

    [RelayCommand]
    private async Task ResetDefaultsClickedAsync() {
        var result = await AskUserToConfirm("Reset Default Colors?", "Are you sure you want to reset all Panels colors to Defaults?");
        if (!result) return; // Exit if the user cancels the delete operation
        Panel.ResetColorsToDefaults();
        OnPropertyChanged(nameof(Panel));
    }

    [RelayCommand]
    private async Task ResetOverridesClickedAsync() {
        var result = await AskUserToConfirm("Reset Overriden Colors?", "This will reset all tracks to the panel colors where they have overriden colors. Do you want to do this?");
        if (!result) return; // Exit if the user cancels the delete operation

        foreach (var track in Panel.Entities) {
            var trackType = track.GetType();
            var property = trackType.GetProperty("TrackColor", BindingFlags.Public | BindingFlags.Instance);
            if (property != null && property.PropertyType == typeof(Color) && property.CanWrite) {
                property.SetValue(track, null);
            }
        }
        OnPropertyChanged(nameof(Panel));
    }

    [RelayCommand]
    private async Task ToAllPanelsClickedAsync() {
        var result = await AskUserToConfirm("Standardize Defaults?", "This will copy these panel colors to all other panels. Do you want to do this?");
        if (!result) return; // Exit if the user cancels the delete operation

        // Worst-mistake! Making a copy means we can't easily reference all panels
        // as the copy is not linked into the main panel system until it 
        // is saved. So we need to know about the original also. 
        // ------------------------------------------------------------------------
        if (Panel.Panels is { } panels) {
            foreach (var panel in panels) Panel.CopyColorsTo(panel);
        }
        if (Original.Panels is { } original) {
            foreach (var panel in original) Panel.CopyColorsTo(panel);
        }
        OnPropertyChanged(nameof(Panel));
    }

    private async Task<bool> AskUserToConfirm(string title, string message) {
        if (App.Current.Windows[0].Page is { } window) {
            var result = await window.DisplayAlertAsync(title, message, "Yes", "No");
            return result;
        }
        return false;
    }
    
    // Find the position of the maximum cols and rows
    private (int maxCol, int maxRow) GetMaxColAndRow() {
        int maxCol = 0;
        int maxRow = 0;
        foreach (var entity in Panel.Entities) {
            if (entity.Col + (entity.Width -1) > maxCol) maxCol = entity.Col;
            if (entity.Row + (entity.Height -1) > maxRow) maxRow = entity.Row;
        }
        return (maxCol, maxRow);
    }

}