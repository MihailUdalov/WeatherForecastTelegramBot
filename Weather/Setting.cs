using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weather
{
    internal class Setting
    {
        public readonly static string ApiBotKey = ConfigurationManager.AppSettings.Get("ApiBotKey");
        public readonly static string ApiWeatherURL = "http://api.openweathermap.org/data/2.5/weather?q={0}&units=metric&appid=d8586230c8514cf851fb224acce09e95";

        public const string ERROR_MESSAGE = "There is no such command. Enter /help to see all available commands.";

        public const string START_MESSAGE= "You are greeted by a weather forecast bot. Write the name of the city so that I can show you the weather:";

        public const string HELP_MESSAGE = "We help you";

    }
}
