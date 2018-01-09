using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityEditor.Build.Interfaces
{
    public interface IBuildLayout : IContextObject
    {
        // NOTES: Only used in Asset Bundle Compatible pipeline
        Dictionary<string, List<GUID>> ExplicitLayout { get; }
    }
}
