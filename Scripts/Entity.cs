#nullable enable
namespace UniT.Entities
{
    using System;
    using UnityEngine;

    [DisallowMultipleComponent]
    public abstract class BaseEntity : Component, IEntity
    {
        public void Recycle() => this.Manager.Recycle(this);
    }

    public class Entity : BaseEntity, IEntityWithoutParams
    {
    }

    public abstract class Entity<TParams> : BaseEntity, IEntityWithParams
    {
        private TParams? @params;

        object IEntityWithParams.Params
        {
            set => this.@params = value switch
            {
                null            => default,
                TParams @params => @params,
                _               => throw new InvalidCastException($"{this.GetType().Name} expected {typeof(TParams)}, got {value.GetType().Name}"),
            };
        }

        protected TParams Params => this.@params ?? throw new InvalidOperationException($"{this.name} not spawned or already recycled");
    }
}