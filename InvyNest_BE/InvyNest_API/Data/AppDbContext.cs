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
        public DbSet<WorkspaceMember> WorkspaceMembers => Set<WorkspaceMember>();

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
                .Property(ic => ic.ChildCount)
                .IsRequired();

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

            b.Entity<ItemComponent>()
                .ToTable(t =>
                    {
                        t.HasCheckConstraint(
                            "ck_item_components_no_self",
                            "\"ParentItemId\" <> \"ChildItemId\"");

                        t.HasCheckConstraint(
                            "ck_item_components_count_positive",
                            "\"ChildCount\" > 0");
                    });

            // WorkspaceItem (bridge)
            b.Entity<WorkspaceItem>()
                .HasKey(wi => wi.Id);

            b.Entity<WorkspaceItem>()
                .HasOne(wi => wi.Workspace)
                .WithMany(w => w.Items)
                .HasForeignKey(wi => wi.WorkspaceId);

            b.Entity<WorkspaceItem>()
                .HasOne(wi => wi.Item)
                .WithMany()
                .HasForeignKey(wi => wi.ItemId);

            // Parent-child hierarchy for WorkspaceItem
            b.Entity<WorkspaceItem>()
                .HasOne(wi => wi.Parent)
                .WithMany(wi => wi.Children)
                .HasForeignKey(wi => wi.ParentWorkspaceItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // NEW: helpful indexes for “who has what, where”
            b.Entity<WorkspaceItem>()
                .HasIndex(wi => wi.Holder);

            b.Entity<WorkspaceItem>()
                .HasIndex(wi => wi.LocationNote);

            //workspacemember
            b.Entity<WorkspaceMember>()
                .HasKey(x => x.Id);

            b.Entity<WorkspaceMember>()
                .HasOne(x => x.Workspace)
                .WithMany()
                .HasForeignKey(x => x.WorkspaceId);

            b.Entity<WorkspaceMember>()
                .Property(x => x.MemberEmail).HasMaxLength(160);

            b.Entity<WorkspaceMember>()
                .Property(x => x.MemberName).HasMaxLength(120).IsRequired();

            b.Entity<WorkspaceMember>()
                .Property(x => x.Role).HasMaxLength(30).IsRequired();

            // Add unique constraint for non-null email per workspace
            b.Entity<WorkspaceMember>()
                .HasIndex(x => new { x.WorkspaceId, x.MemberEmail })
                .IsUnique()
                .HasFilter("\"MemberEmail\" IS NOT NULL");
        }
    }
}
