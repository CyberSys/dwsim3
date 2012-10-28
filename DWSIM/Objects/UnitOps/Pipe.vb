﻿'    Pipe Calculation Routines 
'    Copyright 2008 Daniel Wagner O. de Medeiros
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

Imports Microsoft.MSDN.Samples.GraphicObjects
Imports DWSIM.DWSIM.SimulationObjects.PropertyPackages.Auxiliary
Imports DWSIM.DWSIM.Flowsheet.FlowSheetSolver

Namespace DWSIM.SimulationObjects.UnitOps

    Public Enum FlowPackage
        Beggs_Brill
        Lockhart_Martinelli
        Petalas_Aziz
    End Enum

    <System.Serializable()> Public Class Pipe

        Inherits SimulationObjects_UnitOpBaseClass

        Protected m_profile As New PipeProfile
        Protected m_thermalprofile As New DWSIM.Editors.PipeEditor.ThermalEditorDefinitions
        Protected m_selectedflowpackage As FlowPackage = FlowPackage.Beggs_Brill
        Protected m_dp As Nullable(Of Double)
        Protected m_dt As Nullable(Of Double)
        Protected m_DQ As Nullable(Of Double)

        Protected m_tolP As Double = 10
        Protected m_tolT As Double = 0.01

        Protected m_maxItT As Integer = 25
        Protected m_maxItP As Integer = 25

        Protected m_flashP As Double = 1
        Protected m_flashT As Double = 1

        Protected m_includejteffect As Boolean = False

        Public Property IncludeJTEffect() As Boolean
            Get
                Return m_includejteffect
            End Get
            Set(ByVal value As Boolean)
                m_includejteffect = value
            End Set
        End Property

        Public Property MaxPressureIterations() As Integer
            Get
                Return m_maxItP
            End Get
            Set(ByVal value As Integer)
                m_maxItP = value
            End Set
        End Property

        Public Property MaxTemperatureIterations() As Integer
            Get
                Return m_maxItT
            End Get
            Set(ByVal value As Integer)
                m_maxItT = value
            End Set
        End Property

        Public Property TriggerFlashP() As Double
            Get
                Return m_flashP
            End Get
            Set(ByVal value As Double)
                m_flashP = value
            End Set
        End Property

        Public Property TriggerFlashT() As Double
            Get
                Return m_flashT
            End Get
            Set(ByVal value As Double)
                m_flashT = value
            End Set
        End Property

        Public Property TolP() As Double
            Get
                Return m_tolP
            End Get
            Set(ByVal value As Double)
                m_tolP = value
            End Set
        End Property

        Public Property TolT() As Double
            Get
                Return m_tolT
            End Get
            Set(ByVal value As Double)
                m_tolT = value
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

        Public Property DeltaT() As Nullable(Of Double)
            Get
                Return m_dt
            End Get
            Set(ByVal value As Nullable(Of Double))
                m_dt = value
            End Set
        End Property

        Public Property DeltaQ() As Nullable(Of Double)
            Get
                Return m_DQ
            End Get
            Set(ByVal value As Nullable(Of Double))
                m_DQ = value
            End Set
        End Property

        Public Property SelectedFlowPackage() As FlowPackage
            Get
                Return Me.m_selectedflowpackage
            End Get
            Set(ByVal value As FlowPackage)
                Me.m_selectedflowpackage = value
            End Set
        End Property

        Public Property Profile() As PipeProfile
            Get
                Return m_profile
            End Get
            Set(ByVal value As PipeProfile)
                m_profile = value
            End Set
        End Property

        Public Property ThermalProfile() As DWSIM.Editors.PipeEditor.ThermalEditorDefinitions
            Get
                Return m_thermalprofile
            End Get
            Set(ByVal value As DWSIM.Editors.PipeEditor.ThermalEditorDefinitions)
                m_thermalprofile = value
            End Set
        End Property

        Public Sub New()
            MyBase.New()
        End Sub

        Public Sub New(ByVal nome As String, ByVal descricao As String)
            MyBase.CreateNew()
            Me.Profile = New PipeProfile
            Me.ThermalProfile = New DWSIM.Editors.PipeEditor.ThermalEditorDefinitions
            Me.m_ComponentName = nome
            Me.m_ComponentDescription = descricao
            Me.FillNodeItems()
            Me.QTFillNodeItems()

        End Sub

        Public Overrides Function Calculate(Optional ByVal args As Object = Nothing) As Integer

            Dim form As Global.DWSIM.FormFlowsheet = My.Application.ActiveSimulation
            Dim objargs As New DWSIM.Outros.StatusChangeEventArgs

            If Not Me.GraphicObject.EnergyConnector.IsAttached Then
                'Call function to calculate flowsheet
                With objargs
                    .Calculado = False
                    .Nome = Me.Nome
                    .Tipo = TipoObjeto.Pipe
                End With
                CalculateFlowsheet(FlowSheet, objargs, Nothing)
                Throw New Exception(DWSIM.App.GetLocalString("Nohcorrentedeenergia3"))
            ElseIf Not Me.Profile.Status = PipeEditorStatus.OK Then
                'Call function to calculate flowsheet
                With objargs
                    .Calculado = False
                    .Nome = Me.Nome
                    .Tipo = TipoObjeto.Pipe
                End With
                CalculateFlowsheet(FlowSheet, objargs, Nothing)
                Throw New Exception(DWSIM.App.GetLocalString("Operfilhidrulicodatu"))
            ElseIf Not Me.GraphicObject.OutputConnectors(0).IsAttached Then
                'Call function to calculate flowsheet
                With objargs
                    .Calculado = False
                    .Nome = Me.Nome
                    .Tipo = TipoObjeto.Pipe
                End With
                CalculateFlowsheet(FlowSheet, objargs, Nothing)
                Throw New Exception(DWSIM.App.GetLocalString("Verifiqueasconexesdo"))
            ElseIf Not Me.GraphicObject.InputConnectors(0).IsAttached Then
                'Call function to calculate flowsheet
                With objargs
                    .Calculado = False
                    .Nome = Me.Nome
                    .Tipo = TipoObjeto.Pipe
                End With
                CalculateFlowsheet(FlowSheet, objargs, Nothing)
                Throw New Exception(DWSIM.App.GetLocalString("Verifiqueasconexesdo"))
            End If

            Dim fpp As DWSIM.FlowPackages.FPBaseClass

            Select Case Me.SelectedFlowPackage
                Case FlowPackage.Lockhart_Martinelli
                    fpp = New DWSIM.FlowPackages.LockhartMartinelli
                Case FlowPackage.Petalas_Aziz
                    fpp = New DWSIM.FlowPackages.PetalasAziz
                Case Else
                    fpp = New DWSIM.FlowPackages.BeggsBrill
            End Select

            Dim oms As DWSIM.SimulationObjects.Streams.MaterialStream = form.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.InputConnectors(0).AttachedConnector.AttachedFrom.Name).Clone
            Me.PropertyPackage.CurrentMaterialStream = oms

            Dim Tin, Pin, Tout, Pout, Tout_ant, Pout_ant, Tout_ant2, Pout_ant2, Toutj, Text, Win, Qin, Qvin, Qlin, TinP, PinP, _
                rho_l, rho_v, Cp_l, Cp_v, Cp_m, K_l, K_v, eta_l, eta_v, tens, Hin, Hout, HinP, _
                fT, fT_ant, fT_ant2, fP, fP_ant, fP_ant2, w_v, w_l, w, z, z2, dzdT, hins, houts As Double
            Dim cntP, cntT As Integer

            If Me.ThermalProfile.Tipo = Editors.PipeEditor.ThermalProfileType.Definir_CGTC Then
                Text = Me.ThermalProfile.Temp_amb_definir
            Else
                Text = Me.ThermalProfile.Temp_amb_estimar
            End If

            'Iteração para cada segmento
            Dim count As Integer = 0

            'Calcular DP
            Dim Tpe, Ppe As Double
            Dim resv As Object
            Dim equilibrio As Object = Nothing
            Dim tmp As Object = Nothing
            Dim tipofluxo As String
            Dim first As Boolean = True
            Dim holdup, dpf, dph, dpt, DQ, U, A, eta As Double
            Dim nseg As Double
            Dim segmento As New PipeSection
            Dim results As New PipeResults
            Dim j As Integer = 0

            With oms

                Tin = .Fases(0).SPMProperties.temperature.GetValueOrDefault
                Pin = .Fases(0).SPMProperties.pressure.GetValueOrDefault
                Win = .Fases(0).SPMProperties.massflow.GetValueOrDefault
                Qin = .Fases(0).SPMProperties.volumetric_flow.GetValueOrDefault
                Hin = .Fases(0).SPMProperties.enthalpy.GetValueOrDefault
                Hout = Hin
                Tout = Tin
                Pout = Pin
                TinP = Tin
                PinP = Pin
                HinP = Hin

            End With

            Dim tseg As Integer = 0
            For Each segmento In Me.Profile.Sections.Values
                tseg += segmento.Incrementos
            Next

            For Each segmento In Me.Profile.Sections.Values

                segmento.Resultados.Clear()

                If segmento.Tipo = DWSIM.App.GetLocalString("Tubulaosimples") Then
                    j = 0
                    nseg = segmento.Incrementos

                    With oms

                        w = .Fases(0).SPMProperties.massflow.GetValueOrDefault
                        hins = .Fases(0).SPMProperties.enthalpy.GetValueOrDefault

                        Qlin = .Fases(3).SPMProperties.volumetric_flow.GetValueOrDefault + .Fases(4).SPMProperties.volumetric_flow.GetValueOrDefault + .Fases(5).SPMProperties.volumetric_flow.GetValueOrDefault + .Fases(6).SPMProperties.volumetric_flow.GetValueOrDefault
                        rho_l = .Fases(1).SPMProperties.density.GetValueOrDefault
                        eta_l = .Fases(1).SPMProperties.viscosity.GetValueOrDefault
                        K_l = .Fases(1).SPMProperties.thermalConductivity.GetValueOrDefault
                        Cp_l = .Fases(1).SPMProperties.heatCapacityCp.GetValueOrDefault
                        tens = .Fases(0).TPMProperties.surfaceTension.GetValueOrDefault
                        w_l = .Fases(1).SPMProperties.massflow.GetValueOrDefault

                        Qvin = .Fases(2).SPMProperties.volumetric_flow.GetValueOrDefault
                        rho_v = .Fases(2).SPMProperties.density.GetValueOrDefault
                        eta_v = .Fases(2).SPMProperties.viscosity.GetValueOrDefault
                        K_v = .Fases(2).SPMProperties.thermalConductivity.GetValueOrDefault
                        Cp_v = .Fases(2).SPMProperties.heatCapacityCp.GetValueOrDefault
                        w_v = .Fases(2).SPMProperties.massflow.GetValueOrDefault
                        z = .Fases(2).SPMProperties.compressibilityFactor.GetValueOrDefault

                    End With

                    Do

                        FlowSheet.WriteToLog(Me.GraphicObject.Tag & ": Calculating pipe segment #" & segmento.Indice & ", distance " & (j + 1) * segmento.Comprimento / segmento.Incrementos & "/" & segmento.Comprimento & "m", Color.DarkBlue, FormClasses.TipoAviso.Informacao)

                        If Text > Tin Then
                            Tout = Tin * 1.005
                        Else
                            Tout = Tin / 1.005
                        End If

                        If Tin < Text And Tout > Text Then Tout = Text * 0.98
                        If Tin > Text And Tout < Text Then Tout = Text * 1.02

                        cntT = 0
                        'Loop externo (convergência do Delta T)
                        Do

                            cntP = 0
                            'Loop interno (convergência do Delta P)
                            Do

                                With segmento
                                    count = 0
                                    With results

                                        .TemperaturaInicial = Tin
                                        .PressaoInicial = Pin
                                        .Energia_Inicial = Hin
                                        .Cpl = Cp_l
                                        .Cpv = Cp_v
                                        .Kl = K_l
                                        .Kv = K_v
                                        .RHOl = rho_l
                                        .RHOv = rho_v
                                        .Ql = Qlin
                                        .Qv = Qvin
                                        .MUl = eta_l
                                        .MUv = eta_v
                                        .Surft = tens
                                        .LiqRe = 4 / Math.PI * .RHOl * .Ql / (.MUl * segmento.DI * 0.0254)
                                        .VapRe = 4 / Math.PI * .RHOv * .Qv / (.MUv * segmento.DI * 0.0254)
                                        .LiqVel = .Ql / (Math.PI * (segmento.DI * 0.0254) ^ 2 / 4)
                                        .VapVel = .Qv / (Math.PI * (segmento.DI * 0.0254) ^ 2 / 4)

                                    End With

                                    resv = fpp.CalculateDeltaP(.DI * 0.0254, .Comprimento / .Incrementos, .Elevacao / .Incrementos, Me.rugosidade(.Material), Qvin * 24 * 3600, Qlin * 24 * 3600, eta_v * 1000, eta_l * 1000, rho_v, rho_l, tens)

                                    tipofluxo = resv(0)
                                    holdup = resv(1)
                                    dpf = resv(2)
                                    dph = resv(3)
                                    dpt = resv(4)

                                End With

                                Pout_ant2 = Pout_ant
                                Pout_ant = Pout
                                Pout = Pin - dpt

                                fP_ant2 = fP_ant
                                fP_ant = fP
                                fP = Pout - Pout_ant

                                'T = T - fi * (T - Tant2) / (fi - fi_ant2)

                                If cntP > 3 Then
                                    Pout = Pout - fP * (Pout - Pout_ant2) / (fP - fP_ant2)
                                End If

                                cntP += 1

                                If Pout <= 0 Then
                                    Throw New Exception(DWSIM.App.GetLocalString("Pressonegativadentro"))
                                End If
                                If Double.IsNaN(Pout) Then Throw New Exception(DWSIM.App.GetLocalString("Erronoclculodapresso"))
                                If cntP > Me.MaxPressureIterations Then Throw New Exception(DWSIM.App.GetLocalString("Ocalculadorexcedeuon"))

                                CheckCalculatorStatus()

                            Loop Until Math.Abs(fP) < Me.TolP

                            With segmento

                                Cp_m = holdup * Cp_l + (1 - holdup) * Cp_v
                                Tout_ant2 = Tout_ant
                                Tout_ant = Tout

                                If Not Me.ThermalProfile.Tipo = Editors.PipeEditor.ThermalProfileType.Definir_Q Then
                                    If Me.ThermalProfile.Tipo = Editors.PipeEditor.ThermalProfileType.Definir_CGTC Then
                                        U = Me.ThermalProfile.CGTC_Definido
                                        A = Math.PI * (.DE * 0.0254) * .Comprimento / .Incrementos
                                    ElseIf Me.ThermalProfile.Tipo = Editors.PipeEditor.ThermalProfileType.Estimar_CGTC Then
                                        A = Math.PI * (.DE * 0.0254) * .Comprimento / .Incrementos
                                        U = Me.CALCULAR_CGTC(.Material, holdup, .Comprimento / .Incrementos, _
                                                                            .DI * 0.0254, .DE * 0.0254, Me.rugosidade(.Material), Tpe, results.VapVel, results.LiqVel, _
                                                                            results.Cpl, results.Cpv, results.Kl / 1000, results.Kv / 1000, _
                                                                            results.MUl, results.MUv, results.RHOl, results.RHOv, _
                                                                            Me.ThermalProfile.Incluir_cti, Me.ThermalProfile.Incluir_isolamento, _
                                                                            Me.ThermalProfile.Incluir_paredes, Me.ThermalProfile.Incluir_cte)
                                        U = U
                                    End If
                                    If U <> 0 Then
                                        DQ = (Tout - Tin) / Math.Log((Text - Tin) / (Text - Tout)) * U / 1000 * A
                                        If Double.IsNaN(DQ) Then
                                            DQ = 0
                                            'Tout = Text
                                        Else
                                            Tout = DQ / (Win * Cp_m) + Tin
                                        End If
                                        If Not Double.TryParse(Tout, New Double) Or Double.IsNaN(Tout) Or Tout < 100 Or Tout > 1000 Then
                                            Throw New Exception(DWSIM.App.GetLocalString("Erroaocalculartemper"))
                                        End If
                                    Else
                                        Tout = Tin
                                    End If
                                Else
                                    DQ = Me.ThermalProfile.Calor_trocado / tseg '/ 3600
                                    Tout = DQ / (Win * Cp_m) + Tin
                                    A = Math.PI * (.DE * 0.0254) * .Comprimento / .Incrementos
                                    U = DQ / (A * (Tout - Tin)) * 1000
                                End If
                            End With

                            fT_ant2 = fT_ant
                            fT_ant = fT
                            fT = Tout - Tout_ant

                            'T = T - fi * (T - Tant2) / (fi - fi_ant2)

                            If cntT > 3 Then
                                Tout = Tout - fT * (Tout - Tout_ant2) / (fT - fT_ant2)
                            End If

                            cntT += 1

                            If Tout <= 0 Or Double.IsNaN(Tout) Then
                                Throw New Exception(DWSIM.App.GetLocalString("Erronoclculodatemper"))
                            End If

                            If cntT > Me.MaxPressureIterations Then Throw New Exception(DWSIM.App.GetLocalString("Ocalculadorexcedeuon"))

                            CheckCalculatorStatus()

                        Loop Until Math.Abs(fT) < Me.TolT

                        With Me.PropertyPackage

                            If IncludeJTEffect Then

                                Cp_m = (w_l * Cp_l + w_v * Cp_v) / w

                                If oms.Fases(2).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                    oms.Fases(0).SPMProperties.temperature = Tin - 2
                                    .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                                    z2 = oms.Fases(2).SPMProperties.compressibilityFactor.GetValueOrDefault
                                    dzdT = (z2 - z) / -2
                                Else
                                    dzdT = 0.0#
                                End If

                                If w_l <> 0.0# Then
                                    eta = 1 / (Cp_m * w) * (w_v / rho_v * (-Tin / z * dzdT) + w_l / rho_l)
                                Else
                                    eta = 1 / (Cp_m * w) * (w_v / rho_v * (-Tin / z * dzdT))
                                End If

                                Dim dh As Double = Cp_m * (Tout - Tin) - eta * Cp_m * (Pout - Pin)
                                houts = hins + dh / w

                                'Dim tmpr As Object = .DW_CalcEquilibrio_ISOL(PropertyPackages.FlashSpec.P, PropertyPackages.FlashSpec.H, Pout, houts, Tout)
                                'Toutj = tmpr(2)

                                Toutj = Tout + eta * (Pin - Pout) / 1000

                                Tout = Toutj

                            End If

                            oms.Fases(0).SPMProperties.temperature = Tout
                            oms.Fases(0).SPMProperties.pressure = Pout

                            'Calcular corrente de matéria com Tpe e Ppe
                            .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.T, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P)
                            If oms.Fases(3).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid1)
                            Else
                                .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid1)
                            End If
                            If oms.Fases(4).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid2)
                            Else
                                .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid2)
                            End If
                            If oms.Fases(5).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid3)
                            Else
                                .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid3)
                            End If
                            If oms.Fases(6).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Aqueous)
                            Else
                                .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Aqueous)
                            End If
                            If oms.Fases(7).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Solid)
                            Else
                                .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Solid)
                            End If
                            If oms.Fases(2).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                            Else
                                .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                            End If
                            If oms.Fases(2).SPMProperties.molarfraction.GetValueOrDefault >= 0 And oms.Fases(2).SPMProperties.molarfraction.GetValueOrDefault < 1 Then
                                .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid)
                            Else
                                .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid)
                            End If
                            .DW_CalcCompMolarFlow(-1)
                            .DW_CalcCompMassFlow(-1)
                            .DW_CalcCompVolFlow(-1)
                            .DW_CalcOverallProps()
                            .DW_CalcTwoPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid, DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                            .DW_CalcVazaoVolumetrica()
                            .DW_CalcKvalue()

                        End With

                        With oms

                            w = .Fases(0).SPMProperties.massflow.GetValueOrDefault
                            hins = .Fases(0).SPMProperties.enthalpy.GetValueOrDefault

                            Qlin = .Fases(3).SPMProperties.volumetric_flow.GetValueOrDefault + .Fases(4).SPMProperties.volumetric_flow.GetValueOrDefault + .Fases(5).SPMProperties.volumetric_flow.GetValueOrDefault + .Fases(6).SPMProperties.volumetric_flow.GetValueOrDefault
                            rho_l = .Fases(1).SPMProperties.density.GetValueOrDefault
                            eta_l = .Fases(1).SPMProperties.viscosity.GetValueOrDefault
                            K_l = .Fases(1).SPMProperties.thermalConductivity.GetValueOrDefault
                            Cp_l = .Fases(1).SPMProperties.heatCapacityCp.GetValueOrDefault
                            tens = .Fases(0).TPMProperties.surfaceTension.GetValueOrDefault
                            w_l = .Fases(1).SPMProperties.massflow.GetValueOrDefault

                            Qvin = .Fases(2).SPMProperties.volumetric_flow.GetValueOrDefault
                            rho_v = .Fases(2).SPMProperties.density.GetValueOrDefault
                            eta_v = .Fases(2).SPMProperties.viscosity.GetValueOrDefault
                            K_v = .Fases(2).SPMProperties.thermalConductivity.GetValueOrDefault
                            Cp_v = .Fases(2).SPMProperties.heatCapacityCp.GetValueOrDefault
                            w_v = .Fases(2).SPMProperties.massflow.GetValueOrDefault
                            z = .Fases(2).SPMProperties.compressibilityFactor.GetValueOrDefault

                        End With

                        With results

                            .CalorTransferido = DQ
                            .DpPorFriccao = dpf
                            .DpPorHidrostatico = dph
                            .HoldupDeLiquido = holdup
                            .TipoFluxo = tipofluxo

                            segmento.Resultados.Add(New PipeResults(.PressaoInicial, .TemperaturaInicial, .MUv, .MUl, .RHOv, .RHOl, .Cpv, .Cpl, .Kv, .Kl, .Qv, .Ql, .Surft, .DpPorFriccao, .DpPorHidrostatico, .HoldupDeLiquido, .TipoFluxo, .LiqRe, .VapRe, .LiqVel, .VapVel, .CalorTransferido, .Energia_Inicial, U))

                        End With

                        Hout = Hin + DQ / Win

                        Hin = Hout
                        Tin = Tout
                        Pin = Pout

                        j += 1

                    Loop Until j = nseg

                Else

                    'CALCULAR DP PARA VALVULAS
                    count = 0

                    If segmento.Indice = 1 Then

                        With Me.PropertyPackage

                            Tpe = Tin + (Tout - Tin) / 2
                            Tpe = Tin + (Tout - Tin) / 2
                            Ppe = Pin + (Pout - Pin) / 2

                            oms.Fases(0).SPMProperties.temperature = Tpe
                            oms.Fases(0).SPMProperties.pressure = Ppe

                            'Calcular corrente de matéria com Tpe e Ppe
                            .DW_CalcVazaoMolar()
                            .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.T, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P)
                            If oms.Fases(3).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid1)
                            Else
                                .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid1)
                            End If
                            If oms.Fases(4).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid2)
                            Else
                                .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid2)
                            End If
                            If oms.Fases(2).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                                .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                            Else
                                .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                            End If
                            .DW_CalcLiqMixtureProps()
                            .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Mixture)
                            .DW_CalcOverallProps()
                            .DW_CalcTwoPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid, DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                            .DW_CalcVazaoVolumetrica()

                        End With

                        With oms

                            Qlin = .Fases(3).SPMProperties.volumetric_flow.GetValueOrDefault + .Fases(4).SPMProperties.volumetric_flow.GetValueOrDefault + .Fases(5).SPMProperties.volumetric_flow.GetValueOrDefault + .Fases(6).SPMProperties.volumetric_flow.GetValueOrDefault
                            rho_l = .Fases(1).SPMProperties.density.GetValueOrDefault
                            eta_l = .Fases(1).SPMProperties.viscosity.GetValueOrDefault
                            K_l = .Fases(1).SPMProperties.thermalConductivity.GetValueOrDefault
                            Cp_l = .Fases(1).SPMProperties.heatCapacityCp.GetValueOrDefault
                            tens = .Fases(0).TPMProperties.surfaceTension.GetValueOrDefault

                            Qvin = .Fases(2).SPMProperties.volumetric_flow.GetValueOrDefault
                            rho_v = .Fases(2).SPMProperties.density.GetValueOrDefault
                            eta_v = .Fases(2).SPMProperties.viscosity.GetValueOrDefault
                            K_v = .Fases(2).SPMProperties.thermalConductivity.GetValueOrDefault
                            Cp_v = .Fases(2).SPMProperties.heatCapacityCp.GetValueOrDefault

                        End With

                    End If

                    With segmento
                        count = 0
                        With results
                            .TemperaturaInicial = Tin
                            .PressaoInicial = Pin
                            .Energia_Inicial = Hin
                            .Cpl = Cp_l
                            .Cpv = Cp_v
                            .Kl = K_l
                            .Kv = K_v
                            .RHOl = rho_l
                            .RHOv = rho_v
                            .Ql = Qlin
                            .Qv = Qvin
                            .MUl = eta_l
                            .MUv = eta_v
                            .Surft = tens
                            .LiqRe = 4 / Math.PI * .RHOl * .Ql / (.MUl * segmento.DI * 0.0254)
                            .VapRe = 4 / Math.PI * .RHOv * .Qv / (.MUv * segmento.DI * 0.0254)
                            .LiqVel = .Ql / (Math.PI * (segmento.DI * 0.0254) ^ 2 / 4)
                            .VapVel = .Qv / (Math.PI * (segmento.DI * 0.0254) ^ 2 / 4)

                        End With

                        results.TipoFluxo = ""
                        resv = Me.Kfit(segmento.Tipo)
                        If resv(1) Then
                            dph = 0
                            dpf = resv(0) * (0.0101 * (.DI * 0.0254) ^ -0.2232) * (Qlin / (Qvin + Qlin) * rho_l + Qvin / (Qvin + Qlin) * rho_v) * (results.LiqVel.GetValueOrDefault + results.VapVel.GetValueOrDefault) ^ 2 / 2
                        Else
                            dph = 0
                            dpf = resv(0) * (Qlin / (Qvin + Qlin) * rho_l + Qvin / (Qvin + Qlin) * rho_v) * (results.LiqVel.GetValueOrDefault + results.VapVel.GetValueOrDefault) ^ 2 / 2
                        End If
                        dpt = dpf

                        'If Not tmp Is Nothing Then
                        '    tmp_ant = tmp
                        'Else
                        '    tmp_ant = New Object() {form.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.InputConnectors(0).AttachedConnector.AttachedFrom.Name).Fases(1).SPMProperties.molarfraction.GetValueOrDefault, 0}
                        'End If
                        'tmp = form.Options.SelectedPropertyPackage.DW_CalcEquilibrio_ISOL(PropertyPackages.FlashSpec.P, PropertyPackages.FlashSpec.H, Pin - dpt, Hout, T2)
                        Pout = Pin - dpt
                        Tout = Tin
                        DQ = 0

                        With results
                            .CalorTransferido = DQ
                            .DpPorFriccao = dpf
                            .DpPorHidrostatico = dph
                            .HoldupDeLiquido = holdup
                            .TipoFluxo = DWSIM.App.GetLocalString("Turbulento")
                            .TipoFluxoDescricao = ""

                            segmento.Resultados.Add(New PipeResults(.PressaoInicial, .TemperaturaInicial, .MUv, .MUl, .RHOv, .RHOl, .Cpv, .Cpl, .Kv, .Kl, .Qv, .Ql, .Surft, .DpPorFriccao, .DpPorHidrostatico, .HoldupDeLiquido, .TipoFluxo, .LiqRe, .VapRe, .LiqVel, .VapVel, .CalorTransferido, .Energia_Inicial, U))

                        End With

                    End With

                End If

            Next

            With results
                .TemperaturaInicial = Tout
                .PressaoInicial = Pout
                .Energia_Inicial = Hout
                .Cpl = Cp_l
                .Cpv = Cp_v
                .Kl = K_l
                .Kv = K_v
                .RHOl = rho_l
                .RHOv = rho_v
                .Ql = Qlin
                .Qv = Qvin
                .MUl = eta_l
                .MUv = eta_v
                .Surft = tens
                .LiqRe = 4 / Math.PI * .RHOl * .Ql / (.MUl * segmento.DI * 0.0254)
                .VapRe = 4 / Math.PI * .RHOv * .Qv / (.MUv * segmento.DI * 0.0254)
                .LiqVel = .Ql / (Math.PI * (segmento.DI * 0.0254) ^ 2 / 4)
                .VapVel = .Qv / (Math.PI * (segmento.DI * 0.0254) ^ 2 / 4)
                .CalorTransferido = DQ
                .DpPorFriccao = dpf
                .DpPorHidrostatico = dph
                .HoldupDeLiquido = holdup
                .TipoFluxo = "-"
                .TipoFluxoDescricao = ""
                .HTC = U
            End With
            segmento.Resultados.Add(results)

            Me.DeltaP = -(PinP - Pout)
            Me.DeltaT = -(TinP - Tout)
            Me.DeltaQ = -(HinP - Hout) * Win

            'Atribuir valores à corrente de matéria conectada à jusante
            With form.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.OutputConnectors(0).AttachedConnector.AttachedTo.Name)
                .Fases(0).SPMProperties.temperature = Tout
                .Fases(0).SPMProperties.pressure = Pout
                .Fases(0).SPMProperties.enthalpy = 0
                Dim comp As DWSIM.ClassesBasicasTermodinamica.Substancia
                For Each comp In .Fases(0).Componentes.Values
                    comp.FracaoMolar = form.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.InputConnectors(0).AttachedConnector.AttachedFrom.Name).Fases(0).Componentes(comp.Nome).FracaoMolar
                    comp.FracaoMassica = form.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.InputConnectors(0).AttachedConnector.AttachedFrom.Name).Fases(0).Componentes(comp.Nome).FracaoMassica
                Next
                .Fases(0).SPMProperties.massflow = form.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.InputConnectors(0).AttachedConnector.AttachedFrom.Name).Fases(0).SPMProperties.massflow.GetValueOrDefault
            End With

            'Corrente de energia - atualizar valor da potência (kJ/s)
            With form.Collections.CLCS_EnergyStreamCollection(Me.GraphicObject.EnergyConnector.AttachedConnector.AttachedTo.Name)
                .Energia = -Me.DeltaQ.Value
                .GraphicObject.Calculated = True
            End With

            'Call function to calculate flowsheet
            With objargs
                .Calculado = True
                .Nome = Me.Nome
                .Tag = Me.GraphicObject.Tag
                .Tipo = TipoObjeto.Pipe
            End With

            segmento = Nothing
            results = Nothing

            form.CalculationQueue.Enqueue(objargs)

        End Function

        Public Overrides Function DeCalculate() As Integer

            Dim form As Global.DWSIM.FormFlowsheet = My.Application.ActiveSimulation
            Dim segmento As New PipeSection

            For Each segmento In Me.Profile.Sections.Values
                segmento.Resultados.Clear()
            Next

            'Zerar valores da corrente de matéria conectada a jusante
            If Me.GraphicObject.OutputConnectors(0).IsAttached Then
                With form.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.OutputConnectors(0).AttachedConnector.AttachedTo.Name)
                    .Fases(0).SPMProperties.temperature = Nothing
                    .Fases(0).SPMProperties.pressure = Nothing
                    .Fases(0).SPMProperties.enthalpy = Nothing
                    .Fases(0).SPMProperties.molarfraction = 1
                    .Fases(0).SPMProperties.massfraction = 1
                    Dim comp As DWSIM.ClassesBasicasTermodinamica.Substancia
                    Dim i As Integer = 0
                    For Each comp In .Fases(0).Componentes.Values
                        comp.FracaoMolar = 0
                        comp.FracaoMassica = 0
                        i += 1
                    Next
                    .Fases(0).SPMProperties.massflow = Nothing
                    .Fases(0).SPMProperties.molarflow = Nothing
                    .GraphicObject.Calculated = False
                End With
            End If

            'Corrente de energia - atualizar valor da potência (kJ/s)
            If Me.GraphicObject.EnergyConnector.IsAttached Then
                With form.Collections.CLCS_EnergyStreamCollection(Me.GraphicObject.EnergyConnector.AttachedConnector.AttachedTo.Name)
                    .Energia = Nothing
                    .GraphicObject.Calculated = False
                End With
            End If

            'Call function to calculate flowsheet
            Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
            With objargs
                .Calculado = False
                .Nome = Me.Nome
                .Tipo = TipoObjeto.Pipe
            End With

            segmento = Nothing

            form.CalculationQueue.Enqueue(objargs)

        End Function

