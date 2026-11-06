using System.ComponentModel.DataAnnotations;
using Claims.Models;

namespace Claims.Dtos;

public record CreateCoverRequest : IValidatableObject
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public CoverType Type { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // TimeProvider comes from DI, so tests can fix "today".
        var today = (validationContext.GetService<TimeProvider>() ?? TimeProvider.System).GetUtcNow().UtcDateTime.Date;

        // Compared by date, so a cover can start today.
        if (StartDate.Date < today)
        {
            yield return new ValidationResult(
                "StartDate cannot be in the past.", [nameof(StartDate)]);
        }

        // A calendar year, so a period that crosses 29 February can be 366 days.
        if (EndDate > StartDate.AddYears(1))
        {
            yield return new ValidationResult(
                "The insurance period cannot exceed 1 year.", [nameof(EndDate)]);
        }
    }
}
