using Application.Employees.Queries;
using Application.Employees.Queries.ListEmployee;
using FluentAssertions;
using Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTests.Helpers;

namespace UnitTests.Employees
{
    public class ListEmployeesQueryHandlerTests : IDisposable
    {
        private readonly TestDatabase _database = new();

        [Theory]
        [InlineData(1, 10, 10, "user1@test.com")]
        [InlineData(2, 10, 10, "user11@test.com")]
        [InlineData(3, 10, 5, "user21@test.com")]
        public async Task Handle_ReturnsRequestedPageAndTotalCount(
            int page, int pageSize, int expectedItems, string expectedFirstEmail)
        {
            await _database.SeedEmployeesAsync(SeedData.TenantAId, 25);
            var (db, _) = _database.CreateScope(SeedData.TenantAId);
            var handler = new ListEmployeesQueryHandler(db);

            var result = await handler.Handle(
                new ListEmployeesQuery { Page = page, PageSize = pageSize },
                CancellationToken.None);

            result.TotalCount.Should().Be(25);
            result.Page.Should().Be(page);
            result.PageSize.Should().Be(pageSize);
            result.Items.Should().HaveCount(expectedItems);
            result.Items[0].Email.Should().Be(expectedFirstEmail);
        }

        public void Dispose() => _database.Dispose();
    }
}
