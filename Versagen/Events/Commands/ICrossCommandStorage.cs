using System;
using System.Collections.Generic;
using System.Text;

namespace Versagen.Events.Commands
{
    public interface ICrossCommandStorage : IServiceProvider, IDisposable
    {
        bool AddService(Type key, object obj);
        bool TryAddService(Type serviceType, object obj);
        bool TryAddObject(object obj);
        bool TryAddStringFlag(string str);
        bool AddStringData(string key, string data);
        bool TryAddStringData(string key, string data);
        bool RemoveServices(Type key, out IEnumerable<object> obj);
        bool RemoveObject(object obj);
        bool RemoveStringFlag(string str);
        bool RemoveStringData(string key, out string data);
        bool HasStringFlag(string str);
    }
}
