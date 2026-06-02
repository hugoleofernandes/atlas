namespace Atlas.BuildingBlocks.AspNetCore.Security.InternalApi;

public static class InternalApiKeyDefaults
{
    public const string AuthenticationScheme = "InternalApiKey";
    public const string PolicyName = "internal-api";
    public const string ActorTypeClaim = "actor_type";
    public const string ServiceActorType = "service";
}
