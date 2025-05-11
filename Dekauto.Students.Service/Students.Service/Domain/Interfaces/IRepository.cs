namespace Dekauto.Ts.Service.Ts.Service.Domain.Interfaces
{
    public interface IRepository<T> where T : class
    {
        // Получение всех элементов
        Task<IEnumerable<T>> GetAllAsync();

        // Получение одного элемента по его Id
        Task<T> GetByIdAsync(Guid id);

        // Добавление одного элемента
        Task AddAsync(T obj);

        // Обновление одного элемента
        Task UpdateAsync(T obj);

        // Удаление одного элемента по его Id
        Task DeleteByIdAsync(Guid id);
    }
}
