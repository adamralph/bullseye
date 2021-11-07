using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bullseye.Internal;

namespace Bullseye
{
    /// <summary>
    /// Provides methods for defining and running targets.
    /// </summary>
    public partial class Targets
    {
        /// <summary>
        /// Runs the targets and then calls <see cref="Environment.Exit(int)"/>.
        /// Any code which follows a call to this method will not be executed.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <param name="messageOnly">
        /// A predicate that is called when an exception is thrown.
        /// Return <c>true</c> to display only the exception message instead instead of the full exception details.
        /// </param>
        /// <param name="messagePrefix">
        /// The prefix to use for output and diagnostic messages.
        /// If not specified or <c>null</c>, the name of the entry assembly will be used, as returned by <see cref="System.Reflection.Assembly.GetEntryAssembly"/>.
        /// If the entry assembly is <c>null</c>, the default prefix of "Bullseye" is used.
        /// </param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous running of the targets.</returns>
        public Task RunAndExitAsync(
            IEnumerable<string> args,
            Func<Exception, bool> messageOnly = null,
            string messagePrefix = null) =>
            this.targetCollection.RunAsync(args.Sanitize().ToList(), messageOnly, messagePrefix, Console.Out, Console.Error, true);

        /// <summary>
        /// Runs the targets and then calls <see cref="Environment.Exit(int)"/>.
        /// Any code which follows a call to this method will not be executed.
        /// </summary>
        /// <param name="targets">The targets to run or list.</param>
        /// <param name="options">The options to use when running or listing targets.</param>
        /// <param name="unknownOptions">The unknown options specified in the command line arguments.</param>
        /// <param name="showHelp">A value indicating whether to show help.</param>
        /// <param name="messageOnly">
        /// A predicate that is called when an exception is thrown.
        /// Return <c>true</c> to display only the exception message instead instead of the full exception details.
        /// </param>
        /// <param name="messagePrefix">
        /// The prefix to use for output and diagnostic messages.
        /// If not specified or <c>null</c>, the name of the entry assembly will be used, as returned by <see cref="System.Reflection.Assembly.GetEntryAssembly"/>.
        /// If the entry assembly is <c>null</c>, the default prefix of "Bullseye" is used.
        /// </param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous running of the targets.</returns>
        public Task RunAndExitAsync(
            IEnumerable<string> targets,
            IOptions options,
            IEnumerable<string> unknownOptions = null,
            bool showHelp = false,
            Func<Exception, bool> messageOnly = null,
            string messagePrefix = null) =>
            this.targetCollection.RunAsync(targets, options, unknownOptions, showHelp, messageOnly, messagePrefix, Console.Out, Console.Error, true);

        /// <summary>
        /// Runs the targets.
        /// In most cases, <see cref="RunAndExitAsync(IEnumerable{string}, Func{Exception, bool}, string)"/> should be used instead of this method.
        /// This method should only be used if continued code execution after running targets is specifically required.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <param name="messageOnly">
        /// A predicate that is called when an exception is thrown.
        /// Return <c>true</c> to display only the exception message instead instead of the full exception details.
        /// </param>
        /// <param name="messagePrefix">
        /// The prefix to use for output and diagnostic messages.
        /// If not specified or <c>null</c>, the name of the entry assembly will be used, as returned by <see cref="System.Reflection.Assembly.GetEntryAssembly"/>.
        /// If the entry assembly is <c>null</c>, the default prefix of "Bullseye" is used.
        /// </param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous running of the targets.</returns>
        public Task RunWithoutExitingAsync(
            IEnumerable<string> args,
            Func<Exception, bool> messageOnly = null,
            string messagePrefix = null) =>
            this.targetCollection.RunAsync(args.Sanitize().ToList(), messageOnly, messagePrefix, Console.Out, Console.Error, false);

        /// <summary>
        /// Runs the targets.
        /// In most cases, RunAndExitAsync should be used instead of this method.
        /// This method should only be used if continued code execution after running targets is specifically required.
        /// </summary>
        /// <param name="targets">The targets to run or list.</param>
        /// <param name="options">The options to use when running or listing targets.</param>
        /// <param name="unknownOptions">The unknown options specified in the command line arguments.</param>
        /// <param name="showHelp">A value indicating whether to show help.</param>
        /// <param name="messageOnly">
        /// A predicate that is called when an exception is thrown.
        /// Return <c>true</c> to display only the exception message instead instead of the full exception details.
        /// </param>
        /// <param name="messagePrefix">
        /// The prefix to use for output and diagnostic messages.
        /// If not specified or <c>null</c>, the name of the entry assembly will be used, as returned by <see cref="System.Reflection.Assembly.GetEntryAssembly"/>.
        /// If the entry assembly is <c>null</c>, the default prefix of "Bullseye" is used.
        /// </param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous running of the targets.</returns>
        public Task RunWithoutExitingAsync(
            IEnumerable<string> targets,
            IOptions options,
            IEnumerable<string> unknownOptions = null,
            bool showHelp = false,
            Func<Exception, bool> messageOnly = null,
            string messagePrefix = null) =>
            this.targetCollection.RunAsync(targets, options, unknownOptions, showHelp, messageOnly, messagePrefix, Console.Out, Console.Error, false);
    }
}
