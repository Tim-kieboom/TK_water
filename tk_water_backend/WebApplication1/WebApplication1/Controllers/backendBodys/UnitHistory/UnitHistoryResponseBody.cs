namespace WebApplication1.Controllers.backendBodys.UnitHistory
{
    public class HistoryUnitPoint
    {
        public long MoistureLevel { get; set; }
        public long MoistureThreshold { get; set; }

        public string TimeStamp { get; set; } = string.Empty;

        public HistoryUnitPoint() {}

        public HistoryUnitPoint(long moistureLevel, long moistureThreshold, string timeStamp)
        {
            TimeStamp = timeStamp;
            MoistureLevel = moistureLevel;
            MoistureThreshold = moistureThreshold;
        }
    }
    public class UnitHistoryResponseBody
    {
        public HistoryUnitPoint[] HistoryUnits { get; set; } = [];
    }
}
