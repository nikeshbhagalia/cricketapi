using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cricketapi.Models
{
    public class Player
    {
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public string Country { get; set; }
        
        public string Runs { get; set; }
        
        public string Wickets { get; set; }
        
        public string Catches { get; set; }
        
        public string Url { get; set; }
    }
}
