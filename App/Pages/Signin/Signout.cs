namespace Collector.Pages
{
    public class Signout : Page
    {
        public Signout() : base()
        {
        }

        public override string Render()
        {
            S.User.LogOut();
            return "<script type='text/javascript'>location.href='/signin';</script>";
        }
    }
}
