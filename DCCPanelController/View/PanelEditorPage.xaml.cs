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
            vm.PropertyChanged += (_, args) => {
                Console.WriteLine($"PanelEditorPage.PropertyChanged: {args.PropertyName}");
                if (args.PropertyName == nameof(DesignMode)) UpdateToolbarItems();
                ConfigureToolbarItems();
            };
            AssignToolbarCommands();
            UpdateToolbarItems();
        }
    }

    // ------------------------------------------------------------------------------------------------
    private readonly ToolbarItem _showModeToolbar = new ToolbarItem { Text = "Select Mode", IsEnabled = false};
    private readonly ToolbarItem _selectModeToolbar = new ToolbarItem { Text = "Select Mode"};
    private readonly ToolbarItem _deleteTileToolbar = new ToolbarItem { Text = "Delete Tile", IconImageSource = IconTileDelete};
    private readonly ToolbarItem _propertiesToolbar = new ToolbarItem { Text = "Properties"};
    private readonly ToolbarItem _exitEditModeToolbar = new ToolbarItem { Text = "Exit Edit Mode", IconImageSource = IconExitEdit};
    private readonly ToolbarItem _toggleFullscreenToolbar = new ToolbarItem { Text = "Toggle Fullscreen"};
    private readonly ToolbarItem _addPanelToolbar = new ToolbarItem { Text = "Add Panel", IconImageSource = IconAdd };
    private readonly ToolbarItem _duplicatePanelToolbar = new ToolbarItem { Text = "Duplicate Panel", IconImageSource = IconDuplicate };
    private readonly ToolbarItem _deletePanelToolbar = new ToolbarItem { Text = "Delete Panel", IconImageSource = IconDelete};
    private readonly ToolbarItem _editPanelToolbar = new ToolbarItem { Text = "Edit Panel", IconImageSource = IconEdit};
    private readonly ToolbarItem _spacerToolbar = new ToolbarItem { Text = "", IsEnabled = false};

    private const string IconMove = "move.png";
    private const string IconCopy = "copy.png";
    private const string IconResize = "crop.png";
    private const string IconRotate = "rotate_cw.png";
    private const string IconTileDelete = "trash_2.png";
    private const string IconExitEdit = "log_out.png";
    private const string IconAdd = "plus_circle.png";
    private const string IconDelete = "trash_2.png";
    private const string IconEdit = "edit.png";
    private const string IconDuplicate = "copy.png";
    private const string IconEditTileProperties = "edit_3.png";
    private const string IconEditPanelProperties = "settings.png";
    private const string IconFullscreenLarge = "maximize_2.png";
    private const string IconFullscreenSmall = "minimize_2.png";
    private const string IconSpacer = "blank.png";
    
    private void AssignToolbarCommands() {
        if (_viewModel is { } vm) {
            _selectModeToolbar.Command = new Command(vm.ToggleMode);
            _deleteTileToolbar.Command = new Command(vm.DeleteTile);
            _propertiesToolbar.Command = new Command(vm.EditProperties);
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
            _selectModeToolbar.IconImageSource = vm.EditMode switch {
                EditModeEnum.Copy   => IconCopy,
                EditModeEnum.Move   => IconMove,
                EditModeEnum.Rotate => IconRotate,
                EditModeEnum.Size   => IconResize,
                _                   => IconMove
            };
            _showModeToolbar.Text = vm.EditMode.ToString();
            _deleteTileToolbar.IsEnabled = vm.SelectedPanel?.SelectedTiles.Count > 0;
            _propertiesToolbar.IconImageSource = vm.SelectedPanel?.SelectedTiles.Count == 0 ? IconEditPanelProperties : IconEditTileProperties;

            _addPanelToolbar.IsEnabled = true;
            _duplicatePanelToolbar.IsEnabled = vm.SelectedPanel != null;
            _deletePanelToolbar.IsEnabled = vm.SelectedPanel != null;
            _editPanelToolbar.IsEnabled = vm.SelectedPanel != null;

            _toggleFullscreenToolbar.IconImageSource = vm.IsFullScreen ? IconFullscreenSmall : IconFullscreenLarge;
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
                ToolbarItems.Add(_selectModeToolbar);
                ToolbarItems.Add(_deleteTileToolbar);
                ToolbarItems.Add(_propertiesToolbar);
                ToolbarItems.Add(_spacerToolbar);
                ToolbarItems.Add(_exitEditModeToolbar);
                ToolbarItems.Add(_toggleFullscreenToolbar);
            } else {
                // In View mode so we need a toolbar that supports the panels.
                // --------------------------------------------------------------------------------------------------
                ToolbarItems.Add(_addPanelToolbar);
                ToolbarItems.Add(_duplicatePanelToolbar);
                ToolbarItems.Add(_deletePanelToolbar);
                ToolbarItems.Add(_editPanelToolbar);
                ToolbarItems.Add(_spacerToolbar);
                ToolbarItems.Add(_toggleFullscreenToolbar);
            }
            ConfigureToolbarItems();
        }
    }
}