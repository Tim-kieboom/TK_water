using WebApplication1.data;

namespace WebApplication1.Controllers.controlCentrumBodys.registerUnit
{
    public class RegisterUnitRequestBody
    {
        public string UnitID { get; set; } = string.Empty;
        public short MoistureLevel { get; set; }
        public short MoistureThreshold { get; set; }

    }
}
