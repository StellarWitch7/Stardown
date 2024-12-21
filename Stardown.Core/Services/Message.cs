namespace Stardown.Core.Services
{
    internal class Message
    {
        public int Id { get; set; }

        public required string Text
        {
            get;
            set;
        }
    }
}
