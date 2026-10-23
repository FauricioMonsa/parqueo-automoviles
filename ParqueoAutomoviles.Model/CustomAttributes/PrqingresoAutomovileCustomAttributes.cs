using System.ComponentModel.DataAnnotations.Schema;

namespace ParqueoAutomoviles.Model;

public class PrqingresoAutomovileCustomAttributes : PrqingresoAutomovile
{
    [NotMapped]
    public int? DuracionEstadiaMinutos => ExitDatetime is null
        ? null
        : (int)(ExitDatetime.Value - EntryDatetime).TotalMinutes;

    [NotMapped]
    public double? DuracionEstadiaHoras => ExitDatetime is null
        ? null
        : (ExitDatetime.Value - EntryDatetime).TotalHours;

    [NotMapped]
    public decimal? MontoTotalPagar => DuracionEstadiaHoras is not double hours || Parking is null
        ? null
        : (decimal)hours * Parking.PricePerHour;
}
