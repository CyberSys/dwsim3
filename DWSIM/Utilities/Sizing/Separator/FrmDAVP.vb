﻿Public Class FrmDAVP

    Inherits System.Windows.Forms.Form

    Dim vessel As DWSIM.SimulationObjects.UnitOps.Vessel
    Dim entmat As DWSIM.SimulationObjects.Streams.MaterialStream
    Dim Frm As FormFlowsheet
    Dim rhol, rhov, ql, qv, qe, rhoe, wl, wv As Double
    Dim RLD, C, VGI, SURGE, TR, VMAX, K As Double

    Private Sub Label4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles LblLiq.Click

    End Sub

    Private Sub FrmDAVP_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load


        Me.Frm = My.Application.ActiveSimulation

        Me.ComboBox1.Items.Clear()

        For Each vessel2 In Me.Frm.Collections.CLCS_VesselCollection.Values
            Me.ComboBox1.Items.Add(vessel2.GraphicObject.Tag.ToString)
        Next

        If Me.ComboBox1.Items.Count > 0 Then Me.ComboBox1.SelectedIndex = 0
        Me.ComboBoxTipoVaso.SelectedIndex = 0

        Me.Text = DWSIM.App.GetLocalString("DWSIMUtilitriosDAVP")

    End Sub

    Private Sub VScrollBar1_Scroll(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ScrollEventArgs) Handles VScrollBar1.Scroll

        If e.Type = ScrollEventType.SmallDecrement Then

            TextBox114.Text = Format(CDbl(TextBox114.Text) + 0.1, "#0.0#")

        ElseIf e.Type = ScrollEventType.SmallIncrement Then

            TextBox114.Text = Format(CDbl(TextBox114.Text) - 0.1, "#0.0#")

        End If

    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        If Not Me.ComboBox1.SelectedItem Is Nothing Then

            Dim msv, msl As DWSIM.SimulationObjects.Streams.MaterialStream

            msv = Frm.Collections.CLCS_MaterialStreamCollection(Me.vessel.GraphicObject.OutputConnectors(0).AttachedConnector.AttachedTo.Name)
            msl = Frm.Collections.CLCS_MaterialStreamCollection(Me.vessel.GraphicObject.OutputConnectors(1).AttachedConnector.AttachedTo.Name)

            Dim gobj As Microsoft.MSDN.Samples.GraphicObjects.GraphicObject = FormFlowsheet.SearchSurfaceObjectsByTag(Me.ComboBox1.SelectedItem, Frm.FormSurface.FlowsheetDesignSurface)
            Me.vessel = Frm.Collections.CLCS_VesselCollection(gobj.Name)
            Me.entmat = Frm.Collections.CLCS_MaterialStreamCollection(Me.vessel.GraphicObject.InputConnectors(0).AttachedConnector.AttachedFrom.Name)

            Me.rhol = msl.Fases(0).SPMProperties.density.GetValueOrDefault
            Me.rhov = msv.Fases(0).SPMProperties.density.GetValueOrDefault
            Me.ql = msl.Fases(0).SPMProperties.volumetric_flow.GetValueOrDefault
            Me.qv = msv.Fases(0).SPMProperties.volumetric_flow.GetValueOrDefault
            Me.wl = msl.Fases(0).SPMProperties.massflow.GetValueOrDefault
            Me.wv = msv.Fases(0).SPMProperties.massflow.GetValueOrDefault
            Me.rhoe = Me.entmat.Fases(0).SPMProperties.density.GetValueOrDefault
            Me.qe = Me.entmat.Fases(0).SPMProperties.volumetric_flow.GetValueOrDefault

            Dim su As DWSIM.SistemasDeUnidades.Unidades = Frm.Options.SelectedUnitSystem
            Dim conv As New DWSIM.SistemasDeUnidades.Conversor

            Me.LblLiq.Text = Format(conv.ConverterDoSI(su.spmp_volumetricFlow, ql), Frm.Options.NumberFormat) & " " & su.spmp_volumetricFlow & _
                            ", " & Format(conv.ConverterDoSI(su.spmp_massflow, wl), Frm.Options.NumberFormat) & " " & su.spmp_massflow & _
                            ", " & Format(conv.ConverterDoSI(su.spmp_density, rhol), Frm.Options.NumberFormat) & " " & su.spmp_density

            Me.LblGas.Text = Format(conv.ConverterDoSI(su.spmp_molarflow, msv.Fases(0).SPMProperties.molarflow.GetValueOrDefault), Frm.Options.NumberFormat) & " " & su.spmp_molarflow & _
                            ", " & Format(conv.ConverterDoSI(su.spmp_massflow, wv), Frm.Options.NumberFormat) & " " & su.spmp_massflow & _
                            ", " & Format(conv.ConverterDoSI(su.spmp_density, rhov), Frm.Options.NumberFormat) & " " & su.spmp_density

            Me.RLD = Me.tbRLD.Text
            Me.C = Me.tbC.Text
            Me.VGI = Me.tbVGI.Text
            Me.VMAX = Me.tbVMAX.Text
            Me.TR = Me.tbTR.Text
            Me.K = Me.tbK.Text
            Me.SURGE = Me.TextBox114.Text

            If Me.ComboBoxTipoVaso.SelectedItem = DWSIM.App.GetLocalString("Vertical") Then

                Me.SizeVertical()

            ElseIf Me.ComboBoxTipoVaso.SelectedItem = DWSIM.App.GetLocalString("Horizontal") Then

                Me.SizeHorizontal()

            End If

        Else

            Me.vessel = Nothing

        End If

    End Sub

    Private Sub SizeVertical()

        Try

            Dim qv = Me.qv * Me.SURGE
            Dim ql = Me.ql * Me.SURGE
   
            Dim tres = Me.TR

            Dim rho_ml = Me.rhol
            Dim rho_ns = Me.rhoe

            Dim vk = Me.K * ((rho_ml - Me.rhov) / Me.rhov) ^ 0.5
            Dim vp = Me.VGI / 100 * vk
            Dim At = qv / vp

            Dim dmin = (4 * At / Math.PI) ^ 0.5 * 1000
            Dim lmin = CDbl(tbRLD.Text) * dmin

            'bocal de entrada
            Dim vmaxbe = CDbl(tbC.Text) / (rho_ns) ^ 0.5
            Dim aminbe = (qv + ql) / (vmaxbe)
            Dim dminbe = (4 * aminbe / Math.PI) ^ 0.5 * 39.37

            'bocal de gas
            Dim vmaxbg = Me.C / (Me.rhov) ^ 0.5
            Dim aminbg = (qv) / (vmaxbg)
            Dim dminbg = (4 * aminbg / Math.PI) ^ 0.5 * 39.37

            'bocal de liquido
            Dim vmaxbl = Me.VMAX
            Dim aminbl2 = (ql) / (vmaxbl)
            Dim dminbl = (4 * aminbl2 / Math.PI) ^ 0.5 * 39.37

            TbBE.Text = Format(dminbe, "####0.##")
            TbBSG.Text = Format(dminbg, "####0.##")
            TbBSL.Text = Format(dminbl, "####0.##")
            TbD.Text = Format(dmin, "####0.##")
            TbA.Text = Format(lmin, "####0.##")

        Catch ex As Exception

        End Try

    End Sub

    Private Sub SizeHorizontal()

        Try

            Dim qv = Me.qv * Me.SURGE
            Dim ql = Me.ql * Me.SURGE

            Dim rho_ml = Me.rhol
            Dim rho_ns = Me.rhoe

            Dim x, y, l_d, dv, dl, vl1, vl2, cv As Double

            Dim vk = Me.K * ((rho_ml - Me.rhov) / Me.rhov) ^ 0.5
            Dim vp = Me.VGI / 100 * vk

            'bocal de entrada
            Dim vmaxbe = CDbl(tbC.Text) / (rho_ns) ^ 0.5
            Dim aminbe = (qv + ql) / (vmaxbe)
            Dim dminbe = (4 * aminbe / Math.PI) ^ 0.5 * 39.37

            'bocal de gas
            Dim vmaxbg = Me.C / (Me.rhov) ^ 0.5
            Dim aminbg = (qv) / (vmaxbg)
            Dim dminbg = (4 * aminbg / Math.PI) ^ 0.5 * 39.37

            'bocal de liquido
            Dim vmaxbl = Me.VMAX
            Dim aminbl2 = (ql) / (vmaxbl)
            Dim dminbl = (4 * aminbl2 / Math.PI) ^ 0.5 * 39.37

            'vaso
            Dim tr = Me.TR

            l_d = Me.RLD

            x = 0.01
            Do
                y = (1 / Math.PI) * Math.Acos(1 - 2 * x) - (2 / Math.PI) * (1 - 2 * x) * (x * (1 - x)) ^ 0.5
                dv = (4 / Math.PI * qv / (vp)) ^ 0.5 * ((x / y) / l_d) ^ 0.5
                dl = ((4 / (Math.PI * l_d)) * (ql) * CDbl(tr * 60) / (1 - y)) ^ (1 / 3)
                x += 0.0001
            Loop Until Math.Abs(dv - dl) < 0.0001 Or x >= 0.5
            vl1 = (ql) * tr / (1 / 60)
            vl2 = (1 - y) * Math.PI * dl ^ 3 / 4 * l_d
            If vl2 < vl1 Then
                Do
                    vl2 = (1 - y) * Math.PI * dl ^ 3 / 4 * l_d
                    dl = dl * 1.001
                Loop Until Math.Abs(vl2 - vl1) < 0.001
            End If

            Dim diam As Double
            If dl > dv Then diam = dl
            If dv > dl Then diam = dv

            cv = l_d * diam

            TbBE.Text = Format(dminbe, "####0.##")
            TbBSG.Text = Format(dminbg, "####0.##")
            TbBSL.Text = Format(dminbl, "####0.##")
            TbD.Text = Format(diam * 1000, "####0.##")
            TbA.Text = Format(cv * 1000, "####0.##")

        Catch ex As Exception


        End Try

    End Sub

    Private Sub FrmDAVP_HelpRequested(sender As System.Object, hlpevent As System.Windows.Forms.HelpEventArgs) Handles MyBase.HelpRequested
        DWSIM.App.HelpRequested("NoHelp.htm") 'no topic yet
    End Sub
End Class