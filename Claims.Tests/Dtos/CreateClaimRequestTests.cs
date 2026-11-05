using System.ComponentModel.DataAnnotations;
using Claims.Dtos;
using Claims.Models;
using Xunit;

namespace Claims.Tests.Dtos;

public class CreateClaimRequestTests
{
    [Fact]
    public void DamageCost_of_exactly_100000_is_valid()
    {
        var errors = Validate(Request(damageCost: 100_000m));

        Assert.DoesNotContain(nameof(CreateClaimRequest.DamageCost), errors);
    }

    [Fact]
    public void DamageCost_above_100000_is_invalid()
    {
        var errors = Validate(Request(damageCost: 100_000.01m));

        Assert.Contains(nameof(CreateClaimRequest.DamageCost), errors);
    }

    private static CreateClaimRequest Request(decimal damageCost) => new()
    {
        CoverId = "cover-id",
        Created = DateTime.UtcNow,
        Name = "name",
        Type = ClaimType.Fire,
        DamageCost = damageCost
    };

    // Returns the names of the fields that failed validation.
    private static List<string> Validate(CreateClaimRequest request) =>
        request.Validate(new ValidationContext(request))
            .SelectMany(result => result.MemberNames)
            .ToList();
}
