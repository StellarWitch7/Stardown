using PeterO.Cbor;

namespace Stardown.Core.Data;

public sealed class Thread
{
    public Guid Uuid { get; set; }
    public string Name { get; set; }

    public Thread(Guid uuid, string name)
    {
        Uuid = uuid;
        Name = name;
    }

    public Thread(CBORObject obj)
    {
        Uuid = (Guid) obj["uuid"].ToObject(typeof(Guid));
        Name = obj["name"].AsString();
    }
}
