Option Explicit On
Option Strict On
Imports BESHStatNG.AppInfrastructure

Namespace Multivariate

    Friend Module MultivariateInputHelpers

        Friend Sub ValidateRectangularData(data(,) As Double,
                                           Optional nullMessage As String = "Input data must not be Nothing.",
                                           Optional rankMessage As String = "Input data must be a two-dimensional numeric array.",
                                           Optional emptyMessage As String = "Input data must contain at least one row and one column.",
                                           Optional nullParamName As String = Nothing)
            If data Is Nothing Then
                If String.IsNullOrEmpty(nullParamName) Then
                    AppGlobals.BSerr.LogAndThrow(New ArgumentException(nullMessage))
                ElseIf String.IsNullOrEmpty(nullMessage) Then
                    AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(nullParamName))
                Else
                    AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(nullParamName, nullMessage))
                End If
            End If

            If data.Rank <> 2 Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException(rankMessage))
            End If

            If data.GetUpperBound(0) < 0 OrElse data.GetUpperBound(1) < 0 Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException(emptyMessage))
            End If
        End Sub

        Friend Function NormalizeRowLabels(rowLabels() As String,
                                           rowCount As Integer,
                                           Optional defaultPrefix As String = "Row",
                                           Optional mismatchMessage As String = Nothing,
                                           Optional allowDefaultOnLengthMismatch As Boolean = False) As String()
            Return NormalizeLabels(rowLabels,
                                   rowCount,
                                   defaultPrefix,
                                   useSpaceSeparator:=True,
                                   mismatchMessage:=mismatchMessage,
                                   allowDefaultOnLengthMismatch:=allowDefaultOnLengthMismatch)
        End Function

        Friend Function NormalizeVarNames(varNames() As String,
                                          columnCount As Integer,
                                          Optional defaultPrefix As String = "X",
                                          Optional mismatchMessage As String = Nothing,
                                          Optional allowDefaultOnLengthMismatch As Boolean = False,
                                          Optional useSpaceSeparator As Boolean = False) As String()
            Return NormalizeLabels(varNames,
                                   columnCount,
                                   defaultPrefix,
                                   useSpaceSeparator:=useSpaceSeparator,
                                   mismatchMessage:=mismatchMessage,
                                   allowDefaultOnLengthMismatch:=allowDefaultOnLengthMismatch)
        End Function

        Private Function NormalizeLabels(labels() As String,
                                         expectedLength As Integer,
                                         defaultPrefix As String,
                                         useSpaceSeparator As Boolean,
                                         mismatchMessage As String,
                                         allowDefaultOnLengthMismatch As Boolean) As String()
            If labels Is Nothing Then
                Return BuildDefaultLabels(expectedLength, defaultPrefix, useSpaceSeparator)
            End If

            If labels.Length = expectedLength Then
                Return CType(labels.Clone(), String())
            End If

            If allowDefaultOnLengthMismatch Then
                Return BuildDefaultLabels(expectedLength, defaultPrefix, useSpaceSeparator)
            End If

            AppGlobals.BSerr.LogAndThrow(New ArgumentException(If(mismatchMessage,
                                                                 $"The number of labels does not match the expected length ({expectedLength}).")))
            Return Nothing
        End Function

        Private Function BuildDefaultLabels(count As Integer,
                                            prefix As String,
                                            useSpaceSeparator As Boolean) As String()
            Dim labels(count - 1) As String
            Dim separator As String = If(useSpaceSeparator, " ", String.Empty)
            For i As Integer = 0 To count - 1
                labels(i) = $"{prefix}{separator}{i + 1}"
            Next
            Return labels
        End Function

    End Module
End Namespace
