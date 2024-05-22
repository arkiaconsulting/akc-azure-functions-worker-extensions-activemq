using Akc.Azure.WebJobs.Extensions.ActiveMQ.Config;
using Apache.NMS.AMQP.Message;
using Microsoft.Azure.WebJobs;

namespace Akc.Azure.WebJobs.Extensions.ActiveMQ.Triggers
{
    internal class TextMessageToPocoConverter<TElement> : IConverter<NmsTextMessage, TElement>
    {
        public TElement Convert(NmsTextMessage input)
        {
            var json = input.Text;

            return System.Text.Json.JsonSerializer.Deserialize<TElement>(json, Constants.SerializerOptions);
        }
    }
}
