namespace LupiraWeb.Domain;

public enum LocationKind
{
    Office,
    Home,
    Client,
    Event,
}

public record Location(LocationKind Kind, string? City, string? Country);
