using Microsoft.Azure.WebJobs.Description;
using System;

namespace ActiveMQBinding
{
    [AttributeUsage(AttributeTargets.Parameter)]
    [Binding]
    public class ActiveMQTriggerAttribute : Attribute
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
