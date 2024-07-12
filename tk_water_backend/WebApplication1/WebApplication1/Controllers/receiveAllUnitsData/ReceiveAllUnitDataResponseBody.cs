using WebApplication1.data;

namespace WebApplication1.Controllers.getAllUnitsData
{
    public class ReceiveAllUnitDataResponseBody
    {
        public LinkedList<UnitData> UnitsData { get; set; } = new();
    }
}
