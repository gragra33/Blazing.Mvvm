using Microsoft.EntityFrameworkCore;

namespace Blazing.Mvvm.AnalyzerTest.Data;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public DbSet<TestEntity> TestEntities => Set<TestEntity>();
}

public class TestEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
