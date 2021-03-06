using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Wyam.Common.Configuration;
using Wyam.Common.IO;
using Wyam.Common.Meta;

namespace Wyam.CodeAnalysis
{
    /// <summary>
    /// Reads all the source files from a specified msbuild project.
    /// This module will be executed once and input documents will be ignored if a search path is
    /// specified. Otherwise, if a delegate is specified the module will be executed once per input
    /// document and the resulting output documents will be aggregated.
    /// Note that this requires the MSBuild tools to be installed (included with Visual Studio).
    /// </summary>
    /// <remarks>
    /// The output of this module is similar to executing the ReadFiles module on all source files in the project.
    /// </remarks>
    /// <metadata cref="Keys.SourceFileRoot" usage="Output" />
    /// <metadata cref="Keys.SourceFileBase" usage="Output" />
    /// <metadata cref="Keys.SourceFileExt" usage="Output" />
    /// <metadata cref="Keys.SourceFileName" usage="Output" />
    /// <metadata cref="Keys.SourceFileDir" usage="Output" />
    /// <metadata cref="Keys.SourceFilePath" usage="Output" />
    /// <metadata cref="Keys.SourceFilePathBase" usage="Output" />
    /// <metadata cref="Keys.RelativeFilePath" usage="Output" />
    /// <metadata cref="Keys.RelativeFilePathBase" usage="Output" />
    /// <metadata cref="Keys.RelativeFileDir" usage="Output" />
    /// <category>Input/Output</category>
    public class ReadProject : ReadWorkspace
    {
        /// <summary>
        /// Reads the project file at the specified path.
        /// </summary>
        /// <param name="path">The project file path.</param>
        public ReadProject(FilePath path)
            : base(path)
        {
        }

        /// <summary>
        /// Reads the project file at the specified path. This allows you to specify a different project file depending on the input.
        /// </summary>
        /// <param name="path">A delegate that returns a <c>FilePath</c> with the project file path.</param>
        public ReadProject(DocumentConfig path)
            : base(path)
        {
        }

        protected override IEnumerable<Project> GetProjects(IFile file)
        {
            MSBuildWorkspace workspace = MSBuildWorkspace.Create();
            Project project = workspace.OpenProjectAsync(file.Path.FullPath).Result;
            return new[] {project};
        }
    }
}