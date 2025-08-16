using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Interfaces; // ITileInteractive
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.Tiles; // TileFactory
using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace DCCPanelController.View;

public record EntityTappedEventArgs(Entity Entity, int TapCount);

public record EntitySelectedEventArgs(IEnumerable<Entity> Entities, int TapCount);

public partial class PanelSurfaceView : ContentView {
    // Bindables
    // --------------------------------------------------------------------------------------------
    public static readonly BindableProperty PanelProperty = BindableProperty.Create(nameof(Panel), typeof(Panel), typeof(PanelSurfaceView), propertyChanged: (b, o, n) => ((PanelSurfaceView)b).OnPanelChanged((Panel?)o, (Panel?)n));
    public static readonly BindableProperty ShowGridProperty = BindableProperty.Create(nameof(ShowGrid), typeof(bool), typeof(PanelSurfaceView), true, propertyChanged: (b, o, n) => ((PanelSurfaceView)b).Invalidate());
    public static readonly BindableProperty GridColorProperty = BindableProperty.Create(nameof(GridColor), typeof(Color), typeof(PanelSurfaceView), Colors.DarkGray, propertyChanged: (b, o, n) => ((PanelSurfaceView)b).Invalidate());
    public static readonly BindableProperty GridThicknessProperty = BindableProperty.Create(nameof(GridThickness), typeof(float), typeof(PanelSurfaceView), 1.0f, propertyChanged: (b, o, n) => ((PanelSurfaceView)b).Invalidate());
    public static readonly BindableProperty InteractionModeProperty = BindableProperty.Create(nameof(InteractionMode), typeof(Mode), typeof(PanelSurfaceView), Mode.Edit);
    public static readonly BindableProperty EditToolProperty = BindableProperty.Create(nameof(EditTool), typeof(Tool), typeof(PanelSurfaceView), Tool.Move);

    // Public Propeties for Bindables
    // --------------------------------------------------------------------------------------------
    public Panel? Panel {
        get => (Panel?)GetValue(PanelProperty);
        set => SetValue(PanelProperty, value);
    }

    public bool ShowGrid {
        get => (bool)GetValue(ShowGridProperty);
        set => SetValue(ShowGridProperty, value);
    }

    public Color GridColor {
        get => (Color)GetValue(GridColorProperty);
        set => SetValue(GridColorProperty, value);
    }

    public float GridThickness {
        get => (float)GetValue(GridThicknessProperty);
        set => SetValue(GridThicknessProperty, value);
    }

    public Mode InteractionMode {
        get => (Mode)GetValue(InteractionModeProperty);
        set => SetValue(InteractionModeProperty, value);
    }

    public Tool EditTool {
        get => (Tool)GetValue(EditToolProperty);
        set => SetValue(EditToolProperty, value);
    }

    public enum Mode { Edit, Run }

    public enum Tool { Move, Copy, Size }

    // Events
    // --------------------------------------------------------------------------------------------
    public event EventHandler<EntityTappedEventArgs>? TileTapped; // single/double-tap
    public event EventHandler<EntitySelectedEventArgs>? TileSelected;
    public event EventHandler<EntitySelectedEventArgs>? TileChanged; // move/size/clone

    // Zoom / Pan
    // --------------------------------------------------------------------------------------------
    private float _zoom = 1f;             // world->screen scale
    private SKPoint _pan = SKPoint.Empty; // world->screen translate (in screen px)
    private const float ZoomMin = 0.25f;
    private const float ZoomMax = 8f;

    // Grid sizing: base cell size in screen px at zoom=1
    // --------------------------------------------------------------------------------------------
    private float _cellPx = 0f;
    private float CellPx => _cellPx > 0 ? _cellPx : 24f; // default until measured

    // Selection & drag
    // --------------------------------------------------------------------------------------------
    private Entity? _pressedEntity;
    private SKPoint _pressWorld;
    private (int Col, int Row) _resizeAnchor;

    // Renderer plug-ins
    // --------------------------------------------------------------------------------------------
    private readonly List<ISurfaceEntityRenderer> _renderers = new();
    public void RegisterRenderer(ISurfaceEntityRenderer renderer) => _renderers.Insert(0, renderer);

    public PanelSurfaceView() {
        InitializeComponent();

        // TODO: Default renderers you can extend/replace (Track renderer added separately)
        _renderers.Add(new CircleRenderer());

        SizeChanged += (_, __) => Invalidate();
    }

    private void OnPanelChanged(Panel? oldPanel, Panel? newPanel) {
        if (oldPanel?.Entities is { } oldEntities) oldEntities.CollectionChanged -= OnEntitiesChanged;
        if (newPanel?.Entities is { } newEntities) newEntities.CollectionChanged += OnEntitiesChanged;
        ZoomToFit();
        Invalidate();
    }

