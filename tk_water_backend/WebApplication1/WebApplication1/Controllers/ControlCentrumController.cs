using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using WebApplication1.data.ORM;
using WebApplication1.data;
using WebApplication1.Controllers.postUnitMeasurement;
using WebApplication1.Controllers.registerUnit;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("controlCentrum")]
    public class ControlCentrumController : ControllerBase
    {
        private readonly ILogger<BackendController> _logger;
        private SqliteConnection connection = new("Data Source=unitDataBase.db");

        public ControlCentrumController(ILogger<BackendController> logger)
        {
            _logger = logger;
        }

        [HttpPost("postUnitMeasurement")]
        public async Task<ActionResult> PostUnitMeasurement(PostUnitMeasurementRequestBody request)
        {
            UnitData unitData;
            try
            {
                 unitData = await ORM_SqLite.Select<UnitData>(connection)
                                            .Where(unitData => unitData.UnitID == request.UnitID)
                                            .GetResult()
                                            .AsyncFirst();

                unitData.MoistureLevel = request.MoistureLevel;
                await ORM_SqLite.Insert(unitData, connection);
            }
            catch (SqliteException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(new PostUnitMeasurementResponseBody()
            {
                UnitsData = unitData
            });
        }

        //called when unit connects to server
        [HttpPost("registerUnit")]
        public async Task<ActionResult> RegisterUnit(RegisterUnitRequestBody request)
        {
            long count = await ORM_SqLite.Select<UnitData>(connection)
                                         .Where(unitData => unitData.UnitID == request.UnitID)
                                         .GetAfflictedCount();

            if (count > 0)
                return Ok();

            UnitData newUnit = new()
            {
                UnitID = request.UnitID,
                UnitName = "unitName",
                ModuleID = 0,
                UserID = 0,
                MoistureLevel = request.MoistureLevel,
                MoistureThreshold = request.MoistureThreshold
            };

            try
            {
                await ORM_SqLite.Insert(newUnit, connection);
            }
            catch (SqliteException ex)
            {
                return BadRequest(ex.Message);
            }


            return Ok();
        }
    }
}
