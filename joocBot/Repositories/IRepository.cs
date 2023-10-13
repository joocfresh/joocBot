using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace joocBot.Repositories
{
    public interface IRepository<T>
    {
        IEnumerable<T> GetAll();
        bool SaveOne(T session);
        bool DeleteOne(T session);
        bool Exist(T session);
    }
}
