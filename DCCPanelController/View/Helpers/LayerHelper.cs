using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.View.Helpers;

public static class LayerCyclerByLocation {
    public static void CycleLayersAtSelectedLocations(IList<Entity>? selected, IEnumerable<Entity>? allEntities) {
        if (selected is null || allEntities is null || selected.Count == 0) return;

        var processed = new HashSet<(int col, int row)>();
        var panelEntities = allEntities.ToList();
        foreach (var s in selected) {
            var loc = (s.Col, s.Row);
            if (!processed.Add(loc)) continue; // don't rotate the same spot twice

            var pile = panelEntities.Where(e => ContainsCell(e, loc.Col, loc.Row)).ToList();
            if (pile.Count < 2) continue;

            EnsureUniqueAscendingLayers(pile);
            RotateLowestToTop(pile);
        }
    }

    private static bool ContainsCell(Entity e, int col, int row) {
        var w = Math.Max(1, e.Width);
        var h = Math.Max(1, e.Height);
        return col >= e.Col && col <= e.Col + w - 1 &&
               row >= e.Row && row <= e.Row + h - 1;
    }

    /// <summary>
    /// Make all layers in the pile strictly increasing, preserving order by (Layer, Guid).
    /// Only adjusts within this pile; other locations are unaffected.
    /// </summary>
    private static void EnsureUniqueAscendingLayers(List<Entity> pile) {
        var ordered = pile.OrderBy(e => e.Layer).ThenBy(e => e.Guid).ToList();
        var next = ordered[0].Layer;
        ordered[0].Layer = next;
        for (var i = 1; i < ordered.Count; i++) {
            var current = ordered[i].Layer;
            if (current <= next) current = next + 1;
            ordered[i].Layer = current;
            next = current;
        }
    }

    private static void RotateLowestToTop(List<Entity> pile) {
        var ordered = pile.OrderBy(e => e.Layer).ThenBy(e => e.Guid).ToList();
        var layers = ordered.Select(e => e.Layer).ToArray();
        if (layers.Length < 2) return;

        var lowest = layers[0];
        for (var i = 0; i < layers.Length - 1; i++) layers[i] = layers[i + 1];
        layers[^1] = lowest;
        for (var i = 0; i < ordered.Count; i++) ordered[i].Layer = layers[i];
    }
}