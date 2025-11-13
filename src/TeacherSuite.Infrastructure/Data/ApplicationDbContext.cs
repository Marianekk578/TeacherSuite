using Microsoft.EntityFrameworkCore;

namespace TeacherSuite.Infrastructure.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
    }
}
