using System;
using System.Collections.Generic;

namespace ParqueoAutomoviles.Model;

public partial class Prqparqueo
{
    public long Id { get; set; }

    public string ProvinceName { get; set; } = null!;

    public string Name { get; set; } = null!;

    public decimal PricePerHour { get; set; }

    public virtual ICollection<PrqingresoAutomovile> PrqingresoAutomoviles { get; set; } = new List<PrqingresoAutomovile>();
}
