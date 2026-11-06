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
    public class CoversControllerTests
    {
        private readonly HttpClient _client;
        private readonly CancellationToken _cancellation = TestContext.Current.CancellationToken;

        public CoversControllerTests(WebApplicationFactory<Program> application)
        {
            _client = application.CreateClient();
        }

        [Fact]
        public async Task Get_Covers()
        {
            var response = await _client.GetAsync("/Covers", _cancellation);

            response.EnsureSuccessStatusCode();
            var covers = await response.Content.ReadFromJsonAsync<List<CoverResponse>>(ApiCollection.Json, _cancellation);
            Assert.NotNull(covers);
        }

        [Fact]
        public async Task Created_cover_can_be_read_back()
        {
            var start = DateTime.UtcNow.Date.AddDays(1);
            var end = start.AddDays(30);

            var coverResponse = await PostCoverAsync(start, end);
            Assert.Equal(HttpStatusCode.Created, coverResponse.StatusCode);
            Assert.NotNull(coverResponse.Headers.Location);

            var cover = await _client.GetFromJsonAsync<CoverResponse>(coverResponse.Headers.Location, ApiCollection.Json, _cancellation);
            Assert.NotNull(cover);
            Assert.False(string.IsNullOrEmpty(cover.Id));
            Assert.Equal(start, cover.StartDate);
            Assert.Equal(DateTimeKind.Utc, cover.StartDate.Kind);
            Assert.Equal(end, cover.EndDate);
            Assert.Equal(CoverType.Tanker, cover.Type);

            // The stored premium matches the compute endpoint. The formula itself is tested in Task 5.
            var computeResponse = await _client.PostAsync(
                $"/Covers/compute?startDate={start:yyyy-MM-dd}&endDate={end:yyyy-MM-dd}&coverType=Tanker", null, _cancellation);
            var premium = await computeResponse.Content.ReadFromJsonAsync<decimal>(ApiCollection.Json, _cancellation);
            Assert.Equal(premium, cover.Premium);

            var covers = await _client.GetFromJsonAsync<List<CoverResponse>>("/Covers", ApiCollection.Json, _cancellation);
            Assert.Contains(covers!, c => c.Id == cover.Id);
        }

        [Fact]
        public async Task Get_unknown_cover_returns_404()
        {
            var response = await _client.GetAsync($"/Covers/{Guid.NewGuid()}", _cancellation);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Cover_starting_in_the_past_returns_400()
        {
            var yesterday = DateTime.UtcNow.Date.AddDays(-1);

            var response = await PostCoverAsync(yesterday, yesterday.AddDays(30));

            await AssertValidationErrorAsync(response, nameof(CreateCoverRequest.StartDate));
        }

        [Fact]
        public async Task Cover_longer_than_one_year_returns_400()
        {
            var start = DateTime.UtcNow.Date.AddDays(1);

            var response = await PostCoverAsync(start, start.AddYears(1).AddDays(1));

            await AssertValidationErrorAsync(response, nameof(CreateCoverRequest.EndDate));
        }

        private Task<HttpResponseMessage> PostCoverAsync(DateTime start, DateTime end) =>
            _client.PostAsJsonAsync("/Covers", new { startDate = start, endDate = end, type = "Tanker" }, _cancellation);

        private async Task AssertValidationErrorAsync(HttpResponseMessage response, string field)
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(ApiCollection.Json, _cancellation);
            Assert.Contains(field, problem!.Errors.Keys);
        }
    }
}
