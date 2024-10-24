using System.ComponentModel.DataAnnotations;

namespace WarehouseAssistant.WebUI.Models;

public record ReceivingInputData()
{
    [Required]                         public string Id       { get; set; }
    [Required, Range(1, int.MaxValue)] public int    Quantity { get; set; } = 1;
}