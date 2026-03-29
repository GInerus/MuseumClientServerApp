using System.Text.Json.Serialization;

public class CountResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }
}