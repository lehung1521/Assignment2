using AgentPortal.Web.Models.ViewModels;
using B2BWholesale.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AgentPortal.Web.Controllers
{
    // TASK 3.2 - Master-Detail order. The database transaction lives in OrderPlacementService.
    [Authorize(Roles = "Agent")]
    public class OrderEntryController : Controller
    {
        private readonly ICatalogService _catalog;
        private readonly IOrderPlacementService _placement;
        private readonly ILogger<OrderEntryController> _logger;

        public OrderEntryController(
            ICatalogService catalog,
            IOrderPlacementService placement,
            ILogger<OrderEntryController> logger)
        {
            _catalog = catalog;
            _placement = placement;
            _logger = logger;
        }

        // The agent id always comes from the login cookie, never from the posted form.
        private int? GetAgentId()
        {
            return int.TryParse(User.FindFirstValue("AgentID"), out int agentId) && agentId > 0
                ? agentId
                : null;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? itemId, CancellationToken cancellationToken)
        {
            if (GetAgentId() == null)
                return Forbid();

            var model = new CreateOrderViewModel();
            await FillItemsAsync(model, cancellationToken);

            // "Order" button in the catalogue pre-selects one item
            if (itemId is > 0 && model.Items.Any(i => i.ItemId == itemId && i.Stock > 0))
            {
                model.Lines.Add(new OrderLineInput { ItemId = itemId.Value, Quantity = 1 });
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrderViewModel model, CancellationToken cancellationToken)
        {
            int? agentId = GetAgentId();
            if (agentId == null)
                return Forbid();

            model.Lines ??= new List<OrderLineInput>();

            if (model.Lines.Count == 0)
                ModelState.AddModelError(string.Empty, "Add at least one item to the order.");

            if (ModelState.IsValid)
            {
                try
                {
                    var lines = model.Lines
                        .Select(l => new OrderLineRequest(l.ItemId, l.Quantity))
                        .ToList();

                    var result = await _placement.PlaceOrderAsync(agentId.Value, lines, cancellationToken);

                    TempData["Success"] =
                        $"Order #{result.OrderId} was placed successfully ({result.LineCount} item(s), total {result.Total:N0} VND).";
                    return RedirectToAction("Details", "Orders", new { id = result.OrderId });
                }
                catch (InsufficientStockException ex)
                {
                    // Whole order blocked and rolled back: tell the agent which items failed.
                    ModelState.AddModelError(string.Empty, "Order blocked - insufficient stock:");
                    foreach (var shortage in ex.Shortages)
                    {
                        ModelState.AddModelError(string.Empty,
                            $"{shortage.ItemName} ({shortage.Size}): requested {shortage.Requested}, only {shortage.Available} available.");
                    }
                }
                catch (ServiceValidationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Order could not be saved for agent {AgentId}", agentId);
                    ModelState.AddModelError(string.Empty,
                        "The order could not be saved. No stock was deducted. Please try again.");
                }
            }

            await FillItemsAsync(model, cancellationToken);   // fresh stock numbers
            return View(model);
        }

        private async Task FillItemsAsync(CreateOrderViewModel model, CancellationToken cancellationToken)
        {
            var items = await _catalog.GetAllItemsAsync(cancellationToken);
            model.Items = items
                .Select(i => new ItemOption
                {
                    ItemId = i.ItemId,
                    ItemName = i.ItemName,
                    Size = i.Size,
                    Price = i.Price,
                    Stock = i.StockQuantity
                })
                .ToList();
        }
    }
}
