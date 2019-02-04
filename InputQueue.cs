using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;

namespace TelegramDaletNotificationBot {
    public class InputQueue : IDisposable {
        private IConnectionFactory conFactory;
        private IConnection connection;
        private IModel channel;
        private const String EXCHANGE_NAME = "incoming";

        public InputQueue(String connectionString) {
            conFactory = new ConnectionFactory() { Uri = new Uri(connectionString) };
            connection = conFactory.CreateConnection();
            channel = connection.CreateModel();
            channel.ExchangeDeclare(exchange: EXCHANGE_NAME, type: "direct", durable: true);
        }

        public void Send(InputMessage message) {
            var jsonObject = JsonConvert.SerializeObject(message);
            var jsonData = Encoding.UTF8.GetBytes(jsonObject);
            channel.BasicPublish(EXCHANGE_NAME, message.messanger + "-in", null, jsonData);
        }

        public void Dispose() {
            channel.Dispose();
            connection.Dispose();
        }
    }
}
