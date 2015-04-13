'    LIQUAC2 Property Package 
'    Copyright 2013 Daniel Wagner O. de Medeiros
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

Imports DWSIM.DWSIM.SimulationObjects.PropertyPackages
Imports System.Math
Imports System.Xml.Linq
Imports System.Linq
Imports DWSIM.DWSIM.ClassesBasicasTermodinamica
Imports DWSIM.DWSIM.MathEx
Imports DWSIM.DWSIM.MathEx.Common
Imports Ciloci.Flee

Namespace DWSIM.SimulationObjects.PropertyPackages

    <System.Runtime.InteropServices.Guid(LIQUAC2PropertyPackage.ClassId)> _
<System.Serializable()> Public Class LIQUAC2PropertyPackage

        Inherits DWSIM.SimulationObjects.PropertyPackages.ElectrolyteBasePropertyPackage

        Public Shadows Const ClassId As String = "f3eeff51-eccc-4c15-b4b0-1eb4d13c61f3"

        Private m_props As New DWSIM.SimulationObjects.PropertyPackages.Auxiliary.PROPS
        Public m_uni As New DWSIM.SimulationObjects.PropertyPackages.Auxiliary.LIQUAC2
        Private m_id As New DWSIM.SimulationObjects.PropertyPackages.Auxiliary.Ideal

        Public Sub New(ByVal comode As Boolean)

            MyBase.New(comode)

            Me.ConfigForm = New FormConfigLIQUAC

        End Sub

        Public Sub New()

            MyBase.New()

            Me.ConfigForm = New FormConfigLIQUAC

        End Sub

        Public Overrides Sub ReconfigureConfigForm()
            MyBase.ReconfigureConfigForm()
            Me.ConfigForm = New FormConfigLIQUAC
        End Sub

