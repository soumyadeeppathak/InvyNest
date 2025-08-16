namespace InvyNest_API.Domain
{
    public class WorkspaceItem
    {
        public Guid WorkspaceId { get; set; }
        public Workspace Workspace { get; set; } = null!;

        public Guid ItemId { get; set; }
        public Item Item { get; set; } = null!;

        public decimal Quantity { get; set; }
        public string? Unit { get; set; } // e.g., pcs, kg
    }
}
