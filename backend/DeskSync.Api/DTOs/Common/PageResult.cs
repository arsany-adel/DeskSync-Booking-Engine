using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeskSync.Api.DTOs.Common;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int ItemsPerPage { get; init; }
    
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)ItemsPerPage);
}