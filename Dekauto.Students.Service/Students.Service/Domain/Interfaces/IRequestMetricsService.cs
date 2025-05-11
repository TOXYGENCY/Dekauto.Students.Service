namespace Dekauto.Students.Service.Students.Service.Domain.Interfaces
{
    public interface IRequestMetricsService
    {
        void Increment();
        List<int> GetRecentCounters();
    }
}
