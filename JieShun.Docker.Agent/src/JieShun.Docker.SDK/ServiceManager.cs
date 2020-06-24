using JieShun.Docker.Domain;
using JieShun.Docker.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JieShun.Docker.SDK
{
    public class ServiceManager
    {
        private DockerSDK dockerSdk;

        private const string GetContainers = "SERVICES";

        private const string CreateContainer = "CREATE";

        private const string UpgradeContainer = "UPDATE";

        private const string ExportContainer = "EXPORT";

        public ServiceManager(MQTTNetService mQTTNetService)
        {
            mQTTNetService.SetMqMsgCallBack(DockerTopics.ServicesTopic, MsgCallBack);

            dockerSdk = new DockerSDK();
        }

        private async Task MsgCallBack(ReceviceMessage revMsg)
        {
            Task.Factory.StartNew(OnRecive, revMsg);
            await Task.CompletedTask;
        }

        /// <summary>
        /// mqtt msg
        /// </summary>
        /// <param name="data"></param>
        private void OnRecive(object data)
        {
            try
            {
                ReceviceMessage msg = data as ReceviceMessage;

                string[] topicParam = msg.topic.Split('/');

                if (topicParam.Length > 0)
                {
                    switch (topicParam[topicParam.Length - 1].ToUpper())
                    {
                        case GetContainers:
                            dockerSdk.GetContainers(msg);
                            //TODO: 进行MQTT响应
                            break;
                        case CreateContainer:
                            dockerSdk.CreateContainerAsync(msg);

                            //TODO: 进行MQTT响应
                            break;
                        case UpgradeContainer:
                            dockerSdk.UpgradeContainerAsync(msg);

                            //TODO: 进行MQTT响应
                            break;
                        case ExportContainer:
                            dockerSdk.ExportContainerAsync(msg);

                            //TODO: 进行MQTT响应
                            break;
                        default:
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("消息处理失败:" + ex.Message);
            }
        }
    }
}
