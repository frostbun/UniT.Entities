#nullable enable
namespace UniT.Entities.Controller
{
    using System;

    public abstract class Entity<TController> : Entity, IComponent where TController : IController
    {
        protected virtual Type ControllerType => typeof(TController);

        protected TController Controller { get; private set; } = default!;

        void IComponent.OnInstantiate()
        {
            var controller = (TController)this.Container.Instantiate(this.ControllerType);
            controller.Component = this;
            this.Controller      = controller;
            this.OnInstantiate();
            this.Controller.OnInstantiate();
        }

        void IComponent.OnSpawn()
        {
            this.OnSpawn();
            this.Controller.OnSpawn();
        }

        void IComponent.OnRecycle()
        {
            this.OnRecycle();
            this.Controller.OnRecycle();
        }
    }

    public abstract class Entity<TParams, TController> :  UniT.Entities.Entity<TParams>, IComponent where TController : IController
    {
        protected virtual Type ControllerType => typeof(TController);

        protected TController Controller { get; private set; } = default!;

        void IComponent.OnInstantiate()
        {
            var controller = (TController)this.Container.Instantiate(this.ControllerType);
            controller.Component = this;
            this.Controller      = controller;
            this.OnInstantiate();
            this.Controller.OnInstantiate();
        }

        void IComponent.OnSpawn()
        {
            this.OnSpawn();
            this.Controller.OnSpawn();
        }

        void IComponent.OnRecycle()
        {
            this.OnRecycle();
            this.Controller.OnRecycle();
        }
    }
}