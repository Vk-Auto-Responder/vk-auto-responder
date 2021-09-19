﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using VkNet;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Utils;

namespace VkAutoResponder
{
    public static class Program
    {
        private static readonly VkApi API = new();

        private static readonly Random Random = new();

        private const long ChatId = 2000000002;

        private const long UserId = 386787504;

        private const string Token = "21cd2cdc7610c2b80b481aa56ed60dd9c815cab2f1232dea703abf91a8d81294a257dec745c37f0106fd9";

        private static readonly HashSet<long> VisitedIds = new();
        private const string VisitedIdsFileName = "visitedids.txt";
        
        private static readonly string[] Keywords = {"печать", "распечатать", "скан", "печатает", };

        private static void Message(string message, long chatId)
        {
            try
            {
                API.Messages.Send(new MessagesSendParams
                {
                    RandomId = Random.Next(0, 1000000000),
                    PeerId = chatId,
                    Message = message,
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void Reply(string message, long chatId, long replyingMessageId)
        {
            try
            {
                API.Messages.Send(new MessagesSendParams
                {
                    RandomId = Random.Next(0, 1000000000),
                    PeerId = chatId,
                    Message = message,
                    ReplyTo = replyingMessageId
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void LoadVisitedIds()
        {
            if (!File.Exists(VisitedIdsFileName))
            {
                Console.WriteLine($"{VisitedIdsFileName} file not found.");
                return;
            }

            var visitedJson = File.ReadAllText(VisitedIdsFileName);
            var visitedIds = JsonConvert.DeserializeObject<ICollection<long>>(visitedJson);
            foreach (var visitedId in visitedIds)
            {
                VisitedIds.Add(visitedId);
            }
        }

        private static bool NoticeMessageId(long id)
        {
            if (!VisitedIds.Add(id)) return false;

            File.WriteAllText(VisitedIdsFileName, JsonConvert.SerializeObject(VisitedIds.ToArray(), Formatting.Indented));
            return true;
        }

        private static void Main(string[] args)
        {
            // Console.Write("ID Беседы: "); chatId = Convert.ToInt64(Console.ReadLine());
            // Console.Write("ID Вашей страницы: "); UserId = Convert.ToInt64(Console.ReadLine());
            // Console.Write("Токен: "); token = Console.ReadLine();

            API.Authorize(new ApiAuthParams
            {
                AccessToken = Token
            });

            Console.WriteLine("Authorized");

            LoadVisitedIds();

            Console.WriteLine("Loaded VisitedIds");

            while (true)
            {
                var history = API.Messages.GetHistory(new MessagesGetHistoryParams
                {
                    UserId = ChatId,
                    Count = 20
                });

                var messages = history.Messages.ToCollection();

                Console.WriteLine($"Loaded {messages.Count} messages");

                foreach (var message in messages)
                {
                    if (message.Id is null) continue;

                    if (!NoticeMessageId(message.Id.Value))
                    {
                        continue;
                    }

                    Console.WriteLine(message.Text);
                    if (message.FromId == UserId) continue;

                    var words = message.Text
                        .Replace(".", " ")
                        .Replace("\\n", " ")
                        .Replace("\n", " ")
                        .Replace("#", " ")
                        .Replace("'", " ")
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Select(w => w.ToLower());

                    if (words.Any(word => Keywords.Any(kw => kw == word)))
                    {
                        Reply("702БЛ\nПечать (чб и цветная) - 4р/лист\nСкан - 2р/лист", ChatId, message.Id.Value);
                    }
                }

                Thread.Sleep(4000);
            }
        }
    }
}