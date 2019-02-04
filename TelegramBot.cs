using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramDaletNotificationBot {
    public class TelegramBot : IDisposable {          
        private const String COMMAND_HELP = "help";
        private const String COMMAND_START = "start";
        private const String COMMAND_SUBSCRIBE = "subscribe";
        private const String COMMAND_SUBSCRIPTIONS = "subscriptions";
        private readonly String[] SUPPORTED_SUBSCRIBE_TYPES = { "job", "title" };

        private TelegramBotClient client;
        private Dictionary<long, InputMessage> stateDb;

        public delegate void OnSubscribeDelegate(InputMessage message);
        public event OnSubscribeDelegate OnSubscribe;

        public TelegramBot(String token) {
            stateDb = new Dictionary<long, InputMessage>();
            client = new TelegramBotClient(token);
            client.OnMessage += OnMessage;
            client.StartReceiving();
        }       

        private void ListetingLoop() {
            client.StartReceiving();
        }

        public void Dispose() {
            client.StopReceiving();
        }

        private void OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e) {
            try {
                var message = e.Message;
                var user = message.From;
                var chat = message.Chat;
                if (message.Type == Telegram.Bot.Types.Enums.MessageType.Text) {
                    var text = message.Text;
                    if (text.StartsWith("/")) {
                        var command = text.Substring(1);
                        OnCommand(chat, user, command);
                    }
                    else {
                        OnParams(chat, user, text);
                    }
                }
            }
            catch(Exception ex) {
                Console.Error.WriteLine(ex.ToString());
            }
        }

        private void OnCommand(Chat chat, User user, String command) {
            switch(command) {
                case COMMAND_HELP:
                    SendHelpMessage(chat, user);
                    break;

                case COMMAND_START:
                    SendHelpMessage(chat, user);
                    break;

                case COMMAND_SUBSCRIBE:
                    SubscribeCommand(chat, user);
                    break;

                case COMMAND_SUBSCRIPTIONS:
                    SubscriptionsCommand(chat, user);
                    break;

                default:
                    BadCommand(chat, user);
                    break;
            }
        }

        private void OnParams(Chat chat, User user, String param) {
            InputMessage message;
            if (stateDb.TryGetValue(chat.Id, out message)) {
                switch (message.action) {
                    case COMMAND_SUBSCRIBE:
                        SubscribeCommandParams(chat, user, message, param);
                        break;

                    default:
                        BadCommand(chat, user);
                        break;
                }
            }
            else {
                SendHelpMessage(chat, user);
            }
        }

        private void SendHelpMessage(Chat chat, User user) {
            DisagreeCurrentCommand(chat);
            var message = new StringBuilder();
            message.AppendLine($"Hello {user.FirstName} {user.LastName}!");
            message.AppendLine($"Welcome to Dalet notifier bot!");
            message.AppendLine();
            message.AppendLine("Commands:");
            message.AppendLine($"/{COMMAND_SUBSCRIBE} - Subscribe to notification");
            message.AppendLine($"/{COMMAND_SUBSCRIPTIONS} - Show your subscriptions");
            message.AppendLine($"/{COMMAND_HELP} - Show this message");
            SendText(chat, message.ToString());
        }

        private void SubscribeCommand(Chat chat, User user) {
            DisagreeCurrentCommand(chat);
            stateDb.Add(chat.Id, new InputMessage() {
                user_id = chat.Id.ToString(),
                action = COMMAND_SUBSCRIBE,
                @params = new List<string>()
            });
            SendText(chat, 
                "Please input type of notification." + Environment.NewLine +
                "Supported types: " + String.Join(',', SUPPORTED_SUBSCRIBE_TYPES) 
            );
        }

        private void SubscribeCommandParams(Chat chat, User user, InputMessage message, String param) {
            if (message.@params.Count == 0) {
                var formatedParam = SUPPORTED_SUBSCRIBE_TYPES.FirstOrDefault(x => x.Equals(param, StringComparison.InvariantCultureIgnoreCase));
                if (formatedParam != null) {
                    message.@params.Add(formatedParam);
                    SendText(chat, $"Please input {formatedParam} id.");
                    return;
                }
            }
            else if (message.@params.Count == 1) {
                long id;
                if (long.TryParse(param, out id)) {
                    message.@params.Add(id.ToString());
                    DisagreeCurrentCommand(chat);
                    OnSubscribe?.Invoke(message);
                    SendText(chat, $"You are successfully subscribed to {message.@params[0]} with ID {message.@params[1]}.");
                    return;
                }
            }

            DisagreeCurrentCommand(chat);
            SendText(chat, $"Invalid parameter.{Environment.NewLine}Please try execute '/{message.action}' again.");
        }

        private void SubscriptionsCommand(Chat chat, User user) {
            DisagreeCurrentCommand(chat);
            SendText(chat, "Sorry, but this command not implemented!");
        }

        private void DisagreeCurrentCommand(Chat chat) {
            stateDb.Remove(chat.Id);
        }

        private void BadCommand(Chat chat, User user) {
            DisagreeCurrentCommand(chat);
            SendText(chat, $"Unknown command!{Environment.NewLine}Use '/{COMMAND_HELP}' command for show commands list.");
        }

        public void OnNotification(OutputMessage message) {
            client.SendTextMessageAsync(long.Parse(message.user_id), message.message);
        }

        private void SendText(Chat chat, String text) {
            client.SendTextMessageAsync(chat.Id, text);
        }
    }
}
