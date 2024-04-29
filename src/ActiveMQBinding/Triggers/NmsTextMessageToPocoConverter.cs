using Apache.NMS.AMQP.Message;
using Microsoft.Azure.WebJobs;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ActiveMQBinding.Triggers
{
    internal sealed class NmsTextMessageToPocoConverter<TElement> : IAsyncConverter<NmsTextMessage, TElement>
    {
        public async Task<TElement> ConvertAsync(NmsTextMessage input, CancellationToken cancellationToken)
        {
            var json = input.Text;

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                return await JsonSerializer.DeserializeAsync<TElement>(ms, cancellationToken: cancellationToken);
            }
        }
    }
}
