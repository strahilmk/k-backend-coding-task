using Claims.Auditing;
using Claims.Dtos;
using Claims.Models;
using Claims.Repositories;

namespace Claims.Services;

public class CoversService : ICoversService
{
    private readonly ICoversRepository _coversRepository;
    private readonly IAuditer _auditer;

    public CoversService(ICoversRepository coversRepository, IAuditer auditer)
    {
        _coversRepository = coversRepository;
        _auditer = auditer;
    }

    public async Task<IEnumerable<CoverResponse>> GetAllAsync()
    {
        var covers = await _coversRepository.GetAllAsync();
        return covers.Select(CoverResponse.From);
    }

    public async Task<CoverResponse?> GetByIdAsync(string id)
    {
        var cover = await _coversRepository.GetByIdAsync(id);
        return cover is null ? null : CoverResponse.From(cover);
    }

    public async Task<CoverResponse> CreateAsync(CreateCoverRequest request)
    {
        var cover = new Cover
        {
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Type = request.Type,
            Premium = ComputePremium(request.StartDate, request.EndDate, request.Type)
        };

        await _coversRepository.AddAsync(cover);
        _auditer.AuditCover(cover.Id, HttpRequestType.Post);
        return CoverResponse.From(cover);
    }

    public async Task DeleteAsync(string id)
    {
        _auditer.AuditCover(id, HttpRequestType.Delete);
        await _coversRepository.DeleteAsync(id);
    }

    public decimal ComputePremium(DateTime startDate, DateTime endDate, CoverType coverType)
    {
        var baseDayRate = 1250m;
        var premiumPerDay = baseDayRate * GetMultiplierForCoverType(coverType);
        // Only whole days count, so a partial last day is not charged.
        var insuranceLength = (int)(endDate - startDate).TotalDays;

        var totalPremium = ComputePremiumForDaysWithDiscount(premiumPerDay, insuranceLength, coverType);

        return totalPremium;
    }

    private static decimal GetDiscountForDay(int day, CoverType coverType)
    {
        var isYacht = coverType == CoverType.Yacht;
        if (day < 30) return 0m;
        if (day < 180) return isYacht ? 0.05m : 0.02m;
        return isYacht ? 0.08m : 0.03m;
    }

    private static decimal ComputePremiumForDaysWithDiscount(decimal premiumPerDay, int days, CoverType coverType)
    {
        var total = 0m;
        for (var i = 0; i < days; i++)
        {
            var discount = GetDiscountForDay(i, coverType);
            total += premiumPerDay - premiumPerDay * discount;
        }
        return total;
    }

    private static decimal GetMultiplierForCoverType(CoverType coverType) => coverType switch
    {
        CoverType.Yacht => 1.1m,
        CoverType.PassengerShip => 1.2m,
        CoverType.Tanker => 1.5m,
        _ => 1.3m
    };
}
