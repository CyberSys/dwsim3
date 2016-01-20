﻿'    Sour Water Property Package 
'    Copyright 2016 Daniel Wagner O. de Medeiros
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
'
'
'
'    Based on the SWEQ model described in the USEPA Report EPA-600/2-80-067: 
'    'A new correlation of NH3, CO2, and H2S volatility data from aqueous sour 
'    water systems', by Wilson, Grant M.  
'    Available online at http://nepis.epa.gov/Exe/ZyPDF.cgi?Dockey=9101B309.PDF

Imports DWSIM.DWSIM.SimulationObjects.PropertyPackages
Imports DWSIM.DWSIM.SimulationObjects.PropertyPackages.Auxiliary
Imports DWSIM.DWSIM.MathEx
Imports System.Linq
Imports System.Math
Imports DWSIM.DWSIM.ClassesBasicasTermodinamica

Namespace DWSIM.SimulationObjects.PropertyPackages

    <System.Runtime.InteropServices.Guid(SteamTablesPropertyPackage.ClassId)> _
<System.Serializable()> Public Class SourWaterPropertyPackage

        Inherits ActivityCoefficientPropertyPackage

        Public Shadows Const ClassId As String = "79aefcf3-6c17-4e44-91d4-b70b7642bb78"

        Public Sub New(ByVal comode As Boolean)

            MyBase.New(comode)

            Me.m_act = New Auxiliary.NRTL

        End Sub

        Public Sub New()

            MyBase.New(False)

            Me.m_act = New Auxiliary.NRTL

            Me.IsConfigurable = True
            Me.ConfigForm = New FormConfigNRTL
            Me._packagetype = PropertyPackages.PackageType.ActivityCoefficient

        End Sub

        Public Overrides Sub ReconfigureConfigForm()

            MyBase.ReconfigureConfigForm()
            Me.ConfigForm = New FormConfigNRTL

        End Sub

        Public Overrides ReadOnly Property FlashBase() As Auxiliary.FlashAlgorithms.FlashAlgorithm
            Get
                Dim constprops As New List(Of ConstantProperties)
                For Each su As Substancia In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                    constprops.Add(su.ConstantProperties)
                Next
                Return New Auxiliary.FlashAlgorithms.SourWater With {.CompoundProperties = constprops}
            End Get
        End Property

        Public Overrides Function DW_CalcKvalue(Vx As Array, Vy As Array, T As Double, P As Double, Optional type As String = "LV") As Double()

            Dim val0 As Double() = MyBase.DW_CalcKvalue(Vx, Vy, T, P, type)

            Dim cprops = Me.DW_GetConstantProperties
            Dim conc As New Dictionary(Of String, Double)
            Dim totalkg As Double = AUX_MMM(Vx) / 1000 'kg solution
            Dim CAS, CC, CS As Double

            Setup(conc, Vx, cprops)

            CAS = conc("NH3")
            CC = conc("CO2") + conc("HCO3-") + conc("CO3-2") + conc("H2NCOO-")
            CS = conc("H2S") + conc("HS-") + conc("S-2")

            Dim i As Integer = 0
            For Each cp In cprops
                If cp.IsIon Then val0(i) = 1.0E-60
                If cp.Name = "Sodium Hydroxide" Then val0(i) = 1.0E-60
                If cp.Name = "Ammonia" Then
                    val0(i) = Exp(178.339 - 15517.91 / (T * 1.8) - 25.6767 * Log(T * 1.8) + 0.01966 * (T * 1.8) + (131.4 / (T * 1.8) - 0.1682) * CAS) 'psia/[mol/kg]
                    val0(i) = (val0(i) * conc("NH3") / 0.000145038) / P / Vx(i)
                End If
                If cp.Name = "Carbon dioxide" Then
                    val0(i) = Exp(18.33 - 24895.1 / (T * 1.8) + 22399600.0 / (T * 1.8) ^ 2 - 9091800000.0 / (T * 1.8) ^ 3 + 1260100000000.0 / (T * 1.8) ^ 4) 'psia/[mol/kg]
                    val0(i) = (val0(i) * conc("CO2") / 0.000145038) / P / Vx(i)
                End If
                If cp.Name = "Hydrogen sulfide" Then
                    val0(i) = Exp(100.684 - 246254 / (T * 1.8) + 239029000.0 / (T * 1.8) ^ 2 - 101898000000.0 / (T * 1.8) ^ 3 + 15973400000000.0 / (T * 1.8) ^ 4 - 0.05 * CAS + (0.965 - 486 / (T * 1.8)) * CC) 'psia/[mol/kg]
                    val0(i) = (val0(i) * conc("H2S") / 0.000145038) / P / Vx(i)
                End If
                i += 1
            Next

            Return val0

        End Function

        Public Overrides Function AUX_PVAPi(index As Integer, T As Double) As Double

            Dim cprops = Me.DW_GetConstantProperties

            If cprops(index).IsIon Then
                Return 1.0E-20
            ElseIf cprops(index).Name = "Sodium Hydroxide" Then
                Return 1.0E-20
            Else
                Return MyBase.AUX_PVAPi(index, T)
            End If

        End Function

        Sub Setup(conc As Dictionary(Of String, Double), Vx As Double(), CompoundProperties As List(Of ConstantProperties))

            Dim wid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.CAS_Number = "7732-18-5").FirstOrDefault)
            Dim co2id As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Name = "Carbon dioxide").FirstOrDefault)
            Dim nh3id As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Name = "Ammonia").FirstOrDefault)
            Dim h2sid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Name = "Hydrogen sulfide").FirstOrDefault)
            Dim naohid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Formula = "NaOH").FirstOrDefault)
            Dim naid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Formula = "Na+").FirstOrDefault)
            Dim ohid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Formula = "OH-").FirstOrDefault)
            Dim hid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Formula = "H+").FirstOrDefault)
            Dim nh4id As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Formula = "NH4+").FirstOrDefault)
            Dim hcoid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Formula = "HCO3-").FirstOrDefault)
            Dim co3id As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Formula = "CO3-2").FirstOrDefault)
            Dim h2nid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Formula = "H2NCOO-").FirstOrDefault)
            Dim hsid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Formula = "HS-").FirstOrDefault)
            Dim s2id As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Formula = "S-2").FirstOrDefault)

            'calculate solution amounts

            Dim totalkg As Double = AUX_MMM(Vx) / 1000 'kg solution

            conc.Clear()

            If wid > -1 Then conc.Add("H2O", Vx(wid) / totalkg) Else conc.Add("H2O", 0.0#)
            If hid > -1 Then conc.Add("H+", Vx(hid) / totalkg) Else conc.Add("H+", 0.0#)
            If ohid > -1 Then conc.Add("OH-", Vx(ohid) / totalkg) Else conc.Add("OH-", 0.0#)
            If nh3id > -1 Then conc.Add("NH3", Vx(nh3id) / totalkg) Else conc.Add("NH3", 0.0#)
            If nh4id > -1 Then conc.Add("NH4+", Vx(nh4id) / totalkg) Else conc.Add("NH4+", 0.0#)
            If co2id > -1 Then conc.Add("CO2", Vx(co2id) / totalkg) Else conc.Add("CO2", 0.0#)
            If hcoid > -1 Then conc.Add("HCO3-", Vx(hcoid) / totalkg) Else conc.Add("HCO3-", 0.0#)
            If co3id > -1 Then conc.Add("CO3-2", Vx(co3id) / totalkg) Else conc.Add("CO3-2", 0.0#)
            If h2nid > -1 Then conc.Add("H2NCOO-", Vx(h2nid) / totalkg) Else conc.Add("H2NCOO-", 0.0#)
            If h2sid > -1 Then conc.Add("H2S", Vx(h2sid) / totalkg) Else conc.Add("H2S", 0.0#)
            If hsid > -1 Then conc.Add("HS-", Vx(hsid) / totalkg) Else conc.Add("HS-", 0.0#)
            If s2id > -1 Then conc.Add("S-2", Vx(s2id) / totalkg) Else conc.Add("S-2", 0.0#)
            If naohid > -1 Then conc.Add("NaOH", Vx(naohid) / totalkg) Else conc.Add("NaOH", 0.0#)
            If naid > -1 Then conc.Add("Na+", Vx(naid) / totalkg) Else conc.Add("Na+", 0.0#)

        End Sub


    End Class

End Namespace