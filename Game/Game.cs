using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Runtime.CompilerServices;
using System.Timers;
using Telegram.Bot.Exceptions;

namespace PetBotCs.Game
{
    public class Game
    {
        public System.Timers.Timer BlockCooldown { get; set; }
        public System.Timers.Timer AttackCooldown;
        static sql database = new("server=127.0.0.1;uid=phpmyadmin;pwd=oralcumshot;database=phpmyadmin");
        public Player p1 { get; set; }
        public Player p2 { get; set; }
        private long userId { get; set; }

        public Game(long userId)
        {
            this.userId = userId;
            
            var columnsToRetrieve = new string[] { "p1id", "p2id", "p1name", "p2name", "p1IsReady", "p2IsReady", "p1pos", "p2pos", "p1hp", "p2hp", "rootgroup" };
            var ValuesGet = $"SELECT p1id, p2id, p1name, p2name, p1IsReady, p2IsReady, p1pos, p2pos, p1hp, p2hp, rootgroup FROM duels WHERE p1id = {userId} OR p2id = {userId};";
            List<Dictionary<string, object>> results = database.ExtRead(ValuesGet, columnsToRetrieve);
            System.Timers.Timer timer = new();
            foreach (var result in results)
            {
                long p1id = long.Parse(result["p1id"].ToString());
                long p2id = long.Parse(result["p2id"].ToString());
                var p1name = result["p1name"].ToString();
                var p2name = result["p2name"].ToString();
                var p1hp = int.Parse(result["p1hp"].ToString());
                var p2hp = int.Parse(result["p2hp"].ToString());
                int p1pos = int.Parse(result["p1pos"].ToString());
                int p2pos = int.Parse(result["p2pos"].ToString());
                var p1IsReady = result["p1IsReady"].ToString();
                var p2IsReady = result["p2IsReady"].ToString();
                var rootgroup = result["rootgroup"].ToString();

                p1 = new Player(p1id, p1pos, p1hp, p1name, timer);
                p2 = new Player(p2id, p2pos, p2hp, p2name, timer);

                ITelegramBotClient botClient = new TelegramBotClient("Ключ Telegram-бота");
                sql database = new("Данные для подключения к SQL");
                ReplyKeyboardMarkup keyboard = null;
                if (p1IsReady == "True" &&  p2IsReady == "True")
                {
                    if (GetPlayerByUserId(userId).Pos.Equals(1))
                        keyboard = Keyboard.LeftUp;
                    else if (GetPlayerByUserId(userId).Pos.Equals(2))
                        keyboard = Keyboard.Up;
                    else if (GetPlayerByUserId(userId).Pos.Equals(3))
                        keyboard = Keyboard.RightUp;
                    else if (GetPlayerByUserId(userId).Pos.Equals(7))
                        keyboard = Keyboard.Left;
                    else if (GetPlayerByUserId(userId).Pos.Equals(8))
                        keyboard = Keyboard.Standard;
                    else if (GetPlayerByUserId(userId).Pos.Equals(9))
                        keyboard = Keyboard.Right;
                    else if (GetPlayerByUserId(userId).Pos.Equals(13))
                        keyboard = Keyboard.LeftDown;
                    else if (GetPlayerByUserId(userId).Pos.Equals(14))
                        keyboard = Keyboard.Down;
                    else if (GetPlayerByUserId(userId).Pos.Equals(15))
                        keyboard = Keyboard.RightDown;

                    var p1MoveReset = $"UPDATE `duels` SET `p1IsMoved`='0' WHERE `p1id` = {userId};";
                    var p2MoveReset = $"UPDATE `duels` SET `p2IsMoved`='0' WHERE `p2id` = {userId};";
                    if (userId == p1.Id)
                        database.Read(p1MoveReset, "");
                    else if (userId == p2.Id)
                        database.Read(p2MoveReset, "");

                    botClient.SendTextMessageAsync(userId, "⌨️Клавиатура восстановлена!", replyMarkup: keyboard);
                }
            }
        }

        public void StartGame(ITelegramBotClient botClient, Update update)
        {
            var message = update.Message;

            botClient.SendTextMessageAsync(p1.Id, "Игра началась! Вы можете сделать свой ход!", replyMarkup: Keyboard.LeftDown);
            botClient.SendTextMessageAsync(p2.Id, "Игра началась! Вы можете сделать свой ход!", replyMarkup: Keyboard.RightUp);
        }

