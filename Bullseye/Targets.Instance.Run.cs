using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bullseye.Internal;

namespace Bullseye
{
    /// <summary>
    /// Provides methods for defining and running targets.
    /// </summary>
    public partial class Targets
    {
#if NET8_0_OR_GREATER
        private static readonly List<string> defaultList = [];
#else
        private static readonly List<string> defaultList = new();
#endif
        public static readonly Func<Exception, bool> defaultMessageOnly = _ => false;

        /// <summary>
        /// Runs the targets and then calls <see cref="Environment.Exit(int)"/>.
        /// Any code which follows a call to this method will not be executed.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <param name="messageOnly">
        /// A predicate which is called when an exception is thrown.
        /// Return <c>true</c> to display only the exception message instead instead of the full exception details.
        /// </param>
        /// <param name="getMessagePrefix">
        /// A function which is called for each output or diagnostic message and returns the prefix to use.
        /// If not specified or <c>null</c>, the name of the entry assembly is used, as returned by <see cref="Assembly.GetEntryAssembly"/>.
        /// If the entry assembly is <c>null</c>, the default prefix "Bullseye" is used.
        /// </param>
        /// <param name="outputWriter">The <see cref="TextWriter"/> to use for writing output messages. Defaults to <see cref="Console.Out"/>.</param>
        /// <param name="diagnosticsWriter">The <see cref="TextWriter"/> to use for writing diagnostic messages. Defaults to <see cref="Console.Error"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous running of the targets.</returns>
        public async Task RunAndExitAsync(
            IEnumerable<string> args,
            Func<Exception, bool>? messageOnly = null,
            Func<string>? getMessagePrefix = null,
            TextWriter? outputWriter = null,
            TextWriter? diagnosticsWriter = null) =>
            await this.targetCollection.RunAsync(
                args.ToList(),
                messageOnly ?? defaultMessageOnly,
                getMessagePrefix ?? await GetDefaultGetMessagePrefix(diagnosticsWriter ?? Console.Error).Tax(),
                outputWriter ?? Console.Out,
                diagnosticsWriter ?? Console.Error,
                true).Tax();

        /// <summary>
        /// Runs the targets and then calls <see cref="Environment.Exit(int)"/>.
        /// Any code which follows a call to this method will not be executed.
        /// </summary>
        /// <param name="targets">The targets to run or list.</param>
        /// <param name="options">The options to use when running or listing targets.</param>
        /// <param name="unknownOptions">The unknown options specified in the command line arguments.</param>
        /// <param name="showHelp">A value indicating whether to show help.</param>
        /// <param name="messageOnly">
        /// A predicate which is called when an exception is thrown.
        /// Return <c>true</c> to display only the exception message instead instead of the full exception details.
        /// </param>
        /// <param name="getMessagePrefix">
        /// A function which is called for each output or diagnostic message and returns the prefix to use.
        /// If not specified or <c>null</c>, the name of the entry assembly is used, as returned by <see cref="Assembly.GetEntryAssembly"/>.
        /// If the entry assembly is <c>null</c>, the default prefix "Bullseye" is used.
        /// </param>
        /// <param name="outputWriter">The <see cref="TextWriter"/> to use for writing output messages. Defaults to <see cref="Console.Out"/>.</param>
        /// <param name="diagnosticsWriter">The <see cref="TextWriter"/> to use for writing diagnostic messages. Defaults to <see cref="Console.Error"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous running of the targets.</returns>
        public async Task RunAndExitAsync(
            IEnumerable<string> targets,
            IOptions options,
            IEnumerable<string>? unknownOptions = null,
            bool showHelp = false,
            Func<Exception, bool>? messageOnly = null,
            Func<string>? getMessagePrefix = null,
            TextWriter? outputWriter = null,
            TextWriter? diagnosticsWriter = null) =>
            await this.targetCollection.RunAsync(
                targets.ToList(),
                options,
                unknownOptions?.ToList() ?? defaultList,
                showHelp,
                messageOnly ?? defaultMessageOnly,
                getMessagePrefix ?? await GetDefaultGetMessagePrefix(diagnosticsWriter ?? Console.Error).Tax(),
                outputWriter ?? Console.Out,
                diagnosticsWriter ?? Console.Error,
                true).Tax();

