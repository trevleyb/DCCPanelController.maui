using System.ComponentModel;
using DCCPanelController.Models.DataModel.Interfaces;
using DCCPanelController.View.Helpers;

namespace DCCPanelController.View;

public partial class PanelEditorPage {
    private PanelEditorViewModel? _viewModel;

    public PanelEditorPage() {
        Console.WriteLine($"PanelEditorPage.ctor");
        InitializeComponent();
    }

    protected override void OnBindingContextChanged() {
        base.OnBindingContextChanged();
        if (BindingContext is PanelEditorViewModel { } vm) {
            _viewModel = vm;
            PanelListView.SelectedItem = vm.SelectedPanel;
            PanelView.TileSelected += (sender, e) => {
                if (e is {} args) {
                    vm.SelectedEntity = args.Tile?.Entity ?? null;
                    ConfigureToolbarItems();
                    if (args.IsDoubleTap) vm.EditTileProperties();
                }
            };
            vm.PropertyChanged += ViewModelPropertyChanged;
            AssignToolbarCommands();
            UpdateToolbarItems();
        }
    }

    private void ViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        Console.WriteLine($"PanelEditorPage.PropertyChanged: {e.PropertyName}");
        switch (e.PropertyName) {
        case nameof(DesignMode):
            PanelView.ShowGrid = true;
            if (_viewModel is {} vm) vm.GridVisible = true;
            UpdateToolbarItems();
            break;

        case nameof(_viewModel.GridVisible):
            PanelView.ShowGrid = _viewModel?.GridVisible ?? false;
            UpdateToolbarItems();
            break;

        case nameof(_viewModel.SelectedPanel):
            PanelView.Panel = _viewModel?.SelectedPanel;
            break;

        case nameof(_viewModel.PropertiesChanged):
            if (_viewModel is { PropertiesChanged: true }) {
                PanelView.ForceRefresh();
                _viewModel.PropertiesChanged = false;
            }
            break;

        case nameof(_viewModel.EditMode):
            PanelView.EditMode = _viewModel?.EditMode ?? EditModeEnum.Move;
            break;
        }
        ConfigureToolbarItems();
    }

