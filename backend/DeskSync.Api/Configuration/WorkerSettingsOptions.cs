using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeskSync.Api.Configuration;

public class WorkerSettingsOptions
{
    public const string SectionName =  "WorkerSettings";
    public int DefaultBatchSize {get; init;} = 500;
}
