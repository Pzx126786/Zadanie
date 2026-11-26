using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
namespace BeFit.Models;
public class ExerciseType
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Podaj nazwę ćwiczenia."), StringLength(100, MinimumLength = 2, ErrorMessage = "Pole {0} musi mieć od 2 do 100 znaków."), Display(Name = "Nazwa ćwiczenia")] public string Name { get; set; } = "";
    [StringLength(500, ErrorMessage = "Opis może mieć najwyżej 500 znaków."), Display(Name = "Opis")] public string? Description { get; set; }
}
public class TrainingSession : IValidatableObject
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Pole {0} jest wymagane."), StringLength(100, MinimumLength = 2, ErrorMessage = "Pole {0} musi mieć od 2 do 100 znaków."), Display(Name = "Nazwa treningu")] public string Name { get; set; } = "";
    [Required(ErrorMessage = "Pole {0} jest wymagane."), Display(Name = "Rozpoczęcie")] public DateTime Start { get; set; } = DateTime.Now.AddHours(-1);
    [Required(ErrorMessage = "Pole {0} jest wymagane."), Display(Name = "Zakończenie")] public DateTime End { get; set; } = DateTime.Now;
    [StringLength(2000, ErrorMessage = "Notatki mogą mieć najwyżej 2000 znaków."), Display(Name = "Notatki")] public string? Notes { get; set; }
    public string UserId { get; set; } = "";
    public IdentityUser? User { get; set; }
    public List<PerformedExercise> Exercises { get; set; } = [];
    public IEnumerable<ValidationResult> Validate(ValidationContext c)
    {
        if (Start < new DateTime(2000, 1, 1) || Start > DateTime.Now.AddYears(1)) yield return new("Rozpoczęcie: od roku 2000 do roku naprzód.", [nameof(Start)]);
        if (End <= Start || End - Start > TimeSpan.FromDays(1)) yield return new("Koniec musi być po rozpoczęciu, najpóźniej 24 godziny później.", [nameof(End)]);
    }
}
public class PerformedExercise
{
    public int Id { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Wybierz pozycję z listy."), Display(Name = "Trening")] public int TrainingSessionId { get; set; }
    public TrainingSession? TrainingSession { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Wybierz pozycję z listy."), Display(Name = "Ćwiczenie")] public int ExerciseTypeId { get; set; }
    public ExerciseType? ExerciseType { get; set; }
    [Range(0, 1000, ErrorMessage = "Obciążenie musi wynosić od 0 do 1000 kg."), Display(Name = "Obciążenie (kg)")] public decimal Weight { get; set; }
    [Range(1, 100, ErrorMessage = "Liczba serii musi wynosić od 1 do 100."), Display(Name = "Liczba serii")] public int Sets { get; set; } = 3;
    [Range(1, 1000, ErrorMessage = "Liczba powtórzeń musi wynosić od 1 do 1000."), Display(Name = "Powtórzenia w serii")] public int Repetitions { get; set; } = 10;
}
public class RegisterModel
{
    [Required(ErrorMessage = "Pole {0} jest wymagane."), EmailAddress(ErrorMessage = "Podaj poprawny adres e-mail."), StringLength(254, ErrorMessage = "E-mail może mieć najwyżej 254 znaki."), Display(Name = "Adres e-mail")] public string Email { get; set; } = "";
    [Required(ErrorMessage = "Pole {0} jest wymagane."), StringLength(128, MinimumLength = 10, ErrorMessage = "Hasło musi mieć od 10 do 128 znaków."), DataType(DataType.Password), Display(Name = "Hasło")] public string Password { get; set; } = "";
    [Required(ErrorMessage = "Pole {0} jest wymagane."), Compare(nameof(Password), ErrorMessage = "Hasła muszą być identyczne."), DataType(DataType.Password), Display(Name = "Powtórz hasło")] public string ConfirmPassword { get; set; } = "";
}
public class LoginModel
{
    [Required(ErrorMessage = "Pole {0} jest wymagane."), EmailAddress(ErrorMessage = "Podaj poprawny adres e-mail."), StringLength(254, ErrorMessage = "E-mail może mieć najwyżej 254 znaki."), Display(Name = "Adres e-mail")] public string Email { get; set; } = "";
    [Required(ErrorMessage = "Pole {0} jest wymagane."), StringLength(128, ErrorMessage = "Hasło może mieć najwyżej 128 znaków."), DataType(DataType.Password), Display(Name = "Hasło")] public string Password { get; set; } = "";
}
public record StatisticsRow(string Name, int Count, long TotalRepetitions, decimal? AverageWeight, decimal? MaxWeight);
