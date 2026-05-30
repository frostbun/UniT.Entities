#nullable enable
namespace UniT.Entities.Default.DI
{
    using Zenject;

    public static class EntityManagerZenject
    {
        public static void BindEntityManager(this DiContainer container)
        {
            container.BindInterfacesTo<EntityManager>().AsSingle();
        }
    }
}