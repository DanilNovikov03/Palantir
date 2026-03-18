using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Palantir.Domain.Models;

[Table("operation_sides")]
[PrimaryKey("operation_id", "war_side_id")]
[Index("war_side_id", Name = "idx_operation_sides_war_side_id")]
public partial class OperationSide
{
    [Key]
    public int operation_id { get; set; }

    [Key]
    public int war_side_id { get; set; }

    public string? role_side { get; set; }

    public string? note { get; set; }

    [ForeignKey("operation_id")]
    [InverseProperty("operation_sides")]
    public virtual Operation operation { get; set; } = null!;

    [ForeignKey("war_side_id")]
    [InverseProperty("operation_sides")]
    public virtual WarSide war_side { get; set; } = null!;
}
