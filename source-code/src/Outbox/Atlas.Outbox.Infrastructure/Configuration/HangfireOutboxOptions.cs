namespace Atlas.Outbox.Infrastructure.Configuration;

public sealed class HangfireOutboxOptions
{
    public string StorageConnectionStringName { get; set; } = "Default";
    public string StorageSchema { get; set; } = "atlas_hangfire";
    public string DashboardPath { get; set; } = "/hangfire";
    public string[] Urls { get; set; } = ["http://localhost:5230"];
    public string RecurringCron { get; set; } = "* * * * *";
    public bool TriggerJobsOnStartup { get; set; } = true;
    public TimeSpan ProcessingWindow { get; set; } = TimeSpan.FromSeconds(55);
    public int WorkerCount { get; set; } = 2;
    public string ServerName { get; set; } = "atlas-outbox-hangfire";
    public DashboardAuthOptions DashboardAuth { get; set; } = new();
}

public sealed class DashboardAuthOptions
{
    public bool Enabled { get; set; } = true;
    public string Username { get; set; } = "admin";
    public string Password { get; set; } = "change-me";
    public bool ReadOnly { get; set; }
    public bool AllowInsecureHttp { get; set; } = true;
}
