namespace DCCPanelController.Helpers.EditableProperties;

public class EditableDateProperty : EditableProperty {
    public DateOnly MinValue { get; set; } = DateOnly.MinValue;  // used for Int (Minimum Value)
    public DateOnly MaxValue { get; set; } = DateOnly.MaxValue;  // used for Int (Maximum Value)
}