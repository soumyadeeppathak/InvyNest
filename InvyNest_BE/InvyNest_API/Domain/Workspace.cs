using System.ComponentModel.DataAnnotations;

namespace InvyNest_API.Domain
{
    public class Workspace
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(120)]
        public string Name { get; set; } = null!;

        // Simple owner for now; real auth will come later
        [MaxLength(120)]
        public string OwnerEmail { get; set; } = null!;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public ICollection<WorkspaceItem> Items { get; set; } = new List<WorkspaceItem>();
    }
}
