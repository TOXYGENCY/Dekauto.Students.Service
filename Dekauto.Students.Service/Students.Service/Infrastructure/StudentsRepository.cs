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

        public async Task DeleteAsync(Guid id)
        {
            _сontext.Remove(await GetByIdAsync(id));
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

        public async Task<string> GetGroupNameAsync(Guid groupId)
        {
            var group = await _сontext.Groups.FirstOrDefaultAsync(group => group.Id == groupId);
            return group.Name;
        }
        public async Task<string> GetGroupNameAsync(Student student)
        {
            var group = await _сontext.Groups.FirstOrDefaultAsync(group => group.Id == student.GroupId);
            return group.Name;
        }

        public async Task<Oo> GetOoByIdAsync(Guid ooId)
        {
            return await _сontext.Oos.FirstOrDefaultAsync(oo => oo.Id == ooId);
        }

        public async Task<IEnumerable<Student>> GetStudentsByGroupAsync(Student student)
        {
            return await _сontext.Students.Where(s => s.GroupId == student.GroupId).ToListAsync();
        }

        public async Task<IEnumerable<Student>> GetStudentsByGroupAsync(Guid groupId)
        {
            return await _сontext.Students.Where(s => s.GroupId == groupId).ToListAsync();
        }

        public async Task UpdateAsync(Student updatedStudent)
        {
            var currentStudent = await _сontext.Students.FirstOrDefaultAsync(s => s.Id == updatedStudent.Id);
            if (currentStudent == null) throw new KeyNotFoundException($"Student {updatedStudent.Id} not found");

            _сontext.Entry(currentStudent).CurrentValues.SetValues(updatedStudent);
            await _сontext.SaveChangesAsync();
        }
    }
}
