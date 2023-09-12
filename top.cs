using PetBotCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PetBotCs
{
    internal class top
    {
        public static async Task<string> GetTopPets(sql database, int topCount, long GroupId)
        {
            try
            {
                List<Dictionary<string, object>> topDicks = await database.ReadTopPets(topCount, GroupId);
                if (topDicks.Count == 0)
                {
                    return "Топ пуст";
                }
                else
                {
                    StringBuilder sb = new();
                    sb.AppendLine("📋Топ самых больших питомцев:\n");
                    for (int i = 0; i < topDicks.Count; i++)
                    {
                        var firstname = topDicks[i]["firstname"];
                        var size = topDicks[i]["size"];
                        sb.AppendLine($"{i + 1}. {firstname}: {size}");
                    }
                    return sb.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return "Произошла ошибка при получении топа";
            }
        }
        public static async Task pettop(ITelegramBotClient botClient, Update update, sql database)
        {
            var message = update.Message;
            var topCount = 10;
            long GroupId = message.Chat.Id;

            try
            {
                string topResult = await top.GetTopPets(database, topCount, GroupId);
                await botClient.SendTextMessageAsync(message.Chat, topResult);
            }
            catch (Exception ex)
            {
                await botClient.SendTextMessageAsync(message.Chat, "Произошла ошибка при выводе топа!");
                Console.WriteLine(ex.Message);
            }
        }
    }
}
