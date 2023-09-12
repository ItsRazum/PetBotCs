using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Timers;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Requests;
using System.Runtime.CompilerServices;
using PetBotCs.Game;
using System.Collections;
using System.Collections.Generic;
using PetBotCs;
using Telegram.Bot.Types.Enums;

namespace PetBotCs
{
    public class LobbyTimer
    {
        public System.Timers.Timer WaitTimer { get; set; }
        public string p1id { get; set; }
        public string p2id { get; set; }
        public string p1name { get; set; }
        public string p2name { get; set; }
        public string rootgroup { get; set; }
    }

    class Lobby
    {
        public static async void HandlePetFightCommand(ITelegramBotClient botClient, Update update, string userTag, long GroupId, bool IsFriendly)
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

                else if (IsFriendly == false)
                {
                    await Alert(botClient, update, replFirstName, replUserId, userTag);
                }
                else if (IsFriendly == true)
                {
                    await FriendlyAlert(botClient, update, replFirstName, replUserId, userTag);
                }
                Console.WriteLine($"{replFirstName}, {replUserId}, {userTag}");
            }
            catch { }
        }


        public static sql MySql()
        {
            return new sql("Данные для подключения к SQL");
        }

        public static volatile List<Game.Game> Games = new();

        private static LobbyTimer lobbyTimer = new();
        private Game.Game game;

        private Game.Game GetGame => game;

        private void SetGame(Game.Game game)
        {
            this.game = game;
        }

        public static async Task Alert(ITelegramBotClient botClient, Update update, string repliedUserName, long repliedUserId, string repliedUserTag) //Коренной метод, именно с него начинается инициализация игры
        {
            var message = update.Message;
            var userName = update.Message.From.FirstName;
            long userId = update.Message.From.Id;
            //var repliedUserName = update.Message.ReplyToMessage?.From.FirstName;
            //long? repliedUserId = update.Message.ReplyToMessage?.From.Id;
            //var repliedUserTag = update.Message.ReplyToMessage?.From.Username;
            var groupId = update.Message.Chat.Id;

            var checkp1Out = $"SELECT `p1id` FROM `duels` WHERE p1id = '{userId}';";
            var checkp1In = $"SELECT `p2id` FROM `duels` WHERE p2id = '{userId}';";
            List<string> p1CheckChallengeN1 = MySql().Read(checkp1Out, "p1id");
            List<string> p1CheckChallengeN2 = MySql().Read(checkp1In, "p2id");
            string p1gotChallengeN1 = p1CheckChallengeN1.FirstOrDefault(); //Проверка на наличие исходящего запроса у того, кто использует команду
            string p1gotChallengeN2 = p1CheckChallengeN2.FirstOrDefault(); //Проверка на наличие входящего запроса у того, кто использует команду

            var checkp2N1 = $"SELECT `p1id` FROM `duels` WHERE p1id = '{repliedUserId}';";
            var checkp2N2 = $"SELECT `p2id` FROM `duels` WHERE p2id = '{repliedUserId}';";
            List<string> p2CheckChallengeN1 = MySql().Read(checkp2N1, "p1id");
            List<string> p2CheckChallengeN2 = MySql().Read(checkp2N2, "p2id");
            string p2gotChallengeOut = p2CheckChallengeN1.FirstOrDefault(); //Проверка на наличие исходящего запроса у того, кому адресован запрос
            string p2gotChallengeIn = p2CheckChallengeN2.FirstOrDefault(); //Проверка на наличие входящего запроса у того, кому адресован запрос

            if (userId == repliedUserId)
            { await botClient.SendTextMessageAsync(message.Chat, "🤨Эй, у тебя не два хуя, чтобы сражаться с самим собой!"); }

            else if (p1gotChallengeN1 != null && p2gotChallengeOut != null)
            { await botClient.SendTextMessageAsync(message.Chat, "⏱Прояви терпение! Ты уже отправил запрос на дуэль этому игроку!"); }

            else if (p2gotChallengeOut != null && p1gotChallengeN2 != null)
            { await ChallengeAccept(botClient, update, lobbyTimer); }

            else if (p1gotChallengeN2 != null)
            { await botClient.SendTextMessageAsync(message.Chat, "⛔️Вам уже пришёл вызов от другого игрока! чтобы его принять или отклонить - перейдите в лс бота и проверьте ваши вызовы!"); }

            else if (p1gotChallengeN1 != null)
            { await botClient.SendTextMessageAsync(message.Chat, "⛔️Вы уже отправили вызов другому игроку!"); }

            else if (p2gotChallengeIn != null)
            { await botClient.SendTextMessageAsync(message.Chat, "⛔️Данному игроку уже пришёл вызов от другого игрока!"); }

            else if (p2gotChallengeOut != null)
            { await botClient.SendTextMessageAsync(message.Chat, "⛔️Данный игрок уже отправил запрос кому-то другому!"); }

            else
            {
                var newSession = $"INSERT INTO `duels` (`p1id`, `p2id`, `p1pos`, `p2pos`, `p1hp`, `p2hp`, `p1name`, `p2name`, `rootgroup`, `isAllowed`, `p1IsReady`, `p2IsReady`, `p1IsMoved`, `p2IsMoved`, `IsFriendly`) VALUES ('{userId}', '{repliedUserId}', '13', '3', '3', '3', '{userName}', '{repliedUserName}', {groupId}, '0', '0', '0', '0', '0', '0');";

                var inline = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithUrl("Принять вызов", "https://t.me/Nakolennik_bot"),
                    }
                });

                await botClient.SendTextMessageAsync(message.Chat,
                    $"⚔️Беседа, минуточку внимания!" +
                    $"\n\n{userName} бросил вызов на дуэль игроку {repliedUserName}!" +
                    $"\nВнимание, @{repliedUserTag}! Чтобы принять вызов, перейдите в ЛС бота!", replyMarkup: inline);
                MySql().Read(newSession, "");
                lobbyTimer.p1id = $"{userId}";
                lobbyTimer.p2id = $"{repliedUserId}";
                lobbyTimer.p1name = $"{userName}";
                lobbyTimer.p2name = $"{repliedUserName}";
                lobbyTimer.rootgroup = $"{groupId}";
                lobbyTimer.WaitTimer = new System.Timers.Timer(900000);
                lobbyTimer.WaitTimer.Elapsed += (sender, e) => TimerElapsed(sender, e, lobbyTimer);
                lobbyTimer.WaitTimer.Start();
            }
        }

        public static async Task FriendlyAlert(ITelegramBotClient botClient, Update update, string repliedUserName, long repliedUserId, string repliedUserTag) //Тоже коренной метод, но предназначается он для дружеского поединка
        {
            var message = update.Message;
            var userName = update.Message.From.FirstName;
            long userId = update.Message.From.Id;
            //var repliedUserName = update.Message.ReplyToMessage?.From.FirstName;
            //long? repliedUserId = update.Message.ReplyToMessage?.From.Id;
            //var repliedUserTag = update.Message.ReplyToMessage?.From.Username;
            var groupId = update.Message.Chat.Id;

            var checkp1Out = $"SELECT `p1id` FROM `duels` WHERE p1id = '{userId}';";
            var checkp1In = $"SELECT `p2id` FROM `duels` WHERE p2id = '{userId}';";
            List<string> p1CheckChallengeN1 = MySql().Read(checkp1Out, "p1id");
            List<string> p1CheckChallengeN2 = MySql().Read(checkp1In, "p2id");
            string p1gotChallengeN1 = p1CheckChallengeN1.FirstOrDefault();  //Проверка на наличие исходящего запроса у того, кто использует команду
            string p1gotChallengeN2 = p1CheckChallengeN2.FirstOrDefault(); //Проверка на наличие входящего запроса у того, кто использует команду

            var checkp2N1 = $"SELECT `p1id` FROM `duels` WHERE p1id = '{repliedUserId}';";
            var checkp2N2 = $"SELECT `p2id` FROM `duels` WHERE p2id = '{repliedUserId}';";
            List<string> p2CheckChallengeN1 = MySql().Read(checkp2N1, "p1id");
            List<string> p2CheckChallengeN2 = MySql().Read(checkp2N2, "p2id");
            string p2gotChallengeOut = p2CheckChallengeN1.FirstOrDefault(); //Проверка на наличие исходящего запроса у того, кому адресован запрос
            string p2gotChallengeIn = p2CheckChallengeN2.FirstOrDefault(); //Проверка на наличие входящего запроса у того, кому адресован запрос

            if (userId == repliedUserId)
            { await botClient.SendTextMessageAsync(message.Chat, "🤨Эй, у тебя не два хуя, чтобы сражаться с самим собой!"); }

            if (p1gotChallengeN1 != null && p2gotChallengeOut != null)
            { await botClient.SendTextMessageAsync(message.Chat, "Прояви терпение! Ты уже отправил запрос на дуэль этому игроку!"); }

            else if (p2gotChallengeOut != null && p1gotChallengeN2 != null)
            { await ChallengeAccept(botClient, update, lobbyTimer); }

            else if (p1gotChallengeN2 != null)
            { await botClient.SendTextMessageAsync(message.Chat, "Вам уже пришёл вызов от другого игрока! чтобы его принять или отклонить - перейдите в лс бота и проверьте ваши вызовы!"); }

            else if (p1gotChallengeN1 != null)
            { await botClient.SendTextMessageAsync(message.Chat, "Вы уже отправили вызов другому игроку!"); }

            else if (p2gotChallengeIn != null)
            { await botClient.SendTextMessageAsync(message.Chat, "Данному игроку уже пришёл вызов от другого игрока!"); }

            else if (p2gotChallengeOut != null)
            { await botClient.SendTextMessageAsync(message.Chat, "Данный игрок уже отправил запрос кому-то другому!"); }

            else
            {
                var newSession = $"INSERT INTO `duels` (`p1id`, `p2id`, `p1pos`, `p2pos`, `p1hp`, `p2hp`, `p1name`, `p2name`, `rootgroup`, `isAllowed`, `p1IsReady`, `p2IsReady`, `p1IsMoved`, `p2IsMoved`, `IsFriendly`) VALUES ('{userId}', '{repliedUserId}', '13', '3', '3', '3', '{userName}', '{repliedUserName}', {groupId}, '0', '0', '0', '0', '0', '1');";

                var inline = new InlineKeyboardMarkup(new[]
                {
                new[]
                    {
                        InlineKeyboardButton.WithUrl("Принять вызов", "https://t.me/Nakolennik_bot"),
                    }
                });

                await botClient.SendTextMessageAsync(message.Chat,
                    $"⚔️Беседа, минуточку внимания!" +
                    $"\n\n{userName} бросил дружеский вызов вызов игроку {repliedUserName}!" +
                    $"\nВнимание, @{repliedUserTag}! Чтобы принять вызов, перейдите в ЛС бота!", replyMarkup: inline);
                MySql().Read(newSession, "");
                lobbyTimer.p1id = $"{userId}";
                lobbyTimer.p2id = $"{repliedUserId}";
                lobbyTimer.p1name = $"{userName}";
                lobbyTimer.p2name = $"{repliedUserName}";
                lobbyTimer.rootgroup = $"{groupId}";
                lobbyTimer.WaitTimer = new System.Timers.Timer(900000);
                lobbyTimer.WaitTimer.Elapsed += (sender, e) => TimerElapsed(sender, e, lobbyTimer);
                lobbyTimer.WaitTimer.Start();
            }
        }

        public static async Task GetChallenge(ITelegramBotClient botClient, Update update, LobbyTimer lobbyTimer) //Через ЛС проверяет наличие вызовов на дуэль.
        {
            var message = update.Message;
            long userId = update.Message.From.Id;
            var checkIncoming = $"SELECT `p2id` FROM `duels` WHERE p2id = '{userId}';";
            var checkOutcoming = $"SELECT `p1id` FROM `duels` WHERE p1id = '{userId}';";
            var GetIsFriendly = $"SELECT `IsFriendly` FROM `duels` WHERE `p1id` = '{userId}' OR `p2id` = '{userId}';";

            List<string> IsFriendlyStr = MySql().Read(GetIsFriendly, "IsFriendly");
            string IsFriendly = IsFriendlyStr.FirstOrDefault();
            List<string> In = MySql().Read(checkIncoming, "p2id");
            List<string> Out = MySql().Read(checkOutcoming, "p1id");
            if (In.Count == 0 && Out.Count == 0)
            {
                await botClient.SendTextMessageAsync(message.Chat, "У вас нет никаких вызовов!");
            }
            else if (Out.Count != 0)
            {
                if (IsFriendly == "True")
                {
                    var FriendlyKeyboard = new ReplyKeyboardMarkup(new[] { new KeyboardButton("Отменить вызов"), new KeyboardButton("Отключить дружескую дуэль") }) { ResizeKeyboard = true };
                    await botClient.SendTextMessageAsync(message.Chat, "У вас есть исходящий вызов!", replyMarkup: FriendlyKeyboard);
                }
                else
                {
                    var NotFriendlyKeyboard = new ReplyKeyboardMarkup(new[] { new KeyboardButton("Отменить вызов"), new KeyboardButton("Включить дружескую дуэль") }) { ResizeKeyboard = true };
                    await botClient.SendTextMessageAsync(message.Chat, "У вас есть исходящий вызов!", replyMarkup: NotFriendlyKeyboard);
                }
            }
            else if (In.Count != 0)
            {
                var keyboard = new ReplyKeyboardMarkup(new[]
                {new[]{new KeyboardButton("Принять вызов")},
                new[]{new KeyboardButton("Отклонить вызов")}})
                { ResizeKeyboard = true };

                await botClient.SendTextMessageAsync(message.Chat, "У вас есть входящий вызов!", replyMarkup: keyboard);
            }
        }

        public static async Task ChallengeAccept(ITelegramBotClient botClient, Update update, LobbyTimer lobbyTimer)
        {
            var message = update.Message;
            long userId = update.Message.From.Id;
            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new[]
                {
                    new KeyboardButton("Я готов!")
                }
            })
            { ResizeKeyboard = true };

            var columnsToRetrieve = new string[] { "p1id", "p2id", "p1name", "p2name", "rootgroup" };
            var ValuesGet = $"SELECT p1id, p2id, p1name, p2name, rootgroup FROM duels WHERE p1id = {userId} OR p2id = {userId};";
            List<Dictionary<string, object>> results = MySql().ExtRead(ValuesGet, columnsToRetrieve);
            foreach (var result in results)
            {
                var p1id = result["p1id"].ToString();
                var p2id = result["p2id"].ToString();
                var p1name = result["p1name"].ToString();
                var p2name = result["p2name"].ToString();
                var rootgroup = result["rootgroup"].ToString();

                try
                {
                    var SetGameStart = $"UPDATE `duels` SET `isAllowed` = '1' WHERE p2id = '{userId}';";
                    await botClient.SendTextMessageAsync
                        ($"{p2id}", $"Вызов на дуэль от игрока {p1name} принят!\n" +
                                    $"Подготовься к бою и ознакомься с правилами:\n" +
                                    $"1. Битва будет проходить на поле 3x3, каждый из оппонентов начинает игру в противоположных углах поля: {p1name} начнёт игру с левого нижнего угла, {p2name} начнёт игру с правого верхнего! Обоим игрокам будет выдано по 3 жизни\n" +
                                    $"2. Каждый игрок имеет право сделать по одному взмаху своим хуём за раз в любом из четырёх направлений: вверх, вниз, вправо или влево. После каждого взмаха действует кулдаун в 1 секунду!\n" +
                                    $"3. При столкновении двух игроков на одной позиции считается, что ваши питомцы отразили друг друга! При таком условии засчитывается ничья, и каждый откатывается до первоначальных позиций!\n" +
                                    $"4. Если один из игроков попадает на позицию второго игрока - у того, кто стоял на этой клетке раньше - отнимается жизнь!\n" +
                                    $"5. Игра автоматически заканчивается по истечении 15 минут, либо когда один из игроков потерял все свои жизни! В этом случае победа засчитывается тому, кто смог сохранить жизни!\n" +
                                    $"6. У проигравшего игрока автоматически снимается рандомное количество размеров его питомца (7-10), и отнятое значение присоединяется победителю!\n" +
                                    $"\n" +
                                    $"Всё понятно? Готов к игре? Тогда жми кнопку \"Готов\"! Желаю удачи!", replyMarkup: keyboard);

                    try
                    {
                        await botClient.SendTextMessageAsync
                        ($"{p1id}", $"Внимание! Игрок {p2name} принял ваш вызов на дуэль!\n" +
                                    $"Подготовьтесь к бою и ознакомьтесь с правилами:\n" +
                                    $"1. Битва будет проходить на поле 3x3, каждый из оппонентов начинает игру в противоположных углах поля: {p1name} начнёт игру с левого нижнего угла, {p2name} начнёт игру с правого верхнего! Обоим игрокам будет выдано по 3 жизни\n" +
                                    $"2. Каждый игрок имеет право сделать по одному взмаху своим хуём за раз в любом из четырёх направлений: вверх, вниз, вправо или влево. После каждого взмаха действует кулдаун в 1 секунду!\n" +
                                    $"3. При столкновении двух хуёв на одной позиции считается, что ваши шпаги отразили друг друга! При таком условии засчитывается ничья, и каждый откатывается до первоначальных позиций!\n" +
                                    $"4. Если один из игроков попадает на позицию второго игрока - у того, кто стоял на этой клетке раньше - отнимается жизнь!\n" +
                                    $"5. Игра автоматически заканчивается по истечении 10 минут, либо когда один из игроков потерял все свои жизни! В этом случае победа засчитывается тому, кто смог сохранить жизни!\n" +
                                    $"6. У проигравшего игрока автоматически снимается рандомное количество СМ его хуя (7-10), и отнятое значение присоединяется победителю!\n" +
                                    $"\n" +
                                    $"Всё понятно? Готов к игре? Тогда жми кнопку \"Готов\"! Желаю удачи!", replyMarkup: keyboard);
                        await botClient.SendTextMessageAsync($"{rootgroup}", $"Внимание беседа! {p2name} принял вызов на дуэль на хуях от игрока {p1name}!\nОжидаем готовности обоих участников дуэли!");
                    }
                    catch (ApiRequestException ex) when (ex.Message.Contains("Forbidden: bot can't initiate conversation with a user"))
                    {
                        await botClient.SendTextMessageAsync($"{rootgroup}", $"Внимание беседа! {p2name} принял вызов на дуэль на хуях от игрока {p1name}!\nОжидаем готовности обоих участников дуэли!\n\nОбращение к пользователю {p1name}: Чтобы запустить игру - необходимо начать ЛС со мной! перейди ко мне в чат и напиши /start!");
                    }

                    MySql().Read(SetGameStart, "");
                }

                catch (ApiRequestException ex) when (ex.Message.Contains("bot was blocked by the user") || ex.Message.Contains("Forbidden: bot can't initiate conversation with a user"))
                {
                    var StandardKeyboard = new ReplyKeyboardMarkup(new KeyboardButton("Вызовы от игроков")) { ResizeKeyboard = true };
                    try { await botClient.SendTextMessageAsync(p1id, "Ваш оппонент заблокировал меня. Дуэль отменена!", replyMarkup: StandardKeyboard); }
                    catch (ApiRequestException exm) when (exm.Message.Contains("bot was blocked by the user")) { await botClient.SendTextMessageAsync(p2id, "Ваш оппонент заблокировал меня. Дуэль отменена!", replyMarkup: StandardKeyboard); }
                    await botClient.SendTextMessageAsync(rootgroup, "🚫Внимание! Кто-то из пользователей заблокировал меня! Дуэль отменяется.\nПожалуйста, разблокируйте меня, иначе проведение дуэли будет невозможным!");
                    var GameDelete = $"DELETE FROM `duels` WHERE `p2id` = '{p2id}' OR `p1id` = '{p1id}';";
                    MySql().Read(GameDelete, "");
                }
            }
        }

        public static async Task ChallengeDeny(ITelegramBotClient botClient, Update update, LobbyTimer lobbyTimer)
        {
            var message = update.Message;
            long userId = update.Message.From.Id;
            var keyboard = new ReplyKeyboardMarkup(new KeyboardButton("Вызовы от игроков")) { ResizeKeyboard = true };
            var columnsToRetrieve = new string[] { "p1id", "p1name", "p2name", "rootgroup" };
            var ValuesGet = $"SELECT p1id, p1name, p2name, rootgroup FROM duels WHERE p1id = {userId} OR p2id = {userId};";
            List<Dictionary<string, object>> results = MySql().ExtRead(ValuesGet, columnsToRetrieve);
            foreach (var result in results)
            {
                var p1id = result["p1id"].ToString();
                var p1name = result["p1name"].ToString();
                var p2name = result["p2name"].ToString();
                var rootgroup = result["rootgroup"].ToString();
                var GameDelete = $"DELETE FROM `duels` WHERE `p2id` = '{userId}';";

                try
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Вызов отклонён!", replyMarkup: keyboard);
                    await botClient.SendTextMessageAsync(rootgroup, $"{p2name} отклонил запрос на дуэль от игрока {p1name} :(");
                    try
                    {
                        var chat = await botClient.GetChatAsync(p1id);
                        await botClient.SendTextMessageAsync(p1id, $"Вызов, отправленный игроку {p2name}, был отклонён!", replyMarkup: keyboard);
                    }
                    catch (ApiRequestException ex) when (ex.Message.Contains("chat not found"))
                    {

                    }
                    MySql().Read(GameDelete, "");
                }
                catch (ApiRequestException ex) when (ex.Message.Contains("bot was blocked by the user"))
                {
                    var StandardKeyboard = new ReplyKeyboardMarkup(new KeyboardButton("Вызовы от игроков")) { ResizeKeyboard = true };
                    await botClient.SendTextMessageAsync(p1id, "Ваш оппонент заблокировал меня. Дуэль отменена!", replyMarkup: StandardKeyboard);
                    await botClient.SendTextMessageAsync(rootgroup, "🚫Внимание! Кто-то из пользователей заблокировал меня! Дуэль отменяется.\nПожалуйста, разблокируйте меня, иначе проведение дуэли будет невозможным!");
                    var GameDelete2 = $"DELETE FROM `duels` WHERE `p2id` = '{userId}' OR `p1id` = '{p1id}';";
                    lobbyTimer.WaitTimer.Stop();
                    lobbyTimer.WaitTimer.Dispose();
                    MySql().Read(GameDelete2, "");
                }
            }
        }

        public static async Task ChallengeCancel(ITelegramBotClient botClient, Update update, LobbyTimer lobbyTimer)
        {
            var message = update.Message;
            long userId = update.Message.From.Id;
            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new[]
                {
                    new KeyboardButton("Вызовы от игроков")
                }
            })
            { ResizeKeyboard = true };

            var columnsToRetrieve = new string[] { "p1id", "p2id", "p1name", "p2name", "rootgroup" };
            var ValuesGet = $"SELECT p1id, p2id, p1name, p2name, rootgroup FROM duels WHERE p1id = {userId} OR p2id = {userId};";
            List<Dictionary<string, object>> results = MySql().ExtRead(ValuesGet, columnsToRetrieve);
            foreach (var result in results)
            {
                var p1id = result["p1id"].ToString();
                var p2id = result["p2id"].ToString();
                var p1name = result["p1name"].ToString();
                var p2name = result["p2name"].ToString();
                var rootgroup = result["rootgroup"].ToString();

                try
                {
                    if (p2id != null || rootgroup != null)
                    {
                        var GameDelete = $"DELETE FROM `duels` WHERE `p1id` = '{userId}';";
                        await botClient.SendTextMessageAsync(message.Chat, "Вызов отменён!");
                        try
                        {
                            await botClient.SendTextMessageAsync(p2id, "Входящий вам вызов на дуэль был отменён отправителем!", replyMarkup: keyboard);
                        }
                        catch (ApiRequestException ex) when (ex.Message.Contains("Forbidden: bot can't initiate conversation with a user"))
                        {

                        }

                        await botClient.SendTextMessageAsync(rootgroup, $"⛔️{p1name} Отменил вызов на дуэль игроку {p2name}!");
                        MySql().Read(GameDelete, "");
                    }
                }
                catch (ApiRequestException ex) when (ex.Message.Contains("bot was blocked by the user"))
                {
                    var StandardKeyboard = new ReplyKeyboardMarkup(new KeyboardButton("Вызовы от игроков")) { ResizeKeyboard = true };
                    try { await botClient.SendTextMessageAsync(p1id, "Ваш оппонент заблокировал меня. Дуэль отменена!", replyMarkup: StandardKeyboard); }
                    catch (ApiRequestException exm) when (exm.Message.Contains("bot was blocked by the user")) { await botClient.SendTextMessageAsync(p2id, "Ваш оппонент заблокировал меня. Дуэль отменена!", replyMarkup: StandardKeyboard); }
                    await botClient.SendTextMessageAsync(rootgroup, "⛔️Внимание! Кто-то из пользователей заблокировал меня! Дуэль отменяется.\nПожалуйста, разблокируйте меня, иначе проведение дуэли будет невозможным!");
                    var GameDelete = $"DELETE FROM `duels` WHERE `p2id` = '{p2id}' OR `p1id` = '{p1id}';";
                    MySql().Read(GameDelete, "");
                }
            }
        }

        public static async Task SetReady(ITelegramBotClient botClient, Update update)
        {
            var message = update.Message;
            var userId = update.Message.From.Id;
            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new[]
                {
                    new KeyboardButton("Не готов!")
                }
            })
            { ResizeKeyboard = true };

            var columnsToRetrieve = new string[] { "p1id", "p2id", "p1name", "p2name", "p1IsReady", "p2IsReady", "rootgroup" };
            var ValuesGet = $"SELECT p1id, p2id, p1name, p2name, p1IsReady, p2IsReady, rootgroup FROM duels WHERE p1id = {userId} OR p2id = {userId};";
            List<Dictionary<string, object>> results = MySql().ExtRead(ValuesGet, columnsToRetrieve);
            foreach (var result in results)
            {
                var p1id = result["p1id"].ToString();
                var p2id = result["p2id"].ToString();
                var p1name = result["p1name"].ToString();
                var p2name = result["p2name"].ToString();
                var p1IsReady = result["p1IsReady"].ToString();
                var p2IsReady = result["p2IsReady"].ToString();
                var rootgroup = result["rootgroup"].ToString();

                try
                {
                    if (p1id == $"{userId}" && p2IsReady == "False")
                    {
                        var p1SetReady = $"UPDATE `duels` SET `p1IsReady` = '1' WHERE p1id = '{userId}';";
                        await botClient.SendTextMessageAsync(message.Chat, "✅Ты подтвердил свою готовность! Ожидание оппонента...", replyMarkup: keyboard);
                        await botClient.SendTextMessageAsync(p2id, "✅Твой оппонент готов к бою!");
                        MySql().Read(p1SetReady, "");
                    }
                    else if (p2id == $"{userId}" && p1IsReady == "False")
                    {
                        var p2SetReady = $"UPDATE `duels` SET `p2IsReady` = '1' WHERE p2id = '{userId}';";
                        await botClient.SendTextMessageAsync(message.Chat, "✅Ты подтвердил свою готовность! Ожидание оппонента...", replyMarkup: keyboard);
                        await botClient.SendTextMessageAsync(p1id, "✅Твой оппонент готов к бою!");
                        MySql().Read(p2SetReady, "");
                    }
                    else if (p1id == $"{userId}" && p2IsReady == "True")
                    {
                        var removeKeyboard = new ReplyKeyboardRemove();
                        var p1SetReady = $"UPDATE `duels` SET `p1IsReady` = '1' WHERE p1id = '{userId}';";
                        await botClient.SendTextMessageAsync(message.Chat, "🎮Ты подтвердил свою готовность! Игра начинается!", replyMarkup: removeKeyboard);
                        await botClient.SendTextMessageAsync(p2id, "🎮Игра начинается!", replyMarkup: removeKeyboard);
                        await botClient.SendTextMessageAsync(rootgroup, "Дуэль подтверждена обоими участниками!\n" +
                            "Дуэлянты:\n" +
                            $"{p1name} ⚔️ {p2name}\n" +
                            $"\n" +
                            $"Игра скоро начнётся!");
                        MySql().Read(p1SetReady, "");
                        var GameDelete = $"DELETE FROM `duels` WHERE `p1id` = '{userId}';";
                        Game.Game dickfight = new Game.Game(userId);
                        dickfight.StartGame(botClient, update);
                        Games.Add(dickfight);
                    }
                    else if (p2id == $"{userId}" && p1IsReady == "True")
                    {
                        var removeKeyboard = new ReplyKeyboardRemove();
                        var p2SetReady = $"UPDATE `duels` SET `p2IsReady` = '1' WHERE p2id = '{userId}';";
                        await botClient.SendTextMessageAsync(message.Chat, "🎮Ты подтвердил свою готовность! Игра начинается!", replyMarkup: removeKeyboard);
                        await botClient.SendTextMessageAsync(p1id, "🎮Игра начинается!", replyMarkup: removeKeyboard);
                        await botClient.SendTextMessageAsync(rootgroup, "Дуэль подтверждена обоими участниками!\n" +
                            "Дуэлянты:\n" +
                            $"{p1name} ⚔️ {p2name}\n" +
                            $"\n" +
                            $"Игра скоро начнётся!");
                        MySql().Read(p2SetReady, "");
                        Game.Game dickfight = new Game.Game(userId);
                        dickfight.StartGame(botClient, update);
                        Games.Add(dickfight);
                        Console.WriteLine(Games);
                        lobbyTimer.WaitTimer.Stop();
                        lobbyTimer.WaitTimer.Dispose();
                    }
                }
                catch (ApiRequestException ex) when (ex.Message.Contains("bot was blocked by the user"))
                {
                    var StandardKeyboard = new ReplyKeyboardMarkup(new KeyboardButton("Вызовы от игроков")) { ResizeKeyboard = true };
                    try { await botClient.SendTextMessageAsync(p1id, "Ваш оппонент заблокировал меня. Дуэль отменена!", replyMarkup: StandardKeyboard); }
                    catch (ApiRequestException exm) when (exm.Message.Contains("bot was blocked by the user")) { await botClient.SendTextMessageAsync(p2id, "Ваш оппонент заблокировал меня. Дуэль отменена!", replyMarkup: StandardKeyboard); }
                    await botClient.SendTextMessageAsync(rootgroup, "🚫Внимание! Кто-то из пользователей заблокировал меня! Дуэль отменяется.\nПожалуйста, разблокируйте меня, иначе проведение дуэли будет невозможным!");
                    var GameDelete = $"DELETE FROM `duels` WHERE `p2id` = '{p2id}' OR `p1id` = '{p1id}';";
                    lobbyTimer.WaitTimer.Stop();
                    lobbyTimer.WaitTimer.Dispose();
                    MySql().Read(GameDelete, "");
                }
            }
        }

        public static async Task SetNotReady(ITelegramBotClient botClient, Update update, LobbyTimer lobbyTimer)
        {
            var message = update.Message;
            var userId = update.Message.From.Id;
            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new[]
                {
                    new KeyboardButton("Я готов!")
                }
            })
            { ResizeKeyboard = true };
            var columnsToRetrieve = new string[] { "p1id", "p2id", "p1IsReady", "p2IsReady", "rootgroup" };
            var ValuesGet = $"SELECT p1id, p2id, p1IsReady, p2IsReady, rootgroup FROM duels WHERE p1id = {userId} OR p2id = {userId};";
            List<Dictionary<string, object>> results = MySql().ExtRead(ValuesGet, columnsToRetrieve);
            foreach (var result in results)
            {
                var p1id = result["p1id"].ToString();
                var p2id = result["p2id"].ToString();
                var p1IsReady = result["p1IsReady"].ToString();
                var p2IsReady = result["p2IsReady"].ToString();
                var rootgroup = result["rootgroup"].ToString();
                try
                {
                    if (p1id == $"{userId}" && p2IsReady == "False")
                    {
                        var p1SetReady = $"UPDATE `duels` SET `p1IsReady` = '0' WHERE p1id = '{userId}';";
                        MySql().Read(p1SetReady, "");
                        await botClient.SendTextMessageAsync(message.Chat, "💤Ты отменил свою готовность!", replyMarkup: keyboard);
                        await botClient.SendTextMessageAsync(p2id, "💤Твой оппонент отменил готовность!");
                    }
                    else if (p2id == $"{userId}" && p1IsReady == "False")
                    {
                        var p2SetReady = $"UPDATE `duels` SET `p2IsReady` = '0' WHERE p2id = '{userId}';";
                        MySql().Read(p2SetReady, "");
                        await botClient.SendTextMessageAsync(message.Chat, "💤Ты отменил свою готовность!", replyMarkup: keyboard);
                        await botClient.SendTextMessageAsync(p1id, "💤Твой оппонент отменил готовность!");
                    }
                }
                catch (ApiRequestException ex) when (ex.Message.Contains("bot was blocked by the user"))
                {
                    var StandardKeyboard = new ReplyKeyboardMarkup(new KeyboardButton("Вызовы от игроков")) { ResizeKeyboard = true };
                    try { await botClient.SendTextMessageAsync(p1id, "Ваш оппонент заблокировал меня. Дуэль отменена!", replyMarkup: StandardKeyboard); }
                    catch (ApiRequestException exm) when (exm.Message.Contains("bot was blocked by the user")) { await botClient.SendTextMessageAsync(p2id, "Ваш оппонент заблокировал меня. Дуэль отменена!", replyMarkup: StandardKeyboard); }
                    await botClient.SendTextMessageAsync(rootgroup, "🚫Внимание! Кто-то из пользователей заблокировал меня! Дуэль отменяется.\nПожалуйста, разблокируйте меня, иначе проведение дуэли будет невозможным!");
                    var GameDelete = $"DELETE FROM `duels` WHERE `p2id` = '{p2id}' OR `p1id` = '{p1id}';";
                    MySql().Read(GameDelete, "");
                    lobbyTimer.WaitTimer.Stop();
                    lobbyTimer.WaitTimer.Dispose();
                }
            }
        }
        private static void TimerElapsed(object sender, ElapsedEventArgs e, LobbyTimer lobbyTimer)
        {
            ITelegramBotClient botClient = new TelegramBotClient("Ключ Telegram-бота");
            Update update = new();
            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new[]
                {
                    new KeyboardButton("Вызовы от игроков")
                }
            })
            { ResizeKeyboard = true };

            var columnsToRetrieve = new string[] { "p1id", "p2id", "p1name", "p2name", "rootgroup" };
            var ValuesGet = $"SELECT p1id, p2id, p1name, p2name, rootgroup FROM duels WHERE p1id = {lobbyTimer.p1id} OR p2id = {lobbyTimer.p2id};";
            List<Dictionary<string, object>> results = MySql().ExtRead(ValuesGet, columnsToRetrieve);
            foreach (var result in results)
            {
                var p1id = result["p1id"].ToString();
                var p2id = result["p2id"].ToString();
                var p1name = result["p1name"].ToString();
                var p2name = result["p2name"].ToString();
                var rootgroup = result["rootgroup"].ToString();

                if (p2id != null || rootgroup != null)
                {
                    botClient.SendTextMessageAsync(p1id, "⏱Время ожидания подтверждения дуэли истекло! Дуэль отменяется!", replyMarkup: keyboard);
                    botClient.SendTextMessageAsync(p2id, "⏱Время ожидания подтверждения дуэли истекло! Дуэль отменяется!", replyMarkup: keyboard);
                    botClient.SendTextMessageAsync(rootgroup, $"⏱Время ожидания подтверждения дуэли между {p1name} и {p2name} истекло! Дуэль отменяется!");
                }
                lobbyTimer.WaitTimer.Stop();
                lobbyTimer.WaitTimer.Dispose();
                var GameDelete = $"DELETE FROM `duels` WHERE `p1id` = '{p1id}';";
                MySql().Read(GameDelete, "");
            }
        }
    }
}
