using WebApplication1.data.ORM;

namespace WebApplication1.data;

public class UnitData : ORM_Table
{
    public int UserID { get; set; }
    public string UnitID { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public short MoistureThreshold { get; set; }
    public short MoistureLevel { get; set; }

    public UnitData() {}
    public UnitData(int userID, string unitID, string unitName, short moistureThreshold, short moistureLevel)
    {
        UserID = userID;
        UnitID = unitID;
        UnitName = unitName;
        MoistureThreshold = moistureThreshold;
        MoistureLevel = moistureLevel;
    }

}

