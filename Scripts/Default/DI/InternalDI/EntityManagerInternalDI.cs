#nullable enable
namespace UniT.Entities.Default.DI
{
    using UniT.DI;

    public static class EntityManagerInternalDI
    {
        public static void AddEntityManager(this DependencyContainer container)
        {
            container.AddInterfaces<EntityManager>();
        }
    }
}