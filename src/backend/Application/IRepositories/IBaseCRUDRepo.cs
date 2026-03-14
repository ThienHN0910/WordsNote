namespace Application.IRepositories;

public interface IBaseCRUDRepo<T, TKey> where T : class
{
    Task<T> CreateAsync(T entity);
    Task<T> GetByIdAsync(TKey entityId);
    Task<IEnumerable<T>> GetAllAsync(int page, int limit);
    Task<T> UpdateAsync(TKey entityId, T entityUpdated);
    Task<bool> DeleteAsync(TKey entityId);
}
