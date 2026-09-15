public class Order
{
    public int OrderId { get; set; }
    public string? CustomerName { get; set; }
    public DateTime DatePlaced { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public Order(int orderId, string customerName, DateTime datePlaced)
    {
        OrderId = orderId;
        CustomerName = customerName;
        DatePlaced = datePlaced;
    }

    public void AddItem(InventoryItem item, int quantity)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        OrderItems.Add(new OrderItem
        {
            OrderId = OrderId,
            ItemId = item.ItemId,
            Item = item,
            Quantity = quantity
        });
    }

    public void RemoveItem(int itemId)
    {
        OrderItem? orderItem = OrderItems.FirstOrDefault(item => item.ItemId == itemId);

        if (orderItem != null)
            OrderItems.Remove(orderItem);
    }

    public void GetOrderSummary()
    {
        Console.WriteLine($"Order #{OrderId} for {CustomerName} | Items: {OrderItems.Count} | Placed: {DatePlaced}");
    }

}