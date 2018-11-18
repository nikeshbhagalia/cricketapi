using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace cricketapi.Models
{
    public class PlayerImageItem
    {
        public string Name { get; set; }
        public string Country { get; set; }
        public string Runs { get; set; }
        public string Wickets { get; set; }
        public string Catches { get; set; }
        public IFormFile Image { get; set; }
    }
}
