using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Services;
using DCCPanelController.View.Helpers;

namespace DCCPanelController.View;

public partial class PanelEditorPage {
    private readonly ToolbarItem _addPanelToolbar = new() { Text = "Add Panel", IconImageSource = "plus_circle.png" };
    private readonly ConnectionService? _connectionService;
    private readonly ToolbarItem _deletePanelToolbar = new() { Text = "Delete Panel", IconImageSource = "trash_2.png" };
    private readonly ToolbarItem _duplicatePanelToolbar = new() { Text = "Duplicate Panel", IconImageSource = "copy.png" };
    private readonly ToolbarItem _editCopyToolbar = new() { Text = "Copy", IconImageSource = "copy.png" };
    private readonly ToolbarItem _editDeleteToolbar = new() { Text = "Delete", IconImageSource = "trash_2.png" };
    private readonly ToolbarItem _editMoveCopyResizeToolbar = new() { Text = "Move", IconImageSource = "move.png" };
    private readonly ToolbarItem _editPanelToolbar = new() { Text = "Edit Panel", IconImageSource = "edit.png" };

// ------------------------------------------------------------------------------------------------
    private readonly ToolbarItem _editPropertiesToolbar = new() { Text = "Properties", IconImageSource = "edit_3.png" };
    private readonly ToolbarItem _editRotateToolbar = new() { Text = "Rotate", IconImageSource = "rotate_cw.png" };
    private readonly ToolbarItem _editToggleGridToolbar = new() { Text = "ToggleGrid", IconImageSource = "grid_off.png" };

    private readonly ToolbarItem _exitEditModeToolbar = new() { Text = "Exit Edit Mode", IconImageSource = "log_out.png" };
    private readonly ToolbarItem _panelConnectionToolbar = new() { Text = "Connection", IconImageSource = "wifi_off.png" };
    private readonly ToolbarItem _panelDownloadToolbar = new() { Text = "Download Panel", IconImageSource = "download.png" };
    private readonly ToolbarItem _panelPropertiesToolbar = new() { Text = "Properties", IconImageSource = "settings.png" };
    private readonly ToolbarItem _panelUploadToolbar = new() { Text = "Upload Panel", IconImageSource = "upload.png" };
    private readonly ToolbarItem _spacerToolbar = new() { Text = "", IsEnabled = false };
    private readonly ToolbarItem _toggleFullscreenToolbar = new() { Text = "Toggle Fullscreen" };
    private PanelEditorViewModel? _viewModel;

    public PanelEditorPage(PanelEditorViewModel viewModel, ConnectionService connectionService) {
        InitializeComponent();
        _connectionService = connectionService;
        _connectionService.ConnectionChanged += ConnectionServiceOnConnectionChanged;
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    private void ConnectionServiceOnConnectionChanged(object? sender, ConnectionChangedEvent e) {
        _panelConnectionToolbar.IconImageSource = e.IsConnected ? "wifi.png" : "wifi_off.png";
    }

    protected override void OnBindingContextChanged() {
        base.OnBindingContextChanged();
        if (BindingContext is PanelEditorViewModel { } vm) {
            _viewModel = vm;
            PanelListView.SelectedItem = vm.SelectedPanel;
            PanelView.TileSelected += (sender, e) => {
                if (e is { } args) {
                    vm.SelectedTiles = args.Tiles;
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
            if (_viewModel is { } vm) vm.GridVisible = true;
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
            _panelConnectionToolbar.Command = new AsyncRelayCommand(vm.ToggleConnection);
            _panelDownloadToolbar.Command = new AsyncRelayCommand(vm.DownloadPanelAsync);
            _panelUploadToolbar.Command = new AsyncRelayCommand(vm.UploadPanelAsync);
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
            _editPropertiesToolbar.IsEnabled = vm is { HasSelectedEntities: true };
            _editCopyToolbar.IsEnabled = vm is { SingleEntitySelected     : true };
            _editDeleteToolbar.IsEnabled = vm is { HasSelectedEntities    : true };
            _editRotateToolbar.IsEnabled = vm is { HasSelectedEntities    : true, SelectedEntity: IRotationEntity };
            _addPanelToolbar.IsEnabled = true;
            _duplicatePanelToolbar.IsEnabled = vm.SelectedPanel != null;
            _deletePanelToolbar.IsEnabled = vm.SelectedPanel != null;
            _editPanelToolbar.IsEnabled = vm.SelectedPanel != null;
            _panelDownloadToolbar.IsEnabled = vm.SelectedPanel != null;
            _panelUploadToolbar.IsEnabled = true;
            _panelConnectionToolbar.IconImageSource = _connectionService?.IsConnected ?? false ? "wifi.png" : "wifi_off.png";
            _editToggleGridToolbar.IconImageSource = vm.GridVisible ? "grid_on.png" : "grid_off.png";
            _toggleFullscreenToolbar.IconImageSource = vm.IsFullScreen ? "minimize_2.png" : "maximize_2.png";
        }
    }

    /// <summary>
    ///     Updates the toolbar items based on the current mode (Design/Edit or View mode)
    ///     in the PanelEditorPage. This method clears the existing toolbar items
    ///     and repopulates them with commands relevant to the current mode.
    /// </summary>
    /// <remarks>
    ///     In Design/Edit mode, toolbar items include commands for managing tiles like deleting and editing properties.
    ///     In View mode, toolbar items include commands for managing panels like adding, editing, and deleting panels.
    ///     This method dynamically adjusts the available toolbar options based on the
    ///     <see cref="PanelEditorViewModel.DesignMode" /> property.
    /// </remarks>
    private void UpdateToolbarItems() {
        ToolbarItems.Clear();
        if (_viewModel is { } vm) {
            if (vm.DesignMode) {
                // In Design/Edit mode so we need a toolbar that supports the tiles.
                // --------------------------------------------------------------------------------------------------
                ToolbarItems.Add(_editRotateToolbar);
                ToolbarItems.Add(_editDeleteToolbar);
                ToolbarItems.Add(_editPropertiesToolbar);
                ToolbarItems.Add(_editMoveCopyResizeToolbar);
                ToolbarItems.Add(_panelPropertiesToolbar);
                ToolbarItems.Add(_editToggleGridToolbar);
                ToolbarItems.Add(_exitEditModeToolbar);
            } else {
                // In View mode so we need a toolbar that supports the panels.
                // --------------------------------------------------------------------------------------------------
                ToolbarItems.Add(_addPanelToolbar);
                ToolbarItems.Add(_duplicatePanelToolbar);
                ToolbarItems.Add(_deletePanelToolbar);
                ToolbarItems.Add(_editPanelToolbar);
                ToolbarItems.Add(_panelDownloadToolbar);
                ToolbarItems.Add(_panelUploadToolbar);
                ToolbarItems.Add(_panelConnectionToolbar);
            }
            ToolbarItems.Add(_toggleFullscreenToolbar);
            ConfigureToolbarItems();
        }
    }
}