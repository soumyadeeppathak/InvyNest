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
            var ws = new Workspace { Name = "Shared Home", OwnerEmail = "you@example.com" };
            var socks = new Item { Name = "Socks" };
            var tent = new Item { Name = "Tent" };

            db.Workspaces.Add(ws);
            db.Items.AddRange(socks, tent);

            db.WorkspaceItems.AddRange(
                // Alex keeps socks at their place
                new WorkspaceItem { Workspace = ws, Item = socks, Quantity = 12, Unit = "prs", Holder = "alex@example.com", LocationNote = "Alex’s place" },
                // Jordan keeps the tent in the garage
                new WorkspaceItem { Workspace = ws, Item = tent, Quantity = 1, Unit = "pc", Holder = "jordan@example.com", LocationNote = "Garage" }
            );

            await db.SaveChangesAsync();
            return Ok(new { workspaceId = ws.Id });
        }
    }
}
