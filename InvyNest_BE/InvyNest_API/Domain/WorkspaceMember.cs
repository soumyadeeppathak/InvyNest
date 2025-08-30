namespace InvyNest_API.Domain
{
    public class WorkspaceMember
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid WorkspaceId { get; set; }
        public Workspace Workspace { get; set; } = null!;
        public string? MemberEmail { get; set; }
        public string MemberName { get; set; } = null!; // Always required, set via controller/service
        public string Role { get; set; } = "editor"; // owner|editor|viewer
    }
}
