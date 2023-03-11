using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Net.Mime.MediaTypeNames;

namespace Weather
{

    class Program
    {
        private static TelegramBotClient client;
        private static Message messageNotification;
        static void Main(string[] args)
        {

            client = new TelegramBotClient(Setting.ApiBotKey);
            client.OnMessage += BotOnMessageReceived;
            client.OnMessageEdited += BotOnMessageReceived;
            client.OnCallbackQuery += BotOnCallbackQuery;
            TimerCallback tm = new TimerCallback(DisplayingReminder);
            Timer timer = new Timer(tm, client, 0, 60000);
            client.StartReceiving();
            Console.ReadLine();
            client.StopReceiving();

        }

        private static async void DisplayingReminder(object client)
        {
            TelegramBotClient telegramBotClient = (TelegramBotClient)client;
            List<User> users = await DbHelper.GetUser();
            foreach (User user in users)
            {
                string[] splitTime = user.ResponseWeatherForecastTimes.Split('|');
                for (int i = 0; i < splitTime.Length; i++)
                {
                    string timeNow = DateTime.Now.ToString("hh:mm tt");

                    string[] checkTime = splitTime[i].Split(' ');
                    if (!string.IsNullOrEmpty(checkTime.First()))
                    {
                        if (DateTime.Now.DayOfWeek.ToString() == checkTime.First())
                        {
                            if (new Regex(@"[A-Z].").Match(checkTime.Last()).ToString() == new Regex(@"[A-Z].").Match(timeNow).ToString()
                                && int.Parse(new Regex(@"\d.").Match(timeNow).ToString()) == int.Parse(new Regex(@"\d.").Match(checkTime.Last()).ToString())
                                && int.Parse(new Regex(@"\d.").Match(timeNow).NextMatch().ToString()) >= int.Parse(new Regex(@"\d.").Match(checkTime.Last()).NextMatch().ToString()) &&
                                int.Parse(new Regex(@"\d.").Match(timeNow).NextMatch().ToString()) <= int.Parse(new Regex(@"\d.").Match(checkTime.Last()).NextMatch().ToString()))
                            {
                                await telegramBotClient.SendTextMessageAsync(user.ChatID, GetWeather(user.City));
                                return;
                            }

                        }
                    }
                    if (new Regex(@"[A-Z].").Match(checkTime.Last()).ToString() == new Regex(@"[A-Z].").Match(timeNow).ToString()
                                && int.Parse(new Regex(@"\d.").Match(timeNow).ToString()) == int.Parse(new Regex(@"\d.").Match(checkTime.Last()).ToString())
                                && int.Parse(new Regex(@"\d.").Match(timeNow).NextMatch().ToString()) >= int.Parse(new Regex(@"\d.").Match(checkTime.Last()).NextMatch().ToString()) &&
                                int.Parse(new Regex(@"\d.").Match(timeNow).NextMatch().ToString()) <= int.Parse(new Regex(@"\d.").Match(checkTime.Last()).NextMatch().ToString()))
                    {
                        await telegramBotClient.SendTextMessageAsync(user.ChatID, GetWeather(user.City));
                        return;
                    }
                }

                
            }
        }


