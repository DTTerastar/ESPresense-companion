using System.Text.Json.Serialization;

namespace ESPresense.Models;

public class GlobalUpdating
{
    [JsonPropertyName("auto_update")]
    public bool AutoUpdate { get; set; }

    [JsonPropertyName("beta")]
    public bool Beta { get; set; }
}