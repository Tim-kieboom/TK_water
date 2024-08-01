/*
CREATE TABLE[unitHistory]
(
   [historyID] INTEGER CONSTRAINT[sqlite_master_PK_unitHistory] PRIMARY KEY AUTOINCREMENT NOT NULL 
 , [unitID] TEXT NOT NULL
 , [moistureLevel] INTEGER DEFAULT (0) NOT NULL
 , [moistureThreshold] INTEGER DEFAULT (0) NOT NULL
 , [historyIndex] INTEGER NOT NULL
 , [timestamp] DATETIME DEFAULT CURRENT_TIMESTAMP
 , FOREIGN KEY ([unitID]) REFERENCES unitData([unitID])
);
*/

using WebApplication1.data.ORM;

namespace WebApplication1.data
{
    public class UnitHistory : ORM_Table
    {
        public SqlSerial<long>? HistoryID { get; set; }

        public string UnitID { get; set; } = string.Empty;

        public short MoistureLevel { get; set; }

        public short MoistureThreshold { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public UnitHistory() {}

        public UnitHistory(UnitData currentUnit)
        {
            HistoryID = null;
            Timestamp = DateTime.Now;

            UnitID = currentUnit.UnitID;
            MoistureLevel = currentUnit.MoistureLevel;
            MoistureThreshold = currentUnit.MoistureThreshold;
        }

        public UnitHistory(UnitData currentUnit, DateTime timestamp)
        {
            HistoryID = null;
            Timestamp = timestamp;

            UnitID = currentUnit.UnitID;
            MoistureLevel = currentUnit.MoistureLevel;
            MoistureThreshold = currentUnit.MoistureThreshold;
        }

        public UnitData GetUnitData()
        {
            return new UnitData(0, UnitID, "", MoistureThreshold, MoistureLevel);
        }
    }
}
