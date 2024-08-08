using WebApplication1.Controllers.backendBodys.RemoveAvailebleUnits;
using WebApplication1.Controllers.backendBodys.receiveAllUnitsData;
using WebApplication1.Controllers.backendBodys.getAvailableUnits;
using WebApplication1.Controllers.backendBodys.removeUnitData;
using WebApplication1.Controllers.backendBodys.addUnitData;
using WebApplication1.Controllers.backendBodys.UnitHistory;
using WebApplication1.Controllers.backendBodys.UpdateUnit;
using WebApplication1.Controllers.genericBodys;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.data.ORM;
using WebApplication1.data;
using System.Data.Common;
using Npgsql;

namespace WebApplication1.Controllers
{
    public static class Extensions
    {
        public static IEnumerable<UnitHistory> GetAverageUnitHistory<T, K>(this IEnumerable<IGrouping<T, K>> source) where K : UnitHistory
        {
            return source.Select(group =>
            {
                UnitHistory history     = group.First();
                DateTime timeStamp      = history.Timestamp;

                history.MoistureLevel       = (short)group.Average(unit => unit.MoistureLevel);
                history.MoistureThreshold   = (short)group.Average(unit => unit.MoistureThreshold);

                return history;
            });
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source)
        {
            return source.SelectMany(el => el);
        }

        public static IEnumerable<IGrouping<int, UnitHistory>> GroupTime<T>(this IEnumerable<T> source, TimeStateEnum timeState) where T : UnitHistory
        {
            switch (timeState)
            {
                case TimeStateEnum.days:
                    return source.GroupBy(unit => unit.Timestamp.Day);

                case TimeStateEnum.hours:
                    return source.GroupBy(unit => unit.Timestamp.Hour);

                case TimeStateEnum.TenMinutes:
                    return source.GroupBy(unit => unit.Timestamp.Minute - (unit.Timestamp.Minute % 10));

                default:
                    break;
            }

            return source.GroupBy(unit => unit.Timestamp.Date.Day);
        }

        private static DateTime GetAverageDateTime(DateTime date1, DateTime date2)
        {
            return new((date1.Ticks + date2.Ticks) / 2);
        }
    }

    [ApiController]
    [Route("backend")]
    public class BackendController : ControllerBase
    {
        private readonly ILogger<BackendController> _logger;

        private static readonly string connectionString = "host=postgres;port=5432;Database=WaterUnitData;Username=tkWaterUser;Password=waterUnitPassowrd;SSL mode=prefer;Pooling=true;MinPoolSize=1;MaxPoolSize=100;";
        private static readonly Func<DbConnection> postgressConnection = () => { return new NpgsqlConnection(connectionString); };

        private readonly TK_ORM DataBase = new(postgressConnection);
        private readonly MqttHandler mqtt;

        public BackendController(ILogger<BackendController> logger, MqttHandler mqtt)
        {
            _logger = logger;
            this.mqtt = mqtt;
        }

        [HttpPost("test")]
        public ActionResult Test()
        {
            return Ok(new SuccessResponseBody());
        }

        [HttpPost("manualCheck")]
        public async Task<ActionResult> ManualCheck(UnitIDBody request)
        {
            if (await mqtt.Publish($"unit/{request.UnitID}/checkManual", "true"))
                return Ok(new SuccessResponseBody());

            return BadRequest(new FailedResponseBody() { Message = "couldn't connect to mqtt server" });
        }

        [HttpPost("logs")]
        public ActionResult Logs(UnitIDBody request)
        {
            MqttHandler.UnitLogs.TryGetValue(request.UnitID, out var logs);

            if (logs == null)
                return BadRequest(new FailedResponseBody() { Message = "logs are empty" });
        
            return Ok(new SuccessResponseBody() { Message = logs.ToString()});
        }

        [HttpPost("receiveAllUnitsData")]
        public async Task<ActionResult> ReceiveUnitDataAsync(ReceiveAllUnitsDataRequestBody request)
        {

            LinkedList<UnitData> unitsData;
            try
            {
                unitsData = await DataBase.Select<UnitData>()
                                          .Where(unitData => unitData.UserID == request.UserID)
                                          .GetResult();
            }
            catch (NpgsqlException ex)
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
                UnitData? unit = await DataBase.Select<UnitData>()
                                                 .Where(unitData => unitData.UnitID == request.Unit.UnitID)
                                                 .GetResult()
                                                 .AsyncFirstOrDefault();

                if (unit == null)
                    return BadRequest(new FailedResponseBody() { Message = "!!update failed unit doesn't exist!!" });

                unit.UnitName           = request.Unit.UnitName;
                unit.MoistureThreshold  = request.Unit.MoistureThreshold;
                unit.UserID             = request.Unit.UserID;

                long count = await DataBase.Update(unit)
                                           .Where(unit => unit.UnitID == request.Unit.UnitID)
                                           .Execute();

                if (count <= 0)
                    return BadRequest(new FailedResponseBody() { Message = "!!update failed unit doesn't exist!!" });
            }
            catch (NpgsqlException ex)
            {
                return BadRequest(new FailedResponseBody() { Message = ex.Message });
            }

            return Ok( new UpdateUnitBody() { Unit = request.Unit });
        }

