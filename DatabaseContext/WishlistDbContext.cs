using CartService.Entity;
using Microsoft.EntityFrameworkCore;
using WishlistService.Entity;

namespace WishlistService.DatabaseContext
{
    public class WishlistDbContext : DbContext
    {
        public WishlistDbContext(DbContextOptions<WishlistDbContext> options) : base(options) { }

        public DbSet<Wishlist> Wishlists { get; set; }
        //public object Cart { get; internal set; }
        public DbSet<Cart> Carts { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define composite primary key
            modelBuilder.Entity<Wishlist>()
                .HasKey(w => new { w.CustomerId, w.BookId });

            base.OnModelCreating(modelBuilder);
        }
    }
}
