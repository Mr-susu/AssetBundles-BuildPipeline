using System.Collections.Generic;

namespace UnityEditor.Build.Interfaces
{
    // TODO: Think about a better name than 'BuildContent' -> ISourceContent & IBundleSourceContent
    public interface IBuildContent : IContextObject
    {
        // TODO: List > IList, Mutatible, Allocations?
        List<GUID> Assets { get; }

        List<GUID> Scenes { get; }

        // TODO: Dictionary > IDictionary
        // TODO: Move to IBundleContent -> Lookup Function
        Dictionary<GUID, string> Addresses { get; }

        /* TODO: General census: Interface names could use a pass to better describe how they are used or what they contain
         * Look at usage of the lookup maps and see if multiple maps should be combined into a single map with a struct/class of information
         * Look at usage of the lookup maps and see if they should remain as maps, or be converted into lookup functional calls
         */ 
    }

    public interface IBundleContent : IBuildContent
    {
        Dictionary<string, List<GUID>> ExplicitLayout { get; }
    }
}