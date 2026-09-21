using Domain.Entities;
using Domain.Enums;
using Infrastructure.Multitenancy;
using Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Helpers
{
    public sealed class TestDatabase : IDisposable
    {
        private readonly SqliteConnection _connection;

        public TestDatabase()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var (db, _) = CreateScope(SeedData.TenantAId);
            db.Database.EnsureCreated();
        }

        public (ApplicationDbContext Db, TenantContext Tenant) CreateScope(Guid tenantId)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            var tenant = new TenantContext();
            tenant.SetTenant(tenantId);

            return (new ApplicationDbContext(options, tenant), tenant);
        }

        public async Task<IReadOnlyList<Guid>> SeedEmployeesAsync(Guid tenantId, int count)
        {
            var (db, _) = CreateScope(tenantId);
            var baseTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var ids = new List<Guid>();

            for (var i = 1; i <= count; i++)
            {
                var employee = new Employee
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    FirstName = $"First{i}",
                    LastName = "Last",
                    Email = $"user{i}@test.com",
                    Department = "IT",
                    Status = EmployeeStatus.Active,
                    CustomData = "{}",
                    CreatedAt = baseTime.AddMinutes(i)
                };

                ids.Add(employee.Id);
                db.Employees.Add(employee);
            }

            await db.SaveChangesAsync();
            return ids;
        }

        public void Dispose() => _connection.Dispose();
    }
}
