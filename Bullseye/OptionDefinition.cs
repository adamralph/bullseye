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
        /// <param name="shortName">The short name of the option.</param>
        /// <param name="longName">The long name of the option.</param>
        /// <param name="description"></param>
        public OptionDefinition(string shortName, string longName, string description)
        {
            this.ShortName = shortName;
            this.LongName = longName;
            this.Description = description;
        }

        /// <summary>
        /// Gets the short name of the option.
        /// </summary>
        public string ShortName { get; }

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
