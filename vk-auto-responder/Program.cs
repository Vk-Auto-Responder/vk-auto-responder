using System;
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

        private static readonly long[] ChatIds =
        {
            // 2000000000 + 2, // Family
            2000000000 + 58, // 702БЛ
            2000000000 + 59, // семёрка
            2000000000 + 81, // комплекс общежитий №3
        };

        private const long UserId = 386787504;

        // USE: https://vkhost.github.io/
        // Allow only messages
        // App ID: 6121396

        private const string Token = "720edbeae2719b295fc17e615287fdc033423a4ce5465be9c72d8539f935d75dea3d4cf0d4f9e15032850";

        private static readonly HashSet<long> VisitedIds = new();
        private const string VisitedIdsFileName = "visitedids.txt";

        private static readonly string[] BannedToAllKeywords =
        {
            "1108",
            "1108м"
        };

        private static readonly string[] Keywords =
        {
            "1108",
            "1108м",
            "копию",
            "копия",
            "напечатать",
            "откопировать",
            "отксерит",
            "отксерить",
            "отсканить",
            "печатает",
            "печатаете",
            "печатать",
            "печать",
            "пидор",
            "принтер",
            "распечатать",
            "секс",
            "скан",
            "сканер",
            "скопировать",
        };

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

            do
            {
                foreach (var chatId in ChatIds)
                {
                    var history = API.Messages.GetHistory(new MessagesGetHistoryParams
                    {
                        UserId = chatId,
                        Count = 5,
                        Extended = true
                    });

                    var messages = history.Messages.ToCollection();

                    // Console.WriteLine($"Loaded {messages.Count} messages in {chatId} chat");

                    foreach (var message in messages)
                    {
                        if (message.Id is null) continue;

                        if (!NoticeMessageId(message.Id.Value))
                        {
                            continue;
                        }

                        if (message.FromId == UserId)
                        {
                            Console.WriteLine($"Message from self, skipping! - {message.Text}");
                            continue;
                        }

                        if (message.Date is { } date && date < DateTime.Now.AddMinutes(-20))
                        {
                            Console.WriteLine("Detected message over 20 minutes old");
                            continue;
                        }

                        string text;
                        if (message.ForwardedMessages.Count == 0)
                        {
                            text = message.Text;
                            Console.WriteLine($"New Message {message.Id} - {text}");
                        }
                        else
                        {
                            text = message.ForwardedMessages[0].Text;
                            Console.WriteLine($"New Forwarded Message In {message.Id} - {text}");
                        }

                        var words = string.Create(text.Length, text, (span, s) =>
                            {
                                for (var i = 0; i < s.Length; i++)
                                {
                                    if (char.IsLetter(s[i]))
                                    {
                                        span[i] = s[i];
                                    }
                                    else
                                    {
                                        span[i] = ' ';
                                    }
                                }
                            })
                            .Replace(".", " ")
                            .Replace("?", " ")
                            .Replace("!", " ")
                            .Replace(",", " ")
                            .Replace("\\n", " ")
                            .Replace("\n", " ")
                            .Replace("#", " ")
                            .Replace("'", " ")
                            .Replace("(", " ")
                            .Replace(")", " ")
                            .Replace("/", " ")
                            .Replace("-", " ")
                            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                            .Select(w => w.ToLower())
                            .ToCollection();

                        var index = -1;
                        if (words.Any(word => (index = Array.IndexOf(BannedToAllKeywords, word)) != -1))
                        {
                            Console.WriteLine($"Banned keyword detected - {BannedToAllKeywords[index]}");
                            // Reply("@all", chatId, message.Id.Value);
                        }

                        if (words.Any(word => (index = Array.IndexOf(Keywords, word)) != -1))
                        {
                            Console.WriteLine($"Keyword detected - {Keywords[index]}");
                            Reply("🔥702БЛ🔥\n✨Печать (чб и цветная) - 4р/лист✨\n✨Скан - 2р/лист✨", chatId, message.Id.Value);
                        }
                    }

                    Thread.Sleep(2000);
                }
            } while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Q));
        }
    }
}