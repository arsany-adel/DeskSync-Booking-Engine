using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace DeskSync.Api.Configuration;

public class WorkerSettingsOptions
{
    public const string SectionName =  "WorkerSettings";
    [Range(1, 5000, ErrorMessage = "Batch size must be between 1 and 5000 to prevent database locking.")]
    public int DefaultBatchSize {get; init;} =500;
}
