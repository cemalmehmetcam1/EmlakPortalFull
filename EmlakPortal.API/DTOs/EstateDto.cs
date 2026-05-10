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
        public string Address { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string AddedBy { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? ImageUrl { get; set; }
        public List<string>? ImageUrls { get; set; }
        public List<int>? ImageIds { get; set; }

        // Yeni satıcı bilgileri
        public string? SellerFullName { get; set; }
        public string? SellerPhone { get; set; }
        public string? SellerEmail { get; set; }
    }
}