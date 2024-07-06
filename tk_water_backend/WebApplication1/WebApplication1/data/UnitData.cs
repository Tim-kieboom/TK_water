using Microsoft.Data.Sqlite;
using WebApplication1.data.ORM;

namespace WebApplication1.data;

public class UnitData : ORM_Table
{
    public long UserID { get; set; }
    public long ModuleID { get; set; }
    public string? UnitID { get; set; }
    public string UnitName { get; set; } = string.Empty;

    public long MoistureThreshold { get; set; }
    public long MoistureLevel { get; set; }

    public UnitData() {}
    public UnitData(long userID, long moduleID, string unitID, string unitName, long moistureThreshold, long moistureLevel)
    {
        UserID = userID;
        ModuleID = moduleID;
        UnitID = unitID;
        UnitName = unitName;
        MoistureThreshold = moistureThreshold;
        MoistureLevel = moistureLevel;
    }

}

