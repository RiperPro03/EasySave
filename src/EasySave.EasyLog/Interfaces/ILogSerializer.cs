namespace EasySave.EasyLog.Interfaces
{
    /// <summary>
    /// Serializes log entries into a text format.
    /// </summary>
    public interface ILogSerializer
    {
        /// <summary>
        /// Serializes an entry to text.
        /// </summary>
        /// <param name="entry">The entry to serialize.</param>
        /// <returns>The serialized entry as text.</returns>
        string Serialize(object entry);

        /// <summary>
        /// Gets the file extension for the serialized format.
        /// </summary>
        string FileExtension { get; }
    }
}