#Region "        Funções"

        Function Kfit(ByVal name2 As String) As Array

            Dim name As String = name2.Substring(name2.IndexOf("[") + 1, name2.Length - name2.IndexOf("[") - 2)

            Dim tmp(1) As Double

            'Curva Normal 90°;30,00;1;
            If name = 0 Then
                tmp(0) = 30
                tmp(1) = 1
            End If
            'Curva Normal 45°;16,00;1;
            If name = 1 Then
                tmp(0) = 16
                tmp(1) = 1
            End If
            'Curva Normal 180°;50,00;1;
            If name = 2 Then
                tmp(0) = 50
                tmp(1) = 1
            End If
            'Válvula Angular;55,00;1;
            If name = 3 Then
                tmp(0) = 55
                tmp(1) = 1
            End If
            'Válvula Borboleta (2" a 14");40,00;1;
            If name = 4 Then
                tmp(0) = 40
                tmp(1) = 1
            End If
            'Válvula Esfera;3,00;1;
            If name = 5 Then
                tmp(0) = 3
                tmp(1) = 1
            End If
            'Válvula Gaveta (Aberta);8,00;1;
            If name = 6 Then
                tmp(0) = 8
                tmp(1) = 1
            End If
            'Válvula Globo;340,00;1;
            If name = 7 Then
                tmp(0) = 340
                tmp(1) = 1
            End If
            'Válvula Lift-Check;600,00;1;
            If name = 8 Then
                tmp(0) = 600
                tmp(1) = 1
            End If
            'Válvula Pé (Poppet Disc);420,00;1;
            If name = 9 Then
                tmp(0) = 420
                tmp(1) = 1
            End If
            'Válvula Retenção de Portinhola;100,00;1;
            If name = 10 Then
                tmp(0) = 100
                tmp(1) = 1
            End If
            'Válvula Stop-Check (Globo);400,00;1;
            If name = 11 Then
                tmp(0) = 400
                tmp(1) = 1
            End If
            'Tê (saída bilateral);20,00;1;
            If name = 12 Then
                tmp(0) = 20
                tmp(1) = 1
            End If
            'Tê (saída de lado);60,00;1;
            If name = 13 Then
                tmp(0) = 60
                tmp(1) = 1
            End If
            'Contração Rápida d/D = 1/2;9,60;0;
            If name = 14 Then
                tmp(0) = 9.6
                tmp(1) = 0
            End If
            'Contração Rápida d/D = 1/4;96,00;0;
            If name = 15 Then
                tmp(0) = 96
                tmp(1) = 0
            End If
            'Contração Rápida d/D = 3/4;1,11;0;
            If name = 16 Then
                tmp(0) = 11
                tmp(1) = 0
            End If
            'Entrada Borda;0,25;0;
            If name = 17 Then
                tmp(0) = 0.25
                tmp(1) = 0
            End If
            'Entrada Normal;0,78;0;
            If name = 18 Then
                tmp(0) = 0.78
                tmp(1) = 0
            End If
            'Expansão Rápida d/D = 1/2;9,00;0;
            If name = 19 Then
                tmp(0) = 9
                tmp(1) = 0
            End If
            'Expansão Rápida d/D = 1/4;225,00;0;
            If name = 20 Then
                tmp(0) = 225
                tmp(1) = 0
            End If
            'Expansão Rápida d/D = 3/4;0,60;0;
            If name = 21 Then
                tmp(0) = 0.6
                tmp(1) = 0
            End If
            'Joelho em 90°;60,00;1;
            If name = 22 Then
                tmp(0) = 60
                tmp(1) = 1
            End If
            'Redução Normal 2:1;5,67;0;
            If name = 23 Then
                tmp(0) = 5.67
                tmp(1) = 0
            End If
            'Redução Normal 4:3;0,65;0;
            If name = 24 Then
                tmp(0) = 0.65
                tmp(1) = 0
            End If
            'Saída Borda;1,00;0;
            If name = 25 Then
                tmp(0) = 1
                tmp(1) = 0
            End If
            'Saída Normal;1,00;0;
            If name = 26 Then
                tmp(0) = 1
                tmp(1) = 0
            End If

            Kfit = tmp

        End Function

        Function cond_isol(ByVal meio As Integer) As Double

            'Asfalto
            'Concreto
            'Espuma de Poliuretano
            'Espuma de PVC
            'Fibra de vidro
            'Plástico
            'Vidro
            'Definido pelo usuário

            cond_isol = 0

            If meio = 0 Then

                cond_isol = 0.7

            ElseIf meio = 1 Then

                cond_isol = 1

            ElseIf meio = 2 Then

                cond_isol = 0.018

            ElseIf meio = 3 Then

                cond_isol = 0.04

            ElseIf meio = 4 Then

                cond_isol = 0.035

            ElseIf meio = 5 Then

                cond_isol = 0.036

            ElseIf meio = 6 Then

                cond_isol = 0.08

            ElseIf meio = 7 Then

                cond_isol = 0

            End If

            'condutividade em W/(m.K)

        End Function

        Function rugosidade(ByVal material As String) As Double

            Dim epsilon As Double
            'rugosidade em metros

            If material = DWSIM.App.GetLocalString("AoComum") Then epsilon = 0.0000457
            If material = DWSIM.App.GetLocalString("AoCarbono") Then epsilon = 0.000045
            If material = DWSIM.App.GetLocalString("FerroBottomido") Then epsilon = 0.000259
            If material = DWSIM.App.GetLocalString("AoInoxidvel") Then epsilon = 0.000045
            If material = "PVC" Then epsilon = 0.0000015
            If material = "PVC+PFRV" Then epsilon = 0.0000015

            rugosidade = epsilon

        End Function

        Function k_parede(ByVal material As String, ByVal T As Double) As Double

            Dim kp As Double
            'condutividade térmica da parede do duto, em W/m.K

            If material = DWSIM.App.GetLocalString("AoComum") Then kp = -0.000000004 * T ^ 3 - 0.00002 * T ^ 2 + 0.021 * T + 33.743
            If material = DWSIM.App.GetLocalString("AoCarbono") Then kp = 0.000000007 * T ^ 3 - 0.00002 * T ^ 2 - 0.0291 * T + 70.765
            If material = DWSIM.App.GetLocalString("FerroBottomido") Then kp = -0.00000008 * T ^ 3 + 0.0002 * T ^ 2 - 0.211 * T + 127.99
            If material = DWSIM.App.GetLocalString("AoInoxidvel") Then kp = 0.000000005 * T ^ 3 - 0.00001 * T ^ 2 + 0.024 * T ^ +8.6226
            If material = "PVC" Then kp = 0.16
            If material = "PVC+PFRV" Then kp = 0.16

            k_parede = kp   'W/m.K

        End Function

        Function k_terreno(ByVal terreno As Integer) As Double

            Dim kt = 0

            If terreno = 2 Then kt = 1.1
            If terreno = 3 Then kt = 1.95
            If terreno = 4 Then kt = 0.5
            If terreno = 5 Then kt = 2.2

            k_terreno = kt

        End Function

        Function CALCULAR_CGTC(ByVal materialparede As String, ByVal EL As Double, ByVal L As Double, _
                            ByVal Dint As Double, ByVal Dext As Double, ByVal rugosidade As Double, _
                            ByVal T As Double, ByVal vel_g As Double, ByVal vel_l As Double, _
                            ByVal Cpl As Double, ByVal Cpv As Double, ByVal kl As Double, ByVal kv As Double, _
                            ByVal mu_l As Double, ByVal mu_v As Double, ByVal rho_l As Double, _
                            ByVal rho_v As Double, ByVal hinterno As Boolean, ByVal isolamento As Boolean, _
                            ByVal parede As Boolean, ByVal hexterno As Boolean)

            If Double.IsNaN(rho_l) Then rho_l = 0.0#

            'Calcular propriedades médias
            Dim vel = vel_g + vel_l
            Dim mu = EL * mu_l + (1 - EL) * mu_v
            Dim rho = EL * rho_l + (1 - EL) * rho_v
            Dim Cp = EL * Cpl + (1 - EL) * Cpv
            Dim k = EL * kl + (1 - EL) * kv
            Dim Cpmist = Cp

            'Cálculo do coeficiente de transf. interno
            Dim U_int As Double

            If hinterno Then

                'Calcular numero de Reynolds interno
                Dim Re_int = NRe(rho, vel, Dint, mu)

                Dim epsilon = Me.rugosidade(materialparede)
                Dim ffint = 0
                If Re_int > 3250 Then
                    Dim a1 = Math.Log(((epsilon / Dint) ^ 1.1096) / 2.8257 + (7.149 / Re_int) ^ 0.8961) / Math.Log(10.0#)
                    Dim b1 = -2 * Math.Log((epsilon / Dint) / 3.7065 - 5.0452 * a1 / Re_int) / Math.Log(10.0#)
                    ffint = (1 / b1) ^ 2
                Else
                    ffint = 64 / Re_int
                End If

                'Calcular número de Prandtl interno
                Dim Pr_int = NPr(Cp, mu, k)

                'Calcular coeficiente de transferência de calor interno
                Dim h_int = hint_petukhov(k, Dint, ffint, Re_int, Pr_int)

                'Calcular contribuição da parte interna
                U_int = h_int

            End If

            'Calcular contribuição da parede da tubulação
            Dim U_parede = 0

            If parede = True Then

                U_parede = k_parede(materialparede, T) / (Math.Log(Dext / Dint) * Dint)
                If Dext = Dint Then U_parede = 0

            End If

            'Calcular contribuição do isolamento
            Dim U_isol = 0

            Dim esp_isol = 0
            If isolamento = True Then

                esp_isol = Me.m_thermalprofile.Espessura / 1000
                U_isol = CDbl(Me.m_thermalprofile.Condtermica) / (Math.Log((Dext + esp_isol) / Dext) * Dext)

            End If

            'Calcular coeficiente de transf. externo
            Dim U_ext = 0

            If hexterno = True Then

                Dim mu2, k2, cp2, rho2 As Double

                If Me.m_thermalprofile.Meio <> 0 And Me.m_thermalprofile.Meio <> 1 Then

                    Dim Zb = CDbl(Me.m_thermalprofile.Velocidade)

                    Dim Rs = (Dext + esp_isol) / (2 * k_terreno(Me.m_thermalprofile.Meio)) * Math.Log((2 * Zb + (4 * Zb ^ 2 - (Dext + esp_isol) ^ 2) ^ 0.5) / (Dext + esp_isol))

                    If Zb > 0 Then
                        U_ext = 1 / Rs
                    Else
                        U_ext = 1000000.0
                    End If

                ElseIf Me.m_thermalprofile.Meio = 0 Then

                    'Calcular propriedades médias
                    vel = CDbl(Me.m_thermalprofile.Velocidade)
                    Dim Tamb = Me.m_thermalprofile.Temp_amb_estimar
                    Dim props = PropsAR(Tamb, 101325)
                    mu2 = props(1)
                    rho2 = props(0)
                    cp2 = props(2)
                    k2 = props(3) / 1000

                    'Calcular numero de Reynolds externo
                    Dim Re_ext = NRe(rho2, vel, (Dext + esp_isol), mu2)

                    'Calcular número de Prandtl externo
                    Dim Pr_ext = NPr(cp2, mu2, k2)

                    'Calcular coeficiente de transferência de calor externo
                    Dim h_ext = hext_holman(k2, (Dext + esp_isol), Re_ext, Pr_ext)

                    'Calcular contribuição da parte externa
                    U_ext = h_ext * (Dext + esp_isol) / Dint

                ElseIf Me.m_thermalprofile.Meio = 1 Then

                    'Calcular propriedades médias
                    vel = CDbl(Me.m_thermalprofile.Velocidade)
                    Dim Tamb = Me.m_thermalprofile.Temp_amb_estimar
                    Dim props = PropsAGUA(Tamb, 101325)
                    mu2 = props(1)
                    rho2 = props(0)
                    cp2 = props(2)
                    k2 = props(3) / 1000

                    'Calcular numero de Reynolds externo
                    Dim Re_ext = NRe(rho2, vel, (Dext + esp_isol), mu2)

                    'Calcular número de Prandtl externo
                    Dim Pr_ext = NPr(cp2, mu2, k2)

                    'Calcular coeficiente de transferência de calor externo
                    Dim h_ext = hext_holman(k2, (Dext + esp_isol), Re_ext, Pr_ext)

                    'Calcular contribuição da parte externa
                    U_ext = h_ext * (Dext + esp_isol) / Dint

                End If

            End If

            'Calcular coeficiente global de transferência de calor
            Dim _U As Double

            If U_int <> 0 Then
                _U = _U + 1 / U_int
            Else
                If hinterno = True Then
                    _U = _U + 1.0E+30
                End If
            End If
            If U_parede <> 0 Then
                _U = _U + 1 / U_parede
            Else
                If parede = True Then
                    _U = _U + 1.0E+30
                End If
            End If
            If U_isol <> 0 Then
                _U = _U + 1 / U_isol
            Else
                If isolamento = True Then
                    _U = _U + 1.0E+30
                End If
            End If
            If U_ext <> 0 Then
                _U = _U + 1 / U_ext
            Else
                If hexterno = True Then
                    _U = _U + 1.0E+30
                End If
            End If

            CALCULAR_CGTC = 1 / _U

        End Function

        Function NRe(ByVal rho As Double, ByVal v As Double, ByVal D As Double, ByVal mu As Double) As Double

            NRe = rho * v * D / mu

        End Function

        Function NPr(ByVal Cp As Double, ByVal mu As Double, ByVal k As Double) As Double

            NPr = Cp * mu / k

        End Function

        Function hext_holman(ByVal k As Double, ByVal Dext As Double, ByVal NRe As Double, ByVal NPr As Double) As Double

            hext_holman = k / Dext * 0.25 * NRe ^ 0.6 * NPr ^ 0.38

        End Function

        Function hint_petukhov(ByVal k, ByVal D, ByVal f, ByVal NRe, ByVal NPr)

            hint_petukhov = k / D * (f / 8) * NRe * NPr / (1.07 + 12.7 * (f / 8) ^ 0.5 * (NPr ^ (2 / 3) - 1))

        End Function

        Function PropsAR(ByVal Tamb As Double, ByVal Pamb As Double)

            Dim T = Tamb

            Dim rho = 314.56 * T ^ -0.9812

            'viscosidade
            Dim mu = rho * (0.000001 * (0.00009 * T ^ 2 + 0.035 * T - 2.9346))

            'capacidade calorífica
            Dim Cp = 0.000000000001 * T ^ 4 - 0.000000003 * T ^ 3 + 0.000002 * T ^ 2 - 0.0008 * T + 1.091

            'condutividade térmica
            Dim k = -0.00000002 * T ^ 2 + 0.00009 * T + 0.0012

            Dim tmp2(3)

            tmp2(0) = rho
            tmp2(1) = mu
            tmp2(2) = Cp
            tmp2(3) = k

            PropsAR = tmp2

        End Function

        Protected m_iapws97 As New IAPWS_IF97

        Function PropsAGUA(ByVal Tamb As Double, ByVal Pamb As Double)

            'massa molar
            Dim mm = 18
            Dim Tc = 647.3
            Dim Pc = 217.6 * 101325
            Dim Vc = 0.000001 * 56
            Dim Zc = 0.229
            Dim w = 0.344
            Dim ZRa = 0.237

            Dim R = 8.314
            Dim P = Pamb
            Dim T = Tamb

            'densidade
            Dim rho = Me.m_iapws97.densW(T, P / 100000)

            'viscosidade
            Dim mu = Me.m_iapws97.viscW(T, P / 100000)

            'capacidade calorífica
            Dim Cp = Me.m_iapws97.cpW(T, P / 100000) * 18

            'condutividade térmica
            Dim k = Me.m_iapws97.thconW(T, P / 100000)

            Dim tmp2(3)

            tmp2(0) = rho
            tmp2(1) = mu
            tmp2(2) = Cp
            tmp2(3) = k

            PropsAGUA = tmp2

        End Function

        Function CALCT2(ByVal U As Double, ByVal DQ As Double, ByVal T1 As Double, ByVal Tamb As Double) As Double

            Dim T, Tinf, Tsup As Double
            Dim fT, fT_inf, nsub, delta_T As Double

START_LOOP:

            If T1 > Tamb Then
                Tinf = Tamb
                Tsup = T1
            Else
                Tinf = T1
                Tsup = Tamb
            End If

            nsub = 5

            delta_T = (Tsup - Tinf) / nsub
            Dim idx As Integer = 0
            Do
                fT = Me.FT2(T1, Tinf, Tamb, U, DQ)
                Tinf = Tinf + delta_T
                fT_inf = Me.FT2(T1, Tinf, Tamb, U, DQ)
                idx += 1
                If Not Double.TryParse(Tinf, New Double) Or Double.IsNaN(Tinf) Or Tinf < 100 Or idx > 100 Then Throw New Exception("Erro ao calcular temperatura")
            Loop Until fT * fT_inf < 0 Or Tinf > Tsup
            'If Tinf > Tsup Then Throw New Exception(DWSIM.App.GetLocalString("Erroaocalculartemper"))
            Tsup = Tinf
            Tinf = Tinf - delta_T

            'método de Brent para encontrar Vc

            Dim aaa, bbb, ccc, ddd, eee, min11, min22, faa, fbb, fcc, ppp, qqq, rrr, sss, tol11, xmm As Double
            Dim ITMAX2 As Integer = 10000
            Dim iter2 As Integer

            aaa = Tinf
            bbb = Tsup
            ccc = Tsup

            faa = Me.FT2(T1, Tinf, aaa, U, DQ)
            fbb = Me.FT2(T1, Tinf, bbb, U, DQ)
            fcc = fbb

            iter2 = 0
            Do
                If (fbb > 0 And fcc > 0) Or (fbb < 0 And fcc < 0) Then
                    ccc = aaa
                    fcc = faa
                    ddd = bbb - aaa
                    eee = ddd
                End If
                If Math.Abs(fcc) < Math.Abs(fbb) Then
                    aaa = bbb
                    bbb = ccc
                    ccc = aaa
                    faa = fbb
                    fbb = fcc
                    fcc = faa
                End If
                tol11 = 0.0000001
                xmm = 0.5 * (ccc - bbb)
                If (Math.Abs(xmm) <= tol11) Or (fbb = 0) Then GoTo Final3
                If (Math.Abs(eee) >= tol11) And (Math.Abs(faa) > Math.Abs(fbb)) Then
                    sss = fbb / faa
                    If aaa = ccc Then
                        ppp = 2 * xmm * sss
                        qqq = 1 - sss
                    Else
                        qqq = faa / fcc
                        rrr = fbb / fcc
                        ppp = sss * (2 * xmm * qqq * (qqq - rrr) - (bbb - aaa) * (rrr - 1))
                        qqq = (qqq - 1) * (rrr - 1) * (sss - 1)
                    End If
                    If ppp > 0 Then qqq = -qqq
                    ppp = Math.Abs(ppp)
                    min11 = 3 * xmm * qqq - Math.Abs(tol11 * qqq)
                    min22 = Math.Abs(eee * qqq)
                    Dim tvar2 As Double
                    If min11 < min22 Then tvar2 = min11
                    If min11 > min22 Then tvar2 = min22
                    If 2 * ppp < tvar2 Then
                        eee = ddd
                        ddd = ppp / qqq
                    Else
                        ddd = xmm
                        eee = ddd
                    End If
                Else
                    ddd = xmm
                    eee = ddd
                End If
                aaa = bbb
                faa = fbb
                If (Math.Abs(ddd) > tol11) Then
                    bbb += ddd
                Else
                    bbb += Math.Sign(xmm) * tol11
                End If
                fbb = Me.FT2(T1, bbb, Tamb, U, DQ)
                iter2 += 1
            Loop Until iter2 = ITMAX2

Final3:     T = bbb

            Return T

        End Function

        Function FT2(ByVal T1 As Double, ByVal T2 As Double, ByVal Tamb As Double, ByVal U As Double, ByVal DQ As Double) As Double

            Dim f As Double
            If T1 < Tamb Then
                f = U * (T1 - T2) / Math.Log((Tamb - T2) / (Tamb - T1)) - DQ
            Else
                f = U * (T2 - T1) / Math.Log((Tamb - T1) / (Tamb - T2)) - DQ
            End If

            'If Double.TryParse(f, New Double) Then
            '    Return f
            'Else
            '    Return Tamb
            'End If
            Return f

        End Function

#End Region

        Public Overloads Overrides Sub UpdatePropertyNodes(ByVal su As SistemasDeUnidades.Unidades, ByVal nf As String)

            Dim Conversor As New DWSIM.SistemasDeUnidades.Conversor
            If Me.NodeTableItems Is Nothing Then
                Me.NodeTableItems = New System.Collections.Generic.Dictionary(Of Integer, DWSIM.Outros.NodeItem)
                Me.FillNodeItems()
            End If

            For Each nti As Outros.NodeItem In Me.NodeTableItems.Values
                nti.Value = GetPropertyValue(nti.Text, FlowSheet.Options.SelectedUnitSystem)
                nti.Unit = GetPropertyUnit(nti.Text, FlowSheet.Options.SelectedUnitSystem)
            Next

            If Me.QTNodeTableItems Is Nothing Then
                Me.QTNodeTableItems = New System.Collections.Generic.Dictionary(Of Integer, DWSIM.Outros.NodeItem)
                Me.QTFillNodeItems()
            End If

            With Me.QTNodeTableItems

                Dim valor As String

                If Me.DeltaP.HasValue Then
                    valor = Format(Conversor.ConverterDoSI(su.spmp_deltaP, Me.DeltaP), nf)
                Else
                    valor = DWSIM.App.GetLocalString("NC")
                End If
                .Item(0).Value = valor
                .Item(0).Unit = su.spmp_deltaP

                If Me.DeltaT.HasValue Then
                    valor = Format(Conversor.ConverterDoSI(su.spmp_deltaT, Me.DeltaT), nf)
                Else
                    valor = DWSIM.App.GetLocalString("NC")
                End If
                .Item(1).Value = valor
                .Item(1).Unit = su.spmp_deltaT

                If Me.DeltaQ.HasValue Then
                    valor = Format(Conversor.ConverterDoSI(su.spmp_heatflow, Me.DeltaQ), nf)
                Else
                    valor = DWSIM.App.GetLocalString("NC")
                End If
                .Item(2).Value = valor
                .Item(2).Unit = su.spmp_heatflow

            End With

        End Sub

        Public Overrides Sub QTFillNodeItems()

            With Me.QTNodeTableItems

                .Clear()

                .Add(0, New DWSIM.Outros.NodeItem("DP", "", "", 0, 0, ""))
                .Add(1, New DWSIM.Outros.NodeItem("DT", "", "", 1, 0, ""))
                .Add(2, New DWSIM.Outros.NodeItem("Q", "", "", 2, 0, ""))

            End With
        End Sub

        Public Overrides Sub PopulatePropertyGrid(ByRef pgrid As PropertyGridEx.PropertyGridEx, ByVal su As SistemasDeUnidades.Unidades)

            Dim Conversor As New DWSIM.SistemasDeUnidades.Conversor

            With pgrid

                .PropertySort = PropertySort.Categorized
                .ShowCustomProperties = True
                .Item.Clear()

                MyBase.PopulatePropertyGrid(pgrid, su)

                Dim ent, saida, energ As String
                If Me.GraphicObject.InputConnectors(0).IsAttached = True Then
                    ent = Me.GraphicObject.InputConnectors(0).AttachedConnector.AttachedFrom.Tag
                Else
                    ent = ""
                End If
                If Me.GraphicObject.OutputConnectors(0).IsAttached = True Then
                    saida = Me.GraphicObject.OutputConnectors(0).AttachedConnector.AttachedTo.Tag
                Else
                    saida = ""
                End If

                If Me.GraphicObject.EnergyConnector.IsAttached = True Then
                    energ = Me.GraphicObject.EnergyConnector.AttachedConnector.AttachedTo.Tag
                Else
                    energ = ""
                End If

                .Item.Add(DWSIM.App.GetLocalString("Correntedeentrada"), ent, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIInputMSSelector
                End With

                .Item.Add(DWSIM.App.GetLocalString("Correntedesada"), saida, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIOutputMSSelector
                End With

                .Item.Add(DWSIM.App.GetLocalString("Correntedeenergia"), energ, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIOutputESSelector
                End With

                .Item.Add(DWSIM.App.GetLocalString("PerfilHidrulico"), Me, "Profile", False, DWSIM.App.GetLocalString("Perfis2"), DWSIM.App.GetLocalString("Cliquenobotocomretic2"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(PipeProfile)
                    .CustomEditor = New DWSIM.Editors.PipeEditor.UIPipeEditor
                    .CustomTypeConverter = New DWSIM.Editors.PipeEditor.PipeEditorConverter
                End With
                .Item.Add(DWSIM.App.GetLocalString("Equaopfluxo"), Me, "SelectedFlowPackage", False, DWSIM.App.GetLocalString("Perfis2"), DWSIM.App.GetLocalString("Selecioneaequaoparac"), True)
                .Item.Add("Status", Me.Profile, "Status", True, DWSIM.App.GetLocalString("Perfis2"), "", True)
                .Item.Add(DWSIM.App.GetLocalString("PerfilTrmico"), Me, "ThermalProfile", False, DWSIM.App.GetLocalString("Perfis2"), DWSIM.App.GetLocalString("Cliquenobotocomretic3"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(Global.DWSIM.DWSIM.Editors.PipeEditor.ThermalEditorDefinitions)
                    .CustomEditor = New DWSIM.Editors.PipeEditor.UIThermalProfileEditor
                    .CustomTypeConverter = New DWSIM.Editors.PipeEditor.ThermalProfileEditorConverter
                End With

                .Item.Add(DWSIM.App.GetLocalString("MximodeIteraesemP"), Me, "MaxPressureIterations", False, DWSIM.App.GetLocalString("Parmetros3"), "", True)
                .Item.Add(DWSIM.App.GetLocalString("MximodeIteraesemT"), Me, "MaxTemperatureIterations", False, DWSIM.App.GetLocalString("Parmetros3"), "", True)
                .Item.Add(DWSIM.App.GetLocalString("IncludeJTEffect"), Me, "IncludeJTEffect", False, DWSIM.App.GetLocalString("Parmetros3"), "", True)
                '.Item.Add(DWSIM.App.GetLocalString("Tolernciapreclculode") & " (%)", Me, "TriggerFlashP", False, DWSIM.App.GetLocalString("Parmetros3"), "", True)
                '.Item.Add(DWSIM.App.GetLocalString("Tolernciapreclculode2") & " (%)", Me, "TriggerFlashT", False, DWSIM.App.GetLocalString("Parmetros3"), "", True)
                .Item.Add(DWSIM.App.GetLocalString("Erromximodapresso") & " (Pa)", Me, "TolP", False, DWSIM.App.GetLocalString("Parmetros3"), "", True)
                .Item.Add(DWSIM.App.GetLocalString("Erromximodatemperatu") & " (K)", Me, "TolT", False, DWSIM.App.GetLocalString("Parmetros3"), "", True)

                .Item.Add(FT("Delta P", su.spmp_deltaP), Format(Conversor.ConverterDoSI(su.spmp_deltaP, Me.DeltaP.GetValueOrDefault), FlowSheet.Options.NumberFormat), True, DWSIM.App.GetLocalString("Resultados4"), DWSIM.App.GetLocalString("Diferenadepressoentr"), True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .DefaultType = GetType(Nullable(Of Double))
                End With

                .Item.Add(FT(DWSIM.App.GetLocalString("DeltaT2"), su.spmp_deltaT), Format(Conversor.ConverterDoSI(su.spmp_deltaT, Me.DeltaT.GetValueOrDefault), FlowSheet.Options.NumberFormat), True, DWSIM.App.GetLocalString("Resultados4"), DWSIM.App.GetLocalString("Diferenadetemperatur"), True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .DefaultType = GetType(Nullable(Of Double))
                End With

                .Item.Add(FT(DWSIM.App.GetLocalString("Calortrocado"), su.spmp_heatflow), Format(Conversor.ConverterDoSI(su.spmp_heatflow, Me.DeltaQ.GetValueOrDefault), FlowSheet.Options.NumberFormat), True, DWSIM.App.GetLocalString("Resultados4"), DWSIM.App.GetLocalString("Quantidadedecalortro"), True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .DefaultType = GetType(Nullable(Of Double))
                End With

                .Item.Add(DWSIM.App.GetLocalString("Tabela"), Me, "Profile", False, DWSIM.App.GetLocalString("Resultados4"), DWSIM.App.GetLocalString("Cliquenobotocomretic4"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(Global.DWSIM.DWSIM.Editors.PipeEditor.ThermalEditorDefinitions)
                    .CustomEditor = New DWSIM.Editors.Results.UIFormTable
                End With

                .Item.Add(DWSIM.App.GetLocalString("Grfico"), Me, "Profile", False, DWSIM.App.GetLocalString("Resultados4"), DWSIM.App.GetLocalString("Cliquenobotocomretic5"), True)
                With .Item(.Item.Count - 1)
                    .DefaultType = GetType(Global.DWSIM.DWSIM.Editors.PipeEditor.ThermalEditorDefinitions)
                    .CustomEditor = New DWSIM.Editors.Results.UIFormGraph
                End With

                If Me.GraphicObject.Calculated = False Then
                    .Item.Add(DWSIM.App.GetLocalString("Mensagemdeerro"), Me, "ErrorMessage", True, DWSIM.App.GetLocalString("Miscelnea5"), DWSIM.App.GetLocalString("Mensagemretornadaqua"), True)
                    With .Item(.Item.Count - 1)
                        .DefaultType = GetType(System.String)
                    End With
                End If

                If Me.IsSpecAttached = True Then
                    .Item.Add(DWSIM.App.GetLocalString("ObjetoUtilizadopor"), FlowSheet.Collections.ObjectCollection(Me.AttachedSpecId).GraphicObject.Tag, True, DWSIM.App.GetLocalString("Miscelnea5"), "", True)
                    .Item.Add(DWSIM.App.GetLocalString("Utilizadocomo"), Me.SpecVarType, True, DWSIM.App.GetLocalString("Miscelnea5"), "", True)
                End If

                If Not Me.Annotation Is Nothing Then
                    .Item.Add(DWSIM.App.GetLocalString("Anotaes"), Me, "Annotation", False, DWSIM.App.GetLocalString("Outros"), DWSIM.App.GetLocalString("Cliquenobotocomretic"), True)
                    With .Item(.Item.Count - 1)
                        .IsBrowsable = False
                        .CustomEditor = New DWSIM.Editors.Annotation.UIAnnotationEditor
                    End With
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
                    'PROP_PS_0	Pressure Drop
                    value = cv.ConverterDoSI(su.spmp_deltaP, Me.DeltaP.GetValueOrDefault)
                Case 1
                    'PROP_PS_1	Temperature Drop
                    value = cv.ConverterDoSI(su.spmp_deltaT, Me.DeltaT.GetValueOrDefault)
                Case 2
                    'PROP_PS_2	Heat Exchanged
                    value = cv.ConverterDoSI(su.spmp_heatflow, Me.DeltaQ.GetValueOrDefault)
            End Select

            Return value

        End Function


        Public Overloads Overrides Function GetProperties(ByVal proptype As SimulationObjects_BaseClass.PropertyType) As String()
            Dim i As Integer = 0
            Dim proplist As New ArrayList
            Select Case proptype
                Case PropertyType.RO
                    For i = 0 To 2
                        proplist.Add("PROP_PS_" + CStr(i))
                    Next
                Case PropertyType.WR
                Case PropertyType.ALL
                    For i = 0 To 2
                        proplist.Add("PROP_PS_" + CStr(i))
                    Next
            End Select
            Return proplist.ToArray(GetType(System.String))
            proplist = Nothing
        End Function

        Public Overrides Function SetPropertyValue(ByVal prop As String, ByVal propval As Object, Optional ByVal su As DWSIM.SistemasDeUnidades.Unidades = Nothing) As Object
            Return 0
        End Function

        Public Overrides Function GetPropertyUnit(ByVal prop As String, Optional ByVal su As SistemasDeUnidades.Unidades = Nothing) As Object
            If su Is Nothing Then su = New DWSIM.SistemasDeUnidades.UnidadesSI
            Dim value As String = ""
            Dim propidx As Integer = CInt(prop.Split("_")(2))

            Select Case propidx

                Case 0
                    'PROP_PS_0	Pressure Drop
                    value = su.spmp_deltaP
                Case 1
                    'PROP_PS_1	Temperature Drop
                    value = su.spmp_deltaT
                Case 2
                    'PROP_PS_2	Heat Exchanged
                    value = su.spmp_heatflow

            End Select

            Return value
        End Function
    End Class

End Namespace