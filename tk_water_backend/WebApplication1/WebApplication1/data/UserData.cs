using Microsoft.Data.Sqlite;
using WebApplication1.data.ORM;

namespace WebApplication1.data
{
    public class UserData: ORM_Table
    {
        public long UserID { get; set; }
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";

        public static string GetTableName()
        {
            throw new NotImplementedException();
        }
    }
}
