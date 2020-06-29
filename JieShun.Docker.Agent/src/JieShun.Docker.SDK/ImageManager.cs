using Docker.DotNet;
using Docker.DotNet.Models;
using JieShun.Docker.Domain;
using JieShun.Docker.Domain.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JieShun.Docker.SDK
{
    public class ImageManager
    {
        private const string GetImages = "IMAGES";

        private DockerSDK _dockerSdk;

        MQTTNetService _mQTTNetService;

        public ImageManager(MQTTNetService mQTTNetService, DockerSDK dockerSdk)
        {
            _mQTTNetService = mQTTNetService;
            _dockerSdk = dockerSdk;

            mQTTNetService.SetMqMsgCallBack(DockerTopics.ImagesTopic, MsgCallBack);
            mQTTNetService.Subscribe(DockerTopics.ImagesTopic);
           
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
                            var resultImages = _dockerSdk.GetImages(msg).Result;

                            //进行MQTT响应
                            ResponseBase<List<ImagesListResponse>> responseData = new ResponseBase<List<ImagesListResponse>>();
                            responseData.code = 200;
                            responseData.message = "";
                            responseData.data = resultImages;

                            byte[] imagesBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(responseData));
                            _mQTTNetService.PublicshAsync(DockerTopics.ImagesACK, imagesBytes);

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
