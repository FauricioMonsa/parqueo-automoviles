using ParqueoAutomoviles.Model;

namespace ParqueoAutomoviles.Web.Models;

public sealed class AutomovilesIndexViewModel
{
    public int? MinYear { get; set; }

    public int? MaxYear { get; set; }

    public IReadOnlyList<Prqautomovile> Automoviles { get; set; } = [];
}
