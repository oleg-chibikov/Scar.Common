using System;
using Scar.Common.DAL.Contracts.Model;

namespace Scar.Common.DAL.Contracts;

public interface IDisposableRepository<T, TId> : IRepository<T, TId>, IDisposable
    where T : IEntity<TId>;
