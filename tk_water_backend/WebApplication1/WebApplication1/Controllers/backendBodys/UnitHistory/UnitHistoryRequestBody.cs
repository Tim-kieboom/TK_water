using System.Reflection.Metadata.Ecma335;

namespace WebApplication1.Controllers.backendBodys.UnitHistory
{
    public enum TimeStateEnum
    {
        days = 0,
        hours = 1,
        TenMinutes = 2
    }

    public class TimeSpan
    {
        public string TimeBegin { get; set; } = string.Empty;
        public string TimeEnd { get; set; } = string.Empty;
    }

    public class UnitHistoryRequestBody
    {
        public string UnitID { get; set; } = string.Empty;
        public TimeSpan TimeSpan { get; set; } = new();
        public int TimeState { get; set; }
    }
}
