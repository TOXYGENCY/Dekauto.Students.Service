﻿using System;
using System.Collections.Generic;

namespace Dekauto.Students.Service.Students.Service.Domain.Entities;

public partial class Oo
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? OoAddress { get; set; }

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
