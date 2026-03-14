using Infrastructure.Persistence;
using Domain.Entities.DMS;
using Application.IRepositories.DMS;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.DMS
{
    public class AnnotationRepo : CrudRepository<Annotation>, IAnnotationRepo
    {
        public AnnotationRepo(AppDbContext context) : base(context) { }

        public async Task<List<Annotation>> GetAnnotationsByDocumentIdAndPageNumber(Guid documentId, int pageNum)
        {
            return await _dbSet
                .Where(a => a.DocumentId == documentId && a.PageNumber == pageNum)
                .ToListAsync();
        }
    }
}
