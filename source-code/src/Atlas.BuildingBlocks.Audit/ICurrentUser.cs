namespace Atlas.BuildingBlocks.Audit;

public interface ICurrentUser
{
    string? UserId { get; }
}