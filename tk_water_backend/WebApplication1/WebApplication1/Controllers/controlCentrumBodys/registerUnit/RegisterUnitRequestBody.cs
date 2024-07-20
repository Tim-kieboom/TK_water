using WebApplication1.data;

namespace WebApplication1.Controllers.controlCentrumBodys.registerUnit
{
    public class RegisterUnitRequestBody
    {
        public string UnitID { get; set; } = string.Empty;
        public long MoistureLevel { get; set; }
        public long MoistureThreshold { get; set; }

    }
}
