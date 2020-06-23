using System;
using System.Collections.Generic;
using System.Text;

namespace JieShun.Docker.Domain.Models
{
    public class ReceviceMessage
    {
        public string clientId { get; set; }

        public byte[] payload { get; set; }


        public string topic { get; set; }
    }
}
