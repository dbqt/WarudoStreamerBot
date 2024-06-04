namespace QTExtensions.StreamerBot
{
    /// <summary>
    /// Generic event handler for StreamerBot
    /// </summary>
    public interface IStreamerBotEventHandler
    {
        void Execute(string obj);
    }
}
