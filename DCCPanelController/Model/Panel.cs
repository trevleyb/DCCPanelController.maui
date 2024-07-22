using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Model.Elements;

namespace DCCPanelController.Model;

/// <summary>
/// Represents a Panel or Schematic that we can display on the app to control
/// </summary>
public partial class Panel : ObservableValidator, ICloneable {
    
    [ObservableProperty] private string _id = "new";
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private int _sortOrder = 0;
    //[ObservableProperty] private ImageSource? _panelImage;
    
    [NotifyPropertyChangedFor(nameof(PanelRatio))]
    [ObservableProperty] private int _cols = 24;

    [NotifyPropertyChangedFor(nameof(PanelRatio))]
    [ObservableProperty] private int _rows = 18;
    
    [ObservableProperty] private ObservableCollection<IPanelElement> _elements = [];

    [JsonIgnore]
    public string PanelRatio => CalculateRatio(Cols, Rows);

    /// <summary>
    /// This looks to see if the coordinates provided are currently occupied already.
    /// This looks at the Width and Height of each item in the panel and returns true
    /// if any of them clash with the provided coordinate. 
    /// </summary>
    /// <param name="coordinates">The coordinates to check</param>
    /// <returns>True if the coordinate is already occupied. </returns>
    public bool IsCellOccupied(Coordinate coordinates, IPanelElement? activeElement) {
        foreach (var element in Elements) {
            if (activeElement is null || element != activeElement) {
                for (var coordX = 0; coordX < coordinates.Width; coordX++) {
                    for (var coordY = 0; coordY < coordinates.Height; coordY++) {
                        for (var elementX = 0; elementX < element.Coordinate.Width; elementX++) {
                            for (var elementY = 0; elementY < element.Coordinate.Height; elementY++) {
                                if (element.Coordinate.Col + elementX == coordinates.Col + coordX && element.Coordinate.Row + elementY == coordinates.Row + coordY) {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
        }
        return false;
    } 

    /// <summary>
    /// Works out if the coordinates provided, plus any height/width of the element are inside or outside
    /// the bounds of the Panel. Will return true if it is outside the bounds. 
    /// </summary>
    /// <param name="coordinates"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public bool IsOutsideBounds(Coordinate coordinates) {
        var isOutOfBounds = (coordinates.Col + (coordinates.Width- 1) > Cols || coordinates.Row + (coordinates.Height -1) > Rows);
        return isOutOfBounds;
    } 

    public void Validate() {

        // Make sure that all the Coordinates for the Track Pieces are valid and 
        // if not, make sure they are within the bounds of the Panel. 
        if (Elements.Any()) {
            foreach (var element in Elements) {
                if (element.Coordinate.Col <= 0) element.Coordinate.Col = 1;
                if (element.Coordinate.Col >= Cols) element.Coordinate.Col = Cols;
                if (element.Coordinate.Row <= 0) element.Coordinate.Row = 1;
                if (element.Coordinate.Row >= Rows) element.Coordinate.Row = Rows;
            }
        }

        // Validate that none of the tracks overlap any other tracks. If they do, 
        // then we need to remove them or move them (lets try to move them). 
        List<IPanelElement> invalidElements = new();
        foreach (var element in Elements) {
            while (Elements.Any(t => t.Coordinate.Col == element.Coordinate.Col && t.Coordinate.Row == element.Coordinate.Row && t != element)) {
                element.Coordinate.Col += 1;
                if (element.Coordinate.Col >= Cols) {
                    element.Coordinate.Col = 1;
                    element.Coordinate.Row += 1;
                    if (element.Coordinate.Row >= Rows) {
                        element.Coordinate.Row = -1;
                        element.Coordinate.Col = -1;
                        invalidElements.Add(element);
                        break;
                    }
                }
            }
        }
        foreach (var invalidElement in invalidElements) Elements.Remove(invalidElement);
    }

    /// <summary>
    /// Create a deep copy of the Panel object.
    /// </summary>
    /// <returns>A new instance of the Panel object with the same property values as the original.</returns>
    public object Clone() {
        var newPanel = new Panel {
            Id = Id,
            Name = Name,
            SortOrder = SortOrder,
            //PanelImage = PanelImage,
            Cols = Cols,
            Rows = Rows
        };    
        foreach (var element in Elements) newPanel.Elements.Add(element);
        return newPanel;
    }

    [JsonIgnore]
    public Panel? Copy => Clone() as Panel;
    
    private static string CalculateRatio(int col, int row) {
        var gcd = Gcd(col, row);
        var x = col / gcd;
        var y = row / gcd;
        return $"{x:0.#}:{y:0.#}";

        double Gcd(double a, double b) {
            while (b != 0) {
                var temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }
    }
}

[JsonSerializable(typeof(List<Panel>))]
internal sealed partial class PanelStateContext : JsonSerializerContext{ }
