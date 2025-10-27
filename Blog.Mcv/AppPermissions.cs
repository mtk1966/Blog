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
            "Comments.Transactions",
            "Users.Read",
            "Users.Edit",
            "Users.Delete",
            "Roles.Create",
            "Roles.Read",
            "Roles.Edit",
            "Roles.Delete",
            "LogViewer.View"
        };

        public static class LogViewer
        {
            public const string View = "LogViewer.View";
        }
    }
}
