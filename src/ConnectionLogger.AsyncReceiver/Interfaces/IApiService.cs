namespace ConnectionLogger.AsyncReceiver.Interfaces
{
    public interface IApiService
    {
        Task<string> GetDataFromServer(string pathUrl);
    }
}
