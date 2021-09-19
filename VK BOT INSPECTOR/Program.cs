using System.Threading;
using System;
using System.Collections.Generic;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Utils;
using VkNet.Model.Keyboard;

namespace VK_BOT_INSPECTOR
{
    class Program
    {
        static List<string> swearing = new List<string>();
        static readonly VkApi api = new VkApi();

        static Random rnd = new Random();

        static System.Collections.ObjectModel.Collection<Message> messages;

        static long confId = 2000000002;

        static long adminId = 386787504;

        static string token = "21cd2cdc7610c2b80b481aa56ed60dd9c815cab2f1232dea703abf91a8d81294a257dec745c37f0106fd9";

        static void Message(string message, long chatId)
        {
            try
            {
                api.Messages.Send(new MessagesSendParams
                {
                    RandomId = rnd.Next(0, 1000000000),
                    PeerId = chatId,
                    Message = message,
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static void Reply(string message, long chatId, long replyingMessageId)
        {
            try
            {
                api.Messages.Send(new MessagesSendParams
                {
                    RandomId = rnd.Next(0, 1000000000),
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

        static void Main(string[] args)
        {
            // Console.Write("ID Беседы: "); confId = Convert.ToInt64(Console.ReadLine());
            // Console.Write("ID Вашей страницы: "); adminId = Convert.ToInt64(Console.ReadLine());
            // Console.Write("Токен: "); token = Console.ReadLine();

            api.Authorize(new ApiAuthParams
            {
                AccessToken = token
            });

            Console.WriteLine("Authorized");

            HashSet<long> visitedIds = new();

            int reload = 0;
            while (true)
            {
                var history = api.Messages.GetHistory(new MessagesGetHistoryParams
                {
                    UserId = confId,
                    Count = 20
                });
                messages = history.Messages.ToCollection();

                Console.WriteLine($"Loaded {messages.Count} messages");

                foreach (var message in messages)
                {
                    if (message.Id is not null)
                    {
                        if (!visitedIds.Add(message.Id.Value))
                        {
                            continue;
                        }
                    }

                    Console.WriteLine(message.Text);
                    // if (message.FromId == adminId) continue;

                    string messageString = message.Text;
                    messageString = messageString.Replace(" ", String.Empty).Replace(".", String.Empty).Replace("\\n", String.Empty).Replace("\n", String.Empty).Replace("#", String.Empty).Replace("'", String.Empty);

                    if (messageString.Contains("Test"))
                    {
                        Reply("OK", confId, message.Id!.Value);
                    }
                }

                Thread.Sleep(10000);
            }
        }
    }
}