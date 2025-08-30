using InvyNest_API.Data;
using InvyNest_API.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvyNest_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkspaceController : Controller
    {
        private readonly ILogger<WorkspaceController> _logger;
        private readonly AppDbContext _db;

        public WorkspaceController(ILogger<WorkspaceController> logger, AppDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        // DTOs
        public record CreateWorkspaceDto(string Name, string? OwnerEmail);
        public record AddMemberDto(string? MemberEmail, string Role, string? MemberName);
        public record WorkspaceDto(Guid Id, string Name, string? OwnerEmail, DateTime CreatedAtUtc);

        // POST api/workspace
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateWorkspaceDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                _logger.LogWarning("Invalid workspace create payload: {@Dto}", dto);
                return BadRequest("Name is required.");
            }

            // Default to "you@example.com" if no email provided
            var ownerEmail = string.IsNullOrWhiteSpace(dto.OwnerEmail) 
                ? "you@example.com" 
                : dto.OwnerEmail.Trim().ToLower();

            var ws = new Workspace
            {
                Id = Guid.NewGuid(),
                Name = dto.Name.Trim(),
                OwnerEmail = ownerEmail,
                CreatedAtUtc = DateTime.UtcNow
            };

            var member = new WorkspaceMember
            {
                WorkspaceId = ws.Id,
                MemberEmail = ownerEmail,
                MemberName = "DevLocal1", // Default to DevLocal1
                Role = "owner"
            };

            _db.Workspaces.Add(ws);
            _db.WorkspaceMembers.Add(member);

            try
            {
                await _db.SaveChangesAsync();
                _logger.LogInformation("Workspace created {@Workspace}", ws);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating workspace");
                return StatusCode(500, "Could not create workspace.");
            }

            return CreatedAtAction(nameof(GetById), new { id = ws.Id },
                new WorkspaceDto(ws.Id, ws.Name, ws.OwnerEmail, ws.CreatedAtUtc));
        }

        // GET api/workspace/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var ws = await _db.Workspaces.FindAsync(id);
            if (ws == null) return NotFound();

            return Ok(new WorkspaceDto(ws.Id, ws.Name, ws.OwnerEmail, ws.CreatedAtUtc));
        }

        // POST api/workspace/{id}/members
        [HttpPost("{id:guid}/members")]
        public async Task<IActionResult> AddMember(Guid id, [FromBody] AddMemberDto dto)
        {
            var ws = await _db.Workspaces.FindAsync(id);
            if (ws == null) return NotFound("Workspace not found.");

            if (string.IsNullOrWhiteSpace(dto.Role))
                return BadRequest("Role is required.");

            // Handle case where only name is provided (no email)
            string? email = null;
            string memberName;

            if (!string.IsNullOrWhiteSpace(dto.MemberEmail))
            {
                email = dto.MemberEmail.Trim().ToLower();
                var exists = await _db.WorkspaceMembers.AnyAsync(m => m.WorkspaceId == id && m.MemberEmail == email);
                if (exists) return Conflict("Member already exists in this workspace.");
            }

            if (!string.IsNullOrWhiteSpace(dto.MemberName))
            {
                memberName = dto.MemberName.Trim();
            }
            else if (!string.IsNullOrWhiteSpace(email))
            {
                memberName = email.Contains("@") ? email[..email.IndexOf('@')] : email;
            }
            else
            {
                return BadRequest("Either MemberEmail or MemberName is required.");
            }

            var member = new WorkspaceMember
            {
                WorkspaceId = id,
                MemberEmail = email,
                MemberName = memberName,
                Role = dto.Role
            };

            _db.WorkspaceMembers.Add(member);

            try
            {
                await _db.SaveChangesAsync();
                _logger.LogInformation("Added member {Name} ({Email}) to workspace {WorkspaceId}", memberName, email ?? "no-email", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding member {Name} to workspace {WorkspaceId}", memberName, id);
                return StatusCode(500, "Could not add member.");
            }

            return Ok(member);
        }

        // GET api/workspace/{id}/members
        [HttpGet("{id:guid}/members")]
        public async Task<IActionResult> GetMembers(Guid id)
        {
            var members = await _db.WorkspaceMembers
                .Where(m => m.WorkspaceId == id)
                .Select(m => new {
                    m.MemberEmail,
                    m.MemberName,
                    m.Role
                })
                .ToListAsync();
            return Ok(members);
        }

        // GET api/workspace/mine?email=you@example.com (defaults to you@example.com if no email provided)
        [HttpGet("mine")]
        public async Task<IActionResult> Mine([FromQuery] string? email)
        {
            // Default to "you@example.com" if no email provided
            var searchEmail = string.IsNullOrWhiteSpace(email) 
                ? "you@example.com" 
                : email.Trim().ToLower();

            var owned = await _db.Workspaces
                .Where(w => w.OwnerEmail == searchEmail)
                .Select(w => new WorkspaceDto(w.Id, w.Name, w.OwnerEmail, w.CreatedAtUtc))
                .ToListAsync();

            var memberOf = await _db.WorkspaceMembers
                .Where(m => m.MemberEmail == searchEmail)
                .Join(_db.Workspaces,
                      m => m.WorkspaceId,
                      w => w.Id,
                      (m, w) => new WorkspaceDto(w.Id, w.Name, w.OwnerEmail, w.CreatedAtUtc))
                .ToListAsync();

            // Remove duplicates if any (owner also in members)
            var combined = owned.Concat(memberOf).GroupBy(w => w.Id).Select(g => g.First()).ToList();

            return Ok(combined);
        }

        // DELETE api/workspace/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            // Find the workspace
            var workspace = await _db.Workspaces.FindAsync(id);
            if (workspace == null)
                return NotFound("Workspace not found.");

            // Delete all WorkspaceMembers
            var members = _db.WorkspaceMembers.Where(m => m.WorkspaceId == id);
            _db.WorkspaceMembers.RemoveRange(members);

            // Delete all WorkspaceItems and their Items
            var workspaceItems = _db.WorkspaceItems.Where(wi => wi.WorkspaceId == id);
            var itemIds = await workspaceItems.Select(wi => wi.ItemId).ToListAsync();
            _db.WorkspaceItems.RemoveRange(workspaceItems);

            // Delete all Items associated with this workspace
            var itemsToDelete = _db.Items.Where(i => itemIds.Contains(i.Id));
            _db.Items.RemoveRange(itemsToDelete);

            // Finally, delete the workspace itself
            _db.Workspaces.Remove(workspace);

            try
            {
                await _db.SaveChangesAsync();
                _logger.LogInformation("Workspace {WorkspaceId} and related data deleted.", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting workspace {WorkspaceId}", id);
                return StatusCode(500, "Could not delete workspace.");
            }

            return NoContent();
        }
    }
}
