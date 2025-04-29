namespace DCCClients.Jmri.JMRI.Helpers;

public static class Extensions {
    public static int ConvertToDCCAddress(this string dccLabel) {
        if (string.IsNullOrEmpty(dccLabel)) return 0;
        var numericPart = new string(dccLabel.Where(char.IsDigit).ToArray());
        return string.IsNullOrEmpty(numericPart) ? 0 : int.Parse(numericPart);
    }
}