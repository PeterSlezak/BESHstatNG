Option Explicit On
Option Strict Off
Imports System
Imports System.Collections.Generic
Imports BESHStatNG
' Frozen b48877406c3c185a992d5dd5c42f951611a52569 source; no native execution.
Friend Class LegacyRegressionRemoval08
    Private Function IsDerivedEffectKey(effectText As String) As Boolean
        Dim s As String = If(effectText, String.Empty).Trim()
        If s = String.Empty Then Return False

        'Categorical effect is a main effect, not a derived term
        If s.StartsWith(RegressionDesignCore.CATEGORICAL_EFFECT_PREFIX, StringComparison.Ordinal) Then
            Return False
        End If

        'Polynomial: "<base>"^k or <base>^k
        Dim pCaret As Integer = InStrRev(s, "^")
        If pCaret > 0 AndAlso pCaret < Len(s) Then
            Dim expStr As String = Mid$(s, pCaret + 1)
            Dim expVal As Integer
            If Integer.TryParse(expStr, expVal) Then
                Return True
            End If
        End If

        'Interaction: "A":"B":...
        If InStr(s, ":") > 0 AndAlso InStr(s, ChrW(34)) > 0 Then
            Dim keys As List(Of String) = RegressionDesignCore.ExtractBaseKeysFromEffectText(s)
            If keys.Count >= 2 Then Return True
        End If

        Return False
    End Function

    Private Function ContainsAnyBaseKey(baseKeys As List(Of String),
                                    removedBaseKeys As HashSet(Of String)) As Boolean
        If baseKeys Is Nothing Then Return False

        For Each bk As String In baseKeys
            If removedBaseKeys.Contains(bk) Then Return True
        Next

        Return False
    End Function

    Private Sub SyncTermSpecsToListBoxItems(lbox As Object,
                                        termSpecs As Dictionary(Of String, TermSpec))
        If termSpecs Is Nothing Then Exit Sub

        Dim keep As New HashSet(Of String)(StringComparer.Ordinal)
        For Each it As Object In lbox.Items
            keep.Add(CStr(it))
        Next

        Dim stale As New List(Of String)
        For Each k As String In termSpecs.Keys
            If Not keep.Contains(k) Then stale.Add(k)
        Next

        For Each k As String In stale
            termSpecs.Remove(k)
        Next

        RegressionDesignCore.RefreshTermSpecOrders(lbox.Items, termSpecs)
    End Sub

    Private Function ExtractBaseKeysForSubset(effectText As String, bOnlyMain As Boolean) As List(Of String)
        Dim out As New List(Of String)
        Dim s As String = If(effectText, String.Empty).Trim()

        If Not bOnlyMain Then
            out.Add(s)
            Return out
        End If

        '1) Polynomial: "<base>"^k or <base>^k
        Dim pCaret As Integer = InStrRev(s, "^")
        If pCaret > 0 AndAlso pCaret < Len(s) Then
            Dim expStr As String = Mid$(s, pCaret + 1)
            Dim expVal As Integer
            If Integer.TryParse(expStr, expVal) Then
                Dim baseKey As String = Left$(s, pCaret - 1).Trim()
                out.Add(Global.BESHStatNG.RegressionDesignCore.StripOuterDoubleQuotesPublic(baseKey))
                Return out
            End If
        End If

        '2) New interaction format:  "A":"B"  (also supports multiway in future)
        If InStr(s, ":") > 0 AndAlso InStr(s, ChrW(34)) > 0 Then
            Dim parts() As String = Split(s, ":")
            For Each p As String In parts
                Dim k As String = Global.BESHStatNG.RegressionDesignCore.StripOuterDoubleQuotesPublic(p.Trim())
                If k <> String.Empty Then out.Add(k)
            Next
            If out.Count >= 2 Then Return out
            out.Clear()
        End If

        '3) Categorical predictor
        If s.StartsWith(RegressionDesignCore.CATEGORICAL_EFFECT_PREFIX, StringComparison.Ordinal) Then
            out.Add(s.Substring(RegressionDesignCore.CATEGORICAL_EFFECT_PREFIX.Length).Trim())
            Return out
        End If

        '3) Main effect as-is
        out.Add(s)
        Return out
    End Function

    Public Sub Remove_Item(lbox As Object, Optional sWhich As String = "all",
                           Optional termSpecs As Dictionary(Of String, TermSpec) = Nothing)

        If lbox Is Nothing OrElse lbox.Items.Count = 0 Then Exit Sub

        If String.Equals(sWhich, "all", StringComparison.OrdinalIgnoreCase) Then
            lbox.Items.Clear()

            If termSpecs IsNot Nothing Then
                termSpecs.Clear()
                RegressionDesignCore.RefreshTermSpecOrders(lbox.Items, termSpecs)
            End If

            Exit Sub
        End If

        If Not String.Equals(sWhich, "selected", StringComparison.OrdinalIgnoreCase) Then
            Exit Sub
        End If

        If lbox.SelectedItems.Count = 0 Then
            MsgBox("Please select variable(s) to remove.", vbExclamation, "Input Error!")
            Exit Sub
        End If

        Dim selectedKeys As New HashSet(Of String)(StringComparer.Ordinal)
        For Each it As Object In lbox.SelectedItems
            selectedKeys.Add(CStr(it))
        Next

        'If a main effect is removed, also remove all dependent derived terms
        Dim removedBaseKeys As New HashSet(Of String)(StringComparer.Ordinal)
        For Each effKey As String In selectedKeys
            If Not IsDerivedEffectKey(effKey) Then
                Dim baseKeys As List(Of String) = ExtractBaseKeysForSubset(effKey, True)
                For Each bk As String In baseKeys
                    If bk <> String.Empty Then removedBaseKeys.Add(bk)
                Next
            End If
        Next

        Dim keysToRemove As New HashSet(Of String)(selectedKeys, StringComparer.Ordinal)

        If removedBaseKeys.Count > 0 Then
            For Each it As Object In lbox.Items
                Dim effKey As String = CStr(it)
                If keysToRemove.Contains(effKey) Then Continue For

                Dim baseKeys As List(Of String) = ExtractBaseKeysForSubset(effKey, True)
                If ContainsAnyBaseKey(baseKeys, removedBaseKeys) Then
                    keysToRemove.Add(effKey)
                End If
            Next
        End If

        For i As Integer = lbox.Items.Count - 1 To 0 Step -1
            Dim effKey As String = CStr(lbox.Items(i))
            If keysToRemove.Contains(effKey) Then
                lbox.Items.RemoveAt(i)
            End If
        Next

        If termSpecs IsNot Nothing Then
            SyncTermSpecsToListBoxItems(lbox, termSpecs)
        End If
    End Sub
End Class
