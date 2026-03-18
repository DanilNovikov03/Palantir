using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Palantir.Domain.Models;

[Table("armies")]
[Index("war_side_id", Name = "idx_armies_war_side_id")]
public partial class Army
{
    [Key]
    public int army_id { get; set; }

    public int war_side_id { get; set; }

    public string name_arm { get; set; } = null!;

    public string? type_arm { get; set; }

    public DateOnly? start_date { get; set; }

    public DateOnly? end_date { get; set; }

    public string? summary { get; set; }

    [InverseProperty("army")]
    public virtual ICollection<ArmyPosition> army_positions { get; set; } = new List<ArmyPosition>();

    [ForeignKey("war_side_id")]
    [InverseProperty("armies")]
    public virtual WarSide war_side { get; set; } = null!;
}
