using Dekauto.Students.Service.Students.Service.Domain.Entities;
using Dekauto.Students.Service.Students.Service.Infrastructure;
using Dekauto.Ts.Service.Ts.Service.Domain.Interfaces;

namespace Dekauto.Students.Service.Students.Service.Domain.Interfaces
{
    public interface IStudentsRepository : IRepository<Student>
    {
        Task<IEnumerable<Student>> GetStudentsByGroup(Student student);
        Task<IEnumerable<Student>> GetStudentsByGroup(Guid group_id);

    }
}
