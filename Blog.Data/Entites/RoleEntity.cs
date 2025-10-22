using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Data.Entites
{
    public class RoleEntity :BaseEntity
    {
        [Required,MaxLength(20)]
        public string Name { get; set; } = string.Empty;
        public string? PermissionsJson { get; set; }
    }
}
