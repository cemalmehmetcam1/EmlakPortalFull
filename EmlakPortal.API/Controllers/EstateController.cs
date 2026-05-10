using EmlakPortal.API.Data;
using EmlakPortal.API.DTOs;
using EmlakPortal.API.Models;
using EmlakPortal.API.Models.Enums;
using EmlakPortal.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EmlakPortal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstateController : ControllerBase
    {
        private readonly IGenericRepository<Estate> _estateRepo;
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public EstateController(
            IGenericRepository<Estate> estateRepo,
            AppDbContext context,
            IWebHostEnvironment env)
        {
            _estateRepo = estateRepo;
            _context = context;
            _env = env;
        }

        
        [HttpGet]
        public async Task<IActionResult> GetEstates()
        {
            var estates = await _estateRepo.AsQueryable()
                .Include(e => e.Category)
                .Include(e => e.AppUser)
                .Include(e => e.Images)
                .Where(e => e.IsActive)
                .Select(e => new EstateDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    Price = e.Price,
                    RoomCount = e.RoomCount,
                    SquareMeters = e.SquareMeters,
                    City = e.City,
                    Address = e.Address,
                    StatusName = e.Status == EstateStatus.Satilik ? "Satılık" : "Kiralık",
                    CategoryId = e.CategoryId,
                    CategoryName = e.Category!.Name,
                    AddedBy = e.AppUser!.FullName,
                    IsActive = e.IsActive,
                    CreatedDate = e.CreatedDate,
                    ImageUrl = e.ImageUrl,
                    ImageIds = e.Images.Select(i => i.Id).ToList(),
                    ImageUrls = e.Images.Select(i => i.ImageUrl).ToList(),
                    SellerFullName = e.AppUser!.FullName,
                    SellerPhone = e.AppUser.PhoneNumber,
                    SellerEmail = e.AppUser.Email
                }).ToListAsync();

            return Ok(estates);
        }

      
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEstateById(int id)
        {
            var estate = await _estateRepo.AsQueryable()
                .Include(e => e.Category)
                .Include(e => e.AppUser)
                .Include(e => e.Images)
                .FirstOrDefaultAsync(e => e.Id == id && e.IsActive);

            if (estate == null)
                return NotFound(new ResultDto { Status = false, Message = "İlan bulunamadı." });

            var estateDto = new EstateDto
            {
                Id = estate.Id,
                Title = estate.Title,
                Description = estate.Description,
                Price = estate.Price,
                RoomCount = estate.RoomCount,
                SquareMeters = estate.SquareMeters,
                City = estate.City,
                Address = estate.Address,
                StatusName = estate.Status == EstateStatus.Satilik ? "Satılık" : "Kiralık",
                CategoryId = estate.CategoryId,
                CategoryName = estate.Category!.Name,
                AddedBy = estate.AppUser!.FullName,
                IsActive = estate.IsActive,
                CreatedDate = estate.CreatedDate,
                ImageUrl = estate.ImageUrl,
                ImageUrls = estate.Images.Select(i => i.ImageUrl).ToList(),
                ImageIds = estate.Images.Select(i => i.Id).ToList(),
                SellerFullName = estate.AppUser!.FullName,
                SellerPhone = estate.AppUser.PhoneNumber,
                SellerEmail = estate.AppUser.Email
            };

            return Ok(estateDto);
        }

        
        
        [HttpPost]
        public async Task<IActionResult> AddEstate(EstateCreateDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var estate = new Estate
            {
                Title = model.Title,
                Description = model.Description,
                Price = model.Price,
                RoomCount = model.RoomCount,
                SquareMeters = model.SquareMeters,
                City = model.City,
                Address = model.Address,
                Status = (EstateStatus)model.Status,
                CategoryId = model.CategoryId,
                AppUserId = userId!,
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            await _estateRepo.AddAsync(estate);
            await _estateRepo.SaveAsync();

            return Ok(new ResultDto { Status = true, Message = "İlan başarıyla eklendi.", Data = estate.Id });
        }

        
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEstate(int id)
        {
            var estate = await _estateRepo.GetByIdAsync(id);
            if (estate == null)
                return NotFound(new ResultDto { Status = false, Message = "İlan bulunamadı." });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            // Sadece ilan sahibi veya Admin silebilir
            if (estate.AppUserId != userId && !isAdmin)
                return Forbid();

            estate.IsActive = false;
            _estateRepo.Update(estate);
            await _estateRepo.SaveAsync();

            return Ok(new ResultDto { Status = true, Message = "İlan başarıyla yayından kaldırıldı." });
        }

     
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> UpdateEstate(EstateUpdateDto model)
        {
            var estate = await _estateRepo.GetByIdAsync(model.Id);
            if (estate == null)
                return NotFound(new ResultDto { Status = false, Message = "İlan bulunamadı." });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

      
            if (estate.AppUserId != userId && !isAdmin)
                return Forbid();

            estate.Title = model.Title;
            estate.Description = model.Description;
            estate.Price = model.Price;
            estate.RoomCount = model.RoomCount;
            estate.SquareMeters = model.SquareMeters;
            estate.City = model.City;
            estate.Address = model.Address;
            estate.Status = (EstateStatus)model.Status;
            estate.CategoryId = model.CategoryId;
            estate.IsActive = model.IsActive;

            _estateRepo.Update(estate);
            await _estateRepo.SaveAsync();

            return Ok(new ResultDto { Status = true, Message = "İlan başarıyla güncellendi." });
        }

        
        [HttpGet("ByCategory/{categoryId}")]
        public async Task<IActionResult> GetEstatesByCategory(int categoryId)
        {
            var estates = await _estateRepo.AsQueryable()
                .Include(e => e.Category)
                .Include(e => e.AppUser)
                .Include(e => e.Images)
                .Where(e => e.IsActive && e.CategoryId == categoryId)
                .Select(e => new EstateDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Price = e.Price,
                    RoomCount = e.RoomCount,
                    SquareMeters = e.SquareMeters,
                    City = e.City,
                    StatusName = e.Status == EstateStatus.Satilik ? "Satılık" : "Kiralık",
                    CategoryName = e.Category!.Name,
                    AddedBy = e.AppUser!.FullName,
                    CreatedDate = e.CreatedDate,
                    ImageUrl = e.ImageUrl,
                    ImageUrls = e.Images.Select(i => i.ImageUrl).ToList(),
                    ImageIds = e.Images.Select(i => i.Id).ToList(),
                    SellerFullName = e.AppUser!.FullName,
                    SellerPhone = e.AppUser.PhoneNumber,
                    SellerEmail = e.AppUser.Email
                }).ToListAsync();

            return Ok(estates);
        }

      
        [HttpPost("Filter")]
        public async Task<IActionResult> GetEstatesByFilter(EstateFilterDto filter)
        {
            var query = _estateRepo.AsQueryable()
                .Include(e => e.Category)
                .Include(e => e.AppUser)
                .Include(e => e.Images)
                .Where(e => e.IsActive);

            if (filter.MinPrice.HasValue)
                query = query.Where(e => e.Price >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(e => e.Price <= filter.MaxPrice.Value);

            if (!string.IsNullOrEmpty(filter.City))
                query = query.Where(e => e.City.ToLower().Contains(filter.City.ToLower()));

            if (filter.RoomCount.HasValue)
                query = query.Where(e => e.RoomCount == filter.RoomCount.Value);

            if (filter.Status.HasValue)
                query = query.Where(e => (int)e.Status == filter.Status.Value);

            var estates = await query.Select(e => new EstateDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Price = e.Price,
                RoomCount = e.RoomCount,
                SquareMeters = e.SquareMeters,
                City = e.City,
                Address = e.Address,
                StatusName = e.Status == EstateStatus.Satilik ? "Satılık" : "Kiralık",
                CategoryName = e.Category!.Name,
                AddedBy = e.AppUser!.FullName,
                IsActive = e.IsActive,
                CreatedDate = e.CreatedDate,
                ImageUrl = e.ImageUrl,
                ImageIds = e.Images.Select(i => i.Id).ToList(),
                ImageUrls = e.Images.Select(i => i.ImageUrl).ToList(),
                SellerFullName = e.AppUser!.FullName,
                SellerPhone = e.AppUser.PhoneNumber,
                SellerEmail = e.AppUser.Email
            }).ToListAsync();

            if (estates.Count == 0)
                return Ok(new ResultDto { Status = true, Message = "Kriterlerinize uygun ilan bulunamadı.", Data = estates });

            return Ok(estates);
        }

       

        [HttpPost("{id}/ImageUpload")]
        public async Task<IActionResult> UploadImage(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new ResultDto { Status = false, Message = "Lütfen bir fotoğraf seçin." });

            var estate = await _estateRepo.GetByIdAsync(id);
            if (estate == null)
                return NotFound(new ResultDto { Status = false, Message = "İlan bulunamadı." });

            var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(webRootPath, "images");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            estate.ImageUrl = "/images/" + uniqueFileName;
            _estateRepo.Update(estate);
            await _estateRepo.SaveAsync();

            return Ok(new ResultDto { Status = true, Message = "Vitrin fotoğrafı başarıyla yüklendi.", Data = estate.ImageUrl });
        }

       
        [HttpPost("{id}/UploadImages")]
        public async Task<IActionResult> UploadImages(int id, [FromForm] List<IFormFile> files)
        {
            try
            {
                if (files == null || files.Count == 0)
                    return BadRequest(new ResultDto { Status = false, Message = "Lütfen en az bir fotoğraf seçin." });

                var estate = await _estateRepo.GetByIdAsync(id);
                if (estate == null)
                    return NotFound(new ResultDto { Status = false, Message = "İlan bulunamadı." });

                var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadsFolder = Path.Combine(webRootPath, "images");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                foreach (var file in files)
                {
                    if (file.Length == 0) continue;

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    var estateImage = new EstateImage
                    {
                        EstateId = id,
                        ImageUrl = "/images/" + uniqueFileName,
                        CreatedDate = DateTime.Now,
                        IsActive = true
                    };
                    await _context.EstateImages.AddAsync(estateImage);
                }

                await _context.SaveChangesAsync();
                return Ok(new ResultDto { Status = true, Message = $"{files.Count} fotoğraf başarıyla yüklendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResultDto { Status = false, Message = $"Dosya yükleme hatası: {ex.Message}" });
            }
        }


        [Authorize] 
        [HttpDelete("Image/{imageId}")]
        public async Task<IActionResult> DeleteEstateImage(int imageId)
        {
            var image = await _context.EstateImages
                .Include(i => i.Estate)
                .FirstOrDefaultAsync(i => i.Id == imageId);

            if (image == null)
                return NotFound(new ResultDto { Status = false, Message = "Fotoğraf bulunamadı." });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            if (image.Estate.AppUserId != userId && !isAdmin)
                return Forbid();


            var webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var filePath = Path.Combine(webRootPath, image.ImageUrl.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            _context.EstateImages.Remove(image);
            await _context.SaveChangesAsync();

            return Ok(new ResultDto { Status = true, Message = "Fotoğraf silindi." });
        }
        [Authorize]
        [HttpGet("MyEstates")]
        public async Task<IActionResult> GetMyEstates()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var estates = await _estateRepo.AsQueryable()
                .Include(e => e.Category)
                .Include(e => e.AppUser)
                .Include(e => e.Images)
                .Where(e => e.AppUserId == userId && e.IsActive) 
                .OrderByDescending(e => e.CreatedDate)
                .Select(e => new EstateDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Price = e.Price,
                    RoomCount = e.RoomCount,
                    SquareMeters = e.SquareMeters,
                    City = e.City,
                    StatusName = e.Status == EstateStatus.Satilik ? "Satılık" : "Kiralık",
                    CategoryName = e.Category!.Name,
                    IsActive = e.IsActive,
                    CreatedDate = e.CreatedDate,
                    ImageUrl = e.ImageUrl,
                    ImageUrls = e.Images.Select(i => i.ImageUrl).ToList()
                }).ToListAsync();

            return Ok(estates);
        }
    }
}