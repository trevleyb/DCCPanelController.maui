using System.Globalization;

namespace DCCPanelController.Helpers.Converters;

public class PanelToCardHeightConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double collectionViewWidth && collectionViewWidth > 0)
        {
            // Get the span from the parameter (GridItemsLayout)
            var span = 3; // default
            if (parameter is GridItemsLayout layout)
            {
                span = layout.Span;
            }
            
            // Calculate width per item
            var margin = 20;     // Account for margins (10 on each side of CollectionView + item margins)
            var itemMargin = 10; // Account for item margins (5 on each side)
            var availableWidth = collectionViewWidth - margin;
            var itemWidth = (availableWidth / span) - itemMargin;
            
            // Return the same value for height to make it square
            return Math.Max(itemWidth, 100); // Minimum height of 100
        }
        
        return 150; // Fallback height
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}