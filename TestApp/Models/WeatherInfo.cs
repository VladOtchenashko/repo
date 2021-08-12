using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestApp.Models
{
    public class WeatherInfo
    {
        public int ID { get; set; }
        public string CityName { get; set; }
        public float CurrentTemperature { get; set; }
        public DateTime TimeStamp { get; set; }
        public bool IsArchived { get; set; } //imitation of delete method
    }
}
