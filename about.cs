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
    internal class About
    {
        public static async Task aboutBot(ITelegramBotClient botClient, Update update)
        {
            var message = update.Message;
            await botClient.SendTextMessageAsync(message.Chat, $"Бот '{(await botClient.GetMeAsync()).FirstName}' v1.0\nДата сборки: 07.09.2023\n\nИспользуемые библиотеки:\nTelegram.Bot\nMySql.Connector\n\nРазработчик: @NtRazum\n\nОсобая благодарность Владу за помощь в разработке дуэлей");
        }

        public static async Task UpdateLog(ITelegramBotClient botClient, Update update)
        {
            sql database = new("server=127.0.0.1;uid=phpmyadmin;pwd=oralcumshot;database=phpmyadmin");
            List<string> tableNames = database.GetTableNames();
            foreach (string tableName in tableNames)
            {
                if (tableName.StartsWith("group"))
                {
                    string groupIdString = tableName.Substring("group".Length);
                    if (long.TryParse(groupIdString, out long groupId))
                    {
                        Console.WriteLine(groupId);
                        await botClient.SendTextMessageAsync(groupId, $"Обновление \"{(await botClient.GetMeAsync()).FirstName}\" v1.0" +
                        "\nЧто нового:" +
                        "\n1. Добавлены команды:" +
                        "\n• /dickfight" +
                        "\n• /friendlydickfight" +
                        "\n• /about" +
                        "\n2. Изменена структура поведения бота при добавлении в группу: теперь он не будет покидать другие группы, т.е. каждый может добавить его к себе в группу" +
                        "\n3. Множество косметических дополнений для сообщений от бота" +
                        "\n4. Масштабная оптимизация кода, отлажена работа многих компонентов" +
                        "\n5. Исправление мелких багов" +
                        "\n6. Новые баги");
                    }
                }
                
            }
            var message = update.Message;
            

        }
    }
}
