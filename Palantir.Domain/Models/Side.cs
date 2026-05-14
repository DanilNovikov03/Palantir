namespace Palantir.Domain.Models
{
    [Table("sides")]
    public partial class Side
    {
        [Key]
        public int side_id { get; set; }

        public string title { get; set; } = null!;

        [InverseProperty("side")]
        public virtual ICollection<WarSide> war_sides { get; set; } = new List<WarSide>();
    }
}