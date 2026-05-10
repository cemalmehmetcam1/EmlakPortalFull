using EmlakPortal.API.Data;
using EmlakPortal.API.DTOs;
using EmlakPortal.API.Models;
using EmlakPortal.API.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EmlakPortal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FavoriteController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FavoriteController(AppDbContext context) { _context = context; }

        [HttpPost("Toggle/{estateId}")]
        public async Task<IActionResult> Toggle(int estateId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var estate = await _context.Estates.FindAsync(estateId);
            if (estate == null || !estate.IsActive)
                return NotFound(new ResultDto { Status = false, Message = "İlan bulunamadı." });

            var existing = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.EstateId == estateId);

            if (existing != null)
            {
                _context.Favorites.Remove(existing);
                await _context.SaveChangesAsync();
                return Ok(new ResultDto { Status = true, Message = "Favoriden çıkarıldı.", Data = false });
            }
            else
            {
                _context.Favorites.Add(new Favorite
                {
                    UserId = userId,
                    EstateId = estateId,
                    CreatedDate = DateTime.Now,
                    IsActive = true
                });
                await _context.SaveChangesAsync();
                return Ok(new ResultDto { Status = true, Message = "Favorilere eklendi.", Data = true });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMyFavorites()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var favorites = await _context.Favorites
                .Include(f => f.Estate).ThenInclude(e => e.Category)
                .Include(f => f.Estate).ThenInclude(e => e.AppUser)
                .Include(f => f.Estate).ThenInclude(e => e.Images)
                .Where(f => f.UserId == userId && f.Estate.IsActive)
                .Select(f => new EstateDto
                {
                    Id = f.Estate.Id,
                    Title = f.Estate.Title,
                    Price = f.Estate.Price,
                    RoomCount = f.Estate.RoomCount,
                    SquareMeters = f.Estate.SquareMeters,
                    City = f.Estate.City,
                    StatusName = f.Estate.Status == EstateStatus.Satilik ? "Satılık" : "Kiralık",
                    CategoryName = f.Estate.Category!.Name,
                    ImageUrl = f.Estate.ImageUrl,
                    ImageUrls = f.Estate.Images.Select(i => i.ImageUrl).ToList()
                }).ToListAsync();

            return Ok(favorites);
        }
    }
}