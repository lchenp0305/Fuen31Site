namespace Fuen31Site.Models.DTO
{
    public class UserDTO
    {
        public string? Name { get; set; }
        public int? Age { get; set; } = 29;
        public string? Email { get; set; }
        public IFormFile? Avatar { get; set; }

    }
}
