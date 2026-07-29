#nullable enable
namespace UniT.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using DI;
    using Extensions;
    using Logging;
    using Pooling;
    using UnityEngine;
    using UnityEngine.Scripting;
    using ILogger = Logging.ILogger;

    public sealed class EntityManager : IEntityManager, IDisposable
    {
        #region Constructor

        private readonly IDependencyContainer container;
        private readonly IObjectPoolManager objectPoolManager;
        private readonly ILogger logger;

        private readonly HashSet<object> trackingKeys = new();
        private readonly HashSet<GameObject> trackingPrefabs = new();
        private readonly Dictionary<GameObject, (IEntity Entity, IComponent[] Components)> objToEntity = new();
        private readonly Dictionary<IComponent, Type[]> componentToTypes = new();
        private readonly Dictionary<Type, HashSet<IComponent>> typeToSpawnedComponents = new();

        [Preserve]
        public EntityManager(IDependencyContainer container, IObjectPoolManager objectPoolManager, ILoggerManager loggerManager)
        {
            this.container = container;
            this.objectPoolManager = objectPoolManager;
            this.logger = loggerManager.GetLogger(this);

            this.objectPoolManager.Instantiated += this.OnInstantiated;
            this.objectPoolManager.Spawned += this.OnSpawned;
            this.objectPoolManager.Recycled += this.OnRecycled;
            this.objectPoolManager.CleanedUp += this.OnCleanedUp;

            this.logger.Debug("Constructed");
        }

        #endregion

        #region Public

        event Action<IEntity, IReadOnlyList<IComponent>> IEntityManager.Instantiated { add => this.instantiated += value; remove => this.instantiated -= value; }
        event Action<IEntity, IReadOnlyList<IComponent>> IEntityManager.Spawned { add => this.spawned += value; remove => this.spawned -= value; }
        event Action<IEntity, IReadOnlyList<IComponent>> IEntityManager.Recycled { add => this.recycled += value; remove => this.recycled -= value; }
        event Action<IEntity, IReadOnlyList<IComponent>> IEntityManager.CleanedUp { add => this.cleanedUp += value; remove => this.cleanedUp -= value; }

        void IEntityManager.Load(IEntity prefab, int count)
        {
            this.trackingPrefabs.Add(prefab.gameObject);
            this.objectPoolManager.Load(prefab.gameObject, count);
        }

        UniTask IEntityManager.LoadAsync(object key, int count, IProgress<float>? progress, CancellationToken cancellationToken)
        {
            this.trackingKeys.Add(key);
            return this.objectPoolManager.LoadAsync(key, count, progress, cancellationToken);
        }

        TEntity IEntityManager.Spawn<TEntity>(TEntity prefab, Vector3? position, Quaternion? rotation, Transform? parent, bool spawnInWorldSpace)
        {
            return this.objectPoolManager.Spawn<TEntity>(prefab.gameObject, position, rotation, parent, spawnInWorldSpace);
        }

        TEntity IEntityManager.Spawn<TEntity, TParams>(TEntity prefab, TParams @params, Vector3? position, Quaternion? rotation, Transform? parent, bool spawnInWorldSpace)
        {
            this.nextParams = @params;
            return this.objectPoolManager.Spawn<TEntity>(prefab.gameObject, position, rotation, parent, spawnInWorldSpace);
        }

        TEntity IEntityManager.Spawn<TEntity>(object key, Vector3? position, Quaternion? rotation, Transform? parent, bool spawnInWorldSpace)
        {
            return this.objectPoolManager.Spawn<TEntity>(key, position, rotation, parent, spawnInWorldSpace);
        }

        TEntity IEntityManager.Spawn<TEntity, TParams>(object key, TParams @params, Vector3? position, Quaternion? rotation, Transform? parent, bool spawnInWorldSpace)
        {
            this.nextParams = @params;
            return this.objectPoolManager.Spawn<TEntity>(key, position, rotation, parent, spawnInWorldSpace);
        }

        void IEntityManager.Recycle(IEntity instance)
        {
            if (instance.Equals(null)) return;
            this.objectPoolManager.Recycle(instance.gameObject);
        }

        void IEntityManager.RecycleAll(IEntity prefab) => this.objectPoolManager.RecycleAll(prefab.gameObject);

        void IEntityManager.RecycleAll(object key) => this.objectPoolManager.RecycleAll(key);

        void IEntityManager.Cleanup(IEntity prefab, int retainCount) => this.objectPoolManager.Cleanup(prefab.gameObject, retainCount);

        void IEntityManager.Cleanup(object key, int retainCount) => this.objectPoolManager.Cleanup(key, retainCount);

        void IEntityManager.Unload(IEntity prefab)
        {
            this.trackingPrefabs.Remove(prefab.gameObject);
            this.objectPoolManager.Unload(prefab.gameObject);
        }

        void IEntityManager.Unload(object key)
        {
            this.trackingKeys.Remove(key);
            this.objectPoolManager.Unload(key);
        }

        IEnumerable<T> IEntityManager.Query<T>()
        {
            return this.typeToSpawnedComponents.GetValueOrDefault(typeof(T))?.Cast<T>() ?? Enumerable.Empty<T>();
        }

        void IDisposable.Dispose()
        {
            this.trackingKeys.Clear(this.objectPoolManager.Unload);
            this.trackingPrefabs.Clear(this.objectPoolManager.Unload);

            this.objectPoolManager.Instantiated -= this.OnInstantiated;
            this.objectPoolManager.Spawned -= this.OnSpawned;
            this.objectPoolManager.Recycled -= this.OnRecycled;
            this.objectPoolManager.CleanedUp -= this.OnCleanedUp;

            this.logger.Debug("Disposed");
        }

        #endregion

        #region Private

        private Action<IEntity, IReadOnlyList<IComponent>>? instantiated;
        private Action<IEntity, IReadOnlyList<IComponent>>? spawned;
        private Action<IEntity, IReadOnlyList<IComponent>>? recycled;
        private Action<IEntity, IReadOnlyList<IComponent>>? cleanedUp;

        private object? nextParams;

        private void OnInstantiated(GameObject instance)
        {
            if (!instance.TryGetComponent<IEntity>(out var entity)) return;
            var components = entity.gameObject.GetComponentsInChildren<IComponent>();
            this.objToEntity.Add(instance, (entity, components));
            foreach (var component in components)
            {
                this.componentToTypes.Add(
                    component,
                    component.GetType()
                        .GetInterfaces()
                        .Prepend(component.GetType())
                        .ToArray()
                );
                component.Container = this.container;
                component.Manager = this;
                component.Entity = entity;
            }
            foreach (var component in components) component.OnInstantiate();
            this.instantiated?.Invoke(entity, components);
        }

        private void OnSpawned(GameObject instance)
        {
            if (!this.objToEntity.TryGetValue(instance, out var value)) return;
            var (entity, components) = value;
            if (this.nextParams is not null)
            {
                ((IEntityWithParams)entity).Params = this.nextParams;
                this.nextParams = null;
            }
            foreach (var component in components)
            {
                foreach (var type in this.componentToTypes[component])
                {
                    this.typeToSpawnedComponents.GetOrAdd(type).Add(component);
                }
            }
            foreach (var component in components) component.OnSpawn();
            this.spawned?.Invoke(entity, components);
        }

        private void OnRecycled(GameObject instance)
        {
            if (!this.objToEntity.TryGetValue(instance, out var value)) return;
            var (entity, components) = value;
            foreach (var component in components)
            {
                foreach (var type in this.componentToTypes[component])
                {
                    this.typeToSpawnedComponents[type].Remove(component);
                }
            }
            foreach (var component in components) component.OnRecycle();
            if (entity is IEntityWithParams entityWithParams)
            {
                entityWithParams.Params = null;
            }
            this.recycled?.Invoke(entity, components);
        }

        private void OnCleanedUp(GameObject instance)
        {
            if (!this.objToEntity.Remove(instance, out var value)) return;
            var (entity, components) = value;
            this.componentToTypes.RemoveRange(components);
            foreach (var component in components) component.OnCleanup();
            this.cleanedUp?.Invoke(entity, components);
        }

        #endregion
    }
}