        private static async void BotOnCallbackQuery(object sender, CallbackQueryEventArgs callbackQuery)
        {
            string[] days = new string[] { "Monday", "Tuesday", "Wendnesday", "Thursday", "Friday", "Saturday", "Sunday" };
            User user = await DbHelper.GetUser(callbackQuery.CallbackQuery.From.Id);
            switch (callbackQuery.CallbackQuery.Data)
            {
                case "DailyNotification":
                    {                        
                        user.History = string.Empty;
                        DbHelper.Update(user);

                        messageNotification = await client.EditMessageTextAsync(messageNotification.Chat, messageNotification.MessageId, "I will send you the current weather every day at the selected time",
                                      parseMode:ParseMode.Default,null,false,replyMarkup:SelectedTime(user),default);

                       
                        break;
                    }
                case "SelectTheDay":
                    {
                        messageNotification = await client.EditMessageTextAsync(messageNotification.Chat, messageNotification.MessageId, "I will send you the current weather every day at the selected time",
                                     parseMode: ParseMode.Default, null, false, replyMarkup: SelectedDay(user), default);

                       
                        break;
                    }
                case "Back":
                    {
                        messageNotification = await client.EditMessageTextAsync(messageNotification.Chat, messageNotification.MessageId, "I will send you the current weather every day at the selected time",
                                     parseMode: ParseMode.Default, null, false, replyMarkup: Notification(), default);

                        break;
                    }
                default:
                    {
                        if (days.Any(d => d == callbackQuery.CallbackQuery.Data))
                        {
                            messageNotification = await client.EditMessageTextAsync(messageNotification.Chat, messageNotification.MessageId, "I will send you the current weather every day at the selected time",
                                      parseMode: ParseMode.Default, null, false, replyMarkup: SelectedTime(user), default);                  
                          
                            user.History = callbackQuery.CallbackQuery.Data;
                            DbHelper.Update(user);
                            break;
                        }

                        user.ResponseWeatherForecastTimes = ValidateTime(user.ResponseWeatherForecastTimes, user.History + " " + callbackQuery.CallbackQuery.Data);

                        DbHelper.Update(user);

                        messageNotification = await client.EditMessageTextAsync(messageNotification.Chat, messageNotification.MessageId, "I will send you the current weather every day at the selected time",
                              parseMode: ParseMode.Default, null, false, replyMarkup: SelectedTime(user), default);
                 
                        break;
                    }
            }
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            if (message?.Type == MessageType.Text)
            {
                if (!await DbHelper.CheckRegister(message.Chat.Id))
                {
                    switch (message.Text)
                    {
                        case "/start":
                            {
                                await client.SendTextMessageAsync(message.Chat.Id, Setting.START_MESSAGE);
                                break;
                            }
                        default:
                            {
                                try
                                {

                                    await client.SendTextMessageAsync(message.Chat.Id, await AddUser(message.Text, message.Chat.Id),
                                        parseMode: default, null, false, false, 0, false, ShowMenu(), default);
                                }
                                catch
                                {
                                    await client.SendTextMessageAsync(message.Chat.Id, Setting.ERROR_MESSAGE);
                                }
                                break;
                            }
                    }
                    return;
                }
                switch (message.Text)
                {
                    case "Weather Now":
                        {
                            await client.SendTextMessageAsync(message.Chat.Id, await WeatherNow(message.Chat.Id), ParseMode.Default,
                                null, false, false, 0, false, ShowMenu(), default);
                            break;
                        }
                    case "Change City":
                        {
                            DbHelper.Delete(message.Chat.Id);
                            await client.SendTextMessageAsync(message.Chat.Id, "Write the city you want to change to:",
                                        parseMode: default, null, false, false, 0, false, ShowMenu(), default);
                            break;
                        }
                    case "Notification":
                        {
                            messageNotification = await client.SendTextMessageAsync(message.Chat.Id, "Select when to send notifications Forecast for today",
                                        parseMode: default, null, false, false, 0, false, replyMarkup: Notification(), default);
                            break;
                        }
                    default:
                        {
                            await client.SendTextMessageAsync(message.Chat.Id, GetWeather(message.Text),
                                        parseMode: default, null, false, false, 0, false, ShowMenu(), default);
                            break;
                        }
                }
            }
        }


        static ReplyKeyboardMarkup ShowMenu()
        {
            var replyKeyboardMarkup = new ReplyKeyboardMarkup(new KeyboardButton[][] { new KeyboardButton[] {
                                         "Weather Now","Change City","Notification" }, });
            replyKeyboardMarkup.ResizeKeyboard = true;
            return replyKeyboardMarkup;
        }

