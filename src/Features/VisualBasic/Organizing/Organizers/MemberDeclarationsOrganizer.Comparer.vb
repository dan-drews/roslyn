' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.VisualBasic.Symbols
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.CodeAnalysis.VisualBasic.Utilities

Namespace Microsoft.CodeAnalysis.VisualBasic.Organizing.Organizers
    Partial Class MemberDeclarationsOrganizer
        Class Comparer
            Implements IComparer(Of StatementSyntax)
            ' TODO(cyrusn): Allow users to specify the ordering they want
            Enum OuterOrdering
                Fields
                EventFields
                Constructors
                Destructors
                Properties
                Events
                Indexers
                Operators
                ConversionOperators
                Methods
                Types
                Remaining
            End Enum

            Enum InnerOrdering
                StaticInstance
                Accessibility
                Name
            End Enum

            Enum Accessibility
                [Public]
                [Protected]
                [Friend]
                [Private]
            End Enum

            Public Function Compare(x As StatementSyntax, y As StatementSyntax) As Integer Implements IComparer(Of StatementSyntax).Compare
                If x Is y Then
                    Return 0
                End If

                Dim xOuterOrdering = GetOuterOrdering(x)
                Dim yOuterOrdering = GetOuterOrdering(y)

                Dim value = xOuterOrdering - yOuterOrdering
                If value <> 0 Then
                    Return value
                End If

                If xOuterOrdering = OuterOrdering.Remaining Then
                    Return 1
                ElseIf yOuterOrdering = OuterOrdering.Remaining Then
                    Return -1
                End If

                If xOuterOrdering = OuterOrdering.Fields OrElse yOuterOrdering = OuterOrdering.Fields Then
                    ' Fields with initializers can't be reordered relative to 
                    ' themselves due to ordering issues.
                    Dim xHasInitializer = DirectCast(x, FieldDeclarationSyntax).Declarators.Any(Function(v) v.Initializer IsNot Nothing)
                    Dim yHasInitializer = DirectCast(y, FieldDeclarationSyntax).Declarators.Any(Function(v) v.Initializer IsNot Nothing)
                    If xHasInitializer AndAlso yHasInitializer Then
                        Return 0
                    End If
                End If

                Dim xIsShared = x.GetModifiers().Any(Function(t) t.Kind = SyntaxKind.SharedKeyword)
                Dim yIsShared = y.GetModifiers().Any(Function(t) t.Kind = SyntaxKind.SharedKeyword)

                value = Comparer(Of Boolean).Default.Inverse().Compare(xIsShared, yIsShared)
                If value <> 0 Then
                    Return value
                End If

                Dim xAccessibility = GetAccessibility(x)
                Dim yAccessibility = GetAccessibility(y)
                value = xAccessibility - yAccessibility
                If value <> 0 Then
                    Return value
                End If

                Dim xName = x.GetNameTokenOrNothing()
                Dim yName = y.GetNameTokenOrNothing()

                value = TokenComparer.NormalInstance.Compare(xName, yName)
                If value <> 0 Then
                    Return value
                End If

                ' Their names were the same.  Order them by arity at this point.
                Return x.GetArity() - y.GetArity()
            End Function

            Private Function GetAccessibility(x As StatementSyntax) As Accessibility
                Dim xModifiers = x.GetModifiers()

                If xModifiers.Any(Function(t) t.Kind = SyntaxKind.PublicKeyword) Then
                    Return Accessibility.Public
                ElseIf xModifiers.Any(Function(t) t.Kind = SyntaxKind.FriendKeyword) Then
                    Return Accessibility.Friend
                ElseIf xModifiers.Any(Function(t) t.Kind = SyntaxKind.ProtectedKeyword) Then
                    Return Accessibility.Protected
                Else
                    ' Only fields are private in VB. All other members are public
                    If x.Kind = SyntaxKind.FieldDeclaration Then
                        Return Accessibility.Private
                    Else
                        Return Accessibility.Public
                    End If
                End If
            End Function

            Private Function GetOuterOrdering(x As StatementSyntax) As OuterOrdering
                Select Case x.Kind
                    Case SyntaxKind.FieldDeclaration
                        Return OuterOrdering.Fields
                    Case SyntaxKind.ConstructorBlock
                        Return OuterOrdering.Constructors
                    Case SyntaxKind.PropertyBlock
                        Return OuterOrdering.Properties
                    Case SyntaxKind.EventBlock
                        Return OuterOrdering.Events
                    Case SyntaxKind.OperatorBlock
                        Return OuterOrdering.Operators
                    Case SyntaxKind.SubBlock,
                        SyntaxKind.FunctionBlock
                        Return OuterOrdering.Methods
                    Case SyntaxKind.ClassBlock,
                        SyntaxKind.InterfaceBlock,
                        SyntaxKind.StructureBlock,
                        SyntaxKind.EnumBlock,
                        SyntaxKind.DelegateSubStatement,
                        SyntaxKind.DelegateFunctionStatement
                        Return OuterOrdering.Types
                    Case Else
                        Return OuterOrdering.Remaining
                End Select
            End Function
        End Class
    End Class
End Namespace
