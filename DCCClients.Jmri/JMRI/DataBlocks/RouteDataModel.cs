using System.Text.Json;
using System.Text.Json.Serialization;
namespace DCCClients.Jmri.JMRI.DataBlocks; // Root structure that contains the turnout data

// Root structure that contains the route data
public class JmriRouteData
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
    
    [JsonPropertyName("data")]
    public JmriRoute Data { get; set; }
}

// The route with its properties
public class JmriRoute
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("userName")]
    public string UserName { get; set; }
    
    [JsonPropertyName("comment")]
    public string Comment { get; set; }
    
    [JsonPropertyName("properties")]
    public List<object> Properties { get; set; }
    
    [JsonPropertyName("state")]
    public int State { get; set; }
}