using BeFit.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace BeFit.Data;
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<ExerciseType> ExerciseTypes => Set<ExerciseType>();
    public DbSet<TrainingSession> TrainingSessions => Set<TrainingSession>();
    public DbSet<PerformedExercise> PerformedExercises => Set<PerformedExercise>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<ExerciseType>().HasIndex(x => x.Name).IsUnique();
        b.Entity<TrainingSession>().HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).IsRequired();
        b.Entity<PerformedExercise>().HasOne(x => x.TrainingSession).WithMany(x => x.Exercises).HasForeignKey(x => x.TrainingSessionId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<PerformedExercise>().HasOne(x => x.ExerciseType).WithMany().HasForeignKey(x => x.ExerciseTypeId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<PerformedExercise>().ToTable(t =>
        {
            t.HasCheckConstraint("CK_Weight", "CAST(Weight AS REAL) BETWEEN 0 AND 1000");
            t.HasCheckConstraint("CK_Sets", "Sets BETWEEN 1 AND 100");
            t.HasCheckConstraint("CK_Repetitions", "Repetitions BETWEEN 1 AND 1000");
        });
    }
}
