using WebApplication1.data.ORM;

namespace WebApplication1.data
{
    public class UserData: ORM_Table
    {
        public int UserID { get; set; }
        public string UserName { get; set; } = "";
        public string Password { get; set; } = ""; 


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
