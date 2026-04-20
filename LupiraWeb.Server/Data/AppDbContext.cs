using LupiraWeb.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LupiraWeb.Server.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<MyInfoEntity> MyInfo => Set<MyInfoEntity>();
    public DbSet<EmploymentEntity> Employments => Set<EmploymentEntity>();
    public DbSet<ProjectEntity> Projects => Set<ProjectEntity>();
    public DbSet<SkillEntity> Skills => Set<SkillEntity>();
    public DbSet<EmploymentSkillEntity> EmploymentSkills => Set<EmploymentSkillEntity>();
    public DbSet<ProjectSkillEntity> ProjectSkills => Set<ProjectSkillEntity>();
    public DbSet<EventEntity> Events => Set<EventEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MyInfoEntity>(b =>
        {
            b.ToTable("MyInfo");
            b.HasKey(x => x.Id);
        });

        modelBuilder.Entity<EmploymentEntity>(b =>
        {
            b.ToTable("Employments");
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.StartDate);
            b.HasMany(x => x.Projects)
                .WithOne(p => p.Employment!)
                .HasForeignKey(p => p.EmploymentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ProjectEntity>(b =>
        {
            b.ToTable("Projects");
            b.HasKey(x => x.Id);
        });

        modelBuilder.Entity<SkillEntity>(b =>
        {
            b.ToTable("Skills");
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.Name).IsUnique();
            b.Property(x => x.Category).HasConversion<string>();
        });

        modelBuilder.Entity<EmploymentSkillEntity>(b =>
        {
            b.ToTable("EmploymentSkills");
            b.HasKey(x => new { x.EmploymentId, x.SkillId });
            b.HasOne(x => x.Employment)
                .WithMany(e => e.EmploymentSkills)
                .HasForeignKey(x => x.EmploymentId);
            b.HasOne(x => x.Skill)
                .WithMany(s => s.EmploymentSkills)
                .HasForeignKey(x => x.SkillId);
        });

        modelBuilder.Entity<ProjectSkillEntity>(b =>
        {
            b.ToTable("ProjectSkills");
            b.HasKey(x => new { x.ProjectId, x.SkillId });
            b.HasOne(x => x.Project)
                .WithMany(p => p.ProjectSkills)
                .HasForeignKey(x => x.ProjectId);
            b.HasOne(x => x.Skill)
                .WithMany(s => s.ProjectSkills)
                .HasForeignKey(x => x.SkillId);
        });

        modelBuilder.Entity<EventEntity>(b =>
        {
            b.ToTable("Events");
            b.HasKey(x => x.Id);
            b.HasIndex(x => new { x.AggregateId, x.Sequence }).IsUnique();
            b.HasIndex(x => x.OccurredAt);
            b.Property(x => x.PayloadJson).HasColumnType("TEXT");
        });
    }
}
