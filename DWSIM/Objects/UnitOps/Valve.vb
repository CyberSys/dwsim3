﻿'    Valve Calculation Routines 
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
Imports DWSIM.DWSIM.Flowsheet.FlowSheetSolver

Namespace DWSIM.SimulationObjects.UnitOps

    <System.Serializable()> Public Class Valve

        Inherits SimulationObjects_UnitOpBaseClass

        Protected m_dp As Nullable(Of Double)
        Protected m_dt As Nullable(Of Double)
        Protected m_DQ As Nullable(Of Double)
        Protected m_Pout As Nullable(Of Double) = 101325.0#
        Protected m_cmode As CalculationMode = CalculationMode.DeltaP

        Public Enum CalculationMode
            DeltaP = 0
            OutletPressure = 1
        End Enum

        Public Sub New()
            MyBase.New()
        End Sub

        Public Sub New(ByVal nome As String, ByVal descricao As String)

            MyBase.CreateNew()
            Me.m_ComponentName = nome
            Me.m_ComponentDescription = descricao
            Me.FillNodeItems()
            Me.QTFillNodeItems()

        End Sub

        Public Property OutletPressure() As Nullable(Of Double)
            Get
                Return m_Pout
            End Get
            Set(ByVal value As Nullable(Of Double))
                m_Pout = value
            End Set
        End Property

        Public Property CalcMode() As CalculationMode
            Get
                Return m_cmode
            End Get
            Set(ByVal value As CalculationMode)
                m_cmode = value
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

        Public Overrides Function Calculate(Optional ByVal args As Object = Nothing) As Integer

            Dim form As Global.DWSIM.FormFlowsheet = Me.Flowsheet
            Dim objargs As New DWSIM.Outros.StatusChangeEventArgs

            If Not Me.GraphicObject.OutputConnectors(0).IsAttached Then
                'Call function to calculate flowsheet
                With objargs
                    .Calculado = False
                    .Nome = Me.Nome
                    .Tipo = TipoObjeto.Valve
                End With
                CalculateFlowsheet(FlowSheet, objargs, Nothing)
                Throw New Exception(DWSIM.App.GetLocalString("Verifiqueasconexesdo"))
            ElseIf Not Me.GraphicObject.InputConnectors(0).IsAttached Then
                'Call function to calculate flowsheet
                With objargs
                    .Calculado = False
                    .Nome = Me.Nome
                    .Tipo = TipoObjeto.Valve
                End With
                CalculateFlowsheet(FlowSheet, objargs, Nothing)
                Throw New Exception(DWSIM.App.GetLocalString("Verifiqueasconexesdo"))
            End If

            Dim Ti, Pi, Hi, Wi, ei, ein, T2, P2, H2, H2c As Double

            Me.PropertyPackage.CurrentMaterialStream = form.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.InputConnectors(0).AttachedConnector.AttachedFrom.Name)
            Me.PropertyPackage.CurrentMaterialStream.Validate()
            Ti = form.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.InputConnectors(0).AttachedConnector.AttachedFrom.Name).Fases(0).SPMProperties.temperature.GetValueOrDefault.ToString
            Pi = form.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.InputConnectors(0).AttachedConnector.AttachedFrom.Name).Fases(0).SPMProperties.pressure.GetValueOrDefault.ToString
            Hi = form.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.InputConnectors(0).AttachedConnector.AttachedFrom.Name).Fases(0).SPMProperties.enthalpy.GetValueOrDefault.ToString
            Wi = form.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.InputConnectors(0).AttachedConnector.AttachedFrom.Name).Fases(0).SPMProperties.massflow.GetValueOrDefault.ToString
            ei = Hi * Wi
            ein = ei

            Me.PropertyPackage.CurrentMaterialStream = form.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.InputConnectors(0).AttachedConnector.AttachedFrom.Name)
            H2 = Hi '- Me.DeltaP.GetValueOrDefault / (rho_li * 1000)

            If DebugMode Then AppendDebugLine(String.Format("Property Package: {0}", Me.PropertyPackage.ComponentName))
            If DebugMode Then AppendDebugLine(String.Format("Flash Algorithm: {0}", Me.PropertyPackage.FlashBase.GetType.Name))
            If DebugMode Then AppendDebugLine(String.Format("Input variables: T = {0} K, P = {1} Pa, H = {2} kJ/kg, W = {3} kg/s", Ti, Pi, Hi, Wi))

            If Me.CalcMode = CalculationMode.DeltaP Then
                P2 = Pi - Me.DeltaP.GetValueOrDefault
            Else
                P2 = Me.OutletPressure.GetValueOrDefault
                Me.DeltaP = P2 - Pi
            End If
            CheckSpec(P2, True, "outlet pressure")

            If DebugMode Then AppendDebugLine(String.Format("Doing a PH flash to calculate outlet temperature... P = {0} Pa, H = {1} kJ/[kg.K]", P2, H2))

            Dim tmp = Me.PropertyPackage.DW_CalcEquilibrio_ISOL(PropertyPackages.FlashSpec.P, PropertyPackages.FlashSpec.H, P2, H2, Ti)
            T2 = tmp(2)
            CheckSpec(T2, True, "outlet temperature")
            H2c = tmp(4)
            CheckSpec(H2c, False, "outlet enthalpy")

            If DebugMode Then AppendDebugLine(String.Format("Calculated outlet temperature T2 = {0} K", T2))

            'Dim htol As Double = Me.PropertyPackage.Parameters("PP_PHFELT")
            'Dim herr As Double = Math.Abs((H2c - H2) / H2)

            'If herr > 0.01 Then Throw New Exception("The enthalpy of inlet and outlet streams doesn't match. Result is invalid.")

            Me.DeltaT = T2 - Ti
            Me.DeltaQ = 0

            If Not DebugMode Then

                'Atribuir valores à corrente de matéria conectada à jusante
                With form.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.OutputConnectors(0).AttachedConnector.AttachedTo.Name)
                    .Fases(0).SPMProperties.temperature = T2
                    .Fases(0).SPMProperties.pressure = P2
                    .Fases(0).SPMProperties.enthalpy = H2
                    Dim comp As DWSIM.ClassesBasicasTermodinamica.Substancia
                    Dim i As Integer = 0
                    For Each comp In .Fases(0).Componentes.Values
                        comp.FracaoMolar = form.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.InputConnectors(0).AttachedConnector.AttachedFrom.Name).Fases(0).Componentes(comp.Nome).FracaoMolar
                        comp.FracaoMassica = form.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.InputConnectors(0).AttachedConnector.AttachedFrom.Name).Fases(0).Componentes(comp.Nome).FracaoMassica
                        i += 1
                    Next
                    .SpecType = Streams.MaterialStream.Flashspec.Pressure_and_Enthalpy
                    .Fases(0).SPMProperties.massflow = form.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.InputConnectors(0).AttachedConnector.AttachedFrom.Name).Fases(0).SPMProperties.massflow.GetValueOrDefault
                End With

                'Call function to calculate flowsheet
                With objargs
                    .Calculado = True
                    .Nome = Me.Nome
                    .Tag = Me.GraphicObject.Tag
                    .Tipo = TipoObjeto.Valve
                End With

                form.CalculationQueue.Enqueue(objargs)

            Else

                AppendDebugLine("Calculation finished successfully.")

            End If

        End Function


        Public Overrides Function DeCalculate() As Integer

            Dim form As Global.DWSIM.FormFlowsheet = Me.FlowSheet

            If Me.GraphicObject.OutputConnectors(0).IsAttached Then

                'Zerar valores da corrente de matéria conectada a jusante
                With form.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.OutputConnectors(0).AttachedConnector.AttachedTo.Name)
                    .Fases(0).SPMProperties.temperature = Nothing
                    .Fases(0).SPMProperties.pressure = Nothing
                    .Fases(0).SPMProperties.molarfraction = 1
                    .Fases(0).SPMProperties.massfraction = 1
                    .Fases(0).SPMProperties.enthalpy = Nothing
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

            'Call function to calculate flowsheet
            Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
            With objargs
                .Calculado = False
                .Nome = Me.Nome
                .Tipo = TipoObjeto.Valve
            End With

            form.CalculationQueue.Enqueue(objargs)

        End Function

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

            End With

        End Sub

        Public Overrides Sub QTFillNodeItems()


            With Me.QTNodeTableItems

                .Clear()

                .Add(0, New DWSIM.Outros.NodeItem("DP", "", "", 0, 0, ""))
                .Add(1, New DWSIM.Outros.NodeItem("DT", "", "", 1, 0, ""))

            End With
        End Sub

        Public Overrides Sub PopulatePropertyGrid(ByRef pgrid As PropertyGridEx.PropertyGridEx, ByVal su As SistemasDeUnidades.Unidades)

            Dim Conversor As New DWSIM.SistemasDeUnidades.Conversor

            With pgrid

                .PropertySort = PropertySort.Categorized
                .ShowCustomProperties = True
                .Item.Clear()

                MyBase.PopulatePropertyGrid(pgrid, su)

                Dim ent, saida As String
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

                '.Item.Add(DWSIM.App.GetLocalString("Correntedeenergia"), energ, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                'With .Item(.Item.Count - 1)
                '    .DefaultValue = Nothing
                '    .CustomEditor = New DWSIM.Editors.Streams.UIOutputESSelector
                'End With

                .Item.Add(DWSIM.App.GetLocalString("ValveCalcMode"), Me, "CalcMode", False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), "", True)
                Dim valor As Double

                Select Case Me.CalcMode


                    Case CalculationMode.DeltaP

                        valor = Format(Conversor.ConverterDoSI(su.spmp_deltaP, Me.DeltaP.GetValueOrDefault), FlowSheet.Options.NumberFormat)
                        .Item.Add(FT(DWSIM.App.GetLocalString("Quedadepresso"), su.spmp_deltaP), valor, False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString(""), True)
                        With .Item(.Item.Count - 1)
                            .Tag = New Object() {FlowSheet.Options.NumberFormat, su.spmp_deltaP, "DP"}
                            .CustomEditor = New DWSIM.Editors.Generic.UIUnitConverter
                        End With

                    Case CalculationMode.OutletPressure

                        valor = Format(Conversor.ConverterDoSI(su.spmp_pressure, Me.OutletPressure.GetValueOrDefault), FlowSheet.Options.NumberFormat)
                        .Item.Add(FT(DWSIM.App.GetLocalString("ValveOutletPressure"), su.spmp_pressure), valor, False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), "", True)
                        With .Item(.Item.Count - 1)
                            .Tag = New Object() {FlowSheet.Options.NumberFormat, su.spmp_pressure, "P"}
                            .CustomEditor = New DWSIM.Editors.Generic.UIUnitConverter
                        End With

                End Select

                .Item.Add(FT(DWSIM.App.GetLocalString("DeltaT2"), su.spmp_deltaT), Format(Conversor.ConverterDoSI(su.spmp_deltaT, Me.DeltaT.GetValueOrDefault), FlowSheet.Options.NumberFormat), True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("Diferenadetemperatur"), True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .DefaultType = GetType(Nullable(Of Double))
                End With

                If Me.GraphicObject.Calculated = False Then
                    .Item.Add(DWSIM.App.GetLocalString("Mensagemdeerro"), Me, "ErrorMessage", True, DWSIM.App.GetLocalString("Miscelnea4"), DWSIM.App.GetLocalString("Mensagemretornadaqua"), True)
                    With .Item(.Item.Count - 1)
                        .DefaultType = GetType(System.String)
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
                    'PROP_VA_0	Calculation Mode
                    value = Me.CalcMode
                Case 1
                    'PROP_VA_1	Pressure Drop
                    value = cv.ConverterDoSI(su.spmp_deltaP, Me.DeltaP.GetValueOrDefault)
                Case 2
                    'PROP_VA_2	Outlet Pressure
                    value = cv.ConverterDoSI(su.spmp_pressure, Me.OutletPressure.GetValueOrDefault)
                Case 3
                    'PROP_VA_3	Temperature Drop
                    value = cv.ConverterDoSI(su.spmp_deltaT, Me.DeltaT.GetValueOrDefault)
            End Select

            Return value

        End Function

        Public Overloads Overrides Function GetProperties(ByVal proptype As SimulationObjects_BaseClass.PropertyType) As String()
            Dim i As Integer = 0
            Dim proplist As New ArrayList
            Select Case proptype
                Case PropertyType.RO
                    For i = 3 To 3
                        proplist.Add("PROP_VA_" + CStr(i))
                    Next
                Case PropertyType.RW
                    For i = 0 To 3
                        proplist.Add("PROP_VA_" + CStr(i))
                    Next
                Case PropertyType.WR
                    For i = 0 To 2
                        proplist.Add("PROP_VA_" + CStr(i))
                    Next
                Case PropertyType.ALL
                    For i = 0 To 3
                        proplist.Add("PROP_VA_" + CStr(i))
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
                    'PROP_VA_0	Calculation Mode
                    Me.CalcMode = propval
                Case 1
                    'PROP_VA_1	Pressure Drop
                    Me.DeltaP = cv.ConverterParaSI(su.spmp_deltaP, propval)
                Case 2
                    'PROP_VA_2	Outlet Pressure
                    Me.OutletPressure = cv.ConverterParaSI(su.spmp_pressure, propval)
            End Select
            Return 1
        End Function

        Public Overrides Function GetPropertyUnit(ByVal prop As String, Optional ByVal su As SistemasDeUnidades.Unidades = Nothing) As Object
            If su Is Nothing Then su = New DWSIM.SistemasDeUnidades.UnidadesSI
            Dim value As String = ""
            Dim propidx As Integer = CInt(prop.Split("_")(2))

            Select Case propidx

                Case 0
                    'PROP_VA_0	Calculation Mode
                    value = ""
                Case 1
                    'PROP_VA_1	Pressure Drop
                    value = su.spmp_deltaP
                Case 2
                    'PROP_VA_2	Outlet Pressure
                    value = su.spmp_pressure
                Case 3
                    'PROP_VA_3	Temperature Drop
                    value = su.spmp_deltaT

            End Select

            Return value
        End Function
    End Class

End Namespace

