using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestApp.Models
{
    //class for HttpGet history
    public class WeatherInfoCurrent
    {
        public int ID { get; set; }
        public string CityName { get; set; }
        public float CurTemperature { get; set; }
        public DateTime TimeStamp { get; set; }
        public float MinTemparature { get; set; }
        public float MaxTemperature { get; set; }
        public float AvgTemperature { get; set; }


    }
}
