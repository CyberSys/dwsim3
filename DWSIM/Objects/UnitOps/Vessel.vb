'    Separator Vessel Calculation Routines 
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

Imports Microsoft.MSDN.Samples.GraphicObjects
Imports DWSIM.DWSIM.Flowsheet.FlowSheetSolver

Namespace DWSIM.SimulationObjects.UnitOps

    <System.Serializable()> Public Class Vessel

        Inherits SimulationObjects_UnitOpBaseClass

        Protected m_DQ As Nullable(Of Double)

        Protected m_overrideT As Boolean = False
        Protected m_overrideP As Boolean = False
        Protected m_T As Double = 298.15#
        Protected m_P As Double = 101325.0#

        Public Enum FlashSpec
            PH
            PVF
        End Enum

        Public Property FlashSpecification As FlashSpec = FlashSpec.PH

        Public Enum PressureBehavior
            Average
            Maximum
            Minimum
        End Enum

        Protected m_pressurebehavior As PressureBehavior = PressureBehavior.Minimum

        Public Property PressureCalculation() As PressureBehavior
            Get
                Return Me.m_pressurebehavior
            End Get
            Set(ByVal value As PressureBehavior)
                Me.m_pressurebehavior = value
            End Set
        End Property

        Public Enum OperationMode
            TwoPhase = 0
            ThreePhase = 1
        End Enum

        Public Property OverrideT() As Boolean
            Get
                Return m_overrideT
            End Get
            Set(ByVal value As Boolean)
                m_overrideT = value
            End Set
        End Property

        Public Property OverrideP() As Boolean
            Get
                Return m_overrideP
            End Get
            Set(ByVal value As Boolean)
                m_overrideP = value
            End Set
        End Property

        Public Property FlashPressure() As Double
            Get
                Return m_P
            End Get
            Set(ByVal value As Double)
                m_P = value
            End Set
        End Property

        Public Property FlashTemperature() As Double
            Get
                Return m_T
            End Get
            Set(ByVal value As Double)
                m_T = value
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

        Public Sub New()
            MyBase.New()
        End Sub

        Public Sub New(ByVal nome As String, ByVal descricao As String)

            MyBase.CreateNew()
            Me.m_ComponentName = nome
            Me.m_ComponentDescription = descricao
            Me.FillNodeItems()
            Me.QTFillNodeItems()
            Me.ShowQuickTable = False

        End Sub

        Public Overrides Function Calculate(Optional ByVal args As Object = Nothing) As Integer

            Dim form As FormFlowsheet = Me.FlowSheet
            Dim objargs As New DWSIM.Outros.StatusChangeEventArgs

           If Not Me.GraphicObject.OutputConnectors(0).IsAttached Then
                'Call function to calculate flowsheet
                With objargs
                    .Calculado = False
                    .Nome = Me.Nome
                    .Tipo = TipoObjeto.Vessel
                End With
                CalculateFlowsheet(FlowSheet, objargs, Nothing)
                Throw New Exception(DWSIM.App.GetLocalString("Verifiqueasconexesdo"))
            ElseIf Not Me.GraphicObject.OutputConnectors(1).IsAttached Then
                'Call function to calculate flowsheet
                With objargs
                    .Calculado = False
                    .Nome = Me.Nome
                    .Tipo = TipoObjeto.Vessel
                End With
                CalculateFlowsheet(FlowSheet, objargs, Nothing)
                Throw New Exception(DWSIM.App.GetLocalString("Verifiqueasconexesdo"))
            End If

            Dim E0 As Double = 0.0#

            If Me.OverrideP Or Me.OverrideT Or Me.FlashSpecification = FlashSpec.PVF Then
                If Not Me.GraphicObject.InputConnectors(6).IsAttached Then
                    'Call function to calculate flowsheet
                    With objargs
                        .Calculado = False
                        .Nome = Me.Nome
                        .Tipo = TipoObjeto.Vessel
                    End With
                    CalculateFlowsheet(FlowSheet, objargs, Nothing)
                    Throw New Exception(DWSIM.App.GetLocalString("EnergyStreamRequired"))
                End If
            Else
                If Me.GraphicObject.InputConnectors(6).IsAttached Then
                    E0 = form.Collections.CLCS_EnergyStreamCollection(Me.GraphicObject.InputConnectors(6).AttachedConnector.AttachedFrom.Name).Energia.GetValueOrDefault
                End If
            End If

            Dim H, Hs, T, W, M, We, P, VF, Hf, H0 As Double
            H = 0
            Hs = 0
            T = 0
            W = 0
            We = 0
            P = 0
            VF = 0.0#

            Dim i As Integer = 1
            Dim nc As Integer = 0

            Dim ms As DWSIM.SimulationObjects.Streams.MaterialStream
            Dim mix As New DWSIM.SimulationObjects.Streams.MaterialStream("", "", Me.FlowSheet, Me.PropertyPackage)
            Me.FlowSheet.AddComponentsRows(mix)
            Dim cp As ConnectionPoint

            For Each cp In Me.GraphicObject.InputConnectors
                If cp.IsAttached And cp.Type = ConType.ConIn Then
                    nc += 1
                    If cp.AttachedConnector.AttachedFrom.Calculated = False Then Throw New Exception(DWSIM.App.GetLocalString("Umaoumaiscorrentesna"))
                    ms = form.Collections.CLCS_MaterialStreamCollection(cp.AttachedConnector.AttachedFrom.Name)
                    ms.Validate()
                    If Me.PressureCalculation = PressureBehavior.Minimum Then
                        If ms.Fases(0).SPMProperties.pressure.GetValueOrDefault < P Then
                            P = ms.Fases(0).SPMProperties.pressure
                        ElseIf P = 0 Then
                            P = ms.Fases(0).SPMProperties.pressure
                        End If
                    ElseIf Me.PressureCalculation = PressureBehavior.Maximum Then
                        If ms.Fases(0).SPMProperties.pressure.GetValueOrDefault > P Then
                            P = ms.Fases(0).SPMProperties.pressure
                        ElseIf P = 0 Then
                            P = ms.Fases(0).SPMProperties.pressure
                        End If
                    Else
                        P = P + ms.Fases(0).SPMProperties.pressure.GetValueOrDefault
                        i += 1
                    End If
                    M += ms.Fases(0).SPMProperties.molarflow.GetValueOrDefault
                    We = ms.Fases(0).SPMProperties.massflow.GetValueOrDefault
                    W += We
                    VF += ms.Fases(2).SPMProperties.molarfraction.GetValueOrDefault * ms.Fases(0).SPMProperties.molarflow.GetValueOrDefault
                    If Not Double.IsNaN(ms.Fases(0).SPMProperties.enthalpy.GetValueOrDefault) Then H += We * ms.Fases(0).SPMProperties.enthalpy.GetValueOrDefault
                End If
            Next

            If M <> 0.0# Then VF /= M

            If W <> 0.0# Then Hs = (H + E0) / W Else Hs = 0.0#

            H0 = H

            If Me.PressureCalculation = PressureBehavior.Average Then P = P / (i - 1)

            T = 0

            Dim n As Integer = form.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.OutputConnectors(0).AttachedConnector.AttachedTo.Name).Fases(0).Componentes.Count
            Dim Vw As New Dictionary(Of String, Double)
            For Each cp In Me.GraphicObject.InputConnectors
                If cp.IsAttached And cp.Type = ConType.ConIn Then
                    ms = form.Collections.CLCS_MaterialStreamCollection(cp.AttachedConnector.AttachedFrom.Name)
                    Dim comp As DWSIM.ClassesBasicasTermodinamica.Substancia
                    For Each comp In ms.Fases(0).Componentes.Values
                        If Not Vw.ContainsKey(comp.Nome) Then
                            Vw.Add(comp.Nome, 0)
                        End If
                        Vw(comp.Nome) += comp.FracaoMassica.GetValueOrDefault * ms.Fases(0).SPMProperties.massflow.GetValueOrDefault
                    Next
                    If W <> 0.0# Then T += ms.Fases(0).SPMProperties.massflow.GetValueOrDefault / W * ms.Fases(0).SPMProperties.temperature.GetValueOrDefault
                End If
            Next

            If W = 0.0# Then T = 273.15

            CheckSpec(Hs, False, "enthalpy")
            CheckSpec(W, True, "mass flow")
            CheckSpec(P, True, "pressure")

            With mix

                If W <> 0.0# Then .Fases(0).SPMProperties.enthalpy = Hs
                .Fases(0).SPMProperties.pressure = P
                .Fases(0).SPMProperties.massflow = W
                .Fases(0).SPMProperties.molarfraction = 1
                .Fases(0).SPMProperties.massfraction = 1
                .Fases(2).SPMProperties.molarfraction = VF
                Dim comp As DWSIM.ClassesBasicasTermodinamica.Substancia
                For Each comp In .Fases(0).Componentes.Values
                    If W <> 0.0# Then comp.FracaoMassica = Vw(comp.Nome) / W
                Next
                Dim mass_div_mm As Double = 0
                Dim sub1 As DWSIM.ClassesBasicasTermodinamica.Substancia
                For Each sub1 In .Fases(0).Componentes.Values
                    mass_div_mm += sub1.FracaoMassica.GetValueOrDefault / sub1.ConstantProperties.Molar_Weight
                Next
                For Each sub1 In .Fases(0).Componentes.Values
                    If W <> 0.0# Then
                        sub1.FracaoMolar = sub1.FracaoMassica.GetValueOrDefault / sub1.ConstantProperties.Molar_Weight / mass_div_mm
                    Else
                        sub1.FracaoMolar = 0.0#
                    End If
                Next
                Me.PropertyPackage.CurrentMaterialStream = mix
                .Fases(0).SPMProperties.temperature = T
                .Fases(0).SPMProperties.molarflow = W / Me.PropertyPackage.AUX_MMM(PropertyPackages.Fase.Mixture) * 1000

            End With

            If Me.OverrideT = False And Me.OverrideP = False Then

                W = mix.Fases(0).SPMProperties.massflow.GetValueOrDefault
                Dim j As Integer = 0

                With Me.PropertyPackage

                    'calculate mixture stream
                    If .AUX_IS_SINGLECOMP(PropertyPackages.Fase.Mixture) Then
                        'if it is a single compound stream, needs to calculate a PH-Flash to get phase distribution correctly.
                        .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.H)
                    Else
                        Select Case Me.FlashSpecification
                            Case FlashSpec.PH
                                .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.H)
                            Case FlashSpec.PVF
                                .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.VAP)
                        End Select
                    End If
                    If mix.Fases(3).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                        .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid1)
                    Else
                        .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid1)
                    End If
                    If mix.Fases(4).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                        .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid2)
                    Else
                        .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid2)
                    End If
                    If mix.Fases(5).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                        .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid3)
                    Else
                        .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid3)
                    End If
                    If mix.Fases(6).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                        .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Aqueous)
                    Else
                        .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Aqueous)
                    End If
                    If mix.Fases(7).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                        .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Solid)
                    Else
                        .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Solid)
                    End If
                    If mix.Fases(2).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                        .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                    Else
                        .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                    End If
                    If mix.Fases(2).SPMProperties.molarfraction.GetValueOrDefault >= 0 And mix.Fases(2).SPMProperties.molarfraction.GetValueOrDefault < 1 Then
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

            Else

                W = mix.Fases(0).SPMProperties.massflow.GetValueOrDefault

                If Me.OverrideP Then
                    P = Me.FlashPressure
                    mix.Fases(0).SPMProperties.pressure = P
                Else
                    P = mix.Fases(0).SPMProperties.pressure.GetValueOrDefault
                End If
                If Me.OverrideT Then
                    T = Me.FlashTemperature
                    mix.Fases(0).SPMProperties.temperature = T
                Else
                    T = mix.Fases(0).SPMProperties.temperature.GetValueOrDefault
                End If

                Me.PropertyPackage.CurrentMaterialStream = mix

                mix.PropertyPackage = Me.PropertyPackage

                mix.Calculate(True, True)

            End If

            'Calculate distribution of solids into liquid outlet streams
            'Solids are distributed between liquid phases in the same ratio as the mass ratio of liquid phases
            Dim SR, VnL1(n - 1), VnL2(n - 1), VmL1(n - 1), VmL2(n - 1) As Double
            Dim HL1, HL2, W1, W2, WL1, WL2, WS As Double
            WL1 = mix.Fases(3).SPMProperties.massflow.GetValueOrDefault
            WL2 = mix.Fases(4).SPMProperties.massflow.GetValueOrDefault
            If WL2 > 0 Then
                SR = WL1 / (WL1 + WL2)
            Else
                SR = 1
            End If
            i = 0
            For Each comp In mix.Fases(0).Componentes.Values
                VnL1(i) = mix.Fases(3).Componentes(comp.Nome).MolarFlow.GetValueOrDefault + SR * mix.Fases(7).Componentes(comp.Nome).MolarFlow.GetValueOrDefault
                VmL1(i) = mix.Fases(3).Componentes(comp.Nome).MassFlow.GetValueOrDefault + SR * mix.Fases(7).Componentes(comp.Nome).MassFlow.GetValueOrDefault
                VnL2(i) = mix.Fases(4).Componentes(comp.Nome).MolarFlow.GetValueOrDefault + (1 - SR) * mix.Fases(7).Componentes(comp.Nome).MolarFlow.GetValueOrDefault
                VmL2(i) = mix.Fases(4).Componentes(comp.Nome).MassFlow.GetValueOrDefault + (1 - SR) * mix.Fases(7).Componentes(comp.Nome).MassFlow.GetValueOrDefault
                i += 1
            Next
            If VnL1.SumY > 0.0# Then VnL1 = VnL1.NormalizeY
            If VmL1.SumY > 0.0# Then VmL1 = VmL1.NormalizeY
            If VnL2.SumY > 0.0# Then VnL2 = VnL2.NormalizeY
            If VmL2.SumY > 0.0# Then VmL2 = VmL2.NormalizeY
            WL1 = mix.Fases(3).SPMProperties.massflow.GetValueOrDefault
            WL2 = mix.Fases(4).SPMProperties.massflow.GetValueOrDefault
            WS = mix.Fases(7).SPMProperties.massflow.GetValueOrDefault
            W1 = WL1 + SR * WS
            W2 = WL2 + (1 - SR) * WS
            HL1 = (WL1 * mix.Fases(3).SPMProperties.enthalpy.GetValueOrDefault + WS * SR * mix.Fases(7).SPMProperties.enthalpy.GetValueOrDefault) / (WL1 + WS * SR)
            HL2 = (WL2 * mix.Fases(4).SPMProperties.enthalpy.GetValueOrDefault + WS * (1 - SR) * mix.Fases(7).SPMProperties.enthalpy.GetValueOrDefault) / (WL2 + WS * (1 - SR))

            'Me.PropertyPackage.CurrentMaterialStream = mix
            'If Double.IsNaN(VnL1.SumY) Then VnL1 = mix.PropertyPackage.RET_NullVector
            'If Double.IsNaN(VmL1.SumY) Then VmL1 = mix.PropertyPackage.RET_NullVector
            'If Double.IsNaN(VnL2.SumY) Then VnL2 = mix.PropertyPackage.RET_NullVector
            'If Double.IsNaN(VmL2.SumY) Then VmL2 = mix.PropertyPackage.RET_NullVector

            If Double.IsNaN(HL1) Then HL1 = 0.0#
            If Double.IsNaN(HL2) Then HL2 = 0.0#
            If Double.IsNaN(WL1) Then WL1 = 0.0#
            If Double.IsNaN(WL2) Then WL2 = 0.0#

            cp = Me.GraphicObject.OutputConnectors(0) 'vapour phase
            If cp.IsAttached Then
                ms = form.Collections.CLCS_MaterialStreamCollection(cp.AttachedConnector.AttachedTo.Name)
                With ms
                    .ClearAllProps()
                    .SpecType = Streams.MaterialStream.Flashspec.Pressure_and_Enthalpy
                    .Fases(0).SPMProperties.temperature = T
                    .Fases(0).SPMProperties.pressure = P
                    .Fases(0).SPMProperties.enthalpy = mix.Fases(2).SPMProperties.enthalpy
                    .Fases(0).SPMProperties.massflow = mix.Fases(2).SPMProperties.massflow
                    Dim comp As DWSIM.ClassesBasicasTermodinamica.Substancia
                    For Each comp In .Fases(0).Componentes.Values
                        comp.FracaoMolar = mix.Fases(2).Componentes(comp.Nome).FracaoMolar
                        comp.FracaoMassica = mix.Fases(2).Componentes(comp.Nome).FracaoMassica
                    Next
                End With
            End If

            cp = Me.GraphicObject.OutputConnectors(1) 'liquid 1
            If cp.IsAttached Then
                ms = form.Collections.CLCS_MaterialStreamCollection(cp.AttachedConnector.AttachedTo.Name)
                With ms
                    .ClearAllProps()
                    .SpecType = Streams.MaterialStream.Flashspec.Pressure_and_Enthalpy
                    .Fases(0).SPMProperties.temperature = T
                    .Fases(0).SPMProperties.pressure = P
                    If W1 > 0.0# Then .Fases(0).SPMProperties.massflow = W1 Else .Fases(0).SPMProperties.molarflow = 0.0#
                    .Fases(0).SPMProperties.enthalpy = HL1
                    Dim comp As DWSIM.ClassesBasicasTermodinamica.Substancia
                    i = 0
                    For Each comp In .Fases(0).Componentes.Values
                        comp.FracaoMolar = VnL1(i)
                        comp.FracaoMassica = VmL1(i)
                        i += 1
                    Next
                End With
            End If

            If mix.Fases(4).SPMProperties.massflow.GetValueOrDefault > 0.0# Then
                cp = Me.GraphicObject.OutputConnectors(2) 'liquid 2
                If cp.IsAttached Then
                    ms = form.Collections.CLCS_MaterialStreamCollection(cp.AttachedConnector.AttachedTo.Name)
                    With ms
                        .ClearAllProps()
                        .SpecType = Streams.MaterialStream.Flashspec.Pressure_and_Enthalpy
                        .Fases(0).SPMProperties.temperature = T
                        .Fases(0).SPMProperties.pressure = P
                        If W2 > 0.0# Then .Fases(0).SPMProperties.massflow = W2 Else .Fases(0).SPMProperties.molarflow = 0.0#
                        .Fases(0).SPMProperties.enthalpy = HL2
                        Dim comp As DWSIM.ClassesBasicasTermodinamica.Substancia
                        i = 0
                        For Each comp In .Fases(0).Componentes.Values
                            comp.FracaoMolar = VnL2(i)
                            comp.FracaoMassica = VmL2(i)
                            i += 1
                        Next
                    End With
                Else
                    Throw New Exception(DWSIM.App.GetLocalString("SeparatorVessel_SecondLiquidPhaseFound"))
                End If
            End If

            Hf = mix.Fases(0).SPMProperties.enthalpy.GetValueOrDefault * W

            Me.DeltaQ = Hf - H0

            'Energy stream - update power value (kJ/s)
            If Me.GraphicObject.InputConnectors(6).IsAttached Then
                With form.Collections.CLCS_EnergyStreamCollection(Me.GraphicObject.InputConnectors(6).AttachedConnector.AttachedFrom.Name)
                    .Energia = Me.DeltaQ.GetValueOrDefault
                    .GraphicObject.Calculated = True
                End With
            End If

            'Call function to calculate flowsheet
            With objargs
                .Calculado = True
                .Nome = Me.Nome
                .Tag = Me.GraphicObject.Tag
                .Tipo = TipoObjeto.Vessel
            End With

            form.CalculationQueue.Enqueue(objargs)

        End Function

        Public Overrides Function DeCalculate() As Integer

            'If Not Me.GraphicObject.InputConnectors(0).IsAttached Then Throw New Exception(DWSIM.App.GetLocalString("Nohcorrentedematriac10"))
            'If Not Me.GraphicObject.OutputConnectors(0).IsAttached Then Throw New Exception(DWSIM.App.GetLocalString("Nohcorrentedematriac11"))
            'If Not Me.GraphicObject.OutputConnectors(1).IsAttached Then Throw New Exception(DWSIM.App.GetLocalString("Nohcorrentedematriac11"))

            Dim form As Global.DWSIM.FormFlowsheet = Me.Flowsheet

            Dim j As Integer = 0

            Dim ms As DWSIM.SimulationObjects.Streams.MaterialStream
            Dim cp As ConnectionPoint

            cp = Me.GraphicObject.OutputConnectors(0)
            If cp.IsAttached Then
                ms = form.Collections.CLCS_MaterialStreamCollection(cp.AttachedConnector.AttachedTo.Name)
                With ms
                    .Fases(0).SPMProperties.temperature = Nothing
                    .Fases(0).SPMProperties.pressure = Nothing
                    .Fases(0).SPMProperties.enthalpy = Nothing
                    Dim comp As DWSIM.ClassesBasicasTermodinamica.Substancia
                    j = 0
                    For Each comp In .Fases(0).Componentes.Values
                        comp.FracaoMolar = 0
                        comp.FracaoMassica = 0
                        j += 1
                    Next
                    .Fases(0).SPMProperties.massflow = Nothing
                    .Fases(0).SPMProperties.massfraction = 1
                    .Fases(0).SPMProperties.molarfraction = 1
                    .GraphicObject.Calculated = False
                End With
            End If

            cp = Me.GraphicObject.OutputConnectors(1)
            If cp.IsAttached Then
                ms = form.Collections.CLCS_MaterialStreamCollection(cp.AttachedConnector.AttachedTo.Name)
                With ms
                    .Fases(0).SPMProperties.temperature = Nothing
                    .Fases(0).SPMProperties.pressure = Nothing
                    .Fases(0).SPMProperties.enthalpy = Nothing
                    Dim comp As DWSIM.ClassesBasicasTermodinamica.Substancia
                    j = 0
                    For Each comp In .Fases(0).Componentes.Values
                        comp.FracaoMolar = 0
                        comp.FracaoMassica = 0
                        j += 1
                    Next
                    .Fases(0).SPMProperties.massflow = Nothing
                    .Fases(0).SPMProperties.massfraction = 1
                    .Fases(0).SPMProperties.molarfraction = 1
                    .GraphicObject.Calculated = False
                End With
            End If

            'Call function to calculate flowsheet
            Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
            With objargs
                .Calculado = False
                .Nome = Me.Nome
                .Tipo = TipoObjeto.Vessel
            End With

            form.CalculationQueue.Enqueue(objargs)

        End Function

        Public Overloads Overrides Sub UpdatePropertyNodes(ByVal su As SistemasDeUnidades.Unidades, ByVal nf As String)

        End Sub

        Public Overrides Sub QTFillNodeItems()

        End Sub

        Public Overrides Sub PopulatePropertyGrid(ByVal pgrid As PropertyGridEx.PropertyGridEx, ByVal su As SistemasDeUnidades.Unidades)

            Dim Conversor As New DWSIM.SistemasDeUnidades.Conversor

            With pgrid

                .PropertySort = PropertySort.Categorized
                .ShowCustomProperties = True
                .Item.Clear()

                MyBase.PopulatePropertyGrid(pgrid, su)

                Dim ent1, ent2, ent3, ent4, ent5, ent6, saida1, saida2, saida3, energ As String
                If Me.GraphicObject.InputConnectors(0).IsAttached = True Then
                    ent1 = Me.GraphicObject.InputConnectors(0).AttachedConnector.AttachedFrom.Tag
                Else
                    ent1 = ""
                End If
                If Me.GraphicObject.InputConnectors(1).IsAttached = True Then
                    ent2 = Me.GraphicObject.InputConnectors(1).AttachedConnector.AttachedFrom.Tag
                Else
                    ent2 = ""
                End If
                If Me.GraphicObject.InputConnectors(2).IsAttached = True Then
                    ent3 = Me.GraphicObject.InputConnectors(2).AttachedConnector.AttachedFrom.Tag
                Else
                    ent3 = ""
                End If
                If Me.GraphicObject.InputConnectors(3).IsAttached = True Then
                    ent4 = Me.GraphicObject.InputConnectors(3).AttachedConnector.AttachedFrom.Tag
                Else
                    ent4 = ""
                End If
                If Me.GraphicObject.InputConnectors(4).IsAttached = True Then
                    ent5 = Me.GraphicObject.InputConnectors(4).AttachedConnector.AttachedFrom.Tag
                Else
                    ent5 = ""
                End If
                If Me.GraphicObject.InputConnectors(5).IsAttached = True Then
                    ent6 = Me.GraphicObject.InputConnectors(5).AttachedConnector.AttachedFrom.Tag
                Else
                    ent6 = ""
                End If
                If Me.GraphicObject.OutputConnectors(0).IsAttached = True Then
                    saida1 = Me.GraphicObject.OutputConnectors(0).AttachedConnector.AttachedTo.Tag
                Else
                    saida1 = ""
                End If
                If Me.GraphicObject.OutputConnectors(1).IsAttached = True Then
                    saida2 = Me.GraphicObject.OutputConnectors(1).AttachedConnector.AttachedTo.Tag
                Else
                    saida2 = ""
                End If
                If Me.GraphicObject.OutputConnectors(2).IsAttached = True Then
                    saida3 = Me.GraphicObject.OutputConnectors(2).AttachedConnector.AttachedTo.Tag
                Else
                    saida3 = ""
                End If
                If Me.GraphicObject.InputConnectors(6).IsAttached = True Then
                    energ = Me.GraphicObject.InputConnectors(6).AttachedConnector.AttachedFrom.Tag
                Else
                    energ = ""
                End If

                .Item.Add(DWSIM.App.GetLocalString("Correntedeentrada1"), ent1, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIInputMSSelector
                End With
                .Item.Add(DWSIM.App.GetLocalString("Correntedeentrada2"), ent2, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIInputMSSelector
                End With
                .Item.Add(DWSIM.App.GetLocalString("Correntedeentrada3"), ent3, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIInputMSSelector
                End With
                .Item.Add(DWSIM.App.GetLocalString("Correntedeentrada4"), ent4, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIInputMSSelector
                End With
                .Item.Add(DWSIM.App.GetLocalString("Correntedeentrada5"), ent5, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIInputMSSelector
                End With
                .Item.Add(DWSIM.App.GetLocalString("Correntedeentrada6"), ent6, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIInputMSSelector
                End With

                .Item.Add(DWSIM.App.GetLocalString("Saidadevapor"), saida1, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIOutputMSSelector
                End With

                .Item.Add(DWSIM.App.GetLocalString("Saidadelquido"), saida2, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIOutputMSSelector
                End With

                .Item.Add(DWSIM.App.GetLocalString("Saidadelquido") & " (2)", saida3, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIOutputMSSelector
                End With

                .Item.Add(DWSIM.App.GetLocalString("Correntedeenergia"), energ, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIInputESSelector
                End With

                .Item.Add(DWSIM.App.GetLocalString("Pressoajusante"), Me, "PressureCalculation", False, DWSIM.App.GetLocalString("Parmetros2"), DWSIM.App.GetLocalString("Selecioneumaopoquein"), True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                End With

                .Item.Add(DWSIM.App.GetLocalString("FlashSpecification"), Me, "FlashSpecification", False, DWSIM.App.GetLocalString("Parmetros2"), DWSIM.App.GetLocalString("FlashSpecificationDesc"), True)

                .Item.Add(DWSIM.App.GetLocalString("SobreporTemperaturad"), Me, "OverrideT", False, DWSIM.App.GetLocalString("Parmetros2"), DWSIM.App.GetLocalString("SelecioLiquidrueparaign4"), True)
                If Me.OverrideT Then
                    Dim valor = Format(Conversor.ConverterDoSI(su.spmp_temperature, Me.FlashTemperature), FlowSheet.Options.NumberFormat)
                    .Item.Add(FT(DWSIM.App.GetLocalString("Temperatura"), su.spmp_temperature), Double.Parse(valor), False, DWSIM.App.GetLocalString("Parmetros2"), DWSIM.App.GetLocalString("Temperaturadeseparao"), True)
                    With .Item(.Item.Count - 1)
                        .Tag = New Object() {FlowSheet.Options.NumberFormat, su.spmp_temperature, "T"}
                        .CustomEditor = New DWSIM.Editors.Generic.UIUnitConverter
                    End With
                End If
                .Item.Add(DWSIM.App.GetLocalString("SobreporPressodesepa"), Me, "OverrideP", False, DWSIM.App.GetLocalString("Parmetros2"), DWSIM.App.GetLocalString("SelecioLiquidrueparaign5"), True)
                If Me.OverrideP Then
                    Dim valor = Format(Conversor.ConverterDoSI(su.spmp_pressure, Me.FlashPressure), FlowSheet.Options.NumberFormat)
                    .Item.Add(FT(DWSIM.App.GetLocalString("Presso"), su.spmp_pressure), Double.Parse(valor), False, DWSIM.App.GetLocalString("Parmetros2"), DWSIM.App.GetLocalString("Pressodeseparao"), True)
                    With .Item(.Item.Count - 1)
                        .Tag = New Object() {FlowSheet.Options.NumberFormat, su.spmp_pressure, "P"}
                        .CustomEditor = New DWSIM.Editors.Generic.UIUnitConverter
                    End With
                End If

                If Me.GraphicObject.Calculated Then
                    .Item.Add(FT(DWSIM.App.GetLocalString("RConvPGridItem3"), su.spmp_heatflow), Format(Conversor.ConverterDoSI(su.spmp_heatflow, Me.DeltaQ.GetValueOrDefault), FlowSheet.Options.NumberFormat), True, DWSIM.App.GetLocalString("Resultados3"), "", True)
                End If

                .ExpandAllGridItems()

            End With

        End Sub

        Public Overrides Function GetPropertyValue(ByVal prop As String, Optional ByVal su As SistemasDeUnidades.Unidades = Nothing) As Object
            If su Is Nothing Then su = New DWSIM.SistemasDeUnidades.UnidadesSI
            Dim cv As New DWSIM.SistemasDeUnidades.Conversor
            Dim value As Double = 0
            Dim propidx As Integer = CInt(prop.Split("_")(2))

            Select Case propidx

                Case 0
                    'PROP_SV_0	Separation Temperature
                    value = cv.ConverterDoSI(su.spmp_temperature, Me.FlashTemperature)
                Case 1
                    'PROP_SV_1	Separation Pressure
                    value = cv.ConverterDoSI(su.spmp_pressure, Me.FlashPressure)

            End Select

            Return value
        End Function

        Public Overloads Overrides Function GetProperties(ByVal proptype As SimulationObjects_BaseClass.PropertyType) As String()
            Dim i As Integer = 0
            Dim proplist As New ArrayList
            Select Case proptype
                Case PropertyType.RW
                    For i = 0 To 1
                        proplist.Add("PROP_SV_" + CStr(i))
                    Next
                Case PropertyType.WR
                    For i = 0 To 1
                        proplist.Add("PROP_SV_" + CStr(i))
                    Next
                Case PropertyType.ALL
                    For i = 0 To 1
                        proplist.Add("PROP_SV_" + CStr(i))
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
                    'PROP_SV_0	Separation Temperature
                    Me.FlashTemperature = cv.ConverterParaSI(su.spmp_temperature, propval)
                Case 1
                    'PROP_SV_1	Separation Pressure
                    Me.FlashPressure = cv.ConverterParaSI(su.spmp_pressure, propval)
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
                    'PROP_SV_0	Separation Temperature
                    value = su.spmp_temperature
                Case 1
                    'PROP_SV_1	Separation Pressure
                    value = su.spmp_pressure

            End Select

            Return value
        End Function
    End Class

End Namespace
