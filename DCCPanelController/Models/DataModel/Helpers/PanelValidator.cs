using System;
using System.Collections.Generic;
using System.Linq;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Actions;

namespace DCCPanelController.Models.DataModel.Helpers;

public enum ValidationSeverity {
    Error,
    Warning,
    Info
}

public record ValidationIssue(
    ValidationSeverity Severity,
    string Code,
    string Message,
    string? PanelId = null,
    string? EntityType = null,
    string? EntityId = null);

public static class PanelValidator {
    /// <summary>
    /// Validate a single panel using the Profile-wide collections reachable via panel.Panels.Profile.
    /// </summary>
    public static IResult<List<ValidationIssue>> Validate(Panel panel) {
        var issues = new List<ValidationIssue>();

        // Basic wiring checks
        if (panel.Panels?.Profile is null) {
            issues.Add(new ValidationIssue(ValidationSeverity.Error, "PROFILE_NOT_FOUND", "Panel.Panels.Profile is null — cannot validate without Profile collections.", panel.Id));
            return Result<List<ValidationIssue>>.Fail("Panel.Panels.Profile is null — cannot validate without Profile collections.").WithValue(issues);
        }

        var profile = panel.Panels.Profile;

        // Helpers (case-insensitive)
        bool HasTurnout(string id) => !string.IsNullOrWhiteSpace(id) && profile.Turnouts.Any(t => string.Equals(t.Id, id, StringComparison.InvariantCultureIgnoreCase));
        bool HasRoute(string id) => !string.IsNullOrWhiteSpace(id) && profile.Routes.Any(r => string.Equals(r.Id, id, StringComparison.InvariantCultureIgnoreCase));
        bool HasLight(string id) => !string.IsNullOrWhiteSpace(id) && profile.Lights.Any(l => string.Equals(l.Id, id, StringComparison.InvariantCultureIgnoreCase));
        bool HasTurnoutName(string name) => !string.IsNullOrWhiteSpace(name) && profile.Turnouts.Any(t => string.Equals(t.Name, name, StringComparison.InvariantCultureIgnoreCase));
        string TurnoutIdFromName(string name) => profile.Turnouts.First(t => string.Equals(t.Name, name, StringComparison.InvariantCultureIgnoreCase)).Id ?? "";

        // 1) TurnoutButtonEntity.TurnoutID must exist in Profile.Turnouts
        foreach (var tButton in panel.GetPanelEntitiesByType<TurnoutButtonEntity>()) {
            if (!HasTurnout(tButton.TurnoutID) && !string.IsNullOrWhiteSpace(tButton.TurnoutID)) {
                var tempId = tButton.TurnoutID;
                if (HasTurnoutName(tButton.TurnoutID)) {
                    tButton.TurnoutID = TurnoutIdFromName(tButton.TurnoutID);
                    issues.Add(new ValidationIssue(
                        ValidationSeverity.Info,
                        "TURNOUT_REFERENCE_INCORRECT",
                        $"TurnoutButtonEntity references TurnoutID as '{tempId}' but should be '{tButton.TurnoutID}', corrected...",
                        panel.Id,
                        nameof(TurnoutButtonEntity),
                        tButton.TurnoutID
                    ));
                } else {
                    issues.Add(new ValidationIssue(
                        ValidationSeverity.Error,
                        "TURNOUT_BUTTON_TARGET_MISSING",
                        $"TurnoutButtonEntity references TurnoutID '{tButton.TurnoutID}', which does not exist in Profile.Turnouts.",
                        panel.Id,
                        nameof(TurnoutButtonEntity),
                        tButton.TurnoutID
                    ));
                }
            }
        }

        // 2) TurnoutEntity.TurnoutID must exist in Profile.Turnouts
        foreach (var turnoutEntity in panel.GetPanelEntitiesByType<TurnoutEntity>()) {
            if (!HasTurnout(turnoutEntity.TurnoutID) && !string.IsNullOrWhiteSpace(turnoutEntity.TurnoutID)) {
                var tempId = turnoutEntity.TurnoutID;
                if (HasTurnoutName(turnoutEntity.TurnoutID)) {
                    turnoutEntity.TurnoutID = TurnoutIdFromName(turnoutEntity.TurnoutID);
                    issues.Add(new ValidationIssue(
                        ValidationSeverity.Info,
                        "TURNOUT_REFERENCE_INCORRECT",
                        $"TurnoutButtonEntity references TurnoutID as '{tempId}' but should be '{turnoutEntity.TurnoutID}', corrected...",
                        panel.Id,
                        nameof(TurnoutButtonEntity),
                        turnoutEntity.TurnoutID
                    ));
                } else {
                    issues.Add(new ValidationIssue(
                        ValidationSeverity.Error,
                        "TURNOUT_ENTITY_TARGET_MISSING",
                        $"TurnoutEntity '{turnoutEntity.Id}' references TurnoutID '{turnoutEntity.TurnoutID}', which does not exist in Profile.Turnouts.",
                        panel.Id,
                        turnoutEntity.GetType().Name,
                        turnoutEntity.Id
                    ));
                }
            }
        }

        // 3) RouteEntity.Id must exist in Profile.Routes
        foreach (var routeEntity in panel.GetPanelEntitiesByType<RouteEntity>()) {
            if (!HasRoute(routeEntity.Id) && !string.IsNullOrWhiteSpace(routeEntity.Id)) {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Error,
                    "ROUTE_ENTITY_TARGET_MISSING",
                    $"RouteEntity references RouteID '{routeEntity.Id}', which does not exist in Profile.Routes.",
                    panel.Id,
                    nameof(RouteEntity),
                    routeEntity.Id
                ));
            }
        }

        // 4) "Switch entity (not attached) has an ID which must exist in Profile.Lights"
        foreach (var entity in panel.GetPanelEntitiesByType<SwitchEntity>()) {
            if (!HasLight(entity.Id) && !string.IsNullOrWhiteSpace(entity.Id)) {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Error,
                    "SWITCH_LIGHT_TARGET_MISSING",
                    $"{entity.EntityName} references Light ID '{entity.Id}', which does not exist in Profile.Lights.",
                    panel.Id,
                    entity.EntityName,
                    entity.Id
                ));
            }
        }

        // 5) ActionButtonEntity:
        //    - ButtonActions.ActionID must reference a valid ActionButtonEntity.Id
        //    - TurnoutActions.ActionID must reference a valid Turnout in Profile.Turnouts
        var allActionButtons = panel.GetAllEntitiesByType<ActionButtonEntity>();
        var actionButtonIds = new HashSet<string>(
            allActionButtons.Select(b => b.Id).Where(s => !string.IsNullOrWhiteSpace(s)),
            StringComparer.InvariantCultureIgnoreCase
        );

        foreach (var aButton in panel.GetPanelEntitiesByType<ActionButtonEntity>()) {
            // ButtonActions → ActionButtonEntity.Id
            foreach (var action in aButton.ButtonPanelActions ?? Enumerable.Empty<ButtonAction>()) {
                if (string.IsNullOrWhiteSpace(action.ActionID) || !actionButtonIds.Contains(action.ActionID)) {
                    issues.Add(new ValidationIssue(
                        ValidationSeverity.Error,
                        "BUTTON_ACTION_TARGET_MISSING",
                        $"ActionButtonEntity '{aButton.Id}' has ButtonAction.ActionID='{action.ActionID}' that does not match any ActionButtonEntity.Id.",
                        panel.Id,
                        nameof(ActionButtonEntity),
                        aButton.Id
                    ));
                }
            }

            // TurnoutActions → Profile.Turnouts.Id
            foreach (var action in aButton.TurnoutPanelActions ?? Enumerable.Empty<TurnoutAction>()) {
                if (!HasTurnout(action.ActionID)) {
                    issues.Add(new ValidationIssue(
                        ValidationSeverity.Error,
                        "TURNOUT_ACTION_TARGET_MISSING",
                        $"ActionButtonEntity '{aButton.Id}' has TurnoutAction.ActionID='{action.ActionID}' that does not match any Profile Turnout Id.",
                        panel.Id,
                        nameof(ActionButtonEntity),
                        aButton.Id
                    ));
                }
            }
        }

        return issues.Count == 0
            ? Result<List<ValidationIssue>>.Ok(issues)
            : Result<List<ValidationIssue>>.Fail($"Panel '{panel.Id}' has {issues.Count} validation issues.").WithValue(issues);
    }
}