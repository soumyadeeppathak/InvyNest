using System.ComponentModel.DataAnnotations;

namespace InvyNest_API.Domain
{
    public class Item
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(120)]
        public string Name { get; set; } = null!;

        // Superset/subset relation (e.g., Clothes > Shirt)
        public ICollection<ItemComponent> Components { get; set; } = new List<ItemComponent>();      // this item is parent-of
        public ICollection<ItemComponent> PartOf { get; set; } = new List<ItemComponent>();          // this item is child-of
    }
}
