namespace Tekhnologia.Services.Interfaces
{
    public interface IAIService
    {
        Task<string> GetBusinessCoachingResponse(string userMessage);
    }
}
