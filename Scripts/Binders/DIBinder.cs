#if UNIT_DI
#nullable enable
namespace UniT.Entities
{
    using UniT.DI;

    public static class DIBinder
    {
        public static void AddEntityManager(this DependencyContainer container)
        {
            container.AddInterfaces<EntityManager>();
        }
    }
}
#endif