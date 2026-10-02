Option Explicit On
Option Strict On

Namespace AppInfrastructure

    ''' <summary>
    ''' Centralized numeric predicates and validation guards used across the add-in.
    ''' Keeps small validation helpers in one place so statistical, UDF, and resampling code
    ''' can share the same semantics and exception messages.
    ''' </summary>
    Public Module NumericGuards

        Friend Function IsFinite(value As Double) As Boolean
            Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
        End Function

        Friend Function IsClosedUnitInterval(value As Double) As Boolean
            Return IsFinite(value) AndAlso value >= 0.0R AndAlso value <= 1.0R
        End Function

        Friend Function IsOpenUnitInterval(value As Double) As Boolean
            Return IsFinite(value) AndAlso value > 0.0R AndAlso value < 1.0R
        End Function

        Friend Function IsHalfOpenUnitInterval(value As Double) As Boolean
            Return IsFinite(value) AndAlso value >= 0.0R AndAlso value < 1.0R
        End Function

        Friend Function NormalizeAlpha(alpha As Double) As Double
            If Not IsFinite(alpha) OrElse alpha <= 0.0 OrElse alpha >= 1.0 Then Return 0.05
            Return alpha
        End Function

        Friend Function ClampProbability(value As Double) As Double
            If Double.IsNaN(value) Then Return Double.NaN
            If value <= 0.0R Then Return 0.0R
            If value >= 1.0R Then Return 1.0R
            Return value
        End Function

        Friend Sub ValidateFinite(value As Double, paramName As String, Optional message As String = "Value must be finite.")
            If Not IsFinite(value) Then
                Throw New ArgumentOutOfRangeException(paramName, message)
            End If
        End Sub

        Friend Sub ValidateOpenUnitInterval(value As Double, paramName As String,
                                            Optional message As String = "Value must satisfy 0 < value < 1.")
            If Not IsOpenUnitInterval(value) Then
                Throw New ArgumentOutOfRangeException(paramName, message)
            End If
        End Sub

        Friend Sub ValidateClosedUnitInterval(value As Double, paramName As String,
                                              Optional message As String = "Value must satisfy 0 <= value <= 1.")
            If Not IsClosedUnitInterval(value) Then
                Throw New ArgumentOutOfRangeException(paramName, message)
            End If
        End Sub

        Friend Sub ValidateUnitIntervalExcludingOne(value As Double, paramName As String,
                                                    Optional message As String = "Value must satisfy 0 <= value < 1.")
            If Not IsHalfOpenUnitInterval(value) Then
                Throw New ArgumentOutOfRangeException(paramName, message)
            End If
        End Sub

        Friend Sub ValidatePositive(value As Double, paramName As String,
                                    Optional message As String = "Value must be positive.")
            If Not IsFinite(value) OrElse value <= 0.0R Then
                Throw New ArgumentOutOfRangeException(paramName, message)
            End If
        End Sub

        Friend Sub ValidateAlpha(alpha As Double, Optional paramName As String = "alpha")
            ValidateOpenUnitInterval(alpha, paramName, "Alpha must lie in the open interval (0, 1).")
        End Sub

        Friend Sub ValidateAlphaOneSided(value As Double, argumentName As String)
            ValidateOpenUnitInterval(value, argumentName)
            If (2.0R * value) >= 1.0R Then
                Throw New ArgumentOutOfRangeException(argumentName, "For CI-based margin reporting, one-sided alpha must be less than 0.5.")
            End If
        End Sub

        Friend Sub ValidateProbability(probability As Double, paramName As String)
            ValidateClosedUnitInterval(probability, paramName,
                                       "Probability must lie in the closed interval [0,1].")
        End Sub

        Friend Sub ValidatePositiveReplicates(value As Integer, paramName As String, minimum As Integer)
            If value < minimum Then
                Throw New ArgumentOutOfRangeException(paramName, $"Value must be >= {minimum}.")
            End If
        End Sub

        Friend Sub ValidatePositiveLong(value As Long, paramName As String, minimum As Long)
            If value < minimum Then
                Throw New ArgumentOutOfRangeException(paramName, $"Value must be >= {minimum}.")
            End If
        End Sub

        Friend Sub ValidateFiniteStatistics(values As Double(), paramName As String,
                                            Optional emptyMessage As String = "At least one statistic is required.",
                                            Optional nonFiniteMessage As String = "Resampling statistics must contain only finite values.")
            If values Is Nothing Then Throw New ArgumentNullException(paramName)
            If values.Length = 0 Then Throw New ArgumentException(emptyMessage, paramName)

            For i As Integer = 0 To values.Length - 1
                If Not IsFinite(values(i)) Then
                    Throw New ArgumentException(nonFiniteMessage, paramName)
                End If
            Next
        End Sub

    End Module

End Namespace
