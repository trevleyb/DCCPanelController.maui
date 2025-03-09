using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http.Headers;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers.Result;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Interfaces;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.Tiles;
using DCCPanelController.View.Helpers;
using Microsoft.Maui.Layouts;
using Entity = DCCPanelController.Models.DataModel.Entities.Entity;
#if IOS || MACCATALYST
using CoreGraphics;
using UIKit;
#endif

namespace DCCPanelController.View;

[ObservableObject]
public partial class ControlPanelView  {
    public enum CellHighlightAction {
        Selected,
        DragInvalid,
        DragValid
    }

    public static readonly BindableProperty PanelProperty = BindableProperty.Create(nameof(Panel), typeof(Panel), typeof(ControlPanelView), propertyChanged: OnPanelChanged);
    public static readonly BindableProperty DesignModeProperty = BindableProperty.Create(nameof(DesignMode), typeof(bool), typeof(ControlPanelView), false, BindingMode.Default, propertyChanged: OnDesignModeChanged);
    public static readonly BindableProperty ShowGridProperty = BindableProperty.Create(nameof(ShowGrid), typeof(bool), typeof(ControlPanelView), false, BindingMode.Default, propertyChanged: OnShowGridChanged);
    public static readonly BindableProperty ShowTrackErrorsProperty = BindableProperty.Create(nameof(ShowTrackErrors), typeof(bool), typeof(ControlPanelView), false, BindingMode.Default, propertyChanged: OnShowTrackErrorsChanged);

    [ObservableProperty] private ObservableCollection<ITile> _tiles = [];
    [ObservableProperty] private Color _gridColor = Colors.DarkGrey;
    [ObservableProperty] private double _gridSize;

    private int _lastDragCol;
    private int _lastDragRow;
    [ObservableProperty] private double _viewHeight;
    [ObservableProperty] private double _viewWidth;

    public EditModeEnum EditMode = EditModeEnum.Move;

    public ControlPanelView() {
        InitializeComponent();
        BindingContext = this;
        MainGrid.SizeChanged += OnGridSizeChanged;
    }

    public int Rows => Panel?.Rows ?? 1;
    public int Cols => Panel?.Cols ?? 1;

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

    public bool ShowTrackErrors {
        get => (bool)GetValue(ShowTrackErrorsProperty);
        set => SetValue(ShowTrackErrorsProperty, value);
    }
    
    private static void OnDesignModeChanged(BindableObject bindable, object oldValue, object newValue) {
        var control = (ControlPanelView)bindable;
        control.ShowGrid = control.DesignMode;
        control.DynamicGrid.GestureRecognizers.Clear();
        if (control.DesignMode) {
            var dropRecogniser = new DropGestureRecognizer();
            dropRecogniser.Drop += control.DropTileOnPanel;
            dropRecogniser.DragOver += control.DragOverTileOnPanel;
            dropRecogniser.DragLeave += control.DragLeaveTileOnPanel;
            control.DynamicGrid.GestureRecognizers.Add(dropRecogniser);
            
            // We need to redraw the panel so that we have the drag gestures for the tiles. 
            // ---------------------------------------------------------------------------
            control.DrawPanel(true);
        } 
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
        Console.WriteLine($"ControlPanelView.OnShowTrackErrorsChanged: {newvalue}");
    }

    private static void OnShowGridChanged(BindableObject bindable, object oldvalue, object newvalue) {
        var control = (ControlPanelView)bindable;
        control.DrawGrid();
        Console.WriteLine($"ControlPanelView.OnShowGridChanged: {newvalue}");
    }

    private void OnGridSizeChanged(object? sender, EventArgs e) {
        DrawPanel();
    }

    public bool HasGridSizeChanged(double width, double height) {
        if (width < 1.0 || height < 1.0) return false;
        var difference = Math.Abs(CalculateGridSize(width, height) - GridSize);
        return difference > 1;
    }

    public double CalculateGridSize(double width, double height) {
        if (width <= 0 || height <= 0) return 1;
        var gridSize = Math.Min(width / Cols, height / Rows);

        // Round down to the nearest 0.01
        gridSize = Math.Floor(gridSize * 100) / 100.0;
        return gridSize;
    }

    public void SetScreenSize(double width, double height) {
        GridSize = CalculateGridSize(width, height);
        ViewWidth = GridSize * Cols;
        ViewHeight = GridSize * Rows;
    }

