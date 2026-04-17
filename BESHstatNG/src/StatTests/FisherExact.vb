Option Explicit On
Option Strict On

Imports System
Imports System.Math
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop
Imports Microsoft.Office.Interop.Excel

Namespace contingencytable


    ''' <summary>
    ''' Fisher Exact Test Engine implementing the Mehta–Patel network algorithm.
    ''' This is a structural port of R's fexact.c, but without the C workspace
    ''' allocator (iwork). Arrays are explicitly allocated using ReDim.
    ''' All prterr(...) calls replaced with Throw New Exception(...).
    ''' </summary>
    Public Class FisherExactEngine

        ' ==== PUBLIC RESULTS ====
        Public Property PObserved As Double   ' PRT in the C code
        Public Property PValue As Double      ' PRE in the C code

        ' ==== INPUT PARAMETERS ====
        Private ReadOnly _nrow As Integer
        Private ReadOnly _ncol As Integer
        Private ReadOnly _table(,) As Integer
        Private ReadOnly _expect As Double
        Private ReadOnly _percnt As Double
        Private ReadOnly _emin As Double
        Private ReadOnly _mult As Integer     ' MULT parameter in fexact.c


        ' ==== INTERNAL WORK ARRAYS ====
        ' These correspond to arrays in fexact.c but we allocate them directly.
        Private fact() As Double
        Private ico() As Integer
        Private iro() As Integer
        Private kyy() As Integer
        Private idif() As Integer
        Private irn() As Integer
        Private keyArr() As Integer
        Private ipoin() As Integer
        Private stp() As Double
        Private LP() As Double
        Private SP() As Double
        Private tm() As Double
        Private key2() As Integer
        Private ifrq() As Integer     ' frequencies
        Private npoin() As Integer    ' npoins (C: ifrq[jstp4])
        Private nr() As Integer       ' NEXT pointer
        Private nl() As Integer       ' LEFT pointer (unused in our simplified logic)

        Public Sub New(table As Integer(,), Optional expect As Double = 5.0#, Optional percnt As Double = 80.0#,
                   Optional emin As Double = 1.0#, Optional mult As Integer = 30)
            _nrow = UBound(table, 1) + 1
            _ncol = UBound(table, 2) + 1
            _table = table
            _expect = expect
            _percnt = percnt
            _emin = emin
            _mult = mult
        End Sub


        ' ======================================================================
        ' PUBLIC ENTRY POINT — similar to fexact() in R’s implementation
        ' ======================================================================
        Public Sub Run()
            Dim ntot As Integer = 0

            ' ==== Validate & compute total ====
            For i As Integer = 0 To _nrow - 1
                For j As Integer = 0 To _ncol - 1
                    Dim v = _table(i, j)
                    If v < 0 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("TABLE cannot contain negative values."))
                    ntot += v
                Next
            Next

            If ntot = 0 Then
                PObserved = Double.NaN
                PValue = Double.NaN
                Exit Sub
            End If

            ' ==== Determine workspace sizes ====
            Dim nco As Integer = Math.Max(_nrow, _ncol)
            Dim nro As Integer = Math.Min(_nrow, _ncol)

            ' ---- choose LDKEY and LDSTP ----
            Dim ldkey As Integer = 20000                 ' you can tune this
            Dim ldstp As Integer = _mult * ldkey         ' as in original: LDSTP = MULT * LDKEY

            ' =====================================================================
            ' ALLOCATE ALL WORKSPACE ARRAYS (this replaces iwork() in the C code)
            ' =====================================================================
            ' fact(k) = log(k!) = lgamma(k+1)
            ReDim fact(ntot)
            For k As Integer = 0 To ntot
                fact(k) = LogGamma(k + 1.0)
            Next

            ' --- row/column workspace arrays (1-based)
            ReDim ico(nco)      ' valid range 1..nco
            ReDim iro(nco)      ' valid range 1..nco
            ReDim kyy(nro)      ' valid range 1..nro
            ReDim idif(nro)     ' valid range 1..nro
            ReDim irn(nro)      ' valid range 1..nro

            ' --- hash table & secondary hash table (1..2*ldkey)
            ReDim keyArr(2 * ldkey)
            ReDim key2(2 * ldkey)
            ReDim ipoin(2 * ldkey)     ' root node for each bucket

            ' --- path-length tables (1-based)
            ReDim stp(2 * ldstp)       ' path values (double)
            ReDim ifrq(2 * ldstp)      ' frequencies

            ' --- for tree pointer operations used by F5xact
            ReDim npoin(2 * ldstp)     ' pointer to next in linked list
            ReDim nl(2 * ldstp)        ' left-pointer in the BST tree
            ReDim nr(2 * ldstp)        ' right-pointer in the BST tree

            ' --- longest/shortest path results (indexed by hash table index)
            ReDim LP(2 * ldkey)
            ReDim SP(2 * ldkey)
            ReDim tm(2 * ldkey)

            ' init key banks
            For i As Integer = 1 To 2 * ldkey
                keyArr(i) = -9999
                ipoin(i) = 0
                key2(i) = -9999
                LP(i) = 0.0
                SP(i) = 0.0
            Next

            ' init stp banks
            For i As Integer = 1 To 2 * ldstp
                stp(i) = 0.0
                ifrq(i) = 0
                npoin(i) = -1
                nl(i) = -1
                nr(i) = -1
            Next

            ' =====================================================================
            ' Now call the main computational engine (to be fully translated)
            ' =====================================================================
            F2xact(_nrow, _ncol, _table, _expect, _percnt, _emin, PObserved, PValue, fact, ico, iro, kyy,
               idif, irn, keyArr, ldkey, ipoin, stp, ldstp, ifrq, LP, SP, tm, key2)
        End Sub

        ' =====================================================================
        ' Helper functions
        ' =====================================================================
        ''' <summary>
        ''' Extracts a 1-based slice of source array into a 0-based array.
        ''' src(1..N), slice begins at src(start) for length 'count'.
        ''' </summary>
        Private Function Slice1B_To0B(src() As Integer, start As Integer, count As Integer) As Integer()
            Dim outArr(count - 1) As Integer
            For i As Integer = 0 To count - 1
                outArr(i) = src(start + i)
            Next
            Return outArr
        End Function

        Private Function To1BasedFrom0Based(src0() As Integer, n As Integer) As Integer()
            Dim dst1(n) As Integer ' 0..n (use 1..n)
            For k As Integer = 1 To n
                dst1(k) = src0(k - 1)
            Next
            Return dst1
        End Function

        ' ---- Helper: sort it(1..n) ascending, keeping index 0 unused ----
        Private Sub SortAsc1Based(a() As Integer, n As Integer)
            If n <= 1 Then Return
            Dim tmp(n) As Integer
            For i As Integer = 1 To n
                tmp(i) = a(i)
            Next
            Array.Sort(tmp, 1, n)
            For i As Integer = 1 To n
                a(i) = tmp(i)
            Next
        End Sub


        ''' <summary>
        ''' F2XACT — Main Mehta–Patel network algorithm driver.
        ''' 
        ''' All input arrays (ico, iro, irn, idif, kyy, keyArr, ipoin, stp, ifrq,
        ''' LP, SP, tm, key2) MUST BE 1-based.
        ''' </summary>
        Private Sub F2xact(nrow As Integer, ncol As Integer, table As Integer(,),
                   expect As Double, percnt As Double, emin As Double,
                   ByRef prt As Double, ByRef pre As Double,
                   fact() As Double, ico() As Integer, iro() As Integer, kyy() As Integer,
                   idif() As Integer, irn() As Integer, keyArr() As Integer, ByVal ldkey As Integer,
                   ipoin() As Integer, stp() As Double, ByVal ldstp As Integer, ifrq() As Integer,
                   LP() As Double, SP() As Double, tm() As Double, key2() As Integer)

            Const tol As Double = 0.000000345254
            Dim pastp As Double

            ' -----------------------------
            ' 1) Compute raw marginals
            ' -----------------------------
            Dim i As Integer, j As Integer
            Dim ntot As Integer = 0

            For i = 1 To nrow
                Dim s As Integer = 0
                For j = 1 To ncol
                    s += table(i - 1, j - 1)
                Next
                iro(i) = s
                ntot += s
            Next

            For j = 1 To ncol
                Dim s As Integer = 0
                For i = 1 To nrow
                    s += table(i - 1, j - 1)
                Next
                ico(j) = s
            Next

            ' Sort marginals (R does this)
            SortAsc1Based(iro, nrow)
            SortAsc1Based(ico, ncol)

            ' Orient so nco = max(nrow,ncol), nro = min(...)
            Dim nco As Integer = Math.Max(nrow, ncol)
            Dim nro As Integer = Math.Min(nrow, ncol)
            Dim nr_gt_nc As Boolean = (nrow > ncol)

            ' Build oriented (possibly swapped) marginals into iro(1..nro) and ico(1..nco)
            If nr_gt_nc Then
                ' swap
                Dim tmp(Math.Max(nrow, ncol)) As Integer
                For i = 1 To nrow : tmp(i) = iro(i) : Next
                For i = 1 To nco
                    Dim oldRow As Integer = tmp(i)
                    If i <= nro Then iro(i) = ico(i)   ' smaller side becomes rows
                    ico(i) = oldRow                     ' larger side becomes cols
                Next
            Else
                ' already fine: iro(1..nro)=sorted rows, ico(1..nco)=sorted cols
            End If

            ' ------------------------------------------------------------
            ' R / fexact.c scale:
            '   obs = tol + sum(log(col!)) - sum(log(cell!))
            '   dro = f9xact(nro, ntot, iro) = log(ntot!) - sum(log(row!))
            '   PObserved = exp(obs - dro)
            ' ------------------------------------------------------------
            ' sum log(cell!)
            Dim sumCell As Double = 0.0
            For i = 0 To nrow - 1
                For j = 0 To ncol - 1
                    sumCell += fact(table(i, j))
                Next
            Next

            ' sum log(col!)  (use oriented nco / ico(1..nco))
            Dim sumCol As Double = 0.0
            For j = 1 To nco
                sumCol += fact(ico(j))
            Next

            Dim obs As Double = tol + sumCol - sumCell

            ' dro = log(ntot!) - sum log(row!)  (use oriented nro / iro(1..nro))
            Dim iro0() As Integer = Slice1B_To0B(iro, 1, nro)
            Dim dro As Double = F9xact(nro, ntot, iro0, fact)

            ' PObserved
            prt = Math.Exp(obs - dro)
            pre = 0.0

            ' IMPORTANT: network starts with pastp from stp() head (0.0), so do NOT set pastp = obs here.
            pastp = 0.0

            ' -----------------------------
            ' 3) Build kyy multipliers (R does overflow checks; keep a simple check)
            ' -----------------------------
            kyy(1) = 1
            For i = 1 To nro - 1
                Dim mult As Long = CLng(kyy(i)) * (CLng(iro(i)) + 1L)
                If mult > Integer.MaxValue Then AppGlobals.BSerr.LogAndThrow(New ApplicationException("kyy overflow; increase workspace / change encoding."))
                kyy(i + 1) = CInt(mult)
            Next
            If (CLng(iro(nro)) + 1L) > (CLng(Integer.MaxValue) \ CLng(kyy(nro))) Then
                AppGlobals.BSerr.LogAndThrow(New ApplicationException("kyy overflow (final check)."))
            End If

            ' -----------------------------
            ' 4) Two banks for key/ipoin and stp/ifrq/npoin/nl/nr
            '    Offsets are 0 or ldkey / ldstp (VB arrays are 1..2*X)
            ' -----------------------------
            Dim ikkeyOff As Integer = 0
            Dim jkeyOff As Integer = ldkey
            Dim ikstpOff As Integer = 0
            Dim jstpOff As Integer = ldstp

            ' Stage counter k starts at nco (R)
            Dim k As Integer = nco

            ' Column pointer index (R uses kb = nco - k + 1)
            Dim kb As Integer

            ' Stack scan pointer
            Dim last As Integer = ldkey + 1

            ' Next-stage stp top
            Dim itop As Integer = 0

            ' Seed initial mother: ipo=1; ipoin=1; stp=0; ifrq=1; npoin=-1
            Dim ipo As Integer = 1
            ipoin(ikkeyOff + ipo) = 1
            stp(ikstpOff + 1) = 0.0
            ifrq(ikstpOff + 1) = 1
            npoin(ikstpOff + 1) = -1
            nl(ikstpOff + 1) = -1
            nr(ikstpOff + 1) = -1

            ' Clear secondary hash key2
            For i = 1 To 2 * ldkey
                key2(i) = -9999
                LP(i) = 0.0
                SP(i) = 0.0
            Next

Outer_Loop:
            kb = nco - k + 1

            ' Build FIRST daughter idif() for this mother (same greedy init as R)
            Dim n As Integer = ico(kb)
            Dim kd As Integer = nro + 1
            Dim kmax As Integer = nro
            For i = 1 To nro
                idif(i) = 0
            Next

            Do
                kd -= 1
                Dim put As Integer = Math.Min(n, iro(kd))
                idif(kd) = put
                If idif(kmax) = 0 Then kmax -= 1
                n -= put
            Loop While n > 0 AndAlso kd <> 1

            If n <> 0 Then GoTo L310

            Dim k1 As Integer = k - 1
            n = ico(kb)

            ' Remaining total after removing this column
            Dim ntotRem As Integer = 0
            For j = kb + 1 To nco
                ntotRem += ico(j)
            Next

L150:
            ' Daughter row totals: irn(i) = iro(i) - idif(i)
            For i = 1 To nro
                irn(i) = iro(i) - idif(i)
                If irn(i) < 0 Then AppGlobals.BSerr.LogAndThrow(New ApplicationException("Invalid daughter: irn(i)<0."))
            Next

            Dim nrb As Integer = 1
            If k1 > 1 Then
                SortAsc1Based(irn, nro)
                For i = 1 To nro
                    If irn(i) <> 0 Then
                        nrb = i
                        Exit For
                    End If
                Next
            End If

            Dim nro2 As Integer = nro - nrb + 1

            ' ddf = f9xact(nro, n, idif)
            Dim idif0() As Integer = Slice1B_To0B(idif, 1, nro)
            Dim ddf As Double = F9xact(nro, n, idif0, fact)

            ' drn = f9xact(nro2, ntotRem, irn(nrb..)) - dro + ddf
            Dim irn0() As Integer = Slice1B_To0B(irn, nrb, nro2)
            Dim drn As Double = F9xact(nro2, ntotRem, irn0, fact) - dro + ddf

            ' Compute bounds if k1>1, with caching via key2/LP/SP (as R)
            Dim obs2 As Double
            Dim obs3 As Double
            Dim dspt As Double = 0.0
            Dim kval As Integer = 0
            Dim itp2 As Integer = 0

            If k1 > 1 Then
                kval = irn(1)
                For i = 2 To nro
                    kval += irn(i) * kyy(i)
                Next

                Dim startSlot As Integer = (kval Mod (ldkey * 2))
                If startSlot < 0 Then startSlot += (ldkey * 2)
                startSlot += 1

                Dim found As Boolean = False
                Dim slot As Integer

                For slot = startSlot To 2 * ldkey
                    If key2(slot) = kval Then found = True : itp2 = slot : Exit For
                    If key2(slot) < 0 Then itp2 = slot : Exit For
                Next
                If Not found AndAlso key2(itp2) <> kval Then
                    For slot = 1 To startSlot - 1
                        If key2(slot) = kval Then found = True : itp2 = slot : Exit For
                        If key2(slot) < 0 Then itp2 = slot : Exit For
                    Next
                End If

                If Not found AndAlso key2(itp2) < 0 Then
                    key2(itp2) = kval
                    LP(itp2) = 1.0
                    SP(itp2) = 1.0
                End If

                ' obs2 = obs - Σ fact(remaining cols) - ddf
                obs2 = obs - ddf
                For j = kb + 1 To kb + k1
                    obs2 -= fact(ico(j))
                Next
                dspt = obs - obs2 - ddf

                Dim cols0() As Integer = Slice1B_To0B(ico, kb + 1, k1)

                If LP(itp2) > 0.0 Then
                    LP(itp2) = F3xact(nro2, irn0, k1, cols0, ntotRem, fact, tol)
                    SP(itp2) = F4xact(nro2, irn0, k1, cols0, dspt, fact, tol)
                End If

                obs3 = obs2 - LP(itp2)
                obs2 -= SP(itp2)

            Else
                ' k1 <= 1
                obs2 = obs - drn - dro
                obs3 = obs2
            End If

            ' Process the mother chain for this ipo
            Dim ipn As Integer = ipoin(ikkeyOff + ipo)
            If ipn <= 0 Then AppGlobals.BSerr.LogAndThrow(New Exception("ipoin() head missing for mother."))

            pastp = stp(ikstpOff + ipn)
            Dim ifreq As Integer = ifrq(ikstpOff + ipn)

L300:
            If pastp <= obs3 Then
                pre += CDbl(ifreq) * Math.Exp(pastp + drn)

            ElseIf pastp < obs2 Then
                Dim d1 As Double = pastp + ddf

                ' Push node into NEXT stage structures (jkey/jstp banks)
                F5xact(d1, tol, kval,
               keyArr, jkeyOff, ldkey,
               ipoin,
               stp, jstpOff, ldstp,
               ifrq, npoin, nr, nl,
               ifreq, itop, True)
            End If

            ' Next node on chain (MUST use current STP bank offset)
            Dim nxt As Integer = npoin(ikstpOff + ipn)
            ipn = nxt

            If ipn > 0 Then
                pastp = stp(ikstpOff + ipn)
                ifreq = ifrq(ikstpOff + ipn)
                GoTo L300
            End If

            ' Next daughter from same mother
            Dim iflag As Integer = 0
            Dim ks As Integer = 0  ' R resets ks to 0 for each mother
            F7xact(kmax, iro, idif, kd, ks, iflag)
            If iflag <> 1 Then GoTo L150

L310:
            ' Pop next mother from CURRENT stage stack
            Do
                Dim noMore As Boolean = F6xact(nro, iro, kyy, keyArr, ikkeyOff, ldkey, last, ipo)
                If Not noMore Then GoTo Outer_Loop

                ' No more mothers at this stage -> collapse stage (k--)
                k -= 1
                If k < 2 Then GoTo L90

                itop = 0

                ' swap banks
                Dim tmpOff As Integer = ikkeyOff : ikkeyOff = jkeyOff : jkeyOff = tmpOff
                tmpOff = ikstpOff : ikstpOff = jstpOff : jstpOff = tmpOff

                ' remove one column total from remaining total
                kb = nco - k + 1
                ntot -= ico(kb)

                ' clear key2 cache
                For i = 1 To 2 * ldkey
                    key2(i) = -9999
                    LP(i) = 0.0
                    SP(i) = 0.0
                Next

            Loop

L90:
            ' Ensure includes observed (numerical safety)
            If pre < prt Then pre = prt
            If pre > 1.0 Then pre = 1.0
            If prt > 1.0 Then prt = 1.0
        End Sub


        ' Longest path (LP) bound for Mehta–Patel network algorithm
        ' Port of f3xact() from fexact.c (R).
        ' Returns a NON-POSITIVE value (<= 0). Caller typically uses obs - LP.
        '
        ' NOTE: ntot is the total count in the current subproblem (integer).
        ' Longest path (LP) bound for the current subproblem.
        ' irow0(), icol0() are 0-based (length nrow/ncol).
        Private Function F3xact(nrow As Integer, irow0() As Integer, ncol As Integer, icol0() As Integer,
                            ntot As Integer, fact() As Double, tol As Double) As Double
            Const ldst As Integer = 200 ' half stack size (stack arrays are 1..2*ldst)

            ' C keeps these as static across calls
            Static nst As Integer = 0
            Static nitc As Integer = 0
            Dim i As Integer

            ' ---- convert incoming 0-based marginals to 1-based to mirror C code ----
            Dim irow1(nrow) As Integer ' 1..nrow used
            Dim icol1(ncol) As Integer ' 1..ncol used
            For i = 1 To nrow
                irow1(i) = irow0(i - 1)
            Next
            For i = 1 To ncol
                icol1(i) = icol0(i - 1)
            Next

            ' Safety: if caller passed ntot incorrectly, derive it from marginals
            If ntot <= 0 Then
                Dim t As Integer = 0
                For i = 1 To nrow
                    t += irow1(i)
                Next
                ntot = t
            End If

            Dim tRow As Integer = 0
            For i = 1 To nrow
                tRow += irow1(i)
            Next

            Dim tCol As Integer = 0
            For i = 1 To ncol
                tCol += icol1(i)
            Next

            If ntot <> tRow OrElse ntot <> tCol Then
                ' Prefer row sum (or throw)
                ntot = tRow
            End If

            ' ---- easy cases ----
            If nrow <= 1 Then
                Dim lp As Double = 0.0
                If nrow > 0 Then
                    For i = 1 To ncol
                        lp -= fact(icol1(i))
                    Next
                End If
                Return lp
            End If

            If ncol <= 1 Then
                Dim lp As Double = 0.0
                If ncol > 0 Then
                    For i = 1 To nrow
                        lp -= fact(irow1(i))
                    Next
                End If
                Return lp
            End If

            ' ---- 2x2 shortcut ----
            If nrow * ncol = 4 Then
                Dim n11 As Integer = ((irow1(1) + 1) * (icol1(1) + 1)) \ (ntot + 2)

                ' Clamp to feasible range
                Dim lo As Integer = Math.Max(0, irow1(1) - icol1(2))
                Dim hi As Integer = Math.Min(irow1(1), icol1(1))
                If n11 < lo Then n11 = lo
                If n11 > hi Then n11 = hi

                Dim n12 As Integer = irow1(1) - n11
                Return -(fact(n11) + fact(n12) + fact(icol1(1) - n11) + fact(icol1(2) - n12))
            End If


            ' ---- local work arrays (mirror C work vectors) ----
            Dim maxRC As Integer = Math.Max(nrow, ncol)

            ' 1-based work vectors sized to MAX(nrow,ncol)
            Dim icoW(maxRC) As Integer
            Dim iroW(maxRC) As Integer
            Dim itW(maxRC) As Integer
            Dim lbW(maxRC) As Integer
            Dim nrW(maxRC) As Integer
            Dim ntW(maxRC) As Integer
            Dim nuW(maxRC) As Integer

            ' alen indexed 0..maxRC (alen(0)=0 used)
            Dim alenW(maxRC) As Double

            ' stack banks 1..2*ldst (0 unused)
            Dim itcW(2 * ldst) As Integer
            Dim istW(2 * ldst) As Long
            Dim stvW(2 * ldst) As Double

            ' ---- Test for optimal table via F10act ----
            Dim val As Double = 0.0
            Dim xmin As Boolean = False

            If irow1(nrow) <= irow1(1) + ncol Then
                Dim r0() As Integer = Slice1B_To0B(irow1, 1, nrow)
                Dim c0() As Integer = Slice1B_To0B(icol1, 1, ncol)
                Dim nd(r0.Length - 1) As Integer
                Dim ne(c0.Length - 1) As Integer
                Dim mw(c0.Length - 1) As Integer
                xmin = F10act(nrow, r0, ncol, c0, val, fact, nd, ne, mw)
            End If

            If (Not xmin) AndAlso (icol1(ncol) <= icol1(1) + nrow) Then
                Dim r0() As Integer = Slice1B_To0B(icol1, 1, ncol)
                Dim c0() As Integer = Slice1B_To0B(irow1, 1, nrow)
                Dim nd(r0.Length - 1) As Integer
                Dim ne(c0.Length - 1) As Integer
                Dim mw(c0.Length - 1) As Integer
                xmin = F10act(ncol, r0, nrow, c0, val, fact, nd, ne, mw)
            End If

            If xmin Then
                Return -val
            End If

            ' ---- Setup for dynamic programming ----
            For i = 0 To ncol
                alenW(i) = 0.0
            Next
            For i = 1 To 2 * ldst
                istW(i) = -1
            Next

            Dim nn As Integer = ntot

            ' Minimize ncol (swap roles if needed)
            Dim nro As Integer
            Dim nco As Integer

            If nrow >= ncol Then
                nro = nrow
                nco = ncol

                icoW(1) = icol1(1)
                ntW(1) = nn - icoW(1)
                For i = 2 To ncol
                    icoW(i) = icol1(i)
                    ntW(i) = ntW(i - 1) - icoW(i)
                Next
                For i = 1 To nrow
                    iroW(i) = irow1(i)
                Next
            Else
                nro = ncol
                nco = nrow

                icoW(1) = irow1(1)
                ntW(1) = nn - icoW(1)
                For i = 2 To nrow
                    icoW(i) = irow1(i)
                    ntW(i) = ntW(i - 1) - icoW(i)
                Next
                For i = 1 To ncol
                    iroW(i) = icol1(i)
                Next
            End If

            Dim nc1s As Integer = nco - 1
            Dim kyy As Integer = icoW(nco) + 1

            ' Initialize pointers
            Dim vmn As Double = 1.0E+100
            Dim irl As Integer = 1
            Dim ks As Integer = 0
            Dim k As Integer = ldst

            ' --------------------------
            ' Labels mirror the C code
            ' --------------------------
LnewNode:
            Dim lev As Integer = 1
            Dim nr1 As Integer = nro - 1
            Dim nrt As Integer = iroW(irl)
            Dim nct As Integer = icoW(1)

            lbW(1) = CInt(Math.Truncate((((CDbl(nrt) + 1.0) * (CDbl(nct) + 1.0)) /
                                 CDbl(nn + nr1 * nc1s + 1)) - tol)) - 1

            nuW(1) = CInt(Math.Truncate(((CDbl(nrt) + nc1s) * (CDbl(nct) + nr1)) /
                                CDbl(nn + nr1 + nc1s))) - lbW(1) + 1

            nrW(1) = nrt - lbW(1)

LoopNode:
            nuW(lev) -= 1
            If nuW(lev) = 0 Then
                If lev = 1 Then GoTo L200
                lev -= 1
                GoTo LoopNode
            End If

            lbW(lev) += 1
            nrW(lev) -= 1

            While True
                alenW(lev) = alenW(lev - 1) + fact(lbW(lev))

                If lev >= nc1s Then Exit While

                Dim nn1 As Integer = ntW(lev)
                nrt = nrW(lev)
                lev += 1
                Dim nc1 As Integer = nco - lev
                nct = icoW(lev)

                lbW(lev) = CInt(Math.Truncate((((CDbl(nrt) + 1.0) * (CDbl(nct) + 1.0)) /
                                       CDbl(nn1 + nr1 * nc1 + 1)) - tol))

                nuW(lev) = CInt(Math.Truncate(((CDbl(nrt) + nc1) * (CDbl(nct) + nr1)) /
                                      CDbl(nn1 + nr1 + nc1))) - lbW(lev) + 1

                nrW(lev) = nrt - lbW(lev)
            End While

            alenW(nco) = alenW(lev) + fact(nrW(lev))
            lbW(nco) = nrW(lev)

            Dim v As Double = val + alenW(nco)

            If nro = 2 Then
                v += fact(icoW(1) - lbW(1)) + fact(icoW(2) - lbW(2))
                For i = 3 To nco
                    v += fact(icoW(i) - lbW(i))
                Next
                If v < vmn Then vmn = v

            ElseIf (nro = 3 AndAlso nco = 2) Then
                Dim nn1 As Integer = nn - iroW(irl) + 2
                Dim ic1 As Integer = icoW(1) - lbW(1)
                Dim ic2 As Integer = icoW(2) - lbW(2)
                Dim n11 As Integer = ((iroW(irl + 1) + 1) * (ic1 + 1)) \ nn1
                Dim n12 As Integer = iroW(irl + 1) - n11
                v += fact(n11) + fact(n12) + fact(ic1 - n11) + fact(ic2 - n12)
                If v < vmn Then vmn = v

            Else
                For i = 1 To nco
                    itW(i) = Math.Max(icoW(i) - lbW(i), 0)
                Next

                ' sort itW(1..nco)
                If nco = 2 Then
                    If itW(1) > itW(2) Then
                        Dim tmp As Integer = itW(1) : itW(1) = itW(2) : itW(2) = tmp
                    End If
                Else
                    SortAsc1Based(itW, nco)
                End If

                ' Compute hash key
                Dim keyVal As Long = CLng(itW(1)) * CLng(kyy) + CLng(itW(2))
                For i = 3 To nco
                    keyVal = CLng(itW(i)) + keyVal * CLng(kyy)
                Next

                If keyVal < -1 Then
                    AppGlobals.BSerr.LogAndThrow(New ApplicationException("Bug in FEXACT: negative key computed in F3xact."))
                End If

                Dim ipn As Integer = CInt((keyVal Mod ldst) + 1)

                ' Find empty/occupied position in hash table bank
                Dim itp As Integer
                Dim ii As Integer

                ii = ks + ipn
                For itp = ipn To ldst
                    If istW(ii) < 0 Then GoTo L180
                    If istW(ii) = keyVal Then GoTo L190
                    ii += 1
                Next

                ii = ks + 1
                For itp = 1 To ipn - 1
                    If istW(ii) < 0 Then GoTo L180
                    If istW(ii) = keyVal Then GoTo L190
                    ii += 1
                Next

                AppGlobals.BSerr.LogAndThrow(New ApplicationException("Stack length exceeded in F3xact (ist/itc bank full)."))

L180:
                istW(ii) = keyVal
                stvW(ii) = v
                nst += 1
                ii = nst + ks
                itcW(ii) = itp
                GoTo LoopNode

L190:
                stvW(ii) = Math.Min(v, stvW(ii))
            End If

            GoTo LoopNode

L200:
            If nitc > 0 Then
                ' Pop item from opposite bank
                Dim itp2 As Integer = itcW(nitc + k) + k
                nitc -= 1

                val = stvW(itp2)
                Dim keyVal2 As Long = istW(itp2)
                istW(itp2) = -1

                ' Decode marginals into icoW(1..nco)
                Dim kk As Long = keyVal2
                For i = nco To 2 Step -1
                    icoW(i) = CInt(kk Mod kyy)
                    kk \= kyy
                Next
                icoW(1) = CInt(kk)

                ' Set up ntW
                ntW(1) = nn - icoW(1)
                For i = 2 To nco
                    ntW(i) = ntW(i - 1) - icoW(i)
                Next

                ' Test for optimality
                Dim xmin2 As Boolean = False
                If iroW(nro) <= iroW(irl) + nco Then
                    Dim r0() As Integer = Slice1B_To0B(iroW, irl, nro)
                    Dim c0() As Integer = Slice1B_To0B(icoW, 1, nco)
                    Dim nd(r0.Length - 1) As Integer
                    Dim ne(c0.Length - 1) As Integer
                    Dim mw(c0.Length - 1) As Integer
                    xmin2 = F10act(nro, r0, nco, c0, val, fact, nd, ne, mw)
                End If

                If (Not xmin2) AndAlso (icoW(nco) <= icoW(1) + nro) Then
                    Dim r0() As Integer = Slice1B_To0B(icoW, 1, nco)
                    Dim c0() As Integer = Slice1B_To0B(iroW, irl, nro)
                    Dim nd(r0.Length - 1) As Integer
                    Dim ne(c0.Length - 1) As Integer
                    Dim mw(c0.Length - 1) As Integer
                    xmin2 = F10act(nco, r0, nro, c0, val, fact, nd, ne, mw)
                End If

                If xmin2 Then
                    If vmn > val Then vmn = val
                    GoTo L200
                Else
                    GoTo LnewNode
                End If

            ElseIf (nro > 2 AndAlso nst > 0) Then
                ' Go to next level / swap banks
                nitc = nst
                nst = 0
                k = ks
                ks = ldst - ks
                nn -= iroW(irl)
                irl += 1
                nro -= 1
                GoTo L200
            End If

            Return -vmn
        End Function


        ''' <summary>
        ''' F4XACT – Computes the shortest path length for a given table.
        ''' 
        ''' nrow : number of rows
        ''' irow : row sums (length nrow, 0-based)
        ''' ncol : number of columns
        ''' icol : column sums (length ncol, 0-based)
        ''' dspt : offset for SP computation
        ''' fact : log-factorials (0..maxN)
        ''' tol  : tolerance
        ''' 
        ''' Returns: shortest path SP (double)
        ''' 
        ''' NOTE:
        ''' - This is a direct translation of the C f4xact routine from R's fexact.c,
        '''   with local workspace arrays allocated using ReDim.
        ''' - Uses helper functions F8xact and F11act already defined in this class.
        ''' </summary>
        Private Function F4xact(nrow As Integer, irow() As Integer, ncol As Integer, icol() As Integer,
                            dspt As Double, fact() As Double, tol As Double) As Double
            Dim i As Integer, j As Integer, k As Integer, l As Integer, m As Integer, n As Integer
            Dim ic1 As Integer, ir1 As Integer, ict As Integer, irt As Integer
            Dim istk As Integer, nco As Integer, nro As Integer
            Dim y As Double, amx As Double, SP As Double

            ' ---- Easy cases first (as in C code) ----
            If nrow = 1 Then
                SP = 0.0
                For i = 0 To ncol - 1
                    SP -= fact(icol(i))
                Next
                Return SP
            End If

            If ncol = 1 Then
                SP = 0.0
                For i = 0 To nrow - 1
                    SP -= fact(irow(i))
                Next
                Return SP
            End If

            ' (We skip the 2x2 special-case from the C code; the general algorithm handles it.)

            ' --------------------------------------------------------------------
            ' Local workspace arrays
            ' C docs: ICSTK is NCOL by (NROW+NCOL+1), IRSTK is NROW by MAX(NROW,NCOL).
            ' We implement them as 2D arrays indexed by [level, index].
            ' level dimension is 1..maxLevel; index is 0-based within that level.
            ' --------------------------------------------------------------------
            Dim maxLevel As Integer = nrow + ncol + 1

            Dim irstk(maxLevel, nrow - 1) As Integer
            Dim icstk(maxLevel, ncol - 1) As Integer

            Dim nrstk(maxLevel) As Integer
            Dim ncstk(maxLevel) As Integer
            Dim lstk(maxLevel) As Integer
            Dim mstk(maxLevel) As Integer
            Dim nstk(maxLevel) As Integer
            Dim ystk(maxLevel) As Double

            ' ---- initialization before loop ----

            ' Reverse irow into first stack level
            For i = 1 To nrow
                irstk(1, i - 1) = irow(nrow - i)
            Next

            ' Reverse icol into first stack level
            For j = 1 To ncol
                icstk(1, j - 1) = icol(ncol - j)
            Next

            nro = nrow
            nco = ncol
            nrstk(1) = nro
            ncstk(1) = nco
            ystk(1) = 0.0
            y = 0.0
            istk = 1
            l = 1
            amx = 0.0
            SP = dspt

            ' ---------------- First LOOP ----------------
            ' This corresponds to the big "do { ... } while(1)" in C.
            ' We will use GoTo labels to mimic the structure closely.
            '
            ' Label L60 is used inside the loop for choosing row/column index.
            ' ---------------------------------------------------------------
FirstLoop:
            Do
                ir1 = irstk(istk, 0)
                ic1 = icstk(istk, 0)

                If ir1 > ic1 Then
                    If nro >= nco Then
                        m = nco - 1 : n = 2
                    Else
                        m = nro - 1 : n = 1
                    End If
                ElseIf ir1 < ic1 Then
                    If nro <= nco Then
                        m = nro - 1 : n = 1
                    Else
                        m = nco - 1 : n = 2
                    End If
                Else
                    If nro <= nco Then
                        m = nro - 1 : n = 1
                    Else
                        m = nco - 1 : n = 2
                    End If
                End If


L60:
                If n = 1 Then
                    i = l : j = 1
                Else
                    i = 1 : j = l
                End If

                ' Convert to 0-based indices for arrays
                Dim rowIdx As Integer = i - 1
                Dim colIdx As Integer = j - 1

                irt = irstk(istk, rowIdx)
                ict = icstk(istk, colIdx)

                y += fact(Math.Min(irt, ict))

                ' We'll construct new level istk+1 row/col vectors in temp arrays
                Dim newRows(nro - 1) As Integer
                Dim newCols(nco - 1) As Integer

                If irt = ict Then
                    ' Eliminate both row and column
                    nro -= 1
                    nco -= 1

                    ' f11act on rows: remove row i
                    Dim curRows(nro) As Integer ' old length = nro+1
                    For k = 0 To nro
                        curRows(k) = irstk(istk, k)
                    Next
                    F11act(curRows, i, nro, newRows)

                    ' f11act on cols: remove column j
                    Dim curCols(nco) As Integer ' old length = nco+1
                    For k = 0 To nco
                        curCols(k) = icstk(istk, k)
                    Next
                    F11act(curCols, j, nco, newCols)

                ElseIf irt > ict Then
                    ' Column exhausted first
                    nco -= 1

                    ' f11act on columns: remove column j
                    Dim curCols(nco) As Integer ' old length = nco+1
                    For k = 0 To nco
                        curCols(k) = icstk(istk, k)
                    Next
                    F11act(curCols, j, nco, newCols)

                    ' f8xact on rows: reduce row i by (irt - ict)
                    ' F8xact expects 1-based arrays (1..nro) so wrap/unwarp.
                    Dim curRows0(nro - 1) As Integer
                    For k = 0 To nro - 1
                        curRows0(k) = irstk(istk, k)
                    Next

                    Dim curRows1() As Integer = To1BasedFrom0Based(curRows0, nro)
                    Dim tmpRows1(nro) As Integer

                    Dim isVal As Integer = irt - ict
                    F8xact(curRows1, isVal, i, nro, tmpRows1)

                    Dim tmpRows0() As Integer = Slice1B_To0B(tmpRows1, 1, nro)
                    For k = 0 To nro - 1
                        newRows(k) = tmpRows0(k)
                    Next
                Else
                    ' Row exhausted first (irt < ict)
                    nro -= 1

                    ' f11act on rows: remove row i
                    Dim curRows(nro) As Integer ' old length = nro+1
                    For k = 0 To nro
                        curRows(k) = irstk(istk, k)
                    Next
                    F11act(curRows, i, nro, newRows)

                    ' f8xact on columns: reduce column j by (ict - irt)
                    ' F8xact expects 1-based arrays (1..nco) so wrap/unwarp.
                    Dim curCols0(nco - 1) As Integer
                    For k = 0 To nco - 1
                        curCols0(k) = icstk(istk, k)
                    Next

                    Dim curCols1() As Integer = To1BasedFrom0Based(curCols0, nco)
                    Dim tmpCols1(nco) As Integer

                    Dim isVal2 As Integer = ict - irt
                    F8xact(curCols1, isVal2, j, nco, tmpCols1)

                    Dim tmpCols0() As Integer = Slice1B_To0B(tmpCols1, 1, nco)
                    For k = 0 To nco - 1
                        newCols(k) = tmpCols0(k)
                    Next
                End If

                ' Store newRows/newCols in next stack level (istk+1)
                For k = 0 To nro - 1
                    irstk(istk + 1, k) = newRows(k)
                Next
                For k = 0 To nco - 1
                    icstk(istk + 1, k) = newCols(k)
                Next

                ' Base conditions for this branch
                If nro = 1 Then
                    For k = 0 To nco - 1
                        y += fact(icstk(istk + 1, k))
                    Next
                    Exit Do
                End If

                If nco = 1 Then
                    For k = 0 To nro - 1
                        y += fact(irstk(istk + 1, k))
                    Next
                    Exit Do
                End If

                ' Push state on stack
                lstk(istk) = l
                mstk(istk) = m
                nstk(istk) = n

                istk += 1
                nrstk(istk) = nro
                ncstk(istk) = nco
                ystk(istk) = y

                l = 1

            Loop   ' end FirstLoop

            ' ---- L90 in the C code ----
            If y > amx Then
                amx = y
                If SP - amx <= tol Then
                    Return -dspt
                End If
            End If

            ' ---- L100 block: backtrack through stack ----
Backtrack:
            Do
                istk -= 1
                If istk = 0 Then
                    SP -= amx
                    If SP - amx <= tol Then
                        Return -dspt
                    Else
                        Return SP - dspt
                    End If
                End If

                l = lstk(istk) + 1

                ' L110 in C: try larger l at this level
                Do
                    If l > mstk(istk) Then
                        ' no more at this level, backtrack further
                        Exit Do
                    End If

                    n = nstk(istk)
                    nro = nrstk(istk)
                    nco = ncstk(istk)
                    y = ystk(istk)

                    If n = 1 Then
                        ' compare row(l) vs row(l-1) using 0-based storage
                        If irstk(istk, l - 1) < irstk(istk, l - 2) Then
                            GoTo L60
                        End If
                    ElseIf n = 2 Then
                        ' compare col(l) vs col(l-1) using 0-based storage
                        If icstk(istk, l - 1) < icstk(istk, l - 2) Then
                            GoTo L60
                        End If
                    End If

                    l += 1
                Loop
            Loop   ' continue backtracking
        End Function


        ''' <summary>
        ''' F5xact — Insert (PUT) a past-path value into hash table + tree.
        '''
        ''' pastp : the past path value (double)
        ''' tol   : tolerance for equality
        ''' kval  : hash key value
        ''' key() : hash table (1..2*ldkey)
        ''' ldkey : length of key table (half)
        ''' ipoin(): index of path-tree root for each hash bucket
        ''' stp()  : stored path values (1..ldstp)
        ''' ldstp  : size of stp()
        ''' ifrq() : frequency at each stp node
        ''' npoin(): linked list pointer
        ''' nr()/nl(): tree right/left pointers
        ''' ifreq : frequency of the value we're inserting
        ''' itop  : top-of-stack counter for STP arrays
        ''' psh   : TRUE → new hash entry; FALSE → existing hash entry
        ''' </summary>
        Private Sub F5xact(ByVal pastp As Double, ByVal tol As Double, ByVal kval As Integer, key() As Integer,
                       ByVal keyOff As Integer, ByVal ldkey As Integer, ipoin() As Integer, stp() As Double,
                       ByVal stpOff As Integer, ByVal ldstp As Integer, ifrq() As Integer, npoin() As Integer,
                       nr() As Integer, nl() As Integer, ByVal ifreq As Integer, ByRef itop As Integer,
                       ByVal psh As Boolean)
            ' R/C uses 0..ldkey-1 hash slots. We map to VB 1..ldkey by +1.
            Dim test1 As Double = pastp - tol
            Dim test2 As Double = pastp + tol

            ' Find hash slot (0-based like C)
            Dim ird0 As Integer = kval Mod ldkey
            If ird0 < 0 Then ird0 += ldkey

            Dim itp0 As Integer
            Dim slotVB As Integer

            ' Probe forward ird0..ldkey-1
            For itp0 = ird0 To ldkey - 1
                slotVB = keyOff + itp0 + 1
                If key(slotVB) = kval Then GoTo L_FOUND_HASH
                If key(slotVB) < 0 Then GoTo L_INSERT_HASH
            Next

            ' Probe wrap 0..ird0-1
            For itp0 = 0 To ird0 - 1
                slotVB = keyOff + itp0 + 1
                If key(slotVB) = kval Then GoTo L_FOUND_HASH
                If key(slotVB) < 0 Then GoTo L_INSERT_HASH
            Next

            AppGlobals.BSerr.LogAndThrow(New ApplicationException("LDKEY too small for problem (hash full in F5xact)."))

L_INSERT_HASH:
            key(slotVB) = kval
            itop += 1
            If itop > ldstp Then AppGlobals.BSerr.LogAndThrow(New ApplicationException("LDSTP too small (STP overflow in F5xact)."))

            ipoin(slotVB) = itop

            Dim nodeVB As Integer = stpOff + itop
            stp(nodeVB) = pastp
            ifrq(nodeVB) = ifreq
            npoin(nodeVB) = -1
            nl(nodeVB) = -1
            nr(nodeVB) = -1
            Return

L_FOUND_HASH:
            Dim root As Integer = ipoin(slotVB)
            If root <= 0 Then AppGlobals.BSerr.LogAndThrow(New ApplicationException("Corrupt ipoin() in F5xact (root<=0)."))

            ' BST search by stp() tolerance
            Dim ipn As Integer = root
            Do While ipn > 0
                Dim ipnVB As Integer = stpOff + ipn
                If stp(ipnVB) < test1 Then
                    ipn = nl(ipnVB)
                ElseIf stp(ipnVB) > test2 Then
                    ipn = nr(ipnVB)
                Else
                    ifrq(ipnVB) += ifreq
                    Return
                End If
            Loop

            ' Need a new node
            itop += 1
            If itop > ldstp Then AppGlobals.BSerr.LogAndThrow(New ApplicationException("LDSTP too small (tree overflow in F5xact)."))

            ' Find insertion location again from root
            ipn = root
            Dim parent As Integer = root

L_DESCEND:
            Dim ipnVB2 As Integer = stpOff + ipn
            If stp(ipnVB2) < test1 Then
                parent = ipn
                ipn = nl(ipnVB2)
                If ipn > 0 Then GoTo L_DESCEND
                nl(ipnVB2) = itop
            ElseIf stp(ipnVB2) > test2 Then
                parent = ipn
                ipn = nr(ipnVB2)
                If ipn > 0 Then GoTo L_DESCEND
                nr(ipnVB2) = itop
            Else
                AppGlobals.BSerr.LogAndThrow(New ApplicationException("Unexpected equality state in F5xact insert."))
            End If

            ' Threaded next-node chain (same as R: npoin[new] = npoin[parent]; npoin[parent] = new)
            Dim newVB As Integer = stpOff + itop
            Dim parentVB As Integer = stpOff + parent
            npoin(newVB) = npoin(parentVB)
            npoin(parentVB) = itop

            stp(newVB) = pastp
            ifrq(newVB) = ifreq
            nl(newVB) = -1
            nr(newVB) = -1
        End Sub



        ''' <summary>
        ''' F6xact — Pop a node from the hash table ("GET").
        '''
        ''' nrow  : number of rows
        ''' irow() : returned row marginals (1-based: [1..nrow])
        ''' kyy() : multipliers used to decode the hash key
        ''' key() : hash table (1..ldkey)
        ''' ldkey : size of key table
        ''' last  : index of last popped key (1-based)
        ''' ipn   : output pointer (the position in stp() list)
        '''
        ''' Returns True if no more nodes are available (i.e., key table exhausted).
        ''' </summary>
        Private Function F6xact(nrow As Integer, irow() As Integer, kyy() As Integer,
                            key() As Integer, ByVal keyOff As Integer, ByVal ldkey As Integer,
                            ByRef last As Integer, ByRef ipo As Integer) As Boolean
            ' Advance "last" to the next occupied key slot in this bank.
            Do
                last += 1

                ' No more keys in this bank
                If last > ldkey Then
                    last = 0
                    ipo = 0
                    Return True
                End If

                Dim slotVB As Integer = keyOff + last

                ' Skip empty/free slots (negative)
                If key(slotVB) < 0 Then
                    ' keep looping
                Else
                    ' Found an occupied slot → decode kval and return this mother
                    Dim kval As Integer = key(slotVB)
                    key(slotVB) = -9999   ' mark as free

                    ' Decode kval back into irow(1..nrow)
                    For j As Integer = nrow - 1 To 1 Step -1
                        irow(j + 1) = kval \ kyy(j + 1)
                        kval -= irow(j + 1) * kyy(j + 1)
                    Next
                    irow(1) = kval

                    ipo = last
                    Return False
                End If
            Loop
        End Function


        ''' <summary>
        ''' Generate new nodes for given marginal totals (translation of f7xact).
        ''' 
        ''' nrow : number of rows
        ''' imax : row marginal totals [1..nrow]
        ''' idif : column counts for the new column [1..nrow] (in/out)
        ''' k    : row index to decrement (1-based, in/out)
        ''' ks   : row index to increment (1-based, in/out)
        ''' iflag: 0 => new table generated; 1 => no additional tables
        ''' 
        ''' NOTE: Uses 1-based indexing for imax() and idif() to match the C logic.
        ''' </summary>
        Private Sub F7xact(nrow As Integer, imax() As Integer, idif() As Integer,
                       ByRef k As Integer, ByRef ks As Integer, ByRef iflag As Integer)
            Dim i As Integer, m As Integer, kk As Integer, mm As Integer
            iflag = 0

            ' ---- safety guards (prevents IndexOutOfRange) ----
            If k < 1 Then
                iflag = 1
                Return
            End If
            If k > nrow Then k = nrow
            If ks < 0 OrElse ks > nrow Then ks = 0

            ' Find node which can be incremented, ks
            If ks = 0 Then
                Do
                    ks += 1
                Loop While ks <= nrow AndAlso idif(ks) = imax(ks)

                ' If everything is "full", there is no place to increment → done
                If ks > nrow Then
                    iflag = 1
                    Return
                End If
            End If

            ' Find node to decrement (> ks)
            If idif(k) > 0 AndAlso k > ks Then
                idif(k) -= 1
                Do
                    k -= 1
                Loop While imax(k) = 0

                m = k

                ' Find node to increment (>= ks)
                While m >= 1 AndAlso idif(m) >= imax(m)
                    m -= 1
                End While
                If m < 1 Then
                    ' No place to increment → fallback to the reallocation logic
                    GoTo LoopLabel
                End If


                idif(m) += 1

                ' Change ks if that row is now "full"
                If m = ks AndAlso idif(m) = imax(m) Then
                    ks = k
                End If

            Else
                ' No simple decrement; more involved reallocation
LoopLabel:
                ' Check for finish
                For kk = k + 1 To nrow
                    If idif(kk) > 0 Then
                        GoTo L70
                    End If
                Next

                iflag = 1
                Return

L70:
                ' Reallocate counts
                mm = 1
                For i = 1 To k
                    mm += idif(i)
                    idif(i) = 0
                Next

                k = kk

                Do
                    k -= 1
                    m = Math.Min(mm, imax(k))
                    idif(k) = m
                    mm -= m
                Loop While (mm > 0 AndAlso k <> 1)

                ' Check that all counts reallocated
                If mm > 0 Then
                    If kk <> nrow Then
                        k = kk
                        GoTo LoopLabel
                    End If
                    iflag = 1
                    Return
                End If

                ' Get ks
                idif(kk) -= 1
                ks = 0
                Do
                    ks += 1
                    If ks > k Then
                        Return
                    End If
                Loop While idif(ks) >= imax(ks)
            End If
        End Sub

        ''' <summary>
        ''' Reduce a vector when there is a zero element (translation of f8xact).
        ''' 
        ''' irow  : row counts [1..izero]
        ''' isVal : indicator value to insert
        ''' i1    : position at which insertion logic begins (1-based)
        ''' izero : position of the zero (1-based)
        ''' newArr: output row counts [1..izero]
        ''' 
        ''' NOTE: Uses 1-based indexing for irow() and newArr().
        ''' </summary>
        Private Sub F8xact(irow() As Integer, isVal As Integer, i1 As Integer, izero As Integer, newArr() As Integer)
            Dim i As Integer

            If i1 < 1 OrElse i1 > izero Then AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException("F8xact: i1 out of range"))
            If irow.GetUpperBound(0) < izero Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("F8xact: irow too small"))
            If newArr.GetUpperBound(0) < izero Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("F8xact: newArr too small"))

            ' Copy unchanged prefix
            For i = 1 To i1 - 1
                newArr(i) = irow(i)
            Next

            ' Shift elements until appropriate insertion place
            For i = i1 To izero - 1
                If isVal >= irow(i + 1) Then
                    Exit For
                End If
                newArr(i) = irow(i + 1)
            Next

            ' Insert new value
            newArr(i) = isVal

            ' Copy tail
            Do
                i += 1
                If i > izero Then Exit Do
                newArr(i) = irow(i)
            Loop
        End Sub

        ''' <summary>
        ''' Compute log of multinomial coefficient (translation of f9xact).
        ''' 
        ''' n    : length of ir()
        ''' ntot : total count (factorial in numerator)
        ''' ir   : counts in denominator [0..n-1]
        ''' fact : table of log-factorials [0..ntot]
        ''' </summary>
        Private Function F9xact(n As Integer, ntot As Integer, ir() As Integer, fact() As Double) As Double
            Dim d As Double = fact(ntot)
            For k As Integer = 0 To n - 1
                d -= fact(ir(k))
            Next
            Return d
        End Function

        ''' <summary>
        ''' Special-case shortest path computation (translation of f10act).
        ''' 
        ''' nrow : number of rows
        ''' irow : row totals [0..nrow-1]
        ''' ncol : number of columns
        ''' icol : column totals [0..ncol-1]
        ''' val  : shortest path (input/output); incremented inside
        ''' fact : table of log-factorials
        ''' nd   : workspace [0..nrow-1]
        ''' ne   : workspace [0..ncol-1]
        ''' m    : workspace [0..ncol-1]
        ''' 
        ''' Returns True if shortest path obtained (XMIN in C code).
        ''' </summary>
        Private Function F10act(nrow As Integer, irow() As Integer, ncol As Integer, icol() As Integer,
                            ByRef val As Double, fact() As Double, nd() As Integer, ne() As Integer, m() As Integer) As Boolean
            Dim i As Integer, isSum As Integer, ix As Integer

            ' Initialize ND
            For i = 0 To nrow - 2
                nd(i) = 0
            Next

            ' First column
            isSum = icol(0) \ nrow
            ix = icol(0) - nrow * isSum
            ne(0) = isSum
            m(0) = ix
            If ix <> 0 Then
                nd(ix - 1) += 1
            End If

            ' Remaining columns
            For i = 1 To ncol - 1
                ix = icol(i) \ nrow
                ne(i) = ix
                isSum += ix
                ix = icol(i) - nrow * ix
                m(i) = ix
                If ix <> 0 Then
                    nd(ix - 1) += 1
                End If
            Next

            ' Cumulative ND from bottom (except last two entries)
            For i = nrow - 3 To 0 Step -1
                nd(i) += nd(i + 1)
            Next

            ' Feasibility check
            ix = 0
            For i = nrow To 2 Step -1
                ix += isSum + nd(nrow - i) - irow(i - 1)
                If ix < 0 Then
                    Return False
                End If
            Next

            ' Accumulate val
            For i = 0 To ncol - 1
                ix = ne(i)
                isSum = m(i)
                val += isSum * fact(ix + 1) + (nrow - isSum) * fact(ix)
            Next

            Return True
        End Function

        ''' <summary>
        ''' Revise row totals (translation of f11act).
        ''' 
        ''' irow  : input row totals
        ''' i1    : indicator (1-based in the algorithm)
        ''' i2    : indicator (1-based in the algorithm)
        ''' newArr: output row totals
        ''' 
        ''' NOTE: In the C code, i1 and i2 are 1-based indices.
        ''' The loops use 0-based indexing for the arrays but treat i1, i2 as 1-based.
        ''' </summary>
        Private Sub F11act(irow() As Integer, i1 As Integer, i2 As Integer, newArr() As Integer)
            Dim i As Integer
            ' Copy elements before (i1 - 1)
            For i = 0 To i1 - 2
                newArr(i) = irow(i)
            Next
            ' Shift elements from i1..i2 left by 1
            For i = i1 To i2
                newArr(i - 1) = irow(i)
            Next
        End Sub

    End Class
End Namespace