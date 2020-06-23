using JieShun.Docker.Domain.Models;
using System;

namespace JieShun.Docker.Agent
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Console.WriteLine(nameof(DockerTopics.GetImages));

            Console.ReadKey();
        }
    }
}
