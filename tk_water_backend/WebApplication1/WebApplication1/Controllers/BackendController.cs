using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using WebApplication1.Controllers.addUnitData;
using WebApplication1.Controllers.getAllUnitsData;
using WebApplication1.Controllers.removeUnitData;
using WebApplication1.data;
using WebApplication1.data.ORM;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("backend")]
    public class BackendController : ControllerBase
    {
        private readonly ILogger<BackendController> _logger;
        private static readonly SqliteConnection connection = new("Data Source=unitDataBase.db");

        public BackendController(ILogger<BackendController> logger)
        {
            _logger = logger;
        }

        [HttpPost("postAllUnitsData")]
        public async Task<ActionResult> PostUnitDataAsync(PostAllUnitsDataRequestBody request)
        {
            LinkedList<UnitData> unitsData;
            try
            {
                unitsData = await ORM_SqLite.Select<UnitData>(connection)
                                            .Where(unitData => unitData.UserID == request.UserID)
                                            .GetResult();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(new PostAllUnitDataResponseBody()
            {
                UnitsData = unitsData
            });
        }

        [HttpPost("addUnitData")]
        public async Task<ActionResult> AddUnitData(AddUnitDataRequestBody request)
        {
            UnitData unit;

            try 
            { 
                long unitsCount = await ORM_SqLite.Count<UnitData>(connection)
                                                  .Where(unitData => unitData.UserID == request.UserID)
                                                  .GetAfflictedCount();

                unit = new(request.UserID, unitsCount+1, request.UnitID, "unitName", 0, 0);

                await ORM_SqLite.Insert(unit, connection);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(unit);
        }

        [HttpPost("removeUnitData")]
        public async Task<ActionResult> RemoveUnitData(RemoveUnitDataRequestBody request)
        {
            try
            {
                await ORM_SqLite.Remove<UnitData>(connection)
                                .Where(unitData => unitData.ModuleID == request.ModuleID && unitData.UserID == request.UserID)
                                .Execute();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }
    }
}
