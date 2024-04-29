using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;
using System;

namespace ActiveMQ.WorkerBinding
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ActiveMQTriggerAttribute : TriggerBindingAttribute
    {
        public string Connection { get; set; }

        public string QueueName { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public ActiveMQTriggerAttribute(string connection, string queueName, string userName, string password)
        {
            Connection = connection;
            QueueName = queueName;
            UserName = userName;
            Password = password;
        }
    }
}
