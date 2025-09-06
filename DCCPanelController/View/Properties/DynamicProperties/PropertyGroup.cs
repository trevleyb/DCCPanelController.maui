namespace DCCPanelController.View.Properties.DynamicProperties;

public sealed class PropertyGroup
{
    public string Name { get; }
    public int Order { get; }           // used to sort groups, based on first field's Order
    public List<PropertyRow> Rows { get; } = new();
    public PropertyGroup(string name, int order) { Name = name; Order = order; }
}