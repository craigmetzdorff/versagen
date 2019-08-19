using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Versagen.Exceptions
{
    public class IDTypeException:ArgumentOutOfRangeException
    {
        public IDTypeException() : base("A Versagen ID refers to an unexpected object type!",innerException:null){}
        public IDTypeException(string paramName, VersaCommsID ID):base(paramName, ID, "Wrong ID type encountered!") {}
        public IDTypeException(string paramName, VersaCommsID ID, string message) : base(paramName, ID, message) {}

        public IDTypeException(string paramName, VersaCommsID ID, EVersaCommIDType MissingFlag) : base(paramName, ID,
            "This ID is missing a given flag from its type!")
        {
            Data.Add("Missing Flag", MissingFlag);
            Data.Add("Present Flags", ID.IdType);
        }
        
    }
}
