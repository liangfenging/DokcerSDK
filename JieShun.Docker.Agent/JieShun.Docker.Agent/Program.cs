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
            //Console.WriteLine("Hello World!");

            //DockerSDK dockerSDK = new DockerSDK("http://127.0.0.1:9890");

            //ReceviceMessage getImages = new ReceviceMessage();
            //getImages.topic = "nm/V1/projectNo/docker/images";
            //ImagesListParameters imagesParam = new ImagesListParameters();
            //getImages.payload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(imagesParam));
            //var images = dockerSDK.GetImages(getImages).Result;
            //string jsondata = JsonConvert.SerializeObject(images);

            //ReceviceMessage createContainer = new ReceviceMessage();
            //createContainer.topic = "nm/V1/projectNo/docker/services/create";

            //CreateContainerParameters createContainerParameters = new CreateContainerParameters();
            //createContainerParameters.ExposedPorts = new Dictionary<string, EmptyStruct>();
            //createContainerParameters.ExposedPorts.Add("5000/tcp", default(EmptyStruct));
            //createContainerParameters.Image = "webtest:0.3";

            //createContainer.payload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(createContainerParameters));

            //var result = dockerSDK.CreateContainerAsync(createContainer).Result;
            //string jsondata = JsonConvert.SerializeObject(result);
            //Console.WriteLine(jsondata);

            MQTTNetService mQTTNetService = new MQTTNetService();
            mQTTNetService.ConnectAsync();
            DockerSDK dockerSDK = new DockerSDK("");

            ImageManager imageManager = new ImageManager(mQTTNetService, dockerSDK);
            ServiceManager serviceManager = new ServiceManager(mQTTNetService, dockerSDK);


            Console.ReadKey();
        }
    }
}