// ------------------------------------------------------------------------------------------------
    private readonly ToolbarItem _editPropertiesToolbar = new ToolbarItem { Text = "Properties", IconImageSource = "edit_3.png" };
    private readonly ToolbarItem _editMoveCopyResizeToolbar = new ToolbarItem { Text = "Move", IconImageSource = "move.png"};
    private readonly ToolbarItem _editCopyToolbar = new ToolbarItem { Text = "Copy", IconImageSource = "copy.png"};
    private readonly ToolbarItem _editDeleteToolbar = new ToolbarItem { Text = "Delete", IconImageSource = "trash_2.png"};
    private readonly ToolbarItem _editRotateToolbar = new ToolbarItem { Text = "Rotate", IconImageSource = "rotate_cw.png"};
    private readonly ToolbarItem _editToggleGridToolbar = new ToolbarItem { Text = "ToggleGrid", IconImageSource = "grid_off.png"};
    
    private readonly ToolbarItem _exitEditModeToolbar = new ToolbarItem { Text = "Exit Edit Mode", IconImageSource = "log_out.png" };
    private readonly ToolbarItem _toggleFullscreenToolbar = new ToolbarItem { Text = "Toggle Fullscreen" };
    private readonly ToolbarItem _addPanelToolbar = new ToolbarItem { Text = "Add Panel", IconImageSource = "plus_circle.png" };
    private readonly ToolbarItem _duplicatePanelToolbar = new ToolbarItem { Text = "Duplicate Panel", IconImageSource = "copy.png" };
    private readonly ToolbarItem _deletePanelToolbar = new ToolbarItem { Text = "Delete Panel", IconImageSource = "trash_2.png" };
    private readonly ToolbarItem _editPanelToolbar = new ToolbarItem { Text = "Edit Panel", IconImageSource = "edit.png" };
    private readonly ToolbarItem _panelPropertiesToolbar = new ToolbarItem { Text = "Properties", IconImageSource = "settings.png" };
    private readonly ToolbarItem _spacerToolbar = new ToolbarItem { Text = "", IsEnabled = false };

    //private const string IconFullscreenLarge = "maximize_2.png";
    //private const string IconFullscreenSmall = "minimize_2.png";

    private void AssignToolbarCommands() {
        if (_viewModel is { } vm) {

            _editToggleGridToolbar.Command = new Command(vm.ToggleGrid);
            _editMoveCopyResizeToolbar.Command = new Command(vm.ToggleMode);
            _editDeleteToolbar.Command = new Command(vm.DeleteSelectedTile);
            _editRotateToolbar.Command = new Command(vm.RotateSelectedTile);
            _editPropertiesToolbar.Command = new Command(vm.EditTileProperties);
            _exitEditModeToolbar.Command = new Command(vm.ExitEditMode);
            _toggleFullscreenToolbar.Command = new Command(vm.ToggleFullscreen);
            _addPanelToolbar.Command = new Command(vm.AddPanel);
            _deletePanelToolbar.Command = new Command(vm.DeletePanel);
            _editPanelToolbar.Command = new Command(vm.EditPanel);
            _duplicatePanelToolbar.Command = new Command(vm.DuplicatePanel);
            _panelPropertiesToolbar.Command = new Command(vm.EditPanelProperties);
        }
    }

    private void ConfigureToolbarItems() {
        if (_viewModel is { } vm) {
            _editMoveCopyResizeToolbar.IconImageSource = vm.EditMode switch {
                EditModeEnum.Move => "move.png",
                EditModeEnum.Copy => "copy.png",
                EditModeEnum.Size => "crop.png",
                _                 => "move.png"
            };

            _editPropertiesToolbar.IsEnabled = vm.IsEntitySelected;
            _editCopyToolbar.IsEnabled = vm.IsEntitySelected;
            _editDeleteToolbar.IsEnabled = vm.IsEntitySelected;
            _editRotateToolbar.IsEnabled = vm.IsEntitySelected;
           
            _addPanelToolbar.IsEnabled = true;
            _duplicatePanelToolbar.IsEnabled = vm.SelectedPanel != null;
            _deletePanelToolbar.IsEnabled = vm.SelectedPanel != null;
            _editPanelToolbar.IsEnabled = vm.SelectedPanel != null;
            _editToggleGridToolbar.IconImageSource = vm.GridVisible ? "grid_on.png" : "grid_off.png";
            _toggleFullscreenToolbar.IconImageSource = vm.IsFullScreen ? "minimize_2.png" : "maximize_2.png";
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
        ToolbarItems.Clear();
        if (_viewModel is { } vm) {
            if (vm.DesignMode) {
                // In Design/Edit mode so we need a toolbar that supports the tiles.
                // --------------------------------------------------------------------------------------------------
                ToolbarItems.Add(_editMoveCopyResizeToolbar);
                ToolbarItems.Add(_editRotateToolbar);
                ToolbarItems.Add(_editDeleteToolbar);
                ToolbarItems.Add(_editPropertiesToolbar);
                ToolbarItems.Add(_spacerToolbar);
                ToolbarItems.Add(_panelPropertiesToolbar);
                //ToolbarItems.Add(_editToggleGridToolbar);
                ToolbarItems.Add(_exitEditModeToolbar);
            } else {
                // In View mode so we need a toolbar that supports the panels.
                // --------------------------------------------------------------------------------------------------
                ToolbarItems.Add(_addPanelToolbar);
                ToolbarItems.Add(_duplicatePanelToolbar);
                ToolbarItems.Add(_deletePanelToolbar);
                ToolbarItems.Add(_editPanelToolbar);
            }
            ToolbarItems.Add(_toggleFullscreenToolbar);
            ConfigureToolbarItems();
        }
    }
}