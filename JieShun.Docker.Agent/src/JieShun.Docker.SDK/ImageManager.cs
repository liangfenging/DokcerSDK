using Docker.DotNet;
using JieShun.Docker.Domain;
using JieShun.Docker.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JieShun.Docker.SDK
{
    public class ImageManager
    {
        private const string GetImages = "IMAGES";

        private DockerSDK dockerSdk;

        public ImageManager(MQTTNetService mQTTNetService)
        {
            mQTTNetService.SetMqMsgCallBack(DockerTopics.ImagesTopic, MsgCallBack);

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
                        case GetImages:
                             dockerSdk.GetImages(msg);

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