        static InlineKeyboardMarkup Notification()
        {
            var inline = new InlineKeyboardMarkup(new InlineKeyboardButton[]
            {
                new InlineKeyboardButton{Text ="DailyNotification",CallbackData= "DailyNotification"},
                 new InlineKeyboardButton{Text ="Select the day of the week",CallbackData= "SelectTheDay"}
            });

            return inline;
        }

        static InlineKeyboardMarkup SelectedTime(User user)
        { //✔ ✖
             var inline = new InlineKeyboardMarkup(new InlineKeyboardButton[][]
             {
                 new InlineKeyboardButton[]{
                  new InlineKeyboardButton{Text = SetTime("01:00AM",user), CallbackData= "01:00AM"},
                  new InlineKeyboardButton{Text =SetTime("02:00AM",user),CallbackData= "02:00AM" },
                  new InlineKeyboardButton{Text =SetTime("03:00AM",user),CallbackData= "03:00AM" },
                  new InlineKeyboardButton{Text =SetTime("04:00AM", user),CallbackData= "04:00AM" }, },

                  new InlineKeyboardButton[]{
                  new InlineKeyboardButton{Text =SetTime("05:00AM", user),CallbackData= "05:00AM" },
                  new InlineKeyboardButton{Text =SetTime("06:00AM", user),CallbackData= "06:00AM" },
                  new InlineKeyboardButton{Text =SetTime("07:00AM", user),CallbackData= "07:00AM" },
                  new InlineKeyboardButton{Text =SetTime("08:00AM", user),CallbackData= "08:00AM" }, },

                 new InlineKeyboardButton[]{
                     new InlineKeyboardButton { Text = SetTime("09:00AM",user), CallbackData = "09:00AM" },
                     new InlineKeyboardButton { Text = SetTime("10:00AM", user), CallbackData = "10:00AM" },
                     new InlineKeyboardButton { Text = SetTime("11:00AM", user), CallbackData = "11:00AM" },
                     new InlineKeyboardButton { Text = SetTime("12:00AM",user), CallbackData = "12:00AM" } 
                 },

                 new InlineKeyboardButton[]{
                new InlineKeyboardButton{Text =SetTime("01:00PM",user),CallbackData= "01:00PM"},
                  new InlineKeyboardButton{Text =SetTime("02:00PM", user),CallbackData= "02:00PM" },
                  new InlineKeyboardButton{Text =SetTime("03:00PM", user),CallbackData= "03:00PM" },
                  new InlineKeyboardButton{Text =SetTime("04:00PM", user),CallbackData= "04:00PM" }, },

                 new InlineKeyboardButton[]{
                     new InlineKeyboardButton { Text = SetTime("05:00PM", user), CallbackData = "05:00PM" },
                     new InlineKeyboardButton { Text = SetTime("06:00PM", user), CallbackData = "06:00PM" },
                     new InlineKeyboardButton { Text = SetTime("07:00PM", user), CallbackData = "07:00PM" },
                     new InlineKeyboardButton { Text = SetTime("08:00PM", user), CallbackData = "08:00PM" } },

                 new InlineKeyboardButton[]{
                     new InlineKeyboardButton { Text = SetTime("09:00PM", user), CallbackData = "09:00PM" },
                     new InlineKeyboardButton { Text = SetTime("10:00PM", user), CallbackData = "10:00PM" },
                     new InlineKeyboardButton { Text = SetTime("11:00PM", user), CallbackData = "11:00PM" },
                     new InlineKeyboardButton { Text = SetTime("12:00PM", user), CallbackData = "12:00PM" } },

                  new InlineKeyboardButton[]{
                     new InlineKeyboardButton { Text = "Back", CallbackData = "Back" }, },
             });
            


            return inline;
        }


