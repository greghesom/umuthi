using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using umuthi.Domain.Entities;

namespace umuthi.Infrastructure.Data.Configurations;

public class WorkflowConfiguration : IEntityTypeConfiguration<Workflow>
{
    public void Configure(EntityTypeBuilder<Workflow> builder)
    {
        builder.HasKey(w => w.Id);
        
        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(w => w.Description)
            .HasMaxLength(1000);
            
        builder.Property(w => w.Configuration)
            .HasColumnType("nvarchar(max)");
            
        builder.HasMany(w => w.Executions)
            .WithOne(e => e.Workflow)
            .HasForeignKey(e => e.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(w => w.Nodes)
            .WithOne(n => n.Workflow)
            .HasForeignKey(n => n.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(w => w.Connections)
            .WithOne(c => c.Workflow)
            .HasForeignKey(c => c.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasIndex(w => w.Name);
    }
}

public class WorkflowExecutionConfiguration : IEntityTypeConfiguration<WorkflowExecution>
{
    public void Configure(EntityTypeBuilder<WorkflowExecution> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(2000);
            
        builder.Property(e => e.Result)
            .HasColumnType("nvarchar(max)");
            
        builder.HasIndex(e => e.WorkflowId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.StartedAt);
    }
}

public class WorkflowNodeConfiguration : IEntityTypeConfiguration<WorkflowNode>
{
    public void Configure(EntityTypeBuilder<WorkflowNode> builder)
    {
        builder.HasKey(n => n.Id);
        
        builder.Property(n => n.Configuration)
            .HasColumnType("nvarchar(max)");
            
        builder.Property(n => n.Metadata)
            .HasColumnType("nvarchar(max)");
            
        builder.HasMany(n => n.SourceConnections)
            .WithOne(c => c.SourceNode)
            .HasForeignKey(c => c.SourceNodeId)
            .OnDelete(DeleteBehavior.NoAction);
            
        builder.HasMany(n => n.TargetConnections)
            .WithOne(c => c.TargetNode)
            .HasForeignKey(c => c.TargetNodeId)
            .OnDelete(DeleteBehavior.NoAction);
            
        builder.HasIndex(n => n.WorkflowId);
        builder.HasIndex(n => n.NodeType);
        builder.HasIndex(n => n.ModuleType);
    }
}

public class WorkflowConnectionConfiguration : IEntityTypeConfiguration<WorkflowConnection>
{
    public void Configure(EntityTypeBuilder<WorkflowConnection> builder)
    {
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.SourcePort)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(c => c.TargetPort)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.HasIndex(c => c.WorkflowId);
        builder.HasIndex(c => c.SourceNodeId);
        builder.HasIndex(c => c.TargetNodeId);
        
        // Unique constraint to prevent duplicate connections
        builder.HasIndex(c => new { c.SourceNodeId, c.TargetNodeId, c.SourcePort, c.TargetPort })
            .IsUnique();
    }
}

public class NodeTemplateConfiguration : IEntityTypeConfiguration<NodeTemplate>
{
    public void Configure(EntityTypeBuilder<NodeTemplate> builder)
    {
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(t => t.Description)
            .HasMaxLength(1000);
            
        builder.Property(t => t.Icon)
            .HasMaxLength(500);
            
        builder.Property(t => t.Category)
            .HasMaxLength(100);
            
        builder.Property(t => t.Version)
            .HasMaxLength(20);
            
        builder.Property(t => t.DefaultConfiguration)
            .HasColumnType("nvarchar(max)");
            
        builder.Property(t => t.ConfigurationSchema)
            .HasColumnType("nvarchar(max)");
            
        builder.HasIndex(t => t.Name);
        builder.HasIndex(t => t.NodeType);
        builder.HasIndex(t => t.ModuleType);
        builder.HasIndex(t => t.Category);
        builder.HasIndex(t => t.IsActive);
    }
}

public class FilloutSubmissionConfiguration : IEntityTypeConfiguration<FilloutSubmission>
{
    public void Configure(EntityTypeBuilder<FilloutSubmission> builder)
    {
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.SubmissionId)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(s => s.FormId)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(s => s.FormName)
            .HasMaxLength(255);
            
        builder.Property(s => s.RawData)
            .IsRequired()
            .HasColumnType("nvarchar(max)");
            
        builder.Property(s => s.LastErrorMessage)
            .HasMaxLength(2000);
            
        builder.Property(s => s.CorrelationId)
            .HasMaxLength(50);
            
        // Indexes for performance
        builder.HasIndex(s => s.SubmissionId)
            .IsUnique(); // Enforce uniqueness for idempotency
            
        builder.HasIndex(s => s.FormId);
        builder.HasIndex(s => s.SubmissionTime);
        builder.HasIndex(s => s.ProcessingStatus);
        builder.HasIndex(s => s.CorrelationId);
        builder.HasIndex(s => s.CreatedAt);
    }
}

public class ProjectInitializationConfiguration : IEntityTypeConfiguration<ProjectInitialization>
{
    public void Configure(EntityTypeBuilder<ProjectInitialization> builder)
    {
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.CorrelationId)
            .IsRequired();
            
        builder.Property(p => p.CustomerEmail)
            .IsRequired()
            .HasMaxLength(255);
            
        builder.Property(p => p.GoogleSheetRowId)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(p => p.FilloutData)
            .IsRequired()
            .HasColumnType("nvarchar(max)");
            
        builder.Property(p => p.MakeCustomerId)
            .IsRequired()
            .HasMaxLength(100);
        
        // Indexes for performance and constraints
        builder.HasIndex(p => p.CorrelationId)
            .IsUnique(); // Ensure correlation ID uniqueness
            
        builder.HasIndex(p => p.CustomerEmail);
        builder.HasIndex(p => p.MakeCustomerId);
        builder.HasIndex(p => p.CreatedAt);
        
        // Unique constraint to prevent duplicate projects
        builder.HasIndex(p => new { p.CustomerEmail, p.GoogleSheetRowId })
            .IsUnique();
    }
}