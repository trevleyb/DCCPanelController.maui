using Microsoft.Extensions.Logging;

namespace DCCPanelController.Models.DataModel.Helpers;

public static class ProfileValidator {
    public static void Validate(Profile profile, ILogger logger) {
        
        foreach (var panel in profile.Panels) {
            var results = PanelValidator.Validate(panel);

            if (results.IsOk) {
                logger.LogInformation($"Panel '{panel.Id}' is valid.");
            } else {
                logger.LogWarning(results.Message);
                foreach (var result in results?.Value ?? []) {
                    logger.LogInformation($"{result.Code} : {result.EntityType}@{result.EntityId}: {result.Message}");
                }
            }
        }
    }
}