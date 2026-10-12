Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq

''' <summary>
''' Automatic/manual bandwidth rules supported by <see cref="ViolinPlot"/>.
''' </summary>
Public Enum ViolinBandwidthRule
    ''' <summary>
    ''' Silverman's robust normal-reference rule. The scale is based on the smaller
    ''' of the sample standard deviation and IQR/1.34, with a safe fallback for
    ''' constant samples.
    ''' </summary>
    Silverman

    ''' <summary>
    ''' Scott's normal-reference rule based on the sample standard deviation.
    ''' </summary>
    Scott

    ''' <summary>
    ''' Uses <see cref="ViolinPlotOptions.ManualBandwidth"/> directly.
    ''' </summary>
    Manual
End Enum

''' <summary>
''' Specifies how KDE values are converted to violin half-widths.
''' </summary>
Public Enum ViolinScaleMode
    ''' <summary>
    ''' Every violin is independently scaled so that its maximum density reaches
    ''' <see cref="ViolinPlotOptions.MaximumHalfWidth"/>. This emphasizes shape and
    ''' is the recommended/default comparison mode.
    ''' </summary>
    EqualMaximumWidth

    ''' <summary>
    ''' One common density-to-width factor is used for all groups. Because every KDE
    ''' integrates to approximately one, violin areas are comparable across groups.
    ''' </summary>
    EqualArea

    ''' <summary>
    ''' Each violin is independently shape-normalised and its maximum width is then
    ''' multiplied by n/max(n), so sample size is encoded by violin width.
    ''' </summary>
    Count
End Enum

