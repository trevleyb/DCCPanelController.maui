using System.Collections;

namespace DCCPanelController.View.TileSelectors;

public static class CollectionHitIndex {
    public static int? IndexOf(CollectionView collectionView,
                               Point point,          // point in the CollectionView's coordinate space
                               double scrollXOffset, // horizontal scroll offset (if horizontally scrolling)
                               double scrollYOffset, // vertical   scroll offset (if vertically   scrolling)
                               double edgeMargin,    // left margin before the first column
                               double topMargin,     // top margin before the first row
                               double itemWidth,
                               double itemHeight,
                               double spacingH = 0,
                               double spacingV = 0) {
        // Determine layout & span
        ItemsLayoutOrientation orientation;
        int span;

        switch (collectionView.ItemsLayout) {
        case GridItemsLayout grid:
            orientation = grid.Orientation;
            span = Math.Max(1, grid.Span);
            break;

        case LinearItemsLayout linear:
            orientation = linear.Orientation;
            span = 1; // linear list = single row (Horizontal) or single column (Vertical)
            break;

        default:
            Console.WriteLine($"Unknown ItemsLayout: {collectionView.ItemsLayout}");
            return null; // unknown layout
        }

        var isVertical = orientation == ItemsLayoutOrientation.Vertical;

        // Convert to content coordinates (account for scroll and margins)
        var contentX = point.X + (isVertical ? 0 : scrollXOffset) + edgeMargin;
        var contentY = point.Y + (isVertical ? scrollYOffset : 0) + topMargin;

        var strideX = itemWidth + spacingH;
        var strideY = itemHeight + spacingV;
        if (strideX <= 0 || strideY <= 0) return null;

        // Which column/row did we hit?
        var col = (int)Math.Floor(contentX / strideX);
        var row = (int)Math.Floor(contentY / strideY);
        if (col < 0 || row < 0) return null;

        // If the point is in the spacing gap, treat as "no hit"
        var inCellX = contentX - col * strideX;
        var inCellY = contentY - row * strideY;
        if (inCellX < 0 || inCellY < 0) return null;
        if (inCellX >= itemWidth || inCellY >= itemHeight) return null;

        // Compute index based on orientation + span
        int index;
        if (isVertical) {
            // Vertical grid: span = number of columns; rows grow downward
            var columns = span;
            if (col >= columns) return null;
            index = row * columns + col;
        } else {
            // Horizontal grid: span = number of rows; columns grow to the right
            var rows = span;
            if (row >= rows) return null;
            index = col * rows + row;
        }

        // Clamp to items count
        var count =
            (collectionView.ItemsSource as ICollection)?.Count
         ?? collectionView.ItemsSource?.Cast<object>().Count()
         ?? 0;

        if (index < 0 || index >= count) return null;
        return index;
    }
}