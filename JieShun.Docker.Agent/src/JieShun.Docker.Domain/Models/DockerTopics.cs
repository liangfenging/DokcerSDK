using System;
using System.Collections.Generic;
using System.Text;

namespace JieShun.Docker.Domain.Models
{
    public class DockerTopics
    {

        public const string BaseTopic = "nm/V1/projectNo/docker/#";

        public const string ImagesTopic = "nm/V1/projectNo/docker/images/#";

        public const string ServicesTopic = "nm/V1/projectNo/docker/services/#";

        public const string ImagesACK = "nm/V1/projectNo/docker/ack/images";

        public const string ImagesCreateACK = "nm/V1/projectNo/docker/ack/images/create";

        public const string ServicesACK = "nm/V1/projectNo/docker/ack/services";

        public const string ServicesCreateACK = "nm/V1/projectNo/docker/ack/services/create";

        public const string ServicesUpgradeACK = "nm/V1/projectNo/docker/ack/services/update";

        public const string ServicesStartACK = "nm/V1/projectNo/docker/ack/services/start";

        public const string ServicesStopACK = "nm/V1/projectNo/docker/ack/services/stop";
    }
}
