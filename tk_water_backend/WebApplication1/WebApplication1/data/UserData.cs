using WebApplication1.data.ORM;

namespace WebApplication1.data
{
    public class UserData: ORM_Table
    {
        public int UserID { get; set; }

        public string UserName { get; set; } = "";
        public SqlVarChar UserName_varchar { get; set; } = new(50, "UserName");

        public string Password { get; set; } = ""; 
        public SqlVarChar Password_varchar { get; set; } = new(50, "Password");


        public UserData() {}

        public UserData(int userID, string userName, string password)
        {
            UserID = userID;
            UserName = userName;
            Password = password;
        }

        public static string GetTableName()
        {
            throw new NotImplementedException();
        }
    }
}
