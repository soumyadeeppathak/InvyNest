using InvyNest_API.Data;
using InvyNest_API.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvyNest_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkspaceController(AppDbContext db) : Controller
    {
        // GET /api/workspaces/mine?email=me@example.com
        [HttpGet("mine")]
        public async Task<IActionResult> Mine([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return BadRequest("email required.");

            var owned = db.Workspaces
                .Where(w => w.OwnerEmail == email)
                .Select(w => new { w.Id, w.Name, Role = "owner" });

            var memberOf = db.WorkspaceMembers
                .Where(m => m.MemberEmail == email)
                .Select(m => new { m.Workspace.Id, m.Workspace.Name, Role = m.Role });

            var result = await owned.Union(memberOf)
                .OrderBy(x => x.Name)
                .ToListAsync();

            return Ok(result);
        }

        // POST /api/workspaces/{id}/members
        public record AddMemberDto(string MemberEmail, string Role = "editor");

        [HttpPost("{id:guid}/members")]
        public async Task<IActionResult> AddMember(Guid id, [FromBody] AddMemberDto dto)
        {
            var exists = await db.Workspaces.AnyAsync(w => w.Id == id);
            if (!exists) return NotFound("Workspace not found.");

            var m = await db.WorkspaceMembers.FindAsync(id, dto.MemberEmail);
            if (m is null)
                db.WorkspaceMembers.Add(new WorkspaceMember { WorkspaceId = id, MemberEmail = dto.MemberEmail, Role = dto.Role });
            else
                m.Role = dto.Role;

            await db.SaveChangesAsync();
            return NoContent();
        }
    }
}
