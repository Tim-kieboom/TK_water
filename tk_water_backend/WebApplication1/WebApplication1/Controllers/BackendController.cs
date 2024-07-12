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

        [HttpPost("receiveAllUnitsData")]
        public async Task<ActionResult> ReceiveUnitDataAsync(ReceiveAllUnitsDataRequestBody request)
        {
            LinkedList<UnitData> unitsData;
            try
            {
                unitsData = await ORM_SqLite.Select<UnitData>(connection)
                                            .Where(unitData => unitData.UserID == request.UserID)
                                            .GetResult();
            }
            catch (SqliteException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(new ReceiveAllUnitDataResponseBody()
            {
                UnitsData = unitsData
            });
        }

        [HttpPost("addUnitData")]
        public async Task<ActionResult> AddUnitData(AddUnitDataRequestBody request)
        {
            UnitData addUnit;

            try 
            { 
                long unitsCount = await ORM_SqLite.Count<UnitData>(connection)
                                                  .Where(unitData => unitData.UserID == request.UserID)
                                                  .GetAfflictedCount();

                addUnit = await ORM_SqLite.Select<UnitData>(connection)
                                                    .Where(unitData => unitData.UnitID == request.UnitID)
                                                    .GetResult()
                                                    .AsyncFirst();

                addUnit.UserID = request.UserID;

                await ORM_SqLite.Update(addUnit, connection)
                                .Where(unitData => unitData.UnitID == request.UnitID)
                                .Execute();
            }
            catch (SqliteException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(addUnit);
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
            catch (SqliteException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        [HttpGet("getAvailableUnits")]
        public async Task<ActionResult> GetAvailableUnits()
        {
            LinkedList<UnitData> unitsData;
            try
            {
                unitsData = await ORM_SqLite.Select<UnitData>(connection)
                                            .Where(unitData => unitData.UserID == 0)
                                            .GetResult();
            }
            catch (SqliteException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(unitsData);
    }
}