        [HttpPost("addUnitData")]
        public async Task<ActionResult> AddUnitData(AddUnitDataRequestBody request)
        {
            UnitData? addUnit; 
            try
            { 
                long unitsCount = await DataBase.Count<UnitData>()
                                                  .Where(unitData => unitData.UserID == request.UserID)
                                                  .GetAfflictedCount();

                addUnit = await DataBase.Select<UnitData>()
                                          .Where(unitData => unitData.UnitID == request.UnitID)
                                          .GetResult()
                                          .AsyncFirstOrDefault();

                if (addUnit == null)
                    return NotFound(new FailedResponseBody() { Message = $"no data with unitID: {request.UnitID} found" });

                addUnit.UserID = request.UserID;

                long count = await DataBase.Update(addUnit)
                                             .Where(unitData => unitData.UnitID == request.UnitID)
                                             .Execute();

                if (count <= 0)
                    return BadRequest(new FailedResponseBody() { Message = "!!update failed unit doesn't exist!!" });
            }
            catch (NpgsqlException ex)
            {
                return BadRequest(new FailedResponseBody() { Message = ex.Message });
            }

            return Ok(addUnit);
        }

        [HttpPost("removeUnitData")]
        public async Task<ActionResult> RemoveUnitData(RemoveUnitDataRequestBody request)
        {
            try
            {

                UnitData? unit = await DataBase.Select<UnitData>()
                                                 .Where(unitData => unitData.UnitID == request.Unit.UnitID)
                                                 .GetResult()
                                                 .AsyncFirstOrDefault();

                if (unit == null)
                    return BadRequest(new FailedResponseBody() { Message = "!!update failed unit doesn't exist!!" });

                unit.UserID = 0;
                await DataBase.Update(unit)
                                .Where(unitData => unitData.UnitID == request.Unit.UnitID)
                                .Execute();
            }
            catch (NpgsqlException ex)
            {
                return BadRequest(new FailedResponseBody() { Message = ex.Message });
            }

            return Ok(new SuccessResponseBody());
        }

        [HttpGet("getAvailableUnits")]
        public async Task<ActionResult> GetAvailableUnits()
        {
            LinkedList<UnitData> unitsData;
            try
            {
                unitsData = await DataBase.Select<UnitData>()
                                          .Where(unitData => unitData.UserID == 0)
                                          .GetResult();
            }
            catch (NpgsqlException ex)
            {
                return BadRequest(new FailedResponseBody() { Message = ex.Message });
            }

            return Ok(new GetAvailableUnitsResponseBody() { Units = unitsData });
        }

        [HttpPost("removeAvailableUnit")]
        public async Task<ActionResult> RemoveAvailableUnit(RemoveAvailebleUnitRequestBody request)
        {
            try
            {
                await DataBase.Remove<UnitHistory>()
                              .Where(unit => unit.UnitID == request.UnitID)
                              .Execute();

                long count = await DataBase.Remove<UnitData>()
                                           .Where(unit => unit.UserID == 0 && unit.UnitID == request.UnitID)
                                           .Execute();
                if(count <= 0) 
                    return BadRequest(new FailedResponseBody() { Message = "!!unit to be removed not found in database!!" });

            }
            catch (NpgsqlException ex)
            {
                return BadRequest(new FailedResponseBody() { Message = ex.Message });
            }

            return Ok(new SuccessResponseBody());
        }

        [HttpPost("UnitHistory")]
        public async Task<ActionResult> UnitHistory(UnitHistoryRequestBody request)
        {
            TimeStateEnum timeState = (TimeStateEnum)request.TimeState;

            DateTime timeBegin = DateTime.Parse(request.TimeSpan.TimeBegin);
            DateTime timeEnd   = DateTime.Parse(request.TimeSpan.TimeEnd);

            string unitID = request.UnitID;

            LinkedList<UnitHistory> history;
            try
            {
                history = await DataBase.Select<UnitHistory>()
                                        .Where(unit => unit.UnitID == request.UnitID)
                                        .Where(unit => unit.Timestamp >= timeBegin && unit.Timestamp <= timeEnd)
                                        .GetResult();
            }
            catch (NpgsqlException ex)
            {
                return BadRequest(new FailedResponseBody() { Message = ex.Message });
            }


            var timeGroups = GetTimeGrouping(history, timeState);

            HistoryUnitPoint[] historyUnits = timeGroups.Select(unit => new HistoryUnitPoint(unit.MoistureLevel, unit.MoistureThreshold, unit.Timestamp.ToString()))
                                                        .ToArray();

            return Ok(new UnitHistoryResponseBody() { HistoryUnits = historyUnits });
        }

        private static IEnumerable<UnitHistory> GetTimeGrouping(LinkedList<UnitHistory> history, TimeStateEnum timeState) 
        {
            var sameMonthGroups = history.GroupBy(unit => (unit.Timestamp.Year, unit.Timestamp.Month).GetHashCode());
            var sameDayGroups   = history.GroupBy(unit => (unit.Timestamp.Year, unit.Timestamp.Month, unit.Timestamp.Day).GetHashCode());
            var sameHourGroups  = history.GroupBy(unit => (unit.Timestamp.Year, unit.Timestamp.Month, unit.Timestamp.Day, unit.Timestamp.Hour).GetHashCode());

            if (timeState == TimeStateEnum.days)
                return sameMonthGroups.Select(group => group.GroupTime(TimeStateEnum.days).GetAverageUnitHistory())          
                                      .Flatten();

            if (timeState == TimeStateEnum.hours)
                return sameDayGroups.Select(group => group.GroupTime(TimeStateEnum.hours).GetAverageUnitHistory())
                                    .Flatten();

            return sameHourGroups.Select(group => group.GroupTime(TimeStateEnum.TenMinutes).GetAverageUnitHistory())
                                 .Flatten();
        }
    }
}
