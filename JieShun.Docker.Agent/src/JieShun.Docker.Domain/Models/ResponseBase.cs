using System;
using System.Collections.Generic;
using System.Text;

namespace JieShun.Docker.Domain.Models
{
    public class ResponseBase
    {
        public string transId { get; set; }

        public int code { get; set; }

        public string message { get; set; }

    }

    public class ResponseBase<T> : ResponseBase
    {
        public T data { get; set; }

    }


    public class RequestBase
    { 
       public string transId { get; set; }
    }
}
