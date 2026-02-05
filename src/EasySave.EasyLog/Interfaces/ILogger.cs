namespace EasySave.EasyLog.Interfaces
{
    public interface ILogger<T>
    {
        bool Write(T entry);
    }
}