        public void Attack(Game game, long userId, ITelegramBotClient botClient)
        {
            var GetRootgroup = $"SELECT `rootgroup` FROM `duels` WHERE `p1id` = '{userId}' OR `p2id` = '{userId}';";
            List<string> rootgroupStr = database.Read(GetRootgroup, "rootgroup");
            long rootgroup = long.Parse(rootgroupStr[0]);
            if (GetOpponentByUserId(userId).Hp != 1)
            {
                switch (GetOpponentByUserId(userId).Hp)
                {
                    case 3:
                        botClient.SendTextMessageAsync(GetPlayerByUserId(userId).Id, $"💥Попадание! Твой боец стукнул по питомцу своего оппонента! У него отнимается 1 жизнь! Осталось ещё {GetOpponentByUserId(userId).Hp - 1} раз(а)!\nПроизводится откат до первоначальных позиций!", replyMarkup: Keyboard.Waiting);
                        botClient.SendTextMessageAsync(GetOpponentByUserId(userId).Id, $"😣Ай! Твой оппонент крепко стукнул по твоему бойцу! У тебя отнимается одна жизнь! В запасе осталось ещё {GetOpponentByUserId(userId).Hp - 1}!\nПроизводится откат до первоначальных позиций!", replyMarkup: Keyboard.Waiting);
                        botClient.SendTextMessageAsync(rootgroup, $"💥Бум! Боец игрока {GetPlayerByUserId(userId).Name} крепко стукнул по питомцу игрока {GetOpponentByUserId(userId).Name}! У него осталось ещё {GetOpponentByUserId(userId).Hp - 1} жизни!\nУ дуэлянтов начинается следующий раунд!");
                        break;
                    case 2:
                        botClient.SendTextMessageAsync(GetPlayerByUserId(userId).Id, $"💥Попадание! Твой боец стукнул по питомцу своего оппонента! У него отнимается 1 жизнь! Осталось ещё {GetOpponentByUserId(userId).Hp - 1} раз(а)!\nПроизводится откат до первоначальных позиций!", replyMarkup: Keyboard.Waiting);
                        botClient.SendTextMessageAsync(GetOpponentByUserId(userId).Id, $"😣Ай! Твой оппонент крепко стукнул по твоему бойцу! У тебя отнимается одна жизнь! В запасе осталось ещё {GetOpponentByUserId(userId).Hp - 1}!\nПроизводится откат до первоначальных позиций!", replyMarkup: Keyboard.Waiting);
                        botClient.SendTextMessageAsync(rootgroup, $"💥Бум! Боец игрока {GetPlayerByUserId(userId).Name} крепко стукнул по питомцу игрока {GetOpponentByUserId(userId).Name}! У него осталось ещё {GetOpponentByUserId(userId).Hp - 1} жизнь!\nУ дуэлянтов начинается следующий раунд!");
                        break;
                }
                var PosReset = $"UPDATE `duels` SET `p1pos`='13', `p2pos`='3' WHERE `p1id` = {userId} OR `p2id = {userId}`;";
                game.p1.Pos = 13;
                game.p2.Pos = 3;
                Game.getSessionByUserId(userId).GetOpponentByUserId(userId).Hp--;
                var HpUpdate = $"UPDATE `duels` SET `p1hp` = {p1.Hp}, `p2hp` = {p2.Hp} WHERE `p1id` = {userId} OR `p2id`  = {userId};";
                database.Read(HpUpdate, "");
                AttackCooldown = new(3000);
                AttackCooldown.Elapsed += (sender, e) => AttackCooldownElapsed(sender, e, userId, game);
                if (AttackCooldown.Enabled)
                {
                    return;
                }
                AttackCooldown.Start();
            }
            else
            {
                EndGame(userId, botClient, game);
            }
        }

        public void CheckBlock(Game game, long userId, ITelegramBotClient botClient)
        {
            int PlayerPos = game.GetPlayerByUserId(userId).Pos;
            int OpponentPos = game.GetOpponentByUserId(userId).Pos;
            if (game.GetOpponentByUserId(userId).PosCooldown != null && PlayerPos == OpponentPos)
            {
                sql database = new("Данные для подключения к SQL");
                botClient.SendTextMessageAsync(userId, "⚔️ Бум! ваши питомцы столкнулись и отразили друг друга! Каждый откатывается до первоначальной позиции!\nОжидай продолжения игры...", replyMarkup: Keyboard.Waiting);
                botClient.SendTextMessageAsync(game.GetOpponentByUserId(userId).Id, "⚔️ Бум! ваши питомцы столкнулись и отразили друг друга! Каждый откатывается до первоначальной позиции!\nОжидай продолжения игры...", replyMarkup: Keyboard.Waiting);
                var PosReset = $"UPDATE `duels` SET `p1pos`='13', `p2pos`='3' WHERE `p1id` = '{userId}' OR `p2id` = '{userId}';";
                game.p1.Pos = 13;
                game.p2.Pos = 3;
                database.Read(PosReset, "");
                BlockCooldown = new(3000);
                BlockCooldown.Elapsed += (sender, e) => BlockCooldownElapsed(sender, e, userId, game);
                if (BlockCooldown.Enabled)
                {
                    return;
                }
                BlockCooldown.Start();
            }
            else
            {
                Game.getSessionByUserId(userId).Attack(Game.getSessionByUserId(userId), userId, botClient);
            }
        }

