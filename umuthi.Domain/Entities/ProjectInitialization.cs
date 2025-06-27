using System;

namespace umuthi.Domain.Entities;

/// <summary>
/// Domain entity representing a project initialization request
/// </summary>
public class ProjectInitialization : BaseEntity
{
    /// <summary>
    /// Unique correlation ID for tracking the project (PROJ + 4 chars)
    /// </summary>
    public Guid CorrelationId { get; set; } = Guid.Empty;
    
    /// <summary>
    /// Google Sheet row identifier
    /// </summary>
    public string GoogleSheetRowId { get; set; } = string.Empty;
    
    /// <summary>
    /// Fillout form data as JSON string
    /// </summary>
    public string FilloutData { get; set; } = string.Empty;
    
    /// <summary>
    /// Make.com customer identifier
    /// </summary>
    public string MakeCustomerId { get; set; } = string.Empty;
}