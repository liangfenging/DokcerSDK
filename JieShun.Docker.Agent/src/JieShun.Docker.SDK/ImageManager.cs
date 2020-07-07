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

        private const string CreateImages = "CREATE";

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
                string transId = "";
                RequestBase requestdata = JsonConvert.DeserializeObject<RequestBase>(Encoding.UTF8.GetString(msg.payload));
                if (requestdata == null || string.IsNullOrWhiteSpace(requestdata.transId))
                {
                    transId = Guid.NewGuid().ToString("N");
                }
                else
                {
                    transId = requestdata.transId;
                }
                if (topicParam.Length > 0)
                {
                    switch (topicParam[topicParam.Length - 1].ToUpper())
                    {
                        case GetImages:
                            //进行MQTT响应
                            ResponseBase<List<ImagesListResponse>> responseData = new ResponseBase<List<ImagesListResponse>>();
                            responseData.code = 200;
                            responseData.message = "";
                            responseData.transId = transId;
                            try
                            {
                                var resultImages = _dockerSdk.GetImages(msg).Result;
                                responseData.data = resultImages;
                            }
                            catch (Exception e)
                            {
                                responseData.code = 406;
                                responseData.message = e.Message;
                            }

                            string jsondata = JsonConvert.SerializeObject(responseData);

                            byte[] imagesBytes = Encoding.UTF8.GetBytes(jsondata);
                            _mQTTNetService.PublicshAsync(DockerTopics.ImagesACK, imagesBytes);

                            break;

                        case CreateImages:
                            //进行MQTT响应
                            ResponseBase responseCreateData = new ResponseBase();
                            responseCreateData.code = 200;
                            responseCreateData.message = "";
                            responseCreateData.transId = transId;
                            try
                            {
                                _dockerSdk.CreateImageAsync(msg).Wait();

                            }
                            catch (Exception e)
                            {
                                responseCreateData.code = 406;
                                responseCreateData.message = e.Message;
                            }

                            string jsonCreatedata = JsonConvert.SerializeObject(responseCreateData);

                            byte[] imagesCreateBytes = Encoding.UTF8.GetBytes(jsonCreatedata);
                            _mQTTNetService.PublicshAsync(DockerTopics.ImagesCreateACK, imagesCreateBytes);
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
