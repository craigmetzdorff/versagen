using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Versagen.IO
{
    public interface IVersaWriterDirectory:IDisposable
    {
        IVersaGroupWriter GetOrCreateGroup(VersaCommsID id);
        IVersaGroupWriter CreateGroup(EVersaCommIDType type);
        bool DeleteGroupWriter(VersaCommsID id);
        bool DeleteGroupWriter(IVersaGroupWriter writer);
        IVersaWriter GetWriter(VersaCommsID Id);
        IQueryable<IVersaWriter> Writers { get; }
    }
}
