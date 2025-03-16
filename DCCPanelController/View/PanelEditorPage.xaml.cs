using System.ComponentModel;
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
            PanelView.TileSelected += (sender, args) => vm.EditTileProperties(args.Tile);
            vm.PropertyChanged += ViewModelPropertyChanged;
            AssignToolbarCommands();
            UpdateToolbarItems();
        }
    }

    private void ViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e) {
        Console.WriteLine($"PanelEditorPage.PropertyChanged: {e.PropertyName}");
        switch (e.PropertyName) {
        case nameof(DesignMode):
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
private readonly ToolbarItem _showModeToolbar = new ToolbarItem { Text = "Select Mode", IsEnabled = false };
private readonly ToolbarItem _selectModeToolbar = new ToolbarItem { Text = "Select Mode" };
private readonly ToolbarItem _panelPropertiesToolbar = new ToolbarItem { Text = "Properties" };
private readonly ToolbarItem _exitEditModeToolbar = new ToolbarItem { Text = "Exit Edit Mode", IconImageSource = IconExitEdit };
private readonly ToolbarItem _toggleFullscreenToolbar = new ToolbarItem { Text = "Toggle Fullscreen" };
private readonly ToolbarItem _addPanelToolbar = new ToolbarItem { Text = "Add Panel", IconImageSource = IconPanelAdd };
private readonly ToolbarItem _duplicatePanelToolbar = new ToolbarItem { Text = "Duplicate Panel", IconImageSource = IconPanelDuplicate };
private readonly ToolbarItem _deletePanelToolbar = new ToolbarItem { Text = "Delete Panel", IconImageSource = IconPanelDelete };
private readonly ToolbarItem _editPanelToolbar = new ToolbarItem { Text = "Edit Panel", IconImageSource = IconPanelEdit };
private readonly ToolbarItem _spacerToolbar = new ToolbarItem { Text = "", IsEnabled = false };
private const string IconTileMove = "move.png";
private const string IconTileCopy = "copy.png";
private const string IconTileResize = "crop.png";
private const string IconTileRotate = "rotate_cw.png";
private const string IconTileSelect = "edit_3.png";
private const string IconTileDelete = "trash_2.png";
private const string IconExitEdit = "log_out.png";
private const string IconPanelAdd = "plus_circle.png";
private const string IconPanelDelete = "trash_2.png";
private const string IconPanelEdit = "edit.png";
private const string IconPanelDuplicate = "copy.png";
private const string IconPanelProperties = "settings.png";
private const string IconFullscreenLarge = "maximize_2.png";
private const string IconFullscreenSmall = "minimize_2.png";
private const string IconSpacer = "blank.png";

private void AssignToolbarCommands() {
    if (_viewModel is { } vm) {
        _showModeToolbar.Command = new Command(vm.ToggleMode);
        _selectModeToolbar.Command = new Command(vm.ToggleMode);
        _panelPropertiesToolbar.Command = new Command(vm.EditPanelProperties);
        _exitEditModeToolbar.Command = new Command(vm.ExitEditMode);
        _toggleFullscreenToolbar.Command = new Command(vm.ToggleFullscreen);
        _addPanelToolbar.Command = new Command(vm.AddPanel);
        _deletePanelToolbar.Command = new Command(vm.DeletePanel);
        _editPanelToolbar.Command = new Command(vm.EditPanel);
        _duplicatePanelToolbar.Command = new Command(vm.DuplicatePanel);
    }
}

private void ConfigureToolbarItems() {
    if (_viewModel is { } vm) {
        _ = vm.EditMode switch {
            EditModeEnum.Copy   => SetEditMode(IconTileCopy, "Copy"),
            EditModeEnum.Move   => SetEditMode(IconTileMove, "Move"),
            EditModeEnum.Rotate => SetEditMode(IconTileRotate, "Rotate"),
            EditModeEnum.Size   => SetEditMode(IconTileResize, "Size"),
            EditModeEnum.Delete => SetEditMode(IconTileDelete, "Delete"),
            EditModeEnum.Select => SetEditMode(IconTileSelect, "Properties"),
            _                   => SetEditMode(IconTileMove, "Move"),
        };
        _addPanelToolbar.IsEnabled = true;
        _duplicatePanelToolbar.IsEnabled = vm.SelectedPanel != null;
        _deletePanelToolbar.IsEnabled = vm.SelectedPanel != null;
        _editPanelToolbar.IsEnabled = vm.SelectedPanel != null;
        _panelPropertiesToolbar.IconImageSource = IconPanelProperties;
        _toggleFullscreenToolbar.IconImageSource = vm.IsFullScreen ? IconFullscreenSmall : IconFullscreenLarge;
    }
}

private bool SetEditMode(string icon, string text) {
    _selectModeToolbar.IconImageSource = icon;
    _showModeToolbar.Text = text;
    return true;
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
            ToolbarItems.Add(_showModeToolbar);
            ToolbarItems.Add(_selectModeToolbar);
            ToolbarItems.Add(_panelPropertiesToolbar);
            ToolbarItems.Add(_exitEditModeToolbar);
            ToolbarItems.Add(_toggleFullscreenToolbar);
        } else {
            // In View mode so we need a toolbar that supports the panels.
            // --------------------------------------------------------------------------------------------------
            ToolbarItems.Add(_addPanelToolbar);
            ToolbarItems.Add(_duplicatePanelToolbar);
            ToolbarItems.Add(_deletePanelToolbar);
            ToolbarItems.Add(_editPanelToolbar);
            ToolbarItems.Add(_toggleFullscreenToolbar);
        }
        ConfigureToolbarItems();
    }
}

}