namespace InvyNest_API.Domain
{
    public class ItemComponent
    {
        public Guid ParentItemId { get; set; }
        public Item ParentItem { get; set; } = null!;

        public Guid ChildItemId { get; set; }
        public Item ChildItem { get; set; } = null!;

        // How many ChildItem units per one ParentItem unit
        public int ChildCount { get; set; } = 1;
    }
}
