using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities.Actions;
using DCCPanelController.Models.DataModel.Entities.BaseClasses;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.Models.DataModel.Helpers;
using DCCPanelController.Services;
using DCCPanelController.View.Actions;
using DCCPanelController.View.Properties.DynamicProperties;

namespace DCCPanelController.Models.DataModel.Entities;

public partial class ScriptButtonEntity : ButtonEntity, IInteractiveEntity {

    [ObservableProperty] [property: Editable("Button Size", Order = 2)]
    private ButtonSizeEnum _buttonSize = ButtonSizeEnum.Normal;

    [ObservableProperty] [property: Editable("Button Type", Order = 2)]
    private ButtonStyleEnum _buttonStyle = ButtonStyleEnum.Round;

    [ObservableProperty] [property: Editable("Run Script?", EditorKind  = EditorKinds.Choice, Order = 2, Choices = new [] {"None","Set All Turnouts to Default"})]
    private string _scriptname = "None";

    [ObservableProperty]
    private ButtonStateEnum _state = ButtonStateEnum.Unknown;

    [JsonConstructor]
    public ScriptButtonEntity() { }

    public ScriptButtonEntity(Panel panel) : base(panel) { }
    public ScriptButtonEntity(ScriptButtonEntity entity) : base(entity) { }

    [JsonIgnore] protected override int RotationFactor => 90;
    [JsonIgnore] public override string EntityName => "S-Button";
    [JsonIgnore] public override string EntityDescription => "Script Actions Button";
    [JsonIgnore] public override string EntityInformation =>
        "The Script Button allows you to execute a number of pre-defined Scripts such as resetting all Turnouts to the default state. ";

    public ActionsContext Context => ActionsContext.Button;
    public override Entity Clone() => new ScriptButtonEntity(this);
    
}