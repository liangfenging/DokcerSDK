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
    public class ServiceManager
    {
        private DockerSDK _dockerSdk;

        private const string GetContainers = "SERVICES";

        private const string CreateContainer = "CREATE";

        private const string UpgradeContainer = "UPDATE";

        private const string ExportContainer = "EXPORT";

        private const string StartContainer = "START";

        private const string StopContainer = "STOP";


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
                bool success = false;
                ReceviceMessage msg = data as ReceviceMessage;
                string jsondata = "";
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
                        case GetContainers:

                            //进行MQTT响应
                            ResponseBase<List<ContainerListResponse>> responseData = new ResponseBase<List<ContainerListResponse>>();
                            responseData.code = 200;
                            responseData.message = "";
                            responseData.transId = transId;
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
                            jsondata = JsonConvert.SerializeObject(responseData);

                            byte[] containersBytes = Encoding.UTF8.GetBytes(jsondata);
                            _mQTTNetService.PublicshAsync(DockerTopics.ServicesACK, containersBytes);

                            break;
                        case CreateContainer:
                            
                            //进行MQTT响应
                            ResponseBase<CreateContainerResponse> responseCreateData = new ResponseBase<CreateContainerResponse>();
                            responseCreateData.code = 200;
                            responseCreateData.message = "";
                            responseCreateData.transId = transId;
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
                            jsondata = JsonConvert.SerializeObject(responseCreateData);
                            byte[] createBytes = Encoding.UTF8.GetBytes(jsondata);
                            _mQTTNetService.PublicshAsync(DockerTopics.ServicesCreateACK, createBytes);

                            break;
                        case UpgradeContainer:
                            ResponseBase<CreateContainerResponse> responseUpgradeData = new ResponseBase<CreateContainerResponse>();
                            responseUpgradeData.code = 200;
                            responseUpgradeData.message = "";
                            responseUpgradeData.transId = transId;
                            try
                            {
                                var upgradecontainResult = _dockerSdk.UpgradeContainerAsync(msg).Result;
                                if (upgradecontainResult != null && !string.IsNullOrWhiteSpace(upgradecontainResult.ID))
                                {
                                    success = _dockerSdk.StartContainerAsync(upgradecontainResult.ID).Result;
                                }
                                if (!success)
                                {
                                    responseUpgradeData.code = 406;
                                    responseUpgradeData.message = "容器启动失败，可能主机端口已被占用";
                                }
                                //进行MQTT响应
                                responseUpgradeData.data = upgradecontainResult;
                            }
                            catch (Exception ex)
                            {
                                responseUpgradeData.code = 406;
                                responseUpgradeData.message = ex.Message;
                            }

                            jsondata = JsonConvert.SerializeObject(responseUpgradeData);
                            byte[] upgradeBytes = Encoding.UTF8.GetBytes(jsondata);
                            _mQTTNetService.PublicshAsync(DockerTopics.ServicesUpgradeACK, upgradeBytes);

                            break;
                        case ExportContainer:
                            _dockerSdk.ExportContainerAsync(msg);
                            //暂不实现

                            break;
                        case StartContainer:
                            ResponseBase responseStartData = new ResponseBase();
                            responseStartData.code = 200;
                            responseStartData.message = "";
                            responseStartData.transId = transId;

                            try
                            {
                                string id = topicParam[topicParam.Length - 2];
                                success = _dockerSdk.StartContainerAsync(id).Result;
                                if (!success)
                                {
                                    responseStartData.code = 406;
                                    responseStartData.message = "容器启动失败，可能主机端口已被占用";
                                }
                            }
                            catch (Exception ex)
                            {
                                responseStartData.code = 406;
                                responseStartData.message = ex.Message;
                            }

                            jsondata = JsonConvert.SerializeObject(responseStartData);
                            byte[] startBytes = Encoding.UTF8.GetBytes(jsondata);
                            _mQTTNetService.PublicshAsync(DockerTopics.ServicesStartACK, startBytes);
                            break;
                        case StopContainer:
                            ResponseBase responseStopData = new ResponseBase();
                            responseStopData.code = 200;
                            responseStopData.message = "";
                            responseStopData.transId = transId;

                            try
                            {
                                string id = topicParam[topicParam.Length - 2];
                                success = _dockerSdk.StopContainerAsync(id).Result;
                                if (!success)
                                {
                                    responseStopData.code = 406;
                                    responseStopData.message = "容器停止失败";
                                }
                            }
                            catch (Exception ex)
                            {
                                responseStopData.code = 406;
                                responseStopData.message = ex.Message;
                            }

                            jsondata = JsonConvert.SerializeObject(responseStopData);
                            byte[] stopBytes = Encoding.UTF8.GetBytes(jsondata);
                            _mQTTNetService.PublicshAsync(DockerTopics.ServicesStopACK, stopBytes);
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
