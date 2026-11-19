namespace B2BWholesale.Data.Models;

public class OrderDetail
{
    public int ID { get; set; }
    public int OrderID { get; set; }
    public int ItemID { get; set; }
    public int Quantity { get; set; }
    public decimal UnitAmount { get; set; }   // unit price at the time of order

    public Order? Order { get; set; }
    public Item? Item { get; set; }
}