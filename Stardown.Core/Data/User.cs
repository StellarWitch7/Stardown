using PeterO.Cbor;

namespace Stardown.Core.Data;

public sealed class User
{
    public Guid Uuid { get; set; }
    public string Name { get; set; }

    public User(Guid uuid, string name)
    {
        Uuid = uuid;
        Name = name;
    }

    public User(CBORObject obj)
    {
        Uuid = (Guid) obj["uuid"].ToObject(typeof(Guid));
        Name = obj["name"].AsString();
    }
}
