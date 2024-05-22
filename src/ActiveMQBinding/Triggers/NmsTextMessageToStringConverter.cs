using Apache.NMS.AMQP.Message;
using Microsoft.Azure.WebJobs;

namespace Akc.Azure.WebJobs.Extensions.ActiveMQ.Config
{
    internal class NmsTextMessageToStringConverter : IConverter<NmsTextMessage, string>
    {
        public string Convert(NmsTextMessage input) => input.Text;
    }
}
