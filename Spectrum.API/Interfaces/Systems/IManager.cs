using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum.API.Interfaces.Systems
{
    public interface IManager
    {
        ILoader LuaLoader { get; }
        IExecutor LuaExecutor { get; }
    }
}
