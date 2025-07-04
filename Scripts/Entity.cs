﻿#nullable enable
namespace UniT.Entities
{
    using UnityEngine;

    [DisallowMultipleComponent]
    public abstract class BaseEntity : Component, IEntity
    {
        public void Recycle() => this.Manager.Recycle(this);
    }

    public abstract class Entity : BaseEntity, IEntityWithoutParams
    {
    }

    public abstract class Entity<TParams> : BaseEntity, IEntityWithParams
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        object IEntityWithParams.Params { set => this.Params = value is null ? default! : (TParams)value; }

        protected TParams Params { get; private set; } = default!;
    }
}