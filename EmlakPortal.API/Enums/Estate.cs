using EmlakPortal.API.Models.Enums;

namespace EmlakPortal.API.Models
{
    public class Estate : BaseEntity
    {
        public string Title { get; set; } = string.Empty; 
        public string Description { get; set; } = string.Empty; 
        public decimal Price { get; set; } 
        public int RoomCount { get; set; } 
        public int SquareMeters { get; set; } 
        public string City { get; set; } = string.Empty; 
        public string Address { get; set; } = string.Empty; 

        public EstateStatus Status { get; set; } 

        
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public string AppUserId { get; set; } = string.Empty; 
        public AppUser? AppUser { get; set; }

        public ICollection<EstateImage> Images { get; set; } = new List<EstateImage>();

        public string? ImageUrl { get; set; } 
    }
}