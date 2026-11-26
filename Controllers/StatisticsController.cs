using BeFit.Data;
using BeFit.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace BeFit.Controllers;
[Authorize]
public class StatisticsController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var now = DateTime.Now; var since = now.AddDays(-28); var user = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var entries = await db.PerformedExercises.Where(x => x.TrainingSession!.UserId == user && x.TrainingSession.Start >= since && x.TrainingSession.Start <= now).AsNoTracking().ToListAsync();
        var types = await db.ExerciseTypes.OrderBy(x => x.Name).AsNoTracking().ToListAsync(); ViewBag.Since = since; ViewBag.Now = now;
        return View(types.Select(t => { var items = entries.Where(e => e.ExerciseTypeId == t.Id).ToList(); return new StatisticsRow(t.Name, items.Count, items.Sum(e => (long)e.Sets * e.Repetitions), items.Count == 0 ? null : items.Average(e => e.Weight), items.Count == 0 ? null : items.Max(e => e.Weight)); }).ToList());
    }
}
