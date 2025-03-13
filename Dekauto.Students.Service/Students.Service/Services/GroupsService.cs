using Dekauto.Students.Service.Students.Service.Domain.Entities;
using Dekauto.Students.Service.Students.Service.Domain.Entities.DTO;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Dekauto.Students.Service.Students.Service.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Dekauto.Students.Service.Students.Service.Services
{
    public class GroupsService : IDtoConverter<Group, GroupDto>
    {
        private readonly IGroupsRepository _groupsRepository;
        private readonly DekautoContext _сontext;
        public GroupsService(DekautoContext сontext, IGroupsRepository groupsRepository) 
        {
            _сontext = сontext;
            _groupsRepository = groupsRepository;
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

        // TODO: разобраться правильно ли это
        public async Task<Group> FromDtoAsync(GroupDto dto)
        {
            return await _сontext.Groups.FirstOrDefaultAsync(g => g.Id == dto.Id);

        }

        public GroupDto ToDto(Group group)
        {
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

        
    }
}
