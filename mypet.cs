using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using MySql.Data.MySqlClient;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using PetBotCs;

namespace PetBotCs
{
    class Mypet
    {
        public static sql MySql()
        {
            return new sql("Данные для подключения к SQL");
        }

        public static async Task Dblogic(ITelegramBotClient botClient, Update update)
        {
            Random random = new();
            int randomNum = random.Next(1, 15);

            string finalId;
            var userName = update.Message.From.FirstName;
            var fullName = update.Message.From.FirstName + " " + update.Message.From.LastName;
            var userId = update.Message.From.Id;
            var message = update.Message;
            var groupId = message.Chat.Id;
            var username2 = message.From.Username;
            var userIdGetter = $"SELECT `name` FROM `group{groupId}` WHERE name = '{userId}';";
            var sizeGetter = $"SELECT `size` FROM `group{groupId}` WHERE name = '{userId}';";
            var bonusGetter = $"SELECT `bonus` FROM `group{groupId}` WHERE name = '{userId}';";
            var usageGetter = $"SELECT `IsUsedToday` FROM `group{groupId}` WHERE name = '{userId}';";

            var usageBlocker = $"UPDATE `group{groupId}` SET `IsUsedToday` = 1 WHERE `name` = {userId};";
            var addcm = $"UPDATE `group{groupId}` SET `size` = `size` + {randomNum} + `bonus` WHERE `name` = {userId};";
            var addUserName = $"UPDATE `group{groupId}` SET `username` = '{username2}', `firstname` = '{fullName}' WHERE `name` = {userId};";
            var firstUse = $"INSERT INTO `group{groupId}` (`username`, `firstname`, `name`, `size`, `IsUsedToday`, `IsCuttedToday`, `bonus`) VALUES ('{username2}', '{fullName}', '{userId}', '{randomNum}', '1', '0', '0');";
            var resetBonus = $"UPDATE `group{groupId}` SET `bonus` = 0 WHERE `name` = {userId};";

            try
            {
                List<string> names = MySql().Read(userIdGetter, "name");
                finalId = names.FirstOrDefault();

                List<string> sizes = MySql().Read(sizeGetter, "size");
                string summary = sizes.FirstOrDefault();

                List<string> usage = MySql().Read(usageGetter, "IsUsedToday");
                string isUsedToday = usage.FirstOrDefault();

                List<string> bonuses = MySql().Read(bonusGetter, "bonus");
                string bonus = bonuses.FirstOrDefault();

                if (isUsedToday == "True")
                {
                    await botClient.SendTextMessageAsync(message.Chat, $"⛔️{userName}, сегодня ты уже играл! Следующая попытка завтра!");
                    MySql().Read(addUserName, "");
                }
                else if (isUsedToday == "False")
                {
                    MySql().Read(addcm, "");
                    MySql().Read(usageBlocker, "");
                    MySql().Read(addUserName, "");
                    MySql().Read(resetBonus, "");
                    int currentSize = int.Parse(summary);
                    int sizeValue = currentSize + randomNum;
                    await botClient.SendTextMessageAsync(message.Chat, $"🔼{userName}, размер твоего питомца увеличен на {randomNum}! Сейчас он равен {sizeValue}. Следующая попытка завтра!");
                }
                else
                {
                    MySql().Read(firstUse, "");
                    await botClient.SendTextMessageAsync(message.Chat, $"🔼{userName}, добро пожаловать в игру!👋 Теперь у вас есть питомец. Его размер составляет {randomNum}! Завтра вы сможете увеличить его ещё!");
                }
            }
            catch (Exception ex)
            {
                await botClient.SendTextMessageAsync(message.Chat, $"🚫Произошла ошибка при обращении к базе данных: {ex.Message}. \nЕсли ошибка будет повторяться - свяжитесь с @NtRazum!");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}