using Dekauto.Students.Service.Students.Service.Domain.Entities;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Dekauto.Students.Service.Students.Service.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Dekauto.groups.Service.groups.Service.Infrastructure
{
    public class GroupsRepository : IGroupsRepository
    {
        private readonly DekautoContext _context;
        public GroupsRepository(DekautoContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Group group)
        {
            await _context.Groups.AddAsync(group);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid groupId)
        {
            _context.Groups.Remove(await GetByIdAsync(groupId));
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Group>> GetAllAsync()
        {
            return await _context.Groups.ToListAsync();
        }

        public async Task<Group> GetByIdAsync(Guid groupId)
        {
            return await _context.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
        }

        public async Task UpdateAsync(Group updatedGroup)
        {
            var currentGroup = await _context.Groups.FirstOrDefaultAsync(s => s.Id == updatedGroup.Id);
            if (currentGroup == null) throw new KeyNotFoundException($"Group {updatedGroup.Id} not found");

            _context.Entry(currentGroup).CurrentValues.SetValues(updatedGroup);
            await _context.SaveChangesAsync();
        }
    }
}
