using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Palantir.Domain.Models;

[Table("theaters")]
[Index("war_id", Name = "idx_theaters_war_id")]
public partial class Theater
{
    [Key]
    public int theater_id { get; set; }

    public int war_id { get; set; }

    public string title { get; set; } = null!;

    public string? summary { get; set; }

    [InverseProperty("theater")]
    public virtual ICollection<Operation> operations { get; set; } = new List<Operation>();

    [ForeignKey("war_id")]
    [InverseProperty("theaters")]
    public virtual War war { get; set; } = null!;
}
