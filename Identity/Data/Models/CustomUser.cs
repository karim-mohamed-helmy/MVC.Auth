using Microsoft.AspNetCore.Identity;

namespace Identity.Data.Models
{
    public class CustomUser : IdentityUser
    {
        public string? CustomProperty { get; set; }
    }
}
