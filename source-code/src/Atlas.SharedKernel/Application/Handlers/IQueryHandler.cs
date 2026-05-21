namespace Atlas.SharedKernel.Application.Handlers;

/// <summary>
/// Marker interface for query handlers.
/// Specific query handler interfaces should extend this (e.g. IListRolesQueryHandler).
/// </summary>
public interface IQueryHandler<TQuery, TOutput> : IHandler<TQuery, TOutput>
{
}
