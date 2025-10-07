using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.View.Properties.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class TurnoutButtonEntity : BaseClasses.ButtonEntity, IInteractiveEntity {
    [ObservableProperty] [property: Editable("Button Size", "", 1)]
    private ButtonSizeEnum _buttonSize = ButtonSizeEnum.Normal;

    [ObservableProperty] [property: Editable("Button Type", Order = 2)]
    private ButtonStyleEnum _buttonStyle = ButtonStyleEnum.Round;

    [ObservableProperty]
    [property: Editable("Is Enabled?", "Does this button perform an action", 10, "Colors")]
    private bool _isEnabled = true;

    [ObservableProperty]
    private ButtonStateEnum _state = ButtonStateEnum.Unknown;

    [ObservableProperty] [property: Editable("DCC Turnout", "Turnout to Control with this button", 5, "Turnout", EditorKind = EditorKinds.Turnout)]
    private string _turnoutID = string.Empty;

    [ObservableProperty] [property: Editable("Straight/Normal", "When Turnout is Straight/Thrown set Button to?", 8, "Turnout", EditorKind = EditorKinds.ButtonState)]
    private ButtonStateEnum _whenNormal;

    [ObservableProperty] [property: Editable("Diverging/Thrown", "When Turnout is Diverging/Thrown set Button to?", 8, "Turnout", EditorKind = EditorKinds.ButtonState)]
    private ButtonStateEnum _whenThrown;

    [JsonConstructor]
    public TurnoutButtonEntity() { }

    public TurnoutButtonEntity(Panel panel) : base(panel) { }
    //public TurnoutButtonEntity(TurnoutButtonEntity entity) : base(entity) { }
    public TurnoutButtonEntity(TurnoutButtonEntity entity) : base(entity, "TurnoutPanelActions", "ButtonPanelActions") { }

    [JsonIgnore]
    public Turnout? Turnout => Parent?.Turnout(TurnoutID);

    [JsonIgnore] protected override int RotationFactor => 90;
    [JsonIgnore] public override string EntityName => "T-Button";
    [JsonIgnore] public override string EntityName => "Turnout Toggle Switch";

    public override string EntityInformation =>
        "The `turnout button` is a special button where the button is directly tied to " +
        "a specified turnout. The state of the turnout on the panel is only changed if a turnout " +
        "message is recieved from the controller. So you may find you click the button and nothing happens." +
        "This is because while a message to change the turnout has been sent to the controller,  " +
        "no response has yet been processed so the state will not change. ";

    public override Entity Clone() => new TurnoutButtonEntity(this);
}