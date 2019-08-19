using System;
using System.Collections.Generic;
using System.Text;

namespace Versagen.Utils
{
    internal interface IUnionType
    {
        object Obj { get; }

        Type CurrentType { get; }

        //(Type CurrentType, Type[] OtherTypes) GetCurrentAndOtherTypes();
        Type[] GetTypes();

        bool Is<T>(out T item);
        bool Is<T>();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <remarks>Disabled the MethodOverloadWithOptionalParameter warning because the refactoring result causes a compiler error due to <see cref="As{T}()"/></remarks>
        /// <returns></returns>
        // ReSharper disable once MethodOverloadWithOptionalParameter
        T As<T>(T defaultItem, bool throwIfInvalidCast = false) where T : struct;
        T As<T>() where T : class;
    }
}
