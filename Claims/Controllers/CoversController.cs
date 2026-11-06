using Claims.Dtos;
using Claims.Models;
using Claims.Services;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers;

[ApiController]
[Route("[controller]")]
public class CoversController : ControllerBase
{
    private readonly ICoversService _coversService;

    public CoversController(ICoversService coversService)
    {
        _coversService = coversService;
    }

    [HttpPost("compute")]
    public ActionResult<decimal> ComputePremium(DateTime startDate, DateTime endDate, CoverType coverType)
    {
        return _coversService.ComputePremium(startDate, endDate, coverType);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CoverResponse>>> GetAsync()
    {
        return Ok(await _coversService.GetAllAsync());
    }

    [HttpGet("{id}", Name = "GetCover")]
    public async Task<ActionResult<CoverResponse>> GetAsync(string id)
    {
        var cover = await _coversService.GetByIdAsync(id);
        return cover is null ? NotFound() : cover;
    }

    [HttpPost]
    public async Task<ActionResult<CoverResponse>> CreateAsync(CreateCoverRequest request)
    {
        var created = await _coversService.CreateAsync(request);
        return CreatedAtRoute("GetCover", new { id = created.Id }, created);
    }

    [HttpDelete("{id}")]
    public async Task DeleteAsync(string id)
    {
        await _coversService.DeleteAsync(id);
    }
}
