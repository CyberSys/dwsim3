'    Heat Exchanger Calculation Routines 
'    Copyright 2008-2014 Daniel Wagner O. de Medeiros
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

Imports Microsoft.Msdn.Samples.GraphicObjects
Imports DWSIM.DWSIM.Flowsheet.FlowsheetSolver
Imports DWSIM.DWSIM.SimulationObjects.Streams
Imports DWSIM.DWSIM.SimulationObjects.UnitOps.Auxiliary.HeatExchanger
Imports System.Globalization
Imports System.Reflection
Imports System.Threading.Tasks
Imports System.Math

Namespace DWSIM.SimulationObjects.UnitOps

    Public Enum HeatExchangerCalcMode

        CalcTempHotOut = 0
        CalcTempColdOut = 1
        CalcBothTemp = 2
        CalcBothTemp_UA = 3
        CalcArea = 4
        ShellandTube_Rating = 5
        ShellandTube_CalcFoulingFactor = 6

    End Enum

    Public Enum SpecifiedTemperature
        Hot_Fluid
        Cold_Fluid
    End Enum

    Public Enum FlowDirection
        CounterCurrent
        CoCurrent
    End Enum

    Public Enum HeatExchangerType

        DoublePipe
        ShellTubes_E
        ShellTubes_F
        ShellTubes_G
        ShellTubes_H
        ShellTubes_J
        ShellTubes_K
        ShellTubes_X

    End Enum

    <System.Serializable()> Public Class HeatExchanger

        Inherits SimulationObjects_UnitOpBaseClass

        Protected m_Q As Nullable(Of Double) = 0
        Protected m_dp As Nullable(Of Double) = 0
        Protected m_OverallCoefficient As Nullable(Of Double) = 1000
        Protected m_Area As Nullable(Of Double) = 1.0#
        Protected TempHotOut As Nullable(Of Double) = 298.15#
        Protected TempColdOut As Nullable(Of Double) = 298.15#
        Protected m_tempdiff As Double = 0
        Protected FoulingFactor As Nullable(Of Double) = 0
        Protected Type As Integer
        Protected CalcMode As HeatExchangerCalcMode = HeatExchangerCalcMode.CalcBothTemp_UA
        Protected m_HotSidePressureDrop As Double = 0
        Protected m_ColdSidePressureDrop As Double = 0
        Protected m_specifiedtemperature As SpecifiedTemperature = SpecifiedTemperature.Cold_Fluid
        Protected m_flowdirection As FlowDirection = FlowDirection.CounterCurrent
        Protected m_stprops As New STHXProperties
        Protected m_f As Double = 1.0#

        Public Property STProperties() As STHXProperties
            Get
                If m_stprops Is Nothing Then m_stprops = New STHXProperties
                Return m_stprops
            End Get
            Set(value As STHXProperties)
                m_stprops = value
            End Set
        End Property

        Public Property LMTD_F() As Double
            Get
                Return m_f
            End Get
            Set(ByVal value As Double)
                m_f = value
            End Set
        End Property

        Public Property FlowDir() As FlowDirection
            Get
                Return m_flowdirection
            End Get
            Set(ByVal value As FlowDirection)
                m_flowdirection = value
            End Set
        End Property

        Public Property DefinedTemperature() As SpecifiedTemperature
            Get
                Return m_specifiedtemperature
            End Get
            Set(ByVal value As SpecifiedTemperature)
                m_specifiedtemperature = value
            End Set
        End Property

        Public Property ThermalEfficiency As Double = 0.0#
        Public Property MaxHeatExchange As Double = 0.0#

        Public Overrides Function LoadData(data As List(Of XElement)) As Boolean
            'workaround for renaming CalcBothTemp_KA calculation type to CalcBothTemp_UA
            For Each xel In data
                If xel.Name = "CalculationMode" Then
                    xel.Value = xel.Value.Replace("CalcBothTemp_KA", "CalcBothTemp_UA")
                End If
            Next
            Return MyBase.LoadData(data)
        End Function

        Public Sub New(ByVal nome As String, ByVal descricao As String)

            MyBase.CreateNew()
            m_ComponentName = nome
            m_ComponentDescription = descricao
            FillNodeItems()
            QTFillNodeItems()
            'Define the unitop type for later use.
            ObjectType = TipoObjeto.HeatExchanger
            Type = HeatExchangerType.DoublePipe

        End Sub

        Public Property CalculationMode() As HeatExchangerCalcMode
            Get
                Return Me.CalcMode
            End Get
            Set(ByVal value As HeatExchangerCalcMode)
                Me.CalcMode = value
            End Set
        End Property

        Public Property OverallCoefficient() As Nullable(Of Double)
            Get
                Return m_OverallCoefficient
            End Get
            Set(ByVal value As Nullable(Of Double))
                m_OverallCoefficient = value
            End Set
        End Property

        Public Property Area() As Nullable(Of Double)
            Get
                Return m_Area
            End Get
            Set(ByVal value As Nullable(Of Double))
                m_Area = value
            End Set
        End Property

        Public Property DeltaP() As Nullable(Of Double)
            Get
                Return m_dp
            End Get
            Set(ByVal value As Nullable(Of Double))
                m_dp = value
            End Set
        End Property

        Public Property Q() As Nullable(Of Double)
            Get
                Return m_Q
            End Get
            Set(ByVal value As Nullable(Of Double))
                m_Q = value
            End Set
        End Property

        Public Property HotSidePressureDrop() As Double
            Get
                Return m_HotSidePressureDrop
            End Get
            Set(ByVal value As Double)
                m_HotSidePressureDrop = value
            End Set
        End Property

        Public Property ColdSidePressureDrop() As Double
            Get
                Return m_ColdSidePressureDrop
            End Get
            Set(ByVal value As Double)
                m_ColdSidePressureDrop = value
            End Set
        End Property

        Public Property HotSideOutletTemperature() As Double
            Get
                Return TempHotOut
            End Get
            Set(ByVal value As Double)
                TempHotOut = value
            End Set
        End Property

        Public Property ColdSideOutletTemperature() As Double
            Get
                Return TempColdOut
            End Get
            Set(ByVal value As Double)
                TempColdOut = value
            End Set
        End Property

        Public Property LMTD() As Double
            Get
                Return m_tempdiff
            End Get
            Set(ByVal value As Double)
                m_tempdiff = value
            End Set
        End Property

        Public Sub New()
            MyBase.New()
        End Sub

        Public Overrides Sub Validate()

            MyBase.Validate()

        End Sub

        Public Overrides Function Calculate(Optional ByVal args As Object = Nothing) As Integer

            'Dim form As Global.DWSIM.FormChild = Me.Flowsheet
            Dim Ti1, Ti2, w1, w2, A, Tc1, Th1, Wc, Wh, P1, P2, Th2, Tc2, U As Double
            Dim Pc1, Ph1, Pc2, Ph2, DeltaHc, DeltaHh, H1, H2, Hc1, Hh1, Hc2, Hh2, CPC, CPH As Double
            Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
            Dim StIn0, StIn1, StOut0, StOut1, StInCold, StInHot, StOutHot, StOutCold As Streams.MaterialStream
            Dim coldidx As Integer = 0

            'Validate unitop status.
            Me.Validate()

            StIn0 = FlowSheet.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.InputConnectors(0).AttachedConnector.AttachedFrom.Name)
            StIn1 = FlowSheet.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.InputConnectors(1).AttachedConnector.AttachedFrom.Name)

            StOut0 = FlowSheet.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.OutputConnectors(0).AttachedConnector.AttachedTo.Name)
            StOut1 = FlowSheet.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.OutputConnectors(1).AttachedConnector.AttachedTo.Name)

            If DebugMode Then AppendDebugLine("Calculation mode: " & CalcMode.ToString)
            If DebugMode Then AppendDebugLine("Validating inlet stream 1...")
            StIn0.Validate()
            If DebugMode Then AppendDebugLine("Validating inlet stream 2...")
            StIn1.Validate()

            'First input stream.
            Ti1 = StIn0.Fases(0).SPMProperties.temperature.GetValueOrDefault
            w1 = StIn0.Fases(0).SPMProperties.massflow.GetValueOrDefault
            P1 = StIn0.Fases(0).SPMProperties.pressure.GetValueOrDefault
            H1 = StIn0.Fases(0).SPMProperties.enthalpy.GetValueOrDefault
            'Second input stream.
            Ti2 = StIn1.Fases(0).SPMProperties.temperature.GetValueOrDefault
            w2 = StIn1.Fases(0).SPMProperties.massflow.GetValueOrDefault
            P2 = StIn1.Fases(0).SPMProperties.pressure.GetValueOrDefault
            H2 = StIn1.Fases(0).SPMProperties.enthalpy.GetValueOrDefault

            'Let us use properties at the entrance as an initial implementation.

            If Ti1 < Ti2 Then
                'Input1 is the cold stream.
                Tc1 = Ti1
                Th1 = Ti2
                Wc = w1
                Wh = w2
                Pc1 = P1
                Ph1 = P2
                Hc1 = H1
                Hh1 = H2
                coldidx = 0
                'Identify cold and hot streams.
                StInCold = StIn0
                StInHot = StIn1
                StOutCold = StOut0
                StOutHot = StOut1
            Else
                'Input2 is the cold stream.
                Tc1 = Ti2
                Th1 = Ti1
                Wc = w2
                Wh = w1
                Pc1 = P2
                Ph1 = P1
                Hc1 = H2
                Hh1 = H1
                coldidx = 1
                'Identify cold and hot streams.
                StInCold = StIn1
                StInHot = StIn0
                StOutCold = StOut1
                StOutHot = StOut0
            End If

            Pc2 = Pc1 - ColdSidePressureDrop
            Ph2 = Ph1 - HotSidePressureDrop

            If DebugMode Then AppendDebugLine(StInCold.GraphicObject.Tag & " is the cold stream.")
            If DebugMode Then AppendDebugLine(StInHot.GraphicObject.Tag & " is the hot stream.")

            'calculate maximum theoretical heat exchange

            Dim tmpstr As MaterialStream = StOutHot.Clone

            tmpstr.PropertyPackage = StOutHot.PropertyPackage.Clone
            tmpstr.SetFlowsheet(StOutHot.FlowSheet)
            tmpstr.PropertyPackage.CurrentMaterialStream = tmpstr
            tmpstr.Fases("0").SPMProperties.temperature = Tc1
            tmpstr.Fases("0").SPMProperties.pressure = Ph2
            tmpstr.PropertyPackage.DW_CalcEquilibrium(PropertyPackages.FlashSpec.T, PropertyPackages.FlashSpec.P)
            tmpstr.Calculate(False, True)

            Dim HHx = tmpstr.Fases(0).SPMProperties.enthalpy.GetValueOrDefault

            MaxHeatExchange = Wh * (Hh1 - HHx) 'kW

            tmpstr.PropertyPackage = Nothing
            tmpstr.Dispose()
            tmpstr = Nothing

            If DebugMode Then AppendDebugLine("Maximum possible heat exchange is " & MaxHeatExchange.ToString & " kW.")

            'Copy properties from the input streams.
            StOut0.Assign(StIn0)
            StOut1.Assign(StIn1)

            CPC = StInCold.Fases(0).SPMProperties.heatCapacityCp.GetValueOrDefault
            CPH = StInHot.Fases(0).SPMProperties.heatCapacityCp.GetValueOrDefault

            Select Case CalcMode
                Case HeatExchangerCalcMode.CalcBothTemp_UA
                    Dim Qh, Qc, Qi, Q_UA, Q_old As Double
                    Dim tmp As Object
                    Dim count As Integer
                    A = Area
                    U = OverallCoefficient

                    Select Case Me.FlowDir
                        Case FlowDirection.CoCurrent
                            Tc2 = Tc1 + (Th1 - Tc1) * 0.49
                            Th2 = Th1 - (Th1 - Tc1) * 0.49
                        Case FlowDirection.CounterCurrent
                            Tc2 = Th1 - (Th1 - Tc1) / 10
                            Th2 = Tc1 + (Th1 - Tc1) / 10
                    End Select

                    'calculate maximum possible exchanged heat for both sides.

                    If DebugMode Then AppendDebugLine(String.Format("Doing a PT flash to calculate cold stream outlet enthalpy... P = {0} Pa, T = {1} K", Pc2, Tc1))

                    StInCold.PropertyPackage.CurrentMaterialStream = StInCold
                    tmp = StInCold.PropertyPackage.DW_CalcEquilibrio_ISOL(PropertyPackages.FlashSpec.T, PropertyPackages.FlashSpec.P, Tc2, Pc2, Tc1)
                    Hc2 = tmp(4)
                    Qc = Wc * (Hc2 - Hc1)

                    If DebugMode Then AppendDebugLine(String.Format("Doing a PT flash to calculate hot stream outlet enthalpy... P = {0} Pa, T = {1} K", Ph2, Th1))

                    StInHot.PropertyPackage.CurrentMaterialStream = StInHot
                    tmp = StInCold.PropertyPackage.DW_CalcEquilibrio_ISOL(PropertyPackages.FlashSpec.T, PropertyPackages.FlashSpec.P, Th2, Ph2, Th1)
                    Hh2 = tmp(4)
                    Qh = -Wh * (Hh2 - Hh1)

                    'estimate Q as minimum value

                    Qi = Min(Qc, Qh)

                    If Qi > MaxHeatExchange Then Qi = MaxHeatExchange

                    If DebugMode Then AppendDebugLine(String.Format("Initial estimate for heat exchanged is {0} kW", Qi))

                    Do

                        count += 1

                        If DebugMode Then AppendDebugLine(String.Format("Loop number {0}/100", count))

                        'calculate exit temperatures from energy balance

                        Hc2 = Qi / Wc + Hc1
                        Hh2 = Hh1 - Qi / Wh
                        If DebugMode Then AppendDebugLine(String.Format("Doing a PH flash to calculate cold stream outlet temperature... P = {0} Pa, H = {1} kJ/[kg.K]", Pc2, Hc2))
                        StInCold.PropertyPackage.CurrentMaterialStream = StInCold
                        tmp = StInCold.PropertyPackage.DW_CalcEquilibrio_ISOL(PropertyPackages.FlashSpec.P, PropertyPackages.FlashSpec.H, Pc2, Hc2, Tc2)
                        Tc2 = tmp(2)
                        If DebugMode Then AppendDebugLine(String.Format("Calculated cold stream outlet temperature T2 = {0} K", Tc2))
                        If DebugMode Then AppendDebugLine(String.Format("Doing a PH flash to calculate hot stream outlet temperature... P = {0} Pa, H = {1} kJ/[kg.K]", Ph2, Hh2))
                        StInHot.PropertyPackage.CurrentMaterialStream = StInHot
                        tmp = StInHot.PropertyPackage.DW_CalcEquilibrio_ISOL(PropertyPackages.FlashSpec.P, PropertyPackages.FlashSpec.H, Ph2, Hh2, Th2)
                        Th2 = tmp(2)
                        If DebugMode Then AppendDebugLine(String.Format("Calculated hot stream outlet temperature T2 = {0} K", Th2))

                        Select Case Me.FlowDir
                            Case FlowDirection.CoCurrent
                                If (Th1 - Tc1) / (Th2 - Tc2) = 1 Then
                                    LMTD = ((Th1 - Tc1) + (Th2 - Tc2)) / 2
                                End If
                                LMTD = ((Th1 - Tc1) - (Th2 - Tc2)) / Math.Log((Th1 - Tc1) / (Th2 - Tc2))
                            Case FlowDirection.CounterCurrent
                                If (Th1 - Tc2) / (Th2 - Tc1) = 1 Then
                                    LMTD = ((Th1 - Tc2) + (Th2 - Tc1)) / 2
                                Else
                                    LMTD = ((Th1 - Tc2) - (Th2 - Tc1)) / Math.Log((Th1 - Tc2) / (Th2 - Tc1))
                                End If
                        End Select

                        If Double.IsNaN(LMTD) Or Double.IsInfinity(LMTD) Then Throw New Exception(DWSIM.App.GetLocalString("HXCalcError"))

                        Q_UA = U / 1000 * A * LMTD 'calculate Q due to temperature difference

                        Q_old = Qi
                        Qi = Qi - (Qi - Q_UA) * 0.2

                        If Qi > MaxHeatExchange Then Qi = MaxHeatExchange

                        If DebugMode Then AppendDebugLine(String.Format("Current estimated heat exchange is {0} kW", Qi))

                    Loop Until Abs(Qi - Q_old) < 0.01 Or count > 100
                    Q = Qi

                    If count > 100 Then
                        FlowSheet.WriteToLog(Me.GraphicObject.Tag & ": Iteration loop did not converge.", Color.DarkOrange, FormClasses.TipoAviso.Aviso)
                    End If

                Case HeatExchangerCalcMode.CalcBothTemp

                    If Q > MaxHeatExchange Then Throw New Exception("Defined heat exchange is invalid (higher than the theoretical maximum).")

                    A = Area
                    DeltaHc = Q / Wc
                    DeltaHh = -Q / Wh
                    Hc2 = Hc1 + DeltaHc
                    Hh2 = Hh1 + DeltaHh

                    StInCold.PropertyPackage.CurrentMaterialStream = StInCold

                    If DebugMode Then AppendDebugLine(String.Format("Doing a PH flash to calculate cold stream outlet temperature... P = {0} Pa, H = {1} kJ/[kg.K]", Pc2, Hc2))
                    Dim tmp = StInCold.PropertyPackage.DW_CalcEquilibrio_ISOL(PropertyPackages.FlashSpec.P, PropertyPackages.FlashSpec.H, Pc2, Hc2, Tc1)
                    Tc2 = tmp(2)
                    Hh2 = Hh1 + DeltaHh
                    StInHot.PropertyPackage.CurrentMaterialStream = StInHot

                    If DebugMode Then AppendDebugLine(String.Format("Calculated cold stream outlet temperature T2 = {0} K", Tc2))
                    If DebugMode Then AppendDebugLine(String.Format("Doing a PH flash to calculate hot stream outlet temperature... P = {0} Pa, H = {1} kJ/[kg.K]", Ph2, Hh2))

                    tmp = StInHot.PropertyPackage.DW_CalcEquilibrio_ISOL(PropertyPackages.FlashSpec.P, PropertyPackages.FlashSpec.H, Ph2, Hh2, Th1)
                    Th2 = tmp(2)

                    If DebugMode Then AppendDebugLine(String.Format("Calculated hot stream outlet temperature T2 = {0} K", Th2))

                    Select Case Me.FlowDir
                        Case FlowDirection.CoCurrent
                            LMTD = ((Th1 - Tc1) - (Th2 - Tc2)) / Math.Log((Th1 - Tc1) / (Th2 - Tc2))
                        Case FlowDirection.CounterCurrent
                            LMTD = ((Th1 - Tc2) - (Th2 - Tc1)) / Math.Log((Th1 - Tc2) / (Th2 - Tc1))
                    End Select

                    If Double.IsNaN(LMTD) Or Double.IsInfinity(LMTD) Then Throw New Exception(DWSIM.App.GetLocalString("HXCalcError"))

                    U = Q / (A * LMTD) * 1000

                Case HeatExchangerCalcMode.CalcTempColdOut

                    A = Area
                    Th2 = TempHotOut

                    StInHot.PropertyPackage.CurrentMaterialStream = StInHot
                    If DebugMode Then AppendDebugLine(String.Format("Doing a PT flash to calculate hot stream outlet enthalpy... P = {0} Pa, T = K", Ph2, Th2))
                    Dim tmp = StInHot.PropertyPackage.DW_CalcEquilibrio_ISOL(PropertyPackages.FlashSpec.T, PropertyPackages.FlashSpec.P, Th2, Ph2, 0)
                    Hh2 = tmp(4)
                    Q = -Wh * (Hh2 - Hh1)

                    DeltaHc = Q / Wc
                    Hc2 = Hc1 + DeltaHc
                    StInCold.PropertyPackage.CurrentMaterialStream = StInCold
                    If DebugMode Then AppendDebugLine(String.Format("Doing a PH flash to calculate cold stream outlet temperature... P = {0} Pa, H = {1} kJ/[kg.K]", Pc2, Hc2))
                    tmp = StInCold.PropertyPackage.DW_CalcEquilibrio_ISOL(PropertyPackages.FlashSpec.P, PropertyPackages.FlashSpec.H, Pc2, Hc2, Tc1)
                    Tc2 = tmp(2)
                    If DebugMode Then AppendDebugLine(String.Format("Calculated cold stream outlet temperature T2 = {0} K", Tc2))
                    Select Case Me.FlowDir
                        Case FlowDirection.CoCurrent
                            LMTD = ((Th1 - Tc1) - (Th2 - Tc2)) / Math.Log((Th1 - Tc1) / (Th2 - Tc2))
                        Case FlowDirection.CounterCurrent
                            LMTD = ((Th1 - Tc2) - (Th2 - Tc1)) / Math.Log((Th1 - Tc2) / (Th2 - Tc1))
                    End Select

                    If Double.IsNaN(LMTD) Or Double.IsInfinity(LMTD) Then Throw New Exception(DWSIM.App.GetLocalString("HXCalcError"))

                    U = Q / (A * LMTD) * 1000

                Case HeatExchangerCalcMode.CalcTempHotOut

                    A = Area
                    Tc2 = TempColdOut
                    StInCold.PropertyPackage.CurrentMaterialStream = StInCold
                    If DebugMode Then AppendDebugLine(String.Format("Doing a PT flash to calculate cold stream outlet enthalpy... P = {0} Pa, T = K", Pc2, Tc2))
                    Dim tmp = StInCold.PropertyPackage.DW_CalcEquilibrio_ISOL(PropertyPackages.FlashSpec.T, PropertyPackages.FlashSpec.P, Tc2, Pc2, 0)
                    Hc2 = tmp(4)
                    Q = Wc * (Hc2 - Hc1)
                    DeltaHh = -Q / Wh
                    Hh2 = Hh1 + DeltaHh
                    StInHot.PropertyPackage.CurrentMaterialStream = StInHot
                    If DebugMode Then AppendDebugLine(String.Format("Doing a PH flash to calculate hot stream outlet temperature... P = {0} Pa, H = {1} kJ/[kg.K]", Ph2, Hh2))
                    tmp = StInHot.PropertyPackage.DW_CalcEquilibrio_ISOL(PropertyPackages.FlashSpec.P, PropertyPackages.FlashSpec.H, Ph2, Hh2, Th1)
                    Th2 = tmp(2)
                    If DebugMode Then AppendDebugLine(String.Format("Calculated hot stream outlet temperature T2 = {0} K", Th2))

                    Select Case Me.FlowDir
                        Case FlowDirection.CoCurrent
                            LMTD = ((Th1 - Tc1) - (Th2 - Tc2)) / Math.Log((Th1 - Tc1) / (Th2 - Tc2))
                        Case FlowDirection.CounterCurrent
                            LMTD = ((Th1 - Tc2) - (Th2 - Tc1)) / Math.Log((Th1 - Tc2) / (Th2 - Tc1))
                    End Select

                    If Double.IsNaN(LMTD) Or Double.IsInfinity(LMTD) Then Throw New Exception(DWSIM.App.GetLocalString("HXCalcError"))

                    U = Q / (A * LMTD) * 1000

                Case HeatExchangerCalcMode.CalcArea

                    Select Case Me.DefinedTemperature
                        Case SpecifiedTemperature.Cold_Fluid
                            Tc2 = TempColdOut
                            StInCold.PropertyPackage.CurrentMaterialStream = StInCold
                            If DebugMode Then AppendDebugLine(String.Format("Doing a PT flash to calculate cold stream outlet enthalpy... P = {0} Pa, T = {1} K", Pc2, Tc2))
                            Dim tmp = StInCold.PropertyPackage.DW_CalcEquilibrio_ISOL(PropertyPackages.FlashSpec.T, PropertyPackages.FlashSpec.P, Tc2, Pc2, 0)
                            Hc2 = tmp(4)
                            Q = Wc * (Hc2 - Hc1)
                            DeltaHh = -Q / Wh
                            Hh2 = Hh1 + DeltaHh
                            StInHot.PropertyPackage.CurrentMaterialStream = StInHot
                            If DebugMode Then AppendDebugLine(String.Format("Doing a PH flash to calculate hot stream outlet temperature... P = {0} Pa, H = {1} kJ/[kg.K]", Ph2, Hh2))
                            tmp = StInHot.PropertyPackage.DW_CalcEquilibrio_ISOL(PropertyPackages.FlashSpec.P, PropertyPackages.FlashSpec.H, Ph2, Hh2, 0)
                            Th2 = tmp(2)
                            If DebugMode Then AppendDebugLine(String.Format("Calculated hot stream outlet temperature T2 = {0} K", Th2))
                        Case SpecifiedTemperature.Hot_Fluid
                            Th2 = TempHotOut
                            StInHot.PropertyPackage.CurrentMaterialStream = StInHot
                            If DebugMode Then AppendDebugLine(String.Format("Doing a PT flash to calculate hot stream outlet enthalpy... P = {0} Pa, T = {1} K", Ph2, Th2))
                            Dim tmp = StInHot.PropertyPackage.DW_CalcEquilibrio_ISOL(PropertyPackages.FlashSpec.T, PropertyPackages.FlashSpec.P, Th2, Ph2, 0)
                            Hh2 = tmp(4)
                            Q = -Wh * (Hh2 - Hh1)
                            DeltaHc = Q / Wc
                            Hc2 = Hc1 + DeltaHc
                            StInCold.PropertyPackage.CurrentMaterialStream = StInCold
                            If DebugMode Then AppendDebugLine(String.Format("Doing a PH flash to calculate cold stream outlet temperature... P = {0} Pa, H = {1} kJ/[kg.K]", Pc2, Hc2))
                            tmp = StInCold.PropertyPackage.DW_CalcEquilibrio_ISOL(PropertyPackages.FlashSpec.P, PropertyPackages.FlashSpec.H, Pc2, Hc2, 0)
                            Tc2 = tmp(2)
                            If DebugMode Then AppendDebugLine(String.Format("Calculated cold stream outlet temperature T2 = {0} K", Tc2))
                    End Select
                    Select Case Me.FlowDir
                        Case FlowDirection.CoCurrent
                            LMTD = ((Th1 - Tc1) - (Th2 - Tc2)) / Math.Log((Th1 - Tc1) / (Th2 - Tc2))
                        Case FlowDirection.CounterCurrent
                            LMTD = ((Th1 - Tc2) - (Th2 - Tc1)) / Math.Log((Th1 - Tc2) / (Th2 - Tc1))
                    End Select

                    If Double.IsNaN(LMTD) Or Double.IsInfinity(LMTD) Then Throw New Exception(DWSIM.App.GetLocalString("HXCalcError"))

                    U = Me.OverallCoefficient

                    A = Q / (LMTD * U) * 1000

                Case HeatExchangerCalcMode.ShellandTube_Rating, HeatExchangerCalcMode.ShellandTube_CalcFoulingFactor

                    'Shell and Tube HX calculation using Tinker's method.

                    Dim Tc2_ant, Th2_ant As Double
                    Dim Ud, Ur, U_ant, Rf, fx, Fant, F As Double
                    Dim DTm, Tcm, Thm, R, Sf, P As Double
                    Dim cph0, cpc0, x0, q0 As Double
                    cph0 = StInHot.Fases(0).SPMProperties.heatCapacityCp.GetValueOrDefault
                    cpc0 = StInCold.Fases(0).SPMProperties.heatCapacityCp.GetValueOrDefault
                    x0 = Wh * cph0 / (Wc * cpc0)
                    If CalculationMode = HeatExchangerCalcMode.ShellandTube_Rating Then
                        Tc2 = (Tc1 + x0 * Th1) / (1 + x0)
                        q0 = Wc * cpc0 * (Tc2 - Tc1)
                        Tc2 = 0.3 * q0 / Wc / cpc0 + Tc1
                        Th2 = -0.3 * q0 / Wh / cph0 + Th1
                    Else
                        Tc2 = TempColdOut
                        Th2 = TempHotOut
                    End If
                    Pc2 = Pc1
                    Ph2 = Ph1
                    F = 1.0#
                    U = 500.0#
                    Dim icnt As Integer = 0
                    Do
                        Select Case Me.FlowDir
                            Case FlowDirection.CoCurrent
                                LMTD = ((Th1 - Tc1) - (Th2 - Tc2)) / Math.Log((Th1 - Tc1) / (Th2 - Tc2))
                            Case FlowDirection.CounterCurrent
                                LMTD = ((Th1 - Tc2) - (Th2 - Tc1)) / Math.Log((Th1 - Tc2) / (Th2 - Tc1))
                        End Select

                        If Double.IsNaN(LMTD) Or Double.IsInfinity(LMTD) Then Throw New Exception(DWSIM.App.GetLocalString("HXCalcError"))

                        Dim rhoc, muc, kc, rhoh, muh, kh, rs, rt, Atc, Nc, di, de, pitch, L, n, hi, nt, vt, Ret, Prt As Double
                        If STProperties.Tube_Fluid = 0 Then
                            'cold
                            R = (Th1 - Th2) / (Tc2 - Tc1)
                            P = (Tc2 - Tc1) / (Th1 - Tc1)
                        Else
                            'hot
                            R = (Tc1 - Tc2) / (Th2 - Th1)
                            P = (Th2 - Tc2) / (Tc1 - Th1)
                        End If
                        Fant = F
                        If R <> 1.0# Then
                            Dim alpha As Double
                            alpha = ((1 - R * P) / (1 - P)) ^ (1 / Me.STProperties.Shell_NumberOfPasses)
                            Sf = (alpha - 1) / (alpha - R)
                            F = (R ^ 2 + 1) ^ 0.5 * Math.Log((1 - Sf) / (1 - R * Sf)) / ((R - 1) * Math.Log((2 - Sf * (R + 1 - (R ^ 2 + 1) ^ 0.5)) / (2 - Sf * (R + 1 + (R ^ 2 + 1) ^ 0.5))))
                        Else
                            Sf = P / (Me.STProperties.Shell_NumberOfPasses * (1 - P) + P)
                            F = Sf * 2 ^ 0.5 / ((1 - Sf) * Math.Log((2 * (1 - Sf) + Sf * 2 ^ 0.5) / (2 * (1 - Sf) - Sf * 2 ^ 0.5)))
                        End If
                        If Double.IsNaN(F) Then
                            F = Fant
                            'Throw New Exception("LMTD correction factor 'F'  could not be calculated. R = " & R & ", S = " & Sf)
                        End If
                        DTm = F * LMTD
                        '3
                        Tcm = (Tc2 - Tc1) / 2 + Tc1
                        Thm = (Th1 - Th2) / 2 + Th2
                        '4, 5
                        StInCold.PropertyPackage.CurrentMaterialStream = StInCold
                        Dim tmp = StInCold.PropertyPackage.DW_CalcEquilibrio_ISOL(PropertyPackages.FlashSpec.T, PropertyPackages.FlashSpec.P, Tc2, Pc2, 0)
                        Hc2 = tmp(4)
                        Q = Wc * (Hc2 - Hc1)
                        Dim tms As MaterialStream = StInCold.Clone
                        tms.SetFlowsheet(StInCold.FlowSheet)
                        tms.Fases(0).SPMProperties.temperature = Tcm
                        With tms.PropertyPackage
                            .CurrentMaterialStream = tms
                            .DW_CalcEquilibrium(PropertyPackages.FlashSpec.T, PropertyPackages.FlashSpec.P)
                            If tms.Fases(3).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid1)
                            Else
                                .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid1)
                            End If
                            If tms.Fases(2).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                            Else
                                .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                            End If
                            If tms.Fases(2).SPMProperties.molarfraction.GetValueOrDefault >= 0 And tms.Fases(2).SPMProperties.molarfraction.GetValueOrDefault <= 1 Then
                                .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid)
                            Else
                                .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid)
                            End If
                            tms.PropertyPackage.DW_CalcPhaseProps(PropertyPackages.Fase.Mixture)
                        End With
                        rhoc = tms.Fases(0).SPMProperties.density.GetValueOrDefault
                        CPC = tms.Fases(0).SPMProperties.heatCapacityCp.GetValueOrDefault
                        kc = tms.Fases(0).SPMProperties.thermalConductivity.GetValueOrDefault
                        muc = tms.Fases(1).SPMProperties.viscosity.GetValueOrDefault * tms.Fases(1).SPMProperties.molarfraction.GetValueOrDefault + tms.Fases(2).SPMProperties.viscosity.GetValueOrDefault * tms.Fases(2).SPMProperties.molarfraction.GetValueOrDefault
                        tms = StInHot.Clone
                        tms.SetFlowsheet(StInHot.FlowSheet)
                        tms.Fases(0).SPMProperties.temperature = Thm
                        tms.PropertyPackage.CurrentMaterialStream = tms
                        With tms.PropertyPackage
                            .CurrentMaterialStream = tms
                            .DW_CalcEquilibrium(PropertyPackages.FlashSpec.T, PropertyPackages.FlashSpec.P)
                            If tms.Fases(3).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid1)
                            Else
                                .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid1)
                            End If
                            If tms.Fases(2).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                            Else
                                .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                            End If
                            If tms.Fases(2).SPMProperties.molarfraction.GetValueOrDefault >= 0 And tms.Fases(2).SPMProperties.molarfraction.GetValueOrDefault <= 1 Then
                                .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid)
                            Else
                                .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid)
                            End If
                            tms.PropertyPackage.DW_CalcPhaseProps(PropertyPackages.Fase.Mixture)
                        End With
                        tms.PropertyPackage.DW_CalcPhaseProps(PropertyPackages.Fase.Mixture)
                        rhoh = tms.Fases(0).SPMProperties.density.GetValueOrDefault
                        CPH = tms.Fases(0).SPMProperties.heatCapacityCp.GetValueOrDefault
                        kh = tms.Fases(0).SPMProperties.thermalConductivity.GetValueOrDefault
                        muh = tms.Fases(1).SPMProperties.viscosity.GetValueOrDefault * tms.Fases(1).SPMProperties.molarfraction.GetValueOrDefault + tms.Fases(2).SPMProperties.viscosity.GetValueOrDefault * tms.Fases(2).SPMProperties.molarfraction.GetValueOrDefault
                        '6
                        rs = Me.STProperties.Shell_Fouling
                        rt = Me.STProperties.Tube_Fouling
                        Nc = STProperties.Shell_NumberOfShellsInSeries
                        de = STProperties.Tube_De
                        di = STProperties.Tube_Di
                        L = STProperties.Tube_Length
                        pitch = STProperties.Tube_Pitch
                        n = STProperties.Tube_NumberPerShell
                        nt = n / STProperties.Tube_PassesPerShell
                        A = n * Math.PI * de * (L - 2 * de)
                        Atc = A / Nc
                        If CalculationMode = HeatExchangerCalcMode.ShellandTube_CalcFoulingFactor Then
                            Ud = Q * 1000 / (A * DTm)
                        End If
                        If STProperties.Tube_Fluid = 0 Then
                            'cold
                            vt = Wc / (rhoc * nt * Math.PI * di ^ 2 / 4)
                            Ret = rhoc * vt * di / muc
                            Prt = muc * CPC / kc * 1000
                        Else
                            'hot
                            vt = Wh / (rhoh * nt * Math.PI * di ^ 2 / 4)
                            Ret = rhoh * vt * di / muh
                            Prt = muh * CPH / kh * 1000
                        End If
                        'calcular DeltaP
                        Dim dpt, dps As Double
                        'tube
                        dpt = 0.0#
                        Dim fric As Double = 0
                        Dim epsilon As Double = STProperties.Tube_Roughness
                        If Ret > 3250 Then
                            Dim a1 = Math.Log(((epsilon / di) ^ 1.1096) / 2.8257 + (7.149 / Ret) ^ 0.8961) / Math.Log(10.0#)
                            Dim b1 = -2 * Math.Log((epsilon / di) / 3.7065 - 5.0452 * a1 / Ret) / Math.Log(10.0#)
                            fric = (1 / b1) ^ 2
                        Else
                            fric = 64 / Ret
                        End If
                        fric *= STProperties.Tube_Scaling_FricCorrFactor
                        If STProperties.Tube_Fluid = 0 Then
                            'cold
                            dpt = fric * L * STProperties.Tube_PassesPerShell / di * vt ^ 2 / 2 * rhoc
                        Else
                            'hot
                            dpt = fric * L * STProperties.Tube_PassesPerShell / di * vt ^ 2 / 2 * rhoh
                        End If
                        'tube heat transfer coeff
                        If STProperties.Tube_Fluid = 0 Then
                            'cold
                            hi = Pipe.hint_petukhov(kc, di, fric, Ret, Prt)
                        Else
                            'hot
                            hi = Pipe.hint_petukhov(kh, di, fric, Ret, Prt)
                        End If
                        'shell internal diameter
                        Dim Dsi, Dsf, nsc, HDi, Nb As Double
                        Select Case STProperties.Tube_Layout
                            Case 0, 1
                                nsc = 1.1 * n ^ 0.5
                            Case 2, 3
                                nsc = 1.19 * n ^ 0.5
                        End Select
                        Dsf = (nsc - 1) * pitch + de
                        Dsi = STProperties.Shell_Di 'Dsf / 1.075
                        'Dsf = Dsi / 1.075 * Dsi
                        HDi = STProperties.Shell_BaffleCut / 100
                        Nb = L / STProperties.Shell_BaffleSpacing + 1 'review (l1, l2)
                        'shell pressure drop
                        Dim Gsf, Np, Fp, Ss, Ssf, fs, Cb, Ca, Res, Prs, jh, aa, bb, cc, xx, yy, Nh, Y As Double
                        xx = Dsi / STProperties.Shell_BaffleSpacing
                        yy = pitch / de
                        Select Case STProperties.Tube_Layout
                            Case 0, 1
                                aa = 0.9078565328950694
                                bb = 0.66331106126564476
                                cc = -4.4329764639656482
                                Nh = aa * xx ^ bb * yy ^ cc
                                aa = 5.3718559074820611
                                bb = -0.33416765138071414
                                cc = 0.7267144209289168
                                Y = aa * xx ^ bb * yy ^ cc
                                aa = 0.53807650470841084
                                bb = 0.3761125784751041
                                cc = -3.8741224386187474
                                Np = aa * xx ^ bb * yy ^ cc
                            Case 2
                                aa = 0.84134824361715088
                                bb = 0.61374520485097339
                                cc = -4.2696318466170409
                                Nh = aa * xx ^ bb * yy ^ cc
                                aa = 4.9901814007765743
                                bb = -0.32437442510328618
                                cc = 1.084850423269188
                                Y = aa * xx ^ bb * yy ^ cc
                                aa = 0.5502379008813062
                                bb = 0.36559560225434834
                                cc = -3.99041305625483
                                Np = aa * xx ^ bb * yy ^ cc
                            Case 3
                                aa = 0.66738654406767639
                                bb = 0.680260033886211
                                cc = -4.522291113086232
                                Nh = aa * xx ^ bb * yy ^ cc
                                aa = 4.5749169651729105
                                bb = -0.32201759442337358
                                cc = 1.17295183743691
                                Y = aa * xx ^ bb * yy ^ cc
                                aa = 0.36869631130961067
                                bb = 0.38397859475813922
                                cc = -3.6273465996780421
                                Np = aa * xx ^ bb * yy ^ cc
                        End Select
                        Fp = 1 / (0.8 + Np * (Dsi / pitch) ^ 0.5)
                        Select Case STProperties.Tube_Layout
                            Case 0, 1, 2
                                Cb = 0.97
                            Case 3
                                Cb = 1.37
                        End Select
                        Ca = Cb * (pitch - de) / pitch
                        Ss = Ca * STProperties.Shell_BaffleSpacing * Dsf
                        Ssf = Ss / Fp
                        'Ssf = Math.PI / 4 * (Dsi ^ 2 - nt * de ^ 2)
                        If STProperties.Shell_Fluid = 0 Then
                            'cold
                            Gsf = Wc / Ssf
                            Res = Gsf * de / muc
                            Prs = muc * CPC / kc * 1000
                        Else
                            'hot
                            Gsf = Wh / Ssf
                            Res = Gsf * de / muh
                            Prs = muh * CPH / kh * 1000
                        End If
                        Select Case STProperties.Tube_Layout
                            Case 0, 1
                                If Res < 100 Then
                                    jh = 0.497 * Res ^ 0.54
                                Else
                                    jh = 0.378 * Res ^ 0.59
                                End If
                                If pitch / de <= 1.2 Then
                                    If Res < 100 Then
                                        fs = 276.46 * Res ^ -0.979
                                    ElseIf Res < 1000 Then
                                        fs = 30.26 * Res ^ -0.523
                                    Else
                                        fs = 2.93 * Res ^ -0.186
                                    End If
                                ElseIf pitch / de <= 1.3 Then
                                    If Res < 100 Then
                                        fs = 208.14 * Res ^ -0.945
                                    ElseIf Res < 1000 Then
                                        fs = 27.6 * Res ^ -0.525
                                    Else
                                        fs = 2.27 * Res ^ -0.163
                                    End If
                                ElseIf pitch / de <= 1.4 Then
                                    If Res < 100 Then
                                        fs = 122.73 * Res ^ -0.865
                                    ElseIf Res < 1000 Then
                                        fs = 17.82 * Res ^ -0.474
                                    Else
                                        fs = 1.86 * Res ^ -0.146
                                    End If
                                ElseIf pitch / de <= 1.5 Then
                                    If Res < 100 Then
                                        fs = 104.33 * Res ^ -0.869
                                    ElseIf Res < 1000 Then
                                        fs = 12.69 * Res ^ -0.434
                                    Else
                                        fs = 1.526 * Res ^ -0.129
                                    End If
                                End If
                            Case 2, 3
                                If Res < 100 Then
                                    If STProperties.Tube_Layout = 2 Then
                                        jh = 0.385 * Res ^ 0.526
                                    Else
                                        jh = 0.496 * Res ^ 0.54
                                    End If
                                Else
                                    If STProperties.Tube_Layout = 2 Then
                                        jh = 0.2487 * Res ^ 0.625
                                    Else
                                        jh = 0.354 * Res ^ 0.61
                                    End If
                                End If
                                If pitch / de <= 1.2 Then
                                    If Res < 100 Then
                                        fs = 230 * Res ^ -1
                                    ElseIf Res < 1000 Then
                                        fs = 16.23 * Res ^ -0.43
                                    Else
                                        fs = 2.67 * Res ^ -0.173
                                    End If
                                ElseIf pitch / de <= 1.3 Then
                                    If Res < 100 Then
                                        fs = 142.22 * Res ^ -0.949
                                    ElseIf Res < 1000 Then
                                        fs = 11.93 * Res ^ -0.43
                                    Else
                                        fs = 1.77 * Res ^ -0.144
                                    End If
                                ElseIf pitch / de <= 1.4 Then
                                    If Res < 100 Then
                                        fs = 110.77 * Res ^ -0.965
                                    ElseIf Res < 1000 Then
                                        fs = 7.524 * Res ^ -0.4
                                    Else
                                        fs = 1.01 * Res ^ -0.104
                                    End If
                                ElseIf pitch / de <= 1.5 Then
                                    If Res < 100 Then
                                        fs = 58.18 * Res ^ -0.862
                                    ElseIf Res < 1000 Then
                                        fs = 6.76 * Res ^ -0.411
                                    Else
                                        fs = 0.718 * Res ^ -0.008
                                    End If
                                End If
                        End Select
                        'Cx
                        Dim Cx As Double = 0
                        Select Case STProperties.Tube_Layout
                            Case 0, 1
                                Cx = 1.154
                            Case 2
                                Cx = 1.0#
                            Case 3
                                Cx = 1.414
                        End Select
                        Dim Gsh, Ssh, Fh, Rsh, dis As Double
                        If STProperties.Shell_Fluid = 0 Then
                            dps = 4 * fs * Gsf ^ 2 / (2 * rhoc) * Cx * (1 - HDi) * Dsi / pitch * Nb * (1 + Y * pitch / Dsi)
                        Else
                            dps = 4 * fs * Gsf ^ 2 / (2 * rhoh) * Cx * (1 - HDi) * Dsi / pitch * Nb * (1 + Y * pitch / Dsi)
                        End If
                        dps *= Nc
                        'shell htc
                        Dim M As Double = 0.96#
                        dis = STProperties.Shell_Di
                        Fh = 1 / (1 + Nh * (dis / pitch) ^ 0.5)
                        Ssh = Ss * M / Fh
                        'Ssh = Math.PI / 4 * (Dsi ^ 2 - nt * de ^ 2)
                        If STProperties.Shell_Fluid = 0 Then
                            Gsh = Wc / Ssh
                            Rsh = Gsh * de / muc
                        Else
                            Gsh = Wh / Ssh
                            Rsh = Gsh * de / muh
                        End If
                        Dim Ec, lb, he As Double
                        Select Case STProperties.Tube_Layout
                            Case 0, 1
                                If Rsh < 100 Then
                                    jh = 0.497 * Rsh ^ 0.54
                                Else
                                    jh = 0.378 * Rsh ^ 0.61
                                End If
                            Case 2, 3
                                If Rsh < 100 Then
                                    jh = 0.385 * Rsh ^ 0.526
                                Else
                                    jh = 0.2487 * Rsh ^ 0.625
                                End If
                        End Select
                        If STProperties.Shell_Fluid = 0 Then
                            he = jh * kc * Prs ^ 0.34 / de
                        Else
                            he = jh * kh * Prs ^ 0.34 / de
                        End If
                        lb = STProperties.Shell_BaffleSpacing * (Nb - 1)
                        Ec = lb + (L - lb) * (2 * STProperties.Shell_BaffleSpacing / (L - lb)) ^ 0.6 / L
                        If Double.IsNaN(Ec) Then Ec = 1
                        he *= Ec
                        'global HTC (U)
                        Dim kt As Double = STProperties.Tube_ThermalConductivity
                        Dim f1, f2, f3, f4, f5 As Double
                        f1 = de / (hi * di)
                        f2 = rt * de / di
                        f3 = de / (2 * kt) * Math.Log(de / di)
                        f4 = rs
                        f5 = 1 / he
                        If CalculationMode = HeatExchangerCalcMode.ShellandTube_CalcFoulingFactor Then
                            Ur = f1 + f3 + f5
                            Ur = 1 / Ur
                            Rf = 1 / Ud - 1 / Ur
                            STProperties.OverallFoulingFactor = Rf
                            U_ant = U
                            U = 1 / Ur + Rf
                            U = 1 / U
                        Else
                            U_ant = U
                            U = f1 + f2 + f3 + f4 + f5
                            STProperties.OverallFoulingFactor = f2 + f4
                            U = 1 / U
                            Q = U * A * F * LMTD / 1000
                            DeltaHc = Q / Wc
                            DeltaHh = -Q / Wh
                            Hc2 = Hc1 + DeltaHc
                            Hh2 = Hh1 + DeltaHh
                            StInCold.PropertyPackage.CurrentMaterialStream = StInCold
                            tmp = StInCold.PropertyPackage.DW_CalcEquilibrio_ISOL(PropertyPackages.FlashSpec.P, PropertyPackages.FlashSpec.H, Pc2, Hc2, Tc2)
                            Tc2_ant = Tc2
                            Tc2 = tmp(2)
                            Tc2 = 0.1 * Tc2 + 0.9 * Tc2_ant
                            StInHot.PropertyPackage.CurrentMaterialStream = StInHot
                            tmp = StInHot.PropertyPackage.DW_CalcEquilibrio_ISOL(PropertyPackages.FlashSpec.P, PropertyPackages.FlashSpec.H, Ph2, Hh2, Th2)
                            Th2_ant = Th2
                            Th2 = tmp(2)
                            Th2 = 0.1 * Th2 + 0.9 * Th2_ant
                        End If
                        STProperties.Ft = f1 'tube side
                        STProperties.Fc = f3 'heat conductivity pipe
                        STProperties.Fs = f5 'shell side
                        STProperties.Ff = STProperties.OverallFoulingFactor
                        STProperties.ReS = Res 'Reynolds number shell side
                        STProperties.ReT = Ret 'Reynolds number tube side

                        If STProperties.Shell_Fluid = 0 Then
                            Pc2 = Pc1 - dps
                            Ph2 = Ph1 - dpt
                        Else
                            Pc2 = Pc1 - dpt
                            Ph2 = Ph1 - dps
                        End If
                        Me.LMTD_F = F
                        If CalculationMode = HeatExchangerCalcMode.ShellandTube_Rating Then
                            fx = Math.Abs((Th2 - Th2_ant) ^ 2 + (Tc2 - Tc2_ant) ^ 2)
                        Else
                            fx = Math.Abs((U - U_ant)) ^ 2
                        End If
                        CheckCalculatorStatus()
                        icnt += 1
                    Loop Until fx < 0.01 Or icnt > 100
            End Select

            CheckSpec(Tc2, True, "cold stream outlet temperature")
            CheckSpec(Th2, True, "hot stream outlet temperature")
            CheckSpec(Ph2, True, "hot stream outlet pressure")
            CheckSpec(Pc2, True, "cold stream outlet pressure")

            ThermalEfficiency = Q / MaxHeatExchange * 100

            If Not DebugMode Then

                Me.ColdSideOutletTemperature = Tc2
                Me.HotSideOutletTemperature = Th2
                Me.ColdSidePressureDrop = Pc1 - Pc2
                Me.HotSidePressureDrop = Ph1 - Ph2
                Me.OverallCoefficient = U
                Me.Area = A

                'Define new calculated properties.
                StOutHot.Fases(0).SPMProperties.temperature = Th2
                StOutCold.Fases(0).SPMProperties.temperature = Tc2
                StOutHot.Fases(0).SPMProperties.pressure = Ph2
                StOutCold.Fases(0).SPMProperties.pressure = Pc2
                StOutHot.Fases(0).SPMProperties.enthalpy = Hh2
                StOutCold.Fases(0).SPMProperties.enthalpy = Hc2

                If Th2 < Tc1 Or Tc2 > Th1 Then
                    FlowSheet.WriteToLog(Me.GraphicObject.Tag & ": Temperature Cross", Color.DarkOrange, FormClasses.TipoAviso.Aviso)
                End If

                'Call the flowsheet calculation routine
                With objargs
                    .Calculado = True
                    .Nome = Me.Nome
                    .Tipo = Me.ObjectType
                End With

                FlowSheet.CalculationQueue.Enqueue(objargs)

            Else

                AppendDebugLine("Calculation finished successfully.")

            End If

        End Function

        Public Overrides Function DeCalculate() As Integer

            Dim form As Global.DWSIM.FormFlowsheet = Me.FlowSheet

            If Me.GraphicObject.OutputConnectors(0).IsAttached Then

                'Zerar valores da corrente de matéria conectada a jusante
                FlowSheet.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.OutputConnectors(0).AttachedConnector.AttachedTo.Name).Clear()

            End If

            If Me.GraphicObject.OutputConnectors(1).IsAttached Then

                'Zerar valores da corrente de matéria conectada a jusante
                FlowSheet.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.OutputConnectors(1).AttachedConnector.AttachedTo.Name).Clear()

            End If

            'Call function to calculate flowsheet
            Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
            With objargs
                .Calculado = False
                .Nome = Me.Nome
                .Tag = Me.GraphicObject.Tag
                .Tipo = Me.ObjectType
            End With

            FlowSheet.CalculationQueue.Enqueue(objargs)

        End Function

        Public Overloads Overrides Sub UpdatePropertyNodes(ByVal su As SistemasDeUnidades.Unidades, ByVal nf As String)

            Dim Conversor As New DWSIM.SistemasDeUnidades.Conversor
            If NodeTableItems Is Nothing Then
                NodeTableItems = New System.Collections.Generic.Dictionary(Of Integer, DWSIM.Outros.NodeItem)
                FillNodeItems()
            End If

            For Each nti As Outros.NodeItem In Me.NodeTableItems.Values
                nti.Value = GetPropertyValue(nti.Text, FlowSheet.Options.SelectedUnitSystem)
                nti.Unit = GetPropertyUnit(nti.Text, FlowSheet.Options.SelectedUnitSystem)
            Next

            If QTNodeTableItems Is Nothing Then
                QTNodeTableItems = New System.Collections.Generic.Dictionary(Of Integer, DWSIM.Outros.NodeItem)
                QTFillNodeItems()
            End If


            With QTNodeTableItems

                Dim valor As String

                If Area.HasValue Then
                    valor = Format(Conversor.ConverterDoSI(su.area, Area), nf)
                Else
                    valor = DWSIM.App.GetLocalString("NC")
                End If
                .Item(0).Value = valor
                .Item(0).Unit = su.area

                If Q.HasValue Then
                    valor = Format(Conversor.ConverterDoSI(su.spmp_heatflow, Q), nf)
                Else
                    valor = DWSIM.App.GetLocalString("NC")
                End If
                .Item(1).Value = valor
                .Item(1).Unit = su.spmp_heatflow

            End With


        End Sub

        Public Overrides Sub QTFillNodeItems()

            With QTNodeTableItems

                .Clear()

                .Add(0, New DWSIM.Outros.NodeItem(DWSIM.App.GetLocalString("Area"), "", "", 0, 0, ""))
                .Add(1, New DWSIM.Outros.NodeItem(DWSIM.App.GetLocalString("HeatLoad"), "", "", 1, 0, ""))

            End With

        End Sub

        Public Overrides Sub PopulatePropertyGrid(ByVal pgrid As PropertyGridEx.PropertyGridEx, ByVal su As SistemasDeUnidades.Unidades)

            Dim Conversor As New DWSIM.SistemasDeUnidades.Conversor

            'To be implemented according to the heat exchanger connectors.

            With pgrid

                .PropertySort = PropertySort.Categorized
                .ShowCustomProperties = True
                .Item.Clear()

                MyBase.PopulatePropertyGrid(pgrid, su)

                Dim In1, In2, Out1, Out2, energ As String
                If Me.GraphicObject.InputConnectors(0).IsAttached = True Then
                    In1 = Me.GraphicObject.InputConnectors(0).AttachedConnector.AttachedFrom.Tag
                Else
                    In1 = ""
                End If
                If Me.GraphicObject.InputConnectors(1).IsAttached = True Then
                    In2 = Me.GraphicObject.InputConnectors(1).AttachedConnector.AttachedFrom.Tag
                Else
                    In2 = ""
                End If
                If Me.GraphicObject.OutputConnectors(0).IsAttached = True Then
                    Out1 = Me.GraphicObject.OutputConnectors(0).AttachedConnector.AttachedTo.Tag
                Else
                    Out1 = ""
                End If
                If Me.GraphicObject.OutputConnectors(1).IsAttached = True Then
                    Out2 = Me.GraphicObject.OutputConnectors(1).AttachedConnector.AttachedTo.Tag
                Else
                    Out2 = ""
                End If

                If Me.GraphicObject.EnergyConnector.IsAttached = True Then
                    energ = Me.GraphicObject.EnergyConnector.AttachedConnector.AttachedTo.Tag
                Else
                    energ = ""
                End If

                .Item.Add(DWSIM.App.GetLocalString("Correntedeentrada1"), In1, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIInputMSSelector
                End With
                .Item.Add(DWSIM.App.GetLocalString("Correntedeentrada2"), In2, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIInputMSSelector
                End With

                .Item.Add(DWSIM.App.GetLocalString("Correntedesaida1"), Out1, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIOutputMSSelector
                End With
                .Item.Add(DWSIM.App.GetLocalString("Correntedesaida2"), Out2, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIOutputMSSelector
                End With

                .Item.Add(DWSIM.App.GetLocalString("HXCalculationMode"), Me, "CalculationMode", False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("HXCalculationModeDesc"), True)

                Dim AValue As Double

                Select Case CalcMode
                    Case HeatExchangerCalcMode.CalcArea, HeatExchangerCalcMode.CalcBothTemp, HeatExchangerCalcMode.CalcBothTemp_UA, HeatExchangerCalcMode.CalcTempColdOut, HeatExchangerCalcMode.CalcTempHotOut
                        .Item.Add(DWSIM.App.GetLocalString("HXFlowDirection"), Me, "FlowDir", False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("HXFlowDirectionDesc"), True)
                        AValue = Format(Conversor.ConverterDoSI(su.spmp_deltaP, Me.HotSidePressureDrop), FlowSheet.Options.NumberFormat)
                        .Item.Add(FT(DWSIM.App.GetLocalString("HXHotSidePressureDrop"), su.spmp_deltaP), AValue, False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("HXHotSidePressureDrop"), True)
                        With .Item(.Item.Count - 1)
                            .Tag = New Object() {FlowSheet.Options.NumberFormat, su.spmp_deltaP, "DP"}
                            .CustomEditor = New DWSIM.Editors.Generic.UIUnitConverter
                        End With
                        AValue = Format(Conversor.ConverterDoSI(su.spmp_deltaP, Me.ColdSidePressureDrop), FlowSheet.Options.NumberFormat)
                        .Item.Add(FT(DWSIM.App.GetLocalString("HXColdSidePressureDrop"), su.spmp_deltaP), AValue, False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("HXColdSidePressureDrop"), True)
                        With .Item(.Item.Count - 1)
                            .Tag = New Object() {FlowSheet.Options.NumberFormat, su.spmp_deltaP, "DP"}
                            .CustomEditor = New DWSIM.Editors.Generic.UIUnitConverter
                        End With
                End Select

                Select Case CalcMode
                    Case HeatExchangerCalcMode.CalcBothTemp_UA
                        AValue = Format(Conversor.ConverterDoSI(su.area, Me.Area), FlowSheet.Options.NumberFormat)
                        .Item.Add(FT(DWSIM.App.GetLocalString("Area"), su.area), AValue, False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("HeatTransferArea"), True)
                        With .Item(.Item.Count - 1)
                            .Tag = New Object() {FlowSheet.Options.NumberFormat, su.area, "A"}
                            .CustomEditor = New DWSIM.Editors.Generic.UIUnitConverter
                        End With
                        AValue = Format(Conversor.ConverterDoSI(su.heat_transf_coeff, Me.OverallCoefficient), FlowSheet.Options.NumberFormat)
                        .Item.Add(FT(DWSIM.App.GetLocalString("OverallHeatTranferCoefficient"), su.heat_transf_coeff), AValue, False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("OverallHeatTranferCoefficientDesc"), True)
                        With .Item(.Item.Count - 1)
                            .Tag = New Object() {FlowSheet.Options.NumberFormat, su.heat_transf_coeff, "U"}
                            .CustomEditor = New DWSIM.Editors.Generic.UIUnitConverter
                        End With
                    Case HeatExchangerCalcMode.CalcBothTemp
                        AValue = Format(Conversor.ConverterDoSI(su.area, Me.Area), FlowSheet.Options.NumberFormat)
                        .Item.Add(FT(DWSIM.App.GetLocalString("Area"), su.area), AValue, False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("HeatTransferArea"), True)
                        With .Item(.Item.Count - 1)
                            .Tag = New Object() {FlowSheet.Options.NumberFormat, su.area, "A"}
                            .CustomEditor = New DWSIM.Editors.Generic.UIUnitConverter
                        End With
                        AValue = Format(Conversor.ConverterDoSI(su.spmp_heatflow, Me.Q), FlowSheet.Options.NumberFormat)
                        .Item.Add(FT(DWSIM.App.GetLocalString("HeatLoad"), su.spmp_heatflow), AValue, False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("HeatLoadOfEquipment"), True)
                        With .Item(.Item.Count - 1)
                            .Tag = New Object() {FlowSheet.Options.NumberFormat, su.spmp_heatflow, "E"}
                            .CustomEditor = New DWSIM.Editors.Generic.UIUnitConverter
                        End With
                    Case HeatExchangerCalcMode.CalcTempColdOut
                        AValue = Format(Conversor.ConverterDoSI(su.area, Me.Area), FlowSheet.Options.NumberFormat)
                        .Item.Add(FT(DWSIM.App.GetLocalString("Area"), su.area), AValue, False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("HeatTransferArea"), True)
                        With .Item(.Item.Count - 1)
                            .Tag = New Object() {FlowSheet.Options.NumberFormat, su.area, "A"}
                            .CustomEditor = New DWSIM.Editors.Generic.UIUnitConverter
                        End With
                        AValue = Format(Conversor.ConverterDoSI(su.spmp_temperature, Me.TempHotOut), FlowSheet.Options.NumberFormat)
                        .Item.Add(FT(DWSIM.App.GetLocalString("HXTempHotOut"), su.spmp_temperature), AValue, False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("HXTempHotOutDesc"), True)
                        With .Item(.Item.Count - 1)
                            .Tag = New Object() {FlowSheet.Options.NumberFormat, su.spmp_temperature, "T"}
                            .CustomEditor = New DWSIM.Editors.Generic.UIUnitConverter
                        End With
                    Case HeatExchangerCalcMode.CalcTempHotOut
                        AValue = Format(Conversor.ConverterDoSI(su.area, Me.Area), FlowSheet.Options.NumberFormat)
                        .Item.Add(FT(DWSIM.App.GetLocalString("Area"), su.area), AValue, False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("HeatTransferArea"), True)
                        With .Item(.Item.Count - 1)
                            .Tag = New Object() {FlowSheet.Options.NumberFormat, su.area, "A"}
                            .CustomEditor = New DWSIM.Editors.Generic.UIUnitConverter
                        End With
                        AValue = Format(Conversor.ConverterDoSI(su.spmp_temperature, Me.TempColdOut), FlowSheet.Options.NumberFormat)
                        .Item.Add(FT(DWSIM.App.GetLocalString("HXTempColdOut"), su.spmp_temperature), AValue, False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("HXTempColdOutDesc"), True)
                        With .Item(.Item.Count - 1)
                            .Tag = New Object() {FlowSheet.Options.NumberFormat, su.spmp_temperature, "T"}
                            .CustomEditor = New DWSIM.Editors.Generic.UIUnitConverter
                        End With
                    Case HeatExchangerCalcMode.CalcArea
                        .Item.Add(DWSIM.App.GetLocalString("HXDefinedTemperature"), Me, "DefinedTemperature", False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("HXDefinedTemperatureDesc"), True)
                        Select Case Me.DefinedTemperature
                            Case SpecifiedTemperature.Cold_Fluid
                                AValue = Format(Conversor.ConverterDoSI(su.spmp_temperature, Me.TempColdOut), FlowSheet.Options.NumberFormat)
                                .Item.Add(FT(DWSIM.App.GetLocalString("HXTempColdOut"), su.spmp_temperature), AValue, False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("HXTempColdOutDesc"), True)
                                With .Item(.Item.Count - 1)
                                    .Tag = New Object() {FlowSheet.Options.NumberFormat, su.spmp_temperature, "T"}
                                    .CustomEditor = New DWSIM.Editors.Generic.UIUnitConverter
                                End With
                            Case SpecifiedTemperature.Hot_Fluid
                                AValue = Format(Conversor.ConverterDoSI(su.spmp_temperature, Me.TempHotOut), FlowSheet.Options.NumberFormat)
                                .Item.Add(FT(DWSIM.App.GetLocalString("HXTempHotOut"), su.spmp_temperature), AValue, False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("HXTempHotOutDesc"), True)
                                With .Item(.Item.Count - 1)
                                    .Tag = New Object() {FlowSheet.Options.NumberFormat, su.spmp_temperature, "T"}
                                    .CustomEditor = New DWSIM.Editors.Generic.UIUnitConverter
                                End With
                        End Select
                        AValue = Format(Conversor.ConverterDoSI(su.heat_transf_coeff, Me.OverallCoefficient), FlowSheet.Options.NumberFormat)
                        .Item.Add(FT(DWSIM.App.GetLocalString("OverallHeatTranferCoefficient"), su.heat_transf_coeff), AValue, False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("OverallHeatTranferCoefficientDesc"), True)
                    Case HeatExchangerCalcMode.ShellandTube_Rating
                        .Item.Add(DWSIM.App.GetLocalString("STHXProperties"), Me, "STProperties", False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("STHXPropertiesDesc"), True)
                        With .Item(.Item.Count - 1)
                            .CustomEditor = New DWSIM.Editors.HeatExchanger.UISTHXEditor
                        End With
                    Case HeatExchangerCalcMode.ShellandTube_CalcFoulingFactor
                        AValue = Format(Conversor.ConverterDoSI(su.spmp_temperature, Me.TempColdOut), FlowSheet.Options.NumberFormat)
                        .Item.Add(FT(DWSIM.App.GetLocalString("HXTempColdOut"), su.spmp_temperature), AValue, False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("HXTempColdOutDesc"), True)
                        With .Item(.Item.Count - 1)
                            .Tag = New Object() {FlowSheet.Options.NumberFormat, su.spmp_temperature, "T"}
                            .CustomEditor = New DWSIM.Editors.Generic.UIUnitConverter
                        End With
                        AValue = Format(Conversor.ConverterDoSI(su.spmp_temperature, Me.TempHotOut), FlowSheet.Options.NumberFormat)
                        .Item.Add(FT(DWSIM.App.GetLocalString("HXTempHotOut"), su.spmp_temperature), AValue, False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("HXTempHotOutDesc"), True)
                        With .Item(.Item.Count - 1)
                            .Tag = New Object() {FlowSheet.Options.NumberFormat, su.spmp_temperature, "T"}
                            .CustomEditor = New DWSIM.Editors.Generic.UIUnitConverter
                        End With
                        .Item.Add(DWSIM.App.GetLocalString("STHXProperties"), Me, "STProperties", False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("STHXPropertiesDesc"), True)
                        With .Item(.Item.Count - 1)
                            .CustomEditor = New DWSIM.Editors.HeatExchanger.UISTHXEditor
                        End With
                End Select

                If Me.GraphicObject.Calculated = True Then

                    'Shows some calculated properties, such as the heat load.

                    Select Case CalcMode
                        Case HeatExchangerCalcMode.CalcBothTemp_UA
                            AValue = Format(Conversor.ConverterDoSI(su.spmp_heatflow, Me.Q), FlowSheet.Options.NumberFormat)
                            .Item.Add(FT(DWSIM.App.GetLocalString("HeatLoad"), su.spmp_heatflow), AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("HeatLoadOfEquipment"), True)
                        Case HeatExchangerCalcMode.CalcBothTemp
                            AValue = Format(Conversor.ConverterDoSI(su.heat_transf_coeff, Me.OverallCoefficient), FlowSheet.Options.NumberFormat)
                            .Item.Add(FT(DWSIM.App.GetLocalString("OverallHeatTranferCoefficient"), su.heat_transf_coeff), AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("OverallHeatTranferCoefficientDesc"), True)
                            AValue = Format(Conversor.ConverterDoSI(su.spmp_temperature, Me.TempHotOut), FlowSheet.Options.NumberFormat)
                            .Item.Add(FT(DWSIM.App.GetLocalString("HXTempHotOut"), su.spmp_temperature), AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("HXTempHotOutDesc"), True)
                            AValue = Format(Conversor.ConverterDoSI(su.spmp_temperature, Me.TempColdOut), FlowSheet.Options.NumberFormat)
                            .Item.Add(FT(DWSIM.App.GetLocalString("HXTempColdOut"), su.spmp_temperature), AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("HXTempColdOutDesc"), True)
                        Case HeatExchangerCalcMode.CalcTempColdOut
                            AValue = Format(Conversor.ConverterDoSI(su.heat_transf_coeff, Me.OverallCoefficient), FlowSheet.Options.NumberFormat)
                            .Item.Add(FT(DWSIM.App.GetLocalString("OverallHeatTranferCoefficient"), su.heat_transf_coeff), AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("OverallHeatTranferCoefficientDesc"), True)
                            AValue = Format(Conversor.ConverterDoSI(su.spmp_heatflow, Me.Q), FlowSheet.Options.NumberFormat)
                            .Item.Add(FT(DWSIM.App.GetLocalString("HeatLoad"), su.spmp_heatflow), AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("HeatLoadOfEquipment"), True)
                            AValue = Format(Conversor.ConverterDoSI(su.spmp_temperature, Me.TempColdOut), FlowSheet.Options.NumberFormat)
                            .Item.Add(FT(DWSIM.App.GetLocalString("HXTempColdOut"), su.spmp_temperature), AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("HXTempColdOutDesc"), True)
                        Case HeatExchangerCalcMode.CalcTempHotOut
                            AValue = Format(Conversor.ConverterDoSI(su.heat_transf_coeff, Me.OverallCoefficient), FlowSheet.Options.NumberFormat)
                            .Item.Add(FT(DWSIM.App.GetLocalString("OverallHeatTranferCoefficient"), su.heat_transf_coeff), AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("OverallHeatTranferCoefficientDesc"), True)
                            AValue = Format(Conversor.ConverterDoSI(su.spmp_heatflow, Me.Q), FlowSheet.Options.NumberFormat)
                            .Item.Add(FT(DWSIM.App.GetLocalString("HeatLoad"), su.spmp_heatflow), AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("HeatLoadOfEquipment"), True)
                            AValue = Format(Conversor.ConverterDoSI(su.spmp_temperature, Me.TempHotOut), FlowSheet.Options.NumberFormat)
                            .Item.Add(FT(DWSIM.App.GetLocalString("HXTempHotOut"), su.spmp_temperature), AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("HXTempHotOutDesc"), True)
                        Case HeatExchangerCalcMode.CalcArea
                            Select Case Me.DefinedTemperature
                                Case SpecifiedTemperature.Cold_Fluid
                                    AValue = Format(Conversor.ConverterDoSI(su.spmp_temperature, Me.TempHotOut), FlowSheet.Options.NumberFormat)
                                    .Item.Add(FT(DWSIM.App.GetLocalString("HXTempHotOut"), su.spmp_temperature), AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("HXTempHotOutDesc"), True)
                                Case SpecifiedTemperature.Hot_Fluid
                                    AValue = Format(Conversor.ConverterDoSI(su.spmp_temperature, Me.TempColdOut), FlowSheet.Options.NumberFormat)
                                    .Item.Add(FT(DWSIM.App.GetLocalString("HXTempColdOut"), su.spmp_temperature), AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("HXTempColdOutDesc"), True)
                            End Select
                            AValue = Format(Conversor.ConverterDoSI(su.area, Me.Area), FlowSheet.Options.NumberFormat)
                            .Item.Add(FT(DWSIM.App.GetLocalString("Area"), su.area), AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("HeatTransferArea"), True)
                            AValue = Format(Conversor.ConverterDoSI(su.spmp_heatflow, Me.Q), FlowSheet.Options.NumberFormat)
                            .Item.Add(FT(DWSIM.App.GetLocalString("HeatLoad"), su.spmp_heatflow), AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("HeatLoadOfEquipment"), True)
                        Case HeatExchangerCalcMode.ShellandTube_Rating
                            AValue = Format(Conversor.ConverterDoSI(su.spmp_temperature, Me.TempHotOut), FlowSheet.Options.NumberFormat)
                            .Item.Add(FT(DWSIM.App.GetLocalString("HXTempHotOut"), su.spmp_temperature), AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("HXTempHotOutDesc"), True)
                            AValue = Format(Conversor.ConverterDoSI(su.spmp_temperature, Me.TempColdOut), FlowSheet.Options.NumberFormat)
                            .Item.Add(FT(DWSIM.App.GetLocalString("HXTempColdOut"), su.spmp_temperature), AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("HXTempColdOutDesc"), True)
                            AValue = Format(Conversor.ConverterDoSI(su.spmp_heatflow, Me.Q), FlowSheet.Options.NumberFormat)
                            .Item.Add(FT(DWSIM.App.GetLocalString("HeatLoad"), su.spmp_heatflow), AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("HeatLoadOfEquipment"), True)
                            AValue = Format(Conversor.ConverterDoSI(su.area, Me.Area), FlowSheet.Options.NumberFormat)
                            .Item.Add(FT(DWSIM.App.GetLocalString("Area"), su.area), AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("HeatTransferArea"), True)
                            AValue = Format(Conversor.ConverterDoSI(su.heat_transf_coeff, Me.OverallCoefficient), FlowSheet.Options.NumberFormat)
                            .Item.Add(FT(DWSIM.App.GetLocalString("OverallHeatTranferCoefficient"), su.heat_transf_coeff), AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("OverallHeatTranferCoefficientDesc"), True)
                            AValue = Format(Conversor.ConverterDoSI(su.spmp_deltaP, Me.HotSidePressureDrop), FlowSheet.Options.NumberFormat)
                            .Item.Add(FT(DWSIM.App.GetLocalString("HXHotSidePressureDrop"), su.spmp_deltaP), AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("HXHotSidePressureDrop"), True)
                            AValue = Format(Conversor.ConverterDoSI(su.spmp_deltaP, Me.ColdSidePressureDrop), FlowSheet.Options.NumberFormat)
                            .Item.Add(FT(DWSIM.App.GetLocalString("HXColdSidePressureDrop"), su.spmp_deltaP), AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("HXColdSidePressureDrop"), True)
                            AValue = Format(Me.LMTD_F, FlowSheet.Options.NumberFormat)
                            .Item.Add("F", AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("HXColdSidePressureDrop"), True)
                        Case HeatExchangerCalcMode.ShellandTube_CalcFoulingFactor
                            AValue = Conversor.ConverterDoSI(su.foulingfactor, Me.STProperties.OverallFoulingFactor)
                            .Item.Add(FT(DWSIM.App.GetLocalString("HXFoulingFactor"), su.foulingfactor), AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("HXFoulingFactorDesc"), True)
                            AValue = Format(Conversor.ConverterDoSI(su.spmp_heatflow, Me.Q), FlowSheet.Options.NumberFormat)
                            .Item.Add(FT(DWSIM.App.GetLocalString("HeatLoad"), su.spmp_heatflow), AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("HeatLoadOfEquipment"), True)
                            AValue = Format(Conversor.ConverterDoSI(su.area, Me.Area), FlowSheet.Options.NumberFormat)
                            .Item.Add(FT(DWSIM.App.GetLocalString("Area"), su.area), AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("HeatTransferArea"), True)
                            AValue = Format(Conversor.ConverterDoSI(su.heat_transf_coeff, Me.OverallCoefficient), FlowSheet.Options.NumberFormat)
                            .Item.Add(FT(DWSIM.App.GetLocalString("OverallHeatTranferCoefficient"), su.heat_transf_coeff), AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("OverallHeatTranferCoefficientDesc"), True)
                            AValue = Format(Conversor.ConverterDoSI(su.spmp_deltaP, Me.HotSidePressureDrop), FlowSheet.Options.NumberFormat)
                            .Item.Add(FT(DWSIM.App.GetLocalString("HXHotSidePressureDrop"), su.spmp_deltaP), AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("HXHotSidePressureDrop"), True)
                            AValue = Format(Conversor.ConverterDoSI(su.spmp_deltaP, Me.ColdSidePressureDrop), FlowSheet.Options.NumberFormat)
                            .Item.Add(FT(DWSIM.App.GetLocalString("HXColdSidePressureDrop"), su.spmp_deltaP), AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("HXColdSidePressureDrop"), True)
                            AValue = Format(Me.LMTD_F, FlowSheet.Options.NumberFormat)
                            .Item.Add("F", AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("HXLMTDCorr"), True)
                    End Select

                    .Item.Add(FT(DWSIM.App.GetLocalString("MaximumHeatExchange"), su.spmp_heatflow), Format(Me.MaxHeatExchange, FlowSheet.Options.NumberFormat), True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("MaximumHeatExchangeDesc"), True)
                    .Item.Add(FT(DWSIM.App.GetLocalString("ThermalEfficiency"), "%"), Format(Me.ThermalEfficiency, FlowSheet.Options.NumberFormat), True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("ThermalEfficiencyDesc"), True)

                    AValue = Format(Conversor.ConverterDoSI(su.spmp_deltaT, Me.LMTD), FlowSheet.Options.NumberFormat)
                    .Item.Add(FT(DWSIM.App.GetLocalString("HXLMTD"), su.spmp_deltaT), AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("HXLMTDDesc"), True)

                    If CalcMode = HeatExchangerCalcMode.ShellandTube_CalcFoulingFactor Or CalcMode = HeatExchangerCalcMode.ShellandTube_Rating Then
                        AValue = Format(Me.STProperties.ReS, FlowSheet.Options.NumberFormat)
                        .Item.Add("Re Shell", AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("HXReShell"), True)
                        AValue = Format(Me.STProperties.ReT, FlowSheet.Options.NumberFormat)
                        .Item.Add("Re Tube", AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("HXReTube"), True)
                        AValue = Format(Me.STProperties.Fs, FlowSheet.Options.NumberFormat)
                        .Item.Add(FT("F Shell", su.foulingfactor), AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("HXFShell"), True)
                        AValue = Format(Me.STProperties.Ft, FlowSheet.Options.NumberFormat)
                        .Item.Add(FT("F Tube", su.foulingfactor), AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("HXFTube"), True)
                        AValue = Format(Me.STProperties.Fc, FlowSheet.Options.NumberFormat)
                        .Item.Add(FT("F Pipe", su.foulingfactor), AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("HXFPipe"), True)
                        AValue = Format(Me.STProperties.Ff, FlowSheet.Options.NumberFormat)
                        .Item.Add(FT("F Fouling", su.foulingfactor), AValue, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("HXFFouling"), True)
                    End If

                End If

            End With


        End Sub

        Public Overrides Function GetPropertyValue(ByVal prop As String, Optional ByVal su As SistemasDeUnidades.Unidades = Nothing) As Object

            If su Is Nothing Then su = New DWSIM.SistemasDeUnidades.UnidadesSI
            Dim cv As New DWSIM.SistemasDeUnidades.Conversor
            Dim value As Double = 0
            Dim propidx As Integer = CInt(prop.Split("_")(2))

            Select Case propidx

                Case 0
                    'PROP_HX_0	Global Heat Transfer Coefficient (U)
                    value = cv.ConverterDoSI(su.heat_transf_coeff, Me.OverallCoefficient.GetValueOrDefault)
                Case 1
                    'PROP_HX_1	Heat Exchange Area (A)
                    value = cv.ConverterDoSI(su.area, Me.Area.GetValueOrDefault)
                Case 2
                    'PROP_HX_2	Heat Load
                    value = cv.ConverterDoSI(su.spmp_heatflow, Me.Q.GetValueOrDefault)
                Case 3
                    value = cv.ConverterDoSI(su.spmp_temperature, Me.TempColdOut)
                Case 4
                    value = cv.ConverterDoSI(su.spmp_temperature, Me.TempHotOut)
                Case 5
                    value = cv.ConverterDoSI(su.diameter, Me.STProperties.Shell_Di)
                Case 6
                    value = cv.ConverterDoSI(su.foulingfactor, Me.STProperties.Shell_Fouling)
                Case 7
                    value = Me.STProperties.Shell_BaffleCut
                Case 8
                    value = Me.STProperties.Shell_NumberOfShellsInSeries
                Case 9
                    value = cv.ConverterDoSI(su.thickness, Me.STProperties.Shell_BaffleSpacing)
                Case 10
                    value = cv.ConverterDoSI(su.diameter, Me.STProperties.Tube_Di)
                Case 11
                    value = cv.ConverterDoSI(su.diameter, Me.STProperties.Tube_De)
                Case 12
                    value = cv.ConverterDoSI(su.distance, Me.STProperties.Tube_Length)
                Case 13
                    value = cv.ConverterDoSI(su.foulingfactor, Me.STProperties.Tube_Fouling)
                Case 14
                    value = Me.STProperties.Tube_PassesPerShell
                Case 15
                    value = Me.STProperties.Tube_NumberPerShell
                Case 16
                    value = cv.ConverterDoSI(su.thickness, Me.STProperties.Tube_Pitch)
                Case 17
                    value = cv.ConverterDoSI(su.foulingfactor, Me.STProperties.OverallFoulingFactor)
                Case 18
                    value = Me.LMTD_F
                Case 19
                    value = cv.ConverterDoSI(su.spmp_deltaT, Me.LMTD)
                Case 20
                    value = cv.ConverterDoSI(su.foulingfactor, Me.STProperties.Ft)
                Case 21
                    value = cv.ConverterDoSI(su.foulingfactor, Me.STProperties.Fc)
                Case 22
                    value = cv.ConverterDoSI(su.foulingfactor, Me.STProperties.Fs)
                Case 23
                    value = cv.ConverterDoSI("", Me.STProperties.ReS)
                Case 24
                    value = cv.ConverterDoSI("", Me.STProperties.ReT)
                Case 25
                    value = ThermalEfficiency
                Case 26
                    value = cv.ConverterDoSI(su.spmp_heatflow, MaxHeatExchange)
            End Select

            Return value
        End Function

        Public Overloads Overrides Function GetProperties(ByVal proptype As SimulationObjects_BaseClass.PropertyType) As String()
            Dim i As Integer = 0
            Dim proplist As New ArrayList
            Select Case proptype
                Case PropertyType.RO
                    For i = 2 To 4
                        proplist.Add("PROP_HX_" + CStr(i))
                    Next
                    proplist.Add("PROP_HX_25")
                    proplist.Add("PROP_HX_26")
                    For i = 17 To 24
                        proplist.Add("PROP_HX_" + CStr(i))
                    Next
                Case PropertyType.RW
                    For i = 0 To 26
                        proplist.Add("PROP_HX_" + CStr(i))
                    Next
                Case PropertyType.WR
                    For i = 0 To 16
                        proplist.Add("PROP_HX_" + CStr(i))
                    Next
                Case PropertyType.ALL
                    For i = 0 To 26
                        proplist.Add("PROP_HX_" + CStr(i))
                    Next
            End Select
            Return proplist.ToArray(GetType(System.String))
            proplist = Nothing
        End Function

        Public Overrides Function SetPropertyValue(ByVal prop As String, ByVal propval As Object, Optional ByVal su As DWSIM.SistemasDeUnidades.Unidades = Nothing) As Object
            If su Is Nothing Then su = New DWSIM.SistemasDeUnidades.UnidadesSI
            Dim cv As New DWSIM.SistemasDeUnidades.Conversor
            Dim propidx As Integer = CInt(prop.Split("_")(2))

            Select Case propidx

                Case 0
                    'PROP_HX_0	Global Heat Transfer Coefficient (U)
                    Me.OverallCoefficient = cv.ConverterParaSI(su.heat_transf_coeff, propval)
                Case 1
                    'PROP_HX_1	Heat Exchange Area (A)
                    Me.Area = cv.ConverterParaSI(su.area, propval)
                Case 2
                    'PROP_HX_1	Heat Load (Q)
                    Me.Q = cv.ConverterParaSI(su.spmp_heatflow, propval)
                Case 3
                    'PROP_HX_3	Cold Fluid Outlet Temperature
                    Me.TempColdOut = cv.ConverterParaSI(su.spmp_temperature, propval)
                Case 4
                    'PROP_HX_4	Hot Fluid Outlet Temperature
                    Me.TempHotOut = cv.ConverterParaSI(su.spmp_temperature, propval)
                Case 5
                    Me.STProperties.Shell_Di = cv.ConverterParaSI(su.diameter, propval)
                Case 6
                    Me.STProperties.Shell_Fouling = cv.ConverterParaSI(su.foulingfactor, propval)
                Case 7
                    Me.STProperties.Shell_BaffleCut = propval
                Case 8
                    Me.STProperties.Shell_NumberOfShellsInSeries = propval
                Case 9
                    Me.STProperties.Shell_BaffleSpacing = cv.ConverterParaSI(su.thickness, propval)
                Case 10
                    Me.STProperties.Tube_Di = cv.ConverterParaSI(su.diameter, propval)
                Case 11
                    Me.STProperties.Tube_De = cv.ConverterParaSI(su.diameter, propval)
                Case 12
                    Me.STProperties.Tube_Length = cv.ConverterParaSI(su.distance, propval)
                Case 13
                    Me.STProperties.Tube_Fouling = cv.ConverterParaSI(su.foulingfactor, propval)
                Case 14
                    Me.STProperties.Tube_PassesPerShell = propval
                Case 15
                    Me.STProperties.Tube_NumberPerShell = propval
                Case 16
                    Me.STProperties.Tube_Pitch = cv.ConverterParaSI(su.thickness, propval)
            End Select
            Return 1
        End Function

        Public Overrides Function GetPropertyUnit(ByVal prop As String, Optional ByVal su As SistemasDeUnidades.Unidades = Nothing) As Object
            If su Is Nothing Then su = New DWSIM.SistemasDeUnidades.UnidadesSI
            Dim cv As New DWSIM.SistemasDeUnidades.Conversor
            Dim value As String = ""
            Dim propidx As Integer = CInt(prop.Split("_")(2))

            Select Case propidx

                Case 0
                    'PROP_HX_0	Global Heat Transfer Coefficient (U)
                    value = su.heat_transf_coeff
                Case 1
                    'PROP_HX_1	Heat Exchange Area (A)
                    value = su.area
                Case 2
                    'PROP_HX_2	Heat Load
                    value = su.spmp_heatflow
                Case 3
                    'PROP_HX_3	
                    value = su.spmp_temperature
                Case 4
                    'PROP_HX_4
                    value = su.spmp_temperature
                Case 5
                    value = su.diameter
                Case 6
                    value = su.foulingfactor
                Case 7
                    value = "%"
                Case 8
                    value = ""
                Case 9
                    value = su.thickness
                Case 10
                    value = su.diameter
                Case 11
                    value = su.diameter
                Case 12
                    value = su.distance
                Case 13
                    value = su.foulingfactor
                Case 14
                    value = ""
                Case 15
                    value = ""
                Case 16
                    value = su.thickness
                Case 17
                    value = su.foulingfactor
                Case 18
                    value = ""
                Case 19
                    value = su.spmp_deltaT
                Case 20, 21, 22
                    value = su.foulingfactor
                Case 23, 24
                    value = ""
                Case 25
                    value = "%"
                Case 26
                    value = su.spmp_heatflow
            End Select

            Return value
        End Function

    End Class

End Namespace

Namespace DWSIM.SimulationObjects.UnitOps.Auxiliary.HeatExchanger

    <System.Serializable()> Public Class STHXProperties

        Implements XMLSerializer.Interfaces.ICustomXMLSerialization

        'number of shells in series, integer
        Public Shell_NumberOfShellsInSeries As Integer = 1
        'number of shell passes, integer
        Public Shell_NumberOfPasses As Integer = 2
        'shell internal diameter in m
        Public Shell_Di As Double = 0.5
        'shell fouling in K.m2/W
        Public Shell_Fouling As Double = 0.0#
        'baffle type: 0 = single, 1 = double, 2 = triple, 3 = grid
        Public Shell_BaffleType As Integer = 0
        'baffle orientation: 0 = horizontal, 1 = vertical
        Public Shell_BaffleOrientation As Integer = 1
        'baffle cut in % diameter
        Public Shell_BaffleCut As Double = 20
        'baffle spacing in m
        Public Shell_BaffleSpacing As Double = 0.25
        'fluid in shell: 0 = cold, 1 = hot
        Public Shell_Fluid As Integer = 1
        'tube internal diameter in m
        Public Tube_Di As Double = 0.05
        'tube external diameter in m
        Public Tube_De As Double = 0.06
        'tube length in m
        Public Tube_Length As Double = 5
        'tube fouling in K.m2/W
        Public Tube_Fouling As Double = 0.0#
        'number of tube passes per shell, integer
        Public Tube_PassesPerShell As Integer = 2
        'number of tubes per shell, integer
        Public Tube_NumberPerShell As Integer = 160
        'tube layout: 0 = triangular, 1 = triangular rotated, 2 = square, 2 = square rotated
        Public Tube_Layout As Integer = 0
        'tube pitch in m
        Public Tube_Pitch As Double = 0.04
        'fluid in tubes: 0 = cold, 1 = hot
        Public Tube_Fluid As Integer = 0
        'tube material roughness in m
        Public Tube_Roughness As Double = 0.000045
        'shell material roughness in m
        Public Shell_Roughness As Double = 0.000045
        'tube scaling friction factor correction
        Public Tube_Scaling_FricCorrFactor As Double = 1.2#
        'tube thermal conductivity
        Public Tube_ThermalConductivity As Double = 70.0#
        'overall fouling factor, used only in design mode (as a calculation result)
        Public OverallFoulingFactor = 0.0#
        'partial heat exchange resistances (tube, conduction, shell, fouling), only as calculation result
        Public Ft, Fc, Fs, Ff As Double
        'Reynold numbers, only as calculation results
        Public ReT, ReS As Double

        Public Overrides Function ToString() As String
            Return DWSIM.App.GetLocalString("Cliqueparaeditar")
        End Function

        Public Function LoadData(data As System.Collections.Generic.List(Of System.Xml.Linq.XElement)) As Boolean Implements XMLSerializer.Interfaces.ICustomXMLSerialization.LoadData

            XMLSerializer.XMLSerializer.Deserialize(Me, data, True)

        End Function

        Public Function SaveData() As System.Collections.Generic.List(Of System.Xml.Linq.XElement) Implements XMLSerializer.Interfaces.ICustomXMLSerialization.SaveData

            Return XMLSerializer.XMLSerializer.Serialize(Me, True)

        End Function

    End Class


End Namespace

