using Dekauto.Students.Service.Students.Service.Domain.Entities;
using Dekauto.Students.Service.Students.Service.Domain.Entities.DTO;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Dekauto.Students.Service.Students.Service.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Dekauto.Students.Service.Students.Service.Services
{
    public class GroupsService : IGroupsService, IDtoConverter<Group, GroupDto>
    {
        private readonly IGroupsRepository _groupsRepository;
        private readonly DekautoContext _сontext;
        public GroupsService(DekautoContext сontext, IGroupsRepository groupsRepository) 
        {
            _сontext = сontext;
            _groupsRepository = groupsRepository;
        }

        private async Task<Group> _assignEfStudentModelsAsync(Group group)
        {
            group.Students = (await _сontext.Groups.FirstOrDefaultAsync(g => g.Id == group.Id)).Students;
            return group;
        }

        /// <summary>
        /// Конвертирование из объекта src типа SRC в объект типа DEST через сериализацию и десереализацию в JSON-объект (встроенный авто-маппинг).
        /// </summary>
        /// <typeparam name="SRC"></typeparam>
        /// <typeparam name="DEST"></typeparam>
        /// <param name="src"></param>
        /// <returns></returns>
        private DEST _jsonSerializationConvert<SRC, DEST>(SRC src)
        {
            return JsonSerializer.Deserialize<DEST>(JsonSerializer.Serialize(src));
        }


        public async Task<Group> FromDtoAsync(GroupDto groupDto)
        {
            if (groupDto == null) throw new ArgumentNullException(nameof(groupDto));

            var group = await _сontext.Groups.FirstOrDefaultAsync(g => g.Id == groupDto.Id);
            group ??= _jsonSerializationConvert<GroupDto, Group>(groupDto);

            return group;

        }

        public GroupDto ToDto(Group group)
        {
            if (group == null) throw new ArgumentNullException(nameof(group));

            var groupDto = new GroupDto();
            groupDto.Id = group.Id;
            groupDto.Name = group.Name;

            return groupDto;
        }

        public IEnumerable<GroupDto> ToDtos(IEnumerable<Group> groups)
        {
            if (groups == null) throw new ArgumentNullException(nameof(groups));

            var groupDtos = new List<GroupDto>();
            foreach (var group in groups)
            {
                groupDtos.Add(_jsonSerializationConvert<Group, GroupDto>(group));
            }

            return groupDtos;
        }

        public async Task UpdateAsync(Guid groupId, GroupDto updatedGroupDto)
        {
            if (updatedGroupDto == null || groupId == null) throw new ArgumentNullException("Не все аргументы переданы.");
            if (updatedGroupDto.Id != groupId) throw new ArgumentException("ID не совпадают.");

            var group = _jsonSerializationConvert<GroupDto, Group>(updatedGroupDto);
            await _groupsRepository.UpdateAsync(group);

        }

        public async Task AddAsync(GroupDto groupDto)
        {
            if (groupDto == null) throw new ArgumentNullException(nameof(groupDto));
            if (await _сontext.Groups.FirstOrDefaultAsync(g => g.Id == groupDto.Id) == null)
            {
                var group = await FromDtoAsync(groupDto);
                await _groupsRepository.AddAsync(group);
            } else
            {
                throw new Exception($"Такой элемент уже существует в базе данных; ID = {groupDto.Id}.");
            }
        }
    }
}
