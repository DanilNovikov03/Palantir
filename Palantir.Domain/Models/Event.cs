namespace Palantir.Domain.Models
{
    [Table("events")]
    [Index("war_id", "date_ev", Name = "idx_events_war_date")]
    public partial class Event
    {
        [Key]
        public int event_id { get; set; }

        public int war_id { get; set; }

        public int? operation_id { get; set; }

        public int? war_side_id { get; set; }

        public string title { get; set; } = null!;

        public string? text_ev { get; set; }

        public string? type_ev { get; set; }

        public DateOnly date_ev { get; set; }

        [Column(TypeName = "geometry(Point,4326)")]
        public Point coordinate { get; set; } = null!;

        [ForeignKey("operation_id")]
        [InverseProperty("events")]
        public virtual Operation? operation { get; set; }

        [ForeignKey("war_id")]
        [InverseProperty("events")]
        public virtual War war { get; set; } = null!;

        [ForeignKey("war_side_id")]
        [InverseProperty("events")]
        public virtual WarSide? war_side { get; set; }
    }
}