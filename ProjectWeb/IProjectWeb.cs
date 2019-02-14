using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWeb
{
    public interface IProjectWeb
    {
        void ProcessAsync(object o);
        void Start();
    }
}
