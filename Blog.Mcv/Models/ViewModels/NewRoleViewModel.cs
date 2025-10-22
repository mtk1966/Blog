using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Blog.Mcv.Models
{
    public class NewRoleViewModel
    {
        [Required]
        public int Id { get; set; }
        [Required, MaxLength(20)]
        public string Name { get; set; } = string.Empty;
        public List<PermissionViewModel> Permissions { get; set; } = new List<PermissionViewModel>();
    }
}