    public void DrawPanel(bool forceRefresh = false) {
        // Only redraw the grid if we absolutely need to. Events may mean that this 
        // is called multiple times, but if we really have not changed, then do not 
        // waste time redrawing and rebuilding the grid. 
        // -------------------------------------------------------------------------
        if (Panel is null) return;
        if (MainGrid.Width < 1.0 || MainGrid.Height < 1.0) return;
        if (!forceRefresh && !HasGridSizeChanged(MainGrid.Width, MainGrid.Height)) return;

        var stopwatch = Stopwatch.StartNew();

        SetScreenSize(MainGrid.Width, MainGrid.Height);
        DynamicGrid.WidthRequest = ViewWidth;
        DynamicGrid.HeightRequest = ViewHeight;
        DynamicGrid.BackgroundColor = Panel?.BackgroundColor ?? Colors.Transparent;

        DynamicGrid.Children.Clear();

        if (DynamicGrid.RowDefinitions.Count != Rows || DynamicGrid.ColumnDefinitions.Count != Cols) {
            DynamicGrid.RowDefinitions.Clear();
            DynamicGrid.ColumnDefinitions.Clear();

            for (var i = 0; i < Rows; i++) {
                DynamicGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
            }

            for (var j = 0; j < Cols; j++) {
                DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            }
        }

        DrawGrid();
        AddTilesToGrid(Panel);
        
        stopwatch.Stop();
        Console.WriteLine($"ControlPanelView.RebuildGrid: {stopwatch.ElapsedMilliseconds}ms");
    }

    /// <summary>
    /// Given the Panel list of Entities, add each one as a tile to the panel.
    /// </summary>
    private void AddTilesToGrid(Panel? panel) {
        if (panel is null) return;
        Tiles.Clear(); 
        foreach (var entity in panel.Entities) {
            AddTileToGrid(entity);    
        }
    }

    /// <summary>
    /// Given an Entity, create a tile and add it to the panel grid. 
    /// </summary>
    /// <returns>Returns a instance of the created tile or null if it could not create one. </returns>
    private ITile? AddTileToGrid(Entity entity) {
        var tile = TileFactory.CreateTile(entity, GridSize);
        if (tile is ContentView view) {
            DynamicGrid.SetColumn(view, tile.Entity.Col);
            DynamicGrid.SetRow(view, tile.Entity.Row);
            DynamicGrid.Children.Add(view);
        
            // If this tile is an interactive tile, then add a guesture recogniser
            // so that when tapped, or double tapped, we can interact with it.
            // --------------------------------------------------------------------
            if (tile is ITileInteractive interactiveTile) {
                var primaryTapGesture = new TapGestureRecognizer();
                primaryTapGesture.Buttons = ButtonsMask.Primary;
                primaryTapGesture.Tapped += (_, _) => interactiveTile.PrimaryInteract();
                view.GestureRecognizers.Add(primaryTapGesture);
                
                var secondaryTapGesture = new TapGestureRecognizer();
                secondaryTapGesture.Buttons = ButtonsMask.Secondary;
                secondaryTapGesture.Tapped += (_, _) => interactiveTile.SecondaryInteract();
                view.GestureRecognizers.Add(secondaryTapGesture);
            }
            
            // If we are in Design mode, then add support for 
            // dragging and dropping of the items on the page
            // ---------------------------------------------------------------------------------------
            if (DesignMode) {
                var dragGesture = new DragGestureRecognizer();
                dragGesture.DragStarting += (sender, args) => DragTileStarting(args, tile);
                view.GestureRecognizers.Add(dragGesture);
            }
        }
        return tile;
    }

    private void RemoveTileFromGrid(ITile tile) {
        Panel?.Entities.Remove(tile.Entity);
        Tiles.Remove(tile);
    }

    private void SetTileGridPosition(ITile tile) {
        if (tile is ContentView view) {
            DynamicGrid.SetColumn(view, tile.Entity.Col);
            DynamicGrid.SetRow(view, tile.Entity.Row);
        }
    }
    
    // If we click on a grid that is NOT a track piece and in design mode, 
    // then clear all the selected tracks.
    // -------------------------------------------------------------------------
    private void DynamicGridTapped(object? sender, TappedEventArgs e) {
        Console.WriteLine("ControlPanelView.TapGestureRecognizer_OnTapped");
        if (DesignMode) ClearSelectedTiles();
    }

