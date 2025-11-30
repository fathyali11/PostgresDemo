using Microsoft.EntityFrameworkCore;
using PostgresDemo.Models;

namespace PostgresDemo.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): DbContext(options)
{
    public DbSet<Product> Products { get; set; }
}
