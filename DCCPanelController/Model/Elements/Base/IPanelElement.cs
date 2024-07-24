namespace DCCPanelController.Model.Elements.Base;

public interface IPanelElement {

    string ElementType { get; }
    string SymbolType { get; set; }
    Coordinate Coordinate { get; set; }
    Color Color { get; set; }
    int Rotation { get; set; }
}