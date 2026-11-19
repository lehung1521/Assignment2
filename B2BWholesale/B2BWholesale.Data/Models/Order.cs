namespace B2BWholesale.Data.Models;

public class Order
{
    public int OrderID { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.Now;
    public int AgentID { get; set; }
    public string OrderStatus { get; set; } = "Pending";

    public Agent? Agent { get; set; }
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}