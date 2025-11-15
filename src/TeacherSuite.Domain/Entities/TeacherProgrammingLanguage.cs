
using TeacherSuite.Domain.Enums;

namespace TeacherSuite.Domain.Entities;

public class TeacherProgrammingLanguage
{
    public Guid TeacherId { get; set; }
    public Teacher? Teacher { get; set; }
    public int ProgrammingLanguageId { get; set; }
    public ProgrammingLanguage? ProgrammingLanguage { get; set; }
    public LanguagePreference LanguagePreference { get; set; } = LanguagePreference.None;
}