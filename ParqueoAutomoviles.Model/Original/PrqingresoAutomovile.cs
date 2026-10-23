using System;
using System.Collections.Generic;

namespace ParqueoAutomoviles.Model;

public partial class PrqingresoAutomovile
{
    public long Consecutive { get; set; }

    public long ParkingId { get; set; }

    public long CarId { get; set; }

    public DateTime EntryDatetime { get; set; }

    public DateTime? ExitDatetime { get; set; }

    public virtual Prqautomovile Car { get; set; } = null!;

    public virtual Prqparqueo Parking { get; set; } = null!;
}
