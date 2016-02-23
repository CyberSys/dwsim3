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

Imports System.Runtime.Serialization.Formatters.Binary
Imports DWSIM.DWSIM.FormClasses


Public Class FormUnitGen

    Inherits System.Windows.Forms.Form

    Dim frm As FormFlowsheet

    Private Sub FormUnitGen_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        frm = My.Application.ActiveSimulation

        Dim currentset As DWSIM.SistemasDeUnidades.Unidades = frm.Options.SelectedUnitSystem

        Dim rd As New Random
        Me.TextBox1.Text = "UNID_" & rd.Next(1000, 9999)

        Dim cb As DataGridViewComboBoxCell
        With Me.DataGridView1.Rows
            .Clear()
            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {"K", "R", "C", "F"})
            cb.Value = currentset.spmp_temperature
            .Add(New Object() {DWSIM.App.GetLocalString("Temperatura")})
            .Item(.Count - 1).Cells(1) = cb
            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New Object() {"Pa", "atm", "kgf/cm2", "kgf/cm2g", "lbf/ft2", "kPa", "kPag", "bar", "barg", "ftH2O", "inH2O", "inHg", "mbar", "mH2O", "mmH2O", "mmHg", "MPa", "psi", "psig"})
            cb.Value = currentset.spmp_pressure
            .Add(New Object() {DWSIM.App.GetLocalString("Presso")})
            .Item(.Count - 1).Cells(1) = cb
            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New Object() {"g/s", "lbm/h", "kg/s", "kg/h", "kg/d", "kg/min", "lb/min", "lb/s"})
            cb.Value = currentset.spmp_massflow
            .Add(New Object() {DWSIM.App.GetLocalString("Vazomssica")})
            .Item(.Count - 1).Cells(1) = cb
            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New Object() {"mol/s", "lbmol/h", "mol/h", "mol/d", "kmol/s", "kmol/h", "kmol/d", "m3/d @ BR", "m3/d @ NC", "m3/d @ CNTP", "m3/d @ SC"})
            cb.Value = currentset.spmp_molarflow
            .Add(New Object() {DWSIM.App.GetLocalString("Vazomolar")})
            .Item(.Count - 1).Cells(1) = cb
            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New Object() {"m3/s", "ft3/s", "cm3/s", "m3/h", "m3/d", "bbl/h", "bbl/d", "ft3/min", "gal[UK]/h", "gal[UK]/s", "gal[US]/h", "gal[US]/min", "L/h", "L/min", "L/s"})
            cb.Value = currentset.spmp_volumetricFlow
            .Add(New Object() {DWSIM.App.GetLocalString("Vazovolumtrica")})
            .Item(.Count - 1).Cells(1) = cb
            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New Object() {"kJ/kg", "cal/g", "BTU/lbm", "kcal/kg"})
            cb.Value = currentset.spmp_enthalpy
            .Add(New Object() {DWSIM.App.GetLocalString("EntalpiaEspecfica")})
            .Item(.Count - 1).Cells(1) = cb
            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New Object() {"kJ/[kg.K]", "cal/[g.C]", "BTU/[lbm.R]"})
            cb.Value = currentset.spmp_entropy
            .Add(New Object() {DWSIM.App.GetLocalString("EntropiaEspecfica")})
            .Item(.Count - 1).Cells(1) = cb
            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New Object() {"kg/kmol", "g/mol", "lbm/lbmol"})
            cb.Value = currentset.spmp_molecularWeight
            .Add(New Object() {DWSIM.App.GetLocalString("Massamolar")})
            .Item(.Count - 1).Cells(1) = cb
            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New Object() {"N/m", "dyn/cm", "lbf/in"})
            cb.Value = currentset.tdp_surfaceTension
            .Add(New Object() {DWSIM.App.GetLocalString("Tensosuperficial")})
            .Item(.Count - 1).Cells(1) = cb
            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New Object() {"kg/m3", "g/cm3", "lbm/ft3"})
            cb.Value = currentset.spmp_density
            .Add(New Object() {DWSIM.App.GetLocalString("Massaespecfica")})
            .Item(.Count - 1).Cells(1) = cb
            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New Object() {"kJ/[kg.K]", "cal/[g.C]", "BTU/[lbm.R]"})
            cb.Value = currentset.spmp_heatCapacityCp
            .Add(New Object() {DWSIM.App.GetLocalString("CapacidadeCalorfica")})
            .Item(.Count - 1).Cells(1) = cb
            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New Object() {"W/[m.K]", "cal/[cm.s.C]", "BTU/[ft.h.R]"})
            cb.Value = currentset.spmp_thermalConductivity
            .Add(New Object() {DWSIM.App.GetLocalString("Condutividadetrmica")})
            .Item(.Count - 1).Cells(1) = cb
            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New Object() {"m2/s", "cSt", "ft2/s", "mm2/s"})
            cb.Value = currentset.spmp_cinematic_viscosity
            .Add(New Object() {DWSIM.App.GetLocalString("Viscosidadecinemtica")})
            .Item(.Count - 1).Cells(1) = cb
            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New Object() {"kg/[m.s]", "Pa.s", "cP", "lbm/[ft.h]"})
            cb.Value = currentset.spmp_viscosity
            .Add(New Object() {DWSIM.App.GetLocalString("ViscosidadeDinmica1")})
            .Item(.Count - 1).Cells(1) = cb
            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New Object() {"Pa", "atm", "lbf/ft2", "kgf/cm2", "kPa", "bar", "ftH2O", "inH2O", "inHg", "mbar", "mH2O", "mmH2O", "mmHg", "MPa", "psi"})
            cb.Value = currentset.spmp_deltaP
            .Add(New Object() {DWSIM.App.GetLocalString("DeltaP")})
            .Item(.Count - 1).Cells(1) = cb
            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New Object() {"C.", "K.", "F.", "R."})
            cb.Value = currentset.spmp_deltaT
            .Add(New Object() {DWSIM.App.GetLocalString("DeltaT2")})
            .Item(.Count - 1).Cells(1) = cb
            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New Object() {"m", "ft", "cm"})
            cb.Value = currentset.spmp_head
            .Add(New Object() {DWSIM.App.GetLocalString("ComprimentoHead")})
            .Item(.Count - 1).Cells(1) = cb
            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New Object() {"kW", "kcal/h", "BTU/h", "BTU/s", "cal/s", "HP", "kJ/h", "kJ/d", "MW", "W"})
            cb.Value = currentset.spmp_heatflow
            .Add(New Object() {DWSIM.App.GetLocalString("FluxodeEnergia")})
            .Item(.Count - 1).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {"s", "min.", "h"})
            cb.Value = currentset.time
            .Add(New Object() {DWSIM.App.GetLocalString("Tempo")})
            .Item(.Count - 1).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {"m3", "cm3", "L", "ft3"})
            cb.Value = currentset.volume
            .Add(New Object() {DWSIM.App.GetLocalString("Volume")})
            .Item(.Count - 1).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {DWSIM.App.GetLocalString("m3kmol"), "cm3/mmol", "ft3/lbmol"})
            cb.Value = currentset.molar_volume
            .Add(New Object() {DWSIM.App.GetLocalString("VolumeMolar")})
            .Item(.Count - 1).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {"m2", "cm2", "ft2"})
            cb.Value = currentset.area
            .Add(New Object() {DWSIM.App.GetLocalString("rea")})
            .Item(.Count - 1).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {"m", "mm", "cm", "in", "ft"})
            cb.Value = currentset.diameter
            .Add(New Object() {DWSIM.App.GetLocalString("DimetroEspessura")})
            .Item(.Count - 1).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {DWSIM.App.GetLocalString("N"), "dyn", "kgf", "lbf"})
            cb.Value = currentset.force
            .Add(New Object() {DWSIM.App.GetLocalString("Fora")})
            .Item(.Count - 1).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {"W/[m2.K]", "cal/[cm2.s.C]", "BTU/[ft2.h.R]"})
            cb.Value = currentset.heat_transf_coeff
            .Add(New Object() {DWSIM.App.GetLocalString("CoefdeTransfdeCalor")})
            .Item(.Count - 1).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {"m/s2", "cm/s2", "ft/s2"})
            cb.Value = currentset.accel
            .Add(New Object() {DWSIM.App.GetLocalString("Aceleracao")})
            .Item(.Count - 1).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {"m3/kg", "cm3/g", "ft3/lbm"})
            cb.Value = currentset.spec_vol
            .Add(New Object() {DWSIM.App.GetLocalString("VolumeEspecfico")})
            .Item(.Count - 1).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {"kmol/m3", "mol/m3", "mol/L", "mol/cm3", "mol/mL", "lbmol/ft3"})
            cb.Value = currentset.molar_conc
            .Add(New Object() {DWSIM.App.GetLocalString("ConcentraoMolar")})
            .Item(.Count - 1).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {"kg/m3", "g/L", "g/cm3", "g/mL", "lbm/ft3"})
            cb.Value = currentset.mass_conc
            .Add(New Object() {DWSIM.App.GetLocalString("ConcentraoMssica")})
            .Item(.Count - 1).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {"kmol/[m3.s]", "kmol/[m3.min.]", "kmol/[m3.h]", "mol/[m3.s]", "mol/[m3.min.]", "mol/[m3.h]", "mol/[L.s]", "mol/[L.min.]", "mol/[L.h]", "mol/[cm3.s]", "mol/[cm3.min.]", "mol/[cm3.h]", "lbmol.[ft3.h]"})
            cb.Value = currentset.reac_rate
            .Add(New Object() {DWSIM.App.GetLocalString("TaxadeReao")})
            .Item(.Count - 1).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {"kJ/kmol", "cal/mol", "BTU/lbmol"})
            cb.Value = currentset.molar_enthalpy
            .Add(New Object() {DWSIM.App.GetLocalString("MolarEnthalpy")})
            .Item(.Count - 1).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {"kJ/[kmol.K]", "cal/[mol.C]", "BTU/[lbmol.R]"})
            cb.Value = currentset.molar_entropy
            .Add(New Object() {DWSIM.App.GetLocalString("MolarEntropy")})
            .Item(.Count - 1).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {"m/s", "cm/s", "mm/s", "km/h", "ft/h", "ft/min", "ft/s", "in/s"})
            cb.Value = currentset.velocity
            .Add(New Object() {DWSIM.App.GetLocalString("Velocity")})
            .Item(.Count - 1).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {"K.m2/W", "C.cm2.s/cal", "ft2.h.F/BTU"})
            cb.Value = currentset.foulingfactor
            .Add(New Object() {DWSIM.App.GetLocalString("HXFoulingFactor")})
            .Item(.Count - 1).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {"m/kg", "ft/lbm", "cm/g"})
            cb.Value = currentset.cakeresistance
            .Add(New Object() {DWSIM.App.GetLocalString("FilterSpecificCakeResistance")})
            .Item(.Count - 1).Cells(1) = cb

            cb = New DataGridViewComboBoxCell
            cb.Items.AddRange(New String() {"m-1", "cm-1", "ft-1"})
            cb.Value = currentset.mediumresistance
            .Add(New Object() {DWSIM.App.GetLocalString("FilterMediumResistance")})
            .Item(.Count - 1).Cells(1) = cb

        End With
    End Sub

    Private Sub KryptonButton1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles KryptonButton1.Click
        Me.Close()
    End Sub

    Private Sub KryptonButton2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles KryptonButton2.Click

        If Me.TextBox1.Text <> "" Then

            Dim su As New DWSIM.SistemasDeUnidades.Unidades
            With su
                .nome = Me.TextBox1.Text
                .pdp_boilingPointTemperature = Me.DataGridView1.Rows(0).Cells(1).Value
                .pdp_meltingTemperature = Me.DataGridView1.Rows(0).Cells(1).Value
                .spmp_activity = Me.DataGridView1.Rows(1).Cells(1).Value
                .spmp_activityCoefficient = "-"
                .spmp_compressibility = "-"
                .spmp_compressibilityFactor = "-"
                .spmp_density = Me.DataGridView1.Rows(9).Cells(1).Value
                .spmp_enthalpy = Me.DataGridView1.Rows(5).Cells(1).Value
                .spmp_entropy = Me.DataGridView1.Rows(6).Cells(1).Value
                .spmp_excessEnthalpy = Me.DataGridView1.Rows(5).Cells(1).Value
                .spmp_excessEntropy = Me.DataGridView1.Rows(6).Cells(1).Value
                .spmp_fugacity = Me.DataGridView1.Rows(1).Cells(1).Value
                .spmp_fugacityCoefficient = "-"
                .spmp_heatCapacityCp = Me.DataGridView1.Rows(10).Cells(1).Value
                .spmp_heatCapacityCv = Me.DataGridView1.Rows(10).Cells(1).Value
                .spmp_jouleThomsonCoefficient = "-"
                .spmp_logFugacityCoefficient = "-"
                .spmp_massflow = Me.DataGridView1.Rows(2).Cells(1).Value
                .spmp_massfraction = "-"
                .spmp_molarflow = Me.DataGridView1.Rows(3).Cells(1).Value
                .spmp_molarfraction = "-"
                .spmp_molecularWeight = Me.DataGridView1.Rows(7).Cells(1).Value
                .spmp_pressure = Me.DataGridView1.Rows(1).Cells(1).Value
                .spmp_speedOfSound = "-"
                .spmp_temperature = Me.DataGridView1.Rows(0).Cells(1).Value
                .spmp_thermalConductivity = Me.DataGridView1.Rows(11).Cells(1).Value
                .spmp_viscosity = Me.DataGridView1.Rows(13).Cells(1).Value
                .spmp_volumetricFlow = Me.DataGridView1.Rows(4).Cells(1).Value
                .spmp_cinematic_viscosity = Me.DataGridView1.Rows(12).Cells(1).Value
                .tdp_idealGasHeatCapacity = Me.DataGridView1.Rows(10).Cells(1).Value
                .tdp_surfaceTension = Me.DataGridView1.Rows(8).Cells(1).Value
                .tdp_thermalConductivityOfLiquid = Me.DataGridView1.Rows(11).Cells(1).Value
                .tdp_thermalConductivityOfVapor = Me.DataGridView1.Rows(11).Cells(1).Value
                .tdp_vaporPressure = Me.DataGridView1.Rows(1).Cells(1).Value
                .tdp_viscosityOfLiquid = Me.DataGridView1.Rows(13).Cells(1).Value
                .tdp_viscosityOfVapor = Me.DataGridView1.Rows(13).Cells(1).Value
                .tpmp_kvalue = "-"
                .tpmp_logKvalue = "-"
                .tpmp_surfaceTension = Me.DataGridView1.Rows(8).Cells(1).Value
                .spmp_heatflow = Me.DataGridView1.Rows(17).Cells(1).Value
                .spmp_head = Me.DataGridView1.Rows(16).Cells(1).Value
                .spmp_deltaP = Me.DataGridView1.Rows(14).Cells(1).Value
                .spmp_deltaT = Me.DataGridView1.Rows(15).Cells(1).Value
                .distance = Me.DataGridView1.Rows(16).Cells(1).Value
                .time = Me.DataGridView1.Rows(18).Cells(1).Value
                .volume = Me.DataGridView1.Rows(19).Cells(1).Value
                .molar_volume = Me.DataGridView1.Rows(20).Cells(1).Value
                .area = Me.DataGridView1.Rows(21).Cells(1).Value
                .diameter = Me.DataGridView1.Rows(22).Cells(1).Value
                .thickness = Me.DataGridView1.Rows(22).Cells(1).Value
                .force = Me.DataGridView1.Rows(23).Cells(1).Value
                .heat_transf_coeff = Me.DataGridView1.Rows(24).Cells(1).Value
                .accel = Me.DataGridView1.Rows(25).Cells(1).Value
                .spec_vol = Me.DataGridView1.Rows(26).Cells(1).Value
                .molar_conc = Me.DataGridView1.Rows(27).Cells(1).Value
                .mass_conc = Me.DataGridView1.Rows(28).Cells(1).Value
                .reac_rate = Me.DataGridView1.Rows(29).Cells(1).Value
                .molar_enthalpy = Me.DataGridView1.Rows(30).Cells(1).Value
                .molar_entropy = Me.DataGridView1.Rows(31).Cells(1).Value
                .velocity = Me.DataGridView1.Rows(32).Cells(1).Value
                .foulingfactor = Me.DataGridView1.Rows(33).Cells(1).Value
            End With

            If FormMain.AvailableUnitSystems.ContainsKey(su.nome) Then

                MessageBox.Show(DWSIM.App.GetLocalString("JexisteumSistemadeUn") & vbCrLf & DWSIM.App.GetLocalString("Porfavormodifiqueoet"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)

            Else

                frm.AddUnitSystem(su)

                frm.AddUndoRedoAction(New UndoRedoAction() With {.AType = UndoRedoActionType.SystemOfUnitsAdded,
                                         .ID = New Random().Next(),
                                         .NewValue = su,
                                         .Name = String.Format(DWSIM.App.GetLocalString("UndoRedo_SystemOfUnitsAdded"), su.nome)})

                Me.Close()

            End If
        Else

            MessageBox.Show(DWSIM.App.GetLocalString("DefinaumnomeparaoSis"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Information)

        End If

    End Sub

    Private Sub DataGridView1_DataError(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewDataErrorEventArgs) Handles DataGridView1.DataError

    End Sub


End Class