using Docker.DotNet.Models;
using JieShun.Docker.Domain;
using JieShun.Docker.Domain.Models;
using JieShun.Docker.SDK;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace JieShun.Docker.Agent
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            //DockerSDK dockerSDK = new DockerSDK("http://192.168.29.128:2375");

            //ReceviceMessage getImages = new ReceviceMessage();
            //getImages.topic = "nm/V1/projectNo/docker/images";
            //ImagesListInputParameters imagesParam = new ImagesListInputParameters();
            //imagesParam.All = true;
            //string jsondata = JsonConvert.SerializeObject(imagesParam);

            //getImages.payload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(imagesParam));
            //var images = dockerSDK.GetImages(getImages).Result;


            //ReceviceMessage createContainer = new ReceviceMessage();
            //createContainer.topic = "nm/V1/projectNo/docker/services/ca325e47098c/update";

            CreateContainerParameters createContainerParameters = new CreateContainerParameters();
            //createContainerParameters.ExposedPorts = new Dictionary<string, EmptyStruct>();
            //createContainerParameters.ExposedPorts.Add("80/tcp", default(EmptyStruct));

            //Dictionary<string, IList<PortBinding>> portBindings = new Dictionary<string, IList<PortBinding>>();
            //portBindings.Add("80/tcp", new List<PortBinding> { new PortBinding { HostIP = "0.0.0.0", HostPort = "8990" } });

            //createContainerParameters.HostConfig = new HostConfig { PortBindings = portBindings, AutoRemove = true };
            //createContainerParameters.Image = "webtest:0.1";


            //createContainer.payload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(createContainerParameters));

            //var result = dockerSDK.UpgradeContainerAsync(createContainer).Result;

            //if (result != null && !string.IsNullOrWhiteSpace(result.ID))
            //{
            //    var startResult = dockerSDK.StartContainerAsync(result.ID);
            //}

            //string jsondata = JsonConvert.SerializeObject(result);

            //string testdata = JsonConvert.SerializeObject(createContainerParameters);
            //Console.WriteLine(jsondata);

            string dockerapiUrl = "";//http://192.168.29.128:2375
            if (args != null && args.Length > 0)
            {
                dockerapiUrl = args[0];
            }
            MQTTNetService mQTTNetService = new MQTTNetService();
            if (args != null && args.Length > 1)
            {
                mQTTNetService = new MQTTNetService(args[1]);
            }
           
            mQTTNetService.ConnectAsync();


            DockerSDK dockerSDK = new DockerSDK(dockerapiUrl);

            ImageManager imageManager = new ImageManager(mQTTNetService, dockerSDK);
            ServiceManager serviceManager = new ServiceManager(mQTTNetService, dockerSDK);


            Console.ReadKey();
        }
    }
}
