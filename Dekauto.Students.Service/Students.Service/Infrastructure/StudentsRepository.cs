using Dekauto.Students.Service.Students.Service.Domain.Entities;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dekauto.Students.Service.Students.Service.Infrastructure
{
    public class StudentsRepository : IStudentsRepository
    {
        private DekautoContext context;

        public StudentsRepository(DekautoContext context)
        {
            this.context = context;
        }

        public async Task AddAsync(Student student)
        {
            context.Students.Add(student);
            await context.SaveChangesAsync();
        }

        public async Task DeleteByIdAsync(Guid id)
        {
            context.Remove(await GetByIdAsync(id));
            await context.SaveChangesAsync();
        }

        public async Task DeleteRangeAsync(IEnumerable<Student> range)
        {
            context.RemoveRange(range);
            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Student>> GetAllAsync()
        {
            return await context.Students.ToListAsync();
        }

        public async Task<Student> GetByIdAsync(Guid id)
        {
            return await context.Students.FirstOrDefaultAsync(student => student.Id == id);
        }

        public async Task<string> GetGroupNameAsync(Guid groupId)
        {
            var group = await context.Groups.FirstOrDefaultAsync(group => group.Id == groupId);
            return group.Name;
        }
        public async Task<string> GetGroupNameAsync(Student student)
        {
            var group = await context.Groups.FirstOrDefaultAsync(group => group.Id == student.GroupId);
            return group.Name;
        }

        public async Task<Oo> GetOoByIdAsync(Guid ooId)
        {
            return await context.Oos.FirstOrDefaultAsync(oo => oo.Id == ooId);
        }

        public async Task<IEnumerable<Student>> GetStudentsByGroupAsync(Student student)
        {
            return await context.Students.Where(s => s.GroupId == student.GroupId).ToListAsync();
        }

        public async Task<IEnumerable<Student>> GetStudentsByGroupAsync(Guid groupId)
        {
            return await context.Students.Where(s => s.GroupId == groupId).ToListAsync();
        }

        public async Task UpdateAsync(Student updatedStudent)
        {
            var currentStudent = await context.Students.FirstOrDefaultAsync(s => s.Id == updatedStudent.Id);
            if (currentStudent == null) throw new KeyNotFoundException($"Student {updatedStudent.Id} not found");

            context.Entry(currentStudent).CurrentValues.SetValues(updatedStudent);
            await context.SaveChangesAsync();
        }
    }
}

