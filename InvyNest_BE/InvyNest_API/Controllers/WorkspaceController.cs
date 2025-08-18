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
        public record CreateWorkspaceDto(string Name, string OwnerEmail);
        public record AddMemberDto(string MemberEmail, string Role);
        public record WorkspaceDto(Guid Id, string Name, string OwnerEmail, DateTime CreatedAtUtc);

        // POST api/workspace
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateWorkspaceDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.OwnerEmail))
            {
                _logger.LogWarning("Invalid workspace create payload: {@Dto}", dto);
                return BadRequest("Name and OwnerEmail are required.");
            }

            var ws = new Workspace
            {
                Id = Guid.NewGuid(),
                Name = dto.Name.Trim(),
                OwnerEmail = dto.OwnerEmail.Trim().ToLower(),
                CreatedAtUtc = DateTime.UtcNow
            };

            var member = new WorkspaceMember
            {
                WorkspaceId = ws.Id,
                MemberEmail = ws.OwnerEmail,
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

            if (string.IsNullOrWhiteSpace(dto.MemberEmail) || string.IsNullOrWhiteSpace(dto.Role))
                return BadRequest("MemberEmail and Role are required.");

            var email = dto.MemberEmail.Trim().ToLower();

            var exists = await _db.WorkspaceMembers.AnyAsync(m => m.WorkspaceId == id && m.MemberEmail == email);
            if (exists) return Conflict("Member already exists in this workspace.");

            var member = new WorkspaceMember
            {
                WorkspaceId = id,
                MemberEmail = email,
                Role = dto.Role
            };

            _db.WorkspaceMembers.Add(member);

            try
            {
                await _db.SaveChangesAsync();
                _logger.LogInformation("Added member {Email} to workspace {WorkspaceId}", email, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding member {Email} to workspace {WorkspaceId}", email, id);
                return StatusCode(500, "Could not add member.");
            }

            return Ok(member);
        }

        // GET api/workspace/mine?email=you@example.com
        [HttpGet("mine")]
        public async Task<IActionResult> Mine([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Email query parameter is required.");

            var normEmail = email.Trim().ToLower();

            var owned = await _db.Workspaces
                .Where(w => w.OwnerEmail == normEmail)
                .Select(w => new WorkspaceDto(w.Id, w.Name, w.OwnerEmail, w.CreatedAtUtc))
                .ToListAsync();

            var memberOf = await _db.WorkspaceMembers
                .Where(m => m.MemberEmail == normEmail)
                .Join(_db.Workspaces,
                      m => m.WorkspaceId,
                      w => w.Id,
                      (m, w) => new WorkspaceDto(w.Id, w.Name, w.OwnerEmail, w.CreatedAtUtc))
                .ToListAsync();

            // remove duplicates if any (owner also in members)
            var combined = owned.Concat(memberOf).GroupBy(w => w.Id).Select(g => g.First()).ToList();

            return Ok(combined);
        }
    }
}
