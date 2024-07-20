using WebApplication1.data;

namespace WebApplication1.Controllers.backendBodys.receiveAllUnitsData
{
    public class ReceiveAllUnitDataResponseBody
    {
        public LinkedList<UnitData> UnitsData { get; set; } = new();
    }
}
