using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types.InlineQueryResults;

namespace PetBotCs
{
    internal class Start
    {
        static sql database = new(appConfig.Config.MySQLConnection);
        public static async Task Keyboard(ITelegramBotClient botClient, Update update)
        {
            var message = update.Message;
            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new[]
                {
                    new KeyboardButton("Вызовы от игроков")
                }
            });

            keyboard.ResizeKeyboard = true;
            await botClient.SendTextMessageAsync(message.Chat, "Привет!", replyMarkup: keyboard);
            long userId = message.From.Id;

            var Ready = new ReplyKeyboardMarkup(new[]
            {
                new[]
                {
                    new KeyboardButton("Я готов!")
                }
            });
            var NotReady = new ReplyKeyboardMarkup(new[]
{
                new[]
                {
                    new KeyboardButton("Не готов!")
                }
            });

            var GetUser = $"SELECT `p1id` FROM `duels` WHERE `p1id` = '{userId}';";
            List<string> Id = database.Read(GetUser, "p1id");
            string idString = Id.FirstOrDefault();
            if (idString != null)
            {
                long p1id = long.Parse(idString);
                if (p1id == userId)
                {
                    var columnsToRetrieve = new string[] { "p1name", "p2name" };
                    var ValuesGet = $"SELECT p1name, p2name FROM duels WHERE p1id = {userId};";
                    List<Dictionary<string, object>> results = database.ExtRead(ValuesGet, columnsToRetrieve);
                    foreach (var result in results)
                    {
                        var p1name = result["p1name"].ToString();
                        var p2name = result["p2name"].ToString();
                        await botClient.SendTextMessageAsync
                            ($"{userId}", $"{message.From.FirstName}, а вот и ты!\n" +
                                          $"Добро пожаловать в игру!\n" +
                                          $"Подготовься к бою и ознакомься с правилами:\n" +
                                          $"1. Битва будет проходить на поле 3x3, каждый из оппонентов начинает игру в противоположных углах поля: {p1name} начнёт игру с левого нижнего угла, {p2name} начнёт игру с правого верхнего! Обоим игрокам будет выдано по 3 жизни\n" +
                                          $"2. Каждый игрок имеет право сделать по одному взмаху своим хуём за раз в любом из четырёх направлений: вверх, вниз, вправо или влево. После каждого взмаха действует кулдаун в 1 секунду!\n" +
                                          $"3. При столкновении двух игроков на одной позиции считается, что ваши питомцы отразили друг друга! При таком условии засчитывается ничья, и каждый откатывается до первоначальных позиций!\n" +
                                          $"4. Если один из игроков попадает на позицию второго игрока - у того, кто стоял на этой клетке раньше - отнимается жизнь!\n" +
                                          $"5. Игра автоматически заканчивается по истечении 15 минут, либо когда один из игроков потерял все свои жизни! В этом случае победа засчитывается тому, кто смог сохранить жизни!\n" +
                                          $"6. У проигравшего игрока автоматически снимается рандомное количество размеров его питомца (7-10), и отнятое значение присоединяется победителю!\n" +
                                          $"\n" +
                                          $"Всё понятно? Готов к игре? Тогда жми кнопку \"Готов\"! Желаю удачи!", replyMarkup: keyboard);
                    }
                }
            }
        }
    }
}