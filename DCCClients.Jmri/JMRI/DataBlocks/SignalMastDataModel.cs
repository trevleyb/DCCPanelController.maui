using System.Text.Json;
using System.Text.Json.Serialization;
namespace DCCClients.Jmri.JMRI.DataBlocks; // Root structure that contains the turnout data

// Root structure that contains the signal mast data
public class JmriSignalMastData
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
    
    [JsonPropertyName("data")]
    public JmriSignalMast Data { get; set; }
}

// The signal mast with its properties
public class JmriSignalMast
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("userName")]
    public string UserName { get; set; }
    
    [JsonPropertyName("comment")]
    public string Comment { get; set; }
    
    [JsonPropertyName("properties")]
    public List<object> Properties { get; set; }
    
    [JsonPropertyName("aspect")]
    public string Aspect { get; set; }
    
    [JsonPropertyName("lit")]
    public bool Lit { get; set; }
    
    [JsonPropertyName("held")]
    public bool Held { get; set; }
    
    [JsonPropertyName("state")]
    public string State { get; set; }
}