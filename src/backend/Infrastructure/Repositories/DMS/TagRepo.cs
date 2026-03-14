using Application.IRepositories.DMS;
using Domain.Entities.DMS;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories.DMS
{
    public class TagRepo : CrudRepository<Tag>, ITagRepo
    {
        public TagRepo(AppDbContext context) : base(context) { }

        public async Task AddNewTag(string tagName)
        {
            await CreateAsync(new Tag { TagId = Guid.NewGuid(), TagName = tagName });
        }
    }
}
