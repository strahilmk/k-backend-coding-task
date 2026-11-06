using Claims.Models;

namespace Claims.Dtos;

public record ClaimResponse
{
    public required string Id { get; init; }
    public required string CoverId { get; init; }
    public DateTime Created { get; init; }
    public required string Name { get; init; }
    public ClaimType Type { get; init; }
    public decimal DamageCost { get; init; }

    public static ClaimResponse From(Claim claim) => new()
    {
        Id = claim.Id,
        CoverId = claim.CoverId,
        Created = claim.Created,
        Name = claim.Name,
        Type = claim.Type,
        DamageCost = claim.DamageCost
    };
}
