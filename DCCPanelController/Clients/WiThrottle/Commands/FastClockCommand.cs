namespace DCCPanelController.Clients.WiThrottle.Commands;

public class StopFastClockCommand() : FastClockCommand(DateTime.Now, 0);

public class FastClockCommand(DateTime time, int ratio = 1) : IClientCmd {
    public FastClockCommand() : this(DateTime.Now) { }

    public DateTime Time { get; set; } = time;
    public int Ratio { get; set; } = ratio;

    public long SecondsSince1970 => ConvertToUnixTimeSeconds(Time);

    public string Command => $"PFT{SecondsSince1970}<;>{(Ratio is >= 0 and < 100 ? Ratio : 1)}.0";

    public static long ConvertToUnixTimeSeconds(DateTime dateTime) {
        // Ensure the DateTime is in UTC
        var dateTimeOffset = new DateTimeOffset(dateTime.ToUniversalTime());
        return dateTimeOffset.ToUnixTimeSeconds();
    }
}