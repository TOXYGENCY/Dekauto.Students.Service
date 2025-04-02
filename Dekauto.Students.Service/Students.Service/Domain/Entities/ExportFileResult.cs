namespace Dekauto.Students.Service.Students.Service.Domain.Entities
{
    public class ExportFileResult
    {
        public byte[] FileData { get; set; }
        public string FileName { get; set; }

        public ExportFileResult(byte[] FileData, string FileName) {
            this.FileData = FileData;
            this.FileName = FileName;
        }
    }
}
