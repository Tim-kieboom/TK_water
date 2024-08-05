using Microsoft.AspNetCore.Mvc;
using WebApplication1.data.ORM;
using WebApplication1.data;
using WebApplication1.Controllers.controlCentrumBodys.registerUnit;
using WebApplication1.Controllers.controlCentrumBodys.postUnitMeasurement;
using WebApplication1.Controllers.controlCentrumBodys.signIn;
using Npgsql;
using System.Data.Common;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("controlCentrum")]
    public class ControlCentrumController : ControllerBase
    {
        private readonly ILogger<BackendController> _logger;
        private static readonly string connectionString = "host=postgres;port=5432;Database=WaterUnitData;Username=tkWaterUser;Password=waterUnitPassowrd;SSL mode=prefer;Pooling=true;MinPoolSize=1;MaxPoolSize=100;";
        private static readonly Func<DbConnection> postgressConnection = () => { return new NpgsqlConnection(connectionString); };

        private TK_ORM DataBase = new(postgressConnection);

        public ControlCentrumController(ILogger<BackendController> logger)
        {
            _logger = logger;
        }

        [HttpPost("signIn")]
        public async Task<ActionResult> SignIn(SignInRequestBody request)
        {
            try
            {
                long unitCount = await DataBase.Select<UnitData>()
                                                 .Where(unit => unit.UnitID == request.UnitID)
                                                 .GetAfflictedCount();

                if (unitCount > 0)
                    return Ok("success");

                UnitData unit = new(0, request.UnitID, "unitName", 60, 0);

                bool success = await DataBase.Insert(unit);
                if (!success)
                    return BadRequest("!!insertion into database failed!!");

            }
            catch(NpgsqlException ex) 
            {
                Console.WriteLine(ex.Message); 
            }

            return Ok("unit has been added to database");
        }

        [HttpPost("postUnitMeasurement")]
        public async Task<ActionResult> PostUnitMeasurement(PostUnitMeasurementRequestBody request)
        {
            UnitData? unitData;
            Byte moistureThreshold = 101;
            try
            {
                 unitData = await DataBase.Select<UnitData>()
                                            .Where(unitData => unitData.UnitID == request.UnitID)
                                            .GetResult()
                                            .AsyncFirstOrDefault();

                if(unitData == null)
                    return NotFound("unitID not found in dataBase");

                try
                {
                    moistureThreshold = Convert.ToByte(unitData.MoistureThreshold);
                } 
                catch (OverflowException) {}

                if (request.MoistureLevel == unitData.MoistureLevel)
                {
                    await UpdateUnitHistory(unitData);

                    return Ok(new PostUnitMeasurementResponseBody()
                    {
                        MoistureThreshold = moistureThreshold
                    });
                }

                unitData.MoistureLevel = request.MoistureLevel;
                await DataBase.Update(unitData)
                                .Where(unit => unit.UnitID == unitData.UnitID)
                                .Execute();

                await UpdateUnitHistory(unitData);
            }
            catch (NpgsqlException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(new PostUnitMeasurementResponseBody()
            {
                MoistureThreshold = moistureThreshold
            });
        }

        //called when unit connects to server
        [HttpPost("registerUnit")]
        public async Task<ActionResult> RegisterUnit(RegisterUnitRequestBody request)
        {
            long count = await DataBase.Select<UnitData>()
                                         .Where(unitData => unitData.UnitID == request.UnitID)
                                         .GetAfflictedCount();

            if (count > 0)
                return Ok();

            UnitData newUnit = new()
            {
                UnitID = request.UnitID,
                UnitName = "unitName",
                UserID = 0,
                MoistureLevel = request.MoistureLevel,
                MoistureThreshold = request.MoistureThreshold
            };

            try
            {
                await DataBase.Insert(newUnit);
            }
            catch (NpgsqlException ex)
            {
                return BadRequest(ex.Message);
            }


            return Ok();
        }

        private async Task UpdateUnitHistory(UnitData currentUnit)
        {
            UnitHistory newHistory = new(currentUnit);
            await DataBase.Insert(newHistory);
        }
    }
}
