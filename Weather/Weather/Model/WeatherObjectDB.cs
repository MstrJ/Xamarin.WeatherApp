using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace Weather.Model
{
    public class WeatherObjectDB
    {
        [PrimaryKey,AutoIncrement]
        public int id { get; set; }
        public string cityURL { get; set; }
        public bool isLocation { get; set; } = false;

    }

 
}
