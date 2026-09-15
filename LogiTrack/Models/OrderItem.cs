public class OrderItem
{
    public int OrderId { get; set; }
    public int ItemId { get; set; }

    public int Quantity { get; set; }

    public Order Order { get; set; } = null!;
    public InventoryItem Item { get; set; } = null!;
}