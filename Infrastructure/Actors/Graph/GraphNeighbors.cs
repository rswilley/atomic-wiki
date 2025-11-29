namespace Infrastructure.Actors.Graph;

[GenerateSerializer]
[Alias("Actors.Graph.GraphNeighbors")]
public record GraphNeighbors(
    string Center,
    IReadOnlyList<string> Outgoing,
    IReadOnlyList<string> Incoming);