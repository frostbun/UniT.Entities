#if UNIT_DI
#nullable enable
namespace UniT.Entities.DI
{
    using UniT.DI;
    using UniT.Logging.DI;
    using UniT.Pooling.DI;

    public static class EntityManagerDI
    {
        public static void AddEntityManager(this DependencyContainer container)
        {
            if (container.Contains<IEntityManager>()) return;
            container.AddLoggerManager();
            container.AddObjectPoolManager();
            container.AddInterfaces<EntityManager>();
        }
    }
}
#endif