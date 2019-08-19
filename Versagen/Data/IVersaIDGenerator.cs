using System;
using System.Collections.Generic;
using System.Text;

namespace Versagen.Data
{
    public interface IVersaIDGenerator
    {
        VersaCommsID GetNewID(EVersaCommIDType typeRequired);
    }
}
