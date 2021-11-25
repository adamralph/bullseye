using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Bullseye
{
    /// <summary>
    /// Provides methods for defining and running targets.
    /// </summary>
    public partial class Targets
    {
        /// <summary>
        /// Runs the previously specified targets and then calls <see cref="Environment.Exit(int)"/>.
        /// Any code which follows a call to this method will not be executed.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <param name="messageOnly">
        /// A predicate which is called when an exception is thrown.
        /// Return <c>true</c> to display only the exception message instead instead of the full exception details.
        /// </param>
        /// <param name="getMessagePrefix">
        /// A function which is called for each output or diagnostic message and returns the prefix to use.
        /// If not specified or <c>null</c>, the name of the entry assembly is used, as returned by <see cref="System.Reflection.Assembly.GetEntryAssembly"/>.
        /// If the entry assembly is <c>null</c>, the default prefix "Bullseye" is used.
        /// </param>
        /// <param name="outputWriter">The <see cref="TextWriter"/> to use for writing output messages. Defaults to <see cref="Console.Out"/>.</param>
        /// <param name="diagnosticsWriter">The <see cref="TextWriter"/> to use for writing diagnostic messages. Defaults to <see cref="Console.Error"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous running of the targets.</returns>
        public static Task RunTargetsAndExitAsync(
            IEnumerable<string> args,
            Func<Exception, bool>? messageOnly = null,
            Func<string>? getMessagePrefix = null,
            TextWriter? outputWriter = null,
            TextWriter? diagnosticsWriter = null) =>
            instance.RunAndExitAsync(args, messageOnly, getMessagePrefix, outputWriter, diagnosticsWriter);

        /// <summary>
        /// Runs the previously specified targets and then calls <see cref="Environment.Exit(int)"/>.
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
        /// If not specified or <c>null</c>, the name of the entry assembly is used, as returned by <see cref="System.Reflection.Assembly.GetEntryAssembly"/>.
        /// If the entry assembly is <c>null</c>, the default prefix "Bullseye" is used.
        /// </param>
        /// <param name="outputWriter">The <see cref="TextWriter"/> to use for writing output messages. Defaults to <see cref="Console.Out"/>.</param>
        /// <param name="diagnosticsWriter">The <see cref="TextWriter"/> to use for writing diagnostic messages. Defaults to <see cref="Console.Error"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous running of the targets.</returns>
        public static Task RunTargetsAndExitAsync(
            IEnumerable<string> targets,
            IOptions options,
            IEnumerable<string>? unknownOptions = null,
            bool showHelp = false,
            Func<Exception, bool>? messageOnly = null,
            Func<string>? getMessagePrefix = null,
            TextWriter? outputWriter = null,
            TextWriter? diagnosticsWriter = null) =>
            instance.RunAndExitAsync(targets, options, unknownOptions, showHelp, messageOnly, getMessagePrefix, outputWriter, diagnosticsWriter);

        /// <summary>
        /// Runs the previously specified targets.
        /// In most cases, <see cref="RunTargetsAndExitAsync(IEnumerable{string}, Func{Exception, bool}, Func{string}, TextWriter, TextWriter)"/> should be used instead of this method.
        /// This method should only be used if continued code execution after running targets is specifically required.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <param name="messageOnly">
        /// A predicate which is called when an exception is thrown.
        /// Return <c>true</c> to display only the exception message instead instead of the full exception details.
        /// </param>
        /// <param name="getMessagePrefix">
        /// A function which is called for each output or diagnostic message and returns the prefix to use.
        /// If not specified or <c>null</c>, the name of the entry assembly is used, as returned by <see cref="System.Reflection.Assembly.GetEntryAssembly"/>.
        /// If the entry assembly is <c>null</c>, the default prefix "Bullseye" is used.
        /// </param>
        /// <param name="outputWriter">The <see cref="TextWriter"/> to use for writing output messages. Defaults to <see cref="Console.Out"/>.</param>
        /// <param name="diagnosticsWriter">The <see cref="TextWriter"/> to use for writing diagnostic messages. Defaults to <see cref="Console.Error"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous running of the targets.</returns>
        public static Task RunTargetsWithoutExitingAsync(
            IEnumerable<string> args,
            Func<Exception, bool>? messageOnly = null,
            Func<string>? getMessagePrefix = null,
            TextWriter? outputWriter = null,
            TextWriter? diagnosticsWriter = null) =>
            instance.RunWithoutExitingAsync(args, messageOnly, getMessagePrefix, outputWriter, diagnosticsWriter);

        /// <summary>
        /// Runs the previously specified targets.
        /// In most cases, <see cref="RunTargetsAndExitAsync(IEnumerable{string}, IOptions, IEnumerable{string}, bool, Func{Exception, bool}, Func{string}, TextWriter, TextWriter)"/> should be used instead of this method.
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
        /// If not specified or <c>null</c>, the name of the entry assembly is used, as returned by <see cref="System.Reflection.Assembly.GetEntryAssembly"/>.
        /// If the entry assembly is <c>null</c>, the default prefix "Bullseye" is used.
        /// </param>
        /// <param name="outputWriter">The <see cref="TextWriter"/> to use for writing output messages. Defaults to <see cref="Console.Out"/>.</param>
        /// <param name="diagnosticsWriter">The <see cref="TextWriter"/> to use for writing diagnostic messages. Defaults to <see cref="Console.Error"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous running of the targets.</returns>
        public static Task RunTargetsWithoutExitingAsync(
            IEnumerable<string> targets,
            IOptions options,
            IEnumerable<string>? unknownOptions = null,
            bool showHelp = false,
            Func<Exception, bool>? messageOnly = null,
            Func<string>? getMessagePrefix = null,
            TextWriter? outputWriter = null,
            TextWriter? diagnosticsWriter = null) =>
            instance.RunWithoutExitingAsync(targets, options, unknownOptions, showHelp, messageOnly, getMessagePrefix, outputWriter, diagnosticsWriter);
    }
}