    #region Draw Grid when in Design Mode
    private void DrawGrid() {
        RemoveGrid();
        if (ShowGrid) {
            var gridLines = new GridLinesDrawable(Rows, Cols);
            var graphicsView = new GraphicsView {
                InputTransparent = true,
                Drawable = gridLines,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                ClassId = "GridLines"
            };

            // Add the GraphicsView directly to the AbsoluteLayout
            AbsoluteLayout.SetLayoutBounds(graphicsView, new Rect(0.5, 0.5, ViewWidth, ViewHeight));
            AbsoluteLayout.SetLayoutFlags(graphicsView, AbsoluteLayoutFlags.PositionProportional);
            ControlPanelLayout.Children.Add(graphicsView);
            graphicsView.Invalidate();
        }
    }
    
    /// <summary>
    ///     Draw the Grid Outline
    /// </summary>
    private void RemoveGrid() {
        if (ControlPanelLayout.Children.Count >= 1) {
            var graphicsViewToRemove = ControlPanelLayout.Children.OfType<GraphicsView>().ToList();
            foreach (var view in graphicsViewToRemove) {
                ControlPanelLayout.Children.Remove(view);
            }
        }
    }
    #endregion

    // public void RemoveTrackPiece(ITrack track) {
    //     if (Panel is { Tracks: { } tracks } panel) {
    //         if (track.Parent == panel) {
    //             track.Parent = null;
    //             MarkTrackUnSelected(track);
    //             RemoveDisplayItemFromGrid(track);
    //             Panel.Tracks.Remove(track);
    //         }
    //     }
    // }

    /// <summary>
    ///     Add the tracks from the view model onto the Grid
    /// </summary>
    // private void AddTrackPiecesToGrid() {
    //     if (Panel is { Tracks: { } tracks } panel) {
    //         foreach (var track in tracks) {
    //             if (track.Parent != Panel) track.Parent = panel;
    //
    //             if (DynamicGrid.ColumnDefinitions.Count >= Cols && DynamicGrid.RowDefinitions.Count >= Rows && track.X < Cols && track.Y < Rows) {
    //                 AddDisplayItemToGrid(track);
    //
    //                 // If we need to overlay Valid/Invalid Options. Work out the points and draw error boxes
    //                 // -------------------------------------------------------------------------------------
    //                 if (ShowTrackErrors) {
    //                     var pointImage = new TrackPoints { X = track.X, Y = track.Y };
    //                     var validPoints = TrackPointsValidator.GetConnectedTracksStatus(tracks, track, panel.Cols, panel.Rows);
    //                     pointImage.SetPoints(validPoints);
    //                     AddDisplayItemToGrid(pointImage);
    //                 }
    //
    //                 if (track.IsSelected) MarkTrackSelected(track);
    //             }
    //         }
    //     }
    // }

    // public void InvalidateCell(ITrack track) {
    //     // TODO: We should not need to do this if the image has not actually 
    //     //       changed. This is slow...
    //     track.InvalidateView();
    //             
    //     //RemoveDisplayItemFromGrid(track);
    //     //AddDisplayItemToGrid(track);
    // }

    // private void RemoveDisplayItemFromGrid(ITrack track) {
    //     Console.WriteLine($"Looking to Remove Track: {track.UniqueID}");
    //     var tracks = DynamicGrid.Children.Where(child => (child as Microsoft.Maui.Controls.View)?.ClassId == track.UniqueID.ToString()).ToList();
    //     foreach (var view in tracks) {
    //         Console.WriteLine($"Removing Track: {track.UniqueID}");
    //         DynamicGrid.Children.Remove(view);
    //         track.PropertyChanged -= OnTrackPieceChanged;
    //     }
    // }

