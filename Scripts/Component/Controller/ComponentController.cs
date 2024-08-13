#nullable enable
namespace UniT.Entities.Component.Controller
{
    using UniT.Entities.Controller;
    using UniT.Entities.Entity;

    public abstract class ComponentController<TComponent> : Controller<TComponent>, IComponentController where TComponent : IComponent, IHasController
    {
        protected TComponent Component => this.Owner;

        protected IEntityManager Manager => this.Owner.Manager;

        protected IEntity Entity => this.Owner.Entity;

        public virtual void OnInstantiate() { }

        public virtual void OnSpawn() { }

        public virtual void OnRecycle() { }
    }
}