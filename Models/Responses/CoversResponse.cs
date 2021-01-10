using Covers.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Covers.Models.Responses
{
    public class CoversResponse
    {
        public int TotalCount { get; set; }
        public List<CoverDTO> Covers { get; set; }
    }
}
