using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Palantir.Domain.Models;

[Table("control_zones")]
[Index("war_id", "date_control", Name = "idx_control_zones_war_date")]
public partial class ControlZone
{
    [Key]
    public int control_id { get; set; }

    public int war_id { get; set; }

    public int war_side_id { get; set; }

    public DateOnly date_control { get; set; }

    public string precision_control { get; set; } = null!;

    [Column(TypeName = "geometry(MultiPolygon,4326)")]
    public MultiPolygon geom { get; set; } = null!;

    [ForeignKey("war_id")]
    [InverseProperty("control_zones")]
    public virtual War war { get; set; } = null!;

    [ForeignKey("war_side_id")]
    [InverseProperty("control_zones")]
    public virtual WarSide war_side { get; set; } = null!;
}
