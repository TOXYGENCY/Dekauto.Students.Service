using System;
using System.Collections.Generic;

namespace Dekauto.Students.Service.Students.Service.Domain.Entities;

public partial class Role
{
    public Guid Id { get; set; }

    public string EngName { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
