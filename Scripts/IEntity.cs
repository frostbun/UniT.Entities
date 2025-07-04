#nullable enable
namespace UniT.Entities
{
    public interface IEntity : IComponent
    {
    }

    public interface IEntityWithoutParams : IEntity
    {
    }

    public interface IEntityWithParams : IEntity
    {
        public object Params { set; }
    }
}