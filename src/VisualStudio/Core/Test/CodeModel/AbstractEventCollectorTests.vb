' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Editor.UnitTests.Workspaces
Imports Microsoft.VisualStudio.LanguageServices.Implementation.CodeModel

Namespace Microsoft.VisualStudio.LanguageServices.UnitTests.CodeModel
    Public MustInherit Class AbstractEventCollectorTests

        Protected MustOverride ReadOnly Property LanguageName As String

        Friend Function Add(node As String, Optional parent As String = Nothing) As Action(Of CodeModelEvent, ICodeModelService)
            Return Sub(codeModelEvent, codeModelService)
                       Assert.NotNull(codeModelEvent)

                       Assert.Equal(CodeModelEventType.Add, codeModelEvent.Type)

                       If node IsNot Nothing Then
                           Assert.NotNull(codeModelEvent.Node)
                           Assert.Equal(node, codeModelService.GetName(codeModelEvent.Node))
                       Else
                           Assert.Null(codeModelEvent.Node)
                       End If

                       If parent IsNot Nothing Then
                           Assert.NotNull(codeModelEvent.ParentNode)
                           Assert.Equal(parent, codeModelService.GetName(codeModelEvent.ParentNode))
                       Else
                           Assert.Null(codeModelEvent.ParentNode)
                       End If
                   End Sub
        End Function

        Friend Function Change(type As CodeModelEventType, node As String) As Action(Of CodeModelEvent, ICodeModelService)
            Return Sub(codeModelEvent, codeModelService)
                       Assert.NotNull(codeModelEvent)

                       Assert.Equal(type, codeModelEvent.Type)

                       If node IsNot Nothing Then
                           Assert.NotNull(codeModelEvent.Node)
                           Assert.Equal(node, codeModelService.GetName(codeModelEvent.Node))
                       Else
                           Assert.Null(codeModelEvent.Node)
                       End If
                   End Sub
        End Function

        Friend Function ArgChange(node As String) As Action(Of CodeModelEvent, ICodeModelService)
            Return Change(CodeModelEventType.ArgChange, node)
        End Function

        Friend Function BaseChange(node As String) As Action(Of CodeModelEvent, ICodeModelService)
            Return Change(CodeModelEventType.BaseChange, node)
        End Function

        Friend Function TypeRefChange(node As String) As Action(Of CodeModelEvent, ICodeModelService)
            Return Change(CodeModelEventType.TypeRefChange, node)
        End Function

        Friend Function Rename(node As String) As Action(Of CodeModelEvent, ICodeModelService)
            Return Change(CodeModelEventType.Rename, node)
        End Function

        Friend Function Unknown(node As String) As Action(Of CodeModelEvent, ICodeModelService)
            Return Change(CodeModelEventType.Unknown, node)
        End Function

        Friend Function Remove(node As String, parent As String) As Action(Of CodeModelEvent, ICodeModelService)
            Return Sub(codeModelEvent, codeModelService)
                       Assert.NotNull(codeModelEvent)

                       Assert.Equal(CodeModelEventType.Remove, codeModelEvent.Type)

                       If node IsNot Nothing Then
                           Assert.NotNull(codeModelEvent.Node)
                           Assert.Equal(node, codeModelService.GetName(codeModelEvent.Node))
                       Else
                           Assert.Null(codeModelEvent.Node)
                       End If

                       If parent IsNot Nothing Then
                           Assert.NotNull(codeModelEvent.ParentNode)
                           Assert.Equal(parent, codeModelService.GetName(codeModelEvent.ParentNode))
                       Else
                           Assert.Null(codeModelEvent.ParentNode)
                       End If
                   End Sub
        End Function

        Friend Sub Test(code As XElement, change As XElement, ParamArray expectedEvents As Action(Of CodeModelEvent, ICodeModelService)())
            Dim definition =
<Workspace>
    <Project Language=<%= LanguageName %> CommonReferences="true">
        <Document><%= code.Value %></Document>
        <Document><%= change.Value %></Document>
    </Project>
</Workspace>

            Using workspace = TestWorkspaceFactory.CreateWorkspace(definition, exportProvider:=VisualStudioTestExportProvider.ExportProvider)
                Dim project = workspace.CurrentSolution.Projects.First()
                Dim codeModelService = project.LanguageServices.GetService(Of ICodeModelService)()
                Assert.NotNull(codeModelService)

                Dim codeDocument = workspace.CurrentSolution.GetDocument(workspace.Documents(0).Id)
                Dim codeTree = codeDocument.GetSyntaxTreeAsync().Result

                Dim changeDocument = workspace.CurrentSolution.GetDocument(workspace.Documents(1).Id)
                Dim changeTree = changeDocument.GetSyntaxTreeAsync().Result

                Dim collectedEvents = codeModelService.CollectCodeModelEvents(codeTree, changeTree)
                Assert.NotNull(collectedEvents)
                Assert.Equal(expectedEvents.Length, collectedEvents.Count)

                For Each expectedEvent In expectedEvents
                    Dim collectedEvent = collectedEvents.Dequeue()
                    expectedEvent(collectedEvent, codeModelService)
                Next
            End Using
        End Sub

    End Class
End Namespace
