using Atlas.SharedKernel.Application.Errors;

namespace Atlas.Identity.BffApi.Endpoints.Auth;

/// <summary>
/// Error catalog for authentication and session concerns handled at the API boundary.
/// These errors originate before reaching any domain layer (missing claims, unknown tenant in config).
/// </summary>
public static class AuthErrors
{
    public static class Tenant
    {
        public static readonly ErrorDefinition NameRequired = new(
            Code: "tenant.name_required",
            FallbackMessage: "Tenant name is required.",
            Category: ErrorCategory.Validation
        );

        public static readonly ErrorDefinition Invalid = new(
            Code: "tenant.invalid",
            FallbackMessage: "Invalid tenant.",
            Category: ErrorCategory.Validation
        );
    }

    public static class Claim
    {
        public static readonly ErrorDefinition EmailMissing = new(
            Code: "claim.email_missing",
            FallbackMessage: "Missing email claim.",
            Category: ErrorCategory.Unauthorized
        );

        public static readonly ErrorDefinition IdentityMissing = new(
            Code: "claim.identity_missing",
            FallbackMessage: "Missing required identity claims.",
            Category: ErrorCategory.Unauthorized
        );
    }
}
