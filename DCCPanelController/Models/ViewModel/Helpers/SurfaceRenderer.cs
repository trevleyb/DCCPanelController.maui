using System;
using System.Collections.Generic;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.Models.ViewModel.Helpers;      // TileFactory
using DCCPanelController.Models.ViewModel.ImageManager; // SvgImageManager
using DCCPanelController.Models.ViewModel.StyleManager; // SvgStyleBuilder, SvgElementType
using DCCPanelController.Models.ViewModel.Tiles;        // TrackTile, TileDisplayMode
using Microsoft.Maui.Graphics;
using SkiaSharp;

namespace DCCPanelController.View;

/// <summary>
/// Renders Track and Turnout entities directly to the shared SKCanvas surface.
/// - Resolves the correct SVG and rotation by instantiating the tile off-tree via TileFactory
/// - Applies the same style rules used by TrackTile/TurnoutTile
/// - Draws using SvgImageManager helpers (no reflection)
/// </summary>
public sealed class TrackAndTurnoutSurfaceRenderer : PanelSurfaceView.ISurfaceEntityRenderer {
    public bool CanRender(Entity e) {
        var name = e.GetType().Name;
        return name.Contains("Track", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Turnout", StringComparison.OrdinalIgnoreCase);
    }

    public void Draw(SKCanvas canvas, Entity e, SKRect worldRect, float zoom) {
        // Resolve image & rotation using the existing tile (off-tree)
        string? imageName = null;
        int rotation = e.Rotation;

        var tile = TileFactory.CreateTile(e, gridSize: 8, displayMode: TileDisplayMode.Normal);
        if (tile is TrackTile { SvgImage: not null } tt) {
            imageName = tt.SvgImage.Filename;
            rotation = tt.SvgImage.Rotation;
        }

        if (string.IsNullOrWhiteSpace(imageName)) {
            using var stroke = new SKPaint();
            stroke.Color = SKColors.OrangeRed;
            stroke.IsStroke = true;
            stroke.StrokeWidth = 2f / MathF.Max(0.001f, zoom);
            canvas.DrawRect(worldRect, stroke);
            return;
        }

        // Build style and apply to an SvgImageManager
        var style = BuildStyle(e);
        var mgr = new SvgImageManager(imageName);
        ApplyStyleToManager(mgr, style);

        // Draw
        mgr.Draw(canvas, worldRect, rotation);
    }

    // ---- Style mapping ----
    private static SvgStyleBuilder BuildStyle(Entity e) {
        var style = new SvgStyleBuilder();

        // Track styling
        if (e is TrackEntity track) {
            switch (track.TrackType) {
            case TrackTypeEnum.BranchLine:
                style.Add(s => s.WithName(SvgElementType.Border).Hidden())
                     .Add(s => s.WithName(SvgElementType.BorderDiverging).Hidden())
                     .Add(s => s.WithName(SvgElementType.Track).WithColor(track.TrackColor ?? e.Parent?.BranchLineColor ?? Colors.Gray).Visible());
                break;

            case TrackTypeEnum.MainLine:
            default:
                style.Add(s => s.WithName(SvgElementType.Border).WithColor(track.TrackBorderColor ?? e.Parent?.MainlineBorderColor ?? Colors.Black).Visible())
                     .Add(s => s.WithName(SvgElementType.BorderDiverging).WithColor(track.TrackBorderColor ?? e.Parent?.MainlineBorderColor ?? Colors.Black).Visible())
                     .Add(s => s.WithName(SvgElementType.Track).WithColor(track.TrackColor ?? e.Parent?.MainLineColor ?? Colors.Black).Visible());
                break;
            }

            switch (track.TrackAttribute) {
            case TrackAttributeEnum.Dashed:
                style.Add(s => s.WithName(SvgElementType.Dashline).WithColor(e.Parent?.HiddenColor ?? Colors.White).Visible());
                break;

            case TrackAttributeEnum.Normal:
            default:
                style.Add(s => s.WithName(SvgElementType.Dashline).Hidden());
                break;
            }
        }

        // Turnout extras layered on top
        if (e is TurnoutEntity turnout) {
            style.Add(s => s.WithName(SvgElementType.TrackDiverging)
                            .WithColor(turnout.TrackNotSelectedColor ?? turnout.Parent?.DivergingColor ?? Colors.Gray));

            var neighbor = turnout.GetDivergingEntity() as TrackEntity;
            if (turnout.IsMainLine && neighbor?.IsBranchLine == true && turnout.State == TurnoutStateEnum.Closed) {
                style.Add(s => s.WithName(SvgElementType.BorderDiverging).Hidden());
                style.Add(s => s.WithName(SvgElementType.TrackDiverging)
                                .WithColor(neighbor.TrackColor ?? neighbor.Parent?.BranchLineColor ?? Colors.Gray));
            }
        }

        return style;
    }

    /// <summary>
    /// Applies a SvgStyle (from SvgStyleBuilder) to the manager by mapping common attributes.
    /// </summary>
    private static void ApplyStyleToManager(SvgImageManager mgr, SvgStyleBuilder builder) {
        var style = builder.Build();
        foreach (var kvp in style.Elements) {
            var elementName = kvp.Key;
            foreach (var attr in kvp.Value.Attributes) {
                var key = attr.Key.Trim().ToLowerInvariant();
                var val = attr.Value;

                switch (key) {
                case "color":
                    mgr.SetAllAttributeValues(elementName, "fill", val);
                    mgr.SetAllAttributeValues(elementName, "stroke", val);
                    break;

                case "opacity":
                    mgr.SetAllAttributeValues(elementName, "fill-opacity", val);
                    mgr.SetAllAttributeValues(elementName, "stroke-opacity", val);
                    break;

                case "dashed":
                    mgr.SetAllAttributeValues(elementName, "stroke-dasharray", (val?.ToLowerInvariant() == "true") ? "2,6" : "0,0");
                    break;

                case "visible":
                    var on = val?.ToLowerInvariant() == "true";
                    mgr.SetAllAttributeValues(elementName, "fill-opacity", on ? "100" : "0");
                    mgr.SetAllAttributeValues(elementName, "stroke-opacity", on ? "100" : "0");
                    break;

                case "text":
                    mgr.SetAllAttributeValues(elementName, "text", val);
                    break;

                default:
                    mgr.SetAllAttributeValues(elementName, key, val);
                    break;
                }
            }
        }
    }
}