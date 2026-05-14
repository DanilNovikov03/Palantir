namespace Palantir.Domain.Models
{
    [Table("operations")]
    [Index("theater_id", Name = "idx_operations_theater_id")]
    public partial class Operation
    {
        [Key]
        public int operation_id { get; set; }

        public int theater_id { get; set; }

        public string title { get; set; } = null!;

        public DateOnly start_date { get; set; }

        public DateOnly end_date { get; set; }

        public string? summary { get; set; }

        [InverseProperty("operation")]
        public virtual ICollection<Event> events { get; set; } = new List<Event>();

        [InverseProperty("operation")]
        public virtual ICollection<OperationSide> operation_sides { get; set; } = new List<OperationSide>();

        [ForeignKey("theater_id")]
        [InverseProperty("operations")]
        public virtual Theater theater { get; set; } = null!;
    }
}