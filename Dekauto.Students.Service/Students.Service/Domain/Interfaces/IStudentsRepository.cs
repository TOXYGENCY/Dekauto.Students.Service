using Dekauto.Students.Service.Students.Service.Domain.Entities;
using Dekauto.Ts.Service.Ts.Service.Domain.Interfaces;

namespace Dekauto.Students.Service.Students.Service.Domain.Interfaces
{
    public interface IStudentsRepository : IRepository<Student>
    {
        Task<IEnumerable<Student>> GetStudentsByGroupAsync(Student student);
        Task<IEnumerable<Student>> GetStudentsByGroupAsync(Guid groupId);
        Task<Oo> GetOoByIdAsync(Guid ooId);
        Task<ResidentialType> GetRegistrationTypeByIdAsync(Guid regTypeId);
        Task<ResidentialType> GetResidentialTypeByIdAsync(Guid resTypeId);
        Task<string> GetGroupNameAsync(Guid groupId);
        Task<string> GetGroupNameAsync(Student student);

    }
}
