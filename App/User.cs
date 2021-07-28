using System;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Collector
{

    public class User : IUser
    {
        protected HttpContext Context;

        //fields saved into user session
        public int UserId { get; set; } = 0;
        public string VisitorId { get; set; } = "";
        public string Email { get; set; } = "";
        public string Name { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public bool Photo { get; set; } = false;
        public DateTime DateCreated { get; set; }
        public bool ResetPass { get; set; } = false;

        //private fields
        protected bool changed = false;

        //get User object from session
        public static User Get(HttpContext context)
        {
            User user;
            if (context.Session.Get("user") != null)
            {
                user = JsonSerializer.Deserialize<User>(GetString(context.Session.Get("user")));
            }
            else
            {
                user = (User)new User().SetContext(context);
            }
            user.Init(context);
            return user;
        }

        public IUser SetContext(HttpContext context)
        {
            Context = context;
            return this;
        }

        public virtual void Init(HttpContext context)
        {
            //generate visitor id
            Context = context;
            if (VisitorId == "" || VisitorId == null)
            {
                VisitorId = NewId();
                changed = true;
            }

            //check for persistant cookie
            if (UserId <= 0 && context.Request.Cookies.ContainsKey("authId"))
            {
                var user = Query.Users.AuthenticateUser(context.Request.Cookies["authId"]);
                if (user != null)
                {
                    //persistant cookie was valid, log in
                    LogIn(user.userId, user.email, user.name, user.datecreated, "", user.photo);
                }
            }
        }

        public void LogIn(int userId, string email, string name, DateTime datecreated, string displayName = "", bool photo = false)
        {
            this.UserId = userId;
            this.Email = email;
            this.Photo = photo;
            this.Name = name;
            this.DisplayName = displayName;
            this.DateCreated = datecreated;

            //create persistant cookie
            var auth = Query.Users.CreateAuthToken(userId);
            var options = new CookieOptions()
            {
                Expires = DateTime.Now.AddMonths(1)
            };

            Context.Response.Cookies.Append("authId", auth, options);

            changed = true;
            Save();
        }

        public void LogOut()
        {
            UserId = 0;
            Email = "";
            Name = "";
            Photo = false;
            changed = true;
            Context.Response.Cookies.Delete("authId");
            Save();
        }

        public void Save(bool changed = false)
        {
            if (this.changed == true && changed == false)
            {
                Context.Session.Set("user", GetBytes(JsonSerializer.Serialize<User>(this)));
                this.changed = false;
            }
            if (changed == true)
            {
                this.changed = true;
            }
        }

        public bool CheckSecurity(int boardId)
        {
            return UserId > 0;
        }
        #region "Helpers"

        private static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return string.Join("", chars);
        }

        private static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        private static string NewId(int length = 3)
        {
            string result = "";
            for (var x = 0; x <= length - 1; x++)
            {
                int type = new Random().Next(1, 3);
                int num;
                switch (type)
                {
                    case 1: //a-z
                        num = new Random().Next(0, 26);
                        result += (char)('a' + num);
                        break;

                    case 2: //A-Z
                        num = new Random().Next(0, 26);
                        result += (char)('A' + num);
                        break;

                    case 3: //0-9
                        num = new Random().Next(0, 9);
                        result += (char)('1' + num);
                        break;

                }

            }
            return result;
        }

        #endregion
    }
}