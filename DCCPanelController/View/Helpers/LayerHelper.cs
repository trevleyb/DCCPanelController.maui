using DCCPanelController.Models.DataModel.Entities;

namespace DCCPanelController.View.Helpers;

public static class LayerCyclerByLocation
{
    /// <summary>
    /// Rotates z-order for ALL entities that intersect ANY cell covered by the selected entities.
    /// Entities are grouped by the number of selected-footprint cells they cover (overlap area),
    /// and groups are rotated as UNITS (e.g., three 1x1s move together vs one 3x1).
    /// Only layers of affected entities are reassigned; others are untouched.
    /// </summary>
    public static void CycleLayersAtSelectedLocations(IList<Entity>? selected, IEnumerable<Entity>? allEntities)
    {
        if (selected is null || allEntities is null || selected.Count == 0) return;

        // 1) Build the full set of grid cells covered by the current selection
        var footprint = new HashSet<(int c, int r)>();
        foreach (var s in selected)
            AddEntityCells(s, footprint);

        if (footprint.Count == 0) return;

        // 2) Collect all entities that intersect the selection footprint
        var panelEntities = allEntities.ToList();
        var affected = panelEntities
            .Where(e => OverlapsAny(e, footprint))
            .ToList();

        if (affected.Count < 2) return;

        // 3) Compute how many footprint-cells each affected entity covers (its "overlap area")
        //    This is the GROUP key: area=1 => all single 1x1 tiles in the footprint group together,
        //    area=3 => a 3x1 spanning tile (covering three of the footprint cells) is its own group, etc.
        var overlapByEntity = affected.ToDictionary(
            e => e,
            e => CountOverlap(e, footprint)
        );

        // 4) Group entities by overlap area and order groups by their current z (min layer within group)
        var groups = affected
            .GroupBy(e => overlapByEntity[e])         // group key = overlap area inside footprint
            .Select(g => new Group
            {
                OverlapArea = g.Key,
                Entities = g.OrderBy(x => x.Layer).ThenBy(x => x.Guid).ToList()
            })
            .OrderBy(g => g.MinLayer)                 // lowest group first
            .ThenBy(g => g.OverlapArea)               // tie-breaker: smaller area first
            .ToList();

        if (groups.Count < 2) return;

        // 5) Normalize layers within the affected set so all have unique, strictly increasing layers
        //    (preserves relative order where possible)
        EnsureUniqueAscendingLayers(affected);

        // 6) Rotate groups as units: move the lowest group to the top (same behavior as before)
        //    If you want the opposite direction, rotate the other way.
        var rotated = new List<Group>(groups.Count);
        for (int i = 1; i < groups.Count; i++) rotated.Add(groups[i]);
        rotated.Add(groups[0]); // lowest -> top

        // 7) Reassign layers compactly across groups, preserving per-group internal order.
        //    We keep numbers local to the affected set, so no collisions.
        int startLayer = affected.Min(e => e.Layer);
        int nextLayer = startLayer;

        foreach (var g in rotated)
        {
            // Keep the group's internal order the same, just move the whole block together
            foreach (var e in g.Entities)
                e.Layer = nextLayer++;
        }
    }

    // --- helpers --------------------------------------------------------------

    private static void AddEntityCells(Entity e, HashSet<(int c, int r)> acc)
    {
        int w = Math.Max(1, e.Width);
        int h = Math.Max(1, e.Height);
        for (int r = e.Row; r < e.Row + h; r++)
            for (int c = e.Col; c < e.Col + w; c++)
                acc.Add((c, r));
    }

    private static bool ContainsCell(Entity e, int col, int row)
    {
        int w = Math.Max(1, e.Width);
        int h = Math.Max(1, e.Height);
        return col >= e.Col && col <= e.Col + w - 1 &&
               row >= e.Row && row <= e.Row + h - 1;
    }

    private static bool OverlapsAny(Entity e, HashSet<(int c, int r)> cells)
    {
        int w = Math.Max(1, e.Width);
        int h = Math.Max(1, e.Height);
        for (int rr = e.Row; rr < e.Row + h; rr++)
            for (int cc = e.Col; cc < e.Col + w; cc++)
                if (cells.Contains((cc, rr)))
                    return true;
        return false;
    }

    private static int CountOverlap(Entity e, HashSet<(int c, int r)> cells)
    {
        int count = 0;
        int w = Math.Max(1, e.Width);
        int h = Math.Max(1, e.Height);
        for (int rr = e.Row; rr < e.Row + h; rr++)
            for (int cc = e.Col; cc < e.Col + w; cc++)
                if (cells.Contains((cc, rr)))
                    count++;
        return count;
    }

    /// <summary>
    /// Make all layers among 'entities' strictly increasing, preserving order by (Layer, Guid).
    /// </summary>
    private static void EnsureUniqueAscendingLayers(List<Entity> entities)
    {
        var ordered = entities.OrderBy(e => e.Layer).ThenBy(e => e.Guid).ToList();
        var next = ordered[0].Layer;
        ordered[0].Layer = next;
        for (int i = 1; i < ordered.Count; i++)
        {
            var current = ordered[i].Layer;
            if (current <= next) current = next + 1;
            ordered[i].Layer = current;
            next = current;
        }
    }

    private sealed class Group
    {
        public int OverlapArea { get; init; }
        public List<Entity> Entities { get; init; } = new();
        public int MinLayer => Entities.Count == 0 ? int.MaxValue : Entities.Min(e => e.Layer);
    }
}
