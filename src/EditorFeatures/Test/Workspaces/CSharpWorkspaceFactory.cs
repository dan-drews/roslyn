// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.Composition;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Editor.UnitTests.Workspaces
{
    public partial class CSharpWorkspaceFactory : TestWorkspaceFactory
    {
        /// <summary>
        /// Creates a single buffer in a workspace.
        /// </summary>
        /// <param name="lines">Lines of text, the buffer contents</param>
        public static TestWorkspace CreateWorkspaceFromLines(params string[] lines)
        {
            return CreateWorkspaceFromLines(lines, parseOptions: null, compilationOptions: null, exportProvider: null);
        }

        public static TestWorkspace CreateWorkspaceFromLines(
            string[] lines,
            CSharpParseOptions parseOptions = null,
            CSharpCompilationOptions compilationOptions = null,
            ExportProvider exportProvider = null)
        {
            var file = lines.Join(Environment.NewLine);
            return CreateWorkspaceFromFile(file, parseOptions, compilationOptions, exportProvider);
        }

        /// <param name="content">Can pass in multiple file contents: files will be named test1.cs, test2.cs, etc.</param>
        /// <param name="parseOptions">Parse the source code in interactive mode</param>
        public static TestWorkspace CreateWorkspaceFromFile(
            string file,
            CSharpParseOptions parseOptions = null,
            CSharpCompilationOptions compilationOptions = null,
            ExportProvider exportProvider = null,
            string[] metadataReferences = null)
        {
            return CreateWorkspaceFromFiles(new[] { file }, parseOptions, compilationOptions, exportProvider, metadataReferences);
        }

        /// <param name="files">Can pass in multiple file contents: files will be named test1.cs, test2.cs, etc.</param>
        public static TestWorkspace CreateWorkspaceFromFiles(
            string[] files,
            CSharpParseOptions parseOptions = null,
            CSharpCompilationOptions compilationOptions = null,
            ExportProvider exportProvider = null,
            string[] metadataReferences = null)
        {
            return CreateWorkspaceFromFiles(LanguageNames.CSharp, compilationOptions, parseOptions, files, exportProvider, metadataReferences);
        }

        /// <param name="files">Can pass in multiple file contents with individual source kind: files will be named test1.cs, test2.csx, etc.</param>
        public static TestWorkspace CreateWorkspaceFromFiles(
            string[] files,
            CSharpParseOptions[] parseOptions = null,
            CSharpCompilationOptions compilationOptions = null,
            ExportProvider exportProvider = null)
        {
            return CreateWorkspaceFromFiles(LanguageNames.CSharp, compilationOptions, parseOptions, files, exportProvider);
        }
    }
}
