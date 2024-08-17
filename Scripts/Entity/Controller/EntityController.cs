#nullable enable
namespace UniT.Entities.Entity.Controller
{
    using UniT.Entities.Component.Controller;
    using UniT.Entities.Controller;

    public abstract class EntityController<TEntity> : ComponentController<TEntity>, IEntityController where TEntity : IEntity, IHasController
    {
    }
}