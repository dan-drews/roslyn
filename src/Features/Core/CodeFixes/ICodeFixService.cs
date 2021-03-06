﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CodeFixes
{
    internal interface ICodeFixService
    {
        Task<FirstDiagnosticResult> GetFirstDiagnosticWithFixAsync(Document document, TextSpan textSpan, CancellationToken cancellationToken);

        Task<IEnumerable<CodeFixCollection>> GetFixesAsync(Document document, TextSpan textSpan, CancellationToken cancellationToken);
    }
}
