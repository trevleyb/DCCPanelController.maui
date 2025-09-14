using DCCPanelController.Models.DataModel.Entities.Interfaces;

// ReSharper disable once CheckNamespace
namespace DCCPanelController.Models.DataModel.Entities;

public static class EntityPresets {
    public const int Default   = 0;
    public const int Track     = 5;
    public const int Drawing   = 1;
    public const int Button    = 10;
    public const int Label     = 15;
    public const int Highlight = 99;

    public static int DefaultLayer(Entity entity) => entity switch {
        ITrackEntity        => Track,
        ActionButtonEntity  => Button,
        TurnoutButtonEntity => Button,
        ITextEntity         => Label,
        IDrawingEntity      => Drawing,
        _                   => Default,
    };
}