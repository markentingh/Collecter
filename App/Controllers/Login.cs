namespace Collector.Controllers
{
    public class Login: Controller
    {
        public override string Render(string body = "")
        {
            if(User.UserId > 0)
            {
                //redirect to dashboard
                return base.Render(Redirect("/articles/"));
            }

            //check for database reset
            var view = new View("/Views/Login/login.html");

            if(App.Environment == Environment.development && Server.HasAdmin == false)
            {
                //load new administrator form
                view = new View("/Views/Login/new-admin.html");
                view["title"] = "Create an administrator account";
                Scripts.Append("<script src=\"/js/views/login/new-admin.js\"></script>");
            }
            else
            {
                //load login form (default)
                Scripts.Append("<script src=\"/js/views/login/login.js\"></script>");
            }

            //load login page
            return base.Render(view.Render());
        }
    }
}
