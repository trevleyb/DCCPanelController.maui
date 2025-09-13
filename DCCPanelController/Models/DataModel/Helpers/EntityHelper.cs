using System.Collections;
using System.Text.RegularExpressions;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.DataModel.Entities.Interfaces;

namespace DCCPanelController.Models.DataModel.Helpers;

public static class EntityHelper {
    
    public static List<T> GetAllEntitiesByType<T>(Panel? parent) where T : IEntityID {
        if (parent is null) return[];
        
        var all = parent.Panels?.SelectMany(panel => panel.Entities.OfType<T>()).ToList() ?? [];
        var local = parent.Entities.OfType<T>().ToList() ?? [];
        return all
              .Concat(local)
              .DistinctBy(e => e.Id)              // if Id is string and you need case-insensitive: .DistinctBy(e => e.Id, StringComparer.OrdinalIgnoreCase)
              .ToList();
    }
    
    public static string GenerateID(IEnumerable<IEntityID> entities, string prefix) {
        return GenerateID(entities, t => t.Id, prefix);
    }

    private static string GenerateID<T>(IEnumerable<T> entities, Func<T, string> idSelector, string defaultPrefix = "UKN") {
        var ids = entities.Select(idSelector).Where(id => !string.IsNullOrEmpty(id)).OrderBy(id => id).ToList();
        if (ids.Count == 0) return $"{defaultPrefix}1"; // Default name if the list is empty.
        var numericalPattern = ids.Select(id => {
                                       var match = Regex.Match(id, @"^(.*?)(\d+)$");                                                                               // Match a prefix and a number (e.g., "Button123").
                                       return match.Success ? (Prefix: match.Groups[1].Value, Number: int.Parse(match.Groups[2].Value)) : (Prefix: id, Number: 0); // No numerical suffix, treat as 0.
                                   })
                                  .OrderBy(item => item.Number)
                                  .ToList();

        if (numericalPattern.Count > 0) {
            var latestItem = numericalPattern.Last(); // Get the last item in the sequence.
            var nextNumber = latestItem.Number + 1;   // Increment the number for the new item.
            return $"{latestItem.Prefix}{nextNumber}";
        }

        // Step 3: Look for alphabetical patterns (e.g., ButtonA, ButtonB).
        var alphabeticalPattern = ids
                                 .Where(id => Regex.IsMatch(id, @"^[a-zA-Z]+$")) // Match IDs containing only letters.
                                 .OrderBy(id => id)
                                 .ToList();

        if (alphabeticalPattern.Count > 0) {
            var lastID = alphabeticalPattern.Last();          // Get the last ID in the alphabetical sequence.
            var nextID = IncrementAlphabeticalString(lastID); // Increment alphabetically.
            return nextID;
        }

        // Step 4: Fallback if no discernible pattern is found (e.g., "UnnamedItem1").
        return $"{defaultPrefix}{ids.Count + 1}";
    }

// Helper Method: Increment alphabetical strings (e.g., "A" -> "B", "Z" -> "AA").
    private static string IncrementAlphabeticalString(string input) {
        var chars = input.ToCharArray();

        for (var i = chars.Length - 1; i >= 0; i--) {
            if (chars[i] < 'Z') {
                chars[i]++;
                return new string(chars);
            }

            chars[i] = 'A';

            if (i == 0) {
                // If we're at the beginning and need to carry over (e.g., "Z" -> "AA").
                return 'A' + new string(chars);
            }
        }
        return new string(chars);
    }
}