using System.Text.RegularExpressions;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Accessories;

namespace DCCPanelController.Helpers;

public class TableAnalyser<T>  where T : IAccessory {
    public static string GetUniqueID(List<T> collection) => AnalyzeCollection(collection).NewUniqueId;

    public static (string MostCommonPrefix, string NewUniqueId) AnalyzeCollection(List<T> collection) {
        var patternCount = new Dictionary<string, int>();
        var patternNumbers = new Dictionary<string, List<(int number, string originalFormat)>>();

        foreach (var item in collection) {
            var id = item.Id ?? "1";

            // Extract prefix, number, and suffix
            var (prefix, number, suffix, originalFormat) = ExtractPrefixNumberSuffix(id);
            var pattern = prefix + suffix; // Pattern is prefix + suffix (e.g., "NT", "T...X", "")

            // Count patterns
            if (patternCount.ContainsKey(pattern)) {
                patternCount[pattern]++;
                patternNumbers[pattern].Add((number, originalFormat));
            } else {
                patternCount[pattern] = 1;
                patternNumbers[pattern] = [(number, originalFormat)];
            }
        }

        // Find the most common pattern
        var mostCommonPattern = patternCount
                               .OrderByDescending(kvp => kvp.Value)
                               .FirstOrDefault()
                               .Key ?? string.Empty;

        // Determine next number in sequence
        var nextNumber = 1;
        var formatToUse = ""; // Default format

        if (patternNumbers.TryGetValue(mostCommonPattern, out var found) && found.Count > 0) {
            nextNumber = found.Max(x => x.number) + 1;

            // Use the format from the highest numbered item
            formatToUse = found.OrderByDescending(x => x.number).First().originalFormat;
        }

        // Split pattern back into prefix and suffix
        var (patternPrefix, patternSuffix) = SplitPattern(mostCommonPattern);

        // Create new unique ID maintaining original format
        var numberPart = FormatNumber(nextNumber, formatToUse);
        var newUniqueId = $"{patternPrefix}{numberPart}{patternSuffix}";

        return(patternPrefix, newUniqueId);
    }

    private static (string prefix, int number, string suffix, string originalFormat) ExtractPrefixNumberSuffix(string id) {
        // Match: optional prefix + number + optional suffix
        var match = Regex.Match(id, @"^([^\d]*)(\d+)([^\d]*)$");

        if (match.Success) {
            var prefix = match.Groups[1].Value;
            var numberStr = match.Groups[2].Value;
            var suffix = match.Groups[3].Value;
            var number = int.Parse(numberStr);

            // Preserve original number formatting (leading zeros, etc.)
            var originalFormat = numberStr;

            return(prefix, number, suffix, originalFormat);
        }

        // If no number found, treat as prefix with number 0
        return(id, 0, "", "");
    }

    private static (string prefix, string suffix) SplitPattern(string pattern) {
        // For most patterns, we need to intelligently split
        // This is tricky because "TX" could be prefix "T" + suffix "X" or prefix "TX" + no suffix

        // Common patterns - you might need to expand this based on your data
        var knownSuffixes = new[] { "X", "Y", "Z", "A", "B", "C" };

        foreach (var suffix in knownSuffixes) {
            if (pattern.EndsWith(suffix) && pattern.Length > suffix.Length) {
                return(pattern[..^suffix.Length], suffix);
            }
        }

        // If no known suffix found, treat entire pattern as prefix
        return(pattern, "");
    }

    private static string FormatNumber(int number, string originalFormat) {
        if (string.IsNullOrEmpty(originalFormat)) {
            return number.ToString();
        }

        // If original had leading zeros, preserve that format
        if (originalFormat.StartsWith('0') && originalFormat.Length > 1) {
            return number.ToString($"D{originalFormat.Length}");
        }

        // Otherwise use simple format
        return number.ToString();
    }
}