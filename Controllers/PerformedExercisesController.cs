using BeFit.Data;
using BeFit.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace BeFit.Controllers;
[Authorize]
public class PerformedExercisesController(ApplicationDbContext db) : Controller
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private IQueryable<PerformedExercise> Mine => db.PerformedExercises.Where(x => x.TrainingSession!.UserId == UserId);
    private IQueryable<PerformedExercise> Full => Mine.Include(x => x.ExerciseType).Include(x => x.TrainingSession);
    public async Task<IActionResult> Index() => View(await Full.OrderByDescending(x => x.TrainingSession!.Start).ToListAsync());
    public async Task<IActionResult> Details(int id) => await Full.FirstOrDefaultAsync(x => x.Id == id) is { } item ? View(item) : NotFound();
    private async Task Choices() { ViewBag.Types = new SelectList(await db.ExerciseTypes.OrderBy(x => x.Name).ToListAsync(), "Id", "Name"); ViewBag.Sessions = new SelectList(await db.TrainingSessions.Where(x => x.UserId == UserId).OrderByDescending(x => x.Start).ToListAsync(), "Id", "Name"); }
    public async Task<IActionResult> Create(int? sessionId) { if (sessionId.HasValue && !await db.TrainingSessions.AnyAsync(x => x.Id == sessionId && x.UserId == UserId)) return NotFound(); await Choices(); return View("Edit", new PerformedExercise { TrainingSessionId = sessionId ?? 0 }); }
    [HttpPost] public Task<IActionResult> Create([Bind("TrainingSessionId,ExerciseTypeId,Weight,Sets,Repetitions")] PerformedExercise model) => Save(null, model);
    public async Task<IActionResult> Edit(int id) { var item = await Mine.FirstOrDefaultAsync(x => x.Id == id); if (item == null) return NotFound(); await Choices(); return View(item); }
    [HttpPost] public Task<IActionResult> Edit(int id, [Bind("TrainingSessionId,ExerciseTypeId,Weight,Sets,Repetitions")] PerformedExercise model) => Save(id, model);
    private async Task<IActionResult> Save(int? id, PerformedExercise model)
    {
        var item = id.HasValue ? await Mine.FirstOrDefaultAsync(x => x.Id == id) : new PerformedExercise(); if (item == null) return NotFound();
        if (!await db.TrainingSessions.AnyAsync(x => x.Id == model.TrainingSessionId && x.UserId == UserId)) return NotFound();
        if (!await db.ExerciseTypes.AnyAsync(x => x.Id == model.ExerciseTypeId)) ModelState.AddModelError("ExerciseTypeId", "Wybierz istniejące ćwiczenie.");
        if (decimal.Round(model.Weight, 2) != model.Weight) ModelState.AddModelError("Weight", "Podaj najwyżej dwa miejsca po przecinku.");
        model.Id = id ?? 0; if (!ModelState.IsValid) { await Choices(); return View("Edit", model); }
        item.TrainingSessionId = model.TrainingSessionId; item.ExerciseTypeId = model.ExerciseTypeId; item.Weight = model.Weight; item.Sets = model.Sets; item.Repetitions = model.Repetitions; if (!id.HasValue) db.Add(item);
        await db.SaveChangesAsync(); return RedirectToAction("Details", "TrainingSessions", new { id = item.TrainingSessionId });
    }
    public async Task<IActionResult> Delete(int id) => await Full.FirstOrDefaultAsync(x => x.Id == id) is { } item ? View(item) : NotFound();
    [HttpPost, ActionName("Delete")] public async Task<IActionResult> DeleteConfirmed(int id) { var item = await Mine.FirstOrDefaultAsync(x => x.Id == id); if (item == null) return NotFound(); var session = item.TrainingSessionId; db.Remove(item); await db.SaveChangesAsync(); return RedirectToAction("Details", "TrainingSessions", new { id = session }); }
}
