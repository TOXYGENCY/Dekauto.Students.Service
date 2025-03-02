using Dekauto.Students.Service.Students.Service.Domain.Entities;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dekauto.Students.Service.Students.Service.Infrastructure
{
    public class StudentsRepository : IStudentsRepository
    {
        private DekautoContext _сontext;

        public StudentsRepository(DekautoContext context)
        {
            _сontext = context;
        }

        public async Task AddAsync(Student student)
        {
            _сontext.Students.Add(student);
            await _сontext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Student student)
        {
            _сontext.Remove(student);
            await _сontext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Student>> GetAllAsync()
        {
            return await _сontext.Students.ToListAsync();
        }

        public async Task<Student> GetByIdAsync(Guid id)
        {
            return await _сontext.Students.FirstOrDefaultAsync(student => student.Id == id);
        }

        public async Task UpdateAsync(Guid id, Student student)
        {
            throw new NotImplementedException();
            await _сontext.SaveChangesAsync();
        }
    }
}
