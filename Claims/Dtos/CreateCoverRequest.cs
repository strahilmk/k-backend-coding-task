using Claims.Models;

namespace Claims.Dtos;

public record CreateCoverRequest
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public CoverType Type { get; init; }
}
