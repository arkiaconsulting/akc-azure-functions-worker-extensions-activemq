using System.Text.Json;

namespace Akc.Azure.WebJobs.Extensions.ActiveMQ.Config
{
    internal static class Constants
    {
        public static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    }
}
