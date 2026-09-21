using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        private readonly ITenantContext _tenantContext;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantContext tenantContext)
            : base(options)
        {
            _tenantContext = tenantContext;
        }

        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<TaskItem> Tasks => Set<TaskItem>();

        private Guid CurrentTenantId => _tenantContext.TenantId;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            modelBuilder.Entity<Employee>()
                .HasQueryFilter(e => e.TenantId == CurrentTenantId && e.DeletedAt == null);

            modelBuilder.Entity<TaskItem>()
                .HasQueryFilter(t => t.TenantId == CurrentTenantId);
        }
    }
}