        /// <summary>
        /// Runs the targets.
        /// In most cases, <see cref="RunAndExitAsync(IEnumerable{string}, Func{Exception, bool}, Func{string}, TextWriter, TextWriter)"/> should be used instead of this method.
        /// This method should only be used if continued code execution after running targets is specifically required.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <param name="messageOnly">
        /// A predicate which is called when an exception is thrown.
        /// Return <c>true</c> to display only the exception message instead instead of the full exception details.
        /// </param>
        /// <param name="getMessagePrefix">
        /// A function which is called for each output or diagnostic message and returns the prefix to use.
        /// If not specified or <c>null</c>, the name of the entry assembly is used, as returned by <see cref="Assembly.GetEntryAssembly"/>.
        /// If the entry assembly is <c>null</c>, the default prefix "Bullseye" is used.
        /// </param>
        /// <param name="outputWriter">The <see cref="TextWriter"/> to use for writing output messages. Defaults to <see cref="Console.Out"/>.</param>
        /// <param name="diagnosticsWriter">The <see cref="TextWriter"/> to use for writing diagnostic messages. Defaults to <see cref="Console.Error"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous running of the targets.</returns>
        public async Task RunWithoutExitingAsync(
            IEnumerable<string> args,
            Func<Exception, bool>? messageOnly = null,
            Func<string>? getMessagePrefix = null,
            TextWriter? outputWriter = null,
            TextWriter? diagnosticsWriter = null) =>
            await this.targetCollection.RunAsync(
                args.ToList(),
                messageOnly ?? defaultMessageOnly,
                getMessagePrefix ?? await GetDefaultGetMessagePrefix(diagnosticsWriter ?? Console.Error).Tax(),
                outputWriter ?? Console.Out,
                diagnosticsWriter ?? Console.Error,
                false).Tax();

        /// <summary>
        /// Runs the targets.
        /// In most cases, <see cref="RunAndExitAsync(IEnumerable{string}, IOptions, IEnumerable{string}, bool, Func{Exception, bool}, Func{string}, TextWriter, TextWriter)"/> should be used instead of this method.
        /// This method should only be used if continued code execution after running targets is specifically required.
        /// </summary>
        /// <param name="targets">The targets to run or list.</param>
        /// <param name="options">The options to use when running or listing targets.</param>
        /// <param name="unknownOptions">The unknown options specified in the command line arguments.</param>
        /// <param name="showHelp">A value indicating whether to show help.</param>
        /// <param name="messageOnly">
        /// A predicate which is called when an exception is thrown.
        /// Return <c>true</c> to display only the exception message instead instead of the full exception details.
        /// </param>
        /// <param name="getMessagePrefix">
        /// A function which is called for each output or diagnostic message and returns the prefix to use.
        /// If not specified or <c>null</c>, the name of the entry assembly is used, as returned by <see cref="Assembly.GetEntryAssembly"/>.
        /// If the entry assembly is <c>null</c>, the default prefix "Bullseye" is used.
        /// </param>
        /// <param name="outputWriter">The <see cref="TextWriter"/> to use for writing output messages. Defaults to <see cref="Console.Out"/>.</param>
        /// <param name="diagnosticsWriter">The <see cref="TextWriter"/> to use for writing diagnostic messages. Defaults to <see cref="Console.Error"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous running of the targets.</returns>
        public async Task RunWithoutExitingAsync(
            IEnumerable<string> targets,
            IOptions options,
            IEnumerable<string>? unknownOptions = null,
            bool showHelp = false,
            Func<Exception, bool>? messageOnly = null,
            Func<string>? getMessagePrefix = null,
            TextWriter? outputWriter = null,
            TextWriter? diagnosticsWriter = null) =>
            await this.targetCollection.RunAsync(
                targets.ToList(),
                options,
                unknownOptions?.ToList() ?? defaultList,
                showHelp,
                messageOnly ?? defaultMessageOnly,
                getMessagePrefix ?? await GetDefaultGetMessagePrefix(diagnosticsWriter ?? Console.Error).Tax(),
                outputWriter ?? Console.Out,
                diagnosticsWriter ?? Console.Error,
                false).Tax();

        public static async Task<Func<string>> GetDefaultGetMessagePrefix(TextWriter diagnosticsWriter)
        {
            var messagePrefix = "Bullseye";

            if (Assembly.GetEntryAssembly() is { } entryAssembly)
            {
                messagePrefix = entryAssembly.GetName().Name ?? messagePrefix;
            }
            else
            {
                await diagnosticsWriter.WriteLineAsync($"{messagePrefix}: Failed to get the entry assembly. Using default message prefix \"{messagePrefix}\".").Tax();
            }

            return () => messagePrefix;
        }
    }
}
