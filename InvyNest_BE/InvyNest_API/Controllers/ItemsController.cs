using InvyNest_API.Data;
using InvyNest_API.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvyNest_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly ILogger<ItemsController> _logger;
        private readonly AppDbContext _db;

        public ItemsController(ILogger<ItemsController> logger, AppDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        // Add a new item (optionally as a child)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateWorkspaceItemDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Name is required.");
            if (string.IsNullOrWhiteSpace(dto.Holder))
                return BadRequest("Holder is required.");

            // Create or get the Item entity
            var item = new Item { Name = dto.Name };
            _db.Items.Add(item);
            await _db.SaveChangesAsync();

            WorkspaceItem? parent = null;
            if (dto.ParentWorkspaceItemId.HasValue)
            {
                parent = await _db.WorkspaceItems.FindAsync(dto.ParentWorkspaceItemId.Value);
                if (parent == null)
                    return BadRequest("Parent item not found.");
                if (parent.Holder != dto.Holder)
                    return BadRequest("Child item must have the same holder as the parent.");
            }

            var wsItem = new WorkspaceItem
            {
                WorkspaceId = dto.WorkspaceId,
                ItemId = item.Id,
                Quantity = dto.Quantity,
                Unit = dto.Unit,
                Holder = dto.Holder,
                LocationNote = dto.LocationNote,
                ParentWorkspaceItemId = dto.ParentWorkspaceItemId
            };
            _db.WorkspaceItems.Add(wsItem);
            try
            {
                await _db.SaveChangesAsync();
                if (parent != null)
                {
                    await RecalculateParentQuantity(parent.Id);
                }
                _logger.LogInformation("WorkspaceItem created {@WorkspaceItem}", wsItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating workspace item");
                return StatusCode(500, "Could not create workspace item.");
            }
            return Ok();
        }

        // Get a single workspace item
        [HttpGet("workspaceitem/{id:guid}")]
        public async Task<IActionResult> GetWorkspaceItem(Guid id)
        {
            var wsItem = await _db.WorkspaceItems.Include(wi => wi.Item).FirstOrDefaultAsync(wi => wi.Id == id);
            return wsItem is null ? NotFound() : Ok(wsItem);
        }

        // Update item name
        [HttpPut("workspaceitem/{id:guid}/name")]
        public async Task<IActionResult> UpdateName(Guid id, [FromBody] UpdateItemNameDto dto)
        {
            var wsItem = await _db.WorkspaceItems.Include(wi => wi.Item).FirstOrDefaultAsync(wi => wi.Id == id);
            if (wsItem is null) return NotFound();
            if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Name is required.");
            wsItem.Item.Name = dto.Name;
            try
            {
                await _db.SaveChangesAsync();
                _logger.LogInformation("WorkspaceItem name updated for {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating workspace item name for {Id}", id);
                return StatusCode(500, "Could not update workspace item name.");
            }
            return Ok(wsItem);
        }

        // Update item quantity (only if no children)
        [HttpPut("workspaceitem/{id:guid}/quantity")]
        public async Task<IActionResult> UpdateQuantity(Guid id, [FromBody] UpdateItemQuantityDto dto)
        {
            var wsItem = await _db.WorkspaceItems.Include(wi => wi.Children).FirstOrDefaultAsync(wi => wi.Id == id);
            if (wsItem is null) return NotFound();
            if (wsItem.Children.Any())
                return BadRequest("Cannot update quantity: item has children.");
            wsItem.Quantity = dto.Quantity;
            try
            {
                await _db.SaveChangesAsync();
                if (wsItem.ParentWorkspaceItemId.HasValue)
                {
                    await RecalculateParentQuantity(wsItem.ParentWorkspaceItemId.Value);
                }
                _logger.LogInformation("WorkspaceItem quantity updated for {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating workspace item quantity for {Id}", id);
                return StatusCode(500, "Could not update workspace item quantity.");
            }
            return Ok();
        }

        // Delete a workspace item
        [HttpDelete("workspaceitem/{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var wsItem = await _db.WorkspaceItems.Include(wi => wi.Parent).FirstOrDefaultAsync(wi => wi.Id == id);
            if (wsItem is null) return NotFound();
            var parentId = wsItem.ParentWorkspaceItemId;
            _db.WorkspaceItems.Remove(wsItem);
            try
            {
                await _db.SaveChangesAsync();
                if (parentId.HasValue)
                {
                    await RecalculateParentQuantity(parentId.Value);
                }
                _logger.LogInformation("WorkspaceItem deleted {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting workspace item {Id}", id);
                return StatusCode(500, "Could not delete workspace item.");
            }
            return NoContent();
        }

        // Get full hierarchy for a workspace or person
        [HttpGet("hierarchy")] // ?workspaceId=...&holder=...
        public async Task<IActionResult> GetHierarchy([FromQuery] Guid workspaceId, [FromQuery] string? holder)
        {
            var query = _db.WorkspaceItems
                .Include(wi => wi.Item)
                .Include(wi => wi.Children)
                    .ThenInclude(child => child.Item)
                .Where(wi => wi.WorkspaceId == workspaceId && wi.ParentWorkspaceItemId == null);
            if (!string.IsNullOrWhiteSpace(holder))
                query = query.Where(wi => wi.Holder == holder);
            var roots = await query.ToListAsync();
            var result = roots.Select(BuildHierarchyNode).ToList();
            return Ok(result);
        }

        // Helper: build hierarchy node recursively
        private HierarchyNode BuildHierarchyNode(WorkspaceItem wsItem)
        {
            var children = wsItem.Children.Select(BuildHierarchyNode).ToList();
            var quantity = children.Any() ? children.Sum(c => c.Quantity) : wsItem.Quantity;
            return new HierarchyNode
            {
                Id = wsItem.Id,
                Name = wsItem.Item.Name,
                Quantity = quantity,
                Unit = wsItem.Unit,
                Holder = wsItem.Holder,
                LocationNote = wsItem.LocationNote,
                Children = children
            };
        }

        // Helper: recalculate parent quantity recursively
        private async Task RecalculateParentQuantity(Guid parentId)
        {
            var parent = await _db.WorkspaceItems.Include(wi => wi.Children).FirstOrDefaultAsync(wi => wi.Id == parentId);
            if (parent == null) return;
            parent.Quantity = parent.Children.Sum(c => c.Quantity);
            await _db.SaveChangesAsync();
            if (parent.ParentWorkspaceItemId.HasValue)
            {
                await RecalculateParentQuantity(parent.ParentWorkspaceItemId.Value);
            }
        }
    }

    // DTOs
    public record CreateWorkspaceItemDto(Guid WorkspaceId, string Name, decimal Quantity, string? Unit, string Holder, string? LocationNote, Guid? ParentWorkspaceItemId);
    public record UpdateItemNameDto(string Name);
    public record UpdateItemQuantityDto(decimal Quantity);

    public class HierarchyNode
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Quantity { get; set; }
        public string? Unit { get; set; }
        public string? Holder { get; set; }
        public string? LocationNote { get; set; }
        public List<HierarchyNode> Children { get; set; } = new();
    }
}
