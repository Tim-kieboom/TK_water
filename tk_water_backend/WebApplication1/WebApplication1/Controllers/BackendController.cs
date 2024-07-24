using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Update.Internal;
using WebApplication1.Controllers.backendBodys.addUnitData;
using WebApplication1.Controllers.backendBodys.getAvailableUnits;
using WebApplication1.Controllers.backendBodys.receiveAllUnitsData;
using WebApplication1.Controllers.backendBodys.RemoveAvailebleUnits;
using WebApplication1.Controllers.backendBodys.removeUnitData;
using WebApplication1.Controllers.backendBodys.UpdateUnit;
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

        [HttpPost("test")]
        public ActionResult Test() 
        {
            return Ok("success");
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

        [HttpPost("updateUnit")]
        public async Task<ActionResult> UpdateUnit(UpdateUnitBody request)
        {
            try
            {
                long count = await ORM_SqLite.Update(request.Unit, connection)
                                             .Where(unit => unit.UnitID == request.Unit.UnitID)
                                             .Execute();

                if (count <= 0)
                    return BadRequest("!!update failed unit doesn't exist!!");
            }
            catch (SqliteException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok( new UpdateUnitBody() { Unit = request.Unit });
        }

        [HttpPost("addUnitData")]
        public async Task<ActionResult> AddUnitData(AddUnitDataRequestBody request)
        {
            UnitData? addUnit;

            try 
            { 
                long unitsCount = await ORM_SqLite.Count<UnitData>(connection)
                                                  .Where(unitData => unitData.UserID == request.UserID)
                                                  .GetAfflictedCount();

                addUnit = await ORM_SqLite.Select<UnitData>(connection)
                                                    .Where(unitData => unitData.UnitID == request.UnitID)
                                                    .GetResult()
                                                    .AsyncFirstOrDefault();

                if(addUnit == null)
                    return NotFound($"no data with unitID: {request.UnitID} found");

                addUnit.UserID = request.UserID;

                long count = await ORM_SqLite.Update(addUnit, connection)
                                             .Where(unitData => unitData.UnitID == request.UnitID)
                                             .Execute();

                if (count <= 0)
                    return BadRequest("!!update failed unit doesn't exist!!");
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
                request.Unit.UserID = 0;
                long count = await ORM_SqLite.Update(request.Unit, connection)
                                             .Where(unitData => unitData.UnitID == request.Unit.UnitID)
                                             .Execute();

                if (count <= 0)
                    return BadRequest("!!update failed unit doesn't exist!!");
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

            return Ok(new GetAvailableUnitsResponseBody() { Units = unitsData });
        }

        [HttpPost("removeAvailableUnit")]
        public async Task<ActionResult> RemoveAvailableUnit(RemoveAvailebleUnitRequestBody request)
        {
            try
            {
                long count = await ORM_SqLite.Remove<UnitData>(connection)
                                             .Where(unit => unit.UserID == 0 && unit.UnitID == request.UnitID)
                                             .Execute();
                if(count <= 0) 
                    return BadRequest("!!unit to be removed not found in database!!");
            }
            catch (SqliteException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();

        }
    }
}
