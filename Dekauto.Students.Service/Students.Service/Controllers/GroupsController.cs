using Dekauto.Students.Service.Students.Service.Domain.Entities.DTO;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dekauto.Students.Service.Students.Service.Controllers
{
    [Route("api/groups")]
    [ApiController]
    public class GroupsController : ControllerBase
    {
        private readonly IGroupsRepository _groupsRepository;
        private readonly IGroupsService _groupsService;

        public GroupsController(IGroupsRepository groupsRepository, IGroupsService groupsService)
        {
            _groupsRepository = groupsRepository;
            _groupsService = groupsService;
        }
        // TODO: обезопасить все catch - убрать ex.message из вывода (в продакшен)

        [HttpGet("debug")]
        public async Task<ActionResult<IEnumerable<GroupDto>>> GetAllGroupsDebug()
        {
            try
            {
                var groups = await _groupsRepository.GetAllAsync();
                return Ok(groups);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // INFO: может вернуть пустой список
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GroupDto>>> GetAllGroups()
        {
            try
            {
                var groups = await _groupsRepository.GetAllAsync();
                var groupsDto = _groupsService.ToDtos(groups);
                return Ok(groupsDto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("{groupId}")]
        public async Task<ActionResult<GroupDto>> GetGroupById(Guid groupId)
        {
            try
            {
                var group = await _groupsRepository.GetByIdAsync(groupId);
                var groupDto = _groupsService.ToDto(group);
                if (groupDto == null) return StatusCode(StatusCodes.Status404NotFound);
                return Ok(groupDto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("{groupId}")]
        public async Task<IActionResult> UpdateGroupAsync(Guid groupId, GroupDto updatedGroupDto)
        {
            try
            {
                await _groupsService.UpdateAsync(groupId, updatedGroupDto);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddGroupAsync(GroupDto groupDto)
        {
            try
            {
                await _groupsService.AddAsync(groupDto);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }


        [HttpDelete("{groupId}")]
        public async Task<IActionResult> DeleteGroup(Guid groupId)
        {
            try
            {
                if (groupId == null) return StatusCode(StatusCodes.Status400BadRequest);
                await _groupsRepository.DeleteAsync(groupId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
