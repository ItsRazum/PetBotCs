using PetBotCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;

namespace PetBotCs
{
    internal static class InternalUsages
    {
        static sql database = new ("Данные для подключения к SQL");
        public static async Task ResetIsUsedToday()
        {
            List<string> tableNames = database.GetTableNames();
            foreach (string tableName in tableNames)
            {
                if (tableName.StartsWith("group"))
                {
                    string groupIdString = tableName.Substring("group".Length);
                    if (long.TryParse(groupIdString, out long groupId))
                    {
                        try
                        {
                            var reset = $"UPDATE `group{groupId}` SET `IsUsedToday` = 0, `IsCuttedToday` = 0;";
                            database.Read(reset, "");
                        }
                        catch (Exception ex) { Console.WriteLine(ex.Message); }
                    }
                }
            }


        }

        public static long GetIdFromUserName(string userName, long groupId)
        {
            try
            {
                var GetUserName = $"SELECT `name` FROM `group{groupId}` WHERE `username` = '{userName}';";
                List<string> usernameStr = database.Read(GetUserName, "name");
                long userId = Convert.ToInt64(usernameStr.FirstOrDefault());
                return userId;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            return 0;
        }

        public static string GetFirstNameFromUserName(string userName, long groupId)
        {
            try
            {
                var GetUserName = $"SELECT `firstname` FROM `group{groupId}` WHERE `username` = '{userName}';";
                List<string> usernameStr = database.Read(GetUserName, "firstname");
                string firstname = usernameStr.FirstOrDefault();
                return firstname;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            return null;
        }

        public static async Task ResetUserIsUsedToday(ITelegramBotClient botClient, Update update)
        {
            var message = update.Message;
            var groupId = message.Chat.Id;
            var reset = $"UPDATE `group{groupId}` SET `IsUsedToday` = 0 WHERE `name` = 901152811;";
            try
            {
                database.Read(reset, "");
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }


        public static async Task resetCut(ITelegramBotClient botClient, Update update)
        {
            var message = update.Message;
            var groupId = message.Chat.Id;
            var resetCut = $"UPDATE `group{groupId}` SET `IsCuttedToday` = 0, `IsCuttedToday` = 0;";
            try
            {
                database.Read(resetCut, "");
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }


        private static System.Timers.Timer timer;
        private static readonly TimeSpan resetTime = new(0, 0, 0);
        public static void SetTimer()
        {
            DateTime currentTime = DateTime.Now;
            DateTime resetDateTime = new(currentTime.Year, currentTime.Month, currentTime.Day, resetTime.Hours, resetTime.Minutes, resetTime.Seconds);

            if (currentTime >= resetDateTime)
            {
                resetDateTime = resetDateTime.AddDays(1);
            }

            TimeSpan timeToWait = resetDateTime - currentTime;

            timer = new System.Timers.Timer(timeToWait.TotalMilliseconds);
            timer.Elapsed += async (sender, e) =>
            {
                Console.WriteLine("Счётчик сброшен!");
                await InternalUsages.ResetIsUsedToday();

                SetTimer();
            };

            timer.AutoReset = false;
            timer.Start();
        }
    }
}