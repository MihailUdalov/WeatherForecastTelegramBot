using System.Collections.Generic;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Weather
{
    public class DbHelper
    {
        public static async void Add(User user)
        {
            using (UserContext db = new UserContext())
            {
                db.Users.Add(user);
                await db.SaveChangesAsync();
            }
        }

        public static async Task<bool> CheckRegister(long chatID)
        {
            using (UserContext db = new UserContext())
            {
                return await db.Users.AnyAsync(us => us.ChatID == chatID);
            }
        }

        public static void Update(User user)
        {
            using (UserContext database = new UserContext())
            {
                var updateuser = database.Users.FirstOrDefault(us => us.ChatID == user.ChatID);
                updateuser.History = user.History;
                updateuser.City = user.City;
                updateuser.ResponseWeatherForecastTimes = user.ResponseWeatherForecastTimes;
                database.SaveChanges();

            }
        }

        public static void Delete(long chatID)
        {
            using (UserContext database = new UserContext())
            {
                var deleteUser = database.Users.FirstOrDefault(us => us.ChatID == chatID);
                database.Users.Remove(deleteUser);
                database.SaveChanges();

            }
        }
        public static async Task<User> GetUser(long chatID)
        {
            using (UserContext db = new UserContext())
            {
                return (await db.Users.FirstAsync(us => us.ChatID == chatID));
            }
        }

        public static async Task<string> GetCity(long chatID)
        {
            using (UserContext db = new UserContext())
            {
                return (await db.Users.FirstAsync(us => us.ChatID == chatID)).City;
            }
        }

        public static async Task<List<User>> GetUser()
        {
            using (UserContext database = new UserContext())
            {
                return await database.Users.ToListAsync();
            }

        }
    }
}