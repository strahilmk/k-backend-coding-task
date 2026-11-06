using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Claims.Tests
{
    /// <summary>
    /// Tests in this collection go through Program.cs, so Docker must be running. They share
    /// one app, and so one database, because each app starts its own containers.
    /// </summary>
    [CollectionDefinition(Name)]
    public class ApiCollection : ICollectionFixture<WebApplicationFactory<Program>>
    {
        public const string Name = "Api";

        /// <summary>Matches the API: camelCase names and enums as strings.</summary>
        public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() }
        };
    }
}
