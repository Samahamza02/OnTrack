using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnTrack.Interfaces;
using OnTrack.Models;
using OnTrack.Services;

namespace OnTrack.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class StationsController : Controller
{
    private readonly IStationService _service;
    public StationsController(IStationService service) => _service = service;

    public async Task<IActionResult> Index(string? search)
    {
        ViewBag.Search = search;
        return View(await _service.GetAllAsync(search));
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var station = await _service.GetByIdAsync(id.Value);
        if (station == null) return NotFound();

        var (stops, tickets) = await _service.GetUsageAsync(id.Value);
        ViewBag.StopsCount = stops;
        ViewBag.TicketsCount = tickets;
        return View(station);
    }

    [HttpGet]
    public IActionResult Create() => View(new Station());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("StationName,City,Latitude,Longitude")] Station station)
    {
        if (await _service.ExistsAsync(station.StationName, station.City))
            ModelState.AddModelError(nameof(station.StationName), "This station already exists in this city.");

        if (!ModelState.IsValid) return View(station);

        await _service.CreateAsync(station);
        TempData["Success"] = "Station created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var station = await _service.GetByIdAsync(id.Value);
        return station == null ? NotFound() : View(station);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id, [Bind("StationId,StationName,City,Latitude,Longitude")] Station station)
    {
        if (id != station.StationId) return NotFound();

        if (await _service.ExistsAsync(station.StationName, station.City, id))
            ModelState.AddModelError(nameof(station.StationName), "This station already exists in this city.");

        if (!ModelState.IsValid) return View(station);

        await _service.UpdateAsync(station);
        TempData["Success"] = "Station updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var station = await _service.GetByIdAsync(id.Value);
        if (station == null) return NotFound();

        ViewBag.InUse = await _service.IsInUseAsync(id.Value);
        return View(station);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (await _service.IsInUseAsync(id))
        {
            TempData["Error"] = "This station is used by trip stops or tickets, so it can't be deleted.";
            return RedirectToAction(nameof(Index));
        }

        await _service.DeleteAsync(id);
        TempData["Success"] = "Station deleted.";
        return RedirectToAction(nameof(Index));
    }
}
