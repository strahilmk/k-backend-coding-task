using Claims.Models;

namespace Claims.Dtos;

public record CoverResponse
{
    public required string Id { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public CoverType Type { get; init; }
    public decimal Premium { get; init; }

    public static CoverResponse From(Cover cover) => new()
    {
        Id = cover.Id,
        StartDate = cover.StartDate,
        EndDate = cover.EndDate,
        Type = cover.Type,
        Premium = cover.Premium
    };
}
