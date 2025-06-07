namespace DCCPanelController.Helpers;

public static class DccAddressHelper {
    public static int FromDccAddressString(this string address) {
        if (string.IsNullOrEmpty(address)) return 0;
        var numericChars = address.Where(char.IsDigit);
        var numericString = string.Concat(numericChars);
        return int.TryParse(numericString, out var result) ? result : 0;
    }
}