    // private void AddDisplayItemToGrid(ITrack track) {
    //     // If we are in DesignMode, ensure transparency mode is OFF otherwise
    //     // allow the system to use the passthrough/transparency mode of the object. 
    //     // -----------------------------------------------------------------------
    //     var displayItem = track.TrackView(GridSize, DesignMode ? false : null);
    //     track.PropertyChanged += OnTrackPieceChanged;
    //
    //     Console.WriteLine($"Adding Track: {track.UniqueID}");
    //
    //     // Setup trigger control to trap if we click on or select the track item
    //     // -------------------------------------------------------------------------------------------
    //     // Create TapGestureRecognizer
    //     if (displayItem is Microsoft.Maui.Controls.View view) {
    //         view.ClassId = track.UniqueID.ToString();
    //         var tapGesture = new TapGestureRecognizer {
    //             NumberOfTapsRequired = 1
    //         };
    //         tapGesture.Tapped += (_, _) => TrackPieceTapped?.Invoke(this, track);
    //         view.GestureRecognizers.Add(tapGesture);
    //
    //         var doubleTapGesture = new TapGestureRecognizer {
    //             NumberOfTapsRequired = 2
    //         };
    //         doubleTapGesture.Tapped += (sender, args) => TrackPieceDoubleTapped?.Invoke(this, track);
    //         view.GestureRecognizers.Add(doubleTapGesture);
    //
    //         // If we are in Design mode, then add support for 
    //         // dragging and dropping of the items on the page
    //         // ---------------------------------------------------------------------------------------
    //         if (DesignMode) {
    //             var dragGesture = new DragGestureRecognizer();
    //             dragGesture.DragStarting += (sender, args) => DragTrackStarting(args, track);
    //             view.GestureRecognizers.Add(dragGesture);
    //         }
    //     } else {
    //         Console.WriteLine("ERROR: DisplayItem is not a View but is of type: " + displayItem.GetType().Name);
    //     }
    //
    //     // Add the Track DisplayImage to the appropriate grid position
    //     // ------------------------------------------------------
    //     DynamicGrid.SetRow(displayItem, track.Y);
    //     DynamicGrid.SetColumn(displayItem, track.X);
    //     DynamicGrid.Children.Add(displayItem);
    // }
    
    #region Support Marking and UnMarking Tiles on the Panel
    public void MarkTileSelected(ITile tile) {
        HighlightCell(tile.Entity.Col, tile.Entity.Row, tile.Entity.Width, tile.Entity.Height, CellHighlightAction.Selected);
        tile.IsSelected = true;
    }
    
    public void MarkTileUnSelected(ITile tile) {
        UnHighlightCell(tile.Entity.Col, tile.Entity.Row);
        tile.IsSelected = false;
    }

    public void ClearSelectedTiles() {
        if (Panel is not null) {
            foreach (var tile in Tiles.Where(x => x.IsSelected)) MarkTileUnSelected(tile);
        }
    }
    
    /// <summary>
    ///     Only highlight a cell if we are in Design Mode
    /// </summary>
    public void HighlightCell(int col, int row, int width, int height, CellHighlightAction action) {
        if (!DesignMode) return;

        var border = new Border {
            ClassId = "CellHighlight",
            BackgroundColor = Colors.Transparent,
            Stroke = action switch {
                CellHighlightAction.Selected    => Colors.Blue,
                CellHighlightAction.DragValid   => Colors.Green,
                CellHighlightAction.DragInvalid => Colors.Red,
                _                               => Colors.Red
            },
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Start,
            WidthRequest = width * GridSize,
            HeightRequest = height * GridSize,
            StrokeThickness = 4,
            Opacity = 0.5,
            ZIndex = 10,
            InputTransparent = true
        };

        // Add the Track DisplayImage to the appropriate grid position
        // ------------------------------------------------------
        DynamicGrid.SetRow(border, row);
        DynamicGrid.SetColumn(border, col);
        DynamicGrid.Children.Add(border);
    }

    /// <summary>
    ///     Only UnHighlight a cell if we are operating in Design mode
    ///     If we are in Operate mode, then we do not highlight cells so this has no function.
    /// </summary>
    public void UnHighlightCell(int col, int row) {
        if (!DesignMode) return;
        var children = DynamicGrid.Children.Where(x => x is Border border && x.Parent is Grid && border.ClassId == "CellHighlight").ToList();
        foreach (var child in children) {
            if (DynamicGrid.GetRow(child) == row && DynamicGrid.GetColumn(child) == col) {
                DynamicGrid.Remove(child);
            }
        }
    }
    #endregion
    
    #region Drag and Drop Support for the Tiles
    
    /// <summary>
    /// If we have started a Drag and it is a Tile then store the tile we are dragging in the drag properties
    /// and also that the source is a Tile. We use this along with the mode to either move the tile or add
    /// a new tile to the panel. 
    /// </summary>
    private void DragTileStarting(DragStartingEventArgs args, ITile tile) {
        args.Data.Properties.Add("Tile", tile);
        args.Data.Properties.Add("Source", "Panel");
        _lastDragCol = 0;
        _lastDragRow = 0;

        #if IOS || MACCATALYST
        UIDragPreview Action() {
            var image = UIImage.FromFile("move.png");
            var imageView = new UIImageView(image);
            imageView.ContentMode = UIViewContentMode.Center;
            imageView.Frame = new CGRect(0, 0, 0.5, 0.5);
            return new UIDragPreview(imageView);
        }
        args?.PlatformArgs?.SetPreviewProvider(Action);
        #endif
    }

