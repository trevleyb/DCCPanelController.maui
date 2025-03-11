using DCCPanelController.Models.DataModel;
using DCCPanelController.View.Helpers;

namespace DCCPanelController.View;

public partial class PanelEditorPage : ContentPage {

    PanelEditorViewModel? ViewModel;

    public PanelEditorPage() {
        InitializeComponent();
        ViewModel = new PanelEditorViewModel(new Panels());
        BindingContext = ViewModel;
        AssignToolbarCommands(ViewModel);
        UpdateToolbarItems();
        ViewModel.PropertyChanged += (sender, args) => {
            Console.WriteLine($"PanelEditorPage.PropertyChanged: {args.PropertyName}");
            if (args.PropertyName == nameof(DesignMode)) UpdateToolbarItems();
            ConfigureToolbarItems();
        };
    }

    // ------------------------------------------------------------------------------------------------
    private ToolbarItem showModeToolbar = new ToolbarItem { Text = "Select Mode", IsEnabled = false};
    private ToolbarItem selectModeToolbar = new ToolbarItem { Text = "Select Mode"};
    private ToolbarItem deleteTileToolbar = new ToolbarItem { Text = "Delete Tile", IconImageSource = iconTileDelete};
    private ToolbarItem propertiesToolbar = new ToolbarItem { Text = "Properties"};
    private ToolbarItem exitEditModeToolbar = new ToolbarItem { Text = "Exit Edit Mode", IconImageSource = iconExitEdit};
    private ToolbarItem toggleFullscreenToolbar = new ToolbarItem { Text = "Toggle Fullscreen"};
    private ToolbarItem addPanelToolbar = new ToolbarItem { Text = "Add Panel", IconImageSource = iconAdd };
    private ToolbarItem deletePanelToolbar = new ToolbarItem { Text = "Delete Panel", IconImageSource = iconDelete};
    private ToolbarItem editPanelToolbar = new ToolbarItem { Text = "Edit Panel", IconImageSource = iconEdit};
    private ToolbarItem spacerToolbar = new ToolbarItem { Text = "", IsEnabled = false};

    private const string iconMove = "move.png";
    private const string iconCopy = "copy.png";
    private const string iconResize = "crop.png";
    private const string iconRotate = "rotate_cw.png";
    private const string iconTileDelete = "trash_2.png";
    private const string iconExitEdit = "log_out.png";
    private const string iconAdd = "file_plus.png";
    private const string iconDelete = "file_minus.png";
    private const string iconEdit = "file_text";
    private const string iconEditTileProperties = "edit_3.png";
    private const string iconEditPanelProperties = "edit.png";
    private const string iconFullscreenLarge = "maximize_2.png";
    private const string iconFullscreenSmall = "minimize_2.png";
    private const string iconSpacer = "blank.png";
    
    private void AssignToolbarCommands(PanelEditorViewModel vm) {
        selectModeToolbar.Command = new Command(vm.ToggleMode);
        deleteTileToolbar.Command = new Command(vm.DeleteTile);
        propertiesToolbar.Command = new Command(vm.EditProperties);
        exitEditModeToolbar.Command = new Command(vm.ExitEditMode);
        toggleFullscreenToolbar.Command = new Command(vm.ToggleFullscreen);
        addPanelToolbar.Command = new Command(vm.AddPanel);
        deletePanelToolbar.Command = new Command(vm.DeletePanel);
        editPanelToolbar.Command = new Command(vm.EditPanel);
    }

    private void ConfigureToolbarItems() {
        Console.WriteLine($"ConfigureToolbarItems: ");
        if (ViewModel is { } vm) {
            selectModeToolbar.IconImageSource = vm.EditMode switch {
                EditModeEnum.Copy   => iconCopy,
                EditModeEnum.Move   => iconMove,
                EditModeEnum.Rotate => iconRotate,
                EditModeEnum.Size   => iconResize,
                _                   => iconMove
            };
            showModeToolbar.Text = vm.EditMode.ToString();
            deleteTileToolbar.IsEnabled = vm.SelectedPanel?.SelectedTiles?.Count > 0;
            deleteTileToolbar.IconImageSource = vm.SelectedPanel?.SelectedTiles?.Count > 0 ? iconTileDelete : iconDelete;
            propertiesToolbar.IconImageSource = vm.SelectedPanel?.SelectedTiles?.Count == 0 ? iconEditPanelProperties : iconEditTileProperties;

            addPanelToolbar.IsEnabled = true;
            deletePanelToolbar.IsEnabled = vm.SelectedPanel != null;
            editPanelToolbar.IsEnabled = vm.SelectedPanel != null;

            toggleFullscreenToolbar.IconImageSource = vm.IsFullScreen ? iconFullscreenSmall : iconFullscreenLarge;
        }
    }
    
    /// <summary>
    /// Updates the toolbar items based on the current mode (Design/Edit or View mode)
    /// in the PanelEditorPage. This method clears the existing toolbar items
    /// and repopulates them with commands relevant to the current mode.
    /// </summary>
    /// <remarks>
    /// In Design/Edit mode, toolbar items include commands for managing tiles like deleting and editing properties.
    /// In View mode, toolbar items include commands for managing panels like adding, editing, and deleting panels.
    /// This method dynamically adjusts the available toolbar options based on the
    /// <see cref="PanelEditorViewModel.DesignMode"/> property.
    /// </remarks>
    private void UpdateToolbarItems() {
        Console.WriteLine($"UpdateToolbarItems: ");
        ToolbarItems.Clear();
        if (ViewModel is { } vm) {
            if (vm.DesignMode == true) {
                // In Design/Edit mode so we need a toolbar that supports the tiles.
                // --------------------------------------------------------------------------------------------------
                ToolbarItems.Add(selectModeToolbar);
                ToolbarItems.Add(deleteTileToolbar);
                ToolbarItems.Add(propertiesToolbar);
                ToolbarItems.Add(spacerToolbar);
                ToolbarItems.Add(exitEditModeToolbar);
                ToolbarItems.Add(toggleFullscreenToolbar);
            } else {
                // In View mode so we need a toolbar that supports the panels.
                // --------------------------------------------------------------------------------------------------
                ToolbarItems.Add(addPanelToolbar);
                ToolbarItems.Add(deletePanelToolbar);
                ToolbarItems.Add(editPanelToolbar);
                ToolbarItems.Add(spacerToolbar);
                ToolbarItems.Add(toggleFullscreenToolbar);
            }
            ConfigureToolbarItems();
        }
    }
}