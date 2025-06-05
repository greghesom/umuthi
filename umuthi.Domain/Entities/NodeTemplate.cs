using umuthi.Domain.Enums;

namespace umuthi.Domain.Entities;

public class NodeTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public NodeType NodeType { get; set; }
    public ModuleType ModuleType { get; set; }
    
    public string? Icon { get; set; } // Icon identifier or URL
    public string? Category { get; set; } // For grouping templates
    
    public string? DefaultConfiguration { get; set; } // JSON default configuration
    public string? ConfigurationSchema { get; set; } // JSON schema for validation
    
    public bool IsActive { get; set; } = true;
    public string? Version { get; set; } = "1.0.0";
}