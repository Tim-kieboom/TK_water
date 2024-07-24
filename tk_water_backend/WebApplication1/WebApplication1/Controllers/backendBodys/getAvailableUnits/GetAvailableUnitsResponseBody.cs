using WebApplication1.data;

namespace WebApplication1.Controllers.backendBodys.getAvailableUnits
{
    public class GetAvailableUnitsResponseBody
    {
        public LinkedList<UnitData> Units { get; set; } = new();
    }
}
