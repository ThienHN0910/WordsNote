using Application.IRepositories.DMS;
using Domain.Entities.DMS;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.DMS
{
    public class CommentRepo : CrudRepository<Comment>, ICommentRepo
    {
        public CommentRepo(AppDbContext context) : base(context) { }

        public async Task<List<Comment>> GetAllCommentByDocumentId(Guid documentId)
        {
            return await _dbSet.Where(c => c.DocumentId == documentId).ToListAsync();
        }
    }
}
