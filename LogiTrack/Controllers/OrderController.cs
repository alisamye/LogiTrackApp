using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
public class OrderController : ControllerBase
{
    private readonly LogiTrackContext _context;

    public OrderController(LogiTrackContext context)
    {
        _context = context;
    }

    [HttpGet("/api/orders")]
    public async Task<IActionResult> GetOrders()
    {
        var orders = await _context.Orders
            .Include(order => order.OrderItems)
            .ThenInclude(line => line.Item)
            .ToListAsync();
        return Ok(orders);
    }

    [HttpGet("/api/orders/{orderId:int}")]
    public async Task<IActionResult> GetOrder(int orderId)
    {
        Order? order = await _context.Orders
            .Include(order => order.OrderItems)
            .ThenInclude(line => line.Item)
            .SingleOrDefaultAsync(order => order.OrderId == orderId);

        if (order is null)
            return NotFound($"Order {orderId} was not found.");

        //order.GetOrderSummary();
        return Ok(new
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
    }

    [HttpPost("/api/orders")]
    public async Task<IActionResult> CreateOrder([FromBody] Order order)
    {
        if (order == null)
            return BadRequest("Order cannot be null.");
        if (string.IsNullOrWhiteSpace(order.CustomerName))
            return BadRequest("Customer name is required.");
        if (order.DatePlaced == default)
            return BadRequest("Date placed is required.");
        if (order.OrderItems == null || !order.OrderItems.Any())
            return BadRequest("Order must contain at least one item.");
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetOrder), new { orderId = order.OrderId }, order);
    }

    [HttpDelete("/api/orders/{orderId:int}")]
    public async Task<IActionResult> DeleteOrder(int orderId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order is null)
            return NotFound($"Order {orderId} was not found.");
        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}