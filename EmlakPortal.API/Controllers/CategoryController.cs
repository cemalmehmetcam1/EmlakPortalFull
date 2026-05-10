using EmlakPortal.API.DTOs;
using EmlakPortal.API.Models;
using EmlakPortal.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmlakPortal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IGenericRepository<Category> _categoryRepo;

        public CategoryController(IGenericRepository<Category> categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

      
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _categoryRepo.GetAllAsync();

            
            var result = categories.Where(c => c.IsActive).Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name
            });

            return Ok(result);
        }

      
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddCategory(CategoryDto model)
        {
            var category = new Category
            {
                Name = model.Name,
                Description = "Sistem tarafından eklendi.", 
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            await _categoryRepo.AddAsync(category);
            await _categoryRepo.SaveAsync();

            return Ok(new ResultDto { Status = true, Message = "Kategori başarıyla eklendi." });
        }
    }
}