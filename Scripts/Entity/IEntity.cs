#nullable enable
namespace UniT.Entities.Entity
{
    using UniT.Entities.Component;
    using UnityEngine;

    public interface IEntity : IComponent
    {
        // ReSharper disable once InconsistentNaming
        public GameObject gameObject { get; }
    }

    public interface IEntityWithoutParams : IEntity
    {
    }

    public interface IEntityWithParams<TParams> : IEntity
    {
        public TParams Params { get; set; }
    }
}