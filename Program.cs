using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using System.Timers;
using PetBotCs;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using PetBotCs.Game;
using System.Text.RegularExpressions;

namespace PetBotCs
{
    class Program
    {
        static readonly ITelegramBotClient bot = new TelegramBotClient("Ключ Telegram-бота");
        static sql database = new("Данные для подключения к SQL");


        static async Task Main(string[] args)
        {
            Update update;

            if (args is null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            Console.WriteLine($"Бот под названием '{(await bot.GetMeAsync()).FirstName}' запущен!");
            InternalUsages.SetTimer();

            var GetGames = $"SELECT `p1id` FROM `duels`;";
            List<string> p1idString = database.Read(GetGames, "p1id");
            foreach (var idString in p1idString)
            { Lobby.Games.Add(new Game.Game(long.Parse(idString))); }


            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions { AllowedUpdates = { } };
            bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken);

            Console.ReadLine();

            cts.Cancel();
        }


        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            var message = update.Message;

            if (update.Type == UpdateType.MyChatMember)
            {
                long userId = update.MyChatMember.Chat.Id;
                var chatMemberUpdate = update.MyChatMember;
                Game.Game Session = Game.Game.getSessionByUserId(userId);
                if (Session != null)
                {
                    if (chatMemberUpdate.NewChatMember.Status == ChatMemberStatus.Kicked)
                    {
                        Game.Game.getSessionByUserId(userId).EndGameCauseBlocked(userId, botClient, Game.Game.getSessionByUserId(userId));
                    }
                }
            }
            if (message != null)
            {
                if (message.Text != null)
                {
                    var lowercaseText = message.Text.ToLower();
                    const string botname = "@botname"; //Здесь должен быть username бота в нижнем регистре (маленькими буквами)
                    Random random = new();
                    double chance = 0.3;
                    double randomNumber = random.Next(1, 10) / 10.0;
                    var removeKeyboard = new ReplyKeyboardRemove();
                    long userId = message.From.Id;
                    var getIsAllowed = $"SELECT `isAllowed` FROM `duels` WHERE p2id = '{userId}';";
                    List<string> usage = database.Read(getIsAllowed, "isAllowed");
                    string isAllowed = usage.FirstOrDefault();
                    LobbyTimer lobbyTimer = new();
                    object sender = new();
                    var MoveType = message.Text;

                    if (message.Chat.Type != ChatType.Private)
                    {
                        if (message.Text.StartsWith("/petfight @"))
                        {
                            long groupId = message.Chat.Id;
                            string username = message.Text.Replace("/petfight @", "");
                            if (!string.IsNullOrEmpty(username))
                            {
                                Lobby.HandlePetFightCommand(botClient, update, username, groupId, false);
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(message.Chat, "Чтобы бросить кому-то вызов - впишите его тег полностью!");
                            }
                        }
                        if (message.Text.StartsWith($"/petfight{botname} @"))
                        {
                            long groupId = message.Chat.Id;
                            string username = message.Text.Replace($"/petfight{botname} @", "");
                            if (!string.IsNullOrEmpty(username))
                            {
                                Lobby.HandlePetFightCommand(botClient, update, username, groupId, false);
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(message.Chat, "Чтобы бросить кому-то вызов - впишите его тег полностью!");
                            }
                        }

                        if (message.Text.StartsWith("/friendlypetfight @"))
                        {
                            long groupId = message.Chat.Id;
                            string username = message.Text.Replace("/friendlypetfight @", "");
                            if (!string.IsNullOrEmpty(username))
                            {
                                Lobby.HandlePetFightCommand(botClient, update, username, groupId, false);
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(message.Chat, "Чтобы бросить кому-то вызов - впишите его тег полностью!");
                            }
                        }
                        if (message.Text.StartsWith($"/friendlypetfight{botname} @"))
                        {
                            long groupId = message.Chat.Id;
                            string username = message.Text.Replace($"/friendlypetfight{botname} @", "");
                            if (!string.IsNullOrEmpty(username))
                            {
                                Lobby.HandlePetFightCommand(botClient, update, username, groupId, false);
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(message.Chat, "Чтобы бросить кому-то вызов - впишите его тег полностью!");
                            }
                        }

                        if (message.Text.StartsWith("/stealfood @"))
                        {
                            long groupId = message.Chat.Id;
                            string username = message.Text.Replace("/stealfood @", "");
                            if (!string.IsNullOrEmpty(username))
                            {
                                stealfood.HandleStealfoodCommand(botClient, update, username, groupId);
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(message.Chat, "Чтобы украсть чью-то еду - впишите его тег полностью!");
                            }
                        }
                        if (message.Text.StartsWith($"/stealfood{botname} @"))
                        {
                            long groupId = message.Chat.Id;
                            string username = message.Text.Replace($"/stealfood{botname} @", "");
                            if (!string.IsNullOrEmpty(username))
                            {
                                stealfood.HandleStealfoodCommand(botClient, update, username, groupId);
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(message.Chat, "Чтобы украсть чью-то еду - впишите его тег полностью!");
                            }
                        }
                    }
                    else { }

                    switch (lowercaseText)
                    {
                        //Обработка основных команд бота
                        case "/petfight":
                        case $"/petfight{botname}":
                            if (message.ReplyToMessage?.From.Id == null)
                            {
                                await botClient.SendTextMessageAsync(message.Chat, "↩️Чтобы бросить кому-то вызов - выберите его реплаем!");
                            }
                            else if (message.ReplyToMessage.From.IsBot == true)
                            {
                                await botClient.SendTextMessageAsync(message.Chat, "Отправлять ботам запрос на дуэль запрещено! У них же нет хуя!");
                            }
                            else
                            {
                                string repliedUserName = update.Message.ReplyToMessage?.From.FirstName;
                                long repliedUserId = update.Message.ReplyToMessage.From.Id;
                                string repliedUserTag = update.Message.ReplyToMessage?.From.Username;
                                await Lobby.Alert(botClient, update, repliedUserName, repliedUserId, repliedUserTag);
                            }
                            break;

                        case "/friendlypetfight":
                        case $"/friendlypetfight{botname}":
                            if (message.ReplyToMessage?.From.Id == null)
                            {
                                await botClient.SendTextMessageAsync(message.Chat, "↩️Чтобы бросить кому-то вызов - выберите его реплаем!");
                            }
                            else if (message.ReplyToMessage.From.IsBot == true)
                            {
                                await botClient.SendTextMessageAsync(message.Chat, "😵Отправлять ботам запрос на дуэль запрещено! У них же нет хуя!");
                            }
                            else
                            {
                                string repliedUserName = update.Message.ReplyToMessage.From.FirstName;
                                long repliedUserId = update.Message.ReplyToMessage.From.Id;
                                string repliedUserTag = update.Message.ReplyToMessage.From.Username;
                                await Lobby.FriendlyAlert(botClient, update, repliedUserName, repliedUserId, repliedUserTag);
                            }
                            break;

                        case "/dickinass":
                        case $"/dickinass{botname}":
                            if (message.Chat.Type != ChatType.Private)
                            { await Mypet.Dblogic(botClient, update); }
                            else
                            { await botClient.SendTextMessageAsync(message.Chat, "Данная команда недоступна в личных сообщениях!"); }
                            break;

                        case "/dicktop":
                        case $"/dicktop{botname}":
                            if (message.Chat.Type != ChatType.Private)
                            { await top.pettop(botClient, update, database); }
                            else
                                await botClient.SendTextMessageAsync(message.Chat, "Данная команда недоступна в личных сообщениях!");
                            break;

                        case "/dickcut":
                        case $"/dickcut{botname}":
                            if (message.Chat.Type != ChatType.Private)
                            {
                                if (message.ReplyToMessage?.From.Id == null)
                                {
                                    await botClient.SendTextMessageAsync(message.Chat, "↩️Чтобы бросить кому-то вызов - выберите его реплаем!");
                                }
                                else
                                {
                                    string repliedUserName2 = update.Message.ReplyToMessage.From.FirstName;
                                    long repliedUserId2 = update.Message.ReplyToMessage.From.Id;
                                    string repliedUserTag2 = update.Message.ReplyToMessage.From.Username;
                                    await stealfood.Cut(botClient, update, repliedUserName2, repliedUserId2, repliedUserTag2);
                                }
                            }
                            else
                            { await botClient.SendTextMessageAsync(message.Chat, "Данная команда недоступна в личных сообщениях!"); }
                            break;

                        case "/resetsteal":
                            if (message.From.Id == 0) //служебная команда, вместо 0 должен быть ID администатора
                            {
                                await InternalUsages.resetCut(botClient, update);
                                await botClient.SendTextMessageAsync(message.Chat, "Ограничения на использование /stealfood сняты!");
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(message.Chat, "Отказано в доступе!");
                            }
                            break;
                        case "/resetuses": //служебная команда, вместо 0 должен быть ID администатора
                            if (message.From.Id == 0)
                            {
                                await InternalUsages.ResetIsUsedToday();
                                await botClient.SendTextMessageAsync(message.Chat, "Счётчик /mypet сброшен!");
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(message.Chat, "Отказано в доступе!");
                            }
                            break;
                        case "/todb":
                            if (message.ReplyToMessage?.From.Id == null && message.From.Id != 0) //служебная команда, вместо 0 должен быть ID администатора
                            {
                                await botClient.SendTextMessageAsync(message.Chat, "Ошибка");
                            }
                            long groupId = message.Chat.Id;
                            long replUserId = update.Message.ReplyToMessage.From.Id;
                            string replUserTag = update.Message.ReplyToMessage?.From.Username;
                            var todb = $"UPDATE `group{groupId}` SET `username` = '{replUserTag}' WHERE `name` = '{replUserId}'";
                            database.Read(todb, "");
                            await botClient.DeleteMessageAsync(message.Chat, message.MessageId);
                            break;
                        case "/test":
                            await botClient.SendTextMessageAsync(message.Chat, "Я живой");
                            break;

                        case "/about":
                        case $"/about{botname}":
                            await About.aboutBot(botClient, update);
                            break;
                    }
                    switch (lowercaseText)
                    {
                        //Этот switch предназначен для команд, работающих только в лс бота.
                        case "/start":
                            if (message.Chat.Type == ChatType.Private)
                            {
                                await Start.Keyboard(botClient, update);
                            }
                            break;
                        case "вызовы от игроков":
                            if (message.Chat.Type == ChatType.Private)
                            {
                                await Lobby.GetChallenge(botClient, update, lobbyTimer);
                            }
                            break;
                        case "принять вызов":
                            if (message.Chat.Type == ChatType.Private && isAllowed == "False")
                            {
                                await Lobby.ChallengeAccept(botClient, update, lobbyTimer);
                            }
                            break;
                        case "отклонить вызов":
                            if (message.Chat.Type == ChatType.Private && isAllowed == "False")
                            {
                                await Lobby.ChallengeDeny(botClient, update, lobbyTimer);
                            }
                            break;
                        case "отменить вызов":
                            if (message.Chat.Type == ChatType.Private)
                            {
                                await Lobby.ChallengeCancel(botClient, update, lobbyTimer);
                                await Start.Keyboard(botClient, update);
                            }
                            break;
                        case "я готов!":
                            if (message.Chat.Type == ChatType.Private)
                            {
                                await Lobby.SetReady(botClient, update);
                            }
                            break;
                        case "не готов!":
                            if (message.Chat.Type == ChatType.Private)
                            {
                                await Lobby.SetNotReady(botClient, update, lobbyTimer);
                            }
                            break;
                        case "включить дружескую дуэль":
                            var GetIsFriendly = $"SELECT `IsFriendly` FROM `duels` WHERE `p1id` = '{userId}' OR `p2id` = '{userId}';";
                            List<string> IsFriendlyStr = database.Read(GetIsFriendly, "IsFriendly");
                            string IsFriendly = IsFriendlyStr.FirstOrDefault();
                            var GetP2id = $"SELECT `p2id` FROM `duels` WHERE `p1id` = '{userId}' OR `p2id` = '{userId}';";
                            List<string> p2idStr = database.Read(GetP2id, "p2id");
                            string p2id1 = p2idStr.FirstOrDefault();
                            if (IsFriendly != null && message.Chat.Type == ChatType.Private)
                            {
                                var SetFriendly = $"UPDATE `duels` SET `IsFriendly` = 1 WHERE p1id = {userId}";
                                database.Read(SetFriendly, "");
                                var NotFriendlyKeyboard = new ReplyKeyboardMarkup(new[] { new KeyboardButton("Отменить вызов"), new KeyboardButton("Отключить дружескую дуэль") }) { ResizeKeyboard = true };
                                await botClient.SendTextMessageAsync(message.Chat, "🤝Включена дружеская дуэль!", replyMarkup: NotFriendlyKeyboard);
                                await botClient.SendTextMessageAsync(p2id1, "🤝Ваш оппонент включил дружескую дуэль!");
                            }
                            break;
                        case "отключить дружескую дуэль":
                            var GetIsFriendly2 = $"SELECT `IsFriendly` FROM `duels` WHERE `p1id` = '{userId}' OR `p2id` = '{userId}';";
                            List<string> IsFriendlyStr2 = database.Read(GetIsFriendly2, "isFriendly");
                            string IsFriendly2 = IsFriendlyStr2.FirstOrDefault();
                            var GetP2id2 = $"SELECT `p2id` FROM `duels` WHERE `p1id` = '{userId}' OR `p2id` = '{userId}';";
                            List<string> p2idStr2 = database.Read(GetP2id2, "p2id");
                            string p2id2 = p2idStr2.FirstOrDefault();
                            if (IsFriendly2 != null && message.Chat.Type == ChatType.Private)
                            {
                                var SetNotFriendly = $"UPDATE `duels` SET `isFriendly` = 0 WHERE p1id = {userId}";
                                database.Read(SetNotFriendly, "");
                                var NotFriendlyKeyboard = new ReplyKeyboardMarkup(new[] { new KeyboardButton("Отменить вызов"), new KeyboardButton("Включить дружескую дуэль") }) { ResizeKeyboard = true };
                                await botClient.SendTextMessageAsync(message.Chat, "⚔️Дружеская дуэль отключена!", replyMarkup: NotFriendlyKeyboard);
                                await botClient.SendTextMessageAsync(p2id2, "⚔️Ваш оппонент отключил дружескую дуэль!");
                            }
                            break;

                        case "/updatelog":
                            
                            if (message.From.Id == 0) //служебная команда, вместо 0 должен быть ID администатора
                            {
                                About.UpdateLog(botClient, update);
                            }
                            else
                            { }
                            break;
                    }

                    //Обработка игрового процесса
                    var columnsToRetrieve = new string[] { "p1id", "p2id", "p1IsReady", "p2IsReady", "rootgroup" };
                    var GetDuel = $"SELECT `p1id`, `p2id`, `p1IsReady`, `p2IsReady`, `rootgroup` FROM `duels` WHERE p1id = '{userId}' OR p2id = '{userId}';";
                    List<Dictionary<string, object>> results = database.ExtRead(GetDuel, columnsToRetrieve);

                    foreach (var result in results)
                    {
                        var p1id = result["p1id"].ToString();
                        var p2id = result["p2id"].ToString();
                        var p1IsReady = result["p1IsReady"].ToString();
                        var p2IsReady = result["p2IsReady"].ToString();
                        var rootgroup = result["rootgroup"].ToString();
                        var GameDelete = $"DELETE FROM `duels` WHERE `p2id` = '{userId}';";

                        if (p1id != null && p1IsReady == "True" && p2IsReady == "True" && message.Chat.Type == ChatType.Private)
                        {
                            switch (lowercaseText)
                            {
                                //Функционал для работы мини-игры (движений по полю)
                                case "⬆️ шаг вверх":
                                    int[] Up = new[] { 1, 2, 3 };
                                    if (Up.Contains(Game.Game.getSessionByUserId(userId).GetPlayerByUserId(userId).Pos))
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat, "⛔️Дальше в этом двигаться махать нельзя!");
                                    }
                                    else
                                    {
                                        await botClient.SendTextMessageAsync(userId, $"Ты сделал {MoveType}! Следующий ход можно сделать через секунду!", replyMarkup: Keyboard.Waiting);
                                        await botClient.SendTextMessageAsync(Game.Game.getSessionByUserId(userId).GetOpponentByUserId(userId).Id, $"Твой оппонент сделал {MoveType}!");
                                        Game.Game.getSessionByUserId(userId).GetPlayerByUserId(userId).MoveTo(Direction.Up, Game.Game.getSessionByUserId(userId), botClient, userId);
                                        if (Game.Game.getSessionByUserId(userId).GetPlayerByUserId(userId).Pos == Game.Game.getSessionByUserId(userId).GetOpponentByUserId(userId).Pos)
                                        {
                                            Game.Game.getSessionByUserId(userId).CheckBlock(Game.Game.getSessionByUserId(userId), userId, botClient);
                                        }
                                        else
                                        {
                                            Game.Game.getSessionByUserId(userId).GetPlayerByUserId(userId).SetCooldown(update, Game.Game.getSessionByUserId(userId), userId);
                                        }
                                    }
                                    break;
                                case "⬅️ шаг влево":
                                    int[] Left = new[] { 1, 7, 13 };
                                    if (Left.Contains(Game.Game.getSessionByUserId(userId).GetPlayerByUserId(userId).Pos))
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat, "⛔️Дальше в этом двигаться махать нельзя!");
                                    }
                                    else
                                    {
                                        await botClient.SendTextMessageAsync(userId, $"Ты сделал {MoveType}! Следующий ход можно сделать через секунду!", replyMarkup: Keyboard.Waiting);
                                        await botClient.SendTextMessageAsync(Game.Game.getSessionByUserId(userId).GetOpponentByUserId(userId).Id, $"Твой оппонент сделал {MoveType}!");
                                        Game.Game.getSessionByUserId(userId).GetPlayerByUserId(userId).MoveTo(Direction.Left, Game.Game.getSessionByUserId(userId), botClient, userId);
                                        if (Game.Game.getSessionByUserId(userId).GetPlayerByUserId(userId).Pos == Game.Game.getSessionByUserId(userId).GetOpponentByUserId(userId).Pos)
                                        {
                                            Game.Game.getSessionByUserId(userId).CheckBlock(Game.Game.getSessionByUserId(userId), userId, botClient);
                                        }
                                        else
                                        {
                                            Game.Game.getSessionByUserId(userId).GetPlayerByUserId(userId).SetCooldown(update, Game.Game.getSessionByUserId(userId), userId);
                                        }
                                    }
                                    break;
                                case "➡️ шаг вправо":
                                    int[] Right = new[] { 3, 9, 15 };
                                    if (Right.Contains(Game.Game.getSessionByUserId(userId).GetPlayerByUserId(userId).Pos))
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat, "⛔️Дальше в этом двигаться махать нельзя!");
                                    }
                                    else
                                    {
                                        await botClient.SendTextMessageAsync(userId, $"Ты сделал {MoveType}! Следующий ход можно сделать через секунду!", replyMarkup: Keyboard.Waiting);
                                        await botClient.SendTextMessageAsync(Game.Game.getSessionByUserId(userId).GetOpponentByUserId(userId).Id, $"Твой оппонент сделал {MoveType}!");
                                        Game.Game.getSessionByUserId(userId).GetPlayerByUserId(userId).MoveTo(Direction.Right, Game.Game.getSessionByUserId(userId), botClient, userId);
                                        if (Game.Game.getSessionByUserId(userId).GetPlayerByUserId(userId).Pos == Game.Game.getSessionByUserId(userId).GetOpponentByUserId(userId).Pos)
                                        {
                                            Game.Game.getSessionByUserId(userId).CheckBlock(Game.Game.getSessionByUserId(userId), userId, botClient);
                                        }
                                        else
                                        {
                                            Game.Game.getSessionByUserId(userId).GetPlayerByUserId(userId).SetCooldown(update, Game.Game.getSessionByUserId(userId), userId);
                                        }
                                    }
                                    break;
                                case "⬇️ шаг вниз":
                                    int[] Down = new[] { 13, 14, 15 };
                                    if (Down.Contains(Game.Game.getSessionByUserId(userId).GetPlayerByUserId(userId).Pos))
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat, "⛔️Дальше в этом направлении двигаться нельзя!");
                                    }
                                    else
                                    {
                                        await botClient.SendTextMessageAsync(userId, $"Ты сделал {MoveType}! Следующий ход можно сделать через секунду!", replyMarkup: Keyboard.Waiting);
                                        await botClient.SendTextMessageAsync(Game.Game.getSessionByUserId(userId).GetOpponentByUserId(userId).Id, $"Твой оппонент сделал {MoveType}!");
                                        Game.Game.getSessionByUserId(userId).GetPlayerByUserId(userId).MoveTo(Direction.Down, Game.Game.getSessionByUserId(userId), botClient, userId);
                                        if (Game.Game.getSessionByUserId(userId).GetPlayerByUserId(userId).Pos == Game.Game.getSessionByUserId(userId).GetOpponentByUserId(userId).Pos)
                                        {
                                            Game.Game.getSessionByUserId(userId).CheckBlock(Game.Game.getSessionByUserId(userId), userId, botClient);
                                        }
                                        else
                                        {
                                            Game.Game.getSessionByUserId(userId).GetPlayerByUserId(userId).SetCooldown(update, Game.Game.getSessionByUserId(userId), userId);
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
            else { }

            //Код, который допускает добавление бота в другие беседы
            if (message != null && message.Type == MessageType.ChatMembersAdded)
            {
                foreach (var member in message.NewChatMembers)
                {
                    if (member.Id == bot.BotId)
                    {

                        long ChatId = message.Chat.Id;

                        bool exists = database.TableExists(ChatId);
                        if (!exists)
                        {
                            var CreateTable = $"CREATE TABLE `phpmyadmin`.`group{ChatId}` (`username` MEDIUMTEXT NOT NULL , `firstname` MEDIUMTEXT NOT NULL , `name` VARCHAR(11) NOT NULL , `size` INT(11) NOT NULL , `IsUsedToday` BOOLEAN NOT NULL , `IsCuttedToday` BOOLEAN NOT NULL , `bonus` INT(1) NOT NULL ) ENGINE = InnoDB;";
                            database.Read(CreateTable, "");
                            await botClient.SendTextMessageAsync(message.Chat,
                            "👋Всем привет! Я - бот, который создан против наколенника!" +
                            "\nЧто я умею:" +
                            "\n• 🔼/mypet - начать игру по выращиванию питомца!" +
                            "\n• ✂️/stealfood - украсть у кого-то еду для питомца!" +
                            "\n• 📋/pettop - отобразить топ-10 самых жирных питомцев в беседе!" +
                            "\n• ⚔️/petfight - вызвать игрока на битву питомцев!" +
                            "\n• 🤝/friendlypetfight - дружеский поединок питомцев!" +
                            "\n" +
                            "\nСоревнуйтесь между собой, занимайте топы, воруйте еду, желаю удачи!" +
                            "\n" +
                            "\nВнимание! Для моей корректной работы необходимо выдать мне доступ к переписке. Если вы этого не сделали - пожалуйста, сделайте это.");
                        }
                        else { }
                    }
                }
            }
        }
        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }
    }
}
