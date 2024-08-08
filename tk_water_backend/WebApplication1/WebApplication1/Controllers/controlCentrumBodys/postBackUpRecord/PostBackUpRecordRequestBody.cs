using System.Reflection.Metadata.Ecma335;

namespace WebApplication1.Controllers.controlCentrumBodys.postBackUpRecord
{
    public class BackUpRecord
    {
        public short MoistureLevel { get; set; }
        public string DateTime { get; set; } = string.Empty;
    }
    public class PostBackUpRecordRequestBody
    {
        public string UnitID { get; set; } = string.Empty;

        public BackUpRecord Record { get; set; } = new();
    }
}
