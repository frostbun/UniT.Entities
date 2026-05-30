#nullable enable
namespace UniT.Entities
{
    using UniT.DI;
    using UnityEngine;

    public interface IComponent : IComponentLifecycle
    {
        public IDependencyContainer Container { set; }

        public IEntityManager Manager { get; set; }

        public IEntity Entity { get; set; }

        // ReSharper disable once InconsistentNaming
        public GameObject gameObject { get; }
    }
}