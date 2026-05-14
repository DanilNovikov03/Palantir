namespace Palantir.Domain.Models
{
    [Table("army_positions")]
    [Index("army_id", "date_position", Name = "army_positions_army_id_date_position_key", IsUnique = true)]
    [Index("date_position", Name = "idx_army_positions_date")]
    public partial class ArmyPosition
    {
        [Key]
        public int army_position_id { get; set; }

        public int army_id { get; set; }

        public DateOnly date_position { get; set; }

        [Column(TypeName = "geometry(Point,4326)")]
        public Point coordinate { get; set; } = null!;

        public string? note { get; set; }

        [ForeignKey("army_id")]
        [InverseProperty("army_positions")]
        public virtual Army army { get; set; } = null!;
    }
}