using System;
using System.Collections.Generic;
using System.Threading;
using CommandLine;

namespace TelegramDaletNotificationBot {
    class Entrypoint {
        public class Options {
            [Option('t', "telegram-token", Required = true, HelpText = "Token for access to telegram bot.")]
            public String TelegramToken { get; set; }

            [Option('r', "rabbitmq-host", Required = true, HelpText = "Connection string to RebbitMQ server.")]
            public String RabbitMqHost { get; set; }
        }

        static void Main(string[] args) {
            CommandLine.Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(opts => RunOptionsAndReturnExitCode(opts))
                .WithNotParsed<Options>((errs) => HandleParseError(errs));
        }

        static void RunOptionsAndReturnExitCode(Options options) { 
            var telegramBot = new TelegramBot(options.TelegramToken);
            var inputQueue = new InputQueue(options.RabbitMqHost);
            var outputQueue = new OutputQueue(options.RabbitMqHost);

            telegramBot.OnSubscribe += inputQueue.Send;
            outputQueue.OnNotification += telegramBot.OnNotification;

            Console.WriteLine("Telegram bot handler for Dalet notifications running.");
            while (true) {
                Thread.Sleep(1000);
            }
        }

        static void HandleParseError(IEnumerable<Error> errors) { }
    }
}