        public Game GetInstance() => this;

        public Player GetPlayerByUserId(long userId)
        {
            if (p1.Id == userId)
            {
                return p1;
            }
            else
            {
                return p2;
            }
        }

        public Player GetOpponentByUserId(long userId)
        {
            if (p1.Id == userId)
            {
                return p2;
            }
            else
            {
                return p1;
            }
        }

        public static Game getSessionByUserId(long userId)
        {
            foreach(Game game in Lobby.Games)
            {
                if (game.p1.Id == userId || game.p2.Id == userId)
                {
                    return game.GetInstance();
                }
            }
            return null;
        }

        public void EndGame(long userId, ITelegramBotClient botClient, Game game)
        {

            var GetRootgroup = $"SELECT `rootgroup` FROM `duels` WHERE `p1id` = '{userId}' OR `p2id` = '{userId}';";
            List<string> rootgroupStr = database.Read(GetRootgroup, "rootgroup");
            long rootgroup = long.Parse(rootgroupStr[0]);
            var GetIsFriendly = $"SELECT `IsFriendly` FROM `duels` WHERE `p1id` = '{userId}' OR `p2id` = '{userId}';";
            List<string> IsFriendlyStr = database.Read(GetIsFriendly, "IsFriendly");
            string IsFriendly = IsFriendlyStr.FirstOrDefault();

            var Winner = GetPlayerByUserId(userId);
            var Loser = GetOpponentByUserId(userId);
            ReplyKeyboardMarkup Menu = new(new[]
                { 
                    new[] 
                    { 
                        new KeyboardButton("Вызовы от игроков") 
                    } 
                })
            { ResizeKeyboard = true };
            if (IsFriendly == "False")
            {
                botClient.SendTextMessageAsync(Winner.Id, "😎Поздравляю с победой!\nПитомец твоего оппонента не выдержал под натиском твоего и был разгромлен!\nТеперь он теряет 10 единиц размера, которые переходят к твоему питомцу!", replyMarkup: Menu);
                botClient.SendTextMessageAsync(Loser.Id, "😵Вот чёрт! Ты проиграл!\nТвой питомец не выдержал под натиском вражеского и был разгромлен!\nТы теряешь 10 единиц размера, которые теперь переходят твоему оппоненту...\nНе отчаивайся! Может повезёт в следующий раз!", replyMarkup: Menu);
                botClient.SendTextMessageAsync(rootgroup, $"Внимание! Бой между {Winner.Name} и {Loser.Name} закончен!\n" +
                    $"🏆 Победитель: {Winner.Name}\n" +
                    $"🩸 Проигравший: {Loser.Name}\n\n" +
                    $"У проигравшего питомца отнимается 10 единиц размера, которые переходят победителю!");
                var GameDelete = $"DELETE FROM `duels` WHERE `p1id` = '{userId}' OR `p2id` = '{userId}';";
                database.Read(GameDelete, "");
            }
            else
            {
                botClient.SendTextMessageAsync(Winner.Id, "😎Поздравляю с победой!\nПитомец твоего оппонента уже устал!\nВам стоит прекратить бой, чтобы никто не пострадал!", replyMarkup: Menu);
                botClient.SendTextMessageAsync(Loser.Id, "😵Вот блин! Ты проиграл!\nТвой питомец уже устал от боя!\nНаверное ему стоит отдохнуть и прекратить бой...\nНе отчаивайся! Может повезёт в следующий раз!", replyMarkup: Menu);
                botClient.SendTextMessageAsync(rootgroup, $"Внимание! Бой между {Winner.Name} и {Loser.Name} закончен!\n" +
                    $"🏆 Победитель: {Winner.Name}\n" +
                    $"🩸 Проигравший: {Loser.Name}\n\n" +
                    $"🤝 Дуэль была дружеская, никто не теряет размер и все остаются довольны!");
                var GameDelete = $"DELETE FROM `duels` WHERE `p1id` = '{userId}' OR `p2id` = '{userId}';";
                database.Read(GameDelete, "");
            }
            game = null;
        }

        public void EndGameCauseBlocked(long userId, ITelegramBotClient botClient, Game game)
        {
            var GetRootgroup = $"SELECT `rootgroup` FROM `duels` WHERE `p1id` = '{userId}' OR `p2id` = '{userId}';";
            List<string> rootgroupStr = database.Read(GetRootgroup, "rootgroup");
            long rootgroup = long.Parse(rootgroupStr[0]);
            var GetIsFriendly = $"SELECT `IsFriendly` FROM `duels` WHERE `p1id` = '{userId}' OR `p2id` = '{userId}';";
            List<string> IsFriendlyStr = database.Read(GetIsFriendly, "IsFriendly");
            string IsFriendly = IsFriendlyStr.FirstOrDefault();

            var Winner = GetOpponentByUserId(userId);
            var Loser = GetPlayerByUserId(userId);
            ReplyKeyboardMarkup Menu = new(new[]
                {
                    new[]
                    {
                        new KeyboardButton("Вызовы от игроков")
                    }
                })
            { ResizeKeyboard = true };
            if (IsFriendly == "False")
            {
                botClient.SendTextMessageAsync(Winner.Id, "😎Поздравляю с победой!\nОппонент не выдержал под натиском твоего питомца и заблокировал меня!\nТеперь его питомец теряет 10 единиц размера, которые переходят к тебе!", replyMarkup: Menu);
                botClient.SendTextMessageAsync(rootgroup, $"Внимание! Бой между {Winner.Name} и {Loser.Name} закончен!\n" +
                    $"{Loser.Name} заблокировал меня, поэтому он считается проигравшим!\n" +
                    $"🏆 Победитель: {Winner.Name}\n" +
                    $"🩸 Проигравший: {Loser.Name}\n\n" +
                    $"У проигравшего отнимается 10 единиц размера, которые переходят победителю!");
                var GameDelete = $"DELETE FROM `duels` WHERE `p1id` = '{userId}' OR `p2id` = '{userId}';";
                database.Read(GameDelete, "");
            }
            else
            {
                botClient.SendTextMessageAsync(Winner.Id, "😎Поздравляю с победой!\nТвой оппонент заблокировал меня!\nДуэль является дружеской, поэтому никто ничего не теряет!", replyMarkup: Menu);
                botClient.SendTextMessageAsync(rootgroup, $"Внимание! Бой между {Winner.Name} и {Loser.Name} закончен!\n" +
                    $"{Loser.Name} заблокировал меня, поэтому он считается проигравшим!\n" +
                    $"🏆 Победитель: {Winner.Name}\n" +
                    $"🩸 Проигравший: {Loser.Name}\n\n" +
                    $"🤝 Дуэль была дружеская, никто не теряет свой размер и все остаются довольны!");
                var GameDelete = $"DELETE FROM `duels` WHERE `p1id` = '{userId}' OR `p2id` = '{userId}';";
                database.Read(GameDelete, "");
            }
            game = null;
        }

        public void AttackCooldownElapsed(object sender, ElapsedEventArgs e, long userId, Game game)
        {
            AttackCooldown.Stop();
            AttackCooldown.Dispose();
            ITelegramBotClient botClient = new TelegramBotClient("Ключ Telegram-бота");
            sql database = new("Данные для подключения к SQL");

            var p1MoveReset = $"UPDATE `duels` SET `p1IsMoved`='0' WHERE `p1id` = {userId};";
            var p2MoveReset = $"UPDATE `duels` SET `p2IsMoved`='0' WHERE `p2id` = {userId};";
            if (userId == game.p1.Id)
                database.Read(p1MoveReset, "");
            else if (userId == game.p2.Id)
                database.Read(p2MoveReset, "");

            botClient.SendTextMessageAsync(p1.Id, "Игра продолжается! Ты снова можешь сделать ход!", replyMarkup: Keyboard.LeftDown);
            botClient.SendTextMessageAsync(p2.Id, "Игра продолжается! Ты снова можешь сделать ход!", replyMarkup: Keyboard.RightUp);
        }

        public void BlockCooldownElapsed(object sender, ElapsedEventArgs e, long userId, Game game)
        {
            BlockCooldown.Stop();
            BlockCooldown.Dispose();
            ITelegramBotClient botClient = new TelegramBotClient("Ключ Telegram-бота");
            sql database = new("Данные для подключения к SQL");

            var p1MoveReset = $"UPDATE `duels` SET `p1IsMoved`='0' WHERE `p1id` = {userId};";
            var p2MoveReset = $"UPDATE `duels` SET `p2IsMoved`='0' WHERE `p2id` = {userId};";
            if (userId == game.p1.Id)
                database.Read(p1MoveReset, "");
            else if (userId == game.p2.Id)
                database.Read(p2MoveReset, "");

            botClient.SendTextMessageAsync(game.p1.Id, "Игра продолжается! Ты снова можешь сделать ход!", replyMarkup: Keyboard.LeftDown);
            botClient.SendTextMessageAsync(game.p2.Id, "Игра продолжается! Ты снова можешь сделать ход!", replyMarkup: Keyboard.RightUp);
        }
    }
}
