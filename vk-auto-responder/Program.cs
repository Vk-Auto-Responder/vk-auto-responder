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
        private const string SettingsFilePath = "secret/settings.json";

        private static readonly VkApi API = new();

        private static readonly Random Random = new();

        private static readonly HashSet<long> VisitedIds = new();
        
        private const string VisitedIdsFileName = "visitedids.txt";

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
            foreach (var visitedId in visitedIds!)
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
            if (!File.Exists(SettingsFilePath))
            {
                Console.WriteLine($"File '{SettingsFilePath}' was not found, please ensure it exists");
                Console.ReadKey();
                return;
            }
            
            var settingsJson = File.ReadAllText(SettingsFilePath);

            var settings = JsonConvert.DeserializeObject<Settings>(settingsJson);

            Console.WriteLine($"Parsed settings:\n{settings}");

            if (settings == null)
            {
                return;
            }

            API.Authorize(new ApiAuthParams
            {
                ApplicationId = settings.AuthParams.AppId,
                Login = settings.AuthParams.Login,
                Password = settings.AuthParams.Password,
                TwoFactorAuthorization = () =>
                {
                    Console.Write("Enter confirmation code: ");
                    return Console.ReadLine();
                }
            });

            var userId = API.UserId!.Value;

            Console.WriteLine("Authorized");

            LoadVisitedIds();

            Console.WriteLine("Loaded VisitedIds");

            do
            {
                foreach (var chatId in settings.ChatIds)
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

                        if (message.FromId == userId)
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
                                    if (char.IsLetterOrDigit(s[i]))
                                    {
                                        span[i] = s[i];
                                    }
                                    else
                                    {
                                        span[i] = ' ';
                                    }
                                }
                            })
                            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                            .Select(w => w.ToLower())
                            .ToCollection();

                        var index = -1;
                        if (words.Any(word => (index = settings.BannedToAllKeywords.IndexOf(word)) != -1))
                        {
                            Console.WriteLine($"Banned keyword detected - {settings.BannedToAllKeywords[index]}");
                            // Reply("@all", chatId, message.Id.Value);
                        }

                        if (words.Any(word => (index = settings.Keywords.IndexOf(word)) != -1))
                        {
                            Console.WriteLine($"Keyword detected - {settings.Keywords[index]}");
                            Reply(settings.Reply, chatId, message.Id.Value);
                        }
                    }

                    Thread.Sleep(2000);
                }
            } while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Q));
        }
    }
}