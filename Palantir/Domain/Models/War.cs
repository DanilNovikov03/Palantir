using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Palantir.Domain.Models;

[Table("wars")]
public partial class War
{
    [Key]
    public int war_id { get; set; }

    public string title { get; set; } = null!;

    public DateOnly start_date { get; set; }

    public DateOnly? end_date { get; set; }

    public string? summary { get; set; }

    [InverseProperty("war")]
    public virtual ICollection<ControlZone> control_zones { get; set; } = new List<ControlZone>();

    [InverseProperty("war")]
    public virtual ICollection<Event> events { get; set; } = new List<Event>();

    [InverseProperty("war")]
    public virtual ICollection<Theater> theaters { get; set; } = new List<Theater>();

    [InverseProperty("war")]
    public virtual ICollection<WarSide> war_sides { get; set; } = new List<WarSide>();
}
