using System.Collections.Generic;

namespace Blog.Mcv
{
    public static class AppPermissions
    {
        public static List<string> AllPermissions = new List<string>
        {
            "Blogs.Create",
            "Blogs.Read",
            "Blogs.Edit",
            "Blogs.Delete",
            "Comments.Create",
            "Comments.Read",
            "Comments.Edit",
            "Comments.Delete",
            "Users.Read",
            "Users.Edit",
            "Users.Delete",
            "Roles.Create",
            "Roles.Read",
            "Roles.Edit",
            "Roles.Delete"
        };
    }
}
