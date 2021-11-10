using System;
using System.Collections.Generic;
using System.Linq;
using Bullseye.Internal;

namespace BullseyeTests.Infra
{
    internal static class Helper
    {
        public static Target CreateTarget(string name, Action action) => CreateTarget(name, Array.Empty<string>(), action);

        public static Target CreateTarget(string name, string[] dependencies, Action action) =>
            new ActionTarget(name, "", dependencies, action.ToAsync());

        public static Target CreateTarget(string name, string[] dependencies) =>
#if NET5_0_OR_GREATER
            new(name, "", dependencies);
#else
            new Target(name, "", dependencies);
#endif

        public static Target CreateTarget<TInput>(string name, IEnumerable<TInput> forEach, Action<TInput> action) =>
            new ActionTarget<TInput>(name, "", Array.Empty<string>(), forEach, action.ToAsync());
    }
}
