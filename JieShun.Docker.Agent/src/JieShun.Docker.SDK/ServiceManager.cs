﻿using Docker.DotNet.Models;
using JieShun.Docker.Domain;
using JieShun.Docker.Domain.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JieShun.Docker.SDK
{
    public class ServiceManager
    {
        private DockerSDK _dockerSdk;

        private const string GetContainers = "SERVICES";

        private const string CreateContainer = "CREATE";

        private const string UpgradeContainer = "UPDATE";

        private const string ExportContainer = "EXPORT";

        MQTTNetService _mQTTNetService;

        public ServiceManager(MQTTNetService mQTTNetService, DockerSDK dockerSdk)
        {
            _mQTTNetService = mQTTNetService;
            _dockerSdk = dockerSdk;

            mQTTNetService.SetMqMsgCallBack(DockerTopics.ServicesTopic, MsgCallBack);
            mQTTNetService.Subscribe(DockerTopics.ServicesTopic);
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
                          
                            //进行MQTT响应
                            ResponseBase<List<ContainerListResponse>> responseData = new ResponseBase<List<ContainerListResponse>>();
                            responseData.code = 200;
                            responseData.message = "";

                            try
                            {
                                var containers = _dockerSdk.GetContainers(msg).Result;
                                responseData.data = containers;
                            }
                            catch (Exception ee)
                            {
                                responseData.code = 406;
                                responseData.message = ee.Message;
                            }
                   

                            byte[] containersBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(responseData));
                            _mQTTNetService.PublicshAsync(DockerTopics.ServicesACK, containersBytes);

                            break;
                        case CreateContainer:
                            bool success = false;
                            //进行MQTT响应
                            ResponseBase<CreateContainerResponse> responseCreateData = new ResponseBase<CreateContainerResponse>();
                            responseCreateData.code = 200;
                            responseCreateData.message = "";

                            try
                            {
                                var createcontainResult = _dockerSdk.CreateContainerAsync(msg).Result;
                                if (createcontainResult != null && !string.IsNullOrWhiteSpace(createcontainResult.ID))
                                {
                                    success = _dockerSdk.StartContainerAsync(createcontainResult.ID).Result;
                                }
                                if (!success)
                                {
                                    responseCreateData.code = 406;
                                    responseCreateData.message = "容器启动失败，可能主机端口已被占用";
                                }
                                responseCreateData.data = createcontainResult;
                            }
                            catch (Exception e)
                            {
                                responseCreateData.code = 406;
                                responseCreateData.message = e.Message;
                            }

                            byte[] createBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(responseCreateData));
                            _mQTTNetService.PublicshAsync(DockerTopics.ServicesCreateACK, createBytes);

                            break;
                        case UpgradeContainer:
                            ResponseBase<CreateContainerResponse> responseUpgradeData = new ResponseBase<CreateContainerResponse>();
                            responseUpgradeData.code = 200;
                            responseUpgradeData.message = "";

                            try
                            {
                                var upgradecontainResult = _dockerSdk.UpgradeContainerAsync(msg).Result;

                                //进行MQTT响应
                                responseUpgradeData.data = upgradecontainResult;
                            }
                            catch (Exception ex)
                            {
                                responseUpgradeData.code = 406;
                                responseUpgradeData.message = ex.Message;
                            }


                            byte[] upgradeBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(responseUpgradeData));
                            _mQTTNetService.PublicshAsync(DockerTopics.ServicesUpgradeACK, upgradeBytes);

                            break;
                        case ExportContainer:
                            _dockerSdk.ExportContainerAsync(msg);
                            //暂不实现

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
