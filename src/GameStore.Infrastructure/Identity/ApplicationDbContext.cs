using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace GameStore.Infrastructure.Identity;

public class ApplicationDbContext : 
    IdentityDbContext<IdentityUser, IdentityRole, string, IdentityUserClaim<string>, IdentityUserRole<string>, 
    IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<IdentityUserRole<string>>().HasKey(iur => new { iur.UserId, iur.RoleId });

        builder.ApplyConfiguration(new IdentityRoleConfiguration());
        builder.ApplyConfiguration(new IdentityUserConfiguration());
        builder.ApplyConfiguration(new IdentityUserRoleConfiguration());
        builder.ApplyConfiguration(new IdentityUserClaimConfiguration());
    }

    public override int SaveChanges()
    {
        ValidateEntities();
        return base.SaveChanges();
    }

    private void ValidateEntities()
    {
        foreach (var entry in ChangeTracker.Entries<IdentityUser>())
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                if (string.IsNullOrEmpty(entry.Entity.UserName))
                {
                    throw new InvalidOperationException("The UserName property of IdentityUser cannot be null or empty.");
                }
            }
        }

        foreach (var entry in ChangeTracker.Entries<IdentityRole>())
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                if (string.IsNullOrEmpty(entry.Entity.Name))
                {
                    throw new InvalidOperationException("The Name property of IdentityRole cannot be null or empty.");
                }
            }
        }

        foreach (var entry in ChangeTracker.Entries<IdentityUserRole<string>>())
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                if (string.IsNullOrEmpty(entry.Entity.UserId) || string.IsNullOrEmpty(entry.Entity.RoleId))
                {
                    throw new InvalidOperationException("The UserId and RoleId properties of IdentityUserRole cannot be null or empty.");
                }
            }
        }
    }
}
