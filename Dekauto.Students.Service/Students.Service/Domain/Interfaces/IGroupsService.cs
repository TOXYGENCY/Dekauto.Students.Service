using Dekauto.Students.Service.Students.Service.Domain.Entities;
using Dekauto.Students.Service.Students.Service.Domain.Entities.DTO;

namespace Dekauto.Students.Service.Students.Service.Domain.Interfaces
{
    public interface IGroupsService : IDtoConverter<Group, GroupDto>
    {
        Task AddAsync(GroupDto groupDto);
        Task DeleteByIdAsync(Guid groupId);
        Task UpdateAsync(Guid groupId, GroupDto updatedGroupDto);
        Task<IEnumerable<Student>> GetAllStudentsForGroupsAsync(IEnumerable<string> groupNames);

    }
}
