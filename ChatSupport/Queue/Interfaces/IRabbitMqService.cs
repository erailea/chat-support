using RabbitMQ.Client.Events;

namespace ChatSupport.Queue.Interfaces
{
    public interface IRabbitMqService
    {
        void DeclareQueue(string queueName);
        void PublishMessage(string queueName, string message);
        void AddConsumer(string queueName, EventHandler<BasicDeliverEventArgs> received);
        void RemoveQueue(string queueName);
    }
}