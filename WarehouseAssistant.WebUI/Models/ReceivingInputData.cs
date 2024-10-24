using System.ComponentModel.DataAnnotations;

namespace WarehouseAssistant.WebUI.Models;

public record ReceivingInputData()
{
    [Required(ErrorMessage = "Введите артикул/штрихкод")]
    public string Id { get; set; } = null!;
    
    [Required, Range(1, int.MaxValue, ErrorMessage = "Количество должно быть больше 0")]
    public int Quantity { get; set; } = 1;
}