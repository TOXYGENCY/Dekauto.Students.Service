namespace Dekauto.Students.Service.Students.Service.Domain.Entities.Adapters
{
    // Адаптер для распаковки файлов из контроллера 
    public class ImportFilesAdapter
    {
        public IFormFile? ld { get; set; } // Личное дело
        public IFormFile? log { get; set; } // Журнал договоров
        public IFormFile? log2 { get; set; } // Журнал зачеток
    }
}
