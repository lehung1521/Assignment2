
using B2BWholesale.Data;
using B2BWholesale.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AgentPortal.Web.Controllers
{
    [Authorize(Roles = "Agent")]
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // Lấy AgentID từ tài khoản đang đăng nhập
        private int? GetAgentId()
        {
            string? value = User.FindFirstValue("AgentID");

            if (int.TryParse(value, out int agentId)
                && agentId > 0)
            {
                return agentId;
            }

            return null;
        }

        // TASK 3.3 - My Orders
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int? agentId = GetAgentId();

            if (agentId == null)
                return Forbid();

            var orders = await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderDetails)
                .Where(o => o.AgentID == agentId.Value)
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.OrderID)
                .ToListAsync();

            return View(orders);
        }

        // TASK 3.3 - Order Details
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            int? agentId = GetAgentId();

            if (agentId == null)
                return Forbid();

            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Agent)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Item)
                .FirstOrDefaultAsync(o =>
                    o.OrderID == id &&
                    o.AgentID == agentId.Value);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // TASK 3.3 - Printable Invoice
        [HttpGet]
        public async Task<IActionResult> Invoice(int id)
        {
            int? agentId = GetAgentId();

            if (agentId == null)
                return Forbid();

            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Agent)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Item)
                .FirstOrDefaultAsync(o =>
                    o.OrderID == id &&
                    o.AgentID == agentId.Value);

            if (order == null)
                return NotFound();

            return View(order);
        }
    }
}