    /// <summary>
    /// Called when we have left the bounds of thr Panel so we just reset everything
    /// </summary>
    private void DragLeaveTileOnPanel(object? sender, DragEventArgs e) {
        UnHighlightCell(_lastDragCol, _lastDragRow);
        _lastDragCol = 0;
        _lastDragRow = 0;
    }

    /// <summary>
    /// Called when we are dragging a tile on the panel surface. Works out if it is a valid drop zone
    /// or if it would clash with something else. For example, you cannot have a track on another track but
    /// you could have a non-track on top of a track or a track on top of an image. 
    /// </summary>
    private void DragOverTileOnPanel(object? sender, DragEventArgs e) {
        var tile = e.Data.Properties["Tile"] as ITile ?? null;
        var gridPosition = GetGridPosition(e.GetPosition(DynamicGrid));
        
        if (gridPosition is {} position && tile is not null) { 
            if (EditMode == EditModeEnum.Size) {
                //ResizeTrack(track, position.Col, position.Row);
            } else {
                if (_lastDragCol != position.Col || _lastDragRow != position.Row) {
                    UnHighlightCell(_lastDragCol, _lastDragRow);
                }

                if (!DoesTrackClash(tile, position.Col, position.Row)) {
                    e.AcceptedOperation = DataPackageOperation.Copy;
                    HighlightCell(position.Col, position.Row, tile.Entity.Width, tile.Entity.Height, CellHighlightAction.DragValid);
                } else {
                    e.AcceptedOperation = DataPackageOperation.None;
                    HighlightCell(position.Col, position.Row, tile.Entity.Width, tile.Entity.Height, CellHighlightAction.DragInvalid);
                }
                _lastDragCol = position.Col;
                _lastDragRow = position.Row;
            }
        } else {
            UnHighlightCell(_lastDragCol, _lastDragRow);
            _lastDragCol = 0;
            _lastDragRow = 0;
        }

        #if IOS || MACCATALYST
        e?.PlatformArgs?.SetDropProposal(EditMode switch {
            EditModeEnum.Copy => new UIDropProposal(UIDropOperation.Copy),
            EditModeEnum.Move => new UIDropProposal(UIDropOperation.Move),
            _                 => new UIDropProposal(UIDropOperation.Forbidden),
            });
        #endif

        #if WINDOWS
        var dragUI = e.PlatformArgs.DragEventArgs.DragUIOverride;
        dragUI.IsCaptionVisible = false;
        dragUI.IsGlyphVisible = false;
        #endif
    }

    /// <summary>
    /// Support dropping the dragged tile onto the panel in a new position (or the same position) 
    /// </summary>
    private void DropTileOnPanel(object? sender, DropEventArgs e) {
        try {
            if (!e.Data.Properties.ContainsKey("Source") ||
                !e.Data.Properties.ContainsKey("Tile")) {
                Console.WriteLine("Invalid Drop Properties.");
                return;
            }

            UnHighlightCell(_lastDragCol, _lastDragRow);
            var source = e.Data.Properties["Source"] as string ?? null;
            var tile = e.Data.Properties["Tile"] as ITile ?? null;
            var gridPosition = GetGridPosition(e?.GetPosition(DynamicGrid));

            if (gridPosition is { } position && tile is not null) {
                // Make sure that the item we are placing is onto a point that is 
                // not already occupied unless the item being dropped is an overlay 
                // item that has a higher Z factor. 
                // -----------------------------------------------------------------
                if (!DoesTrackClash(tile, position.Col, position.Row)) {
                    ClearSelectedTiles();

                    if (Panel is { } panel) {
                        switch (source) {
                        case "Panel":
                            switch (EditMode) {
                            case EditModeEnum.Move:
                                tile.Entity.Col = position.Col;
                                tile.Entity.Row = position.Row;
                                SetTileGridPosition(tile);
                                break;

                            case EditModeEnum.Copy:
                                var newEntity = panel.CreateEntityFrom(tile.Entity);
                                newEntity.Col = position.Col;
                                newEntity.Row = position.Row;
                                var newTile = AddTileToGrid(newEntity);
                                if (newTile is not null) MarkTileSelected(newTile);
                                break;

                            case EditModeEnum.Size:
                                break;
                            }
                            break;

                        case "DisplaySymbol":
                            //if (Panel is not null && trackPiece.Clone(Panel) is { } newPiece) {
                            //    newPiece.X = position.Col;
                            //    newPiece.Y = position.Row;
                            //    Panel?.AddTrack(newPiece);
                            //    AddDisplayItemToGrid(newPiece);
                            //    MarkTileSelected(newPiece);
                            //}

                            break;

                        default:
                            Debug.WriteLine($"ERROR: Invalid source: '{source}'");
                            break;
                        }
                    } 
                } else {
                    Debug.WriteLine("ERROR: Item clashes with existing track");
                }
            } else {
                Debug.WriteLine("ERROR: Invalid grid position");
            }
        } catch (Exception ex) {
            Debug.WriteLine("ERROR: Error dropping item: " + ex.Message);
        }

        _lastDragCol = 0;
        _lastDragRow = 0;
    }

