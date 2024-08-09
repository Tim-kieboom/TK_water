using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Collections.Concurrent;
using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebApplication1.Controllers.controlCentrumBodys.postBackUpRecord;
using WebApplication1.Controllers.controlCentrumBodys.postUnitMeasurement;
using WebApplication1.Controllers.controlCentrumBodys.signIn;
using WebApplication1.data;
using WebApplication1.data.ORM;
using WebApplication1.Mqtt;

namespace WebApplication1.Controllers
{
    public class MqttHandler
    {
        private static readonly string connectionString = "host=postgres;port=5432;Database=WaterUnitData;Username=tkWaterUser;Password=waterUnitPassowrd;SSL mode=prefer;Pooling=true;MinPoolSize=1;MaxPoolSize=100;";
        private static readonly Func<DbConnection> postgressConnection = () => { return new NpgsqlConnection(connectionString); };
        private readonly TK_ORM DataBase = new(postgressConnection);

        private static readonly MqttLastWillOptions lastWill = new("backend/online", "0");

        public EasyMqtt Mqtt { get; set; }
        public static ConcurrentDictionary<string, StringBuilder> UnitLogs { get; set; } = new();

        public MqttHandler()
        {
            Mqtt = new(CallBackHandler);
        }

        public async Task<bool> Connect()
        {
            try
            {
                await Mqtt.Connect(lastWill);
            }
            catch(Exception ex)
            {
                Console.WriteLine("[mqtt] connection failed: " + ex.Message);
                return false;
            }

            if (Mqtt.IsConnected())
            {
                Console.WriteLine("[mqtt] connection successful");
                return true;
            }

            Console.WriteLine("[mqtt] connection failed reason unknown");
            return false;
        }

        public async Task<bool> Publish(string topic, string message)
        {
            if (!Mqtt.IsConnected())
                throw new InvalidOperationException("tried to publish something while connection is not open");

            return await Mqtt.Publish(topic, message);
        }

        public async Task<bool> Subscribe(string topic)
        {
            if (!Mqtt.IsConnected())
                throw new InvalidOperationException("tried to Subscribe to something something while connection is not open");

            return await Mqtt.Subscribe(topic); 
        }

        public async Task CallBackHandler(MqttPayload payload)
        {
            await payload.PrintAsync();

            string baseTopic = "";
            string unitID = "";
            string topicType = "";
            int amountOfParts = -1;
            try
            {
                string[] topicParts = payload.Topic.Split('/');

                amountOfParts = topicParts.Length;

                unitID = topicParts[1];
                baseTopic = topicParts[0]+'/'+unitID;
                topicType = topicParts[2];
            }
            catch (IndexOutOfRangeException) { }

            if (amountOfParts > 3 || amountOfParts == -1)
                return;

            switch (topicType)
            {
                case "serialLog":
                    StoreLog(unitID, payload.Message);
                    break;

                case "signIn":
                    await SignIn(payload.Message);
                    break;

                case "postUnitMeasurement":
                    await PostUnitMeasurement(payload.Message, baseTopic);
                    break;

                case "postBackUpRecord":
                    await PostBackUpRecord(payload.Message);
                    break;

                default: 
                    break;
            }
        }

        private static void StoreLog(string unitID, string message)
        {
            UnitLogs.TryGetValue(unitID, out var unitLogs);

            unitLogs ??= new();
            unitLogs.AppendLine($"{DateTime.Now}|\t{message}");

            UnitLogs[unitID] = unitLogs;
        }

        private async Task SignIn(string unitID)
        {
            try
            {
                long unitCount = await DataBase.Select<UnitData>()
                                               .Where(unit => unit.UnitID == unitID)
                                               .GetAfflictedCount();

                if (unitCount > 0)
                    return;

                UnitData unit = new(0, unitID, "unitName", 60, 0);

                if (!await DataBase.Insert(unit))
                    return;

            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            return;
        }

        private async Task PostUnitMeasurement(string rawJson, string baseTopic)
        {
            var request = JsonSerializer.Deserialize<PostUnitMeasurementRequestBody>(rawJson, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            if (request == null)
                return;

            UnitData? unitData;
            Byte moistureThreshold = 101;
            try
            {
                unitData = await DataBase.Select<UnitData>()
                                           .Where(unitData => unitData.UnitID == request.UnitID)
                                           .GetResult()
                                           .AsyncFirstOrDefault();

                if (unitData == null)
                    return;

                try
                {
                    moistureThreshold = Convert.ToByte(unitData.MoistureThreshold);
                }
                catch (OverflowException) { }

                if (request.MoistureLevel == unitData.MoistureLevel)
                {
                    await UpdateUnitHistory(unitData);
                    await Mqtt.Publish(baseTopic + "/postUnitMeasurement_return", moistureThreshold.ToString());
                    return;
                }

                unitData.MoistureLevel = request.MoistureLevel;
                await DataBase.Update(unitData)
                                .Where(unit => unit.UnitID == unitData.UnitID)
                                .Execute();

                await UpdateUnitHistory(unitData);
            }
            catch (NpgsqlException)
            {
                return;
            }

            await Mqtt.Publish(baseTopic + "/postUnitMeasurement_return", moistureThreshold.ToString());
        }

        private async Task PostBackUpRecord(string rawJson)
        {
            var request = JsonSerializer.Deserialize<PostBackUpRecordRequestBody>(rawJson, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            if (request == null)
                return;

            UnitData? unitData;
            try
            {
                unitData = await DataBase.Select<UnitData>()
                                           .Where(unitData => unitData.UnitID == request.UnitID)
                                           .GetResult()
                                           .AsyncFirstOrDefault();

                if (unitData == null)
                    return;

                unitData.MoistureLevel = request.Record.MoistureLevel;
                DateTime time = DateTime.Parse(request.Record.DateTime);
                await UpdateUnitHistory(unitData, time);
            }
            catch (NpgsqlException)
            {
                return;
            }
        }

        private async Task UpdateUnitHistory(UnitData currentUnit, DateTime? time = null)
        {
            UnitHistory newHistory = new(currentUnit, time ?? DateTime.Now);
            await DataBase.Insert(newHistory);
        }
    }
}
