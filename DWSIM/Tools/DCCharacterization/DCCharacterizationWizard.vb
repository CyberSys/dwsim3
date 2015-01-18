'    Petroleum Characterization Wizard
'    Copyright 2009 Daniel Wagner O. de Medeiros
'
'    This file is part of DWSIM.
'
'    DWSIM is free software: you can redistribute it and/or modify
'    it under the terms of the GNU General Public License as published by
'    the Free Software Foundation, either version 3 of the License, or
'    (at your option) any later version.
'
'    DWSIM is distributed in the hope that it will be useful,
'    but WITHOUT ANY WARRANTY; without even the implied warranty of
'    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'    GNU General Public License for more details.
'
'    You should have received a copy of the GNU General Public License
'    along with DWSIM.  If not, see <http://www.gnu.org/licenses/>.

Imports System.Math
Imports DWSIM.DWSIM.MathEx
Imports DWSIM.DWSIM.ClassesBasicasTermodinamica
Imports DWSIM.DWSIM.Utilities.PetroleumCharacterization.Methods
Imports DWSIM.DWSIM.Utilities.PetroleumCharacterization


Public Class DCCharacterizationWizard

    Private Class tmpcomp
        Public tbpm As Double
        Public tbp0 As Double
        Public tbpf As Double
        Public fv0 As Double
        Public fvf As Double
        Public fvm As Double
        Public tv1 As Double
        Public tv2 As Double
    End Class

    Dim nf As String
    Dim cv As DWSIM.SistemasDeUnidades.Conversor
    Dim su As DWSIM.SistemasDeUnidades.Unidades
    Dim form As FormFlowsheet

    Dim pxt, pyt, pxv1, pyv1, pxv2, pyv2, pxm, pym, pxd, pyd As ArrayList
    Dim gxt, gyt, gxv1, gyv1, gxv2, gyv2, gxm, gym, gxd, gyd As ArrayList
    Dim Tmin, Tmax As Double
    Dim ccol As Dictionary(Of String, Substancia)
    Dim tccol As List(Of tmpcomp)

    Private Sub DCCharacterizationWizard_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        form = My.Application.ActiveSimulation
        su = form.Options.SelectedUnitSystem
        nf = form.Options.NumberFormat
        ccol = New Dictionary(Of String, Substancia)
        tccol = New List(Of tmpcomp)

        Me.ComboBoxAF.SelectedIndex = 1
        Me.ComboBoxDistMethod.SelectedIndex = 0
        Me.ComboBoxBasis.SelectedIndex = 0
        Me.ComboBoxMW.SelectedIndex = 0
        Me.ComboBoxPC.SelectedIndex = 0
        Me.ComboBoxSG.SelectedIndex = 0
        Me.ComboBoxTC.SelectedIndex = 0
        Me.ComboBoxViscM.SelectedIndex = 0

        With Me.DataGridView1.Columns
            .Item("temp").HeaderText += " (" & su.spmp_temperature & ")"
            .Item("mm").HeaderText += " (" & su.spmp_molecularWeight & ")"
            .Item("visc1").HeaderText += " (" & su.spmp_cinematic_viscosity & ")"
            .Item("visc2").HeaderText += " (" & su.spmp_cinematic_viscosity & ")"
        End With

        With Me.DataGridView2.Columns
            .Item(2).HeaderText += " (" & su.spmp_temperature & ")"
            .Item(4).HeaderText += " (" & su.spmp_molecularWeight & ")"
            .Item(5).HeaderText += " (" & su.spmp_temperature & ")"
            .Item(6).HeaderText += " (" & su.spmp_pressure & ")"
            .Item(8).HeaderText += " (" & su.spmp_cinematic_viscosity & ")"
            .Item(9).HeaderText += " (" & su.spmp_cinematic_viscosity & ")"
        End With

        P0.BringToFront()

    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click, Button4.Click, Button7.Click, Button10.Click, Button13.Click, Button16.Click, Button19.Click, Button22.Click
        '"cancel" buttons
        Me.Close()
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        P1.BringToFront()
    End Sub

    Private Sub Button6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button6.Click
        'preencher datagrid1 para insercao de dados

        If Me.CheckBoxMW.Checked = False Then Me.DataGridView1.Columns("mm").Visible = False Else Me.DataGridView1.Columns("mm").Visible = True
        If Me.CheckBoxSG.Checked = False Then Me.DataGridView1.Columns("dens").Visible = False Else Me.DataGridView1.Columns("dens").Visible = True
        If Me.CheckBoxVISC.Checked = False Then
            Me.DataGridView1.Columns("visc1").Visible = False
            Me.DataGridView1.Columns("visc2").Visible = False
        Else
            Me.DataGridView1.Columns("visc1").Visible = True
            Me.DataGridView1.Columns("visc2").Visible = True
        End If

        P2.BringToFront()
    End Sub

    Private Sub Button9_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button9.Click

        P3.BringToFront()
        Button28_Click(sender, e)

    End Sub

    Private Sub Button5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button5.Click
        P0.BringToFront()
    End Sub

    Private Sub Button8_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button8.Click
        P1.BringToFront()
    End Sub

    Private Sub Button11_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button11.Click
        P2.BringToFront()
    End Sub

    Private Sub Button12_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button12.Click
        P4.BringToFront()
    End Sub

    Private Sub Button14_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button14.Click
        P3.BringToFront()
    End Sub

    Private Sub Button15_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button15.Click

        'generate pseudos from number or temperature cuts

        Dim i As Integer = 0
        Dim method As Integer = 0
        Dim tbp(), tbpx() As Double

        If Me.RadioButtonGenOpt1.Checked Then method = 0 Else method = 1

        Dim fittedt As New ArrayList
        Dim coeff(6), r_pv, n_pv, aa, bb, cc, dd, ee, ff, gg As Double
        Dim obj As Object

        tbp = Nothing
        tbpx = Nothing

        'generate polynomial from input data

        If Me.ComboBoxDistMethod.SelectedItem.ToString.Contains("ASTM D2892") Then
            'tbp
            tbp = pyt.ToArray(GetType(Double))
            tbpx = pxt.ToArray(GetType(Double))
        ElseIf Me.ComboBoxDistMethod.SelectedItem.ToString.Contains("ASTM D86") Then
            'd86
            Dim T0, T10, T30, T50, T70, T90, T100 As Double
            'interpolate to obtain points
            Dim w As Double() = Nothing
            Interpolation.ratinterpolation.buildfloaterhormannrationalinterpolant(pxt.ToArray(GetType(Double)), pxt.Count, 0.3, w)
            T0 = Interpolation.polinterpolation.barycentricinterpolation(pxt.ToArray(GetType(Double)), pyt.ToArray(GetType(Double)), w, pxt.Count, 0)
            T10 = Interpolation.polinterpolation.barycentricinterpolation(pxt.ToArray(GetType(Double)), pyt.ToArray(GetType(Double)), w, pxt.Count, 0.1)
            T30 = Interpolation.polinterpolation.barycentricinterpolation(pxt.ToArray(GetType(Double)), pyt.ToArray(GetType(Double)), w, pxt.Count, 0.3)
            T50 = Interpolation.polinterpolation.barycentricinterpolation(pxt.ToArray(GetType(Double)), pyt.ToArray(GetType(Double)), w, pxt.Count, 0.5)
            T70 = Interpolation.polinterpolation.barycentricinterpolation(pxt.ToArray(GetType(Double)), pyt.ToArray(GetType(Double)), w, pxt.Count, 0.7)
            T90 = Interpolation.polinterpolation.barycentricinterpolation(pxt.ToArray(GetType(Double)), pyt.ToArray(GetType(Double)), w, pxt.Count, 0.9)
            T100 = Interpolation.polinterpolation.barycentricinterpolation(pxt.ToArray(GetType(Double)), pyt.ToArray(GetType(Double)), w, pxt.Count, 1.0)
            'tbp
            tbp = DistillationCurveConversion.ASTMD86ToPEV_Riazi(New Double() {T0, T10, T30, T50, T70, T90, T100})
            tbpx = New Double() {0.000001, 0.1#, 0.3#, 0.5#, 0.7#, 0.9#, 1.0#}
        ElseIf Me.ComboBoxDistMethod.SelectedItem.ToString.Contains("ASTM D1160") Then
            'vacuum
            Dim T0, T10, T30, T50, T70, T90, T100 As Double
            'interpolate to obtain points
            Dim w As Double() = Nothing
            Interpolation.ratinterpolation.buildfloaterhormannrationalinterpolant(pxt.ToArray(GetType(Double)), pxt.Count, 0.3, w)
            T0 = Interpolation.polinterpolation.barycentricinterpolation(pxt.ToArray(GetType(Double)), pyt.ToArray(GetType(Double)), w, pxt.Count, 0)
            T10 = Interpolation.polinterpolation.barycentricinterpolation(pxt.ToArray(GetType(Double)), pyt.ToArray(GetType(Double)), w, pxt.Count, 0.1)
            T30 = Interpolation.polinterpolation.barycentricinterpolation(pxt.ToArray(GetType(Double)), pyt.ToArray(GetType(Double)), w, pxt.Count, 0.3)
            T50 = Interpolation.polinterpolation.barycentricinterpolation(pxt.ToArray(GetType(Double)), pyt.ToArray(GetType(Double)), w, pxt.Count, 0.5)
            T70 = Interpolation.polinterpolation.barycentricinterpolation(pxt.ToArray(GetType(Double)), pyt.ToArray(GetType(Double)), w, pxt.Count, 0.7)
            T90 = Interpolation.polinterpolation.barycentricinterpolation(pxt.ToArray(GetType(Double)), pyt.ToArray(GetType(Double)), w, pxt.Count, 0.9)
            T100 = Interpolation.polinterpolation.barycentricinterpolation(pxt.ToArray(GetType(Double)), pyt.ToArray(GetType(Double)), w, pxt.Count, 1.0)
            'tbp
            tbp = DistillationCurveConversion.ASTMD1160ToPEVsub_Wauquier(New Double() {T0, T10, T30, T50, T70, T90, T100})
            Dim K As Double = CDbl(Me.TextBoxKAPI.Text)
            tbp(0) = DistillationCurveConversion.PEVsubToPEV_MaxwellBonnel(tbp(0), 1333, K)
            tbp(1) = DistillationCurveConversion.PEVsubToPEV_MaxwellBonnel(tbp(1), 1333, K)
            tbp(2) = DistillationCurveConversion.PEVsubToPEV_MaxwellBonnel(tbp(2), 1333, K)
            tbp(3) = DistillationCurveConversion.PEVsubToPEV_MaxwellBonnel(tbp(3), 1333, K)
            tbp(4) = DistillationCurveConversion.PEVsubToPEV_MaxwellBonnel(tbp(4), 1333, K)
            tbp(5) = DistillationCurveConversion.PEVsubToPEV_MaxwellBonnel(tbp(5), 1333, K)
            tbp(6) = DistillationCurveConversion.PEVsubToPEV_MaxwellBonnel(tbp(6), 1333, K)
            tbpx = New Double() {0.000001, 0.1#, 0.3#, 0.5#, 0.7#, 0.9#, 1.0#}
        ElseIf Me.ComboBoxDistMethod.SelectedItem.ToString.Contains("ASTM D2887") Then
            'simulated
            Dim T5, T10, T30, T50, T70, T90, T95, T100 As Double
            'interpolate to obtain points
            Dim w As Double() = Nothing
            Interpolation.ratinterpolation.buildfloaterhormannrationalinterpolant(pxt.ToArray(GetType(Double)), pxt.Count, 0.8, w)
            T5 = Interpolation.polinterpolation.barycentricinterpolation(pxt.ToArray(GetType(Double)), pyt.ToArray(GetType(Double)), w, pxt.Count, 0.05#)
            T10 = Interpolation.polinterpolation.barycentricinterpolation(pxt.ToArray(GetType(Double)), pyt.ToArray(GetType(Double)), w, pxt.Count, 0.1#)
            T30 = Interpolation.polinterpolation.barycentricinterpolation(pxt.ToArray(GetType(Double)), pyt.ToArray(GetType(Double)), w, pxt.Count, 0.3#)
            T50 = Interpolation.polinterpolation.barycentricinterpolation(pxt.ToArray(GetType(Double)), pyt.ToArray(GetType(Double)), w, pxt.Count, 0.5#)
            T70 = Interpolation.polinterpolation.barycentricinterpolation(pxt.ToArray(GetType(Double)), pyt.ToArray(GetType(Double)), w, pxt.Count, 0.7#)
            T90 = Interpolation.polinterpolation.barycentricinterpolation(pxt.ToArray(GetType(Double)), pyt.ToArray(GetType(Double)), w, pxt.Count, 0.9#)
            T95 = Interpolation.polinterpolation.barycentricinterpolation(pxt.ToArray(GetType(Double)), pyt.ToArray(GetType(Double)), w, pxt.Count, 0.95#)
            T100 = Interpolation.polinterpolation.barycentricinterpolation(pxt.ToArray(GetType(Double)), pyt.ToArray(GetType(Double)), w, pxt.Count, 1.0#)
            'tbp
            tbp = DistillationCurveConversion.ASTMD2887ToPEV_Daubert(New Double() {T5, T10, T30, T50, T70, T90, T95, T100})
            tbpx = New Double() {0.05#, 0.1#, 0.3#, 0.5#, 0.7#, 0.9#, 0.95#, 1.0#}
        End If

        Tmin = Common.Min(tbp)
        Tmax = Common.Max(tbp)

        'y = 10358x5 - 15934x4 + 11822x3 - 4720,2x2 + 1398,2x + 269,23
        'R� = 1

        Dim inest(6) As Double

        If Me.ComboBoxDistMethod.SelectedItem.ToString.Contains("ASTM D2892") Then
            Dim w2 As Double() = Nothing
            Interpolation.ratinterpolation.buildfloaterhormannrationalinterpolant(tbpx, tbpx.Length, 0.8, w2)
            inest(0) = Interpolation.polinterpolation.barycentricinterpolation(tbpx, tbp, w2, tbpx.Length, 0)
        Else
            inest(0) = Tmin
        End If
        inest(1) = 1398
        inest(2) = 4720
        inest(3) = 11821
        inest(4) = 15933
        inest(5) = 10358
        inest(6) = -3000

        Dim lmfit As New DistillationCurveConversion.TBPFit()
        obj = lmfit.GetCoeffs(tbpx, tbp, inest, 0.0000000001, 0.00000001, 0.00000001, 1000)
        coeff = obj(0)
        r_pv = obj(2)
        n_pv = obj(3)

        aa = coeff(0)
        bb = coeff(1)
        cc = coeff(2)
        dd = coeff(3)
        ee = coeff(4)
        ff = coeff(5)
        gg = coeff(6)

        'TBP(K) = aa + bb*fv + cc*fv^2 + dd*fv^3 + ee*fv^4 + ff*fv^5 (fv 0 ~ 1)

        fittedt.Clear()
        For i = 0 To tbp.Length - 1
            fittedt.Add(cv.ConverterDoSI(su.spmp_temperature, coeff(0) + coeff(1) * tbpx(i) + coeff(2) * tbpx(i) ^ 2 + coeff(3) * tbpx(i) ^ 3 + coeff(4) * tbpx(i) ^ 4 + coeff(5) * tbpx(i) ^ 5 + coeff(6) * tbpx(i) ^ 6))
        Next

        'create pseudos

        If method = 0 Then
            Dim np As Integer = CInt(Me.TextBoxNumberOfPseudos.Text)
            Dim deltaT As Double = (Tmax - Tmin) / (np)
            Dim t0, fv0 As Double
            t0 = Tmin
            fv0 = Common.Min(tbpx)
            tccol.Clear()
            For i = 0 To np - 1
                Dim tc As New tmpcomp
                tc.tbp0 = t0
                tc.tbpf = t0 + deltaT
                tc.fv0 = GetFV(coeff, fv0, tc.tbp0)
                tc.fvf = GetFV(coeff, fv0, tc.tbpf)
                tc.fvm = tc.fv0 + (tc.fvf - tc.fv0) / 2
                tc.tbpm = GetT(coeff, tc.fvm)
                tccol.Add(tc)
                t0 = t0 + deltaT
                fv0 = tc.fvf
            Next
        Else
            'parse textbox
            Dim temps() As String
            Dim text As String = Me.TextBoxTempCuts.Text
            temps = text.Split(vbNewLine)
            Dim np As Integer = temps.Length + 1
            Dim deltaT As Double = (Tmax - Tmin) / (np)
            Dim t0, fv0 As Double
            t0 = Tmin
            fv0 = Common.Min(tbpx)
            tccol.Clear()
            For i = 0 To np - 1
                Dim tc As New tmpcomp
                tc.tbp0 = t0
                If i = np - 1 Then
                    tc.tbpf = Tmax
                Else
                    tc.tbpf = cv.ConverterParaSI(su.spmp_temperature, temps(i))
                End If
                tc.fv0 = GetFV(coeff, fv0, tc.tbp0)
                tc.fvf = GetFV(coeff, fv0, tc.tbpf)
                tc.fvm = tc.fv0 + (tc.fvf - tc.fv0) / 2
                tc.tbpm = GetT(coeff, tc.fvm)
                tccol.Add(tc)
                fv0 = tc.fvf
                If i < np - 1 Then t0 = cv.ConverterParaSI(su.spmp_temperature, temps(i))
            Next
        End If

        With Me.GraphComps.GraphPane
            .GraphObjList.Clear()
            .CurveList.Clear()
            .YAxisList.Clear()
            Dim ya0 As New ZedGraph.YAxis("T (" & su.spmp_temperature & ")")
            .YAxisList.Add(ya0)
            With .AddCurve(DWSIM.App.GetLocalString("PF_TBP"), tbpx, fittedt.ToArray(GetType(Double)), Color.Black)
                .Line.IsVisible = True
                .Line.IsSmooth = True
                .Color = Color.Red
                .Symbol.Type = ZedGraph.SymbolType.Circle
                .Symbol.Fill.Color = Color.Red
                .Symbol.Fill.IsVisible = True
                .YAxisIndex = Me.GraphComps.GraphPane.YAxisList.IndexOf("T (" & su.spmp_temperature & ")")
            End With
            For i = 0 To tccol.Count - 1
                Dim box As New ZedGraph.BoxObj(tccol(i).fv0, cv.ConverterDoSI(su.spmp_temperature, tccol(i).tbpm), tccol(i).fvf - tccol(i).fv0, cv.ConverterDoSI(su.spmp_temperature, tccol(i).tbpm), Color.FromArgb(100, Color.Red), Color.Empty)
                box.Fill = New ZedGraph.Fill(Color.FromArgb(100, Color.Red), Color.White, 90.0F)
                box.Fill.IsScaled = True
                box.ZOrder = ZedGraph.ZOrder.E_BehindCurves
                box.IsClippedToChartRect = True
                box.Location.CoordinateFrame = ZedGraph.CoordType.AxisXYScale
                Me.GraphComps.GraphPane.GraphObjList.Add(box)
            Next
            With .Legend
                .Border.IsVisible = False
                .Position = ZedGraph.LegendPos.BottomCenter
                .IsHStack = True
            End With
            With .XAxis
                .Title.Text = DWSIM.App.GetLocalString("PF_FVAP")
                .Scale.Min = 0.0#
                .Scale.Max = 1.0#
            End With
            .Margin.Top = 20
            .Title.IsVisible = False
            Me.GraphComps.IsAntiAlias = True
            .AxisChange(Me.CreateGraphics)
        End With

        P5.BringToFront()

    End Sub

    Private Sub Button17_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button17.Click
        P4.BringToFront()
    End Sub

    Private Sub Button18_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button18.Click

        'select property methods

        If Me.CheckBoxMW.Checked Then Me.ComboBoxMW.Enabled = False Else Me.ComboBoxMW.Enabled = True
        If Me.CheckBoxSG.Checked Then Me.ComboBoxSG.Enabled = False Else Me.ComboBoxSG.Enabled = True
        If Me.CheckBoxVISC.Checked Then Me.ComboBoxViscM.Enabled = False Else Me.ComboBoxViscM.Enabled = True

        P6.BringToFront()

    End Sub

    Private Sub Button20_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button20.Click
        P5.BringToFront()
    End Sub

    Private Sub Button21_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button21.Click

        'calculate all properties and display in the datagrid

        Dim prop As New DWSIM.SimulationObjects.PropertyPackages.Auxiliary.PROPS
        Dim prop2 As New DWSIM.Utilities.PetroleumCharacterization.Methods.GL
        Dim rnd As New Random
        Dim id = rnd.Next(1000, 9999)

        ccol.Clear()

        Dim i As Integer = 0

        For Each tc As tmpcomp In tccol

            Dim cprop As New ConstantProperties()

            With cprop

                .NBP = tc.tbpm
                .OriginalDB = "DWSIM"
                .Name = "PSE_" & id & "_" & i + 1

                'SG
                If Me.ComboBoxSG.Enabled Then
                    Select Case Me.ComboBoxSG.SelectedItem.ToString
                        Case "Riazi-Al-Sahhaf (1996)"
                            If .PF_MM.GetValueOrDefault = 0 Then
                                .PF_MM = (1 / 0.01964 * (6.97996 - Math.Log(1080 - .NBP.GetValueOrDefault))) ^ (3 / 2)
                            End If
                            .PF_SG = PropertyMethods.d15_Riazi(.PF_MM)
                        Case "Viscosity Data"
                            If Not .PF_v1.GetValueOrDefault = 0 And Not .PF_v2.GetValueOrDefault = 0 Then .PF_SG = PropertyMethods.d15_v37v98(.PF_v1, .PF_v2)
                    End Select
                Else
                    Dim w As Double() = Nothing
                    Interpolation.ratinterpolation.buildfloaterhormannrationalinterpolant(Me.pxd.ToArray(GetType(Double)), pxd.Count, 0.5, w)
                    .PF_SG = Interpolation.polinterpolation.barycentricinterpolation(pxd.ToArray(GetType(Double)), pyd.ToArray(GetType(Double)), w, pxd.Count, tc.fvm)
                    If Me.RadioButtonD20.Checked Then
                        .PF_SG = PropertyMethods.d15d20(.PF_SG)
                    End If
                End If

                'MW
                If Me.ComboBoxMW.Enabled Then
                    Select Case Me.ComboBoxMW.SelectedItem.ToString
                        Case "Winn (1956)"
                            .PF_MM = PropertyMethods.MW_Winn(.NBP.GetValueOrDefault, .PF_SG.GetValueOrDefault)
                        Case "Riazi (1986)"
                            .PF_MM = PropertyMethods.MW_Riazi(.NBP.GetValueOrDefault, .PF_SG.GetValueOrDefault)
                        Case "Lee-Kesler (1974)"
                            .PF_MM = PropertyMethods.MW_LeeKesler(.NBP.GetValueOrDefault, .PF_SG.GetValueOrDefault)
                        Case "Farah (2006)"
                            .PF_MM = PropertyMethods.MW_Farah(.PF_vA.GetValueOrDefault, .PF_vB.GetValueOrDefault, .PF_SG.GetValueOrDefault, .NBP.GetValueOrDefault)
                    End Select
                Else
                    Dim w As Double() = Nothing
                    Interpolation.ratinterpolation.buildfloaterhormannrationalinterpolant(Me.pxm.ToArray(GetType(Double)), pxm.Count, 0.5, w)
                    .PF_MM = Interpolation.polinterpolation.barycentricinterpolation(Me.pxm.ToArray(GetType(Double)), pym.ToArray(GetType(Double)), w, pxm.Count, tc.fvm)
                End If

                .Molar_Weight = .PF_MM

            End With

            i += 1

            Dim subst As New Substancia(cprop.Name, "")

            With subst
                .ConstantProperties = cprop
                .Nome = cprop.Name
                .FracaoDePetroleo = True
            End With

            ccol.Add(cprop.Name, subst)

        Next

        CalculateMolarFractions()

        If Me.TextBoxBulkMW.Text <> "" Then
            Dim mixtMW As Double = 0
            For Each c As Substancia In ccol.Values
                mixtMW += c.FracaoMolar * c.ConstantProperties.Molar_Weight
            Next
            Dim facm As Double = CDbl(Me.TextBoxBulkMW.Text) / mixtMW
            For Each c As Substancia In ccol.Values
                c.ConstantProperties.Molar_Weight *= facm
            Next
        End If

        If Me.TextBoxBulkD.Text <> "" Then
            Dim mixtD As Double = 0
            For Each c As Substancia In ccol.Values
                mixtD += c.FracaoMassica * c.ConstantProperties.PF_SG.GetValueOrDefault
            Next
            Dim facd As Double = 141.5 / (131.5 + CDbl(Me.TextBoxBulkD.Text)) / mixtD
            For Each c As Substancia In ccol.Values
                c.ConstantProperties.PF_SG *= facd
            Next
        End If

        i = 0
        For Each subst As Substancia In ccol.Values

            Dim cprop As ConstantProperties = subst.ConstantProperties

            With cprop

                Dim tc As tmpcomp = tccol(i)

                .NBP = tc.tbpm
                .OriginalDB = "DWSIM"

                'VISC
                If Me.ComboBoxViscM.Enabled Then
                    .PF_Tv1 = 311
                    .PF_Tv2 = 372
                    Select Case Me.ComboBoxViscM.SelectedItem.ToString
                        Case "Abbott (1971)"
                            .PF_v1 = PropertyMethods.Visc37_Abbott(.NBP, .PF_SG)
                            .PF_v2 = PropertyMethods.Visc98_Abbott(.NBP, .PF_SG)
                        Case "Beg-Amin (1979)"
                            .PF_v1 = PropertyMethods.ViscT_Beg_Amin(.PF_Tv1, (.NBP.GetValueOrDefault / 0.9013) ^ (1 / 1.0176), .PF_SG)
                            .PF_v2 = PropertyMethods.ViscT_Beg_Amin(.PF_Tv2, (.NBP.GetValueOrDefault / 0.9013) ^ (1 / 1.0176), .PF_SG)
                    End Select
                Else
                    Dim w As Double() = Nothing
                    Interpolation.ratinterpolation.buildfloaterhormannrationalinterpolant(pxv1.ToArray(GetType(Double)), pxv1.Count, 0.5, w)
                    .PF_v1 = Interpolation.polinterpolation.barycentricinterpolation(pxv1.ToArray(GetType(Double)), pyv1.ToArray(GetType(Double)), w, pxv1.Count, tc.fvm)
                    Interpolation.ratinterpolation.buildfloaterhormannrationalinterpolant(pxv2.ToArray(GetType(Double)), pxv2.Count, 0.5, w)
                    .PF_v2 = Interpolation.polinterpolation.barycentricinterpolation(pxv2.ToArray(GetType(Double)), pyv2.ToArray(GetType(Double)), w, pxv2.Count, tc.fvm)
                    .PF_Tv1 = (CDbl(Me.TextBoxVT1.Text) - 32) / 9 * 5 + 273.15
                    .PF_Tv2 = (CDbl(Me.TextBoxVT2.Text) - 32) / 9 * 5 + 273.15
                End If

                .PF_vA = PropertyMethods.ViscWaltherASTM_A(.PF_Tv1, .PF_v1, .PF_Tv2, .PF_v2)
                .PF_vB = PropertyMethods.ViscWaltherASTM_B(.PF_Tv1, .PF_v1, .PF_Tv2, .PF_v2)

                'Tc
                Select Case Me.ComboBoxTC.SelectedItem.ToString
                    Case "Riazi-Daubert (1985)"
                        .Critical_Temperature = PropertyMethods.Tc_RiaziDaubert(.NBP, .PF_SG)
                    Case "Riazi (2005)"
                        .Critical_Temperature = PropertyMethods.Tc_Riazi(.NBP, .PF_SG)
                    Case "Lee-Kesler (1976)"
                        .Critical_Temperature = PropertyMethods.Tc_LeeKesler(.NBP, .PF_SG)
                    Case "Farah (2006)"
                        .Critical_Temperature = PropertyMethods.Tc_Farah(.PF_vA, .PF_vB, .NBP, .PF_SG)
                End Select

                'Pc
                Select Case Me.ComboBoxPC.SelectedItem.ToString
                    Case "Riazi-Daubert (1985)"
                        .Critical_Pressure = PropertyMethods.Pc_RiaziDaubert(.NBP, .PF_SG)
                    Case "Lee-Kesler (1976)"
                        .Critical_Pressure = PropertyMethods.Pc_LeeKesler(.NBP, .PF_SG)
                    Case "Farah (2006)"
                        .Critical_Pressure = PropertyMethods.Pc_Farah(.PF_vA, .PF_vB, .NBP, .PF_SG)
                End Select

                'Af
                Select Case Me.ComboBoxAF.SelectedItem.ToString
                    Case "Lee-Kesler (1976)"
                        .Acentric_Factor = PropertyMethods.AcentricFactor_LeeKesler(.Critical_Temperature, .Critical_Pressure, .NBP)
                    Case "Korsten (2000)"
                        .Acentric_Factor = PropertyMethods.AcentricFactor_Korsten(.Critical_Temperature, .Critical_Pressure, .NBP)
                End Select

                .Normal_Boiling_Point = .NBP

                .IsPF = 1
                .PF_Watson_K = (1.8 * .NBP.GetValueOrDefault) ^ (1 / 3) / .PF_SG.GetValueOrDefault

                Dim tmp = prop2.calculate_Hf_Sf(.PF_SG, .Molar_Weight, .NBP)

                .IG_Enthalpy_of_Formation_25C = tmp(0)
                .IG_Entropy_of_Formation_25C = tmp(1)
                .IG_Gibbs_Energy_of_Formation_25C = tmp(0) - 298.15 * tmp(1)
                .Element_C = tmp(2)
                .Element_H = tmp(3)

                Dim methods2 As New DWSIM.SimulationObjects.PropertyPackages.Auxiliary.PROPS
                Dim methods As New DWSIM.Utilities.Hypos.Methods.HYP

                .HVap_A = methods.DHvb_Vetere(.Critical_Temperature, .Critical_Pressure, .Normal_Boiling_Point) / .Molar_Weight

                .Critical_Compressibility = prop.Zc1(.Acentric_Factor)
                .Critical_Volume = 8314 * .Critical_Compressibility * .Critical_Temperature / .Critical_Pressure
                .Z_Rackett = prop.Zc1(.Acentric_Factor)
                If .Z_Rackett < 0 Then
                    .Z_Rackett = 0.2
                End If

                .Chao_Seader_Acentricity = .Acentric_Factor
                .Chao_Seader_Solubility_Parameter = ((.HVap_A * .Molar_Weight - 8.314 * .Normal_Boiling_Point) * 238.846 * methods2.liq_dens_rackett(.Normal_Boiling_Point, .Critical_Temperature, .Critical_Pressure, .Acentric_Factor, .Molar_Weight) / .Molar_Weight / 1000000.0) ^ 0.5
                .Chao_Seader_Liquid_Molar_Volume = 1 / methods2.liq_dens_rackett(.Normal_Boiling_Point, .Critical_Temperature, .Critical_Pressure, .Acentric_Factor, .Molar_Weight) * .Molar_Weight / 1000 * 1000000.0


                methods2 = Nothing
                methods = Nothing

                .ID = -id - i + 1

            End With

            i += 1

        Next

        'Adjust Acentric Factors and Rackett parameters to fit NBP and Density

        Dim dfit As New Methods.DensityFitting
        Dim prvsfit As New Methods.PRVSFitting
        Dim srkvsfit As New Methods.SRKVSFitting
        Dim nbpfit As New Methods.NBPFitting
        Dim tms As New DWSIM.SimulationObjects.Streams.MaterialStream("", "")
        Dim pp As DWSIM.SimulationObjects.PropertyPackages.PropertyPackage
        Dim fzra, fw, fprvs, fsrkvs As Double

        If form.Options.PropertyPackages.Count > 0 Then
            pp = form.Options.SelectedPropertyPackage
        Else
            pp = New DWSIM.SimulationObjects.PropertyPackages.PengRobinsonPropertyPackage()
        End If

        For Each c As Substancia In ccol.Values
            tms.Fases(0).Componentes.Add(c.Nome, c)
        Next

        Dim recalcVc As Boolean = False

        i = 0
        For Each c As Substancia In ccol.Values
            If Me.CheckBoxADJAF.Checked Then
                With nbpfit
                    ._pp = pp
                    ._ms = tms
                    ._idx = i
                    Dim fwait As New FormLS
                    With fwait
                        .Text = "DWSIM"
                        .Label1.Text = DWSIM.App.GetLocalString("CalculandoFatordeAjuste")
                        .TopMost = True
                        .StartPosition = FormStartPosition.CenterScreen
                    End With
                    fwait.Show()
                    Try
                        fw = .MinimizeError()
                    Catch ex As Exception
                        MessageBox.Show(ex.Message.ToString, "Error fitting Acentric Factors", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    End Try
                    fwait.Close()
                End With
                With c.ConstantProperties
                    c.ConstantProperties.Acentric_Factor *= fw
                    c.ConstantProperties.Z_Rackett = prop.Zc1(c.ConstantProperties.Acentric_Factor)
                    If .Z_Rackett < 0 Then
                        .Z_Rackett = 0.2
                        recalcVc = True
                    End If
                    .Critical_Compressibility = prop.Zc1(.Acentric_Factor)
                    .Critical_Volume = prop.Vc(.Critical_Temperature, .Critical_Pressure, .Acentric_Factor, .Critical_Compressibility)
                End With
            End If
            If Me.CheckBoxADJZRA.Checked Then
                With dfit
                    ._comp = c
                    Dim fwait As New FormLS
                    With fwait
                        .Text = "DWSIM"
                        .Label1.Text = DWSIM.App.GetLocalString("CalculandoFatordeAjuste")
                        .TopMost = True
                        .StartPosition = FormStartPosition.CenterScreen
                    End With
                    fwait.Show()
                    Try
                        fzra = .MinimizeError()
                    Catch ex As Exception
                        MessageBox.Show(ex.Message.ToString, "Error fitting Rackett Parameters", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    End Try
                    fwait.Close()
                End With
                With c.ConstantProperties
                    .Z_Rackett *= fzra
                    If .Critical_Compressibility < 0 Or recalcVc Then
                        .Critical_Compressibility = .Z_Rackett
                        .Critical_Volume = prop.Vc(.Critical_Temperature, .Critical_Pressure, .Acentric_Factor, .Critical_Compressibility)
                    End If
                End With
            End If
            c.ConstantProperties.PR_Volume_Translation_Coefficient = 1
            prvsfit._comp = c
            fprvs = prvsfit.MinimizeError()
            With c.ConstantProperties
                If Math.Abs(fprvs) < 99.0# Then .PR_Volume_Translation_Coefficient *= fprvs Else .PR_Volume_Translation_Coefficient = 0.0#
            End With
            c.ConstantProperties.SRK_Volume_Translation_Coefficient = 1
            srkvsfit._comp = c
            fsrkvs = srkvsfit.MinimizeError()
            With c.ConstantProperties
                If Math.Abs(fsrkvs) < 99.0# Then .SRK_Volume_Translation_Coefficient *= fsrkvs Else .SRK_Volume_Translation_Coefficient = 0.0#
            End With
            recalcVc = False
            i += 1
        Next

        pp = Nothing
        dfit = Nothing
        nbpfit = Nothing
        tms = Nothing

        Me.TextBoxStreamName.Text = "OIL_" & rnd.Next(1000, 9999)

        Dim nm, fm, nbp, sg, mm, ct, cp, af, visc1, visc2, prvs, srkvs As String

        Me.DataGridView2.Rows.Clear()
        For Each subst As Substancia In ccol.Values
            With subst
                nm = .Nome
                fm = Format(.FracaoMolar, nf)
                nbp = Format(cv.ConverterDoSI(su.spmp_temperature, .ConstantProperties.NBP), nf)
                sg = Format(.ConstantProperties.PF_SG, nf)
                mm = Format(.ConstantProperties.PF_MM, nf)
                ct = Format(cv.ConverterDoSI(su.spmp_temperature, .ConstantProperties.Critical_Temperature), nf)
                cp = Format(cv.ConverterDoSI(su.spmp_pressure, .ConstantProperties.Critical_Pressure), nf)
                af = Format(.ConstantProperties.Acentric_Factor, nf)
                visc1 = Format(cv.ConverterDoSI(su.spmp_cinematic_viscosity, .ConstantProperties.PF_v1), "E")
                visc2 = Format(cv.ConverterDoSI(su.spmp_cinematic_viscosity, .ConstantProperties.PF_v2), "E")
                prvs = Format(.ConstantProperties.PR_Volume_Translation_Coefficient, "N6")
                srkvs = Format(.ConstantProperties.SRK_Volume_Translation_Coefficient, "N6")
            End With
            Me.DataGridView2.Rows.Add(New Object() {nm, fm, nbp, sg, mm, ct, cp, af, visc1, visc2, prvs, srkvs})
        Next

        P7.BringToFront()

    End Sub

    Private Sub Button23_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button23.Click
        P6.BringToFront()
    End Sub

    Private Sub Button24_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button24.Click

        'finalize button

        Dim corr As String = Me.TextBoxStreamName.Text
        Dim tmpcomp As New DWSIM.ClassesBasicasTermodinamica.ConstantProperties
        Dim subst As DWSIM.ClassesBasicasTermodinamica.Substancia
        Dim gObj As Microsoft.MSDN.Samples.GraphicObjects.GraphicObject = Nothing
        Dim idx As Integer = 0

        For Each subst In ccol.Values
            tmpcomp = subst.ConstantProperties
            form.Options.NotSelectedComponents.Add(tmpcomp.Name, tmpcomp)
            idx = form.FrmStSim1.AddCompToGrid(tmpcomp)
            form.FrmStSim1.AddCompToSimulation(idx)
        Next

        Dim myMStr As New Microsoft.MSDN.Samples.GraphicObjects.MaterialStreamGraphic(form.FormSurface.FlowsheetDesignSurface.HorizontalScroll.Value + 100, form.FormSurface.FlowsheetDesignSurface.VerticalScroll.Value + 100, 20, 20, 0)
        myMStr.LineWidth = 2
        myMStr.Fill = True
        myMStr.FillColor = Color.WhiteSmoke
        myMStr.LineColor = Color.Red
        myMStr.Tag = corr
        gObj = myMStr
        gObj.Name = "MAT-" & Guid.NewGuid.ToString
        form.Collections.MaterialStreamCollection.Add(gObj.Name, myMStr)
        If Not DWSIM.App.IsRunningOnMono Then form.FormObjList.TreeViewObj.Nodes("NodeMS").Nodes.Add(gObj.Name, gObj.Tag).Name = gObj.Name
        'OBJETO DWSIM
        Dim myCOMS As DWSIM.SimulationObjects.Streams.MaterialStream = New DWSIM.SimulationObjects.Streams.MaterialStream(myMStr.Name, DWSIM.App.GetLocalString("CorrentedeMatria"))
        myCOMS.GraphicObject = myMStr
        form.AddComponentsRows(myCOMS)
        Dim wtotal As Double = 0
        For Each subst In ccol.Values
            wtotal += subst.FracaoMolar.GetValueOrDefault * subst.ConstantProperties.Molar_Weight
        Next
        For Each subst In ccol.Values
            subst.FracaoMassica = subst.FracaoMolar.GetValueOrDefault * subst.ConstantProperties.Molar_Weight / wtotal
        Next
        For Each subst In ccol.Values
            With myCOMS.Fases(0).Componentes
                .Item(subst.Nome).ConstantProperties = subst.ConstantProperties
                .Item(subst.Nome).FracaoMassica = subst.FracaoMassica
                .Item(subst.Nome).FracaoMolar = subst.FracaoMolar
            End With
            myCOMS.Fases(1).Componentes.Item(subst.Nome).ConstantProperties = subst.ConstantProperties
            myCOMS.Fases(2).Componentes.Item(subst.Nome).ConstantProperties = subst.ConstantProperties
        Next
        form.Collections.ObjectCollection.Add(myCOMS.Nome, myCOMS)
        form.Collections.CLCS_MaterialStreamCollection.Add(myCOMS.Nome, myCOMS)
        form.FormSurface.FlowsheetDesignSurface.drawingObjects.Add(gObj)
        form.FormSurface.FlowsheetDesignSurface.Invalidate()

        Me.Close()

    End Sub

    Public Sub PasteData(ByRef dgv As DataGridView)
        Dim tArr() As String
        Dim arT() As String
        Dim i, ii As Integer
        Dim c, cc, r As Integer

        tArr = Clipboard.GetText().Split(Environment.NewLine)

        If dgv.SelectedCells.Count > 0 Then
            r = dgv.SelectedCells(0).RowIndex
            c = dgv.SelectedCells(0).ColumnIndex
        Else
            r = 0
            c = 0
        End If
        For i = 0 To tArr.Length - 1
            If tArr(i) <> "" Then
                arT = tArr(i).Split(vbTab)
                For ii = 0 To arT.Length - 1
                    If r > dgv.Rows.Count - 1 Then
                        dgv.Rows.Add()
                        dgv.Rows(0).Cells(0).Selected = True
                    End If
                Next
                r = r + 1
            End If
        Next
        If dgv.SelectedCells.Count > 0 Then
            r = dgv.SelectedCells(0).RowIndex
            c = dgv.SelectedCells(0).ColumnIndex
        Else
            r = 0
            c = 0
        End If
        For i = 0 To tArr.Length - 1
            If tArr(i) <> "" Then
                arT = tArr(i).Split(vbTab)
                cc = c
                For ii = 0 To arT.Length - 1
                    cc = GetNextVisibleCol(dgv, cc)
                    If cc > dgv.ColumnCount - 1 Then Exit For
                    dgv.Item(cc, r).Value = arT(ii).TrimStart
                    cc = cc + 1
                Next
                r = r + 1
            End If
        Next

    End Sub

    Private Function GetNextVisibleCol(ByRef dgv As DataGridView, ByVal stidx As Integer) As Integer

        Dim i As Integer

        For i = stidx To dgv.ColumnCount - 1
            If dgv.Columns(i).Visible Then Return i
        Next

        Return Nothing

    End Function

    Private Sub DataGridView1_KeyDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles DataGridView1.KeyDown
        If e.KeyCode = Keys.Delete And Not Me.DataGridView1.IsCurrentCellInEditMode Then
            Try
                Me.DataGridView1.Rows.RemoveAt(Me.DataGridView1.SelectedCells(0).RowIndex)
            Catch ex As Exception

            End Try
        End If
    End Sub

    Private Function GetFV(ByVal coeffs() As Double, ByVal fv0 As Double, ByVal t As Double) As Double

        'TBP(K) = aa + bb*fv + cc*fv^2 + dd*fv^3 + ee*fv^4 + ff*fv^5 + gg*fv^6 (fv 0 ~ 1)

        Dim f, f0, df As Double
        Dim cnt As Integer = 0
        Dim fv As Double = fv0
        Do
            f0 = (coeffs(0) + coeffs(1) * fv + coeffs(2) * fv ^ 2 + coeffs(3) * fv ^ 3 + coeffs(4) * fv ^ 4 + coeffs(5) * fv ^ 5 + coeffs(6) * fv ^ 6)
            f = -t + (coeffs(0) + coeffs(1) * fv + coeffs(2) * fv ^ 2 + coeffs(3) * fv ^ 3 + coeffs(4) * fv ^ 4 + coeffs(5) * fv ^ 5 + coeffs(6) * fv ^ 6)
            df = coeffs(1) + 2 * coeffs(2) * fv + 3 * coeffs(3) * fv ^ 2 + 4 * coeffs(4) * fv ^ 3 + 5 * coeffs(5) * fv ^ 4 + 6 * coeffs(6) * fv ^ 5
            fv = -f / df * 0.3 + fv
            If fv < 0 Then fv = Abs(fv)
            cnt += 1
        Loop Until Abs(f) < 0.000000001 Or cnt >= 1000

        Return fv

    End Function

    Private Function GetT(ByVal coeffs() As Double, ByVal fv As Double) As Double

        'TBP(K) = aa + bb*fv + cc*fv^2 + dd*fv^3 + ee*fv^4 + ff*fv^5 + gg*fv^6 (fv 0 ~ 1)

        Return (coeffs(0) + coeffs(1) * fv + coeffs(2) * fv ^ 2 + coeffs(3) * fv ^ 3 + coeffs(4) * fv ^ 4 + coeffs(5) * fv ^ 5 + coeffs(6) * fv ^ 6)

    End Function

    Private Sub CheckBoxVISC_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBoxVISC.CheckedChanged
        If Me.CheckBoxVISC.Checked Then
            Me.TextBoxVT1.Enabled = True
            Me.TextBoxVT2.Enabled = True
        Else
            Me.TextBoxVT1.Enabled = False
            Me.TextBoxVT2.Enabled = False
        End If
    End Sub

    Private Sub CheckBoxSG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBoxSG.CheckedChanged
        If Me.CheckBoxSG.Checked Then
            Me.RadioButtonD20.Enabled = True
            Me.RadioButtonD60.Enabled = True
        Else
            Me.RadioButtonD20.Enabled = False
            Me.RadioButtonD60.Enabled = False
        End If
    End Sub

    Private Sub ComboBoxDistMethod_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBoxDistMethod.SelectedIndexChanged
        If Me.ComboBoxDistMethod.SelectedIndex = 2 Then
            Me.TextBoxKAPI.Enabled = True
        Else
            Me.TextBoxKAPI.Enabled = False
        End If
    End Sub

    Private Sub Button25_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button25.Click
        Me.PasteData(Me.DataGridView1)
    End Sub

    Sub CalculateMolarFractions()

        Dim sum1, fm, fv, fw As Double
        Dim i As Integer

        Select Case Me.ComboBoxBasis.SelectedIndex
            Case 0
                'liquid volume percent
                i = 0
                sum1 = 0
                For Each subst As Substancia In Me.ccol.Values
                    fv = (tccol(i).fvf - tccol(i).fv0) / (tccol(tccol.Count - 1).fvf - tccol(0).fv0)
                    fw = fv * subst.ConstantProperties.PF_SG.GetValueOrDefault
                    fm = fw / subst.ConstantProperties.Molar_Weight
                    sum1 += fm
                    i = i + 1
                Next
                i = 0
                For Each subst As Substancia In Me.ccol.Values
                    fv = (tccol(i).fvf - tccol(i).fv0) / (tccol(tccol.Count - 1).fvf - tccol(0).fv0)
                    fw = fv * subst.ConstantProperties.PF_SG.GetValueOrDefault
                    fm = fw / subst.ConstantProperties.Molar_Weight
                    subst.FracaoMolar = fm / sum1
                    i = i + 1
                Next
            Case 1
                'mole percent
                i = 0
                For Each subst As Substancia In Me.ccol.Values
                    subst.FracaoMolar = (tccol(i).fvf - tccol(i).fv0) / (tccol(tccol.Count - 1).fvf - tccol(0).fv0)
                    i = i + 1
                Next
            Case 2
                'weight percent
                i = 0
                sum1 = 0
                For Each subst As Substancia In Me.ccol.Values
                    fw = (tccol(i).fvf - tccol(i).fv0) / (tccol(tccol.Count - 1).fvf - tccol(0).fv0)
                    fm = fw / subst.ConstantProperties.Molar_Weight
                    sum1 += fm
                    i = i + 1
                Next
                i = 0
                For Each subst As Substancia In Me.ccol.Values
                    fw = (tccol(i).fvf - tccol(i).fv0) / (tccol(tccol.Count - 1).fvf - tccol(0).fv0)
                    fm = fw / subst.ConstantProperties.Molar_Weight
                    subst.FracaoMolar = fm / sum1
                    i = i + 1
                Next

        End Select

        Dim wxtotal As Double = 0

        For Each subst As Substancia In ccol.Values
            wxtotal += subst.FracaoMolar * subst.ConstantProperties.Molar_Weight
        Next

        For Each subst As Substancia In ccol.Values
            subst.FracaoMassica = subst.FracaoMolar * subst.ConstantProperties.Molar_Weight / wxtotal
        Next

    End Sub

    Private Sub Button28_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        cv = New DWSIM.SistemasDeUnidades.Conversor

        'analyze data, interpolate, bla, bla bla...

        pxt = New ArrayList
        pyt = New ArrayList
        pxv1 = New ArrayList
        pyv1 = New ArrayList
        pxv2 = New ArrayList
        pyv2 = New ArrayList
        pxm = New ArrayList
        pym = New ArrayList
        pxd = New ArrayList
        pyd = New ArrayList

        gxt = New ArrayList
        gyt = New ArrayList
        gxv1 = New ArrayList
        gyv1 = New ArrayList
        gxv2 = New ArrayList
        gyv2 = New ArrayList
        gxm = New ArrayList
        gym = New ArrayList
        gxd = New ArrayList
        gyd = New ArrayList


        For Each r As DataGridViewRow In Me.DataGridView1.Rows
            If r.Cells("temp").Value <> Nothing Then
                pxt.Add(r.Cells(0).Value / 100)
                pyt.Add(cv.ConverterParaSI(su.spmp_temperature, r.Cells("temp").Value))
                gxt.Add(CDbl(r.Cells(0).Value))
                gyt.Add(CDbl(r.Cells("temp").Value))
            End If
            If r.Cells("mm").Value <> Nothing Then
                pxm.Add(r.Cells(0).Value / 100)
                pym.Add(CDbl(r.Cells("mm").Value))
                gxm.Add(CDbl(r.Cells(0).Value))
                gym.Add(CDbl(r.Cells("mm").Value))
            End If
            If r.Cells("dens").Value <> Nothing Then
                pxd.Add(r.Cells(0).Value / 100)
                pyd.Add(CDbl(r.Cells("dens").Value))
                gxd.Add(CDbl(r.Cells(0).Value))
                gyd.Add(CDbl(r.Cells("dens").Value))
            End If
            If r.Cells("visc1").Value <> Nothing Then
                pxv1.Add(r.Cells(0).Value / 100)
                pyv1.Add(cv.ConverterParaSI(su.spmp_cinematic_viscosity, r.Cells("visc1").Value))
                gxv1.Add(CDbl(r.Cells(0).Value))
                gyv1.Add(CDbl(r.Cells("visc1").Value))
            End If
            If r.Cells("visc2").Value <> Nothing Then
                pxv2.Add(r.Cells(0).Value / 100)
                pyv2.Add(cv.ConverterParaSI(su.spmp_cinematic_viscosity, r.Cells("visc2").Value))
                gxv2.Add(CDbl(r.Cells(0).Value))
                gyv2.Add(CDbl(r.Cells("visc2").Value))
            End If
        Next

        With Me.GraphCurves.GraphPane
            .CurveList.Clear()
            .YAxisList.Clear()
            If pyt.Count > 0 Then
                Dim ya0 As New ZedGraph.YAxis("T (" & su.spmp_temperature & ")")
                .YAxisList.Add(ya0)
                With .AddCurve(Me.ComboBoxDistMethod.Text, gxt.ToArray(GetType(Double)), gyt.ToArray(GetType(Double)), Color.Black)
                    .Line.IsVisible = True
                    .Line.IsSmooth = True
                    .Color = Color.Red
                    .Symbol.Type = ZedGraph.SymbolType.Circle
                    .Symbol.Fill.Color = Color.Red
                    .Symbol.Fill.IsVisible = True
                    .YAxisIndex = Me.GraphCurves.GraphPane.YAxisList.IndexOf("T (" & su.spmp_temperature & ")")
                End With
            End If
            If pyd.Count > 0 Then
                Dim ya1 As New ZedGraph.YAxis("SG")
                .YAxisList.Add(ya1)
                With .AddCurve(DWSIM.App.GetLocalString("PF_SG"), gxd.ToArray(GetType(Double)), gyd.ToArray(GetType(Double)), Color.Black)
                    .Line.IsVisible = True
                    .Line.IsSmooth = True
                    .Color = Color.Blue
                    .Symbol.Type = ZedGraph.SymbolType.Circle
                    .Symbol.Fill.Color = Color.Blue
                    .Symbol.Fill.IsVisible = True
                    .YAxisIndex = Me.GraphCurves.GraphPane.YAxisList.IndexOf("SG")
                End With
            End If
            If pym.Count > 0 Then
                Dim ya2 As New ZedGraph.YAxis("MW (" & su.spmp_molecularWeight & ")")
                .YAxisList.Add(ya2)
                With .AddCurve(DWSIM.App.GetLocalString("PF_MW"), gxm.ToArray(GetType(Double)), gym.ToArray(GetType(Double)), Color.Black)
                    .Line.IsVisible = True
                    .Line.IsSmooth = True
                    .Color = Color.Gold
                    .Symbol.Type = ZedGraph.SymbolType.Circle
                    .Symbol.Fill.Color = Color.Gold
                    .Symbol.Fill.IsVisible = True
                    .YAxisIndex = Me.GraphCurves.GraphPane.YAxisList.IndexOf("MW (" & su.spmp_molecularWeight & ")")
                End With
            End If
            If pyv1.Count > 0 And pyv2.Count > 0 Then
                Dim ya3 As New ZedGraph.YAxis("Visc (" & su.spmp_cinematic_viscosity & ")")
                .YAxisList.Add(ya3)
                With .AddCurve(DWSIM.App.GetLocalString("PF_Visc1"), gxv1.ToArray(GetType(Double)), gyv1.ToArray(GetType(Double)), Color.Black)
                    .Line.IsVisible = True
                    .Line.IsSmooth = True
                    .Color = Color.LightGreen
                    .Symbol.Type = ZedGraph.SymbolType.Circle
                    .Symbol.Fill.Color = Color.LightGreen
                    .Symbol.Fill.IsVisible = True
                    .YAxisIndex = Me.GraphCurves.GraphPane.YAxisList.IndexOf("Visc (" & su.spmp_cinematic_viscosity & ")")
                End With
                With .AddCurve(DWSIM.App.GetLocalString("PF_Visc2"), gxv2.ToArray(GetType(Double)), gyv2.ToArray(GetType(Double)), Color.Black)
                    .Line.IsVisible = True
                    .Line.IsSmooth = True
                    .Color = Color.Green
                    .Symbol.Type = ZedGraph.SymbolType.Circle
                    .Symbol.Fill.Color = Color.Green
                    .Symbol.Fill.IsVisible = True
                    .YAxisIndex = Me.GraphCurves.GraphPane.YAxisList.IndexOf("Visc (" & su.spmp_cinematic_viscosity & ")")
                End With
            End If
            With .Legend
                .Border.IsVisible = False
                .Position = ZedGraph.LegendPos.BottomCenter
                .IsHStack = True
            End With
            With .XAxis
                .Title.Text = DWSIM.App.GetLocalString("PF_PVAP")
                .Scale.Min = 0.0#
                .Scale.Max = 100.0#
            End With
            .Margin.Top = 20
            .Title.IsVisible = False
            Me.GraphCurves.IsAntiAlias = True
            .AxisChange(Me.CreateGraphics)
        End With

        P3.BringToFront()

    End Sub

    Private Sub Button27_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        P2.BringToFront()
    End Sub

    Private Sub Button27_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button27.Click

        'load assay

        Dim frmam As New FormAssayManager

        cv = New DWSIM.SistemasDeUnidades.Conversor

        If frmam.ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
            Dim myassay As Assay.Assay = frmam.currentassay
            If Not myassay Is Nothing Then
                With myassay
                    ComboBoxDistMethod.SelectedIndex = .NBPType
                    ComboBoxBasis.SelectedItem = .CurveBasis
                    TextBoxBulkD.Text = Format(.API, nf)
                    If .API = 0.0# Then TextBoxBulkD.Text = ""
                    TextBoxBulkMW.Text = Format(cv.ConverterDoSI(su.spmp_molecularWeight, .MW), nf)
                    If .MW = 0.0# Then TextBoxBulkMW.Text = ""
                    TextBoxVT1.Text = Format(cv.ConverterDoSI(su.spmp_temperature, .T1), nf)
                    TextBoxVT2.Text = Format(cv.ConverterDoSI(su.spmp_temperature, .T2), nf)
                    TextBoxKAPI.Text = Format(.K_API, nf)
                    CheckBoxMW.Checked = .HasMWCurve
                    CheckBoxSG.Checked = .HasSGCurve
                    CheckBoxVISC.Checked = .HasViscCurves
                    If .SGCurveType = "SG20" Then RadioButtonD20.Checked = True Else RadioButtonD60.Checked = True
                    'curves
                    If Me.CheckBoxMW.Checked = False Then Me.DataGridView1.Columns("mm").Visible = False Else Me.DataGridView1.Columns("mm").Visible = True
                    If Me.CheckBoxSG.Checked = False Then Me.DataGridView1.Columns("dens").Visible = False Else Me.DataGridView1.Columns("dens").Visible = True
                    If Me.CheckBoxVISC.Checked = False Then
                        Me.DataGridView1.Columns("visc1").Visible = False
                        Me.DataGridView1.Columns("visc2").Visible = False
                    Else
                        Me.DataGridView1.Columns("visc1").Visible = True
                        Me.DataGridView1.Columns("visc2").Visible = True
                    End If
                    DataGridView1.Rows.Clear()
                    Dim i As Integer = 0
                    Dim idx As Integer
                    For i = 0 To .PX.Count - 1
                        idx = DataGridView1.Rows.Add()
                        DataGridView1.Rows(idx).Cells("vap").Value = Format(.PX(i) * 100, nf)
                        DataGridView1.Rows(idx).Cells("temp").Value = Format(cv.ConverterDoSI(su.spmp_temperature, .PY_NBP(i)), nf)
                        If CheckBoxMW.Checked Then
                            DataGridView1.Rows(idx).Cells("mm").Value = Format(.PY_MW(i), nf)
                        End If
                        If CheckBoxSG.Checked Then
                            DataGridView1.Rows(idx).Cells("dens").Value = Format(.PY_SG(i), nf)
                        End If
                        If CheckBoxVISC.Checked Then
                            DataGridView1.Rows(idx).Cells("visc1").Value = Format(cv.ConverterDoSI(su.spmp_cinematic_viscosity, .PY_V1(i)), nf)
                            DataGridView1.Rows(idx).Cells("visc2").Value = Format(cv.ConverterDoSI(su.spmp_cinematic_viscosity, .PY_V2(i)), nf)
                        End If
                    Next
                End With
            End If
            Try
                frmam.Close()
            Catch ex As Exception

            End Try
        End If


    End Sub

    Private Sub Button26_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button26.Click

        'save assay

        Try
            cv = New DWSIM.SistemasDeUnidades.Conversor
            pxt = New ArrayList
            pyt = New ArrayList
            pyv1 = New ArrayList
            pyv2 = New ArrayList
            pym = New ArrayList
            pyd = New ArrayList
            For Each r As DataGridViewRow In Me.DataGridView1.Rows
                If r.Index < Me.DataGridView1.Rows.Count - 1 Then
                    If r.Cells("temp").Value <> Nothing Then
                        If Double.TryParse(r.Cells("temp").Value, New Double()) Then
                            pxt.Add(r.Cells(0).Value / 100)
                            pyt.Add(cv.ConverterParaSI(su.spmp_temperature, r.Cells("temp").Value))
                        End If
                    End If
                    If r.Cells("mm").Value <> Nothing Then
                        pym.Add(CDbl(r.Cells("mm").Value))
                    End If
                    If r.Cells("dens").Value <> Nothing Then
                        pyd.Add(CDbl(r.Cells("dens").Value))
                    End If
                    If r.Cells("visc1").Value <> Nothing Then
                        If Double.TryParse(r.Cells("visc1").Value, New Double()) Then
                            pyv1.Add(cv.ConverterParaSI(su.spmp_cinematic_viscosity, r.Cells("visc1").Value))
                        End If
                    End If
                    If r.Cells("visc2").Value <> Nothing Then
                        If Double.TryParse(r.Cells("visc2").Value, New Double()) Then
                            pyv2.Add(cv.ConverterParaSI(su.spmp_cinematic_viscosity, r.Cells("visc2").Value))
                        End If
                    End If
                End If
            Next
            Dim k_api, mw, api, vt1, vt2 As Double
            k_api = TextBoxKAPI.Text
            If TextBoxBulkMW.Text <> "" Then mw = TextBoxBulkMW.Text Else mw = 0.0#
            If TextBoxBulkD.Text <> "" Then api = TextBoxBulkD.Text Else api = 0.0#
            vt1 = cv.ConverterParaSI(su.spmp_temperature, TextBoxVT1.Text)
            vt2 = cv.ConverterParaSI(su.spmp_temperature, TextBoxVT2.Text)
            Dim myassay As Assay.Assay = New Assay.Assay(k_api, mw, api, vt1, vt2, ComboBoxDistMethod.SelectedIndex, ComboBoxBasis.SelectedItem.ToString, pxt, pyt, pym, pyd, pyv1, pyv2)
            myassay.CurveBasis = ComboBoxBasis.SelectedItem.ToString
            myassay.Name = "NBP_ASSAY_" & New Random().Next(10000).ToString
            form.Options.PetroleumAssays.Add(Guid.NewGuid().ToString, myassay)
            MessageBox.Show("Assay data was saved succesfully.", "DWSIM", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show("Error saving assay data:" & vbCrLf & ex.ToString, "DWSIM", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

    End Sub

End Class
