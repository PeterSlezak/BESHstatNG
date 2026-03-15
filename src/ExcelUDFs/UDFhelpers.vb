Imports ExcelDna.Integration

Module UDFhelpers
    ''' <summary>
    ''' Attempts to convert a value to a finite Double. Returns Nothing if not convertible.
    ''' </summary>
    Function TryGetDouble(v As Object) As Double?
        If v Is Nothing OrElse TypeOf v Is ExcelEmpty OrElse TypeOf v Is ExcelMissing Then
            Return Nothing
        End If
        If TypeOf v Is ExcelError OrElse TypeOf v Is Boolean OrElse TypeOf v Is String Then
            Return Nothing
        End If
        Try
            Dim d As Double = Convert.ToDouble(v)
            If Double.IsNaN(d) OrElse Double.IsInfinity(d) Then Return Nothing
            Return d
        Catch
            Return Nothing
        End Try
    End Function

End Module
