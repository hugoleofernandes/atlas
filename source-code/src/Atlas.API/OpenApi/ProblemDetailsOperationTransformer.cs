using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Atlas.API.OpenApi;

public sealed class ProblemDetailsOperationTransformer
    : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        Add(operation, "400", "Validation error");
        Add(operation, "401", "Unauthorized");
        Add(operation, "404", "Not found");
        Add(operation, "409", "Conflict");
        Add(operation, "422", "Business rule violation");
        Add(operation, "500", "Unexpected error");

        return Task.CompletedTask;
    }

    private static void Add(
        OpenApiOperation operation,
        string status,
        string description)
    {
        if (operation.Responses.ContainsKey(status))
            return;

        operation.Responses[status] = new OpenApiResponse
        {
            Description = description
        };
    }
}