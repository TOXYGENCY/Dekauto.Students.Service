using Dekauto.Students.Service.Students.Service.Domain.Entities.DTO;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dekauto.Students.Service.Students.Service.Controllers
{
    [Route("api/groups")]
    [ApiController]
    [Authorize(Policy = "OnlyAdmin")] // Требует аутентификации в роли "Администратор" для всех методов

    public class GroupsController : ControllerBase
    {
        private readonly IGroupsRepository groupsRepository;
        private readonly IGroupsService groupsService;
        private readonly ILogger<ExportController> logger;

        public GroupsController(IGroupsRepository groupsRepository, IGroupsService groupsService, 
            ILogger<ExportController> logger)
        {
            this.groupsRepository = groupsRepository;
            this.groupsService = groupsService;
            this.logger = logger;
        }

        // INFO: может вернуть пустой список
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GroupDto>>> GetAllGroups()
        {
            try
            {
                var groups = await groupsRepository.GetAllAsync();
                var groupsDto = groupsService.ToDtos(groups);
                return Ok(groupsDto);
            }
            catch (Exception ex)
            {
                var mes = "Возникла непредвиденная ошибка сервера. Обратитесь к администратору или попробуйте позже.";
                logger.LogError(ex, mes);
                return StatusCode(StatusCodes.Status500InternalServerError, mes);
            }
        }

        [HttpGet("{groupId}")]
        public async Task<ActionResult<GroupDto>> GetGroupById(Guid groupId)
        {
            try
            {
                var group = await groupsRepository.GetByIdAsync(groupId);
                if (group == null)
                {
                    throw new KeyNotFoundException($"Нет группы с id = {groupId}");
                }
                var groupDto = groupsService.ToDto(group);
                return Ok(groupDto);
            }
            catch (KeyNotFoundException ex)
            {
                var mes = "Указанная группа не найдена.";
                logger.LogWarning(ex, mes);
                return StatusCode(StatusCodes.Status404NotFound, mes);
            }
            catch (Exception ex)
            {
                var mes = "Возникла непредвиденная ошибка сервера. Обратитесь к администратору или попробуйте позже.";
                logger.LogError(ex, mes);
                return StatusCode(StatusCodes.Status500InternalServerError, mes);
            }
        }

        [HttpPut("{groupId}")]
        public async Task<IActionResult> UpdateGroupAsync(Guid groupId, GroupDto updatedGroupDto)
        {
            try
            {
                if (updatedGroupDto is null)
                {
                    throw new ArgumentNullException(nameof(updatedGroupDto));
                }
                await groupsService.UpdateAsync(groupId, updatedGroupDto);
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                var mes = "Не найдена группа для обновления.";
                logger.LogWarning(ex, mes);
                return StatusCode(StatusCodes.Status500InternalServerError, mes);
            }
            catch (ArgumentNullException ex)
            {
                var mes = "Не передана группа для обновления. Обратитесь к администратору или попробуйте позже.";
                logger.LogError(ex, mes);
                return StatusCode(StatusCodes.Status500InternalServerError, mes);
            }
            catch (Exception ex)
            {
                var mes = "Возникла непредвиденная ошибка сервера. Обратитесь к администратору или попробуйте позже.";
                logger.LogError(ex, mes);
                return StatusCode(StatusCodes.Status500InternalServerError, mes);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddGroupAsync(GroupDto groupDto)
        {
            try
            {
                if (groupDto is null)
                {
                    throw new ArgumentNullException(nameof(groupDto));
                }

                await groupsService.AddAsync(groupDto);
                logger.LogInformation($"Создана группа {groupDto.Name}");

                return Ok();
            }
            catch (Exception ex)
            {
                var mes = "Возникла непредвиденная ошибка сервера. Обратитесь к администратору или попробуйте позже.";
                logger.LogError(ex, mes);
                return StatusCode(StatusCodes.Status500InternalServerError, mes);
            }
        }


        [HttpDelete("{groupId}")]
        public async Task<IActionResult> DeleteGroup(Guid groupId)
        {
            try
            {
                await groupsService.DeleteByIdAsync(groupId);
                logger.LogInformation($"Удалена группа с id = {groupId}");

                return Ok();
            }
            catch (Exception ex)
            {
                var mes = "Возникла непредвиденная ошибка сервера. Обратитесь к администратору или попробуйте позже.";
                logger.LogError(ex, mes);
                return StatusCode(StatusCodes.Status500InternalServerError, mes);
            }
        }
    }
}
