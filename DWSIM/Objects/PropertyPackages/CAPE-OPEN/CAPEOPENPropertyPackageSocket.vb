'    CAPE-OPEN Property Package Wrapper
'    Copyright 2011 Daniel Wagner O. de Medeiros
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

Imports DWSIM.DWSIM.SimulationObjects.Streams
Imports DWSIM.DWSIM.SimulationObjects
Imports DWSIM.DWSIM.ClassesBasicasTermodinamica
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Runtime.Serialization
Imports System.IO
Imports System.Math
Imports CapeOpen
Imports DWSIM.DWSIM.SimulationObjects.UnitOps.Auxiliary.CapeOpen
Imports Microsoft.Win32
Imports System.Linq
Imports System.Runtime.InteropServices
Imports DWSIM.DWSIM.SimulationObjects.UnitOps

Namespace DWSIM.SimulationObjects.PropertyPackages

    <System.Serializable()> Public Class CAPEOPENPropertyPackage

        Inherits DWSIM.SimulationObjects.PropertyPackages.PropertyPackage

        <System.NonSerialized()> Private _copp, _pptpl As Object 'CAPE-OPEN Property Package & Manager

        Private _selts As CapeOpenUnitOpInfo
        Private _istrpp, _istrts As ComIStreamWrapper
        Private _ppname As String = ""

        Private _coversion As String = "1.0"

        Private m_props As New DWSIM.SimulationObjects.PropertyPackages.Auxiliary.PROPS

        Public _mappings As New Dictionary(Of String, String)

#Region "    IDisposable Support "

        Private disposedValue As Boolean = False        ' To detect redundant calls

        ' IDisposable
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: free other state (managed objects).
                End If

                If Not _copp Is Nothing Then
                    If Marshal.IsComObject(_copp) Then Marshal.ReleaseComObject(_copp)
                End If
                If Not _pptpl Is Nothing Then
                    If Marshal.IsComObject(_pptpl) Then Marshal.ReleaseComObject(_pptpl)
                End If
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Overrides Sub Dispose()
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub

#End Region

#Region "    DWSIM Methods and Procedures"

        Public Sub New()

            CreatePhaseMappings()

            Me._packagetype = PropertyPackages.PackageType.CAPEOPEN
            Me.IsConfigurable = True

        End Sub

        Public Overrides Sub ReconfigureConfigForm()

            Me.ConfigForm = New FormConfigCAPEOPEN

        End Sub

        Public Overrides Sub ShowConfigForm(Optional ByVal owner As IWin32Window = Nothing)

            If Me._phasemappings Is Nothing Then CreatePhaseMappings()

            CType(Me.ConfigForm, FormConfigCAPEOPEN)._copp = Me._copp
            CType(Me.ConfigForm, FormConfigCAPEOPEN)._pptpl = Me._pptpl
            CType(Me.ConfigForm, FormConfigCAPEOPEN)._coversion = Me._coversion
            CType(Me.ConfigForm, FormConfigCAPEOPEN)._selts = Me._selts
            CType(Me.ConfigForm, FormConfigCAPEOPEN)._ppname = Me._ppname
            CType(Me.ConfigForm, FormConfigCAPEOPEN)._mappings = Me._mappings
            CType(Me.ConfigForm, FormConfigCAPEOPEN)._phasemappings = Me._phasemappings

            If Not owner Is Nothing Then Me.ConfigForm.ShowDialog(owner) Else Me.ConfigForm.ShowDialog()

            If Me.ConfigForm.DialogResult = DialogResult.OK Then
                Me._copp = CType(Me.ConfigForm, FormConfigCAPEOPEN)._copp
                Me._pptpl = CType(Me.ConfigForm, FormConfigCAPEOPEN)._pptpl
                Me._coversion = CType(Me.ConfigForm, FormConfigCAPEOPEN)._coversion
                Me._selts = CType(Me.ConfigForm, FormConfigCAPEOPEN)._selts
                Me._ppname = CType(Me.ConfigForm, FormConfigCAPEOPEN)._ppname
                Me._mappings = CType(Me.ConfigForm, FormConfigCAPEOPEN)._mappings
                Me._phasemappings = CType(Me.ConfigForm, FormConfigCAPEOPEN)._phasemappings
            End If

            Me.ConfigForm = Nothing

        End Sub

        Public Overrides Sub DW_CalcProp(ByVal [property] As String, ByVal phase As Fase)
            'do nothing
        End Sub

        Public Overrides Function DW_CalcBubP(ByVal Vx As System.Array, ByVal T As Double, Optional ByVal Pref As Double = 0.0, Optional ByVal K As System.Array = Nothing, Optional ByVal ReuseK As Boolean = False) As Object
            Dim res As Object
            res = Me.DW_CalcEquilibrio_ISOL(Vx, FlashSpec.T, FlashSpec.VAP, T, 0, 0)
            Return New Object() {res(0), res(1), res(8), res(9), res(3), 0, res(10)}
        End Function

        Public Overrides Function DW_CalcBubT(ByVal Vx As System.Array, ByVal P As Double, Optional ByVal Tref As Double = 0.0, Optional ByVal K As System.Array = Nothing, Optional ByVal ReuseK As Boolean = False) As Object
            Dim res As Object
            res = Me.DW_CalcEquilibrio_ISOL(Vx, FlashSpec.P, FlashSpec.VAP, P, 0, 0)
            Return New Object() {res(0), res(1), res(8), res(9), res(2), 0, res(10)}
        End Function

        Public Overrides Function DW_CalcDewP(ByVal Vx As System.Array, ByVal T As Double, Optional ByVal Pref As Double = 0.0, Optional ByVal K As System.Array = Nothing, Optional ByVal ReuseK As Boolean = False) As Object
            Dim res As Object
            res = Me.DW_CalcEquilibrio_ISOL(Vx, FlashSpec.T, FlashSpec.VAP, T, 1, 0)
            Return New Object() {res(0), res(1), res(8), res(9), res(3), 0, res(10)}
        End Function

        Public Overrides Function DW_CalcDewT(ByVal Vx As System.Array, ByVal P As Double, Optional ByVal Tref As Double = 0.0, Optional ByVal K As System.Array = Nothing, Optional ByVal ReuseK As Boolean = False) As Object
            Dim res As Object
            res = Me.DW_CalcEquilibrio_ISOL(Vx, FlashSpec.P, FlashSpec.VAP, P, 1, 0)
            Return New Object() {res(0), res(1), res(8), res(9), res(2), 0, res(10)}
        End Function

        Public Overrides Sub DW_CalcEquilibrium(ByVal spec1 As FlashSpec, ByVal spec2 As FlashSpec)

            Me.CurrentMaterialStream.AtEquilibrium = False

            Dim s1 As String() = New String() {}
            Dim s2 As String() = New String() {}
            Dim s11 As String = ""
            Dim s22 As String = ""

            Select Case spec1
                Case FlashSpec.T
                    s1 = New String() {"temperature", Nothing, "Overall"}
                    s11 = "T"
                Case FlashSpec.P
                    s1 = New String() {"pressure", Nothing, "Overall"}
                    s11 = "P"
            End Select

            Select Case spec2
                Case FlashSpec.T
                    s2 = New String() {"temperature", Nothing, "Overall"}
                    s22 = "T"
                Case FlashSpec.P
                    s2 = New String() {"pressure", Nothing, "Overall"}
                    s22 = "P"
                Case FlashSpec.S
                    s2 = New String() {"entropy", Nothing, "Overall"}
                    s22 = "S"
                Case FlashSpec.H
                    s2 = New String() {"enthalpy", Nothing, "Overall"}
                    s22 = "H"
                Case FlashSpec.VAP
                    s2 = New String() {"phaseFraction", "Mole", "Vapor"}
                    s22 = "VF"
            End Select

            Me.DW_ZerarPhaseProps(Fase.Vapor)
            Me.DW_ZerarPhaseProps(Fase.Liquid)
            Me.DW_ZerarPhaseProps(Fase.Liquid1)
            Me.DW_ZerarPhaseProps(Fase.Liquid2)
            Me.DW_ZerarPhaseProps(Fase.Liquid3)
            Me.DW_ZerarPhaseProps(Fase.Aqueous)
            Me.DW_ZerarPhaseProps(Fase.Solid)
            Me.DW_ZerarComposicoes(Fase.Liquid)
            Me.DW_ZerarComposicoes(Fase.Liquid1)
            Me.DW_ZerarComposicoes(Fase.Liquid2)
            Me.DW_ZerarComposicoes(Fase.Liquid3)
            Me.DW_ZerarComposicoes(Fase.Aqueous)
            Me.DW_ZerarComposicoes(Fase.Vapor)
            Me.DW_ZerarComposicoes(Fase.Solid)

            If _coversion = "1.0" Then
                Try
                    Me.CalcEquilibrium(Me.CurrentMaterialStream, s11 + s22, Nothing)
                Catch ex As Exception
                    Dim ecu As CapeOpen.ECapeUser = _copp
                    Me.CurrentMaterialStream.Flowsheet.WriteToLog(Me.ComponentName & ": CAPE-OPEN Exception " & ecu.code & " at " & ecu.interfaceName & "." & ecu.scope & ". Reason: " & ecu.description, Color.Red, FormClasses.TipoAviso.Erro)
                End Try
            Else
                Try
                    Me.SetMaterial(Me.CurrentMaterialStream)
                    Me.CalcEquilibrium1(s1, s2, "Unspecified")
                Catch ex As Exception
                    Dim ecu As CapeOpen.ECapeUser = _copp
                    Me.CurrentMaterialStream.Flowsheet.WriteToLog(Me.ComponentName & ": CAPE-OPEN Exception " & ecu.code & " at " & ecu.interfaceName & "." & ecu.scope & ". Reason: " & ecu.description, Color.Red, FormClasses.TipoAviso.Erro)
                End Try
            End If

            Me.CurrentMaterialStream.AtEquilibrium = True

            Dim summf As Double = 0.0#, sumwf As Double = 0.0#
            For Each pi As PhaseInfo In Me.PhaseMappings.Values
                If Not pi.PhaseLabel = "Disabled" Then
                    summf += Me.CurrentMaterialStream.Fases(pi.DWPhaseIndex).SPMProperties.molarfraction.GetValueOrDefault
                    sumwf += Me.CurrentMaterialStream.Fases(pi.DWPhaseIndex).SPMProperties.massfraction.GetValueOrDefault
                End If
            Next
            If Abs(summf - 1) > 0.000001 Then
                For Each pi As PhaseInfo In Me.PhaseMappings.Values
                    If Not pi.PhaseLabel = "Disabled" Then
                        If Not Me.CurrentMaterialStream.Fases(pi.DWPhaseIndex).SPMProperties.molarfraction.HasValue Then
                            Me.CurrentMaterialStream.Fases(pi.DWPhaseIndex).SPMProperties.molarfraction = 1 - summf
                            Me.CurrentMaterialStream.Fases(pi.DWPhaseIndex).SPMProperties.massfraction = 1 - sumwf
                        End If
                    End If
                Next
            End If

            For Each pi As PhaseInfo In Me.PhaseMappings.Values
                If Not pi.PhaseLabel = "Disabled" Then
                    Dim subst As DWSIM.ClassesBasicasTermodinamica.Substancia
                    Me.CurrentMaterialStream.Fases(pi.DWPhaseIndex).SPMProperties.molecularWeight = Me.AUX_MMM(pi.DWPhaseID)
                    For Each subst In Me.CurrentMaterialStream.Fases(pi.DWPhaseIndex).Componentes.Values
                        subst.FracaoMassica = Me.AUX_CONVERT_MOL_TO_MASS(subst.Nome, pi.DWPhaseIndex)
                    Next
                End If
            Next


        End Sub

        Public Overrides Function DW_CalcEquilibrio_ISOL(ByVal spec1 As FlashSpec, ByVal spec2 As FlashSpec, ByVal val1 As Double, ByVal val2 As Double, ByVal estimate As Double) As Object

            Dim pstr As MaterialStream = Me.CurrentMaterialStream
            Dim tstr As MaterialStream = Me.CurrentMaterialStream.Clone

            Me.CurrentMaterialStream = tstr

            Me.CurrentMaterialStream.AtEquilibrium = False

            Dim s1 As String() = New String() {}
            Dim s2 As String() = New String() {}
            Dim s11 As String = ""
            Dim s22 As String = ""

            Select Case spec1
                Case FlashSpec.T
                    s1 = New String() {"temperature", Nothing, "Overall"}
                    s11 = "T"
                    Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature = val1
                Case FlashSpec.P
                    s1 = New String() {"pressure", Nothing, "Overall"}
                    s11 = "P"
                    Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure = val1
            End Select

            Select Case spec2
                Case FlashSpec.T
                    s2 = New String() {"temperature", Nothing, "Overall"}
                    s22 = "T"
                    Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature = val2
                Case FlashSpec.P
                    s2 = New String() {"pressure", Nothing, "Overall"}
                    s22 = "P"
                    Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure = val2
                Case FlashSpec.S
                    s2 = New String() {"entropy", Nothing, "Overall"}
                    s22 = "S"
                    Me.CurrentMaterialStream.Fases(0).SPMProperties.entropy = val2
                Case FlashSpec.H
                    s2 = New String() {"enthalpy", Nothing, "Overall"}
                    s22 = "H"
                    Me.CurrentMaterialStream.Fases(0).SPMProperties.enthalpy = val2
                Case FlashSpec.VAP
                    s2 = New String() {"phaseFraction", "Mole", "Vapor"}
                    s22 = "VF"
                    Me.CurrentMaterialStream.Fases(2).SPMProperties.molarfraction = val2
            End Select

            Me.DW_ZerarPhaseProps(Fase.Vapor)
            Me.DW_ZerarPhaseProps(Fase.Liquid)
            Me.DW_ZerarPhaseProps(Fase.Liquid1)
            Me.DW_ZerarPhaseProps(Fase.Liquid2)
            Me.DW_ZerarPhaseProps(Fase.Liquid3)
            Me.DW_ZerarPhaseProps(Fase.Aqueous)
            Me.DW_ZerarPhaseProps(Fase.Solid)

            If _coversion = "1.0" Then
                Try
                    Me.CalcEquilibrium(Me.CurrentMaterialStream, s11 + s22, Nothing)
                Catch ex As Exception
                    Dim ecu As CapeOpen.ECapeUser = _copp
                    Me.CurrentMaterialStream.Flowsheet.WriteToLog(Me.ComponentName & ": CAPE-OPEN Exception " & ecu.code & " at " & ecu.interfaceName & "." & ecu.scope & ". Reason: " & ecu.description, Color.Red, FormClasses.TipoAviso.Erro)
                End Try
            Else
                Try
                    Me.SetMaterial(Me.CurrentMaterialStream)
                    Me.CalcEquilibrium1(s1, s2, "Unspecified")
                Catch ex As Exception
                    Dim ecu As CapeOpen.ECapeUser = _copp
                    Me.CurrentMaterialStream.Flowsheet.WriteToLog(Me.ComponentName & ": CAPE-OPEN Exception " & ecu.code & " at " & ecu.interfaceName & "." & ecu.scope & ". Reason: " & ecu.description, Color.Red, FormClasses.TipoAviso.Erro)
                End Try
            End If

            Me.CurrentMaterialStream.AtEquilibrium = True

            Dim summf As Double = 0.0#, sumwf As Double = 0.0#
            For Each pi As PhaseInfo In Me.PhaseMappings.Values
                If Not pi.PhaseLabel = "Disabled" Then
                    summf += Me.CurrentMaterialStream.Fases(pi.DWPhaseIndex).SPMProperties.molarfraction.GetValueOrDefault
                    sumwf += Me.CurrentMaterialStream.Fases(pi.DWPhaseIndex).SPMProperties.massfraction.GetValueOrDefault
                End If
            Next
            If Abs(summf - 1) > 0.000001 Then
                For Each pi As PhaseInfo In Me.PhaseMappings.Values
                    If Not pi.PhaseLabel = "Disabled" And Not Me.CurrentMaterialStream.Fases(pi.DWPhaseIndex).SPMProperties.molarfraction.HasValue Then
                        Me.CurrentMaterialStream.Fases(pi.DWPhaseIndex).SPMProperties.molarfraction = 1 - summf
                        Me.CurrentMaterialStream.Fases(pi.DWPhaseIndex).SPMProperties.massfraction = 1 - sumwf
                    End If
                Next
            End If

            For Each pi As PhaseInfo In Me.PhaseMappings.Values
                If Not pi.PhaseLabel = "Disabled" Then
                    Me.CurrentMaterialStream.Fases(pi.DWPhaseIndex).SPMProperties.molecularWeight = Me.AUX_MMM(pi.DWPhaseID)
                    DW_CalcPhaseProps(pi.DWPhaseID)
                End If
            Next

            DW_CalcPhaseProps(Fase.Liquid)
            DW_CalcPhaseProps(Fase.Mixture)

            Dim T, P, H, S, xl, xv As Double, i As Integer
            Dim Vx(Me.CurrentMaterialStream.Fases(0).Componentes.Count - 1), Vy(Me.CurrentMaterialStream.Fases(0).Componentes.Count - 1) As Double
            i = 0
            For Each su As Substancia In Me.CurrentMaterialStream.Fases(1).Componentes.Values
                Vx(i) = su.FracaoMolar.GetValueOrDefault
                i += 1
            Next
            i = 0
            For Each su As Substancia In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                Vy(i) = su.FracaoMolar.GetValueOrDefault
                i += 1
            Next
            xl = Me.CurrentMaterialStream.Fases(1).SPMProperties.molarfraction.GetValueOrDefault
            xv = Me.CurrentMaterialStream.Fases(2).SPMProperties.molarfraction.GetValueOrDefault
            T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault
            P = Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure.GetValueOrDefault
            H = Me.CurrentMaterialStream.Fases(0).SPMProperties.enthalpy.GetValueOrDefault
            S = Me.CurrentMaterialStream.Fases(0).SPMProperties.entropy.GetValueOrDefault

            Me.CurrentMaterialStream = pstr
            tstr = Nothing

            Return New Object() {xl, xv, T, P, H, S, 1, 1, Vx, Vy, Nothing}

        End Function

        Public Overloads Function DW_CalcEquilibrio_ISOL(ByVal Vz As Array, ByVal spec1 As FlashSpec, ByVal spec2 As FlashSpec, ByVal val1 As Double, ByVal val2 As Double, ByVal estimate As Double) As Object

            Dim pstr As MaterialStream = Me.CurrentMaterialStream
            Dim tstr As MaterialStream = Me.CurrentMaterialStream.Clone

            Me.CurrentMaterialStream = tstr

            Me.CurrentMaterialStream.AtEquilibrium = False

            Dim s1 As String() = New String() {}
            Dim s2 As String() = New String() {}
            Dim s11 As String = ""
            Dim s22 As String = ""

            Select Case spec1
                Case FlashSpec.T
                    s1 = New String() {"temperature", Nothing, "Overall"}
                    s11 = "T"
                    Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature = val1
                Case FlashSpec.P
                    s1 = New String() {"pressure", Nothing, "Overall"}
                    s11 = "P"
                    Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure = val1
            End Select

            Select Case spec2
                Case FlashSpec.T
                    s2 = New String() {"temperature", Nothing, "Overall"}
                    s22 = "T"
                    Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature = val2
                Case FlashSpec.P
                    s2 = New String() {"pressure", Nothing, "Overall"}
                    s22 = "P"
                    Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure = val2
                Case FlashSpec.S
                    s2 = New String() {"entropy", Nothing, "Overall"}
                    s22 = "S"
                    Me.CurrentMaterialStream.Fases(0).SPMProperties.entropy = val2
                Case FlashSpec.H
                    s2 = New String() {"enthalpy", Nothing, "Overall"}
                    s22 = "H"
                    Me.CurrentMaterialStream.Fases(0).SPMProperties.enthalpy = val2
                Case FlashSpec.VAP
                    s2 = New String() {"phaseFraction", "Mole", "Vapor"}
                    s22 = "VF"
                    Me.CurrentMaterialStream.Fases(2).SPMProperties.molarfraction = val2
            End Select

            Dim i As Integer = 0
            For Each c As Substancia In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                c.FracaoMolar = Vz(i)
                i += 1
            Next

            Me.DW_ZerarPhaseProps(Fase.Vapor)
            Me.DW_ZerarPhaseProps(Fase.Liquid)
            Me.DW_ZerarPhaseProps(Fase.Liquid1)
            Me.DW_ZerarPhaseProps(Fase.Liquid2)
            Me.DW_ZerarPhaseProps(Fase.Liquid3)
            Me.DW_ZerarPhaseProps(Fase.Aqueous)
            Me.DW_ZerarPhaseProps(Fase.Solid)

            If _coversion = "1.0" Then
                Try
                    CType(_copp, ICapeThermoPropertyPackage).CalcEquilibrium(Me.CurrentMaterialStream, s11 + s22, Nothing)
                Catch ex As Exception
                    Dim ecu As CapeOpen.ECapeUser = _copp
                    Me.CurrentMaterialStream.Flowsheet.WriteToLog(Me.ComponentName & ": CAPE-OPEN Exception " & ecu.code & " at " & ecu.interfaceName & "." & ecu.scope & ". Reason: " & ecu.description, Color.Red, FormClasses.TipoAviso.Erro)
                End Try
            Else
                Try
                    CType(_copp, ICapeThermoMaterialContext).SetMaterial(Me.CurrentMaterialStream)
                    Dim ok As Boolean = CType(_copp, ICapeThermoEquilibriumRoutine).CheckEquilibriumSpec(s1, s2, "Unspecified")
                    CType(_copp, ICapeThermoEquilibriumRoutine).CalcEquilibrium(s1, s2, "Unspecified")
                Catch ex As Exception
                    Dim ecu As CapeOpen.ECapeUser = _copp
                    Me.CurrentMaterialStream.Flowsheet.WriteToLog(Me.ComponentName & ": CAPE-OPEN Exception " & ecu.code & " at " & ecu.interfaceName & "." & ecu.scope & ". Reason: " & ecu.description, Color.Red, FormClasses.TipoAviso.Erro)
                End Try
            End If

            Me.CurrentMaterialStream.AtEquilibrium = True

            Dim summf As Double = 0, sumwf As Double = 0
            For Each pi As PhaseInfo In Me.PhaseMappings.Values
                If Not pi.PhaseLabel = "Disabled" Then
                    summf += Me.CurrentMaterialStream.Fases(pi.DWPhaseIndex).SPMProperties.molarfraction.GetValueOrDefault
                    sumwf += Me.CurrentMaterialStream.Fases(pi.DWPhaseIndex).SPMProperties.massfraction.GetValueOrDefault
                End If
            Next
            If Abs(summf - 1) > 0.000001 Then
                For Each pi As PhaseInfo In Me.PhaseMappings.Values
                    If Not pi.PhaseLabel = "Disabled" And Not Me.CurrentMaterialStream.Fases(pi.DWPhaseIndex).SPMProperties.molarfraction.HasValue Then
                        Me.CurrentMaterialStream.Fases(pi.DWPhaseIndex).SPMProperties.molarfraction = 1 - summf
                        Me.CurrentMaterialStream.Fases(pi.DWPhaseIndex).SPMProperties.massfraction = 1 - sumwf
                    End If
                Next
            End If

            For Each pi As PhaseInfo In Me.PhaseMappings.Values
                If Not pi.PhaseLabel = "Disabled" Then
                    Me.CurrentMaterialStream.Fases(pi.DWPhaseIndex).SPMProperties.molecularWeight = Me.AUX_MMM(pi.DWPhaseID)
                    DW_CalcPhaseProps(pi.DWPhaseID)
                End If
            Next

            DW_CalcPhaseProps(Fase.Liquid)
            DW_CalcPhaseProps(Fase.Mixture)

            Dim T, P, H, S, xl, xv As Double
            Dim Ki(Me.CurrentMaterialStream.Fases(0).Componentes.Count - 1), Vx(Me.CurrentMaterialStream.Fases(0).Componentes.Count - 1), Vy(Me.CurrentMaterialStream.Fases(0).Componentes.Count - 1) As Double
            i = 0
            For Each su As Substancia In Me.CurrentMaterialStream.Fases(1).Componentes.Values
                Vx(i) = su.FracaoMolar.GetValueOrDefault
                i += 1
            Next
            i = 0
            For Each su As Substancia In Me.CurrentMaterialStream.Fases(2).Componentes.Values
                Vy(i) = su.FracaoMolar.GetValueOrDefault
                i += 1
            Next
            i = 0
            For i = 0 To UBound(Vx)
                Ki(i) = Vy(i) / Vx(i)
            Next

            xl = Me.CurrentMaterialStream.Fases(1).SPMProperties.molarfraction.GetValueOrDefault
            xv = Me.CurrentMaterialStream.Fases(2).SPMProperties.molarfraction.GetValueOrDefault
            T = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault
            P = Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure.GetValueOrDefault
            H = Me.CurrentMaterialStream.Fases(0).SPMProperties.enthalpy.GetValueOrDefault
            S = Me.CurrentMaterialStream.Fases(0).SPMProperties.entropy.GetValueOrDefault

            Me.CurrentMaterialStream = pstr
            tstr = Nothing

            Return New Object() {xl, xv, T, P, H, S, 1, 1, Vx, Vy, Ki}

        End Function

        Public Overrides Function DW_CalcCp_ISOL(ByVal fase1 As DWSIM.SimulationObjects.PropertyPackages.Fase, ByVal T As Double, ByVal P As Double) As Double

            Dim res As Double = 0.0#, phase As String = "", tant As Double, pant As Double

            Select Case fase1
                Case Fase.Vapor
                    phase = "Vapor"
                Case Fase.Liquid
                    phase = "Liquid"
            End Select

            tant = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault
            pant = Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure.GetValueOrDefault

            Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature = T
            Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure = P

            If _coversion = "1.0" Then
                CType(_copp, ICapeThermoCalculationRoutine).CalcProp(Me.CurrentMaterialStream, New String() {"heatCapacity"}, New String() {phase}, "Mixture")
                Return Me.CurrentMaterialStream.Fases(fase1).SPMProperties.heatCapacityCp.GetValueOrDefault
            Else
                CType(_copp, ICapeThermoPropertyRoutine).CalcSinglePhaseProp(New String() {"heatCapacity"}, phase)
                Return Me.CurrentMaterialStream.Fases(fase1).SPMProperties.heatCapacityCp.GetValueOrDefault
            End If

            Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature = tant
            Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure = pant

        End Function

        Public Overrides Function DW_CalcEnergiaMistura_ISOL(ByVal T As Double, ByVal P As Double) As Double

            'do nothing

        End Function

        Public Overrides Function DW_CalcK_ISOL(ByVal fase1 As DWSIM.SimulationObjects.PropertyPackages.Fase, ByVal T As Double, ByVal P As Double) As Double

            Dim res As Double = 0.0#, phase As String = "", tant As Double, pant As Double

            Select Case fase1
                Case Fase.Vapor
                    phase = "Vapor"
                Case Fase.Liquid
                    phase = "Liquid"
            End Select

            tant = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault
            pant = Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure.GetValueOrDefault

            Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature = T
            Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure = P

            If _coversion = "1.0" Then
                CType(_copp, ICapeThermoCalculationRoutine).CalcProp(Me.CurrentMaterialStream, New String() {"thermalConductivity"}, New String() {phase}, "Mixture")
                Return Me.CurrentMaterialStream.Fases(fase1).SPMProperties.thermalConductivity.GetValueOrDefault
            Else
                CType(_copp, ICapeThermoPropertyRoutine).CalcSinglePhaseProp(New String() {"thermalConductivity"}, phase)
                Return Me.CurrentMaterialStream.Fases(fase1).SPMProperties.thermalConductivity.GetValueOrDefault
            End If

            Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature = tant
            Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure = pant

        End Function

        Public Overrides Function DW_CalcMassaEspecifica_ISOL(ByVal fase1 As DWSIM.SimulationObjects.PropertyPackages.Fase, ByVal T As Double, ByVal P As Double, Optional ByVal pvp As Double = 0) As Double

            Dim res As Double = 0.0#, phase As String = "", tant As Double, pant As Double

            Select Case fase1
                Case Fase.Vapor
                    phase = "Vapor"
                Case Fase.Liquid
                    phase = "Liquid"
            End Select

            tant = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault
            pant = Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure.GetValueOrDefault

            Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature = T
            Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure = P

            If _coversion = "1.0" Then
                CType(_copp, ICapeThermoCalculationRoutine).CalcProp(Me.CurrentMaterialStream, New String() {"density"}, New String() {phase}, "Mixture")
                Return Me.CurrentMaterialStream.Fases(fase1).SPMProperties.density.GetValueOrDefault
            Else
                CType(_copp, ICapeThermoPropertyRoutine).CalcSinglePhaseProp(New String() {"density"}, phase)
                Return Me.CurrentMaterialStream.Fases(fase1).SPMProperties.density.GetValueOrDefault
            End If

            Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature = tant
            Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure = pant

        End Function

        Public Overrides Function DW_CalcMM_ISOL(ByVal fase1 As DWSIM.SimulationObjects.PropertyPackages.Fase, ByVal T As Double, ByVal P As Double) As Double
            Return Me.AUX_MMM(fase1)
        End Function

        Public Overrides Sub DW_CalcOverallProps()
            MyBase.DW_CalcOverallProps()
        End Sub

        Public Overrides Sub DW_CalcPhaseProps(ByVal fase As DWSIM.SimulationObjects.PropertyPackages.Fase)

            Dim phase As String = ""
            Dim result As Double
            Dim phasemolarfrac As Double = Nothing
            Dim overallmolarflow As Double = Nothing
            Dim i As Integer
            Dim phaseID As Integer

            Select Case fase
                Case PropertyPackages.Fase.Aqueous
                    phaseID = 6
                Case PropertyPackages.Fase.Liquid
                    phaseID = 1
                Case PropertyPackages.Fase.Liquid1
                    phaseID = 3
                Case PropertyPackages.Fase.Liquid2
                    phaseID = 4
                Case PropertyPackages.Fase.Liquid3
                    phaseID = 5
                Case PropertyPackages.Fase.Mixture
                    phaseID = 0
                Case PropertyPackages.Fase.Solid
                    phaseID = 7
                Case PropertyPackages.Fase.Vapor
                    phaseID = 2
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
            End If

            Select Case fase
                Case fase.Mixture
                    phase = "Overall"
                Case Else
                    For Each pin As PhaseInfo In Me.PhaseMappings.Values
                        If pin.DWPhaseID = fase Then
                            phase = pin.PhaseLabel
                            Exit For
                        End If
                    Next
            End Select

            Dim proplist As String()

            If _coversion = "1.0" Then
                If phase <> "Overall" And phase <> "" Then
                    If fase <> PropertyPackages.Fase.Liquid Then
                        proplist = Me.GetPropList
                        For i = 0 To UBound(proplist) - 1
                            If Not proplist(i).ToLower.Contains(".d") Then
                                Try
                                    Me.CalcProp(Me.CurrentMaterialStream, New String() {proplist(i)}, New String() {phase}, "Mixture")
                                Catch ex As Exception
                                End Try
                            End If
                        Next
                    Else

                    End If
                End If
            Else
                If phase <> "Overall" And phase <> "" Then
                    proplist = Me.GetSinglePhasePropList
                    For i = 0 To UBound(proplist) - 1
                        If Not proplist(i).Contains(".D") Then
                            Try
                                Me.CalcSinglePhaseProp(New String() {proplist(i)}, phase)
                            Catch ex As Exception
                            End Try
                        End If
                    Next
                    result = overallmolarflow * phasemolarfrac * Me.AUX_MMM(fase) / 1000 / Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.density.GetValueOrDefault
                    Me.CurrentMaterialStream.Fases(phaseID).SPMProperties.volumetric_flow = result
                End If
            End If

            If phase = "Overall" Then

                Me.DW_CalcOverallProps()

            ElseIf fase = PropertyPackages.Fase.Liquid Then

                Me.DW_CalcLiqMixtureProps()

            End If

        End Sub

        Public Overrides Function DW_CalcPVAP_ISOL(ByVal T As Double) As Double
            Return Me.m_props.Pvp_leekesler(T, Me.RET_VTC(Fase.Liquid), Me.RET_VPC(Fase.Liquid), Me.RET_VW(Fase.Liquid))
        End Function

        Public Overrides Function DW_CalcTensaoSuperficial_ISOL(ByVal fase1 As DWSIM.SimulationObjects.PropertyPackages.Fase, ByVal T As Double, ByVal P As Double) As Double

            Dim res As Double = 0.0#, phase As String = "", tant As Double, pant As Double

            tant = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault
            pant = Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure.GetValueOrDefault

            Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature = T
            Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure = P

            If _coversion = "1.0" Then
                CType(_copp, ICapeThermoCalculationRoutine).CalcProp(Me.CurrentMaterialStream, New String() {"surfaceTension"}, New String() {phase}, "Mixture")
                Return Me.CurrentMaterialStream.Fases(fase1).TPMProperties.surfaceTension.GetValueOrDefault
            Else
                CType(_copp, ICapeThermoPropertyRoutine).CalcTwoPhaseProp(New String() {"surfaceTension"}, New String() {"VaporLiquid"})
                Return Me.CurrentMaterialStream.Fases(fase1).TPMProperties.surfaceTension.GetValueOrDefault
            End If

            Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature = tant
            Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure = pant

        End Function

        Public Overrides Sub DW_CalcTwoPhaseProps(ByVal fase1 As DWSIM.SimulationObjects.PropertyPackages.Fase, ByVal fase2 As DWSIM.SimulationObjects.PropertyPackages.Fase)

        End Sub

        Public Overrides Sub DW_CalcVazaoMassica()
            With Me.CurrentMaterialStream
                .Fases(0).SPMProperties.massflow = .Fases(0).SPMProperties.molarflow.GetValueOrDefault * Me.AUX_MMM(Fase.Mixture) / 1000
            End With
        End Sub

        Public Overrides Sub DW_CalcVazaoMolar()
            With Me.CurrentMaterialStream
                .Fases(0).SPMProperties.molarflow = .Fases(0).SPMProperties.massflow.GetValueOrDefault / Me.AUX_MMM(Fase.Mixture) * 1000
            End With
        End Sub

        Public Overrides Sub DW_CalcVazaoVolumetrica()
            With Me.CurrentMaterialStream
                .Fases(0).SPMProperties.volumetric_flow = .Fases(0).SPMProperties.massflow.GetValueOrDefault / .Fases(0).SPMProperties.density.GetValueOrDefault
            End With
        End Sub

        Public Overrides Function DW_CalcViscosidadeDinamica_ISOL(ByVal fase1 As DWSIM.SimulationObjects.PropertyPackages.Fase, ByVal T As Double, ByVal P As Double) As Double

            Dim res As Double = 0.0#, phase As String = "", tant As Double, pant As Double

            Select Case fase1
                Case Fase.Vapor
                    phase = Me.PhaseMappings("Vapor").PhaseLabel
                Case Fase.Liquid
                    phase = Me.PhaseMappings("Liquid1").PhaseLabel
            End Select

            tant = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault
            pant = Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure.GetValueOrDefault

            Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature = T
            Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure = P

            If _coversion = "1.0" Then
                CType(_copp, ICapeThermoCalculationRoutine).CalcProp(Me.CurrentMaterialStream, New String() {"viscosity"}, New String() {phase}, "Mixture")
                Return Me.CurrentMaterialStream.Fases(fase1).SPMProperties.viscosity.GetValueOrDefault
            Else
                CType(_copp, ICapeThermoPropertyRoutine).CalcSinglePhaseProp(New String() {"viscosity"}, phase)
                Return Me.CurrentMaterialStream.Fases(fase1).SPMProperties.viscosity.GetValueOrDefault
            End If

            Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature = tant
            Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure = pant

        End Function

        Public Overrides Function SupportsComponent(ByVal comp As ClassesBasicasTermodinamica.ConstantProperties) As Boolean

        End Function

        Public Overrides Function DW_CalcEnthalpy(ByVal Vx As System.Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double

            Dim res As Double = 0.0#, phase As String = "", pid As Integer = 0
            Dim pstr As MaterialStream = Me.CurrentMaterialStream
            Dim tstr As MaterialStream = Me.CurrentMaterialStream.Clone

            Me.CurrentMaterialStream = tstr

            Select Case st
                Case State.Vapor
                    phase = Me.PhaseMappings("Vapor").PhaseLabel
                    pid = Me.PhaseMappings("Vapor").DWPhaseIndex
                    Me.CurrentMaterialStream.SetOverallComposition(Vx)
                    Me.CurrentMaterialStream.SetPhaseComposition(Vx, Me.PhaseMappings("Vapor").DWPhaseID)
                Case State.Liquid
                    phase = Me.PhaseMappings("Liquid1").PhaseLabel
                    pid = Me.PhaseMappings("Liquid1").DWPhaseIndex
                    Me.CurrentMaterialStream.SetOverallComposition(Vx)
                    Me.CurrentMaterialStream.SetPhaseComposition(Vx, Me.PhaseMappings("Liquid1").DWPhaseID)
            End Select

            Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature = T
            Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure = P

            If _coversion = "1.0" Then
                Try
                    CType(_copp, ICapeThermoPropertyPackage).CalcProp(Me.CurrentMaterialStream, New String() {"enthalpy"}, New String() {phase}, "Mixture")
                Catch ex As Exception
                    Dim ecu As CapeOpen.ECapeUser = _copp
                    Me.CurrentMaterialStream.Flowsheet.WriteToLog(Me.ComponentName & ": CAPE-OPEN Exception " & ecu.code & " at " & ecu.interfaceName & "." & ecu.scope & ". Reason: " & ecu.description, Color.Red, FormClasses.TipoAviso.Erro)
                End Try
                Return Me.CurrentMaterialStream.Fases(pid).SPMProperties.enthalpy.GetValueOrDefault
            Else
                Try
                    CType(_copp, ICapeThermoPropertyRoutine).CalcSinglePhaseProp(New String() {"enthalpy"}, phase)
                Catch ex As Exception
                    Dim ecu As CapeOpen.ECapeUser = _copp
                    Me.CurrentMaterialStream.Flowsheet.WriteToLog(Me.ComponentName & ": CAPE-OPEN Exception " & ecu.code & " at " & ecu.interfaceName & "." & ecu.scope & ". Reason: " & ecu.description, Color.Red, FormClasses.TipoAviso.Erro)
                End Try
                Return Me.CurrentMaterialStream.Fases(pid).SPMProperties.enthalpy.GetValueOrDefault
            End If

            Me.CurrentMaterialStream = pstr
            tstr = Nothing

        End Function

        Public Overrides Function DW_CalcEnthalpyDeparture(ByVal Vx As System.Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double

            Dim res As Double = 0.0#, phase As String = "", pid As Integer = 0
            Dim pstr As MaterialStream = Me.CurrentMaterialStream
            Dim tstr As MaterialStream = Me.CurrentMaterialStream.Clone

            Me.CurrentMaterialStream = tstr

            Select Case st
                Case State.Vapor
                    phase = Me.PhaseMappings("Vapor").PhaseLabel
                    pid = Me.PhaseMappings("Vapor").DWPhaseIndex
                    Me.CurrentMaterialStream.SetOverallComposition(Vx)
                    Me.CurrentMaterialStream.SetPhaseComposition(Vx, Me.PhaseMappings("Vapor").DWPhaseID)
                Case State.Liquid
                    phase = Me.PhaseMappings("Liquid1").PhaseLabel
                    pid = Me.PhaseMappings("Liquid1").DWPhaseIndex
                    Me.CurrentMaterialStream.SetOverallComposition(Vx)
                    Me.CurrentMaterialStream.SetPhaseComposition(Vx, Me.PhaseMappings("Liquid1").DWPhaseID)
            End Select

            Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature = T
            Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure = P

            If _coversion = "1.0" Then
                Try
                    CType(_copp, ICapeThermoPropertyPackage).CalcProp(Me.CurrentMaterialStream, New String() {"excessEnthalpy"}, New String() {phase}, "Mixture")
                Catch ex As Exception
                    Dim ecu As CapeOpen.ECapeUser = _copp
                    Me.CurrentMaterialStream.Flowsheet.WriteToLog(Me.ComponentName & ": CAPE-OPEN Exception " & ecu.code & " at " & ecu.interfaceName & "." & ecu.scope & ". Reason: " & ecu.description, Color.Red, FormClasses.TipoAviso.Erro)
                End Try
                Return Me.CurrentMaterialStream.Fases(pid).SPMProperties.excessEnthalpy.GetValueOrDefault
            Else
                Try
                    CType(_copp, ICapeThermoPropertyRoutine).CalcSinglePhaseProp(New String() {"excessEnthalpy"}, phase)
                Catch ex As Exception
                    Dim ecu As CapeOpen.ECapeUser = _copp
                    Me.CurrentMaterialStream.Flowsheet.WriteToLog(Me.ComponentName & ": CAPE-OPEN Exception " & ecu.code & " at " & ecu.interfaceName & "." & ecu.scope & ". Reason: " & ecu.description, Color.Red, FormClasses.TipoAviso.Erro)
                End Try
                Return Me.CurrentMaterialStream.Fases(pid).SPMProperties.excessEnthalpy.GetValueOrDefault
            End If

            Me.CurrentMaterialStream = pstr
            tstr = Nothing

        End Function

        Public Overrides Function DW_CalcEntropy(ByVal Vx As System.Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double

            Dim res As Double = 0.0#, phase As String = "", pid As Integer = 0
            Dim pstr As MaterialStream = Me.CurrentMaterialStream
            Dim tstr As MaterialStream = Me.CurrentMaterialStream.Clone

            Me.CurrentMaterialStream = tstr

            Select Case st
                Case State.Vapor
                    phase = Me.PhaseMappings("Vapor").PhaseLabel
                    pid = Me.PhaseMappings("Vapor").DWPhaseIndex
                    Me.CurrentMaterialStream.SetOverallComposition(Vx)
                    Me.CurrentMaterialStream.SetPhaseComposition(Vx, Me.PhaseMappings("Vapor").DWPhaseID)
                Case State.Liquid
                    phase = Me.PhaseMappings("Liquid1").PhaseLabel
                    pid = Me.PhaseMappings("Liquid1").DWPhaseIndex
                    Me.CurrentMaterialStream.SetOverallComposition(Vx)
                    Me.CurrentMaterialStream.SetPhaseComposition(Vx, Me.PhaseMappings("Liquid1").DWPhaseID)
            End Select

            Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature = T
            Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure = P
            If _coversion = "1.0" Then
                Try
                    CType(_copp, ICapeThermoPropertyPackage).CalcProp(Me.CurrentMaterialStream, New String() {"entropy"}, New String() {phase}, "Mixture")
                Catch ex As Exception
                    Dim ecu As CapeOpen.ECapeUser = _copp
                    Me.CurrentMaterialStream.Flowsheet.WriteToLog(Me.ComponentName & ": CAPE-OPEN Exception " & ecu.code & " at " & ecu.interfaceName & "." & ecu.scope & ". Reason: " & ecu.description, Color.Red, FormClasses.TipoAviso.Erro)
                End Try
                Return Me.CurrentMaterialStream.Fases(pid).SPMProperties.entropy.GetValueOrDefault
            Else
                Try
                    CType(_copp, ICapeThermoPropertyRoutine).CalcSinglePhaseProp(New String() {"entropy"}, phase)
                Catch ex As Exception
                    Dim ecu As CapeOpen.ECapeUser = _copp
                    Me.CurrentMaterialStream.Flowsheet.WriteToLog(Me.ComponentName & ": CAPE-OPEN Exception " & ecu.code & " at " & ecu.interfaceName & "." & ecu.scope & ". Reason: " & ecu.description, Color.Red, FormClasses.TipoAviso.Erro)
                End Try
                Return Me.CurrentMaterialStream.Fases(pid).SPMProperties.entropy.GetValueOrDefault
            End If

            Me.CurrentMaterialStream = pstr
            tstr = Nothing

        End Function

        Public Overrides Function DW_CalcEntropyDeparture(ByVal Vx As System.Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double

            Dim res As Double = 0.0#, phase As String = "", pid As Integer = 0
            Dim pstr As MaterialStream = Me.CurrentMaterialStream
            Dim tstr As MaterialStream = Me.CurrentMaterialStream.Clone

            Me.CurrentMaterialStream = tstr

            Select Case st
                Case State.Vapor
                    phase = Me.PhaseMappings("Vapor").PhaseLabel
                    pid = Me.PhaseMappings("Vapor").DWPhaseIndex
                    Me.CurrentMaterialStream.SetOverallComposition(Vx)
                    Me.CurrentMaterialStream.SetPhaseComposition(Vx, Me.PhaseMappings("Vapor").DWPhaseID)
                Case State.Liquid
                    phase = Me.PhaseMappings("Liquid1").PhaseLabel
                    pid = Me.PhaseMappings("Liquid1").DWPhaseIndex
                    Me.CurrentMaterialStream.SetOverallComposition(Vx)
                    Me.CurrentMaterialStream.SetPhaseComposition(Vx, Me.PhaseMappings("Liquid1").DWPhaseID)
            End Select

            Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature = T
            Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure = P

            If _coversion = "1.0" Then
                Try
                    CType(_copp, ICapeThermoPropertyPackage).CalcProp(Me.CurrentMaterialStream, New String() {"excessEntropy"}, New String() {phase}, "Mixture")
                Catch ex As Exception
                    Dim ecu As CapeOpen.ECapeUser = _copp
                    Me.CurrentMaterialStream.Flowsheet.WriteToLog(Me.ComponentName & ": CAPE-OPEN Exception " & ecu.code & " at " & ecu.interfaceName & "." & ecu.scope & ". Reason: " & ecu.description, Color.Red, FormClasses.TipoAviso.Erro)
                End Try
                Return Me.CurrentMaterialStream.Fases(pid).SPMProperties.excessEntropy.GetValueOrDefault
            Else
                Try
                    CType(_copp, ICapeThermoPropertyRoutine).CalcSinglePhaseProp(New String() {"excessEntropy"}, phase)
                Catch ex As Exception
                    Dim ecu As CapeOpen.ECapeUser = _copp
                    Me.CurrentMaterialStream.Flowsheet.WriteToLog(Me.ComponentName & ": CAPE-OPEN Exception " & ecu.code & " at " & ecu.interfaceName & "." & ecu.scope & ". Reason: " & ecu.description, Color.Red, FormClasses.TipoAviso.Erro)
                End Try
                Return Me.CurrentMaterialStream.Fases(pid).SPMProperties.excessEntropy.GetValueOrDefault
            End If

            Me.CurrentMaterialStream = pstr
            tstr = Nothing

        End Function

        Public Overrides Function DW_CalcCv_ISOL(ByVal fase1 As Fase, ByVal T As Double, ByVal P As Double) As Double

            Dim res As Double = 0.0#, phase As String = "", tant As Double, pant As Double

            Select Case fase1
                Case Fase.Vapor
                    phase = "Vapor"
                Case Fase.Liquid
                    phase = "Liquid"
            End Select

            tant = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault
            pant = Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure.GetValueOrDefault

            Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature = T
            Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure = P

            If _coversion = "1.0" Then
                CType(_copp, ICapeThermoCalculationRoutine).CalcProp(Me.CurrentMaterialStream, New String() {"heatCapacityCv"}, New String() {phase}, "Mixture")
                Return Me.CurrentMaterialStream.Fases(fase1).SPMProperties.heatCapacityCp.GetValueOrDefault
            Else
                CType(_copp, ICapeThermoPropertyRoutine).CalcSinglePhaseProp(New String() {"heatCapacityCv"}, phase)
                Return Me.CurrentMaterialStream.Fases(fase1).SPMProperties.heatCapacityCp.GetValueOrDefault
            End If

            Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature = tant
            Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure = pant

        End Function

        Public Overrides Sub DW_CalcCompPartialVolume(ByVal phase As Fase, ByVal T As Double, ByVal P As Double)



        End Sub

        Public Overrides Function AUX_MMM(Vz() As Double) As Double

            Dim complist As Object = Nothing
            Me.GetCompoundList(complist, Nothing, Nothing, Nothing, Nothing, Nothing)
            Dim mw = Me.CurrentMaterialStream.GetCompoundConstant(New String() {"molecularWeight"}, complist)
            Dim val As Double = 0.0#
            Dim subst As DWSIM.ClassesBasicasTermodinamica.Substancia
            Dim i As Integer = 0
            For Each subst In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                val += Vz(i) * mw(i)
                i += 1
            Next

            Return val

        End Function

        Public Overrides Function AUX_MMM(fase As Fase) As Double

            Dim complist As Object = Nothing
            Me.GetCompoundList(complist, Nothing, Nothing, Nothing, Nothing, Nothing)
            Dim mw = Me.CurrentMaterialStream.GetCompoundConstant(New String() {"molecularWeight"}, complist)
            Dim mwt As Double = 0.0#
            Dim i As Integer = 0
            For Each c As Substancia In Me.CurrentMaterialStream.Fases(Me.RET_PHASEID(fase)).Componentes.Values
                mwt += c.FracaoMolar.GetValueOrDefault * mw(i)
                i += 1
            Next

            Return mwt

        End Function

        Public Overrides Function AUX_CONVERT_MOL_TO_MASS(ByVal subst As String, ByVal phasenumber As Integer) As Double

            Dim complist As Object = Nothing
            Me.GetCompoundList(complist, Nothing, Nothing, Nothing, Nothing, Nothing)
            Dim mw = Me.CurrentMaterialStream.GetCompoundConstant(New String() {"molecularWeight"}, complist)

            Dim mol_x_mm As Double
            Dim sub1 As DWSIM.ClassesBasicasTermodinamica.Substancia
            Dim i As Integer = 0
            Dim j As Integer = 0
            For Each sub1 In Me.CurrentMaterialStream.Fases(phasenumber).Componentes.Values
                mol_x_mm += sub1.FracaoMolar.GetValueOrDefault * mw(i)
                If subst = sub1.Nome Then j = i
                i += 1
            Next

            sub1 = Me.CurrentMaterialStream.Fases(phasenumber).Componentes(subst)
            If mol_x_mm <> 0.0# Then
                Return sub1.FracaoMolar.GetValueOrDefault * mw(j) / mol_x_mm
            Else
                Return 0.0#
            End If

        End Function

        Public Overrides Function AUX_CONVERT_MASS_TO_MOL(ByVal subst As String, ByVal phasenumber As Integer) As Double

            Dim complist As Object = Nothing
            Me.GetCompoundList(complist, Nothing, Nothing, Nothing, Nothing, Nothing)
            Dim mw = Me.CurrentMaterialStream.GetCompoundConstant(New String() {"molecularWeight"}, complist)

            Dim mass_div_mm As Double
            Dim sub1 As DWSIM.ClassesBasicasTermodinamica.Substancia
            Dim i As Integer = 0
            Dim j As Integer = 0
            For Each sub1 In Me.CurrentMaterialStream.Fases(phasenumber).Componentes.Values
                mass_div_mm += sub1.FracaoMassica.GetValueOrDefault / mw(i)
                If subst = sub1.Nome Then j = i
                i += 1
            Next

            sub1 = Me.CurrentMaterialStream.Fases(phasenumber).Componentes(subst)
            Return sub1.FracaoMassica.GetValueOrDefault / mw(j) / mass_div_mm

        End Function

        Public Overrides Function AUX_CONVERT_MOL_TO_MASS(ByVal Vz As Object) As Double()

            Dim complist As Object = Nothing
            Me.GetCompoundList(complist, Nothing, Nothing, Nothing, Nothing, Nothing)
            Dim mw = Me.CurrentMaterialStream.GetCompoundConstant(New String() {"molecularWeight"}, complist)

            Dim Vwe(UBound(Vz)) As Double
            Dim mol_x_mm As Double = 0.0#
            Dim i As Integer = 0
            For i = 0 To UBound(Vz)
                mol_x_mm += Vz(i) * mw(i)
            Next

            For i = 0 To UBound(Vz)
                If mol_x_mm <> 0 Then
                    Vwe(i) = Vz(i) * mw(i) / mol_x_mm
                Else
                    Vwe(i) = 0.0#
                End If
            Next

            Return Vwe

        End Function

        Public Overrides Function AUX_CONVERT_MASS_TO_MOL(ByVal Vz As Object) As Double()

            Dim complist As Object = Nothing
            Me.GetCompoundList(complist, Nothing, Nothing, Nothing, Nothing, Nothing)
            Dim mw = Me.CurrentMaterialStream.GetCompoundConstant(New String() {"molecularWeight"}, complist)

            Dim Vw(UBound(Vz)) As Double
            Dim mass_div_mm As Double
            Dim i As Integer = 0
           For i = 0 To UBound(Vz)
                mass_div_mm += Vz(i) / mw(i)
            Next

            For i = 0 To UBound(Vz)
                Vw(i) = Vz(i) / mw(i) / mass_div_mm
            Next

            Return Vw

        End Function

        Public Overrides Function AUX_VAPDENS(ByVal T As Double, ByVal P As Double) As Double

            Dim res As Double = 0.0#, phase As String = "Vapor", tant As Double, pant As Double

            tant = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault
            pant = Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature.GetValueOrDefault

            Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature = T
            Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure = P

            If _coversion = "1.0" Then
                Try
                    Me.CalcProp(Me.CurrentMaterialStream, New String() {"density"}, New String() {"Vapor"}, "Mixture")
                Catch ex As Exception
                    Me.CalcProp(Me.CurrentMaterialStream, New String() {"volume"}, New String() {"Vapor"}, "Mixture")
                End Try
                Return Me.CurrentMaterialStream.Fases(2).SPMProperties.density.GetValueOrDefault
            Else
                If CType(_copp, ICapeThermoPropertyRoutine).CheckSinglePhasePropSpec("density", "Vapor") Then
                    Me.CalcSinglePhaseProp(New String() {"density"}, "Vapor")
                Else
                    Me.CalcSinglePhaseProp(New String() {"volume"}, "Vapor")
                End If
                Return Me.CurrentMaterialStream.Fases(2).SPMProperties.density.GetValueOrDefault
            End If

            Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature = tant
            Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure = pant

        End Function

        Public Overrides Function DW_CalcFugCoeff(ByVal Vx As System.Array, ByVal T As Double, ByVal P As Double, ByVal st As State) As Double()

            Dim res As Double = 0.0#, phase As String = "", pid As Integer = 0
            Dim pstr As MaterialStream = Me.CurrentMaterialStream
            Dim tstr As MaterialStream = Me.CurrentMaterialStream.Clone

            Me.CurrentMaterialStream = tstr

            Select Case st
                Case State.Vapor
                    phase = Me.PhaseMappings("Vapor").PhaseLabel
                    pid = Me.PhaseMappings("Vapor").DWPhaseIndex
                    Me.CurrentMaterialStream.SetPhaseComposition(Vx, Me.PhaseMappings("Vapor").DWPhaseID)
                Case State.Liquid
                    phase = Me.PhaseMappings("Liquid1").PhaseLabel
                    pid = Me.PhaseMappings("Liquid1").DWPhaseIndex
                    Me.CurrentMaterialStream.SetPhaseComposition(Vx, Me.PhaseMappings("Liquid1").DWPhaseID)
            End Select

            Me.CurrentMaterialStream.Fases(0).SPMProperties.temperature = T
            Me.CurrentMaterialStream.Fases(0).SPMProperties.pressure = P

            Dim lnphi As Object = Nothing
            Dim lnphidt As Object = Nothing
            Dim lnphidp As Object = Nothing
            Dim lnphidn As Object = Nothing

            If _coversion = "1.0" Then
                Try
                    CType(_copp, ICapeThermoPropertyPackage).CalcProp(Me.CurrentMaterialStream, New String() {"fugacityCoefficient"}, New String() {phase}, "Mixture")
                Catch ex As Exception
                    Dim ecu As CapeOpen.ECapeUser = _copp
                    Me.CurrentMaterialStream.Flowsheet.WriteToLog(Me.ComponentName & ": CAPE-OPEN Exception " & ecu.code & " at " & ecu.interfaceName & "." & ecu.scope & ". Reason: " & ecu.description, Color.Red, FormClasses.TipoAviso.Erro)
                End Try
                Dim n As Integer = Me.CurrentMaterialStream.Fases(pid).Componentes.Count - 1
                Dim i As Integer = 0
                Dim fugcoeff(n) As Double
                For Each c As Substancia In Me.CurrentMaterialStream.Fases(pid).Componentes.Values
                    fugcoeff(i) = c.FugacityCoeff.GetValueOrDefault
                    i += 1
                Next
                Return fugcoeff
            Else
                Try
                    Me.CalcAndGetLnPhi(phase, T, P, Vx, 1, lnphi, lnphidt, lnphidp, lnphidn)
                Catch ex As Exception
                    Dim ecu As CapeOpen.ECapeUser = _copp
                    Me.CurrentMaterialStream.Flowsheet.WriteToLog(Me.ComponentName & ": CAPE-OPEN Exception " & ecu.code & " at " & ecu.interfaceName & "." & ecu.scope & ". Reason: " & ecu.description, Color.Red, FormClasses.TipoAviso.Erro)
                End Try
                Dim n As Integer = UBound(lnphi)
                Dim i As Integer
                Dim fugcoeff(n) As Double
                For i = 0 To n
                    fugcoeff(i) = Exp(lnphi(i))
                Next
                Return fugcoeff
            End If

            Me.CurrentMaterialStream = pstr
            tstr = Nothing

        End Function

#End Region

#Region "    CAPE-OPEN 1.0 Methods and Properties"

        Public Overrides Sub CalcEquilibrium(ByVal materialObject As Object, ByVal flashType As String, ByVal props As Object)
            CType(_copp, ICapeThermoPropertyPackage).CalcEquilibrium(materialObject, flashType, props)
        End Sub

        Public Overrides Sub CalcProp(ByVal materialObject As Object, ByVal props As Object, ByVal phases As Object, ByVal calcType As String)
            CType(_copp, ICapeThermoPropertyPackage).CalcProp(materialObject, props, phases, calcType)
        End Sub

        Public Overrides Function GetComponentConstant(ByVal materialObject As Object, ByVal props As Object) As Object
            Return CType(_copp, ICapeThermoPropertyPackage).GetComponentConstant(materialObject, props)
        End Function

        Public Overrides Sub GetComponentList(ByRef compIds As Object, ByRef formulae As Object, ByRef names As Object, ByRef boilTemps As Object, ByRef molWt As Object, ByRef casNo As Object)
            CType(_copp, ICapeThermoPropertyPackage).GetComponentList(compIds, formulae, names, boilTemps, molWt, casNo)
        End Sub

        Public Overrides Function GetPhaseList() As Object
            Return CType(_copp, ICapeThermoPropertyPackage).GetPhaseList()
        End Function

        Public Overrides Function GetPropList() As Object
            Return CType(_copp, ICapeThermoPropertyPackage).GetPropList()
        End Function

        Public Overrides Function GetUniversalConstant(ByVal materialObject As Object, ByVal props As Object) As Object
            Return CType(_copp, ICapeThermoPropertyPackage).GetUniversalConstant(materialObject, props)
        End Function

        Public Overrides Function PropCheck(ByVal materialObject As Object, ByVal props As Object) As Object
            Return CType(_copp, ICapeThermoPropertyPackage).PropCheck(materialObject, props)
        End Function

        Public Overrides Function ValidityCheck(ByVal materialObject As Object, ByVal props As Object) As Object
            Return CType(_copp, ICapeThermoPropertyPackage).ValidityCheck(materialObject, props)
        End Function

        Public Overrides Sub Edit()
            CType(_copp, ICapeUtilities).Edit()
        End Sub

        Public Overrides Sub Initialize()
            If Not _copp Is Nothing Then CType(_copp, ICapeUtilities).Initialize()
        End Sub

        Public Overrides ReadOnly Property parameters1() As Object
            Get
                Return CType(_copp, ICapeUtilities).parameters()
            End Get
        End Property

        Public Overrides WriteOnly Property simulationContext() As Object
            Set(ByVal value As Object)
                CType(_copp, ICapeUtilities).simulationContext = value
            End Set
        End Property

        Public Overrides Sub Terminate()
            CType(_copp, ICapeUtilities).Terminate()
        End Sub

        Public Overrides Sub CalcEquilibrium2(ByVal materialObject As Object, ByVal flashType As String, ByVal props As Object)
            CType(_copp, ICapeThermoPropertyPackage).CalcEquilibrium(materialObject, flashType, props)
        End Sub

        Public Overrides Sub PropCheck1(ByVal materialObject As Object, ByVal flashType As String, ByVal props As Object, ByRef valid As Object)
            CType(_copp, ICapeThermoEquilibriumServer).PropCheck(materialObject, flashType, props, valid)
        End Sub

        Public Overrides Sub PropList(ByRef flashType As Object, ByRef props As Object, ByRef phases As Object, ByRef calcType As Object)
            CType(_copp, ICapeThermoEquilibriumServer).PropList(flashType, props, phases, calcType)
        End Sub

        Public Overrides Sub ValidityCheck1(ByVal materialObject As Object, ByVal props As Object, ByRef relList As Object)
            CType(_copp, ICapeThermoEquilibriumServer).ValidityCheck(materialObject, props, relList)
        End Sub

        Public Overrides Sub CalcProp1(ByVal materialObject As Object, ByVal props As Object, ByVal phases As Object, ByVal calcType As String)
            CType(_copp, ICapeThermoCalculationRoutine).CalcProp(materialObject, props, phases, calcType)
        End Sub

        Public Overrides Function GetPropList1() As Object
            Return CType(_copp, ICapeThermoCalculationRoutine).GetPropList()
        End Function

        Public Overrides Function PropCheck2(ByVal materialObject As Object, ByVal props As Object) As Object
            Return CType(_copp, ICapeThermoCalculationRoutine).PropCheck(materialObject, props)
        End Function

        Public Overrides Function ValidityCheck2(ByVal materialObject As Object, ByVal props As Object) As Object
            Return CType(_copp, ICapeThermoCalculationRoutine).ValidityCheck(materialObject, props)
        End Function

#End Region

#Region "    CAPE-OPEN 1.1 Thermo & Physical Properties"

        Public Overrides Function GetCompoundConstant(ByVal props As Object, ByVal compIds As Object) As Object
            Me.SetMaterial(Me.CurrentMaterialStream)
            Return CType(_copp, ICapeThermoCompounds).GetCompoundConstant(props, compIds)
        End Function

        Public Overrides Sub GetCompoundList(ByRef compIds As Object, ByRef formulae As Object, ByRef names As Object, ByRef boilTemps As Object, ByRef molwts As Object, ByRef casnos As Object)
            Me.SetMaterial(Me.CurrentMaterialStream)
            CType(_copp, ICapeThermoCompounds).GetCompoundList(compIds, formulae, names, boilTemps, molwts, casnos)
        End Sub

        Public Overrides Function GetConstPropList() As Object
            Me.SetMaterial(Me.CurrentMaterialStream)
            Return CType(_copp, ICapeThermoCompounds).GetConstPropList()
        End Function

        Public Overrides Function GetNumCompounds() As Integer
            Me.SetMaterial(Me.CurrentMaterialStream)
            Return CType(_copp, ICapeThermoCompounds).GetNumCompounds()
        End Function

        Public Overrides Sub GetPDependentProperty(ByVal props As Object, ByVal pressure As Double, ByVal compIds As Object, ByRef propVals As Object)
            Me.SetMaterial(Me.CurrentMaterialStream)
            CType(_copp, ICapeThermoCompounds).GetPDependentProperty(props, pressure, compIds, propVals)
        End Sub

        Public Overrides Function GetPDependentPropList() As Object
            Me.SetMaterial(Me.CurrentMaterialStream)
            Return CType(_copp, ICapeThermoCompounds).GetPDependentPropList
        End Function

        Public Overrides Sub GetTDependentProperty(ByVal props As Object, ByVal temperature As Double, ByVal compIds As Object, ByRef propVals As Object)
            Me.SetMaterial(Me.CurrentMaterialStream)
            CType(_copp, ICapeThermoCompounds).GetTDependentProperty(props, temperature, compIds, propVals)
        End Sub

        Public Overrides Function GetTDependentPropList() As Object
            Me.SetMaterial(Me.CurrentMaterialStream)
            Return CType(_copp, ICapeThermoCompounds).GetTDependentPropList
        End Function

        Public Overrides Function GetNumPhases() As Integer
            Me.SetMaterial(Me.CurrentMaterialStream)
            Return CType(_copp, ICapeThermoPhases).GetNumPhases
        End Function

        Public Overrides Function GetPhaseInfo(ByVal phaseLabel As String, ByVal phaseAttribute As String) As Object
            Me.SetMaterial(Me.CurrentMaterialStream)
            Return CType(_copp, ICapeThermoPhases).GetPhaseInfo(phaseLabel, phaseAttribute)
        End Function

        Public Overrides Sub GetPhaseList1(ByRef phaseLabels As Object, ByRef stateOfAggregation As Object, ByRef keyCompoundId As Object)
            Me.SetMaterial(Me.CurrentMaterialStream)
            CType(_copp, ICapeThermoPhases).GetPhaseList(phaseLabels, stateOfAggregation, keyCompoundId)
        End Sub

        Public Overrides Sub CalcAndGetLnPhi(ByVal phaseLabel As String, ByVal temperature As Double, ByVal pressure As Double, ByVal moleNumbers As Object, ByVal fFlags As Integer, ByRef lnPhi As Object, ByRef lnPhiDT As Object, ByRef lnPhiDP As Object, ByRef lnPhiDn As Object)
            Me.SetMaterial(Me.CurrentMaterialStream)
            CType(_copp, ICapeThermoPropertyRoutine).CalcAndGetLnPhi(phaseLabel, temperature, pressure, moleNumbers, fFlags, lnPhi, lnPhiDT, lnPhiDP, lnPhiDn)
        End Sub

        Public Overrides Sub CalcSinglePhaseProp(ByVal props As Object, ByVal phaseLabel As String)
            Me.SetMaterial(Me.CurrentMaterialStream)
            CType(_copp, ICapeThermoPropertyRoutine).CalcSinglePhaseProp(props, phaseLabel)
        End Sub

        Public Overrides Sub CalcTwoPhaseProp(ByVal props As Object, ByVal phaseLabels As Object)
            Me.SetMaterial(Me.CurrentMaterialStream)
            CType(_copp, ICapeThermoPropertyRoutine).CalcTwoPhaseProp(props, phaseLabels)
        End Sub

        Public Overrides Function CheckSinglePhasePropSpec(ByVal [property] As String, ByVal phaseLabel As String) As Boolean
            Return CType(_copp, ICapeThermoPropertyRoutine).CheckSinglePhasePropSpec([property], phaseLabel)
        End Function

        Public Overrides Function CheckTwoPhasePropSpec(ByVal [property] As String, ByVal phaseLabels As Object) As Boolean
            Return CType(_copp, ICapeThermoPropertyRoutine).CheckTwoPhasePropSpec([property], phaseLabels)
        End Function

        Public Overrides Function GetSinglePhasePropList() As Object
            Return CType(_copp, ICapeThermoPropertyRoutine).GetSinglePhasePropList()
        End Function

        Public Overrides Function GetTwoPhasePropList() As Object
            Return CType(_copp, ICapeThermoPropertyRoutine).GetTwoPhasePropList()
        End Function

        Public Overrides Function GetUniversalConstant1(ByVal constantId As String) As Object
            Return CType(_copp, ICapeThermoUniversalConstant).GetUniversalConstant(constantId)
        End Function

        Public Overrides Function GetUniversalConstantList() As Object
            Return CType(_copp, ICapeThermoUniversalConstant).GetUniversalConstantList()
        End Function

        Public Overrides Sub CalcEquilibrium1(ByVal specification1 As Object, ByVal specification2 As Object, ByVal solutionType As String)
            Me.SetMaterial(Me.CurrentMaterialStream)

            Me.DW_ZerarPhaseProps(Fase.Vapor)
            Me.DW_ZerarPhaseProps(Fase.Liquid)
            Me.DW_ZerarPhaseProps(Fase.Liquid1)
            Me.DW_ZerarPhaseProps(Fase.Liquid2)
            Me.DW_ZerarPhaseProps(Fase.Liquid3)
            Me.DW_ZerarPhaseProps(Fase.Aqueous)
            Me.DW_ZerarPhaseProps(Fase.Solid)

            Me.CurrentMaterialStream.AtEquilibrium = False

            CType(_copp, ICapeThermoEquilibriumRoutine).CalcEquilibrium(specification1, specification2, solutionType)

            Me.CurrentMaterialStream.AtEquilibrium = True

        End Sub

        Public Overrides Function CheckEquilibriumSpec(ByVal specification1 As Object, ByVal specification2 As Object, ByVal solutionType As String) As Boolean
            CType(_copp, ICapeThermoEquilibriumRoutine).CheckEquilibriumSpec(specification1, specification2, solutionType)
        End Function

        Public Overrides Sub SetMaterial(ByVal material As Object)
            CType(_copp, ICapeThermoMaterialContext).SetMaterial(material)
        End Sub

        Public Overrides Sub UnsetMaterial()
            CType(_copp, ICapeThermoMaterialContext).UnsetMaterial()
        End Sub

#End Region

#Region "    Auxiliary Functions"

        <OnDeserialized()> Sub PersistLoad(ByVal context As System.Runtime.Serialization.StreamingContext)

            If _selts IsNot Nothing Then

                Dim contains As Boolean = False
                Dim t As Type = Nothing

                Try
                    t = Type.GetTypeFromProgID(_selts.TypeName)
                Catch ex As Exception
                    MessageBox.Show("Error creating CAPE-OPEN Thermo Server / Property Package Manager instance." & vbCrLf & ex.ToString, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try

                Try
                    _pptpl = Activator.CreateInstance(t)
                Catch ex As Exception
                    MessageBox.Show("Error creating CAPE-OPEN Property Package instance." & vbCrLf & ex.ToString, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try

                If _istrts IsNot Nothing Then
                    Dim myuo As Interfaces2.IPersistStreamInit = TryCast(_pptpl, Interfaces2.IPersistStreamInit)
                    If Not myuo Is Nothing Then
                        Try
                            _istrts.baseStream.Position = 0
                            myuo.Load(_istrts)
                        Catch ex As Exception
                        End Try
                    Else
                        Dim myuo2 As Interfaces2.IPersistStream = TryCast(_pptpl, Interfaces2.IPersistStream)
                        If myuo2 IsNot Nothing Then
                            Try
                                _istrts.baseStream.Position = 0
                                myuo2.Load(_istrts)
                            Catch ex As Exception
                            End Try
                        End If
                    End If
                End If

                If Not _pptpl Is Nothing Then

                    Dim pplist As String()

                    If _coversion = "1.0" Then
                        pplist = CType(_pptpl, ICapeThermoSystem).GetPropertyPackages
                    Else
                        pplist = CType(_pptpl, ICapeThermoPropertyPackageManager).GetPropertyPackageList
                    End If

                    For Each pp As String In pplist
                        If pp = _ppname Then
                            contains = True
                            Exit For
                        End If
                    Next

                End If

                If _coversion = "1.0" Then
                    _copp = CType(_pptpl, ICapeThermoSystem).ResolvePropertyPackage(_ppname)
                Else
                    _copp = CType(_pptpl, ICapeThermoPropertyPackageManager).GetPropertyPackage(_ppname)
                End If

                If _istrpp IsNot Nothing Then
                    Dim myuo As Interfaces2.IPersistStreamInit = TryCast(_copp, Interfaces2.IPersistStreamInit)
                    If Not myuo Is Nothing Then
                        Try
                            _istrpp.baseStream.Position = 0
                            myuo.Load(_istrpp)
                        Catch ex As Exception
                            MessageBox.Show(Me.ComponentName + ": error restoring persisted data from CAPE-OPEN Object - " + ex.Message.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        End Try
                    Else
                        Dim myuo2 As Interfaces2.IPersistStream = TryCast(_copp, Interfaces2.IPersistStream)
                        If myuo2 IsNot Nothing Then
                            Try
                                _istrpp.baseStream.Position = 0
                                myuo2.Load(_istrpp)
                            Catch ex As Exception
                                MessageBox.Show(Me.ComponentName + ": error restoring persisted data from CAPE-OPEN Object - " + ex.Message.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                            End Try
                        End If
                    End If
                End If

                Dim myuu As CapeOpen.ICapeUtilities = TryCast(_copp, CapeOpen.ICapeUtilities)
                If Not myuu Is Nothing Then
                    Try
                        myuu.Initialize()
                    Catch ex As Exception
                        Dim ecu As CapeOpen.ECapeUser = _copp
                        MessageBox.Show(Me.ComponentName + ": error initializing CAPE-OPEN Property Package - " + ex.Message.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        MessageBox.Show(Me.ComponentName & ": CAPE-OPEN Exception " & ecu.code & " at " & ecu.interfaceName & "." & ecu.scope & ". Reason: " & ecu.description)
                    End Try
                End If

            End If

        End Sub

        <OnSerializing()> Sub PersistSave(ByVal context As System.Runtime.Serialization.StreamingContext)

            'If the CAPE-OPEN Property Package doesn't implement any of the IPersist interfaces, the _istrpp variable will be null.

            If Not _pptpl Is Nothing Then
                Dim myuo As Interfaces2.IPersistStream = TryCast(_pptpl, Interfaces2.IPersistStream)
                If myuo IsNot Nothing Then
                    _istrts = New DWSIM.SimulationObjects.UnitOps.Auxiliary.CapeOpen.ComIStreamWrapper(New MemoryStream())
                    myuo.Save(_istrts, True)
                Else
                    Dim myuo2 As Interfaces2.IPersistStreamInit = TryCast(_pptpl, Interfaces2.IPersistStreamInit)
                    If myuo2 IsNot Nothing Then
                        _istrts = New DWSIM.SimulationObjects.UnitOps.Auxiliary.CapeOpen.ComIStreamWrapper(New MemoryStream())
                        myuo2.Save(_istrts, True)
                    End If
                End If
            End If

            If Not _copp Is Nothing Then
                Dim myuo As Interfaces2.IPersistStream = TryCast(_copp, Interfaces2.IPersistStream)
                If myuo IsNot Nothing Then
                    _istrpp = New DWSIM.SimulationObjects.UnitOps.Auxiliary.CapeOpen.ComIStreamWrapper(New MemoryStream())
                    myuo.Save(_istrpp, True)
                Else
                    Dim myuo2 As Interfaces2.IPersistStreamInit = TryCast(_copp, Interfaces2.IPersistStreamInit)
                    If myuo2 IsNot Nothing Then
                        _istrpp = New DWSIM.SimulationObjects.UnitOps.Auxiliary.CapeOpen.ComIStreamWrapper(New MemoryStream())
                        myuo2.Save(_istrpp, True)
                    End If
                End If
            End If

        End Sub

#End Region

        Public Overrides Function LoadData(data As System.Collections.Generic.List(Of System.Xml.Linq.XElement)) As Boolean

            Me.ComponentName = (From el As XElement In data Select el Where el.Name = "ComponentName").SingleOrDefault.Value
            Me.ComponentDescription = (From el As XElement In data Select el Where el.Name = "ComponentDescription").SingleOrDefault.Value
            Me.Tag = (From el As XElement In data Select el Where el.Name = "Tag").SingleOrDefault.Value
            Me._coversion = (From el As XElement In data Select el Where el.Name = "CAPEOPEN_Version").SingleOrDefault.Value
            Me._ppname = (From el As XElement In data Select el Where el.Name = "CAPEOPEN_PropertyPackageName").SingleOrDefault.Value

            _mappings.Clear()
            For Each xel2 As XElement In (From xel As XElement In data Select xel Where xel.Name = "CompoundMappings").Elements
                _mappings.Add(xel2.@From, xel2.@To)
            Next

            _phasemappings.Clear()
            For Each xel2 As XElement In (From xel As XElement In data Select xel Where xel.Name = "PhaseMappings").Elements
                _phasemappings.Add(xel2.@From, New PhaseInfo(xel2.@PhaseLabel, xel2.@DWPhaseIndex, [Enum].Parse(Type.GetType("DWSIM.DWSIM.SimulationObjects.PropertyPackages.Fase"), xel2.@DWPhaseID)))
            Next

            Dim pdata1 As XElement = (From el As XElement In data Select el Where el.Name = "PersistedData1").SingleOrDefault
            If Not pdata1 Is Nothing Then
                _istrts = New DWSIM.SimulationObjects.UnitOps.Auxiliary.CapeOpen.ComIStreamWrapper(New MemoryStream(Convert.FromBase64String(pdata1.Value)))
            End If

            Dim pdata2 As XElement = (From el As XElement In data Select el Where el.Name = "PersistedData2").SingleOrDefault
            If Not pdata2 Is Nothing Then
                _istrpp = New DWSIM.SimulationObjects.UnitOps.Auxiliary.CapeOpen.ComIStreamWrapper(New MemoryStream(Convert.FromBase64String(pdata2.Value)))
            End If

            Dim info As XElement = (From el As XElement In data Select el Where el.Name = "CAPEOPEN_Object_Info").SingleOrDefault
            _selts = New CapeOpenUnitOpInfo
            _selts.LoadData(info.Elements.ToList)

            PersistLoad(Nothing)

        End Function

        Public Overrides Function SaveData() As System.Collections.Generic.List(Of System.Xml.Linq.XElement)

            Dim elements As New System.Collections.Generic.List(Of System.Xml.Linq.XElement)
            Dim ci As Globalization.CultureInfo = Globalization.CultureInfo.InvariantCulture

            With elements

                .Add(New XElement("Type", Me.GetType.ToString))
                .Add(New XElement("ComponentName", ComponentName))
                .Add(New XElement("ComponentDescription", ComponentDescription))
                .Add(New XElement("Tag", Tag))
                .Add(New XElement("CAPEOPEN_Version", _coversion))
                .Add(New XElement("CAPEOPEN_PropertyPackageName", _ppname))
                .Add(New XElement("CAPEOPEN_Object_Info", _selts.SaveData().ToArray))
                .Add(New XElement("CompoundMappings"))
                For Each kvp As KeyValuePair(Of String, String) In _mappings
                    .Item(.Count - 1).Add(New XElement("CompoundMapping", New XAttribute("From", kvp.Key), New XAttribute("To", kvp.Value)))
                Next
                .Add(New XElement("PhaseMappings"))
                For Each kvp As KeyValuePair(Of String, PhaseInfo) In _phasemappings
                    .Item(.Count - 1).Add(New XElement("PhaseMapping", New XAttribute("From", kvp.Key),
                                                                        New XAttribute("DWPhaseID", kvp.Value.DWPhaseID),
                                                                        New XAttribute("DWPhaseIndex", kvp.Value.DWPhaseIndex),
                                                                        New XAttribute("PhaseLabel", kvp.Value.PhaseLabel)))
                Next

                If Not _pptpl Is Nothing Then
                    Dim myuo As Interfaces2.IPersistStream = TryCast(_pptpl, Interfaces2.IPersistStream)
                    If myuo IsNot Nothing Then
                        Dim mbs As New DWSIM.SimulationObjects.UnitOps.Auxiliary.CapeOpen.ComIStreamWrapper(New MemoryStream)
                        myuo.Save(mbs, True)
                        .Add(New XElement("PersistedData1", Convert.ToBase64String(CType(mbs.baseStream, MemoryStream).ToArray())))
                    Else
                        Dim myuo2 As Interfaces2.IPersistStreamInit = TryCast(_pptpl, Interfaces2.IPersistStreamInit)
                        If myuo2 IsNot Nothing Then
                            Dim mbs As New DWSIM.SimulationObjects.UnitOps.Auxiliary.CapeOpen.ComIStreamWrapper(New MemoryStream)
                            myuo2.Save(mbs, True)
                            .Add(New XElement("PersistedData1", Convert.ToBase64String(CType(mbs.baseStream, MemoryStream).ToArray())))
                        End If
                    End If
                End If

                If Not _copp Is Nothing Then
                    Dim myuo As Interfaces2.IPersistStream = TryCast(_copp, Interfaces2.IPersistStream)
                    If myuo IsNot Nothing Then
                        Dim mbs As New DWSIM.SimulationObjects.UnitOps.Auxiliary.CapeOpen.ComIStreamWrapper(New MemoryStream)
                        myuo.Save(mbs, True)
                        .Add(New XElement("PersistedData2", Convert.ToBase64String(CType(mbs.baseStream, MemoryStream).ToArray())))
                    Else
                        Dim myuo2 As Interfaces2.IPersistStreamInit = TryCast(_copp, Interfaces2.IPersistStreamInit)
                        If myuo2 IsNot Nothing Then
                            Dim mbs As New DWSIM.SimulationObjects.UnitOps.Auxiliary.CapeOpen.ComIStreamWrapper(New MemoryStream)
                            myuo2.Save(mbs, True)
                            .Add(New XElement("PersistedData2", Convert.ToBase64String(CType(mbs.baseStream, MemoryStream).ToArray())))
                        End If
                    End If
                End If

            End With

            Return elements

        End Function

    End Class

End Namespace
