using System.ComponentModel.DataAnnotations;

namespace DeskSync.Api.Configuration;

public class WorkerSettingsOptions
{
    public const string SectionName = "WorkerSettings";

    [Range(1, 5000, ErrorMessage = "Batch size must be between 1 and 5000.")]
    public int DefaultBatchSize { get; init; } = 500;

    [Range(5, 60, ErrorMessage = "Reminder window must be between 5 and 60 minutes.")]
    // The cron runs every 5 minutes. A 15-minute window ensures we catch all reservations
    // starting within the next 15 minutes, regardless of when the job fires.
    // This means reminders arrive 10-15 minutes before the start time.
    public int ReminderWindowMinutes { get; init; } = 15;
}