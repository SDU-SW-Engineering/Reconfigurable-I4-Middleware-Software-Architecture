using System;
using System.Collections.Generic;

namespace Configurator.Database
{
    public interface IRepository<T>
    {
        List<T> read();
        T readById(int id);
        T create(T entity);
        T update(T entity);
        T delete(T entity);
    }
}
