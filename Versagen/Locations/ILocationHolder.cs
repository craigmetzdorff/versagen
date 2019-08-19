using System;
using System.Collections.Generic;
using System.Text;

namespace Versagen.Locations
{
    public class RootLocationHolder<T>:RootLocationHolder where T : class, ILocation
    {
        public new T Root { get => base.Root as T; set => base.Root = value; }
    }

    public abstract class RootLocationHolder
    {
        public ILocation Root { get; protected set; }
    }
}
