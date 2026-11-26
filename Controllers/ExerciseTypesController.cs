using BeFit.Data;
using BeFit.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace BeFit.Controllers;
public class ExerciseTypesController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index() => View(await db.ExerciseTypes.OrderBy(x => x.Name).ToListAsync());
    public async Task<IActionResult> Details(int id) => await db.ExerciseTypes.FindAsync(id) is { } item ? View(item) : NotFound();
    [Authorize(Roles = "Administrator")] public IActionResult Create() => View("Edit", new ExerciseType());
    [Authorize(Roles = "Administrator"), HttpPost] public Task<IActionResult> Create([Bind("Name,Description")] ExerciseType model) => Save(null, model);
    [Authorize(Roles = "Administrator")] public async Task<IActionResult> Edit(int id) => await db.ExerciseTypes.FindAsync(id) is { } item ? View(item) : NotFound();
    [Authorize(Roles = "Administrator"), HttpPost] public Task<IActionResult> Edit(int id, [Bind("Name,Description")] ExerciseType model) => Save(id, model);
    private async Task<IActionResult> Save(int? id, ExerciseType model)
    {
        var item = id.HasValue ? await db.ExerciseTypes.FindAsync(id) : new ExerciseType(); if (item == null) return NotFound();
        if (await db.ExerciseTypes.AnyAsync(x => x.Name == model.Name && x.Id != (id ?? 0))) ModelState.AddModelError("Name", "Ta nazwa jest już zajęta.");
        model.Id = id ?? 0; if (!ModelState.IsValid) return View("Edit", model);
        item.Name = model.Name; item.Description = model.Description; if (!id.HasValue) db.Add(item);
        try { await db.SaveChangesAsync(); } catch (DbUpdateException) { ModelState.AddModelError("", "Nie udało się zapisać. Nazwa mogła zostać zajęta."); return View("Edit", model); }
        return RedirectToAction(nameof(Index));
    }
    [Authorize(Roles = "Administrator")] public async Task<IActionResult> Delete(int id) => await db.ExerciseTypes.FindAsync(id) is { } item ? View(item) : NotFound();
    [Authorize(Roles = "Administrator"), HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await db.ExerciseTypes.FindAsync(id); if (item == null) return NotFound();
        if (await db.PerformedExercises.AnyAsync(x => x.ExerciseTypeId == id)) { ModelState.AddModelError("", "Ćwiczenie jest używane w treningach i nie może zostać usunięte."); return View(item); }
        db.Remove(item); try { await db.SaveChangesAsync(); } catch (DbUpdateException) { ModelState.AddModelError("", "Ćwiczenie jest używane w treningach."); return View(item); }
        return RedirectToAction(nameof(Index));
    }
}
