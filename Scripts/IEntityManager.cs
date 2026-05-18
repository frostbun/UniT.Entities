#nullable enable
namespace UniT.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using UniT.Extensions;
    using UnityEngine;

    public interface IEntityManager : IDisposable
    {
        public event Action<IEntity, IReadOnlyList<IComponent>> Instantiated;

        public event Action<IEntity, IReadOnlyList<IComponent>> Spawned;

        public event Action<IEntity, IReadOnlyList<IComponent>> Recycled;

        public event Action<IEntity, IReadOnlyList<IComponent>> CleanedUp;

        public void Load(IEntity prefab, int count = 1);

        public UniTask LoadAsync(object key, int count = 1, IProgress<float>? progress = null, CancellationToken cancellationToken = default);

        public TEntity Spawn<TEntity>(TEntity prefab, Vector3? position = null, Quaternion? rotation = null, Transform? parent = null, bool spawnInWorldSpace = true) where TEntity : IEntityWithoutParams;

        public TEntity Spawn<TEntity, TParams>(TEntity prefab, TParams @params, Vector3? position = null, Quaternion? rotation = null, Transform? parent = null, bool spawnInWorldSpace = true) where TEntity : IEntityWithParams<TParams> where TParams : notnull;

        public TEntity Spawn<TEntity>(object key, Vector3? position = null, Quaternion? rotation = null, Transform? parent = null, bool spawnInWorldSpace = true) where TEntity : IEntityWithoutParams;

        public TEntity Spawn<TEntity, TParams>(object key, TParams @params, Vector3? position = null, Quaternion? rotation = null, Transform? parent = null, bool spawnInWorldSpace = true) where TEntity : IEntityWithParams<TParams> where TParams : notnull;

        public void Recycle(IEntity instance);

        public void RecycleAll(IEntity prefab);

        public void RecycleAll(object key);

        public void Cleanup(IEntity prefab, int retainCount = 1);

        public void Cleanup(object key, int retainCount = 1);

        public void Unload(IEntity prefab);

        public void Unload(object key);

        public IEnumerable<T> Query<T>();

        #region Implicit Key

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask LoadAsync<TEntity>(int count = 1, IProgress<float>? progress = null, CancellationToken cancellationToken = default) where TEntity : IEntity => this.LoadAsync(typeof(TEntity).GetKey(), count, progress, cancellationToken);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TEntity Spawn<TEntity>(Vector3? position = null, Quaternion? rotation = null, Transform? parent = null, bool spawnInWorldSpace = true) where TEntity : IEntityWithoutParams => this.Spawn<TEntity>(typeof(TEntity).GetKey(), position, rotation, parent, spawnInWorldSpace);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TEntity Spawn<TEntity, TParams>(TParams @params, Vector3? position = null, Quaternion? rotation = null, Transform? parent = null, bool spawnInWorldSpace = true) where TEntity : IEntityWithParams<TParams> where TParams : notnull => this.Spawn<TEntity, TParams>(typeof(TEntity).GetKey(), @params, position, rotation, parent, spawnInWorldSpace);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RecycleAll<TEntity>() where TEntity : IEntity => this.RecycleAll(typeof(TEntity).GetKey());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Cleanup<TEntity>(int retainCount = 1) where TEntity : IEntity => this.Cleanup(typeof(TEntity).GetKey(), retainCount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unload<TEntity>() where TEntity : IEntity => this.Unload(typeof(TEntity).GetKey());

        #endregion
    }
}