namespace Stardown.Core.Data;

public class Message
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
}
