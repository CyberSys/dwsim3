﻿'    Copyright 2008 Daniel Wagner O. de Medeiros
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

Imports System
Imports System.ComponentModel


Public Class FrmCritpt

    Inherits System.Windows.Forms.Form

    Dim mat As DWSIM.SimulationObjects.Streams.MaterialStream
    Dim Frm As FormFlowsheet

    Dim cp As DWSIM.Utilities.TCP.Methods
    Dim cps As DWSIM.Utilities.TCP.Methods_SRK

    Public su As New DWSIM.SistemasDeUnidades.Unidades
    Public cv As New DWSIM.SistemasDeUnidades.Conversor
    Public nf As String

    Private Sub FrmCritpt_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Me.Frm = My.Application.ActiveSimulation

        Me.Text = DWSIM.App.GetLocalString("DWSIMUtilitriosPonto")

        If Frm.Options.SelectedPropertyPackage.ComponentName.Contains("Peng-Robinson (PR)") Or _
        Frm.Options.SelectedPropertyPackage.ComponentName.Contains("SRK") Then

            Me.su = Frm.Options.SelectedUnitSystem
            Me.nf = Frm.Options.NumberFormat

            Me.ComboBox3.Items.Clear()
            For Each mat In Me.Frm.Collections.CLCS_MaterialStreamCollection.Values
                Me.ComboBox3.Items.Add(mat.GraphicObject.Tag.ToString)
            Next

            If Me.ComboBox3.Items.Count > 0 Then Me.ComboBox3.SelectedIndex = 0

            With Me.Grid1.Columns
                .Item(2).HeaderText = "Tc (" & su.spmp_temperature & ")"
                .Item(3).HeaderText = "Pc (" & su.spmp_pressure & ")"
                .Item(4).HeaderText = "Vc (" & su.molar_volume & ")"
            End With

        Else

            MessageBox.Show(DWSIM.App.GetLocalString("CritptPRSRKOnly"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Me.Close()

        End If

    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        Grid1.Rows.Clear()

        If Not Me.ComboBox3.SelectedItem Is Nothing Then

            Dim gobj As Microsoft.MSDN.Samples.GraphicObjects.GraphicObject = FormFlowsheet.SearchSurfaceObjectsByTag(Me.ComboBox3.SelectedItem, Frm.FormSurface.FlowsheetDesignSurface)
            Me.mat = Frm.Collections.CLCS_MaterialStreamCollection(gobj.Name)
            Dim pr As DWSIM.SimulationObjects.PropertyPackages.PropertyPackage

            pr = Frm.Options.SelectedPropertyPackage
            pr.CurrentMaterialStream = mat

            Dim n As Integer = mat.Fases(0).Componentes.Count - 1

            Dim Vz(n) As Double
            Dim comp As DWSIM.ClassesBasicasTermodinamica.Substancia
            Dim i As Integer = 0
            For Each comp In mat.Fases(0).Componentes.Values
                Vz(i) += comp.FracaoMolar.GetValueOrDefault
                i += 1
            Next

            Dim j, k, l As Integer
            i = 0
            Do
                If Vz(i) = 0 Then j += 1
                i = i + 1
            Loop Until i = n + 1

            Dim VTc(n), Vpc(n), Vw(n), VVc(n), VKij(n, n) As Double
            Dim Vm2(UBound(Vz) - j), VPc2(UBound(Vz) - j), VTc2(UBound(Vz) - j), VVc2(UBound(Vz) - j), Vw2(UBound(Vz) - j), VKij2(UBound(Vz) - j, UBound(Vz) - j)

            VTc = pr.RET_VTC()
            Vpc = pr.RET_VPC()
            VVc = pr.RET_VVC()
            Vw = pr.RET_VW()
            VKij = pr.RET_VKij

            i = 0
            k = 0
            Do
                If Vz(i) <> 0 Then
                    Vm2(k) = Vz(i)
                    VTc2(k) = VTc(i)
                    VPc2(k) = Vpc(i)
                    VVc2(k) = VVc(i)
                    Vw2(k) = Vw(i)
                    j = 0
                    l = 0
                    Do
                        If Vz(l) <> 0 Then
                            VKij2(k, j) = VKij(i, l)
                            j = j + 1
                        End If
                        l = l + 1
                    Loop Until l = n + 1
                    k = k + 1
                End If
                i = i + 1
            Loop Until i = n + 1

            'Try

            Dim pc As New ArrayList, tmp As Object

            If Frm.Options.SelectedPropertyPackage.ComponentName.Contains("Peng-Robinson (PR)") Then

                Me.cp = New DWSIM.Utilities.TCP.Methods
                pc = Me.cp.CRITPT_PR(Vm2, VTc2, VPc2, VVc2, Vw2, VKij2)

            ElseIf Frm.Options.SelectedPropertyPackage.ComponentName.Contains("SRK") Then

                Me.cps = New DWSIM.Utilities.TCP.Methods_SRK
                pc = Me.cps.CRITPT_PR(Vm2, VTc2, VPc2, VVc2, Vw2, VKij2)

            End If

            Dim ppc, ptc, pvc, pzc, tpc, ttc, tvc, tzc As Double

            ppc = Format(cv.ConverterDoSI(su.spmp_pressure, pr.AUX_PCM(DWSIM.SimulationObjects.PropertyPackages.Fase.Mixture)), nf)
            ptc = Format(cv.ConverterDoSI(su.spmp_temperature, pr.AUX_TCM(DWSIM.SimulationObjects.PropertyPackages.Fase.Mixture)), nf)
            pvc = Format(cv.ConverterDoSI(su.molar_volume, pr.AUX_VCM(DWSIM.SimulationObjects.PropertyPackages.Fase.Mixture) * 1000), nf)
            pzc = Format(pr.AUX_ZCM(DWSIM.SimulationObjects.PropertyPackages.Fase.Mixture), nf)

            Grid1.Rows.Add(New Object() {Grid1.Rows.Count + 1, "PCP", ptc, ppc, pvc, pzc})


            If pc.Count > 0 Then

                tmp = pc(0)

                For Each tmp In pc
                    ttc = Format(cv.ConverterDoSI(su.spmp_temperature, tmp(0)), nf)
                    tpc = Format(cv.ConverterDoSI(su.spmp_pressure, tmp(1)), nf)
                    tvc = Format(cv.ConverterDoSI(su.molar_volume, tmp(2) * 1000), nf)
                    tzc = Format(tmp(1) * tmp(2) / (8.314 * tmp(0)), nf)
                    Grid1.Rows.Add(New Object() {Grid1.Rows.Count + 1, "TCP", ttc, tpc, tvc, tzc})
                Next

            End If

            pc = Nothing

        Else

            Me.mat = Nothing
            Me.LblSelected.Text = ""

        End If

    End Sub

    Private Sub FrmCritpt_HelpRequested(sender As System.Object, hlpevent As System.Windows.Forms.HelpEventArgs) Handles MyBase.HelpRequested
        DWSIM.App.HelpRequested("UT_TrueCriticalPoint.htm")
    End Sub
End Class