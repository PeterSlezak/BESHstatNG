Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Xml.Linq

Namespace ExcelCommon
    Public NotInheritable Class RegressionEffectFormulaProjection
        Public Sub New(rhs As String, diagnostics As IEnumerable(Of RegressionEffectDiagnostic))
            Me.Rhs = rhs : Me.Diagnostics = New List(Of RegressionEffectDiagnostic)(diagnostics).AsReadOnly()
        End Sub
        ''' <summary>Nothing on failure. Never pass an empty model as blank RHS: Core treats blank as all main effects.</summary>
        Public ReadOnly Property Rhs As String
        Public ReadOnly Property Diagnostics As IReadOnlyList(Of RegressionEffectDiagnostic)
        Public ReadOnly Property Representable As Boolean
            Get
                Return Rhs IsNot Nothing
            End Get
        End Property
    End Class

    ''' <summary>Version 1 typed serialization uses XML escaping, ordered effects/base keys, invariant
    ''' IEEE-754 double bits, explicit null lists/strings and a separate intercept flag. Only Core's
    ''' existing default treatment/reference coding is supported; no contrast inference is added.</summary>
    Public NotInheritable Class RegressionEffectSerialization
        Private Sub New()
        End Sub
        Public Shared Function Serialize(model As RegressionEffectModel) As String
            If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            Dim root As New XElement("regression-effects", New XAttribute("version", "1"),
                New XAttribute("intercept", If(model.IncludeIntercept, "true", "false")), New XAttribute("contrast", "core-default"))
            For Each e In model.Effects
                Dim node As New XElement("effect", Text("key", e.Key)), s = e.Spec
                If s Is Nothing Then
                    node.Add(New XElement("spec", New XAttribute("null", "true")))
                Else
                    node.Add(New XElement("spec", Text("kind", s.Kind), Strings("bases", s.BaseVarKeys), Strings("display-bases", s.BaseVarDisplayNames),
                        Text("degree", s.Degree.ToString(CultureInfo.InvariantCulture)), Text("label", s.DisplayNameForCoef),
                        Text("order", s.Order.ToString(CultureInfo.InvariantCulture)), Text("scale", CInt(s.Scale).ToString(CultureInfo.InvariantCulture)),
                        Text("reference", If(s.ReferenceValue.HasValue, DoubleBits(s.ReferenceValue.GetValueOrDefault()), Nothing)),
                        Strings("observed", If(s.ObservedLevels Is Nothing, Nothing, s.ObservedLevels.Select(Function(v) DoubleBits(v))))))
                End If
                root.Add(node)
            Next
            Return root.ToString(SaveOptions.DisableFormatting)
        End Function
        Public Shared Function TryDeserialize(serialized As String, ByRef model As RegressionEffectModel,
                                              ByRef diagnostic As RegressionEffectDiagnostic,
                                              Optional maximumCharacters As Integer = 4194304) As Boolean
            model = Nothing : diagnostic = Nothing
            Try
                If serialized Is Nothing OrElse serialized.Length > maximumCharacters OrElse serialized.IndexOf("<!", StringComparison.Ordinal) >= 0 Then Throw New FormatException("Invalid serialized spec or size budget.")
                Dim root = XElement.Parse(serialized, LoadOptions.PreserveWhitespace)
                If root.Name <> "regression-effects" OrElse CStr(root.Attribute("version")) <> "1" OrElse CStr(root.Attribute("contrast")) <> "core-default" Then Throw New FormatException("Unsupported spec version or contrast.")
                Dim intercept = CStr(root.Attribute("intercept"))
                If intercept <> "true" AndAlso intercept <> "false" Then Throw New FormatException("Invalid intercept.")
                Dim effects As New List(Of RegressionAuthoredEffect)
                For Each node In root.Elements("effect")
                    Dim sn = node.Element("spec"), s As TermSpec = Nothing
                    If sn Is Nothing Then Throw New FormatException("Missing spec.")
                    If CStr(sn.Attribute("null")) <> "true" Then
                        Dim observed = ReadStrings(sn.Element("observed")), ref = ReadText(sn.Element("reference"))
                        s = New TermSpec With {.Kind = ReadText(sn.Element("kind")), .BaseVarKeys = ReadStrings(sn.Element("bases")),
                            .BaseVarDisplayNames = ReadStrings(sn.Element("display-bases")), .Degree = Integer.Parse(ReadText(sn.Element("degree")), CultureInfo.InvariantCulture),
                            .DisplayNameForCoef = ReadText(sn.Element("label")), .Order = Integer.Parse(ReadText(sn.Element("order")), CultureInfo.InvariantCulture),
                            .Scale = CType(Integer.Parse(ReadText(sn.Element("scale")), CultureInfo.InvariantCulture), PredictorScale),
                            .ReferenceValue = If(ref Is Nothing, CType(Nothing, Nullable(Of Double)), ReadDoubleBits(ref)),
                            .ObservedLevels = If(observed Is Nothing, Nothing, observed.Select(Function(v) ReadDoubleBits(v)).ToList())}
                    End If
                    effects.Add(New RegressionAuthoredEffect(ReadText(node.Element("key")), s))
                Next
                Dim candidate As New RegressionEffectModel(effects, intercept = "true")
                ' Reject unknown fields and noncanonical input rather than silently discarding settings.
                If Serialize(candidate) <> serialized Then Throw New FormatException("Spec is not canonical version 1.")
                model = candidate : Return True
            Catch ex As Exception When TypeOf ex Is FormatException OrElse TypeOf ex Is ArgumentException OrElse TypeOf ex Is OverflowException OrElse TypeOf ex Is System.Xml.XmlException
                diagnostic = New RegressionEffectDiagnostic("serialization", "Invalid or unsupported canonical authoring spec.")
                Return False
            End Try
        End Function
        Private Shared Function Text(name As String, value As String) As XElement
            If value Is Nothing Then Return New XElement(name, New XAttribute("null", "true"))
            If value.IndexOf(Convert.ToChar(13)) >= 0 OrElse value.IndexOf(Convert.ToChar(10)) >= 0 Then Return EncodedText(name, value)
            Try
                System.Xml.XmlConvert.VerifyXmlChars(value)
                Return New XElement(name, value)
            Catch ex As System.Xml.XmlException
                Return EncodedText(name, value)
            End Try
        End Function
        Private Shared Function DoubleBits(value As Double) As String
            Return BitConverter.DoubleToInt64Bits(value).ToString("X16", CultureInfo.InvariantCulture)
        End Function
        Private Shared Function ReadDoubleBits(value As String) As Double
            Return BitConverter.Int64BitsToDouble(Long.Parse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture))
        End Function
        Private Shared Function EncodedText(name As String, value As String) As XElement
                ' Lossless .NET UTF-16 code units, even when XML 1.0 cannot carry the string.
                Dim bytes(value.Length * 2 - 1) As Byte
                For i = 0 To value.Length - 1
                    Dim unit = Convert.ToUInt16(value(i))
                    bytes(2 * i) = CByte(unit And 255) : bytes(2 * i + 1) = CByte(unit >> 8)
                Next
                Return New XElement(name, New XAttribute("encoding", "utf16le-base64"), Convert.ToBase64String(bytes))
        End Function
        Private Shared Function Strings(name As String, values As IEnumerable(Of String)) As XElement
            If values Is Nothing Then Return New XElement(name, New XAttribute("null", "true"))
            Return New XElement(name, values.Select(Function(v) Text("value", v)))
        End Function
        Private Shared Function ReadText(node As XElement) As String
            If node Is Nothing Then Throw New FormatException("Missing field.")
            If CStr(node.Attribute("encoding")) = "utf16le-base64" Then
                Dim bytes = Convert.FromBase64String(node.Value)
                If bytes.Length Mod 2 <> 0 Then Throw New FormatException("Invalid code-unit encoding.")
                Dim chars(bytes.Length \ 2 - 1) As Char
                For i = 0 To chars.Length - 1
                    chars(i) = Convert.ToChar(CUShort(CInt(bytes(2 * i)) Or (CInt(bytes(2 * i + 1)) << 8)))
                Next
                Return New String(chars)
            End If
            Return If(CStr(node.Attribute("null")) = "true", Nothing, node.Value)
        End Function
        Private Shared Function ReadStrings(node As XElement) As List(Of String)
            If node Is Nothing Then Throw New FormatException("Missing field.")
            If CStr(node.Attribute("null")) = "true" Then Return Nothing
            Return node.Elements("value").Select(Function(n) ReadText(n)).ToList()
        End Function

        ''' <summary>RHS uses relative source-column tokens, avoiding name-based parser ambiguity.
        ''' The supplied ordered catalog is part of the projection contract, and must also be used
        ''' by the model-input compiler. Parser-normalized interaction ordering must be identical.</summary>
        Public Shared Function ProjectFormula(model As RegressionEffectModel, orderedCatalog As IEnumerable(Of String)) As RegressionEffectFormulaProjection
            Dim issues As New List(Of RegressionEffectDiagnostic)
            Dim keys = If(orderedCatalog, Enumerable.Empty(Of String)()).ToList()
            If model Is Nothing OrElse keys.Any(Function(k) k Is Nothing OrElse k.Length = 0) OrElse keys.Distinct(StringComparer.Ordinal).Count() <> keys.Count Then
                Return NotRepresentable("Invalid or ambiguous source catalog.")
            End If
            If model.Effects.Count = 0 Then Return NotRepresentable("Empty models require the explicit typed spec; blank RHS means all main effects in Core.")
            If keys.Any(Function(k) k.Trim() <> k) Then Return NotRepresentable("Core design integration trims raw keys; whitespace identity requires the typed spec.")
            If RegressionEffectAuthoringPolicy.Inspect(model, keys).Count > 0 Then Return NotRepresentable("Invalid or incomplete term metadata; inspect the typed spec.")
            If model.Effects.Select(Function(e) e.Key).Distinct(StringComparer.Ordinal).Count() <> model.Effects.Count Then Return NotRepresentable("Duplicate effect identities.")
            Dim polynomialBases = model.Effects.Where(Function(e) e.Spec IsNot Nothing AndAlso e.Spec.Kind = "Polynomial" AndAlso e.Spec.BaseVarKeys IsNot Nothing).SelectMany(Function(e) e.Spec.BaseVarKeys).ToArray()
            If model.Effects.Any(Function(e) e.Spec IsNot Nothing AndAlso e.Spec.Kind = "Interaction" AndAlso e.Spec.BaseVarKeys IsNot Nothing AndAlso e.Spec.BaseVarKeys.Any(Function(k) polynomialBases.Contains(k, StringComparer.Ordinal))) Then
                Return NotRepresentable("Existing Core polynomial/interaction display-name paths differ; retain typed terms (C08-CORE-LABEL-01).")
            End If
            Dim specs = model.ToTermSpecs(), terms As New List(Of String)
            For Each e In model.Effects
                Dim s = e.Spec
                If s Is Nothing OrElse s.BaseVarKeys Is Nothing OrElse s.BaseVarKeys.Count = 0 OrElse s.BaseVarKeys.Any(Function(k) Not keys.Contains(k, StringComparer.Ordinal)) Then Return NotRepresentable("Missing raw variable or term metadata.", e.Key)
                If s.ObservedLevels IsNot Nothing OrElse s.BaseVarDisplayNames IsNot Nothing Then Return NotRepresentable("RHS cannot preserve observed levels or custom display metadata; retain typed serialization.", e.Key)
                Dim tokens As New List(Of String)
                For Each key In s.BaseVarKeys
                    Dim token = RegressionVariableCatalog.NumberToLetters(keys.IndexOf(key) + 1)
                    Dim cat As TermSpec = Nothing
                    If s.Kind = "MainEffect" AndAlso s.Scale = PredictorScale.Categorical Then
                        cat = s
                    ElseIf s.Kind = "Interaction" Then
                        specs.TryGetValue(RegressionDesignCore.MakeCategoricalEffectKey(key), cat)
                    End If
                    If cat IsNot Nothing Then
                        If cat.ReferenceValue.HasValue AndAlso (Double.IsNaN(cat.ReferenceValue.Value) OrElse Double.IsInfinity(cat.ReferenceValue.Value)) Then Return NotRepresentable("Nonfinite factor reference.", e.Key)
                        token = "factor(" & token & If(cat.ReferenceValue.HasValue, ", ref=" & cat.ReferenceValue.GetValueOrDefault().ToString("R", CultureInfo.InvariantCulture), "") & ")"
                    End If
                    tokens.Add(token)
                Next
                Select Case s.Kind
                    Case "MainEffect"
                        If tokens.Count <> 1 Then Return NotRepresentable("Invalid main effect.", e.Key)
                        terms.Add(tokens(0))
                    Case "Polynomial"
                        If tokens.Count <> 1 Then Return NotRepresentable("Invalid polynomial.", e.Key)
                        terms.Add(tokens(0) & "^" & s.Degree.ToString(CultureInfo.InvariantCulture))
                    Case "Interaction"
                        terms.Add(String.Join(":", tokens))
                    Case Else
                        Return NotRepresentable("Unsupported term kind.", e.Key)
                End Select
            Next
            Dim catalog = RegressionVariableCatalog.Build(keys.Select(Function(k) RegressionDesignCore.GetCoefBaseName(k)), keys)
            Dim parsed As RegressionFormulaDesignSpec = Nothing, message As String = Nothing
            If Not RegressionFormulaParser.TryParseFormulaToDesignSpec(String.Join(" + ", terms), catalog, parsed, message) Then Return NotRepresentable(message)
            Dim reparsed As RegressionFormulaDesignSpec = Nothing
            If Not RegressionFormulaParser.TryParseFormulaToDesignSpec(parsed.NormalizedFormulaText, catalog, reparsed, message) Then Return NotRepresentable(message)
            For Each pair In parsed.TermSpecs
                If Not reparsed.TermSpecs.ContainsKey(pair.Key) OrElse Not Nullable.Equals(pair.Value.ReferenceValue, reparsed.TermSpecs(pair.Key).ReferenceValue) Then
                    Return NotRepresentable("Core normalized reference text loses precision on this runtime; retain typed metadata (C08-FORMULA-REF-01).", pair.Key)
                End If
            Next
            If Not parsed.EffectItems.SequenceEqual(model.Effects.Select(Function(e) e.Key), StringComparer.Ordinal) Then Return NotRepresentable("Core normalization changes effect identity or interaction order.")
            For Each e In model.Effects
                Dim a = e.Spec, b = parsed.TermSpecs(e.Key)
                If a.Kind <> b.Kind OrElse a.Degree <> b.Degree OrElse a.Scale <> b.Scale OrElse a.Order <> b.Order OrElse
                    Not a.BaseVarKeys.SequenceEqual(b.BaseVarKeys, StringComparer.Ordinal) OrElse a.DisplayNameForCoef <> b.DisplayNameForCoef OrElse
                    Not Nullable.Equals(a.ReferenceValue, b.ReferenceValue) Then Return NotRepresentable("RHS would change term metadata.", e.Key)
            Next
            Return New RegressionEffectFormulaProjection(parsed.NormalizedFormulaText, issues)
        End Function
        Private Shared Function NotRepresentable(message As String, Optional effectKey As String = Nothing) As RegressionEffectFormulaProjection
            Return New RegressionEffectFormulaProjection(Nothing, {New RegressionEffectDiagnostic("formula-not-representable", message, effectKey:=effectKey)})
        End Function
    End Class
End Namespace
