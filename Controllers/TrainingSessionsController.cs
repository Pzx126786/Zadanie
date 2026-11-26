using BeFit.Data;
using BeFit.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace BeFit.Controllers;
[Authorize]
public class TrainingSessionsController(ApplicationDbContext db) : Controller
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private IQueryable<TrainingSession> Mine => db.TrainingSessions.Where(x => x.UserId == UserId);
    public async Task<IActionResult> Index() => View(await Mine.Include(x => x.Exercises).OrderByDescending(x => x.Start).ToListAsync());
    public async Task<IActionResult> Details(int id) => await Mine.Include(x => x.Exercises).ThenInclude(x => x.ExerciseType).FirstOrDefaultAsync(x => x.Id == id) is { } item ? View(item) : NotFound();
    public IActionResult Create() => View("Edit", new TrainingSession());
    [HttpPost] public Task<IActionResult> Create([Bind("Name,Start,End,Notes")] TrainingSession model) => Save(null, model);
    public async Task<IActionResult> Edit(int id) => await Mine.FirstOrDefaultAsync(x => x.Id == id) is { } item ? View(item) : NotFound();
    [HttpPost] public Task<IActionResult> Edit(int id, [Bind("Name,Start,End,Notes")] TrainingSession model) => Save(id, model);
    private async Task<IActionResult> Save(int? id, TrainingSession model)
    {
        var item = id.HasValue ? await Mine.FirstOrDefaultAsync(x => x.Id == id) : new TrainingSession { UserId = UserId }; if (item == null) return NotFound();
        model.Id = id ?? 0; if (!ModelState.IsValid) return View("Edit", model);
        item.Name = model.Name; item.Start = model.Start; item.End = model.End; item.Notes = model.Notes; if (!id.HasValue) db.Add(item);
        await db.SaveChangesAsync(); return RedirectToAction(nameof(Details), new { id = item.Id });
    }
    public async Task<IActionResult> Delete(int id) => await Mine.FirstOrDefaultAsync(x => x.Id == id) is { } item ? View(item) : NotFound();
    [HttpPost, ActionName("Delete")] public async Task<IActionResult> DeleteConfirmed(int id) { var item = await Mine.FirstOrDefaultAsync(x => x.Id == id); if (item == null) return NotFound(); db.Remove(item); await db.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
}
