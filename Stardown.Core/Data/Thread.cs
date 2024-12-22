namespace Stardown.Core.Data;

public class Thread
{
    public Guid Uuid { get; set; }
    public string Name { get; set; }

    public Thread(Guid uuid, string name)
    {
        Uuid = uuid;
        Name = name;
    }
}
