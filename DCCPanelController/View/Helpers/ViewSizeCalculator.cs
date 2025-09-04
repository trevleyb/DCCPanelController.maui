using Microsoft.Maui.Layouts;
using MauiView = Microsoft.Maui.Controls.View;

namespace DCCPanelController.View.Helpers;

public static class MauiViewSizeCalculator {
    public static Size CalculateTotalSize(MauiView? mauiView, double width, double height) {
        if (mauiView == null) return new Size(width, height);
        var calculatedSize = CalculateMauiViewSize(mauiView, []);
        return new Size(Math.Min(calculatedSize.Width, width), Math.Min(calculatedSize.Height, height));
    }

    private static Size CalculateMauiViewSize(MauiView mauiView, HashSet<MauiView> visited) {
        // Prevent infinite recursion in case of circular references
        if (visited.Contains(mauiView)) return Size.Zero;
        visited.Add(mauiView);
        
        try {
            // Get the explicit size requests for this MauiView
            var currentSize = GetMauiViewRequestedSize(mauiView);

            // Get children and calculate their total size
            var childrenSize = CalculateChildrenSize(mauiView, visited);
            
            // Combine current MauiView size with children size based on layout type
            return CombineSizes(mauiView, currentSize, childrenSize);
        } finally {
            visited.Remove(mauiView);
        }
    }

    private static Size GetMauiViewRequestedSize(MauiView mauiView) {
        var width = mauiView.WidthRequest >= 0 ? mauiView.WidthRequest : 0;
        var height = mauiView.HeightRequest >= 0 ? mauiView.HeightRequest : 0;

        // Add margin to the requested size
        if (mauiView.Margin != default) {
            width += mauiView.Margin.Left + mauiView.Margin.Right;
            height += mauiView.Margin.Top + mauiView.Margin.Bottom;
        }
        return new Size(width, height);
    }

    private static Size CalculateChildrenSize(MauiView mauiView, HashSet<MauiView> visited) {
        var children = GetChildren(mauiView);
        var childrenList = children.ToList();
        if (!childrenList.Any()) return Size.Zero;
        
        var result = mauiView switch {
            // Stack layouts: children are stacked
            StackLayout stackLayout => CalculateStackLayoutSize(childrenList, stackLayout.Orientation, visited),
            VerticalStackLayout     => CalculateStackLayoutSize(childrenList, StackOrientation.Vertical, visited),
            HorizontalStackLayout   => CalculateStackLayoutSize(childrenList, StackOrientation.Horizontal, visited),

            // Grid: children can overlap, take maximum dimensions
            Grid grid => CalculateGridSize(childrenList, grid, visited),

            // Flex layout: similar to stack but with wrapping
            FlexLayout flexLayout => CalculateFlexLayoutSize(childrenList, flexLayout, visited),

            // Content MauiViews: single child
            ContentView => CalculateSingleChildSize(childrenList, visited),
            Border      => CalculateSingleChildSize(childrenList, visited),

            // Absolute layout: children can be positioned anywhere
            AbsoluteLayout => CalculateAbsoluteLayoutSize(childrenList, visited),

            // Default: treat as overlapping (take maximum)
            _ => CalculateOverlappingSize(childrenList, visited)
        };
        return result;
    }

    private static IEnumerable<MauiView> GetChildren(MauiView mauiView) {
        return mauiView switch {
            Layout layout                        => layout.Children.OfType<MauiView>(),
            ContentView { Content: { } content } => [content],
            Border { Content     : { } content } => [content],
            ScrollView { Content : { } content } => [content],
            _                                    => Enumerable.Empty<MauiView>()
        };
    }

    private static Size CalculateStackLayoutSize(IEnumerable<MauiView> children, StackOrientation orientation, HashSet<MauiView> visited) {
        double totalWidth = 0, totalHeight = 0;
        double maxWidth = 0, maxHeight = 0;

        foreach (var child in children) {
            var childSize = CalculateMauiViewSize(child, visited);
            if (orientation == StackOrientation.Vertical) {
                // Vertical stack: heights add up, take maximum width
                totalHeight += childSize.Height;
                maxWidth = Math.Max(maxWidth, childSize.Width);
            } else {
                // Horizontal stack: widths add up, take maximum height
                totalWidth += childSize.Width;
                maxHeight = Math.Max(maxHeight, childSize.Height);
            }
        }

        return new Size(
            orientation == StackOrientation.Horizontal ? totalWidth : maxWidth,
            orientation == StackOrientation.Vertical ? totalHeight : maxHeight
        );
    }

