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

Imports OutlookStyleControls
Imports System.Xml.Serialization
Imports DWSIM.DWSIM.ClassesBasicasTermodinamica
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Runtime.Serialization.Formatters
Imports System.IO
Imports DWSIM.DWSIM.Outros
Imports DWSIM.DWSIM.Flowsheet.FlowsheetSolver
Imports System.Linq

Public Class FormSimulSettings

    Inherits WeifenLuo.WinFormsUI.Docking.DockContent

    Private FrmChild As FormFlowsheet
    Dim loaded As Boolean = False
    Dim initialized As Boolean = False
    Public supports As Boolean = True

    Private prevsort As System.ComponentModel.ListSortDirection = System.ComponentModel.ListSortDirection.Ascending
    Private prevcol As Integer = 1
    Private prevgroup As IOutlookGridGroup

    'Public ComponentDataSet As DataSet
    'Public ComponentBindingSource As BindingSource
    Dim ACSC1 As AutoCompleteStringCollection

    Dim vdPP, vdSR As MessageBox()

    Private Sub FormSimulSettings_DockStateChanged(sender As Object, e As EventArgs) Handles Me.DockStateChanged
        If Not Me.DockHandler Is Nothing OrElse Not Me.DockHandler.FloatPane Is Nothing Then
            ' set the bounds of this form's FloatWindow to our desired position and size
            If Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.Float Then
                Dim floatWin = Me.DockHandler.FloatPane.FloatWindow
                If Not floatWin Is Nothing Then
                    floatWin.SetBounds(floatWin.Location.X, floatWin.Location.Y, 820, 490)
                End If
            End If
        End If
    End Sub

    Private Sub FormStSim_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        '818; 483

        Me.TabText = Me.Text

        If DWSIM.App.IsRunningOnMono Then
            Me.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.Float
        Else
            Me.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.Document
        End If

        initialized = True

        If DWSIM.App.IsRunningOnMono Then
            Me.ListViewA.View = View.List
            Me.ogc1.SelectionMode = DataGridViewSelectionMode.CellSelect
            Me.dgvpp.SelectionMode = DataGridViewSelectionMode.CellSelect
        End If

        Init()

    End Sub

    Private Sub FormStSim_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing

        If Me.FrmChild.Options.SelectedComponents.Count = 0 And Me.FrmChild.Options.PropertyPackages.Count = 0 Then
            MessageBox.Show(DWSIM.App.GetLocalString("Adicionesubstnciassi"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
            MessageBox.Show(DWSIM.App.GetLocalString("NoexistemPacotesdePr"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
            'e.Cancel = True
        ElseIf Me.FrmChild.Options.SelectedComponents.Count = 0 Then
            MessageBox.Show(DWSIM.App.GetLocalString("Adicionesubstnciassi"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
            'e.Cancel = True
        ElseIf Me.FrmChild.Options.PropertyPackages.Count = 0 Then
            MessageBox.Show(DWSIM.App.GetLocalString("NoexistemPacotesdePr"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
            'e.Cancel = True
        Else
            FrmChild.FormProps.PGEx1.Refresh()
            FrmChild.FormProps.PGEx2.Refresh()
        End If

    End Sub

    Sub Init(Optional ByVal reset As Boolean = False)

        Dim pathsep As Char = Path.DirectorySeparatorChar

        FrmChild = My.Application.ActiveSimulation

        Dim comp As DWSIM.ClassesBasicasTermodinamica.ConstantProperties
        If Not loaded Or reset Then

            ACSC1 = New AutoCompleteStringCollection

            Me.ListViewA.Items.Clear()
            For Each comp In Me.FrmChild.Options.SelectedComponents.Values
                Me.ListViewA.Items.Add(comp.Name, DWSIM.App.GetComponentName(comp.Name) & " (" & comp.OriginalDB & ")", 0).Tag = comp.Name
            Next
            For Each comp In Me.FrmChild.Options.NotSelectedComponents.Values
                Dim idx As Integer = Me.AddCompToGrid(comp)
                If Not idx = -1 Then
                    For Each c As DataGridViewCell In Me.ogc1.Rows(idx).Cells
                        If comp.OriginalDB <> "Electrolytes" Then
                            If comp.Acentric_Factor = 0.0# Or comp.Critical_Compressibility = 0.0# Then
                                c.Style.ForeColor = Color.Red
                                c.ToolTipText = DWSIM.App.GetLocalString("CompMissingData")
                            End If
                        End If
                    Next
                End If
            Next

            'Me.TextBox1.AutoCompleteCustomSource = ACSC1

            'property packages
            Me.DataGridViewPP.Rows.Clear()
            For Each pp2 As DWSIM.SimulationObjects.PropertyPackages.PropertyPackage In FormMain.PropertyPackages.Values
                Me.DataGridViewPP.Rows.Add(New String() {pp2.ComponentName, pp2.ComponentName, pp2.ComponentDescription})
            Next

        Else

            For Each r As DataGridViewRow In ogc1.Rows
                If FrmChild.Options.NotSelectedComponents.ContainsKey(r.Cells(0).Value) Then
                    comp = FrmChild.Options.NotSelectedComponents(r.Cells(0).Value)
                    For Each c As DataGridViewCell In r.Cells
                        If comp.Acentric_Factor = 0.0# Or comp.Critical_Compressibility = 0.0# Then
                            c.Style.ForeColor = Color.Red
                            c.ToolTipText = DWSIM.App.GetLocalString("CompMissingData")
                        End If
                    Next
                End If
            Next

        End If

        With Me.dgvpp.Rows
            .Clear()
            For Each pp2 As DWSIM.SimulationObjects.PropertyPackages.PropertyPackage In FrmChild.Options.PropertyPackages.Values
                .Add(New Object() {pp2.UniqueID, pp2.Tag, pp2.ComponentName})
            Next
        End With

        Dim array1(FormMain.AvailableUnitSystems.Count - 1) As String
        FormMain.AvailableUnitSystems.Keys.CopyTo(array1, 0)
        Me.ComboBox2.Items.Clear()
        Me.ComboBox2.Items.AddRange(array1)
        FrmChild.ToolStripComboBoxUnitSystem.Items.Clear()
        FrmChild.ToolStripComboBoxUnitSystem.Items.AddRange(array1)

        ComboBox1.SelectedItem = Me.FrmChild.Options.NumberFormat
        ComboBox3.SelectedItem = Me.FrmChild.Options.FractionNumberFormat

        CheckBox1.Checked = Me.FrmChild.Options.CalculateBubbleAndDewPoints

        chkValidateEqCalc.Checked = Me.FrmChild.Options.ValidateEquilibriumCalc

        chkDoPhaseId.Checked = Me.FrmChild.Options.UsePhaseIdentificationAlgorithm

        tbFlashValidationTolerance.Enabled = chkValidateEqCalc.Checked

        tbFlashValidationTolerance.Text = Me.FrmChild.Options.FlashValidationDGETolerancePct

        Select Case Me.FrmChild.Options.PropertyPackageFlashAlgorithm
            Case DWSIM.SimulationObjects.PropertyPackages.FlashMethod.DWSIMDefault
                ComboBoxFlashAlg.SelectedIndex = 0
            Case DWSIM.SimulationObjects.PropertyPackages.FlashMethod.NestedLoops3P,
                    DWSIM.SimulationObjects.PropertyPackages.FlashMethod.NestedLoops3PV2,
                    DWSIM.SimulationObjects.PropertyPackages.FlashMethod.NestedLoops3PV3
                ComboBoxFlashAlg.SelectedIndex = 1
            Case DWSIM.SimulationObjects.PropertyPackages.FlashMethod.InsideOut
                ComboBoxFlashAlg.SelectedIndex = 2
            Case DWSIM.SimulationObjects.PropertyPackages.FlashMethod.InsideOut3P
                ComboBoxFlashAlg.SelectedIndex = 3
            Case DWSIM.SimulationObjects.PropertyPackages.FlashMethod.GibbsMin2P
                ComboBoxFlashAlg.SelectedIndex = 4
            Case DWSIM.SimulationObjects.PropertyPackages.FlashMethod.GibbsMin3P
                ComboBoxFlashAlg.SelectedIndex = 5
            Case DWSIM.SimulationObjects.PropertyPackages.FlashMethod.NestedLoopsSLE
                ComboBoxFlashAlg.SelectedIndex = 6
            Case DWSIM.SimulationObjects.PropertyPackages.FlashMethod.NestedLoopsSLE_SS
                ComboBoxFlashAlg.SelectedIndex = 7
            Case DWSIM.SimulationObjects.PropertyPackages.FlashMethod.NestedLoopsImmiscible
                ComboBoxFlashAlg.SelectedIndex = 8
            Case Else
                ComboBoxFlashAlg.SelectedIndex = 0
        End Select

        FrmChild.ToolStripComboBoxNumberFormatting.SelectedItem = Me.FrmChild.Options.NumberFormat
        FrmChild.ToolStripComboBoxNumberFractionFormatting.SelectedItem = Me.FrmChild.Options.FractionNumberFormat

        If Me.FrmChild.Options.SelectedUnitSystem.nome <> "" Then
            ComboBox2.SelectedItem = Me.FrmChild.Options.SelectedUnitSystem.nome
            FrmChild.ToolStripComboBoxUnitSystem.SelectedItem = ComboBox2.SelectedItem
        Else
            ComboBox2.SelectedIndex = 0
            FrmChild.ToolStripComboBoxUnitSystem.SelectedIndex = 0
        End If

        CheckBox3.Checked = FrmChild.Options.SempreCalcularFlashPH

        Me.TBaut.Text = Me.FrmChild.Options.SimAutor
        Me.TBdesc.Text = Me.FrmChild.Options.SimComentario
        Me.TBtit.Text = Me.FrmChild.Options.SimNome

        SetupKeyCompounds()

        Select Case FrmChild.Options.ThreePhaseFlashStabTestSeverity
            Case 0
                Me.RadioButton1.Checked = True
            Case 1
                Me.RadioButton2.Checked = True
            Case 2
                Me.RadioButton3.Checked = True
        End Select

        Me.tbPassword.Text = FrmChild.Options.Password
        Me.chkUsePassword.Checked = FrmChild.Options.UsePassword

        If DWSIM.App.IsRunningOnMono Then btnConfigPP.Enabled = True

        Me.loaded = True

    End Sub

    Private Sub SetupKeyCompounds()
        Dim comps, selected As New ArrayList
        If FrmChild.Options.ThreePhaseFlashStabTestCompIds Is Nothing Then FrmChild.Options.ThreePhaseFlashStabTestCompIds = New String() {}
        For Each c As ConstantProperties In FrmChild.Options.SelectedComponents.Values
            comps.Add(c.Name)
            For Each s As String In FrmChild.Options.ThreePhaseFlashStabTestCompIds
                If s = c.Name Then
                    selected.Add(c.Name)
                    Exit For
                End If
            Next
        Next

        Me.ListView2.Items.Clear()

        Dim i, n As Integer
        n = comps.Count - 1
        For i = 0 To n
            With Me.ListView2.Items.Add(DWSIM.App.GetComponentName(comps(i)))
                For Each s As String In selected
                    If s = comps(i) Then
                        .Checked = True
                        Exit For
                    End If
                Next
                .Tag = comps(i)
            End With
        Next
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

        Me.Close()

    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBox1.SelectedIndexChanged
        FrmChild.Options.NumberFormat = Me.ComboBox1.SelectedItem
    End Sub

    Private Sub CheckBox3_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBox3.CheckedChanged
        FrmChild.Options.SempreCalcularFlashPH = CheckBox3.Checked
    End Sub

    Private Sub TBtit_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBtit.TextChanged
        If Me.loaded Then
            Me.FrmChild.Options.SimNome = Me.TBtit.Text
            Me.FrmChild.Text = Me.TBtit.Text + " (" + Me.FrmChild.Options.FilePath + ")"
        End If
    End Sub

    Private Sub TBaut_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBaut.TextChanged
        If Me.loaded Then Me.FrmChild.Options.SimAutor = Me.TBaut.Text
    End Sub

    Private Sub TBdesc_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBdesc.TextChanged
        If Me.loaded Then Me.FrmChild.Options.SimComentario = Me.TBdesc.Text
    End Sub

    Private Sub Button2_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Me.FrmChild.FrmPCBulk.ShowDialog(Me)
    End Sub

    Private Sub ComboBox2_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBox2.SelectedIndexChanged

        FrmChild.Options.SelectedUnitSystem = FormMain.AvailableUnitSystems.Item(ComboBox2.SelectedItem.ToString)
        Dim su As DWSIM.SistemasDeUnidades.Unidades = FrmChild.Options.SelectedUnitSystem

        With Me.DataGridView1.Rows
            .Clear()
            .Add(New String() {DWSIM.App.GetLocalString("Temperatura"), su.spmp_temperature, DWSIM.App.GetLocalString("Presso"), su.spmp_pressure})
            .Add(New String() {DWSIM.App.GetLocalString("Vazomssica"), su.spmp_massflow, DWSIM.App.GetLocalString("Vazomolar"), su.spmp_molarflow})
            .Add(New String() {DWSIM.App.GetLocalString("Vazovolumtrica"), su.spmp_volumetricFlow, DWSIM.App.GetLocalString("EntalpiaEspecfica"), su.spmp_enthalpy})
            .Add(New String() {DWSIM.App.GetLocalString("EntropiaEspecfica"), su.spmp_entropy, DWSIM.App.GetLocalString("Massamolar"), su.spmp_molecularWeight})
            .Add(New String() {DWSIM.App.GetLocalString("Massaespecfica"), su.spmp_density, DWSIM.App.GetLocalString("Tensosuperficial"), su.tdp_surfaceTension})
            .Add(New String() {DWSIM.App.GetLocalString("CapacidadeCalorfica"), su.spmp_heatCapacityCp, DWSIM.App.GetLocalString("Condutividadetrmica"), su.spmp_thermalConductivity})
            .Add(New String() {DWSIM.App.GetLocalString("Viscosidadecinemtica"), su.spmp_cinematic_viscosity, DWSIM.App.GetLocalString("Viscosidadedinmica"), su.spmp_viscosity})
            .Add(New String() {DWSIM.App.GetLocalString("DeltaT2"), su.spmp_deltaT, DWSIM.App.GetLocalString("DeltaP"), su.spmp_deltaP})
            .Add(New String() {DWSIM.App.GetLocalString("ComprimentoHead"), su.spmp_head, DWSIM.App.GetLocalString("FluxodeEnergia"), su.spmp_heatflow})
            .Add(New String() {DWSIM.App.GetLocalString("Tempo"), su.time, DWSIM.App.GetLocalString("Volume"), su.volume})
            .Add(New String() {DWSIM.App.GetLocalString("VolumeMolar"), su.molar_volume, DWSIM.App.GetLocalString("rea"), su.area})
            .Add(New String() {DWSIM.App.GetLocalString("DimetroEspessura"), su.diameter, DWSIM.App.GetLocalString("Fora"), su.force})
            .Add(New String() {DWSIM.App.GetLocalString("Aceleracao"), su.accel, DWSIM.App.GetLocalString("CoefdeTransfdeCalor"), su.heat_transf_coeff})
            .Add(New String() {DWSIM.App.GetLocalString("ConcMolar"), su.molar_conc, DWSIM.App.GetLocalString("ConcMssica"), su.mass_conc})
            .Add(New String() {DWSIM.App.GetLocalString("TaxadeReao"), su.reac_rate, DWSIM.App.GetLocalString("VolEspecfico"), su.spec_vol})
            .Add(New String() {DWSIM.App.GetLocalString("MolarEnthalpy"), su.molar_enthalpy, DWSIM.App.GetLocalString("MolarEntropy"), su.molar_entropy})
            .Add(New String() {DWSIM.App.GetLocalString("Velocity"), su.velocity, DWSIM.App.GetLocalString("HXFoulingFactor"), su.foulingfactor})
            .Add(New String() {DWSIM.App.GetLocalString("FilterSpecificCakeResistance"), su.cakeresistance, DWSIM.App.GetLocalString("FilterMediumResistance"), su.mediumresistance})
        End With

        If ComboBox2.SelectedIndex <= 2 Then
            Me.DataGridView1.Columns(1).ReadOnly = True
            Me.DataGridView1.Columns(3).ReadOnly = True
        Else
            Me.DataGridView1.Columns(1).ReadOnly = False
            Me.DataGridView1.Columns(3).ReadOnly = False
        End If

        '.Add(New Object() {DWSIM.App.GetLocalString("Temperatura")})
        With DirectCast(Me.DataGridView1.Rows.Item(0).Cells(1), DataGridViewComboBoxCell)
            .Items.Clear()
            .Items.AddRange(New String() {"K", "R", "C", "F"})
            .Style.Tag = 1
            .Value = su.spmp_temperature
        End With

        '.Add(New Object() {DWSIM.App.GetLocalString("Presso")})
        With DirectCast(Me.DataGridView1.Rows.Item(0).Cells(3), DataGridViewComboBoxCell)
            .Items.Clear()
            .Items.AddRange(New String() {"Pa", "atm", "kgf/cm2", "kgf/cm2g", "lbf/ft2", "kPa", "kPag", "bar", "barg", "ftH2O", "inH2O", "inHg", "mbar", "mH2O", "mmH2O", "mmHg", "MPa", "psi", "psig"})
            .Style.Tag = 2
            .Value = su.spmp_pressure
        End With

        '.Add(New Object() {DWSIM.App.GetLocalString("Vazomssica")})
        With DirectCast(Me.DataGridView1.Rows.Item(1).Cells(1), DataGridViewComboBoxCell)
            .Items.Clear()
            .Items.AddRange(New String() {"g/s", "lbm/h", "kg/s", "kg/h", "kg/d", "kg/min", "lb/min", "lb/s"})
            .Style.Tag = 3
            .Value = su.spmp_massflow
        End With

        '.Add(New Object() {DWSIM.App.GetLocalString("Vazomolar")})
        With DirectCast(Me.DataGridView1.Rows.Item(1).Cells(3), DataGridViewComboBoxCell)
            .Items.Clear()
            .Items.AddRange(New String() {"mol/s", "lbmol/h", "mol/h", "mol/d", "kmol/s", "kmol/h", "kmol/d", "m3/d @ BR", "m3/d @ NC", "m3/d @ CNTP", "m3/d @ SC"})
            .Value = su.spmp_molarflow
            .Style.Tag = 4
        End With

        '.Add(New Object() {DWSIM.App.GetLocalString("Vazovolumtrica")})
        With DirectCast(Me.DataGridView1.Rows.Item(2).Cells(1), DataGridViewComboBoxCell)
            .Items.Clear()
            .Items.AddRange(New String() {"m3/s", "ft3/s", "cm3/s", "m3/h", "m3/d", "bbl/h", "bbl/d", "ft3/min", "gal[UK]/h", "gal[UK]/s", "gal[US]/h", "gal[US]/min", "L/h", "L/min", "L/s"})
            .Value = su.spmp_volumetricFlow
            .Style.Tag = 5
        End With

        '.Add(New Object() {DWSIM.App.GetLocalString("EntalpiaEspecfica")})
        With DirectCast(Me.DataGridView1.Rows.Item(2).Cells(3), DataGridViewComboBoxCell)
            .Items.Clear()
            .Items.AddRange(New String() {"kJ/kg", "cal/g", "BTU/lbm", "kcal/kg"})
            .Value = su.spmp_enthalpy
            .Style.Tag = 6
        End With

        '.Add(New Object() {DWSIM.App.GetLocalString("EntropiaEspecfica")})
        With DirectCast(Me.DataGridView1.Rows.Item(3).Cells(1), DataGridViewComboBoxCell)
            .Items.Clear()
            .Items.AddRange(New String() {"kJ/[kg.K]", "cal/[g.C]", "BTU/[lbm.R]"})
            .Value = su.spmp_entropy
            .Style.Tag = 7
        End With

        '.Add(New Object() {DWSIM.App.GetLocalString("Massamolar")})
        With DirectCast(Me.DataGridView1.Rows.Item(3).Cells(3), DataGridViewComboBoxCell)
            .Items.Clear()
            .Items.AddRange(New String() {"kg/kmol", "g/mol", "lbm/lbmol"})
            .Value = su.spmp_molecularWeight
            .Style.Tag = 8
        End With

        '.Add(New Object() {DWSIM.App.GetLocalString("Massaespecfica")})
        With DirectCast(Me.DataGridView1.Rows.Item(4).Cells(1), DataGridViewComboBoxCell)
            .Items.Clear()
            .Items.AddRange(New String() {"kg/m3", "g/cm3", "lbm/ft3"})
            .Value = su.spmp_density
            .Style.Tag = 10
        End With

        '.Add(New Object() {DWSIM.App.GetLocalString("Tensosuperficial")})
        With DirectCast(Me.DataGridView1.Rows.Item(4).Cells(3), DataGridViewComboBoxCell)
            .Items.Clear()
            .Items.AddRange(New String() {"N/m", "dyn/cm", "lbf/in"})
            .Value = su.tdp_surfaceTension
            .Style.Tag = 9
        End With

        '.Add(New Object() {DWSIM.App.GetLocalString("CapacidadeCalorfica")})
        With DirectCast(Me.DataGridView1.Rows.Item(5).Cells(1), DataGridViewComboBoxCell)
            .Items.Clear()
            .Items.AddRange(New String() {"kJ/[kg.K]", "cal/[g.C]", "BTU/[lbm.R]"})
            .Value = su.spmp_heatCapacityCp
            .Style.Tag = 11
        End With

        '.Add(New Object() {DWSIM.App.GetLocalString("Condutividadetrmica")})
        With DirectCast(Me.DataGridView1.Rows.Item(5).Cells(3), DataGridViewComboBoxCell)
            .Items.Clear()
            .Items.AddRange(New String() {"W/[m.K]", "cal/[cm.s.C]", "BTU/[ft.h.R]"})
            .Value = su.spmp_thermalConductivity
            .Style.Tag = 12
        End With

        '.Add(New Object() {DWSIM.App.GetLocalString("Viscosidadecinemtica")})
        With DirectCast(Me.DataGridView1.Rows.Item(6).Cells(1), DataGridViewComboBoxCell)
            .Items.Clear()
            .Items.AddRange(New String() {"m2/s", "cSt", "ft2/s", "mm2/s"})
            .Value = su.spmp_cinematic_viscosity
            .Style.Tag = 13
        End With

        '.Add(New Object() {DWSIM.App.GetLocalString("ViscosidadeDinmica1")})
        With DirectCast(Me.DataGridView1.Rows.Item(6).Cells(3), DataGridViewComboBoxCell)
            .Items.Clear()
            .Items.AddRange(New String() {"kg/[m.s]", "Pa.s", "cP", "lbm/[ft.h]"})
            .Value = su.spmp_viscosity
            .Style.Tag = 14
        End With

        '.Add(New Object() {DWSIM.App.GetLocalString("DeltaP")})
        With DirectCast(Me.DataGridView1.Rows.Item(7).Cells(3), DataGridViewComboBoxCell)
            .Items.Clear()
            .Items.AddRange(New String() {"Pa", "atm", "lbf/ft2", "kgf/cm2", "kPa", "bar", "ftH2O", "inH2O", "inHg", "mbar", "mH2O", "mmH2O", "mmHg", "MPa", "psi"})
            .Value = su.spmp_deltaP
            .Style.Tag = 15
        End With

        '.Add(New Object() {DWSIM.App.GetLocalString("DeltaT2")})
        With DirectCast(Me.DataGridView1.Rows.Item(7).Cells(1), DataGridViewComboBoxCell)
            .Items.Clear()
            .Items.AddRange(New String() {"C.", "K.", "F.", "R."})
            .Value = su.spmp_deltaT
            .Style.Tag = 16
        End With

        '.Add(New Object() {DWSIM.App.GetLocalString("ComprimentoHead")})
        With DirectCast(Me.DataGridView1.Rows.Item(8).Cells(1), DataGridViewComboBoxCell)
            .Items.Clear()
            .Items.AddRange(New String() {"m", "ft", "cm"})
            .Value = su.spmp_head
            .Style.Tag = 17
        End With

        '.Add(New Object() {DWSIM.App.GetLocalString("FluxodeEnergia")})
        With DirectCast(Me.DataGridView1.Rows.Item(8).Cells(3), DataGridViewComboBoxCell)
            .Items.Clear()
            .Items.AddRange(New String() {"kW", "kcal/h", "BTU/h", "BTU/s", "cal/s", "HP", "kJ/h", "kJ/d", "MW", "W"})
            .Value = su.spmp_heatflow
            .Style.Tag = 18
        End With

        '.Add(New Object() {DWSIM.App.GetLocalString("Tempo")})
        With DirectCast(Me.DataGridView1.Rows.Item(9).Cells(1), DataGridViewComboBoxCell)
            .Items.Clear()
            .Items.AddRange(New String() {"s", "min.", "h"})
            .Value = su.time
            .Style.Tag = 19
        End With

        '.Add(New Object() {DWSIM.App.GetLocalString("Volume")})
        With DirectCast(Me.DataGridView1.Rows.Item(9).Cells(3), DataGridViewComboBoxCell)
            .Items.Clear()
            .Items.AddRange(New String() {"m3", "cm3", "L", "ft3"})
            .Value = su.volume
            .Style.Tag = 20
        End With

        '.Add(New Object() {DWSIM.App.GetLocalString("VolumeMolar")})
        With DirectCast(Me.DataGridView1.Rows.Item(10).Cells(1), DataGridViewComboBoxCell)
            .Items.Clear()
            .Items.AddRange(New String() {DWSIM.App.GetLocalString("m3kmol"), "cm3/mmol", "ft3/lbmol"})
            .Value = su.molar_volume
            .Style.Tag = 21
        End With

        '.Add(New Object() {DWSIM.App.GetLocalString("rea")})
        With DirectCast(Me.DataGridView1.Rows.Item(10).Cells(3), DataGridViewComboBoxCell)
            .Items.Clear()
            .Items.AddRange(New String() {"m2", "cm2", "ft2"})
            .Value = su.area
            .Style.Tag = 22
        End With

        '.Add(New Object() {DWSIM.App.GetLocalString("DimetroEspessura")})
        With DirectCast(Me.DataGridView1.Rows.Item(11).Cells(1), DataGridViewComboBoxCell)
            .Items.Clear()
            .Items.AddRange(New String() {"m", "mm", "cm", "in", "ft"})
            .Value = su.diameter
            .Style.Tag = 23
        End With

        '.Add(New Object() {DWSIM.App.GetLocalString("Fora")})
        With DirectCast(Me.DataGridView1.Rows.Item(11).Cells(3), DataGridViewComboBoxCell)
            .Items.Clear()
            .Items.AddRange(New String() {DWSIM.App.GetLocalString("N"), "dyn", "kgf", "lbf"})
            .Value = su.force
            .Style.Tag = 24
        End With

        '.Add(New Object() {DWSIM.App.GetLocalString("CoefdeTransfdeCalor")})
        With DirectCast(Me.DataGridView1.Rows.Item(12).Cells(3), DataGridViewComboBoxCell)
            .Items.Clear()
            .Items.AddRange(New String() {"W/[m2.K]", "cal/[cm2.s.C]", "BTU/[ft2.h.R]"})
            .Value = su.heat_transf_coeff
            .Style.Tag = 25
        End With

        '.Add(New Object() {DWSIM.App.GetLocalString("Aceleracao")})
        With DirectCast(Me.DataGridView1.Rows.Item(12).Cells(1), DataGridViewComboBoxCell)
            .Items.Clear()
            .Items.AddRange(New String() {"m/s2", "cm/s2", "ft/s2"})
            .Value = su.accel
            .Style.Tag = 26
        End With

        '.Add(New Object() {DWSIM.App.GetLocalString("ConcentraoMolar")})
        With DirectCast(Me.DataGridView1.Rows.Item(13).Cells(1), DataGridViewComboBoxCell)
            .Items.AddRange(New String() {"kmol/m3", "mol/m3", "mol/L", "mol/cm3", "mol/mL", "lbmol/ft3"})
            .Value = su.molar_conc
            .Style.Tag = 28
        End With

        '.Add(New Object() {DWSIM.App.GetLocalString("ConcentraoMssica")})
        With DirectCast(Me.DataGridView1.Rows.Item(13).Cells(3), DataGridViewComboBoxCell)
            .Items.Clear()
            .Items.AddRange(New String() {"kg/m3", "g/L", "g/cm3", "g/mL", "lbm/ft3"})
            .Value = su.mass_conc
            .Style.Tag = 29
        End With

        '.Add(New Object() {DWSIM.App.GetLocalString("TaxadeReao")})
        With DirectCast(Me.DataGridView1.Rows.Item(14).Cells(1), DataGridViewComboBoxCell)
            .Items.Clear()
            .Items.AddRange(New String() {"kmol/[m3.s]", "kmol/[m3.min.]", "kmol/[m3.h]", "mol/[m3.s]", "mol/[m3.min.]", "mol/[m3.h]", "mol/[L.s]", "mol/[L.min.]", "mol/[L.h]", "mol/[cm3.s]", "mol/[cm3.min.]", "mol/[cm3.h]", "lbmol.[ft3.h]"})
            .Value = su.reac_rate
            .Style.Tag = 30
        End With

        '.Add(New Object() {DWSIM.App.GetLocalString("VolumeEspecfico")})
        With DirectCast(Me.DataGridView1.Rows.Item(14).Cells(3), DataGridViewComboBoxCell)
            .Items.Clear()
            .Items.AddRange(New String() {"m3/kg", "cm3/g", "ft3/lbm"})
            .Value = su.spec_vol
            .Style.Tag = 27
        End With

        '.Add(New Object() {DWSIM.App.GetLocalString("MolarEnthalpy")})
        With DirectCast(Me.DataGridView1.Rows.Item(15).Cells(1), DataGridViewComboBoxCell)
            .Items.Clear()
            .Items.AddRange(New String() {"kJ/kmol", "cal/mol", "BTU/lbmol"})
            .Value = su.molar_enthalpy
            .Style.Tag = 31
        End With

        '.Add(New Object() {DWSIM.App.GetLocalString("MolarEntropy")})
        With DirectCast(Me.DataGridView1.Rows.Item(15).Cells(3), DataGridViewComboBoxCell)
            .Items.Clear()
            .Items.AddRange(New String() {"kJ/[kmol.K]", "cal/[mol.C]", "BTU/[lbmol.R]"})
            .Value = su.molar_entropy
            .Style.Tag = 32
        End With

        '.Add(New Object() {DWSIM.App.GetLocalString("Velocity")})
        With DirectCast(Me.DataGridView1.Rows.Item(16).Cells(1), DataGridViewComboBoxCell)
            .Items.Clear()
            .Items.AddRange(New String() {"m/s", "cm/s", "mm/s", "km/h", "ft/h", "ft/min", "ft/s", "in/s"})
            .Value = su.velocity
            .Style.Tag = 33
        End With

        '.Add(New Object() {DWSIM.App.GetLocalString("HXFoulingFactor")})
        With DirectCast(Me.DataGridView1.Rows.Item(16).Cells(3), DataGridViewComboBoxCell)
            .Items.Clear()
            .Items.AddRange(New String() {"K.m2/W", "C.cm2.s/cal", "ft2.h.F/BTU"})
            .Value = su.foulingfactor
            .Style.Tag = 34
        End With

        '.Add(New Object() {DWSIM.App.GetLocalString("FilterSpecificCakeResistance")})
        With DirectCast(Me.DataGridView1.Rows.Item(17).Cells(1), DataGridViewComboBoxCell)
            .Items.Clear()
            .Items.AddRange(New String() {"m/kg", "ft/lbm", "cm/g"})
            .Value = su.cakeresistance
            .Style.Tag = 35
        End With

        '.Add(New Object() {DWSIM.App.GetLocalString("FilterMediumResistance")})
        With DirectCast(Me.DataGridView1.Rows.Item(17).Cells(3), DataGridViewComboBoxCell)
            .Items.Clear()
            .Items.AddRange(New String() {"m-1", "cm-1", "ft-1"})
            .Value = su.mediumresistance
            .Style.Tag = 36
        End With

        FrmChild.ToolStripComboBoxUnitSystem.SelectedItem = ComboBox2.SelectedItem

    End Sub

    Private Sub DataGridView1_CellValueChanged1(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles DataGridView1.CellValueChanged

        If loaded Then

            Dim cell As DataGridViewCell = Me.DataGridView1.Rows(e.RowIndex).Cells(e.ColumnIndex)

            Dim su As DWSIM.SistemasDeUnidades.Unidades = FrmChild.Options.SelectedUnitSystem

            Select Case cell.Style.Tag
                Case 1
                    su.spmp_temperature = cell.Value
                Case 2
                    su.spmp_pressure = cell.Value
                Case 3
                    su.spmp_massflow = cell.Value
                Case 4
                    su.spmp_molarflow = cell.Value
                Case 5
                    su.spmp_volumetricFlow = cell.Value
                Case 6
                    su.spmp_enthalpy = cell.Value
                Case 7
                    su.spmp_entropy = cell.Value
                Case 8
                    su.spmp_molecularWeight = cell.Value
                Case 9
                    su.tdp_surfaceTension = cell.Value
                Case 10
                    su.spmp_density = cell.Value
                Case 11
                    su.spmp_heatCapacityCp = cell.Value
                Case 12
                    su.spmp_thermalConductivity = cell.Value
                Case 13
                    su.spmp_cinematic_viscosity = cell.Value
                Case 14
                    su.spmp_viscosity = cell.Value
                Case 15
                    su.spmp_deltaP = cell.Value
                Case 16
                    su.spmp_deltaT = cell.Value
                Case 17
                    su.spmp_head = cell.Value
                Case 18
                    su.spmp_heatflow = cell.Value
                Case 19
                    su.time = cell.Value
                Case 20
                    su.volume = cell.Value
                Case 21
                    su.molar_volume = cell.Value
                Case 22
                    su.area = cell.Value
                Case 23
                    su.diameter = cell.Value
                Case 24
                    su.force = cell.Value
                Case 25
                    su.heat_transf_coeff = cell.Value
                Case 26
                    su.accel = cell.Value
                Case 27
                    su.spec_vol = cell.Value
                Case 28
                    su.molar_conc = cell.Value
                Case 29
                    su.mass_conc = cell.Value
                Case 30
                    su.reac_rate = cell.Value
                Case 31
                    su.molar_enthalpy = cell.Value
                Case 32
                    su.molar_entropy = cell.Value
                Case 33
                    su.velocity = cell.Value
                Case 34
                    su.foulingfactor = cell.Value
                Case 35
                    su.cakeresistance = cell.Value
                Case 36
                    su.mediumresistance = cell.Value
            End Select

            Me.FrmChild.FormSurface.UpdateSelectedObject()

            For Each o In Me.FrmChild.Collections.ObjectCollection.Values
                o.UpdatePropertyNodes(Me.FrmChild.Options.SelectedUnitSystem, Me.FrmChild.Options.NumberFormat)
            Next

        End If

    End Sub

    Private Sub KryptonButton15_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles KryptonButton15.Click
        Dim frmUnit As New FormUnitGen
        frmUnit.ShowDialog(Me)
    End Sub

    Private Sub KryptonButton18_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles KryptonButton18.Click

        If Me.ComboBox2.SelectedItem <> DWSIM.App.GetLocalString("SistemaSI") And _
            Me.ComboBox2.SelectedItem <> DWSIM.App.GetLocalString("SistemaCGS") And _
            Me.ComboBox2.SelectedItem <> DWSIM.App.GetLocalString("SistemaIngls") And _
            Me.ComboBox2.SelectedItem <> DWSIM.App.GetLocalString("Personalizado1BR") And _
            Me.ComboBox2.SelectedItem <> DWSIM.App.GetLocalString("Personalizado2SC") And _
            Me.ComboBox2.SelectedItem <> DWSIM.App.GetLocalString("Personalizado3CNTP") Then

            Dim str = Me.ComboBox2.SelectedItem
            My.MyApplication.UserUnitSystems.Remove(str)
            FormMain.AvailableUnitSystems.Remove(Me.ComboBox2.SelectedItem)
            Me.ComboBox2.SelectedIndex = 0
            Me.ComboBox2.Items.Remove(str)
            Me.FrmChild.ToolStripComboBoxUnitSystem.SelectedIndex = 0
            Me.FrmChild.ToolStripComboBoxUnitSystem.Items.Remove(str)

        Else
            MessageBox.Show(DWSIM.App.GetLocalString("EsteSistemadeUnidade"))
        End If


    End Sub

    Private Sub KryptonButton23_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles KryptonButton23.Click

        If Me.ComboBox2.SelectedItem <> DWSIM.App.GetLocalString("SistemaSI") And _
            Me.ComboBox2.SelectedItem <> DWSIM.App.GetLocalString("SistemaCGS") And _
            Me.ComboBox2.SelectedItem <> DWSIM.App.GetLocalString("SistemaIngls") And _
            Me.ComboBox2.SelectedItem <> DWSIM.App.GetLocalString("Personalizado1BR") And _
            Me.ComboBox2.SelectedItem <> DWSIM.App.GetLocalString("Personalizado2SC") And _
            Me.ComboBox2.SelectedItem <> DWSIM.App.GetLocalString("Personalizado3CNTP") Then

            Dim myStream As System.IO.FileStream

            If Me.SaveFileDialog1.ShowDialog() = Windows.Forms.DialogResult.OK Then
                myStream = Me.SaveFileDialog1.OpenFile()
                If Not (myStream Is Nothing) Then
                    Dim su As DWSIM.SistemasDeUnidades.Unidades = FrmChild.Options.SelectedUnitSystem
                    Dim mySerializer As Binary.BinaryFormatter = New Binary.BinaryFormatter(Nothing, New System.Runtime.Serialization.StreamingContext())
                    Try
                        mySerializer.Serialize(myStream, su)
                    Catch ex As System.Runtime.Serialization.SerializationException
                        MessageBox.Show(ex.Message, DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Finally
                        myStream.Close()
                    End Try
                End If
            End If

        Else
            MessageBox.Show(DWSIM.App.GetLocalString("EsteSistemadeUnidade"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If

    End Sub

    Private Sub KryptonButton22_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles KryptonButton22.Click

        Dim myStream As System.IO.FileStream

        If Me.OpenFileDialog2.ShowDialog() = Windows.Forms.DialogResult.OK Then
            myStream = Me.OpenFileDialog2.OpenFile()
            If Not (myStream Is Nothing) Then
                Dim su As New DWSIM.SistemasDeUnidades.Unidades
                Dim mySerializer As Binary.BinaryFormatter = New Binary.BinaryFormatter(Nothing, New System.Runtime.Serialization.StreamingContext())
                Try
                    su = DirectCast(mySerializer.Deserialize(myStream), DWSIM.SistemasDeUnidades.Unidades)
                    If FormMain.AvailableUnitSystems.ContainsKey(su.nome) Then
                        su.nome += "_1"
                    End If
                    FormMain.AvailableUnitSystems.Add(su.nome, su)
                    Me.ComboBox2.Items.Add(su.nome)
                    Me.FrmChild.Options.SelectedUnitSystem.nome = su.nome
                    Dim array1(FormMain.AvailableUnitSystems.Count - 1) As String
                    FormMain.AvailableUnitSystems.Keys.CopyTo(array1, 0)
                    FrmChild.ToolStripComboBoxUnitSystem.Items.Clear()
                    FrmChild.ToolStripComboBoxUnitSystem.Items.AddRange(array1)
                    ComboBox2.SelectedItem = Me.FrmChild.Options.SelectedUnitSystem.nome
                    FrmChild.ToolStripComboBoxUnitSystem.SelectedItem = ComboBox2.SelectedItem
                Catch ex As System.Runtime.Serialization.SerializationException
                    MessageBox.Show(ex.Message, DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                Finally
                    myStream.Close()
                End Try
            End If
        End If

    End Sub

    Private Sub KryptonButton7_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles KryptonButton7.Click
        Dim rm As New FormReacManager
        rm.Show(Me.FrmChild.dckPanel)
    End Sub

    Private Sub ogc1_CellValueChanged(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles ogc1.CellValueChanged

    End Sub

    Public Function AddCompToGrid(ByRef comp As DWSIM.ClassesBasicasTermodinamica.ConstantProperties) As Integer



        'If Not initialized Then
        '    Me.Visible = False
        '    Me.Show()
        '    Me.Visible = False
        'End If

        Dim contains As Boolean = False
        Dim index As Integer = -1
        For Each r As OutlookGridRow In ogc1.Rows
            If r.Cells(0).Value = comp.Name Then
                contains = True
                index = r.Index
            End If
        Next

        Dim translatedname As String = ""

        If Not contains Then
            Try
                Dim r As New OutlookGridRow
                translatedname = DWSIM.App.GetComponentName(comp.Name)
                r.CreateCells(ogc1, New Object() {comp.Name, translatedname, comp.CAS_Number, DWSIM.App.GetComponentType(comp), comp.Formula, comp.OriginalDB, comp.IsCOOLPROPSupported, comp.IsFPROPSSupported})
                ogc1.Rows.Add(r)
                Return ogc1.Rows.Count - 1
            Catch ex As Exception
                Console.WriteLine(ex.ToString)
                Return -1
            Finally
                If ACSC1 Is Nothing Then ACSC1 = New AutoCompleteStringCollection
                ACSC1.Add(translatedname)
                ACSC1.Add(comp.CAS_Number)
                ACSC1.Add(comp.Formula)
                'Me.TextBox1.AutoCompleteCustomSource = ACSC1
            End Try
        Else
            Return index
        End If

    End Function

    Public Function GetCompRowIndex(ByRef comp As DWSIM.ClassesBasicasTermodinamica.ConstantProperties) As Integer

        For Each r As OutlookGridRow In ogc1.Rows

            If r.Cells(0).Value = comp.Name Then Return r.Index Else Return -1

        Next

    End Function


    Private Sub TextBox1_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox1.TextChanged
        For Each r As DataGridViewRow In ogc1.Rows
            If Not r.Cells(1).Value Is Nothing Then
                If r.Cells(1).Value.ToString.ToLower.Contains(Me.TextBox1.Text.ToLower) Or
                   r.Cells(2).Value.ToString.ToLower.Contains(Me.TextBox1.Text.ToLower) Or
                   r.Cells(4).Value.ToString.ToLower.Contains(Me.TextBox1.Text.ToLower) Then
                    r.Visible = True
                    If r.Cells(1).Value.ToString.ToLower.Equals(Me.TextBox1.Text.ToLower) Or
                                       r.Cells(2).Value.ToString.ToLower.Equals(Me.TextBox1.Text.ToLower) Or
                                       r.Cells(4).Value.ToString.ToLower.Equals(Me.TextBox1.Text.ToLower) Then
                        r.Selected = True
                    End If
                Else
                    r.Selected = False
                    r.Visible = False
                End If
            End If
        Next
        If TextBox1.Text = "" Then
            ogc1.FirstDisplayedScrollingRowIndex = 0
            For Each r As DataGridViewRow In ogc1.Rows
                r.Selected = False
                r.Visible = True
            Next
        End If

    End Sub

    Private Sub btnConfigPP_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnConfigPP.Click
        Dim ppid As String = ""
        If DWSIM.App.IsRunningOnMono Then
            ppid = dgvpp.Rows(dgvpp.SelectedCells(0).RowIndex).Cells(0).Value
        Else
            ppid = dgvpp.SelectedRows(0).Cells(0).Value
        End If
        Dim pp As DWSIM.SimulationObjects.PropertyPackages.PropertyPackage = FrmChild.Options.PropertyPackages(ppid)
        pp.ReconfigureConfigForm()
        pp.ConfigForm._pp = pp
        pp.ConfigForm._comps = FrmChild.Options.SelectedComponents
        pp.ConfigForm._form = FrmChild
        pp.ShowConfigForm(FrmChild)
    End Sub

    Private Sub btnDeletePP_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDeletePP.Click
        If DWSIM.App.IsRunningOnMono Then
            If dgvpp.SelectedCells.Count > 0 Then
                FrmChild.Options.PropertyPackages.Remove(dgvpp.Rows(dgvpp.SelectedCells(0).RowIndex).Cells(0).Value)
                dgvpp.Rows.RemoveAt(dgvpp.SelectedCells(0).RowIndex)
            End If
        Else
            If Not dgvpp.SelectedRows.Count = 0 Then
                FrmChild.Options.PropertyPackages.Remove(dgvpp.SelectedRows(0).Cells(0).Value)
                dgvpp.Rows.Remove(dgvpp.SelectedRows(0))
            End If
        End If
    End Sub

    Private Sub btnCopyPP_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCopyPP.Click
        Dim pp As DWSIM.SimulationObjects.PropertyPackages.PropertyPackage
        Try
            Dim ppid As String = ""
            If DWSIM.App.IsRunningOnMono Then
                ppid = dgvpp.Rows(dgvpp.SelectedCells(0).RowIndex).Cells(0).Value
            Else
                ppid = dgvpp.SelectedRows(0).Cells(0).Value
            End If
            pp = FrmChild.Options.PropertyPackages(ppid).Clone
            With pp
                pp.Tag = pp.Tag & CStr(FormFlowsheet.Options.PropertyPackages.Count)
                pp.UniqueID = Guid.NewGuid.ToString
            End With
            FrmChild.Options.PropertyPackages.Add(pp.UniqueID, pp)
            Me.dgvpp.Rows.Add(New Object() {pp.UniqueID, pp.Tag, pp.ComponentName})
        Catch ex As Exception

        End Try
    End Sub

    Private Sub dgvpp_CellValueChanged(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvpp.CellValueChanged
        If loaded Then
            FrmChild.Options.PropertyPackages(dgvpp.Rows(e.RowIndex).Cells(0).Value).Tag = dgvpp.Rows(e.RowIndex).Cells(1).Value
        End If
    End Sub

    Private Sub dgvpp_SelectionChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles dgvpp.SelectionChanged
        If DWSIM.App.IsRunningOnMono Then
            If dgvpp.SelectedCells.Count > 0 Then
                If dgvpp.SelectedCells(0).RowIndex >= 0 Then
                    If dgvpp.Rows(dgvpp.SelectedCells(0).RowIndex).Cells(0).Value <> Nothing Then
                        btnDeletePP.Enabled = True
                        If FrmChild.Options.PropertyPackages.ContainsKey(dgvpp.Rows(dgvpp.SelectedCells(0).RowIndex).Cells(0).Value) Then
                            If FrmChild.Options.PropertyPackages(dgvpp.Rows(dgvpp.SelectedCells(0).RowIndex).Cells(0).Value).IsConfigurable Then btnConfigPP.Enabled = True Else btnConfigPP.Enabled = False
                            btnCopyPP.Enabled = True
                        End If
                    End If
                End If
            End If
        Else
            If dgvpp.SelectedRows.Count > 0 Then
                btnDeletePP.Enabled = True
                If FrmChild.Options.PropertyPackages.ContainsKey(dgvpp.SelectedRows(0).Cells(0).Value) Then
                    If FrmChild.Options.PropertyPackages(dgvpp.SelectedRows(0).Cells(0).Value).IsConfigurable Then btnConfigPP.Enabled = True Else btnConfigPP.Enabled = False
                    btnCopyPP.Enabled = True
                End If
            End If
        End If
    End Sub

    Private Sub Button9_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button9.Click
        Dim frmdc As New DCCharacterizationWizard
        frmdc.ShowDialog(Me)
    End Sub

    Private Sub Button7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button7.Click
        If DWSIM.App.IsRunningOnMono Then
            If Me.ogc1.SelectedCells.Count > 0 Then
                Me.AddCompToSimulation(Me.ogc1.SelectedCells(0).RowIndex)
            End If
        Else
            If Me.ogc1.SelectedRows.Count > 0 Then
                Me.AddCompToSimulation(Me.ogc1.SelectedRows(0).Index)
            End If
        End If
        'If Me.ogc1.SelectedRows.Count > 0 Then
        '    For Each r As DataGridViewRow In Me.ogc1.SelectedRows
        '        Me.AddCompToSimulation(r.Index)
        '    Next
        'End If
    End Sub

    Sub AddCompToSimulation(ByVal index As Integer)

        ' TODO Add code to check that index is within range. If it is out of range, don't do anything.
        If Me.loaded Then

            If Not Me.FrmChild.Options.SelectedComponents.ContainsKey(ogc1.Rows(index).Cells(0).Value) Then

                Dim tmpcomp As New DWSIM.ClassesBasicasTermodinamica.ConstantProperties
                tmpcomp = Me.FrmChild.Options.NotSelectedComponents(ogc1.Rows(index).Cells(0).Value)

                If tmpcomp.OriginalDB = "CoolProp" And My.Settings.ShowCoolPropWarning Then
                    Dim fc As New FormCoolPropWarning
                    fc.ShowDialog()
                End If

                Me.FrmChild.Options.SelectedComponents.Add(tmpcomp.Name, tmpcomp)
                Me.FrmChild.Options.NotSelectedComponents.Remove(tmpcomp.Name)
                Dim ms As DWSIM.SimulationObjects.Streams.MaterialStream

                Dim proplist As New ArrayList
                For Each ms In FrmChild.Collections.CLCS_MaterialStreamCollection.Values
                    For Each phase As DWSIM.ClassesBasicasTermodinamica.Fase In ms.Fases.Values
                        phase.Componentes.Add(tmpcomp.Name, New DWSIM.ClassesBasicasTermodinamica.Substancia(tmpcomp.Name, ""))
                        phase.Componentes(tmpcomp.Name).ConstantProperties = tmpcomp
                    Next

                    proplist.Clear()
                    For Each pi As DWSIM.Outros.NodeItem In ms.NodeTableItems.Values
                        If pi.Checked Then
                            proplist.Add(pi.Text)
                        End If
                    Next
                    ms.FillNodeItems()
                    For Each pi As DWSIM.Outros.NodeItem In ms.NodeTableItems.Values
                        If proplist.Contains(pi.Text) Then
                            pi.Checked = True
                        End If
                    Next
                Next

                Me.ListViewA.Items.Add(tmpcomp.Name, DWSIM.App.GetComponentName(tmpcomp.Name) & " (" & tmpcomp.OriginalDB & ")", 0).Tag = tmpcomp.Name

                If Not DWSIM.App.IsRunningOnMono Then Me.ogc1.Rows.RemoveAt(index)

                SetupKeyCompounds()

            End If


        End If

    End Sub

    Sub RemoveCompFromSimulation(ByVal compid As String)

        Dim tmpcomp As New DWSIM.ClassesBasicasTermodinamica.ConstantProperties
        Dim nm As String = compid
        tmpcomp = Me.FrmChild.Options.SelectedComponents(nm)
        Me.FrmChild.Options.SelectedComponents.Remove(tmpcomp.Name)
        Me.ListViewA.Items.RemoveByKey(tmpcomp.Name)
        Me.FrmChild.Options.NotSelectedComponents.Add(tmpcomp.Name, tmpcomp)
        Me.AddCompToGrid(tmpcomp)
        Dim ms As DWSIM.SimulationObjects.Streams.MaterialStream
        Dim proplist As New ArrayList

        For Each ms In FrmChild.Collections.CLCS_MaterialStreamCollection.Values
            For Each phase As DWSIM.ClassesBasicasTermodinamica.Fase In ms.Fases.Values
                phase.Componentes.Remove(tmpcomp.Name)
            Next

            proplist.Clear()
            For Each pi As DWSIM.Outros.NodeItem In ms.NodeTableItems.Values
                If pi.Checked Then
                    proplist.Add(pi.Text)
                End If
            Next
            ms.FillNodeItems()
            For Each pi As DWSIM.Outros.NodeItem In ms.NodeTableItems.Values
                If proplist.Contains(pi.Text) Then
                    pi.Checked = True
                End If
            Next
        Next
        SetupKeyCompounds()

    End Sub

    Private Sub Button10_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button10.Click
        If MessageBox.Show(DWSIM.App.GetLocalString("ConfirmOperation"), "DWSIM", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = Windows.Forms.DialogResult.Yes Then
            If Me.ListViewA.SelectedItems.Count > 0 Then
                For Each lvi As ListViewItem In Me.ListViewA.SelectedItems
                    Me.RemoveCompFromSimulation(lvi.Tag)
                Next
            End If
        End If
    End Sub

    Private Sub Button11_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button11.Click
        If MessageBox.Show(DWSIM.App.GetLocalString("ConfirmOperation"), "DWSIM", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = Windows.Forms.DialogResult.Yes Then
            For Each lvi As ListViewItem In Me.ListViewA.Items
                Me.RemoveCompFromSimulation(lvi.Tag)
            Next
        End If
    End Sub

    Private Sub ogc1_DataError(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewDataErrorEventArgs) Handles ogc1.DataError

    End Sub

    Private Sub Button8_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button8.Click

        Dim pp As DWSIM.SimulationObjects.PropertyPackages.PropertyPackage
        pp = FormMain.PropertyPackages(Me.DataGridViewPP.SelectedRows(0).Cells(0).Value).Clone

        With pp
            pp.Tag = "PP_" & CStr(Me.dgvpp.Rows.Count + 1)
            pp.UniqueID = "PP-" & Guid.NewGuid.ToString
        End With

        FrmChild.Options.PropertyPackages.Add(pp.UniqueID, pp)
        Me.dgvpp.Rows.Add(New Object() {pp.UniqueID, pp.Tag, pp.ComponentName})

    End Sub

    Private Sub ListViewPP_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles DataGridViewPP.SelectionChanged
        If Me.DataGridViewPP.SelectedRows.Count > 0 Then
            Me.Button8.Enabled = True
        Else
            Me.Button8.Enabled = False
        End If
    End Sub

    Private Sub ComboBox3_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBox3.SelectedIndexChanged
        FrmChild.Options.FractionNumberFormat = Me.ComboBox3.SelectedItem
    End Sub

    Private Sub ComboBoxFlashAlg_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBoxFlashAlg.SelectedIndexChanged
        Me.chkValidateEqCalc.Enabled = True
        Select Case ComboBoxFlashAlg.SelectedIndex
            Case 0
                Me.FrmChild.Options.PropertyPackageFlashAlgorithm = DWSIM.SimulationObjects.PropertyPackages.FlashMethod.DWSIMDefault
                Me.GroupBox11.Enabled = False
            Case 1
                Me.FrmChild.Options.PropertyPackageFlashAlgorithm = DWSIM.SimulationObjects.PropertyPackages.FlashMethod.NestedLoops3PV3
                Me.GroupBox11.Enabled = True
            Case 2
                Me.FrmChild.Options.PropertyPackageFlashAlgorithm = DWSIM.SimulationObjects.PropertyPackages.FlashMethod.InsideOut
                Me.GroupBox11.Enabled = False
            Case 3
                Me.FrmChild.Options.PropertyPackageFlashAlgorithm = DWSIM.SimulationObjects.PropertyPackages.FlashMethod.InsideOut3P
                Me.GroupBox11.Enabled = True
            Case 4
                Me.FrmChild.Options.PropertyPackageFlashAlgorithm = DWSIM.SimulationObjects.PropertyPackages.FlashMethod.GibbsMin2P
                Me.GroupBox11.Enabled = False
            Case 5
                Me.FrmChild.Options.PropertyPackageFlashAlgorithm = DWSIM.SimulationObjects.PropertyPackages.FlashMethod.GibbsMin3P
                Me.GroupBox11.Enabled = True
            Case 6
                Me.FrmChild.Options.PropertyPackageFlashAlgorithm = DWSIM.SimulationObjects.PropertyPackages.FlashMethod.NestedLoopsSLE
                Me.GroupBox11.Enabled = False
                Me.chkValidateEqCalc.Enabled = False
            Case 7
                Me.FrmChild.Options.PropertyPackageFlashAlgorithm = DWSIM.SimulationObjects.PropertyPackages.FlashMethod.NestedLoopsSLE_SS
                Me.GroupBox11.Enabled = False
                Me.chkValidateEqCalc.Enabled = False
            Case 8
                Me.FrmChild.Options.PropertyPackageFlashAlgorithm = DWSIM.SimulationObjects.PropertyPackages.FlashMethod.NestedLoopsImmiscible
                Me.GroupBox11.Enabled = True
        End Select
    End Sub

    Private Sub CheckBox1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBox1.CheckedChanged
        Me.FrmChild.Options.CalculateBubbleAndDewPoints = Me.CheckBox1.Checked
    End Sub

    Private Sub RadioButton1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButton1.CheckedChanged, RadioButton2.CheckedChanged, RadioButton3.CheckedChanged

        If Me.RadioButton1.Checked Then FrmChild.Options.ThreePhaseFlashStabTestSeverity = 0
        If Me.RadioButton2.Checked Then FrmChild.Options.ThreePhaseFlashStabTestSeverity = 1
        If Me.RadioButton3.Checked Then FrmChild.Options.ThreePhaseFlashStabTestSeverity = 2

    End Sub

    Private Sub ListView2_ItemChecked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ItemCheckedEventArgs) Handles ListView2.ItemChecked

        If loaded Then

            Try
                Dim i As Integer = 0
                Dim sel As New ArrayList
                Dim lvi2 As ListViewItem
                For Each lvi2 In Me.ListView2.Items
                    If Not lvi2 Is Nothing Then
                        If lvi2.Checked Then sel.Add(lvi2.Tag)
                    End If
                Next
                FrmChild.Options.ThreePhaseFlashStabTestCompIds = sel.ToArray(Type.GetType("System.String"))
            Catch ex As Exception

            End Try


        End If

    End Sub

    Private Sub TextBox1_KeyDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles TextBox1.KeyDown
        If e.KeyCode = Keys.Enter Then
            Call Button7_Click(sender, e)
            Me.TextBox1.Text = ""
        End If
    End Sub

    Private Sub chkUsePassword_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkUsePassword.CheckedChanged
        If chkUsePassword.Checked Then tbPassword.Enabled = True Else tbPassword.Enabled = False
        FrmChild.Options.UsePassword = chkUsePassword.Checked
    End Sub

    Private Sub tbPassword_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tbPassword.TextChanged
        FrmChild.Options.Password = tbPassword.Text
    End Sub

    Private Sub Button3_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs)

        Dim myStream As System.IO.FileStream
        Dim cp As ConstantProperties
        Dim col As New DWSIM.ClassesBasicasTermodinamica.ConstantPropertiesCollection
        Dim col2 As New ArrayList

        Dim dbid As String = "User_" & Guid.NewGuid().ToString

        For Each r As DataGridViewRow In Me.ogc1.SelectedRows
            cp = FrmChild.Options.NotSelectedComponents(r.Cells(0).Value)
            cp.CurrentDB = dbid
            col2.Add(cp)
        Next

        col.Collection = col2.ToArray(Type.GetType("DWSIM.DWSIM.ClassesBasicasTermodinamica.ConstantProperties"))

        If Me.sfdxml1.ShowDialog() = Windows.Forms.DialogResult.OK Then
            myStream = Me.sfdxml1.OpenFile()
            Dim filename As String = myStream.Name
            myStream.Close()
            Dim x As Serialization.XmlSerializer = New Serialization.XmlSerializer(GetType(DWSIM.ClassesBasicasTermodinamica.ConstantPropertiesCollection))
            Dim writer As IO.TextWriter = New IO.StreamWriter(filename)
            x.Serialize(writer, col)
            writer.Close()

        End If

    End Sub

    Private Sub DataGridView1_DataError(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewDataErrorEventArgs) Handles DataGridView1.DataError

    End Sub

    Private Sub Button3_Click_2(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        Dim frmam As New FormAssayManager
        frmam.ShowDialog(Me)
        frmam.Close()
    End Sub

    Private Sub chkValidateEqCalc_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkValidateEqCalc.CheckedChanged
        Me.FrmChild.Options.ValidateEquilibriumCalc = Me.chkValidateEqCalc.Checked
        Me.tbFlashValidationTolerance.Enabled = Me.chkValidateEqCalc.Checked
    End Sub

    Private Sub tbFlashValidationTolerance_TextChanged(sender As System.Object, e As System.EventArgs) Handles tbFlashValidationTolerance.TextChanged
        Try
            Me.FrmChild.Options.FlashValidationDGETolerancePct = Me.tbFlashValidationTolerance.Text
            Me.tbFlashValidationTolerance.ForeColor = Color.Blue
        Catch ex As Exception
            Me.tbFlashValidationTolerance.ForeColor = Color.Red
        End Try
    End Sub

    Private Sub LinkLabelPropertyMethods_LinkClicked(sender As System.Object, e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles LinkLabelPropertyMethods.LinkClicked
        Process.Start("http://dwsim.inforside.com.br/wiki/index.php?title=Property_Methods_and_Correlation_Profiles")
    End Sub

    Private Sub ogc1_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles ogc1.CellDoubleClick
        If e.RowIndex > -1 Then
            AddCompToSimulation(e.RowIndex)
        End If
    End Sub

    Private Sub ListViewPP_DoubleClick(sender As Object, e As EventArgs) Handles DataGridViewPP.DoubleClick
        If Me.DataGridViewPP.SelectedRows.Count = 1 Then
            Button8.PerformClick()
        End If
    End Sub

    Private Sub chkDoPhaseId_CheckedChanged(sender As System.Object, e As System.EventArgs) Handles chkDoPhaseId.CheckedChanged
        Me.FrmChild.Options.UsePhaseIdentificationAlgorithm = Me.chkDoPhaseId.Checked
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Dim comps As New List(Of ConstantProperties)
        If Me.sfdxml1.ShowDialog() = Windows.Forms.DialogResult.OK Then
            If Not File.Exists(sfdxml1.FileName) Then File.Create(sfdxml1.FileName).Close()
            For Each lvi As ListViewItem In Me.ListViewA.SelectedItems
                comps.Add(FrmChild.Options.SelectedComponents(lvi.Tag))
            Next
            DWSIM.Databases.UserDB.AddCompounds(comps.ToArray, sfdxml1.FileName, True)
        End If
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click

        If Me.loaded Then

            If MessageBox.Show(DWSIM.App.GetLocalString("ConfirmOperation"), "DWSIM", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = Windows.Forms.DialogResult.Yes Then

                Dim tmpsubst As New DWSIM.ClassesBasicasTermodinamica.Substancia("", "")
                Dim toreplace As DWSIM.ClassesBasicasTermodinamica.ConstantProperties = Nothing

                If DWSIM.App.IsRunningOnMono Then
                    If Me.ogc1.SelectedCells.Count > 0 Then
                        toreplace = FrmChild.Options.NotSelectedComponents(ogc1.Rows(ogc1.SelectedCells(0).RowIndex).Cells(0).Value)
                    End If
                Else
                    If Me.ogc1.SelectedRows.Count > 0 Then
                        toreplace = FrmChild.Options.NotSelectedComponents(ogc1.Rows(ogc1.SelectedRows(0).Index).Cells(0).Value)
                    End If
                End If

                Dim proplist As New ArrayList
                For Each mstr In FrmChild.Collections.CLCS_MaterialStreamCollection.Values
                    For Each phase As DWSIM.ClassesBasicasTermodinamica.Fase In mstr.Fases.Values
                        tmpsubst = phase.Componentes(Me.ListViewA.SelectedItems(0).Tag)
                        phase.Componentes.Remove(Me.ListViewA.SelectedItems(0).Tag)
                        tmpsubst.ConstantProperties = toreplace
                        tmpsubst.Nome = toreplace.Name
                        phase.Componentes.Add(tmpsubst.Nome, tmpsubst)
                    Next
                    proplist.Clear()
                    For Each pi As DWSIM.Outros.NodeItem In mstr.NodeTableItems.Values
                        If pi.Checked Then
                            proplist.Add(pi.Text)
                        End If
                    Next
                    mstr.FillNodeItems()
                    For Each pi As DWSIM.Outros.NodeItem In mstr.NodeTableItems.Values
                        If proplist.Contains(pi.Text) Then
                            pi.Checked = True
                        End If
                    Next
                Next
                If Not DWSIM.App.IsRunningOnMono Then Me.ogc1.Rows.RemoveAt(Me.ogc1.SelectedRows(0).Index)
                Me.FrmChild.Options.NotSelectedComponents.Add(Me.ListViewA.SelectedItems(0).Tag, Me.FrmChild.Options.SelectedComponents(Me.ListViewA.SelectedItems(0).Tag))
                Me.FrmChild.Options.SelectedComponents.Remove(Me.ListViewA.SelectedItems(0).Tag)
                Me.FrmChild.Options.SelectedComponents.Add(toreplace.Name, toreplace)
                Me.ListViewA.SelectedItems(0).Tag = toreplace.Name
                Me.ListViewA.SelectedItems(0).Text = DWSIM.App.GetLocalString(toreplace.Name)
                SetupKeyCompounds()
            End If
        End If

    End Sub

    Private Sub Button12_Click(sender As Object, e As EventArgs) Handles Button12.Click
        TextBox1.Text = ""
    End Sub

    Private Sub FloatToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FloatToolStripMenuItem.Click, DocumentToolStripMenuItem.Click,
                                                               DockLeftToolStripMenuItem.Click, DockLeftAutoHideToolStripMenuItem.Click,
                                                               DockRightAutoHideToolStripMenuItem.Click, DockRightToolStripMenuItem.Click,
                                                               DockTopAutoHideToolStripMenuItem.Click, DockTopToolStripMenuItem.Click,
                                                               DockBottomAutoHideToolStripMenuItem.Click, DockBottomToolStripMenuItem.Click

        For Each ts As ToolStripMenuItem In dckMenu.Items
            ts.Checked = False
        Next

        sender.Checked = True

        Select Case sender.Name
            Case "FloatToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.Float
            Case "DocumentToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.Document
            Case "DockLeftToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockLeft
            Case "DockLeftAutoHideToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockLeftAutoHide
            Case "DockRightAutoHideToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockRightAutoHide
            Case "DockRightToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockRight
            Case "DockBottomAutoHideToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockBottomAutoHide
            Case "DockBottomToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockBottom
            Case "DockTopAutoHideToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockTopAutoHide
            Case "DockTopToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockTop
            Case "HiddenToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.Hidden
        End Select

    End Sub


End Class
