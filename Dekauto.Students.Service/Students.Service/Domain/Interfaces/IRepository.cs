namespace Dekauto.Ts.Service.Ts.Service.Domain.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();

        Task<T> GetByIdAsync(Guid id);

        Task AddAsync(T obj);

        Task UpdateAsync(Guid id, T obj);

        Task DeleteAsync(T obj);
    }
}
