using System.Reflection.Metadata.Ecma335;

namespace Blog.Mcv.Models
{
    public class UserViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public int RoleId { get; set; }
    }
}
