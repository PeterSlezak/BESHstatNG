# Proposed controlled native smoke ? NOT EXECUTED

Status: **NATIVE_NOT_RUN**. Requires separate owner/native-risk authorization. Before any later run record HWND/PID, loaded XLL/assembly hashes, architecture, branch/source/build hashes, workbook/session attribution and cleanup plan. No original control release/discharge or scientific golden is implied.

Use only a new synthetic workbook with no personal data. Verify actual plotted Series.XValues/Values under the later authorized capture protocol, then appearance separately. This batch only supplies host-free expected coordinates and source ownership.

XYZ fixture: X=[-3,0,2,8], Y=[8,2,0,-3], Z=[-6,0,4,16]; labels=[A,B,C,D]. ScaleAxis=False; X/Z flipped, Y normal; rotations X=120,Y=180(default),Z=60; zoom=0,shiftX=50,shiftY=50. Enable grid, all three plane series, labels and Z-drop. Add sphere(center0,diameter2) then ellipsoid(center[2,1,2],diameters[2,4,6]); each lat2/lon3/pts16. Their bounds lie inside data bounds, so the frozen scene is identical to actual pre-edit capture case `xyz/0-5-0`.

Expected data XY/drop vectors (decimal view; exact IEEE in pre-edit capture):

```json
{
  "xs_norm_data": [
    0.1830127018922193,
    -0.1530011547174745,
    -0.219551228132827,
    -0.1830127018922193
  ],
  "ys_norm_data": [
    1.0915063509461098,
    1.1097756140664137,
    1.0765005773587373,
    0.9084936490538902
  ],
  "error_bars": [
    0.8660254037844388,
    0.6298366572977737,
    0.4723774929733302,
    0.0
  ],
  "tx": [
    5.0,
    5.0,
    5.0
  ]
}
```

Native chart domain remains X[-1,1],Y[0,2]. Series order: Cage(11 points),x_ticks,y_ticks,z_ticks; XY_X_gridlines,XY_Y_gridlines,XZ_X_gridlines,XZ_Y_gridlines,YZ_Y_gridlines,YZ_Z_gridlines; x_plane,y_plane,z_plane; WireSphere,WireEllipsoid; Data. Sixteen series. Each tick/grid uses base,tip,separator triples; tick counts are tx+1 from the corpus. Each plane/data series has four points. Each wire has90 points,structural breaks at zero-based17,35,53,71,89 (Excel18,36,54,72,90); native point-hiding also hides the following connecting segment exactly as source. Labels retain original input order. Repeat after changing rotation and flipping Z off; compare numeric vectors to the frozen-fixture tests, including fixed normalized Z=-.5 drop baseline. Do not infer native visibility from this packet.

Scatter fixture: five rows [[0,-1,-2],[1,2,3],[4,7,10],[9,14,19],[16,23,30]], names=[A,B,C],constructor dMargin=.37. Effective margin remains.1. Domain[0,1] on both axes. HorizontalGrid1,HorizontalGrid2 then VerticalGrid1,VerticalGrid2,each two points at1/3 and2/3. Pair series A vs B,A vs C,B vs A,B vs C,C vs A,C vs B,each five points. With defaults append Regression Lines(18 points;breaks at zero-based2,5,8,11,14,17),Variabe Names(three anchors at1/6,1/2,5/6;historical spelling retained),Correlation Coefficient(nine anchors;i outer/j inner;diagonal labels hidden). Thirteen series. Upper triangle text uses r= with0.00;lower p= with0.0000 as current source. Compare scaled/label/regression/correlation values to `scatter/3-3-0-0` in the capture corpus (the rows above match that fixture exactly).

Repeat all four correlation/regression toggle combinations. Expected series totals: both on13;correlation only12;regression only12;both off11. Names,series order,marker/color,legend and axis deletion remain Windows-owned. Actual chart creation,point hiding,labels,animation/export/events and COM cleanup are **NOT_RUN** here.
