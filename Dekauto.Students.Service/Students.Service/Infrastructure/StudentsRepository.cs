using Dekauto.Students.Service.Students.Service.Domain.Entities;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dekauto.Students.Service.Students.Service.Infrastructure
{
    public class StudentsRepository : IStudentsRepository
    {
        private DekautoContext сontext;

        public StudentsRepository(DekautoContext context)
        {
            сontext = context;
        }

        public async Task AddAsync(Student student)
        {
            сontext.Students.Add(student);
            await сontext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            сontext.Remove(await GetByIdAsync(id));
            await сontext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Student>> GetAllAsync()
        {
            return await сontext.Students.ToListAsync();
        }

        public async Task<Student> GetByIdAsync(Guid id)
        {
            return await сontext.Students.FirstOrDefaultAsync(student => student.Id == id);
        }

        public async Task<string> GetGroupNameAsync(Guid groupId)
        {
            var group = await сontext.Groups.FirstOrDefaultAsync(group => group.Id == groupId);
            return group.Name;
        }
        public async Task<string> GetGroupNameAsync(Student student)
        {
            var group = await сontext.Groups.FirstOrDefaultAsync(group => group.Id == student.GroupId);
            return group.Name;
        }

        public async Task<Oo> GetOoByIdAsync(Guid ooId)
        {
            return await сontext.Oos.FirstOrDefaultAsync(oo => oo.Id == ooId);
        }

        public async Task<IEnumerable<Student>> GetStudentsByGroupAsync(Student student)
        {
            return await сontext.Students.Where(s => s.GroupId == student.GroupId).ToListAsync();
        }

        public async Task<IEnumerable<Student>> GetStudentsByGroupAsync(Guid groupId)
        {
            return await сontext.Students.Where(s => s.GroupId == groupId).ToListAsync();
        }

        public async Task UpdateAsync(Student updatedStudent)
        {
            var currentStudent = await сontext.Students.FirstOrDefaultAsync(s => s.Id == updatedStudent.Id);
            if (currentStudent == null) throw new KeyNotFoundException($"Student {updatedStudent.Id} not found");

            сontext.Entry(currentStudent).CurrentValues.SetValues(updatedStudent);
            await сontext.SaveChangesAsync();
        }
    }
}
