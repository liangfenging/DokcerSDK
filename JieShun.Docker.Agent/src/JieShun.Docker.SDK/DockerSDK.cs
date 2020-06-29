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


        public DockerSDK(string url)
        {
            if (_client == null)
            {
                if (string.IsNullOrWhiteSpace(url))
                {
                    url = "unix:/var/run/docker.sock";
                }
                _client = new DockerClientConfiguration(new Uri(url)).CreateClient();
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
            if (string.IsNullOrWhiteSpace(parameters.MatchName))
            {
                parameters.All = true;
            }
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
            return result;
        }

        public async Task<CreateContainerResponse> UpgradeContainerAsync(ReceviceMessage revMsg)
        {
            string[] topicParam = revMsg.topic.Split('/');
            string id = topicParam[topicParam.Length - 2];

            //先停止容器
            await _client.Containers.StopContainerAsync(id, new ContainerStopParameters { });

            string data = Encoding.UTF8.GetString(revMsg.payload);
            CreateContainerParameters parameters = JsonConvert.DeserializeObject<CreateContainerParameters>(data);
            var result = await _client.Containers.CreateContainerAsync(parameters);

            if (result != null && !string.IsNullOrWhiteSpace(result.ID))
            {
                //移除之前容器
                await _client.Containers.RemoveContainerAsync(id, new ContainerRemoveParameters { });
            }
            return result;
        }

        public async Task<Stream> ExportContainerAsync(ReceviceMessage revMsg)
        {
            string[] topicParam = revMsg.topic.Split('/');
            string id = topicParam[topicParam.Length - 2];

            var result = await _client.Containers.ExportContainerAsync(id);

            return result;
        }

    }
}
