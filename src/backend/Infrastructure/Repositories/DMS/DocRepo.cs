using Infrastructure.Persistence;
using Domain.Entities.DMS;
using Application.IRepositories.DMS;

namespace Infrastructure.Repositories.DMS
{
    public class DocRepo : CrudRepository<Document>, IDocRepo
    {
        public DocRepo(AppDbContext context) : base(context) { }

        public async Task<string> UpdateStatus(Guid docId, string status)
        {
            var doc = await GetByIdAsync(docId);
            if (doc == null)
            {
                return "Document not found";
            }
            doc.UploadStatus = status;
            await UpdateAsync(doc);
            return "Status updated";
        }
    }
}
