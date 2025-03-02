using System.Text.RegularExpressions;
using DCCPanelController.Model;

namespace DCCPanelController.Helpers;

public static class TurnoutAnalyzer {
    public static string GetUniqueID(List<Turnout> turnouts) {
        return AnalyzeTurnouts(turnouts).NewUniqueId;
    }

    public static (string MostCommonPrefix, string NewUniqueId) AnalyzeTurnouts(List<Turnout> turnouts) {
        var prefixCount = new Dictionary<string, int>();
        var prefixNumbers = new Dictionary<string, List<int>>();

        foreach (var turnout in turnouts) {
            var id = turnout.Id ?? "NT";

            // Extract prefix and number
            var (prefix, number) = ExtractPrefixAndNumber(id);

            // Count prefixes
            if (prefixCount.TryGetValue(prefix, out var value)) {
                prefixCount[prefix] = ++value;
                prefixNumbers[prefix].Add(number);
            } else {
                prefixCount[prefix] = 1;
                prefixNumbers[prefix] = [number];
            }
        }

        // Finding the most common prefix
        var mostCommonPrefix = prefixCount.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key ?? string.Empty;

        // Determine next number in sequence
        var nextNumber = 1;

        if (prefixNumbers.TryGetValue(mostCommonPrefix, out var found) && found.Count > 0) {
            nextNumber = found.Max() + 1;
        }

        // Create new unique ID
        var newUniqueId = $"{mostCommonPrefix}{nextNumber:D3}";
        return (mostCommonPrefix, newUniqueId);
    }

    private static (string, int) ExtractPrefixAndNumber(string id) {
        // Match prefix and number using Regex
        var match = Regex.Match(id, @"^([^\d]+)(\d+)$");

        if (match.Success) {
            var prefix = match.Groups[1].Value;
            var number = int.Parse(match.Groups[2].Value);
            return (prefix, number);
        }

        return (id, 0); // Return the whole as prefix if there's no number part
    }
}