    private void OnEntitiesChanged(object? sender, NotifyCollectionChangedEventArgs e) => Invalidate();

    // ===== Rendering =====
    // --------------------------------------------------------------------------------------------
    private void Invalidate() => Surface?.InvalidateSurface();

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e) {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        if (Panel is not { } panel) return;

        // Compute cell size to fit content (square cells)
        var worldCols = Math.Max(1, panel.Cols);
        var worldRows = Math.Max(1, panel.Rows);
        var screen = e.Info; // px
        var cellX = (float)screen.Width / worldCols;
        var cellY = (float)screen.Height / worldRows;
        _cellPx = MathF.Floor(MathF.Min(cellX, cellY));

        // World->screen transform (draw in world units where 1 unit = 1 cell)
        var m = SKMatrix.CreateScale(_zoom * CellPx, _zoom * CellPx);
        m = SKMatrix.Concat(SKMatrix.CreateTranslation(_pan.X, _pan.Y), m);
        canvas.SetMatrix(m);

        // Draw grid (in world units)
        if (ShowGrid) {
            using var p = new SKPaint { Color = GridColor.ToSKColor(), StrokeWidth = GridThickness / _zoom, IsStroke = true, IsAntialias = true };
            for (var c = 0; c <= worldCols; c++) canvas.DrawLine(c, 0, c, worldRows, p);
            for (var r = 0; r <= worldRows; r++) canvas.DrawLine(0, r, worldCols, r, p);
        }

        // Draw entities sorted by layer
        foreach (var entity in panel.Entities.OrderBy(e2 => e2.Layer)) {
            var rect = new SKRect(entity.Col, entity.Row, entity.Col + entity.Width, entity.Row + entity.Height);

            // Find a renderer that can draw this entity
            // TODO: Must be a quicker way via a dictionary?
            var handled = false;
            foreach (var r in _renderers) {
                if (r.CanRender(entity)) {
                    r.Draw(canvas, entity, rect, _zoom);
                    handled = true;
                    break;
                }
            }

            // If we could not find a renderer to draw this Entity, draw a default one??
            // -------------------------------------------------------------------------
            if (!handled) {
                using var fill = new SKPaint();
                fill.Color = new SKColor(0, 0, 0, 20);

                using var stroke = new SKPaint();
                stroke.Color = SKColors.DimGray;
                stroke.IsStroke = true;
                stroke.StrokeWidth = 1 / _zoom;
                canvas.DrawRect(rect, fill);
                canvas.DrawRect(rect, stroke);

                using var text = new SKPaint();
                text.Color = SKColors.DimGray;
                text.TextSize = 0.35f;
                text.IsAntialias = true;
                text.TextAlign = SKTextAlign.Center;

                canvas.DrawText(entity.Type, rect.MidX, rect.MidY, text);
            }
        }

        // Selection highlight (edit mode)
        if (InteractionMode == Mode.Edit && _pressedEntity is not null) {
            using var sel = new SKPaint { Color = new SKColor(100, 149, 237, 64), IsStroke = false };
            var r = new SKRect(_pressedEntity.Col, _pressedEntity.Row, _pressedEntity.Col + _pressedEntity.Width, _pressedEntity.Row + _pressedEntity.Height);
            canvas.DrawRect(r, sel);
        }
    }

    // Gestures
    // --------------------------------------------------------------------------------
    private void OnPan(object? sender, PanUpdatedEventArgs e) {
        if (e.StatusType == GestureStatus.Running) {
            _pan.X += (float)e.TotalX;
            _pan.Y += (float)e.TotalY;
            Invalidate();
        }
    }

    private void OnPinch(object? sender, PinchGestureUpdatedEventArgs e) {
        if (e.Status == GestureStatus.Running) {
            var newZoom = Math.Clamp(_zoom * (float)e.Scale, ZoomMin, ZoomMax);
            _zoom = newZoom;
            Invalidate();
        }
    }

    private void HandleTap(TappedEventArgs e, int tapCount, bool useSecondary) {
        var world = ScreenToWorld(e.GetPosition(GestureOverlay));
        var entity = HitTestTopMost(world);
        if (entity is null) return;

        if (InteractionMode == Mode.Run) {
            if (_behavior.TryGetInteractive(entity, out var interactive)) {
                _ = useSecondary ? interactive.Secondary(null) : interactive.Interact(null);
            }
        }
        else {
            _pressedEntity = entity;
            TileSelected?.Invoke(this, new EntitySelectedEventArgs([entity], tapCount));
            Invalidate();
        }
        TileTapped?.Invoke(this, new EntityTappedEventArgs(entity, tapCount));
    }

    private void OnTapped(object? sender, TappedEventArgs e) => HandleTap(e, tapCount: 1, useSecondary: false);
    private void OnDoubleTapped(object? sender, TappedEventArgs e) => HandleTap(e, tapCount: 2, useSecondary: true);
    
    // Drag & Drop (palette or in-canvas drag)
    // --------------------------------------------------------------------------------
    private void OnDragOver(object? sender, DragEventArgs e) {
        e.AcceptedOperation = DataPackageOperation.Copy;
    }

    private void OnDrop(object? sender, DropEventArgs e) {
        if (Panel is null) return;
        
        var pt = e.GetPosition(GestureOverlay);
        var world = ScreenToWorld(pt);
        var col = Math.Clamp((int)Math.Floor(world.X), 0, Panel.Cols - 1);
        var row = Math.Clamp((int)Math.Floor(world.Y), 0, Panel.Rows - 1);

        if (e.Data.Properties.TryGetValue("Tile", out var tileObj) && tileObj is ITile tile) {
            var newEntity = tile.Entity.Clone();
            newEntity.Parent = Panel;
            newEntity.Col = col;
            newEntity.Row = row;
            Panel.Entities.Add(newEntity);
            TileChanged?.Invoke(this, new EntitySelectedEventArgs(new[] { newEntity }, 0));
            return;
        }

        if (e.Data.Properties.TryGetValue("Entity", out var entObj) && entObj is Entity entity) {
            var newEntity = entity.Clone();
            newEntity.Parent = Panel;
            newEntity.Col = col;
            newEntity.Row = row;
            Panel.Entities.Add(newEntity);
            TileChanged?.Invoke(this, new EntitySelectedEventArgs(new[] { newEntity }, 0));
        }
    }

    // Commands
    // --------------------------------------------------------------------------------
    public void ZoomToFit() {
        if (Panel is null || Surface == null) return;
        
        _zoom = 1f;
        _pan = SKPoint.Empty;
        Invalidate();
    }

    public void ZoomIn(float factor = 1.25f) {
        _zoom = Math.Clamp(_zoom * factor, ZoomMin, ZoomMax);
        Invalidate();
    }

    public void ZoomOut(float factor = 0.8f) {
        _zoom = Math.Clamp(_zoom * factor, ZoomMin, ZoomMax);
        Invalidate();
    }

    // Helpers
    // --------------------------------------------------------------------------------
    private SKPoint ScreenToWorld(Point? p) {
        if (p is null) return SKPoint.Empty;
        var px = (float)p.Value.X;
        var py = (float)p.Value.Y;
        var invScale = 1f / (_zoom * CellPx);
        var x = (px - _pan.X) * invScale;
        var y = (py - _pan.Y) * invScale;
        return new SKPoint(x, y);
    }

    private Entity? HitTestTopMost(SKPoint world) {
        if (Panel is null) return null;
        var c = (int)Math.Floor(world.X);
        var r = (int)Math.Floor(world.Y);
        return Panel.Entities
                    .Where(e => e.Col <= c && c < e.Col + e.Width && e.Row <= r && r < e.Row + e.Height)
                    .OrderByDescending(e => e.Layer)
                    .FirstOrDefault();
    }

    // Nested interface for renderers
    public interface ISurfaceEntityRenderer {
        bool CanRender(Entity e);
        void Draw(SKCanvas canvas, Entity e, SKRect worldRect, float zoom);
    }

    // Example primitive renderer (Circle)
    sealed class CircleRenderer : ISurfaceEntityRenderer {
        public bool CanRender(Entity e) => e.GetType().Name.Contains("Circle", StringComparison.OrdinalIgnoreCase);

        public void Draw(SKCanvas canvas, Entity e, SKRect rect, float zoom) {
            using var fill = new SKPaint { Color = new SKColor(0, 0, 0, (byte)(e.Opacity * 255)), IsAntialias = true, IsStroke = false };
            using var stroke = new SKPaint { Color = SKColors.Black, IsAntialias = true, IsStroke = true, StrokeWidth = 1 / zoom };
            var cx = rect.MidX;
            var cy = rect.MidY;
            var radius = Math.Min(rect.Width, rect.Height) * 0.5f - 0.05f;
            canvas.DrawCircle(cx, cy, radius, fill);
            canvas.DrawCircle(cx, cy, radius, stroke);
        }
    }

    // Behavior adapter: invoke tile actions in Run mode without adding UI views
    readonly TileBehaviorAdapter _behavior = new();

    sealed class TileBehaviorAdapter {
        public bool TryGetInteractive(Entity entity, out ITileInteractive interactive) {
            interactive = null!;
            try {
                var tile = TileFactory.CreateTile(entity, gridSize: 8, TileDisplayMode.Normal);
                if (tile is ITileInteractive it) {
                    interactive = it;
                    return true;
                }
                return false;
            } catch {
                return false;
            }
        }
    }
}