    private static Size CalculateGridSize(IEnumerable<MauiView> children, Grid grid, HashSet<MauiView> visited) {
        // For Grid, we need to consider column and row definitions
        // This is a simplified version - in reality, Grid sizing is complex

        var columnWidths = new Dictionary<int, double>();
        var rowHeights = new Dictionary<int, double>();

        foreach (var child in children) {
            var childSize = CalculateMauiViewSize(child, visited);
            var column = Grid.GetColumn(child);
            var row = Grid.GetRow(child);
            var columnSpan = Math.Max(1, Grid.GetColumnSpan(child));
            var rowSpan = Math.Max(1, Grid.GetRowSpan(child));

            // Distribute child size across spanned columns/rows
            var widthPerColumn = childSize.Width / columnSpan;
            var heightPerRow = childSize.Height / rowSpan;

            for (int c = column; c < column + columnSpan; c++) {
                columnWidths[c] = Math.Max(columnWidths.GetValueOrDefault(c), widthPerColumn);
            }

            for (int r = row; r < row + rowSpan; r++) {
                rowHeights[r] = Math.Max(rowHeights.GetValueOrDefault(r), heightPerRow);
            }
        }

        return new Size(
            columnWidths.Values.Sum(),
            rowHeights.Values.Sum()
        );
    }

    private static Size CalculateFlexLayoutSize(IEnumerable<MauiView> children, FlexLayout flexLayout, HashSet<MauiView> visited) {
        // Simplified flex layout calculation
        // In reality, this would need to consider flex properties, wrapping, etc.
        var direction = flexLayout.Direction;

        if (direction == FlexDirection.Row || direction == FlexDirection.RowReverse) {
            return CalculateStackLayoutSize(children, StackOrientation.Horizontal, visited);
        } else {
            return CalculateStackLayoutSize(children, StackOrientation.Vertical, visited);
        }
    }

    private static Size CalculateSingleChildSize(IEnumerable<MauiView> children, HashSet<MauiView> visited) {
        var child = children.FirstOrDefault();
        return child != null ? CalculateMauiViewSize(child, visited) : Size.Zero;
    }

    private static Size CalculateAbsoluteLayoutSize(IEnumerable<MauiView> children, HashSet<MauiView> visited) {
        double maxWidth = 0, maxHeight = 0;

        foreach (var child in children) {
            var childSize = CalculateMauiViewSize(child, visited);
            var bounds = AbsoluteLayout.GetLayoutBounds(child);

            // Calculate the furthest extent of each child
            var childMaxX = bounds.X + childSize.Width;
            var childMaxY = bounds.Y + childSize.Height;

            maxWidth = Math.Max(maxWidth, childMaxX);
            maxHeight = Math.Max(maxHeight, childMaxY);
        }

        return new Size(maxWidth, maxHeight);
    }

    private static Size CalculateOverlappingSize(IEnumerable<MauiView> children, HashSet<MauiView> visited) {
        // For overlapping layouts, take the maximum dimensions
        double maxWidth = 0, maxHeight = 0;

        foreach (var child in children) {
            var childSize = CalculateMauiViewSize(child, visited);
            maxWidth = Math.Max(maxWidth, childSize.Width);
            maxHeight = Math.Max(maxHeight, childSize.Height);
        }

        return new Size(maxWidth, maxHeight);
    }

    private static Size CombineSizes(MauiView MauiView, Size MauiViewSize, Size childrenSize) {
        // If MauiView has explicit size requests, use them as minimum
        // Otherwise, use children size
        var finalWidth = Math.Max(MauiViewSize.Width, childrenSize.Width);
        var finalHeight = Math.Max(MauiViewSize.Height, childrenSize.Height);

        // Add padding if the MauiView supports it
        if (MauiView is Layout layout && layout.Padding != default) {
            finalWidth += layout.Padding.Left + layout.Padding.Right;
            finalHeight += layout.Padding.Top + layout.Padding.Bottom;
        }

        return new Size(finalWidth, finalHeight);
    }
}