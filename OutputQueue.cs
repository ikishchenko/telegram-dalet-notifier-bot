using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;

namespace TelegramDaletNotificationBot {
    public class OutputQueue : IDisposable {
        public delegate void OnNotificationDelegate(OutputMessage message);
        public event OnNotificationDelegate OnNotification;

        private IConnectionFactory conFactory;
        private IConnection connection;
        private IModel channel;
        private const String EXCHANGE_NAME = "dex";

        public OutputQueue(String connectionString) {
            conFactory = new ConnectionFactory() { Uri = new Uri(connectionString) };
            connection = conFactory.CreateConnection();
            channel = connection.CreateModel();
            //channel.ExchangeDeclare(exchange: EXCHANGE_NAME, type: "direct", durable: true);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += Receive;
            channel.BasicConsume("telegram", true, consumer);
        }

        public void Receive(object obj, BasicDeliverEventArgs args) {
            var jsonData = Encoding.UTF8.GetString(args.Body);
            var jsonObject = JsonConvert.DeserializeObject<OutputMessage>(jsonData);
            OnNotification?.Invoke(jsonObject);
        }

        public void Dispose() {
            channel.Dispose();
            connection.Dispose();
        }
    }
}
