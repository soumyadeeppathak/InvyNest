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

        [HttpGet("{id:guid}/inventory")]
        public async Task<IActionResult> GetInventory(Guid id, [FromQuery] string? holder, [FromQuery] string? location)
        {
            var q = db.WorkspaceItems
                .Where(wi => wi.WorkspaceId == id)
                .Select(wi => new
                {
                    wi.WorkspaceId,
                    wi.ItemId,
                    ItemName = wi.Item.Name,
                    wi.Quantity,
                    wi.Unit,
                    wi.Holder,
                    wi.LocationNote
                })
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(holder))
                q = q.Where(x => x.Holder != null && x.Holder.Contains(holder));

            if (!string.IsNullOrWhiteSpace(location))
                q = q.Where(x => x.LocationNote != null && x.LocationNote.Contains(location));

            var rows = await q.OrderBy(x => x.ItemName).ToListAsync();
            return Ok(rows);
        }

        // POST /api/workspaces/{id}/inventory (add or update a WorkspaceItem row)
        [HttpPost("{id:guid}/inventory")]
        public async Task<IActionResult> UpsertInventory(Guid id, [FromBody] UpsertInventoryDto dto)
        {
            if (dto.Quantity < 0) return BadRequest("Quantity cannot be negative.");
            var exists = await db.Items.AnyAsync(i => i.Id == dto.ItemId);
            if (!exists) return NotFound("Item not found.");

            var row = await db.WorkspaceItems
                .FirstOrDefaultAsync(wi => wi.WorkspaceId == id && wi.ItemId == dto.ItemId);

            if (row is null)
            {
                row = new WorkspaceItem
                {
                    WorkspaceId = id,
                    ItemId = dto.ItemId,
                    Quantity = dto.Quantity,
                    Unit = dto.Unit,
                    Holder = dto.Holder,
                    LocationNote = dto.LocationNote
                };
                db.WorkspaceItems.Add(row);
            }
            else
            {
                row.Quantity = dto.Quantity;
                row.Unit = dto.Unit;
                row.Holder = dto.Holder;
                row.LocationNote = dto.LocationNote;
            }

            await db.SaveChangesAsync();
            return Ok(row);
        }
        public record UpsertInventoryDto(Guid ItemId, decimal Quantity, string? Unit, string? Holder, string? LocationNote);
    }
}
