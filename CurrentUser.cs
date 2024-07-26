using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ashwell_Maintenance
{
    public static class CurrentUser
    {
        public static string UserId { get; private set; }
        public static bool IsAdmin { get; private set; }

        public static void SetUser(string userId, bool isAdmin)
        {
            UserId = userId;
            IsAdmin = isAdmin;
        }

        public static void ClearUser()
        {
            UserId = null;
            IsAdmin = false;
        }
    }

}
