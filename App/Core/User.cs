using Newtonsoft.Json;

namespace Collector
{
    public enum enumSecurityType
    {
        read = 0,
        write = 1,
        both = 2
    }

    public class User
    {
        [JsonIgnore]
        public Core S;

        public int userId = 0;
        public int userType = 0;
        public string email = "";

        public User()
        {
        }

        public void Load(Core CollectorCore)
        {
            S = CollectorCore;
        }

        public void LogOut()
        {
            userId = 0;
            userType = 0;
            email = "";
        }

        public bool CheckSecurity(string feature = "", enumSecurityType security = enumSecurityType.both)
        {
            if (userId > 0)
            {
                return true;
            }
            return false;
        }
    }
}
