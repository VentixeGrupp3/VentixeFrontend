using Microsoft.AspNetCore.Mvc.Rendering;

namespace Frontend_Test.Models;

public class OrderTicketsViewModel
{
    public string? TicketId { get; set; }
    public string TicketCategory { get; set; } = null!;
    public decimal TicketPrice { get; set; }
    public int TicketQuantity { get; set; }

    public IEnumerable<SelectListItem> CategoryList { get; set; }
    public IEnumerable<SelectListItem> QuantityList { get; set; }
}
