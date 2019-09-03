using System;
using System.Collections.Generic;
using System.Text;

namespace Bullseye
{
    /// <summary>
    /// User-defined context object Bullseye will flow through to dependent targets
    /// </summary>
    public interface IBuildContext
    {
        /// <summary>
        /// Container for items to be made available to the target chain
        /// </summary>
        Dictionary<string, object> Items { get; }
    }
}
