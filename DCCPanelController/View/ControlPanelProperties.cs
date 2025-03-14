using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Interfaces;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.View.Helpers;

namespace DCCPanelController.View;

public partial class ControlPanelView {
    public static readonly BindableProperty PanelProperty = BindableProperty.Create(nameof(Panel), typeof(Panel), typeof(ControlPanelView), propertyChanged: OnPanelChanged);
    public static readonly BindableProperty DesignModeProperty = BindableProperty.Create(nameof(DesignMode), typeof(bool), typeof(ControlPanelView), false, BindingMode.Default, propertyChanged: OnDesignModeChanged);
    public static readonly BindableProperty ShowGridProperty = BindableProperty.Create(nameof(ShowGrid), typeof(bool), typeof(ControlPanelView), false, BindingMode.Default, propertyChanged: OnShowGridChanged);
    public static readonly BindableProperty ShowTrackErrorsProperty = BindableProperty.Create(nameof(ShowTrackErrors), typeof(bool), typeof(ControlPanelView), false, BindingMode.Default, propertyChanged: OnShowTrackErrorsChanged);
    public static readonly BindableProperty EditModeProperty = BindableProperty.Create(nameof(EditMode), typeof(EditModeEnum), typeof(ControlPanelView), EditModeEnum.Move, BindingMode.Default, propertyChanged: OnEditModeChanged);
    
    public Panel? Panel {
        get => (Panel)GetValue(PanelProperty);
        set => SetValue(PanelProperty, value);
    }

    public bool DesignMode {
        get => (bool)GetValue(DesignModeProperty);
        set => SetValue(DesignModeProperty, value);
    }

    public bool ShowGrid {
        get => (bool)GetValue(ShowGridProperty);
        set => SetValue(ShowGridProperty, value);
    }

    public EditModeEnum EditMode {
        get => (EditModeEnum)GetValue(EditModeProperty);
        set => SetValue(EditModeProperty, value);
    }

    public bool ShowTrackErrors {
        get => (bool)GetValue(ShowTrackErrorsProperty);
        set => SetValue(ShowTrackErrorsProperty, value);
    }
    
    private static void OnDesignModeChanged(BindableObject bindable, object oldValue, object newValue) {
        var control = (ControlPanelView)bindable;
        control.ShowGrid = control.DesignMode;
        control.DynamicGrid.GestureRecognizers.Clear();
        if (control.DesignMode) {
            // In design mode we need to support drag and drop for the tiles on the screen.
            // ----------------------------------------------------------------------------
            var dropRecogniser = new DropGestureRecognizer();
            dropRecogniser.Drop += control.DropTileOnPanel;
            dropRecogniser.DragOver += control.DragOverTileOnPanel;
            dropRecogniser.DragLeave += control.DragLeaveTileOnPanel;
            control.DynamicGrid.GestureRecognizers.Add(dropRecogniser);

            // In design mode, also support tapping anywhere that is not a tile so we clear selections.
            // ----------------------------------------------------------------------------
            var tapRecogniser = new TapGestureRecognizer();
            tapRecogniser.Tapped += control.DynamicGridTapped;
            control.DynamicGrid.GestureRecognizers.Add(tapRecogniser);
        }
        control.DrawPanel(true);
    }
    
    /// <summary>
    /// If the Panel object is changed, then we need to clear and rebuild the whole Panel
    /// </summary>
    private static void OnPanelChanged(BindableObject bindable, object oldValue, object newValue) {
        var control = (ControlPanelView)bindable;
        control.ClearSelectedTiles();
        control.DrawPanel(true);
    }
    
    private static void OnShowTrackErrorsChanged(BindableObject bindable, object oldvalue, object newvalue) {
        var control = (ControlPanelView)bindable;
    }

    private static void OnShowGridChanged(BindableObject bindable, object oldvalue, object newvalue) {
        var control = (ControlPanelView)bindable;
        control.DrawGrid();
    }

    private static void OnEditModeChanged(BindableObject bindable, object oldvalue, object newvalue) {
        var control = (ControlPanelView)bindable;
        switch (control.EditMode) {
            case EditModeEnum.Move:
                Console.WriteLine("EditMode=>Move");
                control._canDragTiles = true;
                break;
            case EditModeEnum.Rotate:
                Console.WriteLine("EditMode=>Rotate");
                control._canDragTiles = false;
                break;
            case EditModeEnum.Copy:
                Console.WriteLine("EditMode=>Copy");
                control._canDragTiles = true;
                break;
            case EditModeEnum.Size:
                Console.WriteLine("EditMode=>Size");
                control._canDragTiles = true;
                break;
            case EditModeEnum.Select:
                Console.WriteLine("EditMode=>Select");
                control._canDragTiles = false;
                break;
        }
        
    }
}