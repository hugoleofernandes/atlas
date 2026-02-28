namespace Atlas.Identity.Domain.Entities;

public sealed class IdentityUser
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string? ExternalId { get; private set; }
    public bool IsActive { get; private set; } = true;

    public IdentityUser() { }

    public IdentityUser(string externalId)
    {
        ExternalId = externalId;
    }

    public void Deactivate() => IsActive = false;
}