using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using DCCPanelController.Components.Tracks;
using DCCPanelController.Components.Tracks.Base;
using DCCPanelController.Components.Tracks.Views;
using DCCPanelController.Helpers;
using DCCPanelController.Model;
using DCCPanelController.ViewModel;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;

namespace DCCPanelController.Components.Panel;

public partial class PanelViewer : ContentView {

    public GridHelper? GridHelper;
    private readonly ObservableCollection<ITrackViewModel> _tracks = [];
    
    public PanelViewer() {
        InitializeComponent();
        PanelEditorContainer.SizeChanged += PanelEditorContainerSizeChanged;
        _tracks.CollectionChanged += TracksOnCollectionChanged;
    }

    public static readonly BindableProperty PanelProperty =
        BindableProperty.Create(
            nameof(Panel),
            typeof(Model.Panel),
            typeof(PanelViewer),
            null,
            BindingMode.TwoWay,
            propertyChanged: OnPanelChanged);

    public Model.Panel Panel {
        get => (Model.Panel)GetValue(PanelProperty);
        set => SetValue(PanelProperty, value);
    }
    
    protected override void OnBindingContextChanged() {
        base.OnBindingContextChanged();
        if (BindingContext is Model.Panel panel) {
            // We have now bound to a Panel, so we need to do something about it.
        }
    }

    private static void OnPanelChanged(BindableObject bindable, object oldValue, object newValue) {
        var control = (PanelViewer)bindable;
        control.OnPanelChanged((Model.Panel)oldValue, (Model.Panel)newValue);
    }

    private void OnPanelChanged(Model.Panel oldPanel, Model.Panel newPanel) {
        
    }
    
    private void PanelEditorContainerSizeChanged(object? sender, EventArgs e) {
        if (sender is AbsoluteLayout layout && sender == PanelEditorContainer) {
            GridHelper = new GridHelper((int)layout.Width, (int)layout.Height);
            LoadTrackPlan();
            ResizePanelViewArea();
        }
    }
    
    private void LoadTrackPlan() {
        _tracks.Clear();
        if (Panel?.Tracks != null) {
            foreach (var track in Panel.Tracks) {
                var viewModel = TrackViewModelFactory.GetViewModel(track.TrackType);
                viewModel.Track = track;
                AddTrackToPlan(viewModel);
            }
        }
    }
    
    private void ResizePanelViewArea() {
        if (GridHelper is not null) {
            var rect = new Rect(GridHelper.XMargin, GridHelper.YMargin, GridHelper.PanelWidth, GridHelper.PanelHeight);
            PanelEditorContainer.SetLayoutBounds(PanelEditorViewPane, rect);
            PanelEditorContainer.SetLayoutFlags(PanelEditorViewPane,AbsoluteLayoutFlags.None);
        }
    }
    
    private void AddTrackToPlan(ITrackViewModel viewModel) {
        var gridData = GridHelper?.GetGridCoordinates(viewModel.Track.Coordinate);
        if (gridData is { IsOk: true } gd) {
            viewModel.Bounds = new Rect(gd.XOffset, gd.YOffset, gd.BoxSize, gd.BoxSize);
            _tracks.Add(viewModel);
        }
    }
    
    public void UpdatePanelEditorGrid() {
        RefreshPlanLayoutTracks(_tracks.ToList());
    }
    
    /// <summary>
    /// Forces a refresh of all the tracks on the screen. 
    /// </summary>
    private void RefreshPlanLayoutTracks(List<ITrackViewModel> trackPieces) {
        var tracks = trackPieces.ToList();
        _tracks.Clear();
        foreach (var track in tracks) {
            AddTrackToPlan(track);
        }
    }

    
    /// <summary>
    /// Manual Create/Update the display of the Tracks on the Screen as using a CollectionView
    /// was not working. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void TracksOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        switch (e.Action) {
        case NotifyCollectionChangedAction.Add:
            // Add any new items added to the collection.
            // ---------------------------------------------------------
            if (e.NewItems is not null && e.NewItems.Count > 0) {
                foreach (var item in e.NewItems) {
                    if (item is ITrackViewModel viewModel) {
                        var track = new TrackView(viewModel);
                        AbsoluteLayout.SetLayoutBounds(track, viewModel.Bounds);
                        AbsoluteLayout.SetLayoutFlags(track, AbsoluteLayoutFlags.None);
                    
                        var tapGestureRecognizer = new TapGestureRecognizer();
                        tapGestureRecognizer.Tapped += TapGestureRecognizerOnTapped;
                        track.GestureRecognizers.Add(tapGestureRecognizer);
                        
                        track.ZIndex = 10;
                        PanelEditorViewPane.Children.Add(track);
                    }
                }
            }
            break;
        
        case NotifyCollectionChangedAction.Remove:
            // Remove any deleted items added to the collection.
            // ---------------------------------------------------------
            if (e.OldItems is not null && e.OldItems.Count > 0) {
                foreach (var item in e.OldItems) {
                    if (item is ITrackViewModel track) {
                        var itemsToDelete = PanelEditorViewPane.Children.OfType<TrackView>().Where(view => view.ViewModel.Track.Coordinate.Equals(track.Track.Coordinate)).ToList();
                        foreach (var view in itemsToDelete) {
                            PanelEditorViewPane.Children.Remove(view);
                        }
                    }
                }
            }
            break;

        case NotifyCollectionChangedAction.Reset:
            var itemsToReset = PanelEditorViewPane.Children.OfType<TrackView>().ToList();
            foreach (var view in itemsToReset) {
                PanelEditorViewPane.Children.Remove(view);
            }
            break;
        }
    }

    private void TapGestureRecognizerOnTapped(object? sender, TappedEventArgs e) {
        // Add functions to send a message to the tapped item and do something
    }
}