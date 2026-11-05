using Claims.Models;

namespace Claims.Dtos;

public record CreateClaimRequest
{
    public required string CoverId { get; init; }
    public DateTime Created { get; init; }
    public required string Name { get; init; }
    public ClaimType Type { get; init; }
    public decimal DamageCost { get; init; }
}