''' <summary>
''' Numerical options for a categorical violin plot.
''' </summary>
Public Class ViolinPlotOptions
    ''' <summary>Bandwidth rule used for each group. Default: Silverman.</summary>
    Public Property BandwidthRule As ViolinBandwidthRule = ViolinBandwidthRule.Silverman

    ''' <summary>
    ''' Multiplicative adjustment applied to automatically selected bandwidths.
    ''' Values below one show more local detail; values above one produce smoother
    ''' densities. Ignored when <see cref="BandwidthRule"/> is Manual.
    ''' </summary>
    Public Property BandwidthAdjustment As Double = 1.0R

    ''' <summary>
    ''' Absolute KDE bandwidth used when <see cref="BandwidthRule"/> is Manual.
    ''' </summary>
    Public Property ManualBandwidth As Nullable(Of Double) = Nothing

    ''' <summary>Number of density evaluation points per group. Default: 128.</summary>
    Public Property GridPoints As Integer = 128

    ''' <summary>How density is scaled to a graphical violin width.</summary>
    Public Property ScaleMode As ViolinScaleMode = ViolinScaleMode.EqualMaximumWidth

    ''' <summary>
    ''' Maximum half-width in category-coordinate units. With groups centred at
    ''' x = 1, 2, 3, ... the default 0.4 leaves a 0.2-unit gap between adjacent
    ''' maximum-width violins.
    ''' </summary>
    Public Property MaximumHalfWidth As Double = 0.4R

    ''' <summary>
    ''' If True, non-constant group densities are evaluated only between the sample
    ''' minimum and maximum. If False, the grid extends three bandwidths beyond each
    ''' end of the observed range.
    ''' </summary>
    Public Property Trim As Boolean = True

    ''' <summary>Creates an independent copy of the numerical options.</summary>
    Friend Function Copy() As ViolinPlotOptions
        Return New ViolinPlotOptions With {
            .BandwidthRule = BandwidthRule,
            .BandwidthAdjustment = BandwidthAdjustment,
            .ManualBandwidth = ManualBandwidth,
            .GridPoints = GridPoints,
            .ScaleMode = ScaleMode,
            .MaximumHalfWidth = MaximumHalfWidth,
            .Trim = Trim
        }
    End Function
End Class

''' <summary>
''' Immutable KDE and descriptive-statistics result for one categorical level.
''' </summary>
Public NotInheritable Class ViolinPlotSeries
    Private ReadOnly _name As String
    Private ReadOnly _groupValue As Object
    Private ReadOnly _observations As Double()
    Private ReadOnly _evaluationPoints As Double()
    Private ReadOnly _density As Double()
    Private ReadOnly _scaledHalfWidths As Double()
    Private ReadOnly _bandwidth As Double
    Private ReadOnly _minimum As Double
    Private ReadOnly _q1 As Double
    Private ReadOnly _median As Double
    Private ReadOnly _q3 As Double
    Private ReadOnly _maximum As Double
    Private ReadOnly _mean As Double
    Private ReadOnly _whiskerLow As Double
    Private ReadOnly _whiskerHigh As Double

    Friend Sub New(name As String,
                   groupValue As Object,
                   observations As Double(),
                   evaluationPoints As Double(),
                   density As Double(),
                   scaledHalfWidths As Double(),
                   bandwidth As Double,
                   minimum As Double,
                   q1 As Double,
                   median As Double,
                   q3 As Double,
                   maximum As Double,
                   mean As Double,
                   whiskerLow As Double,
                   whiskerHigh As Double)
        _name = name
        _groupValue = groupValue
        _observations = DirectCast(observations.Clone(), Double())
        _evaluationPoints = DirectCast(evaluationPoints.Clone(), Double())
        _density = DirectCast(density.Clone(), Double())
        _scaledHalfWidths = DirectCast(scaledHalfWidths.Clone(), Double())
        _bandwidth = bandwidth
        _minimum = minimum
        _q1 = q1
        _median = median
        _q3 = q3
        _maximum = maximum
        _mean = mean
        _whiskerLow = whiskerLow
        _whiskerHigh = whiskerHigh
    End Sub

    Public ReadOnly Property Name As String
        Get
            Return _name
        End Get
    End Property

    Public ReadOnly Property GroupValue As Object
        Get
            Return _groupValue
        End Get
    End Property

    Public ReadOnly Property SampleSize As Integer
        Get
            Return _observations.Length
        End Get
    End Property

    Public ReadOnly Property Observations As Double()
        Get
            Return DirectCast(_observations.Clone(), Double())
        End Get
    End Property

    Public ReadOnly Property EvaluationPoints As Double()
        Get
            Return DirectCast(_evaluationPoints.Clone(), Double())
        End Get
    End Property

    Public ReadOnly Property Density As Double()
        Get
            Return DirectCast(_density.Clone(), Double())
        End Get
    End Property

    Public ReadOnly Property ScaledHalfWidths As Double()
        Get
            Return DirectCast(_scaledHalfWidths.Clone(), Double())
        End Get
    End Property

    Public ReadOnly Property Bandwidth As Double
        Get
            Return _bandwidth
        End Get
    End Property

    Public ReadOnly Property Minimum As Double
        Get
            Return _minimum
        End Get
    End Property

    Public ReadOnly Property Q1 As Double
        Get
            Return _q1
        End Get
    End Property

    Public ReadOnly Property Median As Double
        Get
            Return _median
        End Get
    End Property

    Public ReadOnly Property Q3 As Double
        Get
            Return _q3
        End Get
    End Property

    Public ReadOnly Property Maximum As Double
        Get
            Return _maximum
        End Get
    End Property

    Public ReadOnly Property Mean As Double
        Get
            Return _mean
        End Get
    End Property

    ''' <summary>Lower Tukey whisker (smallest observation not below Q1 - 1.5 IQR).</summary>
    Public ReadOnly Property WhiskerLow As Double
        Get
            Return _whiskerLow
        End Get
    End Property

    ''' <summary>Upper Tukey whisker (largest observation not above Q3 + 1.5 IQR).</summary>
    Public ReadOnly Property WhiskerHigh As Double
        Get
            Return _whiskerHigh
        End Get
    End Property
End Class

