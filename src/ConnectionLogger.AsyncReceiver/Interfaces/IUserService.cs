namespace ConnectionLogger.AsyncReceiver.Interfaces
{
    public interface IUserService
    {
        Task<string> GetDataFromServer(int id);
    }
}
