using Docker.DotNet;
using System;
using Docker.DotNet.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using JieShun.Docker.Domain;
using JieShun.Docker.Domain.Models;
using System.Reflection;
using Newtonsoft.Json;
using System.Text;
using System.IO;

namespace JieShun.Docker.SDK
{
    /// <summary>
    /// docker sdk
    /// </summary>
    public class DockerSDK
    {
        private static DockerClient _client;

        private static MQTTNetService mQTTNetService;

        public DockerSDK()
        {
            if (_client == null)
            {
                _client = new DockerClientConfiguration(new Uri("unix:/var/run/docker.sock")).CreateClient();
            }

            if (mQTTNetService == null)
            {
                mQTTNetService = new MQTTNetService();
                mQTTNetService.SetMqMsgCallBack(MsgCallBack);
                mQTTNetService.ConnectAsync();
            }
        }

        /// <summary>
        /// 查询镜像
        /// </summary>
        /// <param name="revMsg"></param>
        /// <returns></returns>
        public async Task<List<ImagesListResponse>> GetImages(ReceviceMessage revMsg)
        {
            string data = Encoding.UTF8.GetString(revMsg.payload);
            ImagesListParameters parameters = JsonConvert.DeserializeObject<ImagesListParameters>(data);
            var list = await _client.Images.ListImagesAsync(parameters);
            return list.ToList();
        }

        /// <summary>
        /// 查询容器
        /// </summary>
        /// <param name="revMsg"></param>
        /// <returns></returns>
        public async Task<List<ContainerListResponse>> GetContainers(ReceviceMessage revMsg)
        {
            string data = Encoding.UTF8.GetString(revMsg.payload);
            ContainersListParameters parameters = JsonConvert.DeserializeObject<ContainersListParameters>(data);
            var list = await _client.Containers.ListContainersAsync(parameters);
            return list.ToList();
        }

        /// <summary>
        /// 容器部署
        /// </summary>
        /// <param name="revMsg"></param>
        /// <returns></returns>
        public async Task<CreateContainerResponse> CreateContainerAsync(ReceviceMessage revMsg)
        {
            string data = Encoding.UTF8.GetString(revMsg.payload);
            CreateContainerParameters parameters = JsonConvert.DeserializeObject<CreateContainerParameters>(data);
            var result = await _client.Containers.CreateContainerAsync(parameters);

            //TODO:  订阅mqtt  升级result.ID

            return result;
        }

        public async Task<CreateContainerResponse> UpgradeContainerAsync(ReceviceMessage revMsg)
        {
            string id = revMsg.topic;//TODO: 从topic中找出Id

            //先停止容器
            await _client.Containers.StopContainerAsync(id, new ContainerStopParameters { });

            string data = Encoding.UTF8.GetString(revMsg.payload);
            CreateContainerParameters parameters = JsonConvert.DeserializeObject<CreateContainerParameters>(data);
            var result = await _client.Containers.CreateContainerAsync(parameters);

            if (result != null && !string.IsNullOrWhiteSpace(result.ID))
            {
                //TODO:  订阅mqtt  升级result.ID

                //移除之前容器
                await _client.Containers.RemoveContainerAsync(id, new ContainerRemoveParameters { });
            }
            return result;
        }

        public async Task ExportContainerAsync(ReceviceMessage revMsg)
        {
            string id = revMsg.topic;//TODO: 从topic中找出Id

            await _client.Containers.ExportContainerAsync(id);
        }

        /// <summary>
        /// mqtt call back
        /// </summary>
        /// <param name="revMsg"></param>
        /// <returns></returns>
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

                //Type t = typeof(DockerSDK);//类名
                //MethodInfo mt = t.GetMethod(nameof(DockerTopics.GetImages));//加载方法
                //mt.Invoke(this, new object[] { msg });
            }
            catch (Exception ex)
            {
                Console.WriteLine("消息处理失败:" + ex.Message);
            }
        }
    }
}
