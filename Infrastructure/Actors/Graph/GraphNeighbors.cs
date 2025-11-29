namespace Infrastructure.Actors.Graph;

public record GraphNeighbors(
    string Center,
    IReadOnlyList<string> Outgoing,
    IReadOnlyList<string> Incoming);