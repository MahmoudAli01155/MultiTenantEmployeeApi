using Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IntegrationTests
{
    public class EmployeesApiTests : IClassFixture<ApiFactory>
    {
        private const string EmployeesUrl = "/api/v1/employees";

        private readonly ApiFactory _factory;

        public EmployeesApiTests(ApiFactory factory) => _factory = factory;

        [Fact]
        public async Task CreateThenGet_RoundTripsEmployeeIncludingCustomData()
        {
            var client = ClientFor(SeedData.TenantAId);
            var email = NewEmail();

            var createResponse = await client.PostAsJsonAsync(EmployeesUrl, NewEmployee(email));
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            var id = (await ReadData(createResponse)).GetProperty("id").GetGuid();

            var getResponse = await client.GetAsync($"{EmployeesUrl}/{id}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var fetched = await ReadData(getResponse);

            Assert.Equal(email, fetched.GetProperty("email").GetString());
            Assert.Equal(SeedData.TenantAId, fetched.GetProperty("tenantId").GetGuid());
            Assert.Equal(3, fetched.GetProperty("customData").GetProperty("grade").GetInt32());
        }

        [Fact]
        public async Task EmployeeOfTenantA_IsInvisibleToTenantB()
        {
            var clientA = ClientFor(SeedData.TenantAId);
            var clientB = ClientFor(SeedData.TenantBId);

            var createResponse = await clientA.PostAsJsonAsync(EmployeesUrl, NewEmployee(NewEmail()));
            var id = (await ReadData(createResponse)).GetProperty("id").GetGuid();

            var getAsB = await clientB.GetAsync($"{EmployeesUrl}/{id}");
            Assert.Equal(HttpStatusCode.NotFound, getAsB.StatusCode);

            var listAsB = await ReadData(await clientB.GetAsync($"{EmployeesUrl}?pageSize=100"));
            Assert.DoesNotContain(listAsB.EnumerateArray(), e => e.GetProperty("id").GetGuid() == id);

            var listAsA = await ReadData(await clientA.GetAsync($"{EmployeesUrl}?pageSize=100"));
            Assert.Contains(listAsA.EnumerateArray(), e => e.GetProperty("id").GetGuid() == id);
        }

        [Fact]
        public async Task Create_WithDuplicateEmail_ReturnsConflictOnlyWithinSameTenant()
        {
            var clientA = ClientFor(SeedData.TenantAId);
            var clientB = ClientFor(SeedData.TenantBId);
            var email = NewEmail();

            var first = await clientA.PostAsJsonAsync(EmployeesUrl, NewEmployee(email));
            var duplicate = await clientA.PostAsJsonAsync(EmployeesUrl, NewEmployee(email));
            var otherTenant = await clientB.PostAsJsonAsync(EmployeesUrl, NewEmployee(email));

            Assert.Equal(HttpStatusCode.Created, first.StatusCode);
            Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
            Assert.Equal(HttpStatusCode.Created, otherTenant.StatusCode);
        }

        [Fact]
        public async Task Request_WithoutTenantHeader_ReturnsBadRequestEnvelope()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync(EmployeesUrl);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("InvalidTenantHeader", body.GetProperty("error").GetProperty("code").GetString());
        }

        private HttpClient ClientFor(Guid tenantId)
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());
            return client;
        }

        private static string NewEmail() => $"{Guid.NewGuid():N}@test.com";

        private static object NewEmployee(string email) => new
        {
            firstName = "Ali",
            lastName = "Hassan",
            email,
            department = "IT",
            customData = new { grade = 3 }
        };

        private static async Task<JsonElement> ReadData(HttpResponseMessage response)
        {
            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            return body.GetProperty("data");
        }
    }
}
