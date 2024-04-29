using Apache.NMS;

namespace ActiveMQBinding.Tests.Assets;

internal class TextMessage : ITextMessage
{
    public string Text { get; set; } = default!;
    public IPrimitiveMap Properties { get; } = default!;
    public string NMSCorrelationID { get; set; } = default!;
    public IDestination NMSDestination { get; set; } = default!;
    public TimeSpan NMSTimeToLive { get; set; }
    public string NMSMessageId { get; set; } = default!;
    public MsgDeliveryMode NMSDeliveryMode { get; set; }
    public MsgPriority NMSPriority { get; set; }
    public bool NMSRedelivered { get; set; }
    public IDestination NMSReplyTo { get; set; } = default!;
    public DateTime NMSTimestamp { get; set; }
    public string NMSType { get; set; } = default!;
    public DateTime NMSDeliveryTime { get; set; }

    public void Acknowledge() => throw new NotImplementedException();
    public Task AcknowledgeAsync() => throw new NotImplementedException();
    public T Body<T>() => throw new NotImplementedException();
    public void ClearBody() => throw new NotImplementedException();
    public void ClearProperties() => throw new NotImplementedException();
    public bool IsBodyAssignableTo(Type type) => throw new NotImplementedException();
}