    private bool DoesTrackClash(ITile tile, int col, int row) {
        if (Tiles.Count == 0) return false;                 // No clashes possible if no tiles are present
        if (tile.Entity is not ITrackEntity) return false;  // No clashes possible if the track is not a track piece
        var tilesInGrid = Tiles.Where(eTile =>              
                // Exclude the same track we're checking against
                eTile != tile && eTile.Entity is ITrackEntity &&

                // Check if there's a column overlap between the tracks
                col < eTile.Entity.Col + eTile.Entity.Width && col + eTile.Entity.Width > eTile.Entity.Col &&

                // Check if there's a row overlap between the tracks
                row < eTile.Entity.Row + eTile.Entity.Height && row + eTile.Entity.Height > eTile.Entity.Row
        );

        // If there are any tracks in the clashing list, return true
        return tilesInGrid.Any();
    }
    #endregion
    
//
//     private void ResizeTrack(ITrack? track, int newCol, int newRow) {
//         if (track is null) return;
//
//         // Original position and size
//         var originalX = track.X;
//         var originalY = track.Y;
//         var originalWidth = track.Width;
//         var originalHeight = track.Height;
//
//         // Resizing right (increasing width)
//         if (newCol > originalX) {
//             track.Width = newCol - originalX + 1; // +1 to include the new column
//         }
//
//         // Resizing left (shifting X and adjusting width)
//         else if (newCol < originalX) {
//             var deltaX = originalX - newCol;
//             track.X -= deltaX;     // Shift left
//             track.Width += deltaX; // Increase width
//         }
//
//         // Resizing down (increasing height)
//         if (newRow > originalY) {
//             track.Height = newRow - originalY + 1; // +1 to include the new row
//         }
//
//         // Resizing up (shifting Y and adjusting height)
//         else if (newRow < originalY) {
//             var deltaY = originalY - newRow;
//             track.Y -= deltaY;      // Shift up
//             track.Height += deltaY; // Increase height
//         }
//
//         // Ensure minimum size limits
//         track.Width = Math.Max(1, track.Width);
//         track.Height = Math.Max(1, track.Height);
//
//         // Refresh Grid and Update
//         InvalidateCell(track); // Re-render the grid for the resized component
//     }

    /// <summary>
    ///     Convert a position in the grid (absolute) to a Grid position within the col/row definitions
    /// </summary>
    /// <param name="point">A point object of where the item was tapped</param>
    /// <returns>Either a null, or (-1,-1) or (row,col) </returns>
    private (int Col, int Row)? GetGridPosition(Point? point) {
        if (point is { } tapPosition) {
            var totalHeight = DynamicGrid.Height;
            var totalWidth = DynamicGrid.Width;
            var rowCount = DynamicGrid.RowDefinitions.Count;
            var colCount = DynamicGrid.ColumnDefinitions.Count;

            var cellHeight = totalHeight / rowCount;
            var cellWidth = totalWidth / colCount;

            if (cellHeight == 0 || cellWidth == 0) {
                Console.WriteLine($"Cell Height or Width is zero? {cellHeight},{cellWidth}");
                return null;
            }

            // Calculate row and column indices
            var row = (int)(tapPosition.Y / cellHeight);
            var col = (int)(tapPosition.X / cellWidth);

            // Ensure indices are within bounds
            row = Math.Min(row, rowCount - 1);
            col = Math.Min(col, colCount - 1);

            return (col, row);
        }
        Console.WriteLine($"Could not determine the Grid Position from the point provided: {point.ToString()}");
        return null;
    }
}

public enum EditModeEnum {
    Move,
    Copy,
    Size
}