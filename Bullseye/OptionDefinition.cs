using System.Collections.Generic;

namespace Bullseye
{
    /// <summary>
    /// Defines an option.
    /// </summary>
    public class OptionDefinition
    {
        /// <summary>
        /// Constructs a new option definition.
        /// </summary>
        /// <param name="longName">The long name of the option.</param>
        /// <param name="description">The description of the option.</param>
        public OptionDefinition(string longName, string description)
        {
            this.LongName = longName;
            this.Aliases = new List<string> { longName };
            this.Description = description;
        }

        /// <summary>
        /// Constructs a new option definition.
        /// </summary>
        /// <param name="shortName">The short name of the option.</param>
        /// <param name="longName">The long name of the option.</param>
        /// <param name="description">The description of the option.</param>
        public OptionDefinition(string shortName, string longName, string description)
        {
            this.ShortName = shortName;
            this.LongName = longName;
            this.Aliases = new List<string> { shortName, longName };
            this.Description = description;
        }

        /// <summary>
        /// Gets the aliases of the option.
        /// The aliases will either be:
        /// The short name and the long name (if the short name is not <c>null</c>)
        /// -or-
        /// just the long name (if the short name is <c>null</c>).
        /// </summary>
        public IReadOnlyList<string> Aliases { get; }

        /// <summary>
        /// Gets the short name of the option.
        /// </summary>
        public string? ShortName { get; }

        /// <summary>
        /// Gets the long name of the option.
        /// </summary>
        public string LongName { get; }

        /// <summary>
        /// Gets the description of the option.
        /// </summary>
        public string Description { get; }
    }
}
