namespace BotSharp.Abstraction.VectorStorage.Models;

public class VectorCollectionData
{
    public string Id { get; set; }
    public Dictionary<string, string> Data { get; set; } = new();
    public double? Score { get; set; }
    public float[]? Vector { get; set; }
}