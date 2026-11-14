Option Explicit On
Option Strict On

' Test-only native boundary. Compiled ONLY into an isolated, unregistered
' BESHStatNG.Test reference library, never a production/packaged assembly.
' Original UDF bodies and actual entry Core/Common/host helpers remain bound.
Namespace Global.ExcelDna.Integration
    Public NotInheritable Class ExcelDnaUtil
        Private Sub New()
        End Sub
        Public Shared WizardFalseBranchCalls As Integer
        Public Shared Function IsInFunctionWizard() As Boolean
            WizardFalseBranchCalls += 1
            Return False
        End Function
    End Class

    Public NotInheritable Class XlCall
        Private Sub New()
        End Sub
        Public Shared NativeAttempts As Integer
        Public Shared ReadOnly Property xlCoerce As Integer
            Get
                NativeAttempts += 1
                Throw New InvalidOperationException("NATIVE_EXCEL_NOT_AUTHORIZED_04A38")
            End Get
        End Property
        Public Shared Function Excel(functionNumber As Integer, ParamArray arguments() As Object) As Object
            NativeAttempts += 1
            Throw New InvalidOperationException("NATIVE_EXCEL_NOT_AUTHORIZED_04A38")
        End Function
    End Class
End Namespace
