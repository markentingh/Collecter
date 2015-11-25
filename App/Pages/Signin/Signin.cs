
namespace Collector.Pages
{
    public class Signin : Page
    {
        public Signin() : base()
        {
        }

        public override string Render()
        {
            Scaffold scaffold;
            Includes.Interface iface;
            bool err = false;

            if(S.User.userId > 0)
            {
                //user is already signed in
                //setup scaffolding variables
                scaffold = new Scaffold(S, "/app/pages/signin/already.html", "", new string[] { });

                //load website interface
                iface = new Includes.Interface(S, scaffold);

                return iface.Render(scaffold.Render(), "signin.css");
            }
            else
            {
                if (S.Request.ContentType != null)
                {
                    if (S.Request.Form.Count > 0)
                    {
                        //authenticate user
                        string email = S.Request.Form["email"];
                        string pass = S.Request.Form["pass"];
                        Utility.Encryption crypt = new Utility.Encryption(S);
                        string salt = crypt.GetMD5Hash(email + S.Server.saltPrivateKey + pass);
                        SqlClasses.User sqlUser = new SqlClasses.User(S);
                        SqlReader reader = sqlUser.AuthenticateUser(email, salt);
                        if (reader.Rows.Count > 0)
                        {
                            //sign in successful
                            reader.Read();
                            S.User.email = email;
                            S.User.userId = reader.GetInt("userid");
                            S.User.userType = reader.GetInt("usertype");

                            //setup scaffolding variables
                            scaffold = new Scaffold(S, "/app/pages/signin/success.html", "", new string[] { });

                            //load website interface
                            iface = new Includes.Interface(S, scaffold);

                            return iface.Render(scaffold.Render(), "signin.css");
                        }
                        else
                        {
                            //incorrect email or password
                            err = true;
                        }
                    }

                }

                //setup scaffolding variables
                scaffold = new Scaffold(S, "/app/pages/signin/form.html", "", new string[] { "error", "errmsg" });
                if (err == true) {
                    scaffold.Data["error"] = "1";
                    scaffold.Data["errmsg"] = "The email or password you have provided is incorrect.";
                }

                //load website interface
                iface = new Includes.Interface(S, scaffold);

                return iface.Render(scaffold.Render(), "signin.css");
            }
            
        }
    }
}
