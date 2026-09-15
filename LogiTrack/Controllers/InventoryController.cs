using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class InventoryController : ControllerBase
{
    private readonly LogiTrackContext _context;

    public InventoryController(LogiTrackContext context)
    {
        _context = context;
    }

    [HttpGet("/api/inventory")]
    public async Task<IActionResult> GetInventory()
    {
        var items = await _context.InventoryItems.ToListAsync();
        return Ok(items);
    }

    [HttpPost("/api/inventory")]
    public async Task<IActionResult> AddInventoryItem([FromBody] InventoryItem item)
    {
        if (item == null)
            return BadRequest("Inventory item cannot be null.");
        if (string.IsNullOrWhiteSpace(item.Name))
            return BadRequest("Item name is required.");
        if (item.Quantity < 0)
            return BadRequest("Quantity cannot be negative.");
        if (string.IsNullOrWhiteSpace(item.Location))
            return BadRequest("Item location is required.");
        _context.InventoryItems.Add(item);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetInventory), new { id = item.ItemId }, item);
    }

    [HttpDelete("/api/inventory/{id:int}")]
    public async Task<IActionResult> DeleteInventoryItem(int id)
    {
        var item = await _context.InventoryItems.FindAsync(id);
        if (item is null)
            return NotFound($"Item {id} was not found.");
        _context.InventoryItems.Remove(item);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}