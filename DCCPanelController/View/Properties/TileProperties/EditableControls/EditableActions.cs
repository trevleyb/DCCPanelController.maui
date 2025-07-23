using System.Reflection;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Interfaces;
using DCCPanelController.View.Actions;
using Microsoft.Extensions.Logging;

namespace DCCPanelController.View.Properties.TileProperties.EditableControls;

public class EditableButtonActions(string label, string description = "", int order = 0, string? group = null, ActionsContext context = ActionsContext.Button)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            var entity = owner as Entity;
            var entityID = (owner as IEntityID)?.Id ?? "";
            var availableButtons = entity?.Parent?.GetAllEntitiesWithID<ActionButtonEntity>().Where(b => !string.IsNullOrWhiteSpace(b.Id) && b.Id != entityID).Select(b => b.Id).ToList<string>() ?? [];
            if (entity is IActionEntity actionsEntity) {
                return new ButtonActionsGrid(actionsEntity, context, availableButtons) {
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill
                };
            }
        } catch (Exception e) {
            PropertyLogger.LogDebug("Unable to create a Action {Message}",e.Message);
            return null;
        }
        PropertyLogger.LogDebug("Creating an Action but no valid Action attributes were found.");
        return null;
    }
}

public class EditableTurnoutActions(string label, string description = "", int order = 0, string? group = null, ActionsContext context = ActionsContext.Button)
    : EditableProperty(label, description, order, group), IEditableProperty {
    public IView? CreateView(object owner, PropertyInfo info) {
        try {
            var entity = owner as Entity;
            var entityID = (owner as IEntityID)?.Id ?? "";
            var availableTurnouts = entity?.Parent?.GetAllEntitiesWithID<TurnoutEntity>().Where(b => !string.IsNullOrWhiteSpace(b.Id) && b.Id != entityID).Select(b => b.Id).ToList<string>() ?? [];
            if (entity is IActionEntity actionsEntity) {
                return new TurnoutActionsGrid(actionsEntity, context, availableTurnouts) {
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill
                };
            }
        } catch (Exception e) {
            PropertyLogger.LogDebug("Unable to create a Action {Message}",e.Message);
            return null;
        }
        PropertyLogger.LogDebug("Creating an Action but no valid Action attributes were found.");
        return null;
    }
}