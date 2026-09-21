using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Configurations
{
    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.ToTable("Employees");
            builder.HasKey(e => e.Id);

            builder.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            builder.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            builder.Property(e => e.Email).IsRequired().HasMaxLength(320);
            builder.Property(e => e.Department).IsRequired().HasMaxLength(100);
            builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(e => e.CustomData).HasColumnType("jsonb").IsRequired();

            builder.HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(e => new { e.TenantId, e.Email })
                .IsUnique()
                .HasFilter("\"DeletedAt\" IS NULL");
        }
    }
}
