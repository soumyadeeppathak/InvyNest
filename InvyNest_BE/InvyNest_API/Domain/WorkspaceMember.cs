namespace InvyNest_API.Domain
{
    public class WorkspaceMember
    {
        public Guid WorkspaceId { get; set; }
        public Workspace Workspace { get; set; } = null!;
        public string MemberEmail { get; set; } = null!;
        public string Role { get; set; } = "editor"; // owner|editor|viewer
    }
}