        static string SetTime(string text, User user) 
        {
            if (!string.IsNullOrEmpty(user.ResponseWeatherForecastTimes))
            {
                string[] userResponseTimes = user.ResponseWeatherForecastTimes.Split('|');
                for (int i = 0; i < userResponseTimes.Length; i++)
                {
                    string[] userResponseTime = userResponseTimes[i].Split(' ');
                    if (userResponseTime.Last().ToString() == text)
                    {
                        return text + "✔";
                    }
                }
            }
            return text + "✖";
        }

        static string SetDay(string text, User user)
        {
            if (!string.IsNullOrEmpty(user.ResponseWeatherForecastTimes))
            {
                string[] userResponseTimes = user.ResponseWeatherForecastTimes.Split('|');
                for (int i = 0; i < userResponseTimes.Length; i++)
                {
                    string[] userResponseTime = userResponseTimes[i].Split(' ');
                    if (userResponseTime.First().ToString() == text)
                    {
                        return text + "✔";
                    }
                }
            }
            return text + "✖";
        }

        static InlineKeyboardMarkup SelectedDay(User user)
        {
            var inline = new InlineKeyboardMarkup(new InlineKeyboardButton[][]
            {
                new InlineKeyboardButton[]{
                new InlineKeyboardButton{Text = SetDay("Monday",user) , CallbackData= "Monday"},
                 new InlineKeyboardButton{Text = SetDay("Tuesday",user) ,CallbackData= "Tuesday" }, 
                },

                 new InlineKeyboardButton[]{
                 new InlineKeyboardButton{Text =SetDay("Wendnesday",user),CallbackData= "Wendnesday" },
                 new InlineKeyboardButton{Text =SetDay("Thursday",user),CallbackData= "Thursday" },
                 new InlineKeyboardButton{Text =SetDay("Friday",user),CallbackData= "Friday" },
                 },

                new InlineKeyboardButton[]{
                    new InlineKeyboardButton { Text = SetDay("Saturday",user), CallbackData = "Saturday" },
                    new InlineKeyboardButton { Text = SetDay("Sunday",user), CallbackData = "Sunday" },
                },

                new InlineKeyboardButton[]{
                     new InlineKeyboardButton { Text = "Back", CallbackData = "Back" }, },
            });

            return inline;
        }

        static async Task<string> AddUser(string city, long chatID)
        {
            string url = string.Format(Setting.ApiWeatherURL, city);

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            string response;

            using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
            {
                response = streamReader.ReadToEnd();
            }

            WeatherResponse weatherResponse = JsonConvert.DeserializeObject<WeatherResponse>(response);

            DbHelper.Add(new User(chatID, city));
            return $"Congrats, you can start working with the bot";

        }

        static async Task<string> WeatherNow(long chatID)
        {
            return GetWeather(await DbHelper.GetCity(chatID));
        }

        static string GetWeather(string city)
        {
            try
            {
                string url = string.Format(Setting.ApiWeatherURL, city);

                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                string response;

                using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                {
                    response = streamReader.ReadToEnd();
                }

                WeatherResponse weatherResponse = JsonConvert.DeserializeObject<WeatherResponse>(response);
                return $"Temperature in {weatherResponse.Name}: {weatherResponse.Main.Temp.ToString()}°С";
            }
            catch (Exception exp)
            {
                return $" Write city again:";
            }
        }

        static string ValidateTime(string userResponseWeatherForecastTimes, string setTime)
        {
            string[] splitTime = userResponseWeatherForecastTimes.Split('|');

            if (splitTime.Any(st => st == setTime))
            {
                userResponseWeatherForecastTimes = "";
                for (int i = 0; i < splitTime.Length - 1; i++)
                {
                    if (splitTime[i] != setTime)
                        userResponseWeatherForecastTimes += splitTime[i] + "|";
                }
                return userResponseWeatherForecastTimes;
            }
            return userResponseWeatherForecastTimes + setTime + "|";
        }
    }
}
