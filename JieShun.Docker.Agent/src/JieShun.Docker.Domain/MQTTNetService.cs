using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using JieShun.Docker.Domain.Models;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;

namespace JieShun.Docker.Domain
{
    public class MQTTNetService
    {
        Func<ReceviceMessage, Task> MqMsgCallBack;

        IMqttClient mqttClient;

        IMqttClientOptions mqoptions;

        private ConcurrentDictionary<string, MqttTopicFilter> dicSubscribe = new ConcurrentDictionary<string, MqttTopicFilter>();
        public async Task ConnectAsync()
        {
            if (mqttClient == null)
            {
                mqttClient = new MqttFactory().CreateMqttClient();
            }

            var mqBuild = new MqttClientOptionsBuilder().WithTcpServer("127.0.0.1", 1883);

            mqoptions = mqBuild.WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V310).Build();

            mqttClient.UseApplicationMessageReceivedHandler(ReceivedHandler);

            mqttClient.UseConnectedHandler(ConnectedHandler);

            mqttClient.UseDisconnectedHandler(DisconnectedHandler);

            Console.WriteLine("开始连接MQTT");

            try
            {
                var mqttConnectResult = await mqttClient.ConnectAsync(mqoptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task ReceivedHandler(MqttApplicationMessageReceivedEventArgs e)
        {
            if (e.ApplicationMessage.Payload != null)
            {
                ReceviceMessage revMsg = new ReceviceMessage();
                revMsg.clientId = e.ClientId;
                revMsg.topic = e.ApplicationMessage.Topic;
                revMsg.payload = e.ApplicationMessage.Payload;

                if (MqMsgCallBack != null)
                {
                    await MqMsgCallBack(revMsg);
                }
            }
        }

        private async Task ConnectedHandler(MqttClientConnectedEventArgs e)
        {
            Console.WriteLine("已连接MQTT");

            if (dicSubscribe.Count > 0)
            {
                foreach (var item in dicSubscribe)
                {
                    await mqttClient.SubscribeAsync(item.Value);
                }
            }
        }

        private async Task DisconnectedHandler(MqttClientDisconnectedEventArgs e)
        {
            if (!mqttClient.IsConnected)
            {
                Console.WriteLine("MQTT连接已断开");
                try
                {
                    await mqttClient.ReconnectAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                await Task.Delay(500);
            }
        }

        public void SetMqMsgCallBack(Func<ReceviceMessage, Task> msgCallBack)
        {
            MqMsgCallBack += msgCallBack;
        }

        public async Task Subscribe(string topic, int qos = 0)
        {
            var subscribe = new MqttTopicFilterBuilder().WithTopic(topic);
            subscribe.WithAtMostOnceQoS();

            var topicfilter = subscribe.Build();

            await mqttClient.SubscribeAsync(topicfilter);

            dicSubscribe.AddOrUpdate(topic, topicfilter, (k, v) => topicfilter);
        }

        public async Task PublicshAsync(string topic, byte[] payload, int retain = 0, int qos = 0)
        {
            if (mqttClient != null)
            {
                bool retainFlag = retain == 1;

                var mqMessageBuilder = new MqttApplicationMessageBuilder().WithTopic(topic).WithPayload(payload).WithRetainFlag(retainFlag);

                mqMessageBuilder.WithAtMostOnceQoS();

                var result = await mqttClient.PublishAsync(mqMessageBuilder.Build());

                if (result.ReasonCode != MQTTnet.Client.Publishing.MqttClientPublishReasonCode.Success)
                {
                    Console.WriteLine("发送消息失败，topic[" + topic + "],Code:" + result.ReasonCode.ToString());
                }
            }
        }
    }
}
