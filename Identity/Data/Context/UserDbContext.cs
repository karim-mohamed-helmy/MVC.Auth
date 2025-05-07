using Identity.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.Data.Context
{
    public class UserDbContext : IdentityDbContext<CustomUser>
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<CustomUser>(entity =>
            {
                entity.ToTable("Users");
                entity.Property(e => e.CustomProperty).HasMaxLength(150);
            });
        }
    }
}
