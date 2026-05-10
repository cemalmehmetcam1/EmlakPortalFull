namespace EmlakPortal.API.Models
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = string.Empty; 
        public string Description { get; set; } = string.Empty;


        public ICollection<Estate>? Estates { get; set; }
    }
}