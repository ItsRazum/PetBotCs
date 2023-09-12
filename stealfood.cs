using PetBotCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PetBotCs
{
    class stealfood
    {
        public static async void HandleStealfoodCommand(ITelegramBotClient botClient, Update update, string userTag, long GroupId)
        {
            try
            {
                var message = update.Message;
                long replUserId = InternalUsages.GetIdFromUserName(userTag, GroupId);
                string replFirstName = InternalUsages.GetFirstNameFromUserName(userTag, GroupId);

                if (replUserId == 0)
                {
                    await botClient.SendTextMessageAsync(message.Chat, "😔Пользователь не найден или ещё не начал играть в /mypet!");
                }
                else
                {
                    await Cut(botClient, update, replFirstName, replUserId, userTag);
                }
                Console.WriteLine($"{replFirstName}, {replUserId}, {userTag}");
            }
            catch { }
        }

        public static sql MySql()
        {
            return new sql("Данные для подключения к SQL");
        }
        public static async Task Cut(ITelegramBotClient botClient, Update update, string repliedUserName, long repliedUserId, string userTag)
        {
            string summary;
            string RepliedSummary;
            string finalId;
            var userId = update.Message.From.Id;
            var userName = update.Message.From.FirstName;
            var message = update.Message;
            var groupId = message.Chat.Id;
            Random random = new();
            int randomNum = random.Next(1, 4);
            var dickCutter = $"UPDATE `group{groupId}` SET `size` = `size` - {randomNum} WHERE `name` = {userId};";
            var repliedDickCutter = $"UPDATE `group{groupId}` SET `size` = `size` - {randomNum} WHERE `name` = {repliedUserId};";
            var userIdGetter = $"SELECT `name` FROM `group{groupId}` WHERE name = '{userId}';";
            var usageGetter = $"SELECT `IsCuttedToday` FROM `group{groupId}` WHERE name = '{userId}';";
            var sizeGetter = $"SELECT `size` FROM `group{groupId}` WHERE name = '{userId}';";
            var repliedSizeGetter = $"SELECT `size` FROM `group{groupId}` WHERE name = '{repliedUserId}';";
            var usageBlocker = $"UPDATE `group{groupId}` SET `IsCuttedToday` = 1 WHERE `name` = {userId};";

            try
            {
                List<string> names = MySql().Read(userIdGetter, "name");
                finalId = names.FirstOrDefault();

                List<string> usage = MySql().Read(usageGetter, "IsCuttedToday");
                string isCuttedToday = usage.FirstOrDefault();

                List<string> repSizes = MySql().Read(repliedSizeGetter, "size");
                RepliedSummary = repSizes.FirstOrDefault();

                List<string> sizes = MySql().Read(sizeGetter, "size");
                summary = sizes.FirstOrDefault();

                if (repliedUserId == userId)
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Зачем ты режешь хуй сам себе?");
                }
                else if (repliedUserId == null)
                {
                    await botClient.SendTextMessageAsync(message.Chat, $"Для использования этой команды нужно реплаем выбрать вашу цель!");
                }
                else if (summary == null)
                {
                    await botClient.SendTextMessageAsync(message.Chat, $"Данный пользователь не начал играть в /mypet!");
                }
                else if (isCuttedToday == "True" && repliedUserId != userId)
                {
                    await botClient.SendTextMessageAsync(message.Chat, $"{userName}, Сегодня ты уже воровал еду у другого игрока! Следующая возможность завтра!");
                }
                else if (isCuttedToday == "False" && repliedUserId != userId)
                {
                    int randomSelfCut = random.Next(1, 10);
                    double selfCutChance = 3;
                    Console.WriteLine(randomSelfCut);

                    if (summary == null)
                    {
                        await botClient.SendTextMessageAsync(message.Chat, $"⛔️Тебе нельзя использовать эту команду, т.к. ты не начал играть в /mypet! Тебе нужно срочно исправить положение, прописав команду /mypet!");
                    }
                    else if (randomSelfCut < selfCutChance)
                    {
                        int currentSize = int.Parse(summary);
                        int sizeValue = currentSize - randomNum;
                        await botClient.SendTextMessageAsync(message.Chat, $"✂️{userName}, о нет! Ты пытался украсть еду игрока {repliedUserName}, однако что-то перепутал и случайно испортил еду для своего питомца! Теперь он похудел на {randomNum}!😬\nСейчас размер твоего питомца составляет {sizeValue}.\nСледующая возможность воровать еду завтра!");
                        MySql().Read(dickCutter, "");
                        MySql().Read(usageBlocker, "");
                    }
                    else
                    {
                        int repliedCurrentSize = int.Parse(RepliedSummary);
                        int repliedSizeValue = repliedCurrentSize - randomNum;
                        await botClient.SendTextMessageAsync(message.Chat, $"✂️{userName} успешно своровал еду питомца пользователя {repliedUserName}! Теперь его питомец похудел на {randomNum}!\nСейчас его размер составляет {repliedSizeValue}\nСледующая возможность воровать еду завтра!");
                        MySql().Read(repliedDickCutter, "");
                        MySql().Read(usageBlocker, "");
                    }
                }
                else
                {
                    await botClient.SendTextMessageAsync(message.Chat, $"😕Данный пользователь ещё не начал играть в /mypet!");
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
