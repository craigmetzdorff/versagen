using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Versagen.Utils
{
    public class PathBrowser<T>
    {
        private Func<T,T> ParentSelector { get; }
        private Func<T,T> RootSelector { get; }
        private Func<T,IQueryable<T>> ChildSelector { get; }
        private Func<T,string> NameSelector { get; }
        public PathBrowser(Func<T, T> parentSelector, Func<T, IQueryable<T>> childSelector, Func<T,string> pathNameSelector)
        {
            ParentSelector = parentSelector;
            ChildSelector = childSelector;
            RootSelector = GetRootDefault;
            NameSelector = pathNameSelector;
        }

        public PathBrowser(Func<T, T> parentSelector, Func<T, IQueryable<T>> childSelector, Func<T, string> pathNameSelector, Func<T,T> rootSelector)
        {
            ParentSelector = parentSelector;
            ChildSelector = childSelector;
            RootSelector = rootSelector;
            NameSelector = pathNameSelector;
        }

        protected T GetRootDefault(T current)
        {
            var root = current;
            var retVal = root;
            while ((root = ParentSelector(root)) != null)
                retVal = root;
            return retVal;
        }

        public string GetPath(T startingPoint)
        {
            var pathout = new StringBuilder();
            var current = startingPoint;
            do
            {
                pathout.Append("/");
                pathout.Append(NameSelector(current));
            } while ((current = ParentSelector(current)) != null);

            return pathout.ToString();
        }

        public T BrowseTo(T startingPoint, string path)
        {

            T current = path.StartsWith('/') ? RootSelector(startingPoint) : startingPoint;
            foreach (var direction in path.Split('/', StringSplitOptions.RemoveEmptyEntries))
            {
                switch (direction)
                {
                    case ".":
                        break;
                    case "..":
                        current = ParentSelector(current);
                        break;
                    default:
                        current = ChildSelector(current).First(c => NameSelector(c) == direction);
                        break;
                }
            }
            return current;
        }

    }
}
