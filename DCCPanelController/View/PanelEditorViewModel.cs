using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Services;
using DCCPanelController.View.DynamicProperties;
using DCCPanelController.View.Helpers;
using DCCPanelController.View.PanelProperties;
using DCCPanelController.View.Properties;

namespace DCCPanelController.View;

public partial class PanelEditorViewModel : ObservableObject {
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    private Panel? _panel;

    [ObservableProperty] private bool _gridVisible;
    [ObservableProperty] private EditModeEnum _editMode = EditModeEnum.Move;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedEntities))]
    [NotifyPropertyChangedFor(nameof(HasSelectedEntities))]
    [NotifyPropertyChangedFor(nameof(MultipleEntitiesSelected))]
    [NotifyPropertyChangedFor(nameof(SingleEntitySelected))]
    [NotifyPropertyChangedFor(nameof(SelectedEntity))]
    private ObservableCollection<ITile> _selectedTiles = [];
    
    public List<Entity> SelectedEntities => SelectedTiles.Select(x => x.Entity).ToList();
    public int SelectedEntitiesCount => SelectedEntities.Count;
    public bool HasSelectedEntities => SelectedEntitiesCount > 0;
    public bool MultipleEntitiesSelected => SelectedEntitiesCount > 1;
    public bool SingleEntitySelected => SelectedEntitiesCount == 1;
    public Entity? SelectedEntity => SelectedEntities.FirstOrDefault();

    private readonly INavigation _navigation;
    public string Title => Panel?.Title ?? "Panel";
    
    public double ScreenWidth   = 100;
    public double ScreenHeight  = 100;
    
    public PanelEditorViewModel(Panel panel, INavigation navigation) {
        _panel = panel;
        _navigation = navigation;
    }
    
    [RelayCommand]
    private async Task RotateTileAsync() {
        if (HasSelectedEntities && Panel is not null) {
            foreach (var entity in SelectedEntities) {
                entity.RotateRight();
            }
        }
    }

    [RelayCommand]
    private async Task SwitchModeAsync() {
        EditMode = EditMode switch {
            EditModeEnum.Move => EditModeEnum.Copy,
            EditModeEnum.Copy => EditModeEnum.Size,
            EditModeEnum.Size => EditModeEnum.Move,
            _                 => EditModeEnum.Move
        };
    }

    [RelayCommand]
    private async Task DeleteTileAsync() {
        if (HasSelectedEntities && Panel is not null) {
            foreach (var entity in SelectedEntities) {
                Panel.Entities.Remove(entity);
            }
            SelectedEntities.Clear();
            OnPropertyChanged(nameof(Panels));
        }
    }

    [RelayCommand]
    private async Task ToggleGridAsync() {
        GridVisible = !GridVisible;
    }

    [RelayCommand]
    public async Task EditPanelPropertiesAsync() {
        if (Panel is { } panel && _navigation is {} navigation) {
            Console.WriteLine("Panel Properties Editor: Launching");
            var propertiesViewModel = new PanelPropertyViewModel(panel);
            bool result = await PropertyDisplayService.ShowPropertiesAsync(
                navigation, propertiesViewModel, ScreenWidth, ScreenHeight);

            if (result) {
                // Properties were applied and closed (e.g., "Done" or "Close" was hit)
                System.Diagnostics.Debug.WriteLine("Properties applied successfully.");
            } else {
                // Properties dialog was dismissed without explicit apply (e.g., tap outside popup)
                System.Diagnostics.Debug.WriteLine("Properties view dismissed.");
            }
            Console.WriteLine("Panel Properties Editor: Closed");
        }
    }

    [RelayCommand]
    private async Task EditTilePropertiesAsync() {
        if (_navigation is {} navigation && SelectedEntities?.Count > 0) {
            var propertiesViewModel = new DynamicPropertyPageViewModel(SelectedEntities);
            bool result = await PropertyDisplayService.ShowPropertiesAsync(
                navigation, propertiesViewModel, ScreenWidth, ScreenHeight);

            if (result) {
                // Properties were applied and closed (e.g., "Done" or "Close" was hit)
                System.Diagnostics.Debug.WriteLine("Properties applied successfully.");
            } else {
                // Properties dialog was dismissed without explicit apply (e.g., tap outside popup)
                System.Diagnostics.Debug.WriteLine("Properties view dismissed.");
            }
        }
    }
}