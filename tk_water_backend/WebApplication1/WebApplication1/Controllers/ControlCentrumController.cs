using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using WebApplication1.data.ORM;
using WebApplication1.data;
using WebApplication1.Controllers.postUnitMeasurement;
using WebApplication1.Controllers.controlCentrumBodys.registerUnit;
using WebApplication1.Controllers.controlCentrumBodys.postUnitMeasurement;
using WebApplication1.Controllers.controlCentrumBodys.signIn;

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

        [HttpPost("signIn")]
        public async ActionResult signIn(SignInRequestBody request)
        {
            try
            {
                long unitCount = await ORM_SqLite.Select<UnitData>(connection)
                                                 .Where(unit => unit.UnitID == request.UnitID)
                                                 .GetAfflictedCount();

                if (unitCount > 0)
                    return Ok("success");

                UnitData unit = new(0, 0, request.UnitID, "", 70, 0);
                await ORM_SqLite.Insert(unit, connection);
            }
            catch(Exception ex) 
            {
                Console.WriteLine(ex.Message); 
            }

            return Ok("unit has been added to data");
        }

        [HttpPost("postUnitMeasurement")]
        public async Task<ActionResult> PostUnitMeasurement(PostUnitMeasurementRequestBody request)
        {
            UnitData? unitData;
            try
            {
                 unitData = await ORM_SqLite.Select<UnitData>(connection)
                                            .Where(unitData => unitData.UnitID == request.UnitID)
                                            .GetResult()
                                            .AsyncFirstOrDefault();

                if(unitData == null)
                    return NotFound("unitID not found in dataBase");

                if(request.MoistureLevel == unitData.MoistureLevel)
                    return Ok(new PostUnitMeasurementResponseBody() { UnitsData = unitData });

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
