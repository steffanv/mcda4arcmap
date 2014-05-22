using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCDA.Model
{
    public interface IDeepClonable<T> where T : class
    {
        T DeepClone();
    }
}
