using System.Text.Json.Serialization;

namespace DCCClients.Jmri.JMRI.DataBlocks; // Root structure that contains the turnout data

public class JmriTurnoutData {
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("data")]
    public required JmriTurnout Data { get; set; }
}

// The turnout with its properties
public class JmriTurnout {
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("userName")]
    public string UserName { get; set; } = string.Empty;

    [JsonPropertyName("comment")]
    public string Comment { get; set; } = string.Empty;

    [JsonPropertyName("properties")]
    public List<object>? Properties { get; set; }

    [JsonPropertyName("inverted")]
    public bool Inverted { get; set; }

    [JsonPropertyName("state")]
    public int State { get; set; }

    [JsonPropertyName("feedbackMode")]
    public int FeedbackMode { get; set; }

    [JsonPropertyName("feedbackModes")]
    public List<int>? FeedbackModes { get; set; }

    [JsonPropertyName("sensor")]
    public List<object>? Sensor { get; set; }
}