#Region "    DWSIM Functions"

        Public Overrides Sub DW_CalcProp(ByVal [property] As String, ByVal phase As Fase)

            Dim result As Double = 0.0#
            Dim resultObj As Object = Nothing
            Dim phaseID As Integer = -1
            Dim state As String = ""

            Dim T, P As Double
            T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault
            P = Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure.GetValueOrDefault

            Select Case phase
                Case Fase.Vapor
                    state = "V"
                Case Fase.Liquid, Fase.Liquid1, Fase.Liquid2, Fase.Liquid3
                    state = "L"
                Case Fase.Solid
                    state = "S"
            End Select

            Select Case phase
                Case PropertyPackages.Fase.Mixture
                    phaseID = 0
                Case PropertyPackages.Fase.Vapor
                    phaseID = 2
                Case PropertyPackages.Fase.Liquid1
                    phaseID = 3
                Case PropertyPackages.Fase.Liquid2
                    phaseID = 4
                Case PropertyPackages.Fase.Liquid3
                    phaseID = 5
                Case PropertyPackages.Fase.Liquid
                    phaseID = 1
                Case PropertyPackages.Fase.Aqueous
                    phaseID = 6
                Case PropertyPackages.Fase.Solid
                    phaseID = 7
            End Select

            Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight = Me.AUX_MMM(phase)

            Select Case [property].ToLower
                Case "compressibilityfactor"
                    result = 0.0#
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.compressibilityFactor = result
                Case "heatcapacity", "heatcapacitycp"
                    Dim constprops As New List(Of ConstantProperties)
                    For Each su As Substancia In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                        constprops.Add(su.ConstantProperties)
                    Next
                    If phase = Fase.Solid Then
                        result = Me.AUX_SOLIDCP(RET_VMAS(phase), constprops, T)
                        Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCp = result
                    ElseIf phase = Fase.Vapor Then
                        resultObj = m_id.CpCv(state, T, P, RET_VMOL(phase), RET_VKij(), RET_VMAS(phase), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())
                        Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCp = resultObj(1)
                    Else
                        Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCp = Me.m_elec.HeatCapacityCp(T, RET_VMOL(phase), constprops)
                    End If
                Case "heatcapacitycv"
                    Dim constprops As New List(Of ConstantProperties)
                    For Each su As Substancia In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                        constprops.Add(su.ConstantProperties)
                    Next
                    If phase = Fase.Solid Then
                        result = Me.AUX_SOLIDCP(RET_VMAS(phase), constprops, T)
                        Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCv = result
                    Else
                        resultObj = m_id.CpCv(state, T, P, RET_VMOL(phase), RET_VKij(), RET_VMAS(phase), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())
                        Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCv = Me.m_elec.HeatCapacityCp(T, RET_VMOL(phase), constprops)
                    End If
                Case "enthalpy", "enthalpynf"
                    Dim constprops As New List(Of ConstantProperties)
                    For Each su As Substancia In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                        constprops.Add(su.ConstantProperties)
                    Next
                    If phase = Fase.Solid Then
                        result = Me.m_elec.SolidEnthalpy(T, RET_VMOL(phase), constprops)
                        Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy = result
                        result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                        Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_enthalpy = result
                    ElseIf phase = Fase.Vapor Then
                        result = Me.m_elec.LiquidEnthalpy(T, RET_VMOL(phase), constprops, Me.m_uni.GAMMA_MR(T + 0.1, RET_VMOL(phase), constprops), Me.m_uni.GAMMA_MR(T, RET_VMOL(phase), constprops), False)
                        result += Me.RET_HVAPM(RET_VMAS(phase), T)
                        Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy = result
                        result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                        Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_enthalpy = result
                    Else
                        result = Me.m_elec.LiquidEnthalpy(T, RET_VMOL(phase), constprops, Me.m_uni.GAMMA_MR(T + 0.1, RET_VMOL(phase), constprops), Me.m_uni.GAMMA_MR(T, RET_VMOL(phase), constprops), False)
                        Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy = result
                    End If
                Case "entropy", "entropynf"
                    Dim constprops As New List(Of ConstantProperties)
                    For Each su As Substancia In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                        constprops.Add(su.ConstantProperties)
                    Next
                    If phase = Fase.Solid Then
                        result = 0.0#
                        Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy = result
                        result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                        Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_entropy = result
                    ElseIf phase = Fase.Vapor Then
                        result = m_id.S_RA_MIX(state, T, P, RET_VMOL(phase), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Sid(298.15, T, P, phase), Me.RET_VHVAP(T), Me.RET_Hid(298.15, T, phase))
                        Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy = result
                        result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                        Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_entropy = result
                    Else
                        Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_entropy = 0.0#
                    End If
                Case "excessenthalpy"
                    result = m_id.H_RA_MIX(state, T, P, RET_VMOL(phase), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), 0, Me.RET_VHVAP(T))
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.excessEnthalpy = result
                Case "excessentropy"
                    result = m_id.S_RA_MIX(state, T, P, RET_VMOL(phase), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), 0, Me.RET_VHVAP(T), Me.RET_Hid(298.15, T, phase))
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.excessEntropy = result
                Case "enthalpyf"
                    Dim entF As Double = Me.AUX_HFm25(phase)
                    result = m_id.H_RA_MIX(state, T, P, RET_VMOL(phase), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Hid(298.15, T, phase), Me.RET_VHVAP(T))
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpyF = result + entF
                    result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpyF.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_enthalpyF = result
                Case "entropyf"
                    Dim entF As Double = Me.AUX_SFm25(phase)
                    result = m_id.S_RA_MIX(state, T, P, RET_VMOL(phase), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Sid(298.15, T, P, phase), Me.RET_VHVAP(T), Me.RET_Hid(298.15, T, phase))
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropyF = result + entF
                    result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropyF.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_entropyF = result
                Case "viscosity"
                    If state = "L" Then
                        result = Me.AUX_LIQVISCm(T)
                    Else
                        result = Me.AUX_VAPVISCm(T, Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density.GetValueOrDefault, Me.AUX_MMM(phase))
                    End If
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.viscosity = result
                Case "thermalconductivity"
                    If state = "L" Then
                        result = Me.AUX_CONDTL(T)
                    Else
                        result = Me.AUX_CONDTG(T, P)
                    End If
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.thermalConductivity = result
                Case "fugacity", "fugacitycoefficient", "logfugacitycoefficient", "activity", "activitycoefficient"
                    Me.DW_CalcCompFugCoeff(phase)
                Case "volume", "density"
                    If state = "L" Then
                        result = Me.AUX_LIQDENS(T, P, 0.0#, phaseID, False)
                    Else
                        result = Me.AUX_VAPDENS(T, P)
                    End If
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density = result
                Case "surfacetension"
                    Me.CurrentMaterialStream.Fases(0).TPMProperties.surfaceTension = Me.AUX_SURFTM(T)
                Case "osmoticcoefficient"
                    Dim constprops As New List(Of ConstantProperties)
                    For Each su As Substancia In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                        constprops.Add(su.ConstantProperties)
                    Next
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.osmoticCoefficient = Me.m_elec.OsmoticCoeff(RET_VMOL(phase), Me.m_uni.GAMMA_MR(T, RET_VMOL(phase), constprops), constprops)
                Case "freezingpoint"
                    Dim constprops As New List(Of ConstantProperties)
                    For Each su As Substancia In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                        constprops.Add(su.ConstantProperties)
                    Next
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.freezingPoint = Me.m_elec.FreezingPointDepression(RET_VMOL(phase), Me.m_uni.GAMMA_MR(T, RET_VMOL(phase), constprops), constprops)(0)
                Case "freezingpointdepression"
                    Dim constprops As New List(Of ConstantProperties)
                    For Each su As Substancia In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                        constprops.Add(su.ConstantProperties)
                    Next
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.freezingPointDepression = Me.m_elec.FreezingPointDepression(RET_VMOL(phase), Me.m_uni.GAMMA_MR(T, RET_VMOL(phase), constprops), constprops)(1)
                Case "ph"
                    Dim constprops As New List(Of ConstantProperties)
                    For Each su As Substancia In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                        constprops.Add(su.ConstantProperties)
                    Next
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.pH = Me.m_elec.pH(RET_VMOL(phase), T, Me.m_uni.GAMMA_MR(T, RET_VMOL(phase), constprops), constprops)
                Case Else
                    Dim ex As Exception = New CapeOpen.CapeThrmPropertyNotAvailableException
                    ThrowCAPEException(ex, "Error", ex.Message, "ICapeThermoMaterial", ex.Source, ex.StackTrace, "CalcSinglePhaseProp/CalcTwoPhaseProp/CalcProp", ex.GetHashCode)
            End Select

        End Sub

        Public Overrides Sub DW_CalcPhaseProps(ByVal fase As DWSIM.SimulationObjects.PropertyPackages.Fase)

            Dim result As Double
            Dim resultObj As Object
            Dim dwpl As Fase

            Dim T, P As Double
            Dim phasemolarfrac As Double = Nothing
            Dim overallmolarflow As Double = Nothing

            Dim phaseID As Integer
            T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault
            P = Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure.GetValueOrDefault

            Select Case fase
                Case PropertyPackages.Fase.Mixture
                    phaseID = 0
                    dwpl = PropertyPackages.Fase.Mixture
                Case PropertyPackages.Fase.Vapor
                    phaseID = 2
                    dwpl = PropertyPackages.Fase.Vapor
                Case PropertyPackages.Fase.Liquid1
                    phaseID = 3
                    dwpl = PropertyPackages.Fase.Liquid1
                Case PropertyPackages.Fase.Liquid2
                    phaseID = 4
                    dwpl = PropertyPackages.Fase.Liquid2
                Case PropertyPackages.Fase.Liquid3
                    phaseID = 5
                    dwpl = PropertyPackages.Fase.Liquid3
                Case PropertyPackages.Fase.Liquid
                    phaseID = 1
                    dwpl = PropertyPackages.Fase.Liquid
                Case PropertyPackages.Fase.Aqueous
                    phaseID = 6
                    dwpl = PropertyPackages.Fase.Aqueous
                Case PropertyPackages.Fase.Solid
                    phaseID = 7
                    dwpl = PropertyPackages.Fase.Solid
            End Select

            If phaseID > 0 Then
                overallmolarflow = Me.CurrentMaterialStream.Fases(0).SPMProperties.molarflow.GetValueOrDefault
                phasemolarfrac = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molarfraction.GetValueOrDefault
                result = overallmolarflow * phasemolarfrac
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molarflow = result
                result = result * Me.AUX_MMM(fase) / 1000
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.massflow = result
                result = phasemolarfrac * overallmolarflow * Me.AUX_MMM(fase) / 1000 / Me.CurrentMaterialStream.Fases(0).SPMProperties.massflow.GetValueOrDefault
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.massfraction = result
                Me.DW_CalcCompVolFlow(phaseID)
                Me.DW_CalcCompFugCoeff(fase)
            End If

            If phaseID = 3 Then

                Me.DW_CalcProp("osmoticcoefficient", PropertyPackages.Fase.Liquid1)
                Me.DW_CalcProp("freezingpoint", PropertyPackages.Fase.Liquid1)
                Me.DW_CalcProp("freezingpointdepression", PropertyPackages.Fase.Liquid1)
                Me.DW_CalcProp("ph", PropertyPackages.Fase.Liquid1)

            End If

            Dim constprops As New List(Of ConstantProperties)
            For Each su As Substancia In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                constprops.Add(su.ConstantProperties)
            Next

            If phaseID = 3 Or phaseID = 4 Or phaseID = 5 Or phaseID = 6 Then

                result = Me.AUX_LIQDENS(T, P, 0.0#, phaseID, False)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density = result
                result = Me.m_elec.LiquidEnthalpy(T, RET_VMOL(dwpl), constprops, Me.m_uni.GAMMA_MR(T + 0.1, RET_VMOL(dwpl), constprops), Me.m_uni.GAMMA_MR(T, RET_VMOL(dwpl), constprops), False)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy = result
                result = 0.0#
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy = result
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.compressibilityFactor = 0.0#
                result = Me.m_elec.HeatCapacityCp(T, RET_VMOL(dwpl), constprops)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCp = result
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCv = result
                result = Me.AUX_MMM(fase)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight = result
                result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_enthalpy = result
                result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_entropy = result
                result = Me.AUX_CONDTL(T)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.thermalConductivity = result
                result = Me.AUX_LIQVISCm(T)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.viscosity = result
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.kinematic_viscosity = result / Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density.Value

            ElseIf phaseID = 2 Then

                result = Me.AUX_VAPDENS(T, P)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density = result
                result = Me.m_elec.LiquidEnthalpy(T, RET_VMOL(fase.Vapor), constprops, Me.m_uni.GAMMA_MR(T + 0.1, RET_VMOL(fase.Vapor), constprops), Me.m_uni.GAMMA_MR(T, RET_VMOL(fase.Vapor), constprops), False)
                result += Me.RET_HVAPM(RET_VMAS(fase.Vapor), T)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy = result
                result = Me.m_id.S_RA_MIX("V", T, P, RET_VMOL(fase.Vapor), RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Sid(298.15, T, P, fase.Vapor), Me.RET_VHVAP(T), Me.RET_Hid(298.15, T, fase.Vapor))
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy = result
                result = 1
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.compressibilityFactor = result
                result = Me.AUX_CPm(PropertyPackages.Fase.Vapor, T)
                resultObj = Me.m_id.CpCv("V", T, P, RET_VMOL(PropertyPackages.Fase.Vapor), RET_VKij(), RET_VMAS(PropertyPackages.Fase.Vapor), RET_VTC(), RET_VPC(), RET_VCP(T), RET_VMM(), RET_VW(), RET_VZRa())
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCp = resultObj(1)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCv = resultObj(2)
                result = Me.AUX_MMM(fase)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight = result
                result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_enthalpy = result
                result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_entropy = result
                result = Me.AUX_CONDTG(T, P)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.thermalConductivity = result
                result = Me.AUX_VAPVISCm(T, Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density.GetValueOrDefault, Me.AUX_MMM(fase))
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.viscosity = result
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.kinematic_viscosity = result / Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density.Value

            ElseIf phaseID = 7 Then

                result = Me.AUX_SOLIDDENS
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density = result
                result = Me.m_elec.SolidEnthalpy(T, RET_VMOL(PropertyPackages.Fase.Solid), constprops)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy = result
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy = 0.0# 'result
                result = 1
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.compressibilityFactor = 0.0# 'result
                result = Me.AUX_SOLIDCP(RET_VMAS(fase), constprops, T)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCp = result
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.heatCapacityCv = result
                result = Me.AUX_MMM(fase)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight = result
                result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.enthalpy.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_enthalpy = 0.0# 'result
                result = Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.entropy.GetValueOrDefault * Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molecularWeight.GetValueOrDefault
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.molar_entropy = 0.0# 'result
                result = Me.AUX_CONDTG(T, P)
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.thermalConductivity = 0.0# 'result
                result = Me.AUX_VAPVISCm(T, Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density.GetValueOrDefault, Me.AUX_MMM(fase))
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.viscosity = 0.0# 'result
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.kinematic_viscosity = 0.0# 'result / Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density.Value

            ElseIf phaseID = 1 Then

                DW_CalcLiqMixtureProps()

            Else

                DW_CalcOverallProps()

            End If

            If phaseID > 0 Then
                result = overallmolarflow * phasemolarfrac * Me.AUX_MMM(fase) / 1000 / Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density.GetValueOrDefault
                Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.volumetric_flow = result
            End If

        End Sub

        Public Overrides Function DW_CalcEnthalpy(ByVal Vx As System.Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double

            Dim H As Double

            Dim constprops As New List(Of ConstantProperties)
            For Each su As Substancia In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                constprops.Add(su.ConstantProperties)
            Next

            Select Case st
                Case State.Liquid
                    H = Me.m_elec.LiquidEnthalpy(T, Vx, constprops, Me.m_uni.GAMMA_MR(T + 0.1, Vx, constprops), Me.m_uni.GAMMA_MR(T, Vx, constprops), False)
                Case State.Solid
                    H = Me.m_elec.SolidEnthalpy(T, Vx, constprops)
                Case State.Vapor
                    H = m_id.H_RA_MIX("V", T, P, Vx, RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), Me.RET_Hid(298.15, T, Vx), Me.RET_VHVAP(T))
            End Select

            Return H

        End Function

        Public Overrides Function DW_CalcEnthalpyDeparture(ByVal Vx As System.Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double

            Dim H As Double

            Dim constprops As New List(Of ConstantProperties)
            For Each su As Substancia In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                constprops.Add(su.ConstantProperties)
            Next

            Select Case st
                Case State.Liquid
                    H = Me.m_elec.LiquidEnthalpy(T, Vx, constprops, Me.m_uni.GAMMA_MR(T + 0.1, Vx, constprops), Me.m_uni.GAMMA_MR(T, Vx, constprops), True)
                Case State.Solid
                    H = Me.m_elec.SolidEnthalpy(T, Vx, constprops)
                Case State.Vapor
                    H = m_id.H_RA_MIX("V", T, P, Vx, RET_VKij, RET_VTC(), RET_VPC(), RET_VW(), RET_VMM(), 0.0#, Me.RET_VHVAP(T))
            End Select

            Return H

        End Function

        Public Overrides Function DW_CalcEntropy(ByVal Vx As System.Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double

            Return 0.0#

        End Function

        Public Overrides Function DW_CalcEntropyDeparture(ByVal Vx As System.Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double

            Return 0.0#

        End Function

        Public Overrides Function DW_CalcFugCoeff(ByVal Vx As System.Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double()

            DWSIM.App.WriteToConsole(Me.ComponentName & " fugacity coefficient calculation for phase '" & st.ToString & "' requested at T = " & T & " K and P = " & P & " Pa.", 2)
            DWSIM.App.WriteToConsole("Compounds: " & Me.RET_VNAMES.ToArrayString, 2)
            DWSIM.App.WriteToConsole("Mole fractions: " & Vx.ToArrayString(), 2)

            Dim prn As New PropertyPackages.ThermoPlugs.PR

            Dim n As Integer = UBound(Vx)
            Dim lnfug(n), ativ(n) As Double
            Dim fugcoeff(n) As Double
            Dim i As Integer

            Dim Tc As Object = Me.RET_VTC()

            Dim constprops As New List(Of ConstantProperties)
            For Each s As Substancia In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                constprops.Add(s.ConstantProperties)
            Next

            If st = State.Liquid Then
                ativ = Me.m_uni.GAMMA_MR(T, Vx, constprops)
                For i = 0 To n
                    If T / Tc(i) >= 1 Then
                        lnfug(i) = Math.Log(AUX_KHenry(Me.RET_VNAMES(i), T) / P)
                    Else
                        lnfug(i) = Math.Log(ativ(i) * Me.AUX_PVAPi(i, T) / (P))
                    End If
                Next
            ElseIf st = State.Vapor Then
                For i = 0 To n
                    lnfug(i) = 0.0#
                Next
            ElseIf st = State.Solid Then
                For i = 0 To n
                    If constprops(i).TemperatureOfFusion <> 0 Then
                        lnfug(i) = Log(Me.AUX_PVAPi(i, T) * Exp(-constprops(i).EnthalpyOfFusionAtTf / (8.314 * T) * (1 - T / constprops(i).TemperatureOfFusion)))
                    Else
                        lnfug(i) = 0.0#
                    End If
                Next
            End If

            For i = 0 To n
                fugcoeff(i) = Exp(lnfug(i))
            Next

            DWSIM.App.WriteToConsole("Result: " & fugcoeff.ToArrayString(), 2)

            Return fugcoeff

        End Function

#End Region

    End Class

End Namespace
