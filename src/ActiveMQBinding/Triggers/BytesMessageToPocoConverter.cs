using Akc.Azure.WebJobs.Extensions.ActiveMQ.Config;
using Apache.NMS.AMQP.Message;
using Microsoft.Azure.WebJobs;

namespace Akc.Azure.WebJobs.Extensions.ActiveMQ.Triggers
{
    internal class BytesMessageToPocoConverter<TElement> : IConverter<NmsBytesMessage, TElement>
    {
        public TElement Convert(NmsBytesMessage input)
        {
            var json = System.Text.Encoding.UTF8.GetString(input.Content);

            return System.Text.Json.JsonSerializer.Deserialize<TElement>(json, Constants.SerializerOptions);
        }
    }
}
