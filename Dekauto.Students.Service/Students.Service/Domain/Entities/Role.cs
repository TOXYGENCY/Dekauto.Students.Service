using System.Text.Json.Serialization;

namespace Dekauto.Students.Service.Students.Service.Domain.Entities;

public partial class Role
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
