using System.ComponentModel.DataAnnotations;

namespace InvyNest_API.Domain
{
    public class WorkspaceItem
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid WorkspaceId { get; set; }
        public Workspace Workspace { get; set; } = null!;

        public Guid ItemId { get; set; }
        public Item Item { get; set; } = null!;

        public decimal Quantity { get; set; }
        public string? Unit { get; set; } // e.g., pcs, kg
        public string? Holder { get; set; }
        public string? LocationNote { get; set; }

        // Parent-child hierarchy for items in a workspace
        public Guid? ParentWorkspaceItemId { get; set; }
        public WorkspaceItem? Parent { get; set; }
        public ICollection<WorkspaceItem> Children { get; set; } = new List<WorkspaceItem>();
    }
}
