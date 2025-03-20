namespace Dekauto.Students.Service.Students.Service.Domain.Entities.Adapters
{
    // Адаптер для распаковки файлов из контроллера 
    public class ImportFilesAdapter
    {
        public IFormFile? ld { get; set; } // Личное дело
        public IFormFile? contract { get; set; } // Журнал договоров
        public IFormFile? journal { get; set; } // Журнал зачеток
    }
}
