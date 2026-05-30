using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Commands.DevLogin;

public interface IDevLoginCommandHandler : ICommandHandler<DevLoginCommand, DevLoginOutput>;
