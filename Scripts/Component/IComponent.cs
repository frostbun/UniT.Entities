#nullable enable
namespace UniT.Entities.Component
{
    using UniT.Entities.Entity;
    using UnityEngine;

    public interface IComponent
    {
        public IEntityManager Manager { get; set; }

        public IEntity Entity { get; set; }

        public void OnInstantiate();

        public void OnSpawn();

        public void OnRecycle();

        // ReSharper disable once InconsistentNaming
        public GameObject gameObject { get; }
    }
}