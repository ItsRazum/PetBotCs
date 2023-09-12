using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using System.Timers;
using System.ComponentModel.DataAnnotations.Schema;
using PetBotCs.Game;

namespace PetBotCs
{
    public class Player
    {
        public System.Timers.Timer Cooldown { get; set; }
        public System.Timers.Timer PosCooldown { get; set; }
        public long Id { get; set; }
        public int Pos { get; set; }
        public int Hp { get; set; }
        public string Name { get; set; }

        public Player(long id, int pos, int hp, string name, System.Timers.Timer timer)
        {
            Cooldown = timer ?? new System.Timers.Timer();
            Id = id;
            Pos = pos;
            Hp = hp;
            Name = name;
        }

        public void MoveTo(Direction direction, Game.Game game, ITelegramBotClient botClient, long userId)
        {
            string[] strings = { "p1IsMoved", "p2IsMoved" };
            sql database = new("Данные для подключения к SQL");
            var IsMovedGet = $"SELECT `p1IsMoved`, `p2IsMoved` FROM `duels` WHERE `p1id` = {Id} OR `p2id` = {Id};";
            List<Dictionary<string, object>> IsMoved = database.ExtRead(IsMovedGet, strings);
            foreach (var result in IsMoved)
            {
                int[] RestrictedPos = new[] { -5, -4, -3, -2, -1, 0, 4, 5, 6, 10, 11, 12, 16, 17, 18, 19, 20, 21 };
                int p1IsMoved = int.Parse(result["p1IsMoved"].ToString());
                int p2IsMoved = int.Parse(result["p2IsMoved"].ToString());

                switch (direction)
                {
                    case Direction.Left:
                        Pos--;
                        break;
                    case Direction.Right:
                        Pos++;
                        break;
                    case Direction.Up:
                        Pos -= 6;
                        break;
                    case Direction.Down:
                        Pos += 6;

                        break;
                }
                var p1Move = $"UPDATE `duels` SET `p1IsMoved`='1', `p1pos`={Pos} WHERE `p1id` = {Id};";
                var p2Move = $"UPDATE `duels` SET `p2IsMoved`='1', `p2pos`={Pos} WHERE `p2id` = {Id};";

                game.GetPlayerByUserId(userId).PosCooldown = new(400);
                game.GetPlayerByUserId(userId).PosCooldown.Elapsed += (sender, e) => PosCooldownElapsed(sender, e, userId, game);
                game.GetPlayerByUserId(userId).PosCooldown.Start();

                if (Id == game.p1.Id && p1IsMoved == 0)
                { database.Read(p1Move, ""); }

                else if (Id == game.p2.Id && p2IsMoved == 0)
                { database.Read(p2Move, ""); }

                else { }

                if (RestrictedPos.Contains(game.GetPlayerByUserId(userId).Pos))
                {
                    var PosReset = $"UPDATE `duels` SET `p1pos`='13', `p2pos`='3' WHERE `p1id` = '{userId}' OR `p2id` = '{userId}';";
                    game.p1.Pos = 13;
                    game.p2.Pos = 3;
                    Console.WriteLine($"{game.p1.Pos} {game.p2.Pos}");
                    database.Read(PosReset, "");
                    botClient.SendTextMessageAsync(userId, "Кажется ты оказался за пределами игрового поля! Оба игрока откатываются до первоначальных позиций!", replyMarkup: Keyboard.Waiting);
                    botClient.SendTextMessageAsync(game.GetOpponentByUserId(userId).Id, "Твой оппонент оказался за пределами игрового поля! Вы оба возвращаетесь на первоначальную позицию!", replyMarkup: Keyboard.Waiting);
                }
            }
        }

        public void SetCooldown(Update update, Game.Game game, long userId)
        {
            var message = update.Message;
            var removeKeyboard = new ReplyKeyboardRemove();
            if (Cooldown.Enabled)
            {
                return;
            }
            Cooldown = new(1000);
            Cooldown.Elapsed += (sender, e) => CooldownElapsed(sender, e, userId, game);
            Cooldown.Start();
        }


        public void PosCooldownElapsed(object sender, ElapsedEventArgs e, long userId, Game.Game game)
        {
            if (game.GetPlayerByUserId(userId).PosCooldown == null) { }
            else
            {
                game.GetPlayerByUserId(userId).PosCooldown.Stop();
                game.GetPlayerByUserId(userId).PosCooldown.Dispose();
                game.GetPlayerByUserId(userId).PosCooldown = null;
            }
            sql database = new("Данные для подключения к SQL");
        }

        public void CooldownElapsed(object sender, ElapsedEventArgs e, long userId, Game.Game game)
        {
            Cooldown.Stop();
            Cooldown.Dispose();
            ITelegramBotClient botClient = new TelegramBotClient("6334305252:AAGz6ldivq79Hzk0iFD1MbxYxoajSJDIdjw");
            sql database = new("Данные для подключения к SQL");
            ReplyKeyboardMarkup keyboard = null;
            if (game.GetPlayerByUserId(userId).Pos.Equals(1))
                keyboard = Keyboard.LeftUp;
            else if (game.GetPlayerByUserId(userId).Pos.Equals(2))
                keyboard = Keyboard.Up;
            else if (game.GetPlayerByUserId(userId).Pos.Equals(3))
                keyboard = Keyboard.RightUp;
            else if (game.GetPlayerByUserId(userId).Pos.Equals(7))
                keyboard = Keyboard.Left;
            else if (game.GetPlayerByUserId(userId).Pos.Equals(8))
                keyboard = Keyboard.Standard;
            else if (game.GetPlayerByUserId(userId).Pos.Equals(9))
                keyboard = Keyboard.Right;
            else if (game.GetPlayerByUserId(userId).Pos.Equals(13))
                keyboard = Keyboard.LeftDown;
            else if (game.GetPlayerByUserId(userId).Pos.Equals(14))
                keyboard = Keyboard.Down;
            else if (game.GetPlayerByUserId(userId).Pos.Equals(15))
                keyboard = Keyboard.RightDown;

            var p1MoveReset = $"UPDATE `duels` SET `p1IsMoved`='0' WHERE `p1id` = {userId};";
            var p2MoveReset = $"UPDATE `duels` SET `p2IsMoved`='0' WHERE `p2id` = {userId};";
            if (userId == game.p1.Id)
                database.Read(p1MoveReset, "");
            else if (userId == game.p2.Id)
                database.Read(p2MoveReset, "");

            botClient.SendTextMessageAsync(userId, "✅Ты снова можешь сделать ход!", replyMarkup: keyboard);
        }
    }
}