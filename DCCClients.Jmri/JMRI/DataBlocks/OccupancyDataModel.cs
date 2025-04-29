using System.Text.Json;
using System.Text.Json.Serialization;
namespace DCCClients.Jmri.JMRI.DataBlocks; // Root structure that contains the turnout data

// Root structure that contains the block data
public class JmriOccupancyData
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
    
    [JsonPropertyName("data")]
    public JmriOccupancy Data { get; set; }
}

// The actual block with all its properties
public class JmriOccupancy
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
    
    [JsonPropertyName("value")]
    public object Value { get; set; }
    
    [JsonPropertyName("sensor")]
    public object Sensor { get; set; }
    
    [JsonPropertyName("reporter")]
    public object Reporter { get; set; }
    
    [JsonPropertyName("speed")]
    public string Speed { get; set; }
    
    [JsonPropertyName("curvature")]
    public int Curvature { get; set; }
    
    [JsonPropertyName("direction")]
    public int Direction { get; set; }
    
    [JsonPropertyName("length")]
    public double Length { get; set; }
    
    [JsonPropertyName("permissive")]
    public bool Permissive { get; set; }
    
    [JsonPropertyName("speedLimit")]
    public double SpeedLimit { get; set; }
    
    [JsonPropertyName("denied")]
    public List<object> Denied { get; set; }
}