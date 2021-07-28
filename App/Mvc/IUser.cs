using System;
using Microsoft.AspNetCore.Http;

namespace Collector
{
    public interface IUser
    {
        int UserId { get; set; }
        string VisitorId { get; set; }
        string Email { get; set; }
        string Name { get; set; }
        string DisplayName { get; set; }
        bool Photo { get; set; }
        DateTime DateCreated { get; set; }
        bool ResetPass { get; set; } 

        IUser SetContext(HttpContext context);
        void LogIn(int userId, string email, string name, DateTime datecreated, string displayName = "", bool photo = false);
        void LogOut();
        void Save(bool changed = false);
    }
}
