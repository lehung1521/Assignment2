using B2BWholesale.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgentPortal.Web.Controllers
{
    // TASK 3.2 - Catalogue: agents browse items, prices and available stock
    [Authorize(Roles = "Agent")]
    public class CatalogController : Controller
    {
        private const int PageSize = 10;

        private readonly ICatalogService _catalog;

        public CatalogController(ICatalogService catalog)
        {
            _catalog = catalog;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? q, int page = 1, CancellationToken cancellationToken = default)
        {
            var result = await _catalog.GetCatalogAsync(q, page, PageSize, cancellationToken);
            ViewData["Query"] = q?.Trim();
            return View(result);
        }
    }
}
