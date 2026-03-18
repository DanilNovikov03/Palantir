using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Palantir.Domain.Models;

[Table("war_sides")]
[Index("side_id", Name = "idx_war_sides_side_id")]
[Index("war_id", Name = "idx_war_sides_war_id")]
[Index("war_id", "side_id", Name = "war_sides_war_id_side_id_key", IsUnique = true)]
public partial class WarSide
{
    [Key]
    public int war_side_id { get; set; }

    public int war_id { get; set; }

    public int side_id { get; set; }

    public DateOnly? joined_date { get; set; }

    public DateOnly? out_date { get; set; }

    public string? note { get; set; }

    [InverseProperty("war_side")]
    public virtual ICollection<Army> armies { get; set; } = new List<Army>();

    [InverseProperty("war_side")]
    public virtual ICollection<ControlZone> control_zones { get; set; } = new List<ControlZone>();

    [InverseProperty("war_side")]
    public virtual ICollection<Event> events { get; set; } = new List<Event>();

    [InverseProperty("war_side")]
    public virtual ICollection<OperationSide> operation_sides { get; set; } = new List<OperationSide>();

    [ForeignKey("side_id")]
    [InverseProperty("war_sides")]
    public virtual Side side { get; set; } = null!;

    [ForeignKey("war_id")]
    [InverseProperty("war_sides")]
    public virtual War war { get; set; } = null!;
}
