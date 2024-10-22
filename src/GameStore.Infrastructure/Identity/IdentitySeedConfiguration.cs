using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Infrastructure.Identity;

public class IdentityUserConfiguration : IEntityTypeConfiguration<IdentityUser>
{
    public static readonly string AdminUserId = Guid.NewGuid().ToString();

    public void Configure(EntityTypeBuilder<IdentityUser> builder)
    {
        var adminUser = new IdentityUser
        {
            Id = AdminUserId,
            UserName = "admin@example.com",
            NormalizedUserName = "ADMIN@EXAMPLE.COM",
            Email = "admin@example.com",
            NormalizedEmail = "ADMIN@EXAMPLE.COM",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString()
        };

        adminUser.PasswordHash = new PasswordHasher<IdentityUser>().HashPassword(adminUser, "P@ssw0rd!");

        builder.HasData(adminUser);
    }
}

public class IdentityRoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        builder.HasData(
            new IdentityRole { Id = "1", Name = "Admin", NormalizedName = "ADMIN" },
            new IdentityRole { Id = "2", Name = "Box", NormalizedName = "BOX" },
            new IdentityRole { Id = "3", Name = "Order", NormalizedName = "ORDER" },
            new IdentityRole { Id = "4", Name = "Product", NormalizedName = "PRODUCT" }
        );
    }
}

public class IdentityUserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole<string>>
{
    public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
    {
        builder.HasData(
            new IdentityUserRole<string>
            {
                UserId = IdentityUserConfiguration.AdminUserId,
                RoleId = "1"
            }
        );
    }
}

public class IdentityUserClaimConfiguration : IEntityTypeConfiguration<IdentityUserClaim<string>>
{
    public void Configure(EntityTypeBuilder<IdentityUserClaim<string>> builder)
    {
        builder.HasData(
            new IdentityUserClaim<string> { Id = 1, UserId = IdentityUserConfiguration.AdminUserId, ClaimType = "Box", ClaimValue = "Get" },
            new IdentityUserClaim<string> { Id = 2, UserId = IdentityUserConfiguration.AdminUserId, ClaimType = "Box", ClaimValue = "Add" },
            new IdentityUserClaim<string> { Id = 3, UserId = IdentityUserConfiguration.AdminUserId, ClaimType = "Box", ClaimValue = "Update" },
            new IdentityUserClaim<string> { Id = 4, UserId = IdentityUserConfiguration.AdminUserId, ClaimType = "Box", ClaimValue = "Delete" },
            new IdentityUserClaim<string> { Id = 5, UserId = IdentityUserConfiguration.AdminUserId, ClaimType = "Order", ClaimValue = "Get" },
            new IdentityUserClaim<string> { Id = 6, UserId = IdentityUserConfiguration.AdminUserId, ClaimType = "Order", ClaimValue = "Add" },
            new IdentityUserClaim<string> { Id = 7, UserId = IdentityUserConfiguration.AdminUserId, ClaimType = "Order", ClaimValue = "Update" },
            new IdentityUserClaim<string> { Id = 8, UserId = IdentityUserConfiguration.AdminUserId, ClaimType = "Order", ClaimValue = "Delete" },
            new IdentityUserClaim<string> { Id = 9, UserId = IdentityUserConfiguration.AdminUserId, ClaimType = "Product", ClaimValue = "Get" },
            new IdentityUserClaim<string> { Id = 10, UserId = IdentityUserConfiguration.AdminUserId, ClaimType = "Product", ClaimValue = "Add" },
            new IdentityUserClaim<string> { Id = 11, UserId = IdentityUserConfiguration.AdminUserId, ClaimType = "Product", ClaimValue = "Update" },
            new IdentityUserClaim<string> { Id = 12, UserId = IdentityUserConfiguration.AdminUserId, ClaimType = "Product", ClaimValue = "Delete" }
        );
    }
}
