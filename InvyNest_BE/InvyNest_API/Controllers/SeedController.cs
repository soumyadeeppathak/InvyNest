using InvyNest_API.Data;
using InvyNest_API.Domain;
using Microsoft.AspNetCore.Mvc;

namespace InvyNest_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeedController(AppDbContext db) : Controller
    {
        [HttpPost("demo")]
        public async Task<IActionResult> CreateDemo()
        {
            var ws = new Workspace { Name = "Home", OwnerEmail = "you@example.com" };
            var socks = new Item { Name = "Socks" };
            db.Workspaces.Add(ws);
            db.Items.Add(socks);
            db.WorkspaceItems.Add(new WorkspaceItem { Workspace = ws, Item = socks, Quantity = 12, Unit = "pcs" });
            await db.SaveChangesAsync();
            return Ok(new { workspaceId = ws.Id });
        }
    }
}
