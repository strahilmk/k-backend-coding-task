using System.ComponentModel.DataAnnotations;
using Claims.Models;

namespace Claims.Dtos;

public record CreateClaimRequest : IValidatableObject
{
    public const decimal MaxDamageCost = 100_000m;

    public required string CoverId { get; init; }
    public DateTime Created { get; init; }
    public required string Name { get; init; }
    public ClaimType Type { get; init; }
    public decimal DamageCost { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DamageCost > MaxDamageCost)
        {
            yield return new ValidationResult(
                $"DamageCost cannot exceed {MaxDamageCost}.", [nameof(DamageCost)]);
        }
    }
}
