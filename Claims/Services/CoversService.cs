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
        var multiplier = 1.3m;
        if (coverType == CoverType.Yacht)
        {
            multiplier = 1.1m;
        }

        if (coverType == CoverType.PassengerShip)
        {
            multiplier = 1.2m;
        }

        if (coverType == CoverType.Tanker)
        {
            multiplier = 1.5m;
        }

        var premiumPerDay = 1250 * multiplier;
        var insuranceLength = (endDate - startDate).TotalDays;
        var totalPremium = 0m;

        for (var i = 0; i < insuranceLength; i++)
        {
            if (i < 30) totalPremium += premiumPerDay;
            if (i < 180 && coverType == CoverType.Yacht) totalPremium += premiumPerDay - premiumPerDay * 0.05m;
            else if (i < 180) totalPremium += premiumPerDay - premiumPerDay * 0.02m;
            if (i < 365 && coverType != CoverType.Yacht) totalPremium += premiumPerDay - premiumPerDay * 0.03m;
            else if (i < 365) totalPremium += premiumPerDay - premiumPerDay * 0.08m;
        }

        return totalPremium;
    }
}
