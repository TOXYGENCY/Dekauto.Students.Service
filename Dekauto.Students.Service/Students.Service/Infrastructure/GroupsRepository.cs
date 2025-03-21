using Dekauto.Students.Service.Students.Service.Domain.Entities;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Dekauto.Students.Service.Students.Service.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Dekauto.groups.Service.groups.Service.Infrastructure
{
    public class GroupsRepository : IGroupsRepository
    {
        private readonly DekautoContext context;
        public GroupsRepository(DekautoContext context)
        {
            this.context = context;
        }
        public async Task AddAsync(Group group)
        {
            await context.Groups.AddAsync(group);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid groupId)
        {
            context.Groups.Remove(await GetByIdAsync(groupId));
            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Group>> GetAllAsync()
        {
            return await context.Groups.ToListAsync();
        }

        public async Task<Group> GetByIdAsync(Guid groupId)
        {
            return await context.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
        }

        public async Task UpdateAsync(Group updatedGroup)
        {
            var currentGroup = await context.Groups.FirstOrDefaultAsync(s => s.Id == updatedGroup.Id);
            if (currentGroup == null) throw new KeyNotFoundException($"Group {updatedGroup.Id} not found");

            context.Entry(currentGroup).CurrentValues.SetValues(updatedGroup);
            await context.SaveChangesAsync();
        }
    }
}
