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