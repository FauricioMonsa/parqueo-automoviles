using System.ComponentModel.DataAnnotations;
using ParqueoAutomoviles.Model;

namespace ParqueoAutomoviles.Web.Models;

public sealed class AutomovilFormViewModel
{
    public long Id { get; set; }

    [Required]
    [StringLength(40)]
    public string Color { get; set; } = string.Empty;

    [Range(1, 9999)]
    public int Year { get; set; }

    [Required]
    [StringLength(100)]
    public string Manufacturer { get; set; } = string.Empty;

    [Required]
    public PRQTipoAutomovil? Type { get; set; }
}
