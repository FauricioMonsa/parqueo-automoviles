namespace ParqueoAutomoviles.Web.Models;

public sealed record IngresoAutomovilConsultaViewModel(
    long ParkingId,
    DateTime EntryDateTime,
    DateTime? ExitDateTime,
    double StayDurationMinutes,
    double StayDurationHours,
    decimal AmountToPay);
