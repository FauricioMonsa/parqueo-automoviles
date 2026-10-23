using System;
using System.Collections.Generic;

namespace ParqueoAutomoviles.Model;

public partial class Prqautomovile
{
    public long Id { get; set; }

    public string Color { get; set; } = null!;

    public int Year { get; set; }

    public string Manufacturer { get; set; } = null!;

    public PRQTipoAutomovil Type { get; set; }

    public virtual ICollection<PrqingresoAutomovile> PrqingresoAutomoviles { get; set; } = new List<PrqingresoAutomovile>();
}
