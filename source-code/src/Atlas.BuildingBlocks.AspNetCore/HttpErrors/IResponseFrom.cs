namespace Atlas.BuildingBlocks.AspNetCore.HttpErrors;

/// <summary>
/// Marks an HTTP response DTO that can be created from an application output.
/// This keeps controller result mapping explicit while letting the compiler
/// verify that the response supports the expected output type.
/// </summary>
public interface IResponseFrom<TOutput, TResponse>
    where TResponse : IResponseFrom<TOutput, TResponse>
{
    static abstract TResponse From(TOutput output);
}
