namespace Atlas.API.Models.Session;

public class GetSessionResponse
{
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string Sub { get; init; }
}
