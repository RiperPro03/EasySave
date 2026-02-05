namespace EasySave.EasyLog.Interfaces
{
    public interface ILogWriter
    {
        bool Write(string filepath, string message);
    }
}
