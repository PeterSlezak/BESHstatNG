Option Explicit On
Option Strict On

Imports System
Imports BESHStatNG.AppInfrastructure

Namespace Matrix

    ''' <summary>
    ''' Host-neutral statistical matrix helpers used by the legacy Matrix facade.
    ''' This module contains covariance/centering, JK eigensystem, and the small
    ''' regression helpers that historically lived in Matrix.vb.
    ''' </summary>
    Friend Module MatrixStatisticsCore

        Public NotInheritable Class EigenResult
            Public Property Eigenvalues As Double()
            Public Property Eigenvectors As Double(,)
        End Class

        Public Function SampleCovariance(matrix(,) As Double) As Double(,)
            Dim rowCount As Integer = matrix.GetLength(0)
            Dim columnCount As Integer = matrix.GetLength(1)

            ' Enumerable.Average() on the legacy extracted column throws for zero rows.
            ' Preserve that contract explicitly without depending on LINQ/VB helpers.
            If rowCount = 0 AndAlso columnCount > 0 Then
                Throw New InvalidOperationException("Sequence contains no elements")
            End If

            Dim means(columnCount - 1) As Double
            Dim covariance(columnCount - 1, columnCount - 1) As Double

            For column As Integer = 0 To columnCount - 1
                Dim sum As Double = 0.0R
                For row As Integer = 0 To rowCount - 1
                    sum += matrix(row, column)
                Next
                If rowCount > 0 Then means(column) = sum / rowCount
            Next

            For i As Integer = 0 To columnCount - 1
                For j As Integer = i To columnCount - 1
                    Dim crossProduct As Double = 0.0R
                    For row As Integer = 0 To rowCount - 1
                        crossProduct += (matrix(row, i) - means(i)) * (matrix(row, j) - means(j))
                    Next

                    Dim value As Double = 0.0R
                    If rowCount > 1 Then value = crossProduct / (rowCount - 1)

                    covariance(i, j) = value
                    covariance(j, i) = value
                Next
            Next

            Return covariance
        End Function

        Public Function DoubleCenter(matrix(,) As Double) As Double(,)
            Dim rowUpperBound As Integer = matrix.GetUpperBound(0)
            Dim columnUpperBound As Integer = matrix.GetUpperBound(1)

            If rowUpperBound <> columnUpperBound Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("Input matrix is not square (p x p)"))
            End If

            Dim rowCount As Integer = matrix.GetLength(0)
            Dim columnCount As Integer = matrix.GetLength(1)
            Dim rowMeans(rowCount - 1) As Double
            Dim columnMeans(columnCount - 1) As Double
            Dim totalSum As Double = 0.0R

            For row As Integer = 0 To rowCount - 1
                For column As Integer = 0 To columnCount - 1
                    Dim value As Double = matrix(row, column)
                    rowMeans(row) += value
                    columnMeans(column) += value
                    totalSum += value
                Next
            Next

            For row As Integer = 0 To rowCount - 1
                rowMeans(row) /= columnCount
            Next

            For column As Integer = 0 To columnCount - 1
                columnMeans(column) /= rowCount
            Next

            Dim totalMean As Double = totalSum / (rowCount * columnCount)
            Dim output(rowCount - 1, columnCount - 1) As Double

            For row As Integer = 0 To rowCount - 1
                For column As Integer = 0 To columnCount - 1
                    output(row, column) = matrix(row, column) - rowMeans(row) - columnMeans(column) + totalMean
                Next
            Next

            Return output
        End Function

        Public Function EigenJk(matrix(,) As Double,
                                Optional maxIterations As Integer = 20,
                                Optional epsilon As Double = 0.0000000001R) As EigenResult

            Dim iteration As Integer
            Dim cotangent2 As Double
            Dim temporary As Double
            Dim sine2 As Double
            Dim cosine2 As Double
            Dim tangent2 As Double
            Dim working(,) As Double = DirectCast(matrix.Clone(), Double(,))
            Dim upperBound As Integer = working.GetUpperBound(0)

            For iteration = 1 To maxIterations
                Dim maxAbsoluteNumerator As Double = 0.0R

                For j As Integer = 0 To upperBound - 1
                    For k As Integer = j + 1 To upperBound
                        Dim denominator As Double = 0.0R
                        Dim numerator As Double = 0.0R

                        For i As Integer = 0 To upperBound
                            numerator += 2.0R * working(i, j) * working(i, k)
                            denominator += (working(i, j) + working(i, k)) * (working(i, j) - working(i, k))
                        Next

                        maxAbsoluteNumerator = Math.Max(maxAbsoluteNumerator, Math.Abs(numerator))

                        If Math.Abs(numerator) < epsilon Then Continue For

                        If Math.Abs(numerator) <= Math.Abs(denominator) Then
                            tangent2 = Math.Abs(numerator) / Math.Abs(denominator)
                            cosine2 = 1.0R / Math.Sqrt(1.0R + tangent2 * tangent2)
                            sine2 = tangent2 * cosine2
                        Else
                            cotangent2 = Math.Abs(denominator) / Math.Abs(numerator)
                            sine2 = 1.0R / Math.Sqrt(1.0R + cotangent2 * cotangent2)
                            cosine2 = cotangent2 * sine2
                        End If

                        Dim cosine As Double = Math.Sqrt((1.0R + cosine2) / 2.0R)
                        Dim sine As Double = sine2 / (2.0R * cosine)

                        If denominator < 0.0R Then
                            temporary = cosine
                            cosine = sine
                            sine = temporary
                        End If

                        sine = Math.Sign(numerator) * sine

                        For i As Integer = 0 To upperBound
                            temporary = working(i, j)
                            working(i, j) = temporary * cosine + working(i, k) * sine
                            working(i, k) = -temporary * sine + working(i, k) * cosine
                        Next
                    Next
                Next

                If maxAbsoluteNumerator < epsilon AndAlso iteration > 1 Then Exit For
            Next

            If iteration >= maxIterations Then
                CoreServices.Log("JK Iteration has not converged.", LogMsgType.Warn)
            End If

            Dim eigenvalues(upperBound) As Double
            Dim eigenvectors(upperBound, upperBound) As Double

            For j As Integer = 0 To upperBound
                For k As Integer = 0 To upperBound
                    eigenvalues(j) += working(k, j) * working(k, j)
                Next
                eigenvalues(j) = Math.Sqrt(eigenvalues(j))

                For i As Integer = 0 To upperBound
                    eigenvectors(i, j) = If(eigenvalues(j) <= 0.0R, 0.0R, working(i, j) / eigenvalues(j))
                Next
            Next

            Return New EigenResult With {
                .Eigenvalues = eigenvalues,
                .Eigenvectors = eigenvectors
            }
        End Function

        Public Function FitLinearRegression(response() As Double,
                                            predictors(,) As Double,
                                            includeIntercept As Boolean) As Double(,)
            Dim errorSumSquares As Double = 0.0R
            Dim design(,) As Double

            CoreServices.Log("RegrL execution start")

            Dim rowUpperBound As Integer = predictors.GetUpperBound(0)
            Dim predictorUpperBound As Integer = predictors.GetUpperBound(1)
            Dim responseMatrix(rowUpperBound, 0) As Double

            If includeIntercept Then
                predictorUpperBound += 1
                ReDim design(rowUpperBound, predictorUpperBound)

                For i As Integer = 0 To rowUpperBound
                    For j As Integer = 0 To predictorUpperBound
                        If j = 0 Then
                            design(i, j) = 1.0R
                        Else
                            design(i, j) = predictors(i, j - 1)
                        End If
                    Next
                Next
            Else
                design = DirectCast(predictors.Clone(), Double(,))
            End If

            For i As Integer = 0 To rowUpperBound
                responseMatrix(i, 0) = response(i)
            Next

            Dim scaledDesign(rowUpperBound, predictorUpperBound) As Double
            Dim columnScale(predictorUpperBound) As Double
            Dim inverseColumnScale(predictorUpperBound) As Double

            For j As Integer = 0 To predictorUpperBound
                If includeIntercept AndAlso j = 0 Then
                    columnScale(j) = 1.0R
                Else
                    Dim sumSquares As Double = 0.0R
                    For i As Integer = 0 To rowUpperBound
                        sumSquares += design(i, j) * design(i, j)
                    Next

                    columnScale(j) = Math.Sqrt(sumSquares)
                    If columnScale(j) = 0.0R Then columnScale(j) = 1.0R
                End If

                inverseColumnScale(j) = 1.0R / columnScale(j)

                For i As Integer = 0 To rowUpperBound
                    scaledDesign(i, j) = design(i, j) * inverseColumnScale(j)
                Next
            Next

            Dim scaledPseudoInverse(,) As Double = MatrixDecompositionCore.ComputePseudoInverse(scaledDesign, 0.0R)
            Dim gammaEstimate(,) As Double = MatrixArithmeticCore.Multiply(scaledPseudoInverse, responseMatrix)

            Const MaxRefinementIterations As Integer = 6

            For iteration As Integer = 1 To MaxRefinementIterations
                Dim fitted(,) As Double = MatrixArithmeticCore.Multiply(scaledDesign, gammaEstimate)
                Dim residuals(rowUpperBound, 0) As Double

                For i As Integer = 0 To rowUpperBound
                    residuals(i, 0) = response(i) - fitted(i, 0)
                Next

                Dim correction(,) As Double = MatrixArithmeticCore.Multiply(scaledPseudoInverse, residuals)
                Dim maxAbsoluteGamma As Double = 0.0R
                Dim maxAbsoluteCorrection As Double = 0.0R

                For j As Integer = 0 To predictorUpperBound
                    gammaEstimate(j, 0) += correction(j, 0)

                    Dim absoluteGamma As Double = Math.Abs(gammaEstimate(j, 0))
                    Dim absoluteCorrection As Double = Math.Abs(correction(j, 0))

                    If absoluteGamma > maxAbsoluteGamma Then maxAbsoluteGamma = absoluteGamma
                    If absoluteCorrection > maxAbsoluteCorrection Then maxAbsoluteCorrection = absoluteCorrection
                Next

                If maxAbsoluteCorrection <= 0.00000000000001R * Math.Max(1.0R, maxAbsoluteGamma) Then Exit For
            Next

            Dim parameterEstimate(predictorUpperBound, 0) As Double
            For j As Integer = 0 To predictorUpperBound
                parameterEstimate(j, 0) = gammaEstimate(j, 0) * inverseColumnScale(j)
            Next

            Dim inverseXtXScaled(,) As Double = MatrixArithmeticCore.Multiply(
                scaledPseudoInverse,
                MatrixArithmeticCore.Transpose(scaledPseudoInverse))
            Dim inverseXtX(predictorUpperBound, predictorUpperBound) As Double

            For i As Integer = 0 To predictorUpperBound
                For j As Integer = 0 To predictorUpperBound
                    inverseXtX(i, j) = inverseXtXScaled(i, j) * inverseColumnScale(i) * inverseColumnScale(j)
                Next
            Next

            Dim fittedFinal(,) As Double = MatrixArithmeticCore.Multiply(design, parameterEstimate)
            For i As Integer = 0 To rowUpperBound
                Dim residual As Double = response(i) - fittedFinal(i, 0)
                errorSumSquares += residual * residual
            Next

            Dim varianceCovariance(,) As Double = MatrixArithmeticCore.Multiply(
                inverseXtX,
                errorSumSquares / (rowUpperBound - predictorUpperBound))

            Dim output(predictorUpperBound, 1) As Double
            For i As Integer = 0 To predictorUpperBound
                output(i, 0) = parameterEstimate(i, 0)
                output(i, 1) = Math.Sqrt(Math.Max(0.0R, varianceCovariance(i, i)))
            Next

            CoreServices.Log("RegrL execution end")
            Return output
        End Function

        Public Function FitWeightedLeastSquares(response() As Double,
                                                predictors(,) As Double,
                                                weights() As Double) As Double(,)
            Dim rowUpperBound As Integer = weights.GetUpperBound(0)
            Dim predictorUpperBound As Integer = predictors.GetUpperBound(1)
            Dim squareRootWeight(rowUpperBound) As Double
            Dim weightedResponse(rowUpperBound) As Double
            Dim weightedPredictors(rowUpperBound, predictorUpperBound) As Double

            For i As Integer = 0 To rowUpperBound
                squareRootWeight(i) = If(weights(i) > 0.0R, Math.Sqrt(weights(i)), 0.0R)
                weightedResponse(i) = squareRootWeight(i) * response(i)

                For j As Integer = 0 To predictorUpperBound
                    weightedPredictors(i, j) = predictors(i, j) * squareRootWeight(i)
                Next
            Next

            Return FitLinearRegression(weightedResponse, weightedPredictors, False)
        End Function

    End Module

End Namespace
