using System.Text.Json;
using MQTTnet.Protocol;
using MQTTnet.Client;
using MQTTnet.Server;
using System.Text;
using MQTTnet;

namespace WebApplication1.Mqtt
{
    public class EasyMqtt
    {
        private readonly IMqttClient mqttClient;
        public Func<MqttPayload, Task>? MessageHandler { private get; set; }

        public EasyMqtt(Func<MqttPayload, Task> messageHandler)
        {
            MessageHandler = messageHandler;
            mqttClient = new MqttFactory().CreateMqttClient();
        }

        public EasyMqtt()
        {
            MessageHandler = null;
            mqttClient = new MqttFactory().CreateMqttClient();
        }

        public async Task<bool> Connect(string configPath, MqttLastWillOptions? lastWill = null)
        {
            StreamReader sr = new(configPath);
            string rawJson = sr.ReadToEnd();

            MqttConfig? json = JsonSerializer.Deserialize<MqttConfig>(rawJson, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            if (json == null)
            {
                await Console.Out.WriteLineAsync("!!<error> mqtt.config.json can not be Deserialized!!");
                return false;
            }

            var optionBuilder = new MqttClientOptionsBuilder()
                              .WithTcpServer(json.Host, json.Port)
                              .WithCredentials(json.Username, json.Password)
                              .WithCleanSession();

            if (lastWill != null)
                optionBuilder.WithWillTopic(lastWill.Topic)
                             .WithWillPayload(lastWill.Message)
                             .WithWillQualityOfServiceLevel(lastWill.QualityOfServiceLevel)
                             .WithWillRetain(lastWill.Retain);

            MqttClientOptions options = optionBuilder.Build();

            await Console.Out.WriteLineAsync("\nstarting mqtt connection...\n");

            MqttClientConnectResult? connectResult = default;
            try
            {
                connectResult = await mqttClient.ConnectAsync(options);
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.ToString());
            }

            if (connectResult == null || !IsConnectionSuccess(connectResult))
            {
                string reason = connectResult?.ReasonString ?? "connectResult.ReasonString not found";
                await Console.Out.WriteLineAsync($"!!<error> connection not successful: {reason}!!");
                return false;
            }

            await SubscribeTopics(json.Subscribe_topics);
            await PublishTopics(json.Publish_topics);

            return true;
        }

        public async Task<bool> Connect(MqttLastWillOptions? lastWill = null)
        {
            return await Connect("Mqtt/Mqtt.config.json", lastWill);
        }

        public bool IsConnected()
        {
            return mqttClient.IsConnected;
        }

        public async Task<bool> Publish(string topic, string message)
        {
            return await Publish(new(topic, message));
        }

        public async Task<bool> Subscribe(string topic)
        {
            try
            {
                await mqttClient.SubscribeAsync(topic);
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync($"!!<error> while subscribing[topic:{topic}]: {ex.Message}!!");
                return false;
            }

            return true;
        }

        private async Task<bool> SubscribeTopics(List<string> topics)
        {
            foreach (string topic in topics)
            {
                bool isSuccess = await Subscribe(topic);

                if (!isSuccess)
                    return false;
            }

            if (MessageHandler != null)
                mqttClient.ApplicationMessageReceivedAsync += MessageReceivedEventHandler;

            return true;
        }

        private async Task MessageReceivedEventHandler(MqttApplicationMessageReceivedEventArgs args)
        {
            string topic = args.ApplicationMessage.Topic;

            byte[]? payLoad = args.ApplicationMessage.PayloadSegment.Array;
            if (payLoad == null)
            {
                await Console.Out.WriteLineAsync("!!error!! message is empty");
                return;
            }

            string? message = PayloadToString(payLoad);

            try
            {
                if (MessageHandler != null)
                    await MessageHandler(new MqttPayload(topic, message));
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync($"!!<error> messageHandler throws exception[topic: {topic}, message: {message}]: {ex}!!");
            }
        }

        private static string PayloadToString(byte[] payload)
        {
            return Encoding.UTF8.GetString(payload);
        }

        private async Task<bool> PublishTopics(List<MqttPayload> publishes)
        {
            foreach (MqttPayload publish in publishes)
            {
                bool isSuccess = await Publish(publish);

                if (!isSuccess)
                    return false;
            }

            return true;
        }

        private async Task<bool> Publish(MqttPayload publish)
        {
            MqttApplicationMessage message = new MqttApplicationMessageBuilder()
                .WithTopic(publish.Topic)
                .WithPayload(publish.Message)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag()
                .Build();

            try
            {
                await mqttClient.PublishAsync(message);
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync($"!!<error> while publishing[topic:\"{publish.Topic}\", message: \"{publish.Message}\"] error-message: {ex.Message}!!");
                return false;
            }

            return true;

        }

        private static bool IsConnectionSuccess(MqttClientConnectResult connectResult)
        {
            return connectResult.ResultCode == MqttClientConnectResultCode.Success;
        }
    }
}
