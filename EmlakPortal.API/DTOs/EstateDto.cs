namespace EmlakPortal.API.DTOs
{
    public class EstateDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int RoomCount { get; set; }
        public int SquareMeters { get; set; }
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty; // eklenen
        public string StatusName { get; set; } = string.Empty;
        public int CategoryId { get; set; }                // eklenen
        public string CategoryName { get; set; } = string.Empty;
        public string AddedBy { get; set; } = string.Empty;
        public bool IsActive { get; set; }                // eklenen
        public DateTime CreatedDate { get; set; }
        public string? ImageUrl { get; set; }
    }
}