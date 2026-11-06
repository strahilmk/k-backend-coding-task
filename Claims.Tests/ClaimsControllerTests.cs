using System.Net;
using System.Net.Http.Json;
using Claims.Dtos;
using Claims.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Claims.Tests
{
    [Collection(ApiCollection.Name)]
    public class ClaimsControllerTests
    {
        private readonly HttpClient _client;
        private readonly CancellationToken _cancellation = TestContext.Current.CancellationToken;

        public ClaimsControllerTests(WebApplicationFactory<Program> application)
        {
            _client = application.CreateClient();
        }

        [Fact]
        public async Task Get_Claims()
        {
            var response = await _client.GetAsync("/Claims", _cancellation);

            response.EnsureSuccessStatusCode();
            var claims = await response.Content.ReadFromJsonAsync<List<ClaimResponse>>(ApiCollection.Json, _cancellation);
            Assert.NotNull(claims);
        }

        [Fact]
        public async Task Created_claim_can_be_read_back()
        {
            var start = DateTime.UtcNow.Date.AddDays(1);
            var cover = await CreateCoverAsync(start, start.AddDays(30));

            var created = start.AddDays(5);
            var claimResponse = await PostClaimAsync(cover.Id, created, damageCost: 1000m);
            Assert.Equal(HttpStatusCode.Created, claimResponse.StatusCode);
            Assert.NotNull(claimResponse.Headers.Location);

            var claim = await _client.GetFromJsonAsync<ClaimResponse>(claimResponse.Headers.Location, ApiCollection.Json, _cancellation);
            Assert.NotNull(claim);
            Assert.False(string.IsNullOrEmpty(claim.Id));
            Assert.Equal(cover.Id, claim.CoverId);
            Assert.Equal(created, claim.Created);
            Assert.Equal(DateTimeKind.Utc, claim.Created.Kind);
            Assert.Equal("Engine fire", claim.Name);
            Assert.Equal(ClaimType.Fire, claim.Type);
            Assert.Equal(1000m, claim.DamageCost);

            var claims = await _client.GetFromJsonAsync<List<ClaimResponse>>("/Claims", ApiCollection.Json, _cancellation);
            Assert.Contains(claims!, c => c.Id == claim.Id);
        }

        [Fact]
        public async Task Get_unknown_claim_returns_404()
        {
            var response = await _client.GetAsync($"/Claims/{Guid.NewGuid()}", _cancellation);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Claim_with_damage_cost_above_100000_returns_400()
        {
            var start = DateTime.UtcNow.Date.AddDays(1);
            var cover = await CreateCoverAsync(start, start.AddDays(30));

            var response = await PostClaimAsync(cover.Id, start, damageCost: 100_000.01m);

            await AssertValidationErrorAsync(response, nameof(CreateClaimRequest.DamageCost));
        }

        [Fact]
        public async Task Claim_for_an_unknown_cover_returns_400()
        {
            var response = await PostClaimAsync(Guid.NewGuid().ToString(), DateTime.UtcNow.Date, damageCost: 1000m);

            await AssertValidationErrorAsync(response, nameof(CreateClaimRequest.CoverId));
        }

        [Fact]
        public async Task Claim_created_outside_the_cover_period_returns_400()
        {
            var start = DateTime.UtcNow.Date.AddDays(1);
            var cover = await CreateCoverAsync(start, start.AddDays(30));

            var response = await PostClaimAsync(cover.Id, start.AddDays(31), damageCost: 1000m);

            await AssertValidationErrorAsync(response, nameof(CreateClaimRequest.Created));
        }

        private async Task<CoverResponse> CreateCoverAsync(DateTime start, DateTime end)
        {
            var response = await _client.PostAsJsonAsync("/Covers", new { startDate = start, endDate = end, type = "Yacht" }, _cancellation);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return (await response.Content.ReadFromJsonAsync<CoverResponse>(ApiCollection.Json, _cancellation))!;
        }

        private Task<HttpResponseMessage> PostClaimAsync(string coverId, DateTime created, decimal damageCost) =>
            _client.PostAsJsonAsync("/Claims", new
            {
                coverId,
                created,
                name = "Engine fire",
                type = "Fire",
                damageCost
            }, _cancellation);

        private async Task AssertValidationErrorAsync(HttpResponseMessage response, string field)
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(ApiCollection.Json, _cancellation);
            Assert.Contains(field, problem!.Errors.Keys);
        }
    }
}
