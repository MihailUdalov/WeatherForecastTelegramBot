using System;
using System.Collections.Generic;

namespace Weather
{
    public class User
    {
        public long ID { get; set; }
        public long ChatID { get; set; }
        public string City { get; set; }
        public string ResponseWeatherForecastTimes { get; set; }

        public string History { get; set; }
        public User()
        {
             
        }
        public User(long chatID, string city)
        {
            ChatID = chatID;
            City = city;
            ResponseWeatherForecastTimes = "";
            History = "";
        }
    }
}
