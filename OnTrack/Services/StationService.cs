using Microsoft.EntityFrameworkCore;
using OnTrack.Data;
using OnTrack.Interfaces;
using OnTrack.Models;
using OnTrack.Repositories;

namespace OnTrack.Services;

public class StationService : IStationService
{
    private readonly IGenericRepository<Station> _repo;   // basic CRUD
    private readonly ApplicationDbContext _db;            // filtered queries

    public StationService(IGenericRepository<Station> repo, ApplicationDbContext db)
    {
        _repo = repo;
        _db = db;
    }

    public async Task<List<Station>> GetAllAsync(string? search = null)
    {
        var query = _db.Stations.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(s => s.StationName.Contains(search) || s.City.Contains(search));

        return await query.OrderBy(s => s.City).ThenBy(s => s.StationName).ToListAsync();
    }

    public async Task<Station?> GetByIdAsync(int id) =>
        await _repo.GetByIdAsync(id);

    public async Task<bool> ExistsAsync(string name, string city, int? excludeId = null) =>
        await _db.Stations.AsNoTracking().AnyAsync(s =>
            s.StationName == name && s.City == city &&
            (excludeId == null || s.StationId != excludeId));

    public async Task<bool> IsInUseAsync(int id)
    {
        var (stops, tickets) = await GetUsageAsync(id);
        return stops > 0 || tickets > 0;
    }

    public async Task<(int stops, int tickets)> GetUsageAsync(int id)
    {
        var stops = await _db.TripStops.CountAsync(t => t.StationId == id);
        var tickets = await _db.Tickets
            .CountAsync(t => t.FromStationId == id || t.ToStationId == id);
        return (stops, tickets);
    }

    public async Task CreateAsync(Station station)
    {
        await _repo.AddAsync(station);
        await _repo.SaveChangesAsync();
    }

    public async Task UpdateAsync(Station station)
    {
        _repo.Update(station);
        await _repo.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var station = await _repo.GetByIdAsync(id);
        if (station == null) return;
        _repo.Remove(station);
        await _repo.SaveChangesAsync();
    }
}