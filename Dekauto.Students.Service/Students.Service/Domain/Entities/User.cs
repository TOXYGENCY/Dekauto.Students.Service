using System.Text.Json.Serialization;

namespace Dekauto.Students.Service.Students.Service.Domain.Entities;

public partial class User
{
    public Guid Id { get; set; }

    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    public Guid RoleId { get; set; }

    [JsonIgnore]
    public virtual Role Role { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
