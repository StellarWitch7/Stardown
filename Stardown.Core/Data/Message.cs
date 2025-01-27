using PeterO.Cbor;

namespace Stardown.Core.Data;

public sealed class Message
{
    public Guid Uuid { get; set; }
    public Guid ThreadUuid { get; set; }
    public Guid SenderUuid { get; set; }
    public Guid? ReplyMessageUuid { get; set; }
    public long SentTime { get; set; }
    public string Contents { get; set; }

    public Message(Guid uuid, Guid threadUuid, Guid senderUuid, Guid? replyMessageUuid, long sentTime, string contents)
    {
        Uuid = uuid;
        ThreadUuid = threadUuid;
        SenderUuid = senderUuid;
        ReplyMessageUuid = replyMessageUuid;
        SentTime = sentTime;
        Contents = contents;
    }

    public Message(CBORObject obj)
    {
        Uuid = (Guid) obj["uuid"].ToObject(typeof(Guid));
        ThreadUuid = (Guid) obj["thread_uuid"].ToObject(typeof(Guid));
        SenderUuid = (Guid) obj["sender_uuid"].ToObject(typeof(Guid));
        ReplyMessageUuid = (Guid?) obj.GetOrDefault("reply_message_uuid", null)?.ToObject(typeof(Guid));
        SentTime = obj["sent_time"].AsNumber().ToInt64Checked();
        Contents = obj["contents"].AsString();
    }
}
