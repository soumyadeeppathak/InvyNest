using InvyNest_API.Domain;
using Microsoft.EntityFrameworkCore;

namespace InvyNest_API.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options)
        : DbContext(options)
    {
        public DbSet<Workspace> Workspaces => Set<Workspace>();
        public DbSet<Item> Items => Set<Item>();
        public DbSet<ItemComponent> ItemComponents => Set<ItemComponent>();
        public DbSet<WorkspaceItem> WorkspaceItems => Set<WorkspaceItem>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // Workspace
            b.Entity<Workspace>()
                .Property(x => x.Name).IsRequired();

            // Item
            b.Entity<Item>()
                .Property(x => x.Name).IsRequired();

            // ItemComponent (self M2M)
            b.Entity<ItemComponent>()
                .HasKey(ic => new { ic.ParentItemId, ic.ChildItemId });

            b.Entity<ItemComponent>()
                .HasOne(ic => ic.ParentItem)
                .WithMany(i => i.Components)
                .HasForeignKey(ic => ic.ParentItemId)
                .OnDelete(DeleteBehavior.Restrict);

            b.Entity<ItemComponent>()
                .HasOne(ic => ic.ChildItem)
                .WithMany(i => i.PartOf)
                .HasForeignKey(ic => ic.ChildItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // WorkspaceItem (bridge)
            b.Entity<WorkspaceItem>()
                .HasKey(wi => new { wi.WorkspaceId, wi.ItemId });

            b.Entity<WorkspaceItem>()
                .HasOne(wi => wi.Workspace)
                .WithMany(w => w.Items)
                .HasForeignKey(wi => wi.WorkspaceId);

            b.Entity<WorkspaceItem>()
                .HasOne(wi => wi.Item)
                .WithMany()
                .HasForeignKey(wi => wi.ItemId);
        }
    }
}
