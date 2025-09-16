using DCCPanelController.Models.DataModel.Entities.Interfaces;

// ReSharper disable once CheckNamespace
namespace DCCPanelController.Models.DataModel.Entities;

public static class EntityPresets {
    public const int Default   = 0;
    public const int Drawing   = 1;
    public const int Track     = 2;
    public const int Button    = 3;
    public const int Label     = 4;
    public const int Highlight = 99;

    public static int DefaultLayer(Entity entity) => entity switch {
        ITrackEntity        => Track,
        ITextEntity         => Label,
        IDrawingEntity      => Drawing,
        ActionButtonEntity  => Button,
        TurnoutButtonEntity => Button,
        _                   => Default,
    };
}