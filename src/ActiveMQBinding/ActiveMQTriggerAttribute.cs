﻿using Microsoft.Azure.WebJobs.Description;
using System;

namespace Akc.Azure.WebJobs.Extensions.ActiveMQ
{
    /// <summary>
    /// ActiveMQ trigger binding attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    [Binding]
    public class ActiveMQTriggerAttribute : Attribute
    {
        /// <summary>
        /// App setting name that contains the ActiveMQ endpoint
        /// </summary>
        [AppSetting]
        public string Connection { get; set; }

        /// <summary>
        /// App setting name that contains the ActiveMQ queue name
        /// </summary>
        public string QueueName { get; private set; }

        /// <summary>
        /// App setting name that contains the ActiveMQ username
        /// </summary>
        [AppSetting]
        public string UserName { get; set; }

        /// <summary>
        /// App setting name that contains the ActiveMQ password
        /// </summary>
        [AppSetting]
        public string Password { get; set; }

        /// <summary>
        /// Initializes a new <see cref="ActiveMQTriggerAttribute"/>.
        /// </summary>
        /// <param name="queueName">The name of the queue to listen to (%...% placeholder supported)</param>
        public ActiveMQTriggerAttribute(string queueName) => QueueName = queueName;
    }
}
