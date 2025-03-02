using Dekauto.Students.Service.Students.Service.Domain.Entities;
using Dekauto.Students.Service.Students.Service.Infrastructure;
using Dekauto.Ts.Service.Ts.Service.Domain.Interfaces;

namespace Dekauto.Students.Service.Students.Service.Domain.Interfaces
{
    public interface IStudentsRepository : IRepository<Student>
    {
        
    }
}
