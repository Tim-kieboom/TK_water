using MQTTnet.Protocol;

namespace WebApplication1.Mqtt
{
    public class MqttLastWillOptions
    {
        public string Topic { get; set; }
        public string Message { get; set; }
        public MqttQualityOfServiceLevel QualityOfServiceLevel { get; set; }
        public bool Retain { get; set; }

        public MqttLastWillOptions(string topic, string message = "", bool retain = true, MqttQualityOfServiceLevel qualityOfServiceLevel = MqttQualityOfServiceLevel.ExactlyOnce)
        {
            Topic = topic;
            Message = message;
            Retain = retain;
            QualityOfServiceLevel = qualityOfServiceLevel;
        }
    }

    public class MqttPayload
    {
        public string Topic { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public MqttPayload(string topic, string message)
        {
            Topic = topic;
            Message = message;
        }

        public void Print()
        {
            Console.WriteLine($"[mqtt({Topic})]: {Message}");
        }

        public async Task PrintAsync() 
        {
            await Console.Out.WriteLineAsync($"[mqtt({Topic})]: {Message}");
        }
    }

    public class MqttConfig
    {
        public string Password { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;

        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }

        public List<string> Subscribe_topics { get; set; } = new();
        public List<MqttPayload> Publish_topics { get; set; } = new();
    }
}
