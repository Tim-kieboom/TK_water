namespace WebApplication1.Controllers.postUnitMeasurement
{
    public class PostUnitMeasurementRequestBody
    {
        public string UnitID { get; set; } = string.Empty;
        public long MoistureLevel { get; set; }
    }
}
