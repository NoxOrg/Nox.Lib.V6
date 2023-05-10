﻿using Nox.Core.Components;

namespace Nox.Core.Models.Entity;

public sealed class EntityRelationship : MetaBase
{
    public string Name { get; set; } = string.Empty;

    public string Entity { get; set; } = string.Empty;

    public bool IsMany { get; set; } = false;

    public bool IsOneWay { get; set; } = false;

    public bool IsRequired { get; set; } = false;
}