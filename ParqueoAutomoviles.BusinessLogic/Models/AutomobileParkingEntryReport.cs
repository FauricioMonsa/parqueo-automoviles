using ParqueoAutomoviles.Model;

namespace ParqueoAutomoviles.BusinessLogic.Models;

public sealed record AutomobileParkingEntryReport(
    long Consecutive,
    long CarId,
    string Color,
    int Year,
    string Manufacturer,
    PRQTipoAutomovil Type,
    long ParkingId,
    string ParkingName,
    string ProvinceName,
    DateTime EntryDatetime,
    DateTime? ExitDatetime,
    decimal? AmountToPay);
