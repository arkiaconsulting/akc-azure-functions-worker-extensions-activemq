﻿using Apache.NMS;

namespace ActiveMQBinding.Context
{
    internal class ActiveMQTriggerContext
    {
        public IConnection Connection { get; set; }

        public ActiveMQTriggerAttribute ActiveMQTriggerAttribute { get; set; }

        public ActiveMQTriggerContext(
            IConnection connection,
            ActiveMQTriggerAttribute activeMQTriggerAttribute)
        {
            Connection = connection;
            ActiveMQTriggerAttribute = activeMQTriggerAttribute;
        }
    }
}