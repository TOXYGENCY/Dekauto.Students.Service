using System.Text.Json.Serialization;

namespace Dekauto.Students.Service.Students.Service.Domain.Entities;

public partial class ResidentialType
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    [JsonIgnore]
    public virtual ICollection<Student> StudentAddressRegistrationTypes { get; set; } = new List<Student>();

    [JsonIgnore]
    public virtual ICollection<Student> StudentAddressResidentialTypes { get; set; } = new List<Student>();
}
