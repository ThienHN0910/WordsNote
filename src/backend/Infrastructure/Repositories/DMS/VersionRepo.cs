using Application.IRepositories.DMS;
using Domain.Entities.DMS;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.DMS
{
    public class VersionRepo : IVersionRepo
    {
        private readonly AppDbContext _context;

        public VersionRepo(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Guid> AddNewVersion(Guid docId, int versionNumber, string filePath)
        {
            var docVersion = new DocVersion
            {
                VersionId = new Guid(),
                VersionNumber = versionNumber,
                DocumentId = docId,
                UpdatedAt = DateTime.UtcNow,
                FilePath = filePath,
            };
            try
            {

            _context.Versions.Add(docVersion);
            await _context.SaveChangesAsync();
            }catch(Exception e)
            {
                Console.WriteLine(e);
            }
            return docVersion.VersionId;
        }

        public async Task<List<DocVersion>> GetAllVersionByDocumentId(Guid docId)
        {
            return await _context.Versions
                 .Where(v => v.DocumentId == docId)
                 .ToListAsync();
        }
    }
}
