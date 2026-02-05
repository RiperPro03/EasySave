namespace EasySave.EasyLog.Interfaces
{
    public interface ILogSerializer
    {
        string Serialize(object entry);
        string FileExtension { get; }
    }
}