''' <summary>
''' Immutable numerical result returned by <see cref="ViolinPlotCore.Compute"/>.
''' </summary>
Public NotInheritable Class ViolinPlotResult
    Private ReadOnly _series As ViolinPlotSeries()
    Private ReadOnly _options As ViolinPlotOptions
    Private ReadOnly _yMinimum As Double
    Private ReadOnly _yMaximum As Double
    Private ReadOnly _sourceObservationCount As Integer
    Private ReadOnly _usableObservationCount As Integer
    Private ReadOnly _excludedObservationCount As Integer

    Friend Sub New(series As ViolinPlotSeries(),
                   options As ViolinPlotOptions,
                   yMinimum As Double,
                   yMaximum As Double,
                   sourceObservationCount As Integer,
                   usableObservationCount As Integer,
                   excludedObservationCount As Integer)
        _series = DirectCast(series.Clone(), ViolinPlotSeries())
        _options = options.Copy()
        _yMinimum = yMinimum
        _yMaximum = yMaximum
        _sourceObservationCount = sourceObservationCount
        _usableObservationCount = usableObservationCount
        _excludedObservationCount = excludedObservationCount
    End Sub

    Public ReadOnly Property Series As ViolinPlotSeries()
        Get
            Return DirectCast(_series.Clone(), ViolinPlotSeries())
        End Get
    End Property

    Public ReadOnly Property Options As ViolinPlotOptions
        Get
            Return _options.Copy()
        End Get
    End Property

    Public ReadOnly Property GroupCount As Integer
        Get
            Return _series.Length
        End Get
    End Property

    Public ReadOnly Property SourceObservationCount As Integer
        Get
            Return _sourceObservationCount
        End Get
    End Property

    Public ReadOnly Property UsableObservationCount As Integer
        Get
            Return _usableObservationCount
        End Get
    End Property

    Public ReadOnly Property ExcludedObservationCount As Integer
        Get
            Return _excludedObservationCount
        End Get
    End Property

    ''' <summary>
    ''' Lowest KDE grid value across groups. For an untrimmed plot this can be lower
    ''' than the smallest observation.
    ''' </summary>
    Public ReadOnly Property YMinimum As Double
        Get
            Return _yMinimum
        End Get
    End Property

    ''' <summary>
    ''' Highest KDE grid value across groups. For an untrimmed plot this can be higher
    ''' than the largest observation.
    ''' </summary>
    Public ReadOnly Property YMaximum As Double
        Get
            Return _yMaximum
        End Get
    End Property
End Class

''' <summary>
''' Computes categorical violin-plot geometry from one continuous variable and one
''' categorical variable. This calculation engine is independent of Excel and chart rendering.
''' </summary>
Public NotInheritable Class ViolinPlotCore
    Private Sub New()
    End Sub

    Private Const GaussianNormalizingConstant As Double = 0.3989422804014327R
    Private Const DensityTailBandwidths As Double = 3.0R
    Private Const IqrNormalScale As Double = 1.34R

    Private NotInheritable Class WorkingGroup
        Friend Key As String
        Friend Name As String
        Friend Value As Object
        Friend Values As New List(Of Double)()
    End Class

    Private NotInheritable Class WorkingSeries
        Friend Name As String
        Friend GroupValue As Object
        Friend Observations As Double()
        Friend EvaluationPoints As Double()
        Friend Density As Double()
        Friend ScaledHalfWidths As Double()
        Friend Bandwidth As Double
        Friend Minimum As Double
        Friend Q1 As Double
        Friend Median As Double
        Friend Q3 As Double
        Friend Maximum As Double
        Friend Mean As Double
        Friend WhiskerLow As Double
        Friend WhiskerHigh As Double
        Friend MaximumDensity As Double
        Friend DensityArea As Double
    End Class

    ''' <summary>
    ''' Computes grouped Gaussian KDEs and violin widths.
    ''' </summary>
    ''' <param name="values">
    ''' Continuous observations. Missing continuous values can be represented by NaN.
    ''' </param>
    ''' <param name="groups">
    ''' Categorical observations aligned row-by-row with <paramref name="values"/>.
    ''' Text and numeric levels are supported. Missing/blank group values are omitted.
    ''' </param>
    ''' <param name="options">Numerical KDE/scaling options.</param>
    Public Shared Function Compute(values As Double(),
                                   groups As Array,
                                   Optional options As ViolinPlotOptions = Nothing) As ViolinPlotResult
        If values Is Nothing Then Throw New ArgumentNullException(NameOf(values))
        If groups Is Nothing Then Throw New ArgumentNullException(NameOf(groups))
        If groups.Rank <> 1 Then
            Throw New ArgumentException("The categorical variable must be a one-dimensional array.", NameOf(groups))
        End If
        If groups.Length <> values.Length Then
            Throw New ArgumentException("The continuous and categorical variables must contain the same number of rows.",
                                        NameOf(groups))
        End If
        If values.Length = 0 Then
            Throw New ArgumentException("At least one observation is required.", NameOf(values))
        End If

        Dim resolvedOptions As ViolinPlotOptions = If(options, New ViolinPlotOptions()).Copy()
        ValidateOptions(resolvedOptions)

        Dim orderedGroups As New List(Of WorkingGroup)()
        Dim groupsByKey As New Dictionary(Of String, WorkingGroup)(StringComparer.Ordinal)
        Dim pooledValues As New List(Of Double)()
        Dim excludedCount As Integer = 0
        Dim groupLowerBound As Integer = groups.GetLowerBound(0)

        For i As Integer = 0 To values.Length - 1
            Dim x As Double = values(i)
            Dim groupValue As Object = groups.GetValue(groupLowerBound + i)

            If Double.IsNaN(x) OrElse IsMissingGroupValue(groupValue) Then
                excludedCount += 1
                Continue For
            End If
            If Double.IsInfinity(x) Then
                Throw New ArgumentOutOfRangeException(NameOf(values),
                                                      "Continuous observation at row " &
                                                      (i + 1).ToString(CultureInfo.CurrentCulture) &
                                                      " is infinite.")
            End If

            Dim key As String = BuildGroupKey(groupValue)
            Dim working As WorkingGroup = Nothing
            If Not groupsByKey.TryGetValue(key, working) Then
                working = New WorkingGroup With {
                    .Key = key,
                    .Name = FormatGroupName(groupValue),
                    .Value = groupValue
                }
                groupsByKey.Add(key, working)
                orderedGroups.Add(working)
            End If

            working.Values.Add(x)
            pooledValues.Add(x)
        Next

        If pooledValues.Count = 0 Then
            Throw New ArgumentException("No usable paired continuous/categorical observations were found.")
        End If
        If orderedGroups.Count = 0 Then
            Throw New ArgumentException("No usable categorical levels were found.", NameOf(groups))
        End If

        Dim pooled() As Double = pooledValues.ToArray()
        Dim pooledScale As Double = ResolveFallbackScale(pooled)
        Dim workingSeries As New List(Of WorkingSeries)(orderedGroups.Count)
        Dim globalMaximumDensity As Double = 0.0R
        Dim globalMaximumAreaNormalizedDensity As Double = 0.0R
        Dim maximumGroupN As Integer = orderedGroups.Max(Function(g) g.Values.Count)
        Dim resultYMinimum As Double = Double.PositiveInfinity
        Dim resultYMaximum As Double = Double.NegativeInfinity

        For Each group As WorkingGroup In orderedGroups
            Dim observations() As Double = group.Values.ToArray()
            Dim sorted() As Double = DirectCast(observations.Clone(), Double())
            Array.Sort(sorted)

            Dim quartileInput() As Double = DirectCast(sorted.Clone(), Double())
            Dim quartiles As udQuartiles = StatFunc.QuartilesComp(quartileInput)
            Dim minimum As Double = sorted(0)
            Dim maximum As Double = sorted(sorted.Length - 1)
            Dim mean As Double = observations.Average()
            Dim bandwidth As Double = ResolveBandwidth(observations,
                                                       quartiles.Q1,
                                                       quartiles.Q3,
                                                       pooledScale,
                                                       resolvedOptions)

            Dim evaluationMinimum As Double = minimum
            Dim evaluationMaximum As Double = maximum
            If Not resolvedOptions.Trim OrElse NearlyEqual(minimum, maximum) Then
                evaluationMinimum = minimum - DensityTailBandwidths * bandwidth
                evaluationMaximum = maximum + DensityTailBandwidths * bandwidth
            End If
            If Not IsFinite(evaluationMinimum) OrElse Not IsFinite(evaluationMaximum) OrElse
               evaluationMinimum >= evaluationMaximum Then
                evaluationMinimum = minimum - bandwidth
                evaluationMaximum = maximum + bandwidth
            End If

            Dim evaluationPoints() As Double = CreateLinearGrid(evaluationMinimum,
                                                                evaluationMaximum,
                                                                resolvedOptions.GridPoints)
            Dim density() As Double = ComputeGaussianDensity(observations,
                                                             evaluationPoints,
                                                             bandwidth)
            Dim maximumDensity As Double = density.Max()
            Dim densityArea As Double = TrapezoidalArea(evaluationPoints, density)
            If Not IsFinitePositive(maximumDensity) Then
                Throw New InvalidOperationException("Kernel-density computation produced a non-positive maximum density for group '" &
                                                    group.Name & "'.")
            End If
            If Not IsFinitePositive(densityArea) Then
                Throw New InvalidOperationException("Kernel-density computation produced a non-positive displayed density area for group '" &
                                                    group.Name & "'.")
            End If

            Dim whiskerLow As Double
            Dim whiskerHigh As Double
            StatFunc.ResolveTukeyWhiskers(sorted,
                                          quartiles.Q1,
                                          quartiles.Q3,
                                          whiskerLow,
                                          whiskerHigh)

            Dim ws As New WorkingSeries With {
                .Name = group.Name,
                .GroupValue = group.Value,
                .Observations = observations,
                .EvaluationPoints = evaluationPoints,
                .Density = density,
                .Bandwidth = bandwidth,
                .Minimum = minimum,
                .Q1 = quartiles.Q1,
                .Median = quartiles.Median,
                .Q3 = quartiles.Q3,
                .Maximum = maximum,
                .Mean = mean,
                .WhiskerLow = whiskerLow,
                .WhiskerHigh = whiskerHigh,
                .MaximumDensity = maximumDensity,
                .DensityArea = densityArea
            }
            workingSeries.Add(ws)

            If maximumDensity > globalMaximumDensity Then globalMaximumDensity = maximumDensity
            Dim areaNormalizedMaximum As Double = maximumDensity / densityArea
            If areaNormalizedMaximum > globalMaximumAreaNormalizedDensity Then
                globalMaximumAreaNormalizedDensity = areaNormalizedMaximum
            End If
            If evaluationMinimum < resultYMinimum Then resultYMinimum = evaluationMinimum
            If evaluationMaximum > resultYMaximum Then resultYMaximum = evaluationMaximum
        Next

        If Not IsFinitePositive(globalMaximumDensity) Then
            Throw New InvalidOperationException("Kernel-density computation produced no usable density values.")
        End If

        Dim output(workingSeries.Count - 1) As ViolinPlotSeries
        For groupIndex As Integer = 0 To workingSeries.Count - 1
            Dim ws As WorkingSeries = workingSeries(groupIndex)
            ws.ScaledHalfWidths = ScaleDensity(ws.Density,
                                               ws.MaximumDensity,
                                               ws.DensityArea,
                                               globalMaximumAreaNormalizedDensity,
                                               ws.Observations.Length,
                                               maximumGroupN,
                                               resolvedOptions)

            output(groupIndex) = New ViolinPlotSeries(ws.Name,
                                                      ws.GroupValue,
                                                      ws.Observations,
                                                      ws.EvaluationPoints,
                                                      ws.Density,
                                                      ws.ScaledHalfWidths,
                                                      ws.Bandwidth,
                                                      ws.Minimum,
                                                      ws.Q1,
                                                      ws.Median,
                                                      ws.Q3,
                                                      ws.Maximum,
                                                      ws.Mean,
                                                      ws.WhiskerLow,
                                                      ws.WhiskerHigh)
        Next

        Return New ViolinPlotResult(output,
                                    resolvedOptions,
                                    resultYMinimum,
                                    resultYMaximum,
                                    values.Length,
                                    pooledValues.Count,
                                    excludedCount)
    End Function

    Private Shared Sub ValidateOptions(options As ViolinPlotOptions)
        If Not [Enum].IsDefined(GetType(ViolinBandwidthRule), options.BandwidthRule) Then
            Throw New ArgumentOutOfRangeException(NameOf(options.BandwidthRule), "The violin bandwidth rule is not defined.")
        End If
        If Not [Enum].IsDefined(GetType(ViolinScaleMode), options.ScaleMode) Then
            Throw New ArgumentOutOfRangeException(NameOf(options.ScaleMode), "The violin scale mode is not defined.")
        End If
        If options.GridPoints < 32 OrElse options.GridPoints > 1024 Then
            Throw New ArgumentOutOfRangeException(NameOf(options.GridPoints),
                                                  "GridPoints must be between 32 and 1024.")
        End If
        If Not IsFinitePositive(options.MaximumHalfWidth) OrElse options.MaximumHalfWidth >= 0.5R Then
            Throw New ArgumentOutOfRangeException(NameOf(options.MaximumHalfWidth),
                                                  "MaximumHalfWidth must be finite, positive, and smaller than 0.5 category units.")
        End If
        If Not IsFinitePositive(options.BandwidthAdjustment) Then
            Throw New ArgumentOutOfRangeException(NameOf(options.BandwidthAdjustment),
                                                  "BandwidthAdjustment must be finite and positive.")
        End If
        If options.BandwidthRule = ViolinBandwidthRule.Manual Then
            If Not options.ManualBandwidth.HasValue OrElse Not IsFinitePositive(options.ManualBandwidth.Value) Then
                Throw New ArgumentOutOfRangeException(NameOf(options.ManualBandwidth),
                                                      "A finite positive ManualBandwidth is required when Manual bandwidth is selected.")
            End If
        ElseIf options.ManualBandwidth.HasValue AndAlso Not IsFinitePositive(options.ManualBandwidth.Value) Then
            Throw New ArgumentOutOfRangeException(NameOf(options.ManualBandwidth),
                                                  "ManualBandwidth, when supplied, must be finite and positive.")
        End If
    End Sub

    Private Shared Function ResolveBandwidth(observations As Double(),
                                              q1 As Double,
                                              q3 As Double,
                                              fallbackScale As Double,
                                              options As ViolinPlotOptions) As Double
        If options.BandwidthRule = ViolinBandwidthRule.Manual Then
            Return options.ManualBandwidth.Value
        End If

        Dim n As Double = observations.Length
        Dim nFactor As Double = Math.Pow(n, -0.2R)
        Dim sd As Double = SampleStandardDeviation(observations)
        Dim groupScale As Double = If(IsFinitePositive(sd), sd, fallbackScale)
        If Not IsFinitePositive(groupScale) Then groupScale = 1.0R

        Dim bandwidth As Double
        Select Case options.BandwidthRule
            Case ViolinBandwidthRule.Silverman
                Dim iqrScale As Double = (q3 - q1) / IqrNormalScale
                Dim robustScale As Double = groupScale
                If IsFinitePositive(iqrScale) Then robustScale = Math.Min(groupScale, iqrScale)
                If Not IsFinitePositive(robustScale) Then robustScale = groupScale
                bandwidth = 0.9R * robustScale * nFactor

            Case ViolinBandwidthRule.Scott
                bandwidth = groupScale * nFactor

            Case Else
                Throw New ArgumentOutOfRangeException(NameOf(options.BandwidthRule))
        End Select

        bandwidth *= options.BandwidthAdjustment
        If Not IsFinitePositive(bandwidth) Then
            Throw New InvalidOperationException("Automatic bandwidth selection produced a non-positive bandwidth.")
        End If
        Return bandwidth
    End Function

    Private Shared Function ResolveFallbackScale(values As Double()) As Double
        Dim sd As Double = SampleStandardDeviation(values)
        If IsFinitePositive(sd) Then Return sd

        If values.Length > 0 Then
            Dim magnitude As Double = Math.Abs(values(0))
            If IsFinitePositive(magnitude) Then Return magnitude
        End If
        Return 1.0R
    End Function

    Private Shared Function SampleStandardDeviation(values As Double()) As Double
        If values Is Nothing OrElse values.Length <= 1 Then Return 0.0R
        Dim mean As Double = values.Average()
        Dim sumSquares As Double = 0.0R
        For Each value As Double In values
            Dim d As Double = value - mean
            sumSquares += d * d
        Next
        Dim variance As Double = sumSquares / Convert.ToDouble(values.Length - 1)
        If variance <= 0.0R OrElse Not IsFinite(variance) Then Return 0.0R
        Return Math.Sqrt(variance)
    End Function

    Private Shared Function CreateLinearGrid(minimum As Double,
                                              maximum As Double,
                                              pointCount As Integer) As Double()
        Dim grid(pointCount - 1) As Double
        Dim stepSize As Double = (maximum - minimum) / Convert.ToDouble(pointCount - 1)
        For i As Integer = 0 To pointCount - 1
            grid(i) = minimum + i * stepSize
        Next
        grid(pointCount - 1) = maximum
        Return grid
    End Function

    Private Shared Function ComputeGaussianDensity(observations As Double(),
                                                    evaluationPoints As Double(),
                                                    bandwidth As Double) As Double()
        Dim result(evaluationPoints.Length - 1) As Double
        Dim multiplier As Double = GaussianNormalizingConstant / (Convert.ToDouble(observations.Length) * bandwidth)

        For i As Integer = 0 To evaluationPoints.Length - 1
            Dim y As Double = evaluationPoints(i)
            Dim kernelSum As Double = 0.0R
            For Each x As Double In observations
                Dim z As Double = (y - x) / bandwidth
                kernelSum += Math.Exp(-0.5R * z * z)
            Next
            result(i) = multiplier * kernelSum
        Next
        Return result
    End Function

    Private Shared Function ScaleDensity(density As Double(),
                                          groupMaximumDensity As Double,
                                          groupDensityArea As Double,
                                          globalMaximumAreaNormalizedDensity As Double,
                                          groupN As Integer,
                                          maximumGroupN As Integer,
                                          options As ViolinPlotOptions) As Double()
        Dim result(density.Length - 1) As Double
        Dim denominator As Double
        Dim countMultiplier As Double = 1.0R

        Select Case options.ScaleMode
            Case ViolinScaleMode.EqualMaximumWidth
                denominator = groupMaximumDensity

            Case ViolinScaleMode.EqualArea
                denominator = globalMaximumAreaNormalizedDensity

            Case ViolinScaleMode.Count
                denominator = groupMaximumDensity
                countMultiplier = Convert.ToDouble(groupN) / Convert.ToDouble(maximumGroupN)

            Case Else
                Throw New ArgumentOutOfRangeException(NameOf(options.ScaleMode))
        End Select

        For i As Integer = 0 To density.Length - 1
            Dim densityForScaling As Double = density(i)
            If options.ScaleMode = ViolinScaleMode.EqualArea Then
                densityForScaling /= groupDensityArea
            End If
            result(i) = options.MaximumHalfWidth * countMultiplier * densityForScaling / denominator
        Next
        Return result
    End Function


    Private Shared Function TrapezoidalArea(x As Double(), y As Double()) As Double
        If x Is Nothing OrElse y Is Nothing OrElse x.Length <> y.Length OrElse x.Length < 2 Then Return 0.0R
        Dim area As Double = 0.0R
        For i As Integer = 1 To x.Length - 1
            Dim dx As Double = x(i) - x(i - 1)
            If dx > 0.0R Then area += 0.5R * dx * (y(i - 1) + y(i))
        Next
        Return area
    End Function

    Private Shared Function IsMissingGroupValue(value As Object) As Boolean
        If value Is Nothing OrElse Convert.IsDBNull(value) Then Return True
        If TypeOf value Is String Then Return String.IsNullOrWhiteSpace(DirectCast(value, String))
        Return False
    End Function

    'Treat CLR numerics and parseable numeric text as numerical categories, without
    'using Visual Basic's IsNumeric helper. Current-culture text is tried first;
    'invariant-culture text is also supported for portable worksheet inputs.
    'Other values (Boolean, DateTime, etc.) remain categorical text.
    Private Shared Function TryGetNumericGroupValue(value As Object, ByRef number As Double) As Boolean
        number = 0.0R
        If value Is Nothing OrElse Convert.IsDBNull(value) Then Return False
        Dim kind As TypeCode = Type.GetTypeCode(value.GetType())
        Select Case kind
            Case TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16,
                 TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64,
                 TypeCode.Single, TypeCode.Double, TypeCode.Decimal
                Try
                    number = Convert.ToDouble(value, CultureInfo.InvariantCulture)
                    Return True
                Catch ex As OverflowException
                    Return False
                End Try
            Case TypeCode.String
                Dim text As String = DirectCast(value, String)
                Dim style As NumberStyles = NumberStyles.Float Or NumberStyles.AllowThousands
                Return Double.TryParse(text, style, CultureInfo.CurrentCulture, number) OrElse
                       Double.TryParse(text, style, CultureInfo.InvariantCulture, number)
            Case Else
                Return False
        End Select
    End Function

    Private Shared Function BuildGroupKey(value As Object) As String
        Dim numericValue As Double
        If TryGetNumericGroupValue(value, numericValue) Then
            Return "N:" & numericValue.ToString("R", CultureInfo.InvariantCulture)
        End If
        Return "S:" & Convert.ToString(value, CultureInfo.InvariantCulture).Trim()
    End Function

    Private Shared Function FormatGroupName(value As Object) As String
        Dim numericValue As Double
        If TryGetNumericGroupValue(value, numericValue) Then
            Return numericValue.ToString(CultureInfo.CurrentCulture)
        End If
        Return Convert.ToString(value, CultureInfo.CurrentCulture).Trim()
    End Function

    Private Shared Function NearlyEqual(a As Double, b As Double) As Boolean
        Dim scale As Double = Math.Max(1.0R, Math.Max(Math.Abs(a), Math.Abs(b)))
        Return Math.Abs(a - b) <= 1.0E-12R * scale
    End Function

    Private Shared Function IsFinite(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
    End Function

    Private Shared Function IsFinitePositive(value As Double) As Boolean
        Return IsFinite(value) AndAlso value > 0.0R
    End Function
End Class
