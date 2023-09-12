using PetBotCs.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace PetBotCs.Game
{
    public class Keyboard
    {
        public static ReplyKeyboardRemove Remove = new();

        public static ReplyKeyboardMarkup Waiting = new(new[]
            {
                new[]
                {
                    new KeyboardButton("⛔️ ОЖИДАЙТЕ ХОД ⛔️")
                },
                new[]
                {
                    new KeyboardButton("🕑"),
                    new KeyboardButton("🕑")
                },
                new[]
                {
                    new KeyboardButton("🕑")
                }
            })
        { ResizeKeyboard = true };

        public static ReplyKeyboardMarkup Menu = new(new[]
            { 
                new[]
                {
                    new KeyboardButton("Вызовы от игроков") 
                } 
            })
        { ResizeKeyboard = true };

        public static ReplyKeyboardMarkup Left = new(new[]
            {
                new[]
                {
                    new KeyboardButton("⬆️ шаг вверх")
                },
                new[]
                {
                    new KeyboardButton("⛔️ шаг влево"),
                    new KeyboardButton("➡️ шаг вправо")
                },
                new[]
                {
                    new KeyboardButton("⬇️ шаг вниз")
                }
            })
        { ResizeKeyboard = true };


        public static ReplyKeyboardMarkup Up = new(new[]
            {
                new[]
                {
                    new KeyboardButton("⛔️ шаг вверх")
                },
                new[]
                {
                    new KeyboardButton("⬅️ шаг влево"),
                    new KeyboardButton("➡️ шаг вправо")
                },
                new[]
                {
                    new KeyboardButton("⬇️ шаг вниз")
                }
            })
        { ResizeKeyboard = true };


        public static ReplyKeyboardMarkup Right = new(new[]
            {
                new[]
                {
                    new KeyboardButton("⬆️ шаг вверх")
                },
                new[]
                {
                    new KeyboardButton("⬅️ шаг влево"),
                    new KeyboardButton("⛔️ шаг вправо")
                },
                new[]
                {
                    new KeyboardButton("⬇️ шаг вниз")
                }
            })
        { ResizeKeyboard = true };


        public static ReplyKeyboardMarkup Down = new(new[]
            {
                new[]
                {
                    new KeyboardButton("⬆️ шаг вверх")
                },
                new[]
                {
                    new KeyboardButton("⬅️ шаг влево"),
                    new KeyboardButton("➡️ шаг вправо")
                },
                new[]
                {
                    new KeyboardButton("⛔️ шаг вниз")
                }
            })
        { ResizeKeyboard = true };



        public static ReplyKeyboardMarkup LeftUp = new(new[]
            {
                new[]
                {
                    new KeyboardButton("⛔️ шаг вверх")
                },
                new[]
                {
                    new KeyboardButton("⛔️ шаг влево"),
                    new KeyboardButton("➡️ шаг вправо")
                },
                new[]
                {
                    new KeyboardButton("⬇️ шаг вниз")
                }
            })
        { ResizeKeyboard = true };


        public static ReplyKeyboardMarkup RightUp = new(new[]
            {
                new[]
                {
                    new KeyboardButton("⛔️ шаг вверх")
                },
                new[]
                {
                    new KeyboardButton("⬅️ шаг влево"),
                    new KeyboardButton("⛔️ шаг вправо")
                },
                new[]
                {
                    new KeyboardButton("⬇️ шаг вниз")
                }
            })
        { ResizeKeyboard = true };


        public static ReplyKeyboardMarkup RightDown = new(new[]
            {
                new[]
                {
                    new KeyboardButton("⬆️ шаг вверх")
                },
                new[]
                {
                    new KeyboardButton("⬅️ шаг влево"),
                    new KeyboardButton("⛔️ шаг вправо")
                },
                new[]
                {
                    new KeyboardButton("⛔️ шаг вниз")
                }
            })
        { ResizeKeyboard = true };


        public static ReplyKeyboardMarkup LeftDown = new(new[]
            {
                new[]
                {
                    new KeyboardButton("⬆️ шаг вверх")
                },
                new[]
                {
                    new KeyboardButton("⛔️ шаг влево"),
                    new KeyboardButton("➡️ шаг вправо")
                },
                new[]
                {
                    new KeyboardButton("⛔️ шаг вниз")
                }
            })
        { ResizeKeyboard = true };

        public static ReplyKeyboardMarkup Standard = new(new[]
            {
                new[]
                {
                    new KeyboardButton("⬆️ шаг вверх")
                },
                new[]
                {
                    new KeyboardButton("⬅️ шаг влево"),
                    new KeyboardButton("➡️ шаг вправо")
                },
                new[]
                {
                    new KeyboardButton("⬇️ шаг вниз")
                }
            })
        { ResizeKeyboard = true };
    }
    
}


//⬆️ ⬅️ ➡️ ⬇️ ⛔️ \\