using Microsoft.AspNetCore.Identity;

namespace MVC.Identity.Data.Models
{
    public class CustomUser : IdentityUser
    {
        public string? CustomProperty { get; set; }
    }
}
