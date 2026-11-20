using System.ComponentModel.DataAnnotations;
using B2BWholesale.Data.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AgentPortal.Web.Models.ViewModels
{
    // TASK 3.2 - master (order) + detail (lines) form
    public class CreateOrderViewModel
    {
        public List<OrderLineInput> Lines { get; set; } = new();

        // Filled by the server for the item drop-down; never read from the form.
        [BindNever]
        public List<ItemOption> Items { get; set; } = new();
    }

    public class OrderLineInput
    {
        [Range(1, int.MaxValue, ErrorMessage = "Choose a valid item for every order line.")]
        public int ItemId { get; set; }

        [Range(1, OrderCalculator.MaxQuantityPerLine, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }

    public class ItemOption
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}
