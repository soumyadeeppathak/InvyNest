using InvyNest_API.Data;
using InvyNest_API.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace InvyNest_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemsController(AppDbContext db) : Controller
    {
        // Create a new Item
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateItemDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Name is required.");

            var item = new Item { Name = dto.Name };
            db.Items.Add(item);
            await db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
        }

        // Get single Item
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var item = await db.Items.FindAsync(id);
            return item is null ? NotFound() : Ok(item);
        }

        // Add/Update a child component with count
        [HttpPost("{parentId:guid}/components")]
        public async Task<IActionResult> UpsertComponent(Guid parentId, [FromBody] AddComponentDto dto)
        {
            if (parentId == dto.ChildItemId) return BadRequest("Parent and child cannot be the same.");
            if (dto.ChildCount <= 0) return BadRequest("ChildCount must be > 0.");

            var parentExists = await db.Items.AnyAsync(i => i.Id == parentId);
            var childExists = await db.Items.AnyAsync(i => i.Id == dto.ChildItemId);
            if (!parentExists || !childExists) return NotFound("Parent or child item not found.");

            // Cycle check: would adding (parent -> child) create a loop?
            var wouldCycle = await WouldCreateCycle(db, parentId, dto.ChildItemId);
            if (wouldCycle) return BadRequest("This link would create a cycle in the BOM.");

            var link = await db.ItemComponents
                .FirstOrDefaultAsync(ic => ic.ParentItemId == parentId && ic.ChildItemId == dto.ChildItemId);

            if (link is null)
            {
                link = new ItemComponent
                {
                    ParentItemId = parentId,
                    ChildItemId = dto.ChildItemId,
                    ChildCount = dto.ChildCount
                };
                db.ItemComponents.Add(link);
            }
            else
            {
                link.ChildCount = dto.ChildCount; // update
            }

            await db.SaveChangesAsync();
            return Ok(link);
        }

        // Remove a child component
        [HttpDelete("{parentId:guid}/components/{childId:guid}")]
        public async Task<IActionResult> RemoveComponent(Guid parentId, Guid childId)
        {
            var link = await db.ItemComponents
                .FirstOrDefaultAsync(ic => ic.ParentItemId == parentId && ic.ChildItemId == childId);
            if (link is null) return NotFound();

            db.ItemComponents.Remove(link);
            await db.SaveChangesAsync();
            return NoContent();
        }

        // Get flattened BOM with depth using Postgres recursive CTE
        [HttpGet("{rootId:guid}/bom")]
        public async Task<IActionResult> GetBom(Guid rootId)
        {
            var sql = """
                      WITH RECURSIVE bom AS (
                        SELECT ic."ParentItemId", ic."ChildItemId", ic."ChildCount", 1 AS depth
                        FROM "ItemComponents" ic
                        WHERE ic."ParentItemId" = @root
                        UNION ALL
                        SELECT ic."ParentItemId", ic."ChildItemId", ic."ChildCount", b.depth + 1
                        FROM "ItemComponents" ic
                        JOIN bom b ON ic."ParentItemId" = b."ChildItemId"
                      )
                      SELECT b."ParentItemId", p."Name" as ParentName,
                             b."ChildItemId", c."Name" as ChildName,
                             b."ChildCount", b.depth
                      FROM bom b
                      JOIN "Items" p ON p."Id" = b."ParentItemId"
                      JOIN "Items" c ON c."Id" = b."ChildItemId"
                      ORDER BY depth, ParentName, ChildName;
                      """;

            var rows = await db.BomRows
                .FromSqlRaw(sql, new NpgsqlParameter("root", rootId))
                .ToListAsync();

            return Ok(rows);
        }

        // ——— helpers ———

        private static async Task<bool> WouldCreateCycle(AppDbContext db, Guid newParentId, Guid newChildId)
        {
            // If newChild is (directly or indirectly) a parent of newParent → cycle.
            var sql = """
                        WITH RECURSIVE up AS (
                          SELECT "ParentItemId", "ChildItemId"
                          FROM "ItemComponents"
                          WHERE "ParentItemId" = @start
                          UNION ALL
                          SELECT ic."ParentItemId", ic."ChildItemId"
                          FROM "ItemComponents" ic
                          JOIN up ON ic."ParentItemId" = up."ChildItemId"
                        )
                        SELECT 1 FROM up WHERE "ChildItemId" = @target LIMIT 1;
                        """;

            var result = await db.Database
                .SqlQueryRaw<int>(sql,
                    new NpgsqlParameter("start", newChildId),
                    new NpgsqlParameter("target", newParentId))
                .ToListAsync();

            return result.Count > 0;
        }
    }

    // DTOs
    public record CreateItemDto(string Name);

    public record AddComponentDto(Guid ChildItemId, int ChildCount);

    // Query type for BOM
    public class BomRow
    {
        public Guid ParentItemId { get; set; }
        public string ParentName { get; set; } = null!;
        public Guid ChildItemId { get; set; }
        public string ChildName { get; set; } = null!;
        public int ChildCount { get; set; }
        public int Depth { get; set; }
    }
}
