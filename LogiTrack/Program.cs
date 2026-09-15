using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<LogiTrackContext>();

var app = builder.Build();

using (var context = new LogiTrackContext())
{
    // Add test inventory item if none exist
    if (!context.InventoryItems.Any())
    {
        context.InventoryItems.Add(new InventoryItem(
            1,
            "Pallet Jack",
            12,
            "Warehouse A"));

        context.SaveChanges();
    }

    // Retrieve and print inventory to confirm
    var items = context.InventoryItems.ToList();
    foreach (var item in items)
    {
        item.DisplayInfo(); // Should print: Item: Pallet Jack | Quantity: 12 | Location: Warehouse A
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/item/add", async (CreateItemRequest request, LogiTrackContext context) =>
{
    if (request.Quantity < 0)
        return Results.BadRequest("Quantity cannot be negative.");

    InventoryItem item = new(0, request.Name, request.Quantity, request.Location);
    context.InventoryItems.Add(item);
    await context.SaveChangesAsync();

    return Results.Created($"/item/{item.ItemId}", item);
});

app.MapPost("/order/add", async (CreateOrderRequest request, LogiTrackContext context) =>
{
    if (string.IsNullOrWhiteSpace(request.CustomerName))
        return Results.BadRequest("CustomerName is required.");

    if (request.Items.Count == 0)
        return Results.BadRequest("An order must contain at least one item.");

    List<int> itemIds = request.Items.Select(item => item.ItemId).ToList();
    List<InventoryItem> inventoryItems = await context.InventoryItems
        .Where(item => itemIds.Contains(item.ItemId))
        .ToListAsync();

    if (inventoryItems.Count != itemIds.Distinct().Count())
        return Results.NotFound("One or more inventory items were not found.");

    Order order = new(0, request.CustomerName, request.DatePlaced ?? DateTime.UtcNow);
    foreach (CreateOrderLine line in request.Items)
    {
        InventoryItem item = inventoryItems.Single(item => item.ItemId == line.ItemId);
        order.AddItem(item, line.Quantity);
    }

    context.Orders.Add(order);
    await context.SaveChangesAsync();

    return Results.Created($"/order/{order.OrderId}/summary", new
    {
        order.OrderId,
        order.CustomerName,
        order.DatePlaced,
        Items = order.OrderItems.Select(line => new { line.ItemId, line.Quantity })
    });
});

app.MapGet("/order/{orderId:int}/summary", async (int orderId, LogiTrackContext context) =>
{
    Order? order = await context.Orders
        .Include(order => order.OrderItems)
        .ThenInclude(line => line.Item)
        .SingleOrDefaultAsync(order => order.OrderId == orderId);

    if (order is null)
        return Results.NotFound($"Order {orderId} was not found.");

    order.GetOrderSummary();
    return Results.Ok(new
    {
        order.OrderId,
        order.CustomerName,
        order.DatePlaced,
        Items = order.OrderItems.Select(line => new
        {
            line.ItemId,
            ItemName = line.Item.Name,
            line.Quantity
        })
    });
});

app.MapDelete("/order/{orderId:int}/item/{itemId:int}", async (int orderId, int itemId, LogiTrackContext context) =>
{
    OrderItem? orderItem = await context.OrderItems
        .SingleOrDefaultAsync(line => line.OrderId == orderId && line.ItemId == itemId);

    if (orderItem is null)
        return Results.NotFound("The order item was not found.");

    context.OrderItems.Remove(orderItem);
    await context.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/order/remove/{orderId:int}", async (int orderId, LogiTrackContext context) =>
{
    Order? order = await context.Orders.FindAsync(orderId);

    if (order is null)
        return Results.NotFound($"Order {orderId} was not found.");

    context.Orders.Remove(order);
    await context.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();

public record CreateItemRequest(string Name, int Quantity, string Location);

public record CreateOrderRequest(
    string CustomerName,
    DateTime? DatePlaced,
    List<CreateOrderLine> Items);

public record CreateOrderLine(int ItemId, int Quantity);

