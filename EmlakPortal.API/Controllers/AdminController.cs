using EmlakPortal.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 

namespace EmlakPortal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context) { _context = context; }

        [HttpGet("Dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var totalEstates = await _context.Estates.CountAsync();
            var activeEstates = await _context.Estates.CountAsync(e => e.IsActive);
            var totalUsers = await _context.Users.CountAsync();
            var totalCategories = await _context.Categories.CountAsync();

            
            var latestEstates = await _context.Estates
                .OrderByDescending(e => e.CreatedDate)
                .Take(5)
                .Select(e => new { e.Title, e.City, e.Price, e.CreatedDate })
                .ToListAsync();

            
            var cityStats = await _context.Estates
                .GroupBy(e => e.City)
                .Select(g => new { City = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            
            var categoryStats = await _context.Estates
                .Include(e => e.Category)
                .GroupBy(e => e.Category.Name)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToListAsync();

            return Ok(new
            {
                TotalEstates = totalEstates,
                ActiveEstates = activeEstates,
                TotalUsers = totalUsers,
                TotalCategories = totalCategories,
                LatestEstates = latestEstates,
                CityStats = cityStats,
                CategoryStats = categoryStats
            });
        }
    }
}