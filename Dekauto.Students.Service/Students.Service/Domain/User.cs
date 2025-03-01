﻿using System;
using System.Collections.Generic;

namespace Dekauto.Students.Service.Students.Service.Domain;

public partial class User
{
    public Guid Id { get; set; }

    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    public Guid RoleId { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
