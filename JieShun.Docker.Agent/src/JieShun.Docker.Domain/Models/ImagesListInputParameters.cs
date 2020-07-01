using System;
using System.Collections.Generic;
using System.Text;

namespace JieShun.Docker.Domain.Models
{
    public class ImagesListInputParameters
    {

        public string MatchName { get; set; }

        public bool? All { get; set; }

        public IDictionary<string, IDictionary<string, bool>> Filters { get; set; }

    }
}
