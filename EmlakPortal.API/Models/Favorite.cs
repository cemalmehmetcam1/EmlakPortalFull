namespace EmlakPortal.API.Models
{
    public class Favorite : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public AppUser? User { get; set; }
        public int EstateId { get; set; }
        public Estate? Estate { get; set; }
    }
}