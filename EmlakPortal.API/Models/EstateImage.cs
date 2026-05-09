namespace EmlakPortal.API.Models
{
    public class EstateImage : BaseEntity
    {
        public int EstateId { get; set; }
        public Estate Estate { get; set; } = null!;
        public string ImageUrl { get; set; } = string.Empty;
    }
}