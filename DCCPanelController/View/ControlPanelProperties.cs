using System.Collections.Specialized;
using DCCPanelController.Models.DataModel;
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
        if (bindable is ControlPanelView control) {
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
            control.DrawPanel();
        }
    }

    /// <summary>
    ///     If the Panel object is changed, then we need to clear and rebuild the whole Panel
    /// </summary>
    private static void OnPanelChanged(BindableObject bindable, object oldValue, object newValue) {
        if (bindable is ControlPanelView control) {
            if (oldValue != newValue) {
                if (oldValue is Panel oldPanel) {
                    oldPanel.Entities.CollectionChanged -= EntitiesOnCollectionChanged;
                }
                if (newValue is Panel newPanel) {
                    newPanel.Entities.CollectionChanged += (_, args) => EntitiesOnCollectionChanged(control, args);
                    control.Panel = newPanel;
                    control.ClearAllSelectedTiles();
                    control.DrawPanel(true);
                }
            }
        }
    }

    private static void EntitiesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        Console.WriteLine("Entities Collection Changed....");
        if (sender is ControlPanelView control) {
            control.ClearAllSelectedTiles();

            // Any items that have been removed from the collection need to be
            // removed from the display panel. 
            // -------------------------------------------------------------------------
            var oldEntities = e.OldItems?.Cast<Entity>().ToList() ?? new List<Entity>();
            foreach (var oldEntity in oldEntities) {
                Console.WriteLine($"Removing {oldEntity.EntityName} from the grid");
                control.RemoveEntityFromGrid(oldEntity);
            }

            // Any new items added to the Panel.Entities collection need to be drawn
            // on the display panel. This will iterate and add the items
            // -------------------------------------------------------------------------
            var newEntities = e.NewItems?.Cast<Entity>().ToList() ?? new List<Entity>();
            ITile? lastTile = null;
            foreach (var newEntity in newEntities) {
                Console.WriteLine($"Adding {newEntity.EntityName} to the grid");
                lastTile = control.AddEntityToGrid(newEntity);
            }

            // Highlight the last item added to the collection. Normally there would 
            // only be one anyway, but just in case we have a mass-add function.
            // ----------------------------------------------------------------------
            if (lastTile is { } ITile) {
                control.MarkTileSelected(lastTile);
            }
        }
    }

    private static void OnClientChanged(BindableObject bindable, object oldvalue, object newvalue) {
        if (bindable is ControlPanelView control) {
            Console.WriteLine("Client Connection Property Changed");
        }
    }

    private static void OnShowTrackErrorsChanged(BindableObject bindable, object oldvalue, object newvalue) {
        if (bindable is ControlPanelView control) { }
    }

    private static void OnShowGridChanged(BindableObject bindable, object oldvalue, object newvalue) {
        if (bindable is ControlPanelView control) {
            control.DrawGrid();
        }
    }

    private static void OnEditModeChanged(BindableObject bindable, object oldvalue, object newvalue) { }
}