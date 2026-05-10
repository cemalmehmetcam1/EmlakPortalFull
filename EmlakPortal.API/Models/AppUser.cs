using Microsoft.AspNetCore.Identity;

namespace EmlakPortal.API.Models
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        public ICollection<Favorite>? Favorites { get; set; }

     
        public ICollection<Estate>? Estates { get; set; }
    }
}