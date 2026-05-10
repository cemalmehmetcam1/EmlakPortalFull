namespace EmlakPortal.API.DTOs
{
    public class EstateFilterDto
    {
        public decimal? MinPrice { get; set; } 
        public decimal? MaxPrice { get; set; } 
        public string? City { get; set; } 
        public int? RoomCount { get; set; } 
        public int? Status { get; set; } 
    }
}