namespace Atlas.SharedKernel.Application.OutboxMessages;

public enum OutboxMessageOrigin
{
    Automatic      = 0,
    ManualResubmit = 1,
}
