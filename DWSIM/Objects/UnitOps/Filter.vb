﻿'    Continuous Cake Filter Unit Operation Calculation Routines 
'
'    Model based on the Cake Filter equations of Chapter 29 - 
'    "Mechanical Separations" from the "Unit Operations of Chemical Engineering" 
'    book by McCabe, Smith and Harriott, Seventh Edition. 
'
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

Imports Microsoft.MSDN.Samples.GraphicObjects
Imports DWSIM.DWSIM.ClassesBasicasTermodinamica
Imports DWSIM.DWSIM.SimulationObjects.Streams
Imports DWSIM.DWSIM.SimulationObjects.UnitOps.Auxiliary
Imports DWSIM.DWSIM.Flowsheet.FlowsheetSolver

Namespace DWSIM.SimulationObjects.UnitOps

    <System.Serializable()> Public Class Filter

        Inherits SimulationObjects_UnitOpBaseClass

        Protected m_ei As Double

        Public Overrides Function LoadData(data As System.Collections.Generic.List(Of System.Xml.Linq.XElement)) As Boolean
            Return MyBase.LoadData(data)
        End Function

        Public Overrides Function SaveData() As System.Collections.Generic.List(Of System.Xml.Linq.XElement)

            Dim elements As System.Collections.Generic.List(Of System.Xml.Linq.XElement) = MyBase.SaveData()
            Dim ci As Globalization.CultureInfo = Globalization.CultureInfo.InvariantCulture

            Return elements

        End Function

        Public Enum CalculationMode
            Design = 0
            Simulation = 1
        End Enum

        Public Property EnergyImb As Double = 0.0#
        Public Property PressureDrop As Double = 0.0#
        Public Property TotalFilterArea As Double = 1.0#
        Public Property SubmergedAreaFraction As Double = 0.3#
        Public Property SpecificCakeResistance As Double = 10000000000.0
        Public Property FilterMediumResistance As Double = 0.000000001
        Public Property FilterCycleTime As Double = 300.0#
        Public Property CakeRelativeHumidity As Double = 0.0#
        Public Property CalcMode As CalculationMode = CalculationMode.Simulation

        Public Sub New()
            MyBase.New()
        End Sub

        Public Sub New(ByVal nome As String, ByVal descricao As String)

            MyBase.CreateNew()
            Me.m_ComponentName = nome
            Me.m_ComponentDescription = descricao
            Me.FillNodeItems()
            Me.QTFillNodeItems()
            Me.ShowQuickTable = True

        End Sub

        Public Overrides Function Calculate(Optional ByVal args As Object = Nothing) As Integer

            Dim form As FormFlowsheet = Me.FlowSheet
            Dim objargs As New DWSIM.Outros.StatusChangeEventArgs

            If Not Me.GraphicObject.InputConnectors(0).IsAttached Then
                'Call function to calculate flowsheet
                With objargs
                    .Calculado = False
                    .Nome = Me.Nome
                    .Tipo = TipoObjeto.Filter
                End With
                CalculateFlowsheet(FlowSheet, objargs, Nothing)
                Throw New Exception(DWSIM.App.GetLocalString("Verifiqueasconexesdo"))
            ElseIf Not Me.GraphicObject.OutputConnectors(0).IsAttached Then
                'Call function to calculate flowsheet
                With objargs
                    .Calculado = False
                    .Nome = Me.Nome
                    .Tipo = TipoObjeto.Filter
                End With
                CalculateFlowsheet(FlowSheet, objargs, Nothing)
                Throw New Exception(DWSIM.App.GetLocalString("Verifiqueasconexesdo"))
            ElseIf Not Me.GraphicObject.OutputConnectors(1).IsAttached Then
                'Call function to calculate flowsheet
                With objargs
                    .Calculado = False
                    .Nome = Me.Nome
                    .Tipo = TipoObjeto.Filter
                End With
                CalculateFlowsheet(FlowSheet, objargs, Nothing)
                Throw New Exception(DWSIM.App.GetLocalString("Verifiqueasconexesdo"))
            End If

            Dim instr, outstr1, outstr2 As Streams.MaterialStream
            instr = FlowSheet.Collections.ObjectCollection(Me.GraphicObject.InputConnectors(0).AttachedConnector.AttachedFrom.Name)
            outstr1 = FlowSheet.Collections.ObjectCollection(Me.GraphicObject.OutputConnectors(0).AttachedConnector.AttachedTo.Name)
            outstr2 = FlowSheet.Collections.ObjectCollection(Me.GraphicObject.OutputConnectors(1).AttachedConnector.AttachedTo.Name)

            'the filter doesn't support a vapor phase in the inlet stream.
            If instr.Fases(2).SPMProperties.massflow.GetValueOrDefault > 0.0# Then
                Throw New Exception(DWSIM.App.GetLocalString("FilterVaporPhaseNotSupported"))
            End If

            Dim W As Double = instr.Fases(0).SPMProperties.massflow.GetValueOrDefault
            Dim Wsin As Double = instr.Fases(7).SPMProperties.massflow.GetValueOrDefault
            Dim Wlin As Double = W - Wsin

            Dim n, At, c, alpha, Rm, f, tc, mf_mc, dp As Double

            tc = Me.FilterCycleTime
            n = 1 / tc
            f = Me.SubmergedAreaFraction
            alpha = Me.SpecificCakeResistance
            Rm = Me.FilterMediumResistance
            mf_mc = 100 / (100 - Me.CakeRelativeHumidity)

            Dim rho, mu, cf, frh, crh As Double

            rho = instr.Fases(1).SPMProperties.density.GetValueOrDefault
            mu = instr.Fases(1).SPMProperties.viscosity.GetValueOrDefault
            cf = instr.Fases(7).SPMProperties.massflow.GetValueOrDefault / instr.Fases(0).SPMProperties.volumetric_flow.GetValueOrDefault
            frh = instr.Fases(1).SPMProperties.massflow.GetValueOrDefault / (instr.Fases(1).SPMProperties.massflow.GetValueOrDefault + instr.Fases(7).SPMProperties.massflow.GetValueOrDefault)
            crh = Me.CakeRelativeHumidity / 100

            If crh > frh Then
                'Call function to calculate flowsheet
                With objargs
                    .Calculado = False
                    .Nome = Me.Nome
                    .Tipo = TipoObjeto.Filter
                End With
                CalculateFlowsheet(FlowSheet, objargs, Nothing)
                Throw New Exception(DWSIM.App.GetLocalString("FilterInvalidCakeHumidity"))
            End If

            c = cf / (1 - (mf_mc - 1) * cf / rho)

            Select Case CalcMode
                Case CalculationMode.Design
                    dp = Me.PressureDrop
                    At = Wsin * alpha / ((2 * c * alpha * dp * f * n / mu + (n * Rm) ^ 2) ^ 0.5 - n * Rm)
                    Me.TotalFilterArea = At
                Case CalculationMode.Simulation
                    At = Me.TotalFilterArea
                    dp = ((n * Rm) ^ 2 + (n * Rm + Wsin * alpha / At) ^ 2) / (2 * c * alpha * f * n / mu)
                    Me.PressureDrop = dp
            End Select

            Dim Wsout As Double = Wsin / (1 - crh)
            Dim Wlout As Double = W - Wsout

            Dim mw As Double

            Dim cp As ConnectionPoint

            cp = Me.GraphicObject.OutputConnectors(0)
            If cp.IsAttached Then
                outstr1 = form.Collections.CLCS_MaterialStreamCollection(cp.AttachedConnector.AttachedTo.Name)
                With outstr1
                    .ClearAllProps()
                    .Fases(0).SPMProperties.massflow = Wlout
                    Dim comp As DWSIM.ClassesBasicasTermodinamica.Substancia
                    For Each comp In .Fases(0).Componentes.Values
                        comp.MassFlow = instr.Fases(1).Componentes(comp.Nome).MassFlow * Wlout / Wlin
                        comp.FracaoMassica = comp.MassFlow / Wlout
                    Next
                    mw = 0.0#
                    For Each comp In .Fases(0).Componentes.Values
                        mw += comp.FracaoMassica / comp.ConstantProperties.Molar_Weight
                    Next
                    For Each comp In .Fases(0).Componentes.Values
                        comp.FracaoMolar = comp.FracaoMassica / comp.ConstantProperties.Molar_Weight / mw
                    Next
                    For Each comp In .Fases(0).Componentes.Values
                        comp.MolarFlow = comp.MassFlow / comp.ConstantProperties.Molar_Weight / 1000
                    Next
                End With
            End If

            cp = Me.GraphicObject.OutputConnectors(1)
            If cp.IsAttached Then
                outstr2 = form.Collections.CLCS_MaterialStreamCollection(cp.AttachedConnector.AttachedTo.Name)
                With outstr2
                    .ClearAllProps()
                    .Fases(0).SPMProperties.massflow = Wsout
                    Dim comp As DWSIM.ClassesBasicasTermodinamica.Substancia
                    For Each comp In .Fases(0).Componentes.Values
                        comp.MassFlow = instr.Fases(1).Componentes(comp.Nome).MassFlow * (Wlin - Wlout) / Wlin + instr.Fases(7).Componentes(comp.Nome).MassFlow
                        comp.FracaoMassica = comp.MassFlow / Wsout
                    Next
                    mw = 0.0#
                    For Each comp In .Fases(0).Componentes.Values
                        mw += comp.FracaoMassica / comp.ConstantProperties.Molar_Weight
                    Next
                    For Each comp In .Fases(0).Componentes.Values
                        comp.FracaoMolar = comp.FracaoMassica / comp.ConstantProperties.Molar_Weight / mw
                    Next
                    For Each comp In .Fases(0).Componentes.Values
                        comp.MolarFlow = comp.MassFlow / comp.ConstantProperties.Molar_Weight / 1000
                    Next
                End With
            End If

            'pass conditions

            outstr1.Fases(0).SPMProperties.temperature = instr.Fases(0).SPMProperties.temperature.GetValueOrDefault
            outstr1.Fases(0).SPMProperties.pressure = instr.Fases(0).SPMProperties.pressure.GetValueOrDefault - dp
            outstr2.Fases(0).SPMProperties.temperature = instr.Fases(0).SPMProperties.temperature.GetValueOrDefault
            outstr2.Fases(0).SPMProperties.pressure = instr.Fases(0).SPMProperties.pressure.GetValueOrDefault - dp

            'do a flash calculation on streams to calculate energy imbalance

            outstr1.PropertyPackage.CurrentMaterialStream = outstr1
            outstr1.PropertyPackage.DW_CalcEquilibrium(PropertyPackages.FlashSpec.T, PropertyPackages.FlashSpec.P)
            outstr2.PropertyPackage.CurrentMaterialStream = outstr2
            outstr2.PropertyPackage.DW_CalcEquilibrium(PropertyPackages.FlashSpec.T, PropertyPackages.FlashSpec.P)

            Dim Hi, Ho1, Ho2, Wi, Wo1, Wo2 As Double

            Hi = instr.Fases(0).SPMProperties.enthalpy.GetValueOrDefault
            Wi = instr.Fases(0).SPMProperties.massflow.GetValueOrDefault
            Ho1 = outstr1.Fases(0).SPMProperties.enthalpy.GetValueOrDefault
            Wo1 = outstr1.Fases(0).SPMProperties.massflow.GetValueOrDefault
            Ho2 = outstr2.Fases(0).SPMProperties.enthalpy.GetValueOrDefault
            Wo2 = outstr2.Fases(0).SPMProperties.massflow.GetValueOrDefault

            'calculate imbalance

            Me.EnergyImb = Hi * Wi - Ho1 * Wo1 - Ho2 * Wo2

            'update energy stream power value

            With form.Collections.CLCS_EnergyStreamCollection(Me.GraphicObject.EnergyConnector.AttachedConnector.AttachedTo.Name)
                .Energia = Me.EnergyImb
                .GraphicObject.Calculated = True
            End With

            'call the flowsheet calculator

            With objargs
                .Calculado = True
                .Nome = Me.Nome
                .Tag = Me.GraphicObject.Tag
                .Tipo = Me.GraphicObject.TipoObjeto
            End With

            form.CalculationQueue.Enqueue(objargs)

        End Function

        Public Overrides Function DeCalculate() As Integer

            Dim form As Global.DWSIM.FormFlowsheet = Me.FlowSheet

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
                .Tipo = TipoObjeto.Vessel
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

                Me.ShowQuickTable = True

                Dim valor As String
                valor = Format(Conversor.ConverterDoSI(su.spmp_heatflow, Me.EnergyImb), nf)

                .Item(0).Value = valor
                .Item(0).Unit = su.spmp_heatflow

            End With

        End Sub

        Public Overrides Sub QTFillNodeItems()

            With Me.QTNodeTableItems

                .Clear()

                .Add(0, New DWSIM.Outros.NodeItem(DWSIM.App.GetLocalString("CSepEnergyImbalance"), "", "", 0, 0, ""))

            End With

        End Sub

        Public Overrides Sub PropertyValueChanged(ByVal s As Object, ByVal e As System.Windows.Forms.PropertyValueChangedEventArgs)

            MyBase.PropertyValueChanged(s, e)

            If FlowSheet.Options.CalculatorActivated Then

                'Call function to calculate flowsheet
                Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                With objargs
                    .Tag = Me.GraphicObject.Tag
                    .Calculado = False
                    .Nome = Me.GraphicObject.Name
                    .Tipo = Me.GraphicObject.TipoObjeto
                    .Emissor = "PropertyGrid"
                End With

                If Me.IsSpecAttached = True And Me.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then FlowSheet.Collections.CLCS_SpecCollection(Me.AttachedSpecId).Calculate()
                FlowSheet.CalculationQueue.Enqueue(objargs)

            End If

        End Sub

        Public Overrides Sub PopulatePropertyGrid(ByVal pgrid As PropertyGridEx.PropertyGridEx, ByVal su As SistemasDeUnidades.Unidades)

            Dim Converter As New DWSIM.SistemasDeUnidades.Conversor

            With pgrid

                .PropertySort = PropertySort.Categorized
                .ShowCustomProperties = True
                .Item.Clear()

                MyBase.PopulatePropertyGrid(pgrid, su)

                Dim ent, saida1, saida2, en As String
                If Me.GraphicObject.InputConnectors(0).IsAttached = True Then
                    ent = Me.GraphicObject.InputConnectors(0).AttachedConnector.AttachedFrom.Tag
                Else
                    ent = ""
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
                If Me.GraphicObject.EnergyConnector.IsAttached = True Then
                    en = Me.GraphicObject.EnergyConnector.AttachedConnector.AttachedTo.Tag
                Else
                    en = ""
                End If

                .Item.Add(DWSIM.App.GetLocalString("Correntedeentrada"), ent, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIInputMSSelector
                End With

                .Item.Add(DWSIM.App.GetLocalString("OutletStream1"), saida1, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIOutputMSSelector
                End With

                .Item.Add(DWSIM.App.GetLocalString("OutletStream2"), saida2, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIOutputMSSelector
                End With

                .Item.Add(DWSIM.App.GetLocalString("Correntedeenergia"), en, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIOutputESSelector
                End With

                Dim value As Double

                value = Converter.ConverterDoSI(su.mediumresistance, Me.FilterMediumResistance)
                .Item.Add(FT(DWSIM.App.GetLocalString("FilterMediumResistance"), su.mediumresistance), Format(value, FlowSheet.Options.NumberFormat), False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("FilterMediumResistanceDesc"), True)
                .Item(.Item.Count - 1).Tag2 = "PROP_FT_4"
                value = Converter.ConverterDoSI(su.cakeresistance, Me.SpecificCakeResistance)
                .Item.Add(FT(DWSIM.App.GetLocalString("FilterSpecificCakeResistance"), su.cakeresistance), Format(value, FlowSheet.Options.NumberFormat), False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("FilterSpecificCakeResistanceDesc"), True)
                .Item(.Item.Count - 1).Tag2 = "PROP_FT_5"
                value = Converter.ConverterDoSI(su.time, Me.FilterCycleTime)
                .Item.Add(FT(DWSIM.App.GetLocalString("FilterCycleTime"), su.time), Format(value, FlowSheet.Options.NumberFormat), False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("FilterCycleTimeDesc"), True)
                .Item(.Item.Count - 1).Tag2 = "PROP_FT_3"

                .Item.Add(DWSIM.App.GetLocalString("FilterSubmergedAreaFraction"), Me, "SubmergedAreaFraction", False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("FilterSubmergedAreaFractionDesc"), True)
                .Item(.Item.Count - 1).Tag2 = "PROP_FT_6"
                .Item.Add(DWSIM.App.GetLocalString("FilterCakeRelativeHumidity"), Me, "CakeRelativeHumidity", False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("FilterCakeRelativeHumidityDesc"), True)
                .Item(.Item.Count - 1).Tag2 = "PROP_FT_2"

                .Item.Add(DWSIM.App.GetLocalString("FilterCalculationMode"), Me, "CalcMode", False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("FilterCalculationModeDesc"), True)
                .Item(.Item.Count - 1).Tag2 = "CalcMode"

                Select Case Me.CalcMode
                    Case CalculationMode.Design
                        value = Converter.ConverterDoSI(su.spmp_deltaP, Me.PressureDrop)
                        .Item.Add(FT(DWSIM.App.GetLocalString("FilterPressureDrop"), su.spmp_deltaP), Format(value, FlowSheet.Options.NumberFormat), False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("FilterPressureDropDesc"), True)
                        .Item(.Item.Count - 1).Tag2 = "PROP_FT_7"
                    Case CalculationMode.Simulation
                        value = Converter.ConverterDoSI(su.area, Me.TotalFilterArea)
                        .Item.Add(FT(DWSIM.App.GetLocalString("FilterArea"), su.area), Format(value, FlowSheet.Options.NumberFormat), False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("FilterAreaDesc"), True)
                        .Item(.Item.Count - 1).Tag2 = "PROP_FT_1"
                End Select

                If Me.GraphicObject.Calculated Then
                    Select Case Me.CalcMode
                        Case CalculationMode.Design
                            .Item.Add(FT(DWSIM.App.GetLocalString("FilterArea"), su.area), Format(Converter.ConverterDoSI(su.area, Me.TotalFilterArea), FlowSheet.Options.NumberFormat), True, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("FilterAreaDesc"), True)
                        Case CalculationMode.Simulation
                            .Item.Add(FT(DWSIM.App.GetLocalString("FilterPressureDrop"), su.spmp_deltaP), Format(Converter.ConverterDoSI(su.spmp_deltaP, Me.PressureDrop), FlowSheet.Options.NumberFormat), True, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("FilterPressureDropDesc"), True)
                    End Select
                    .Item.Add(FT(DWSIM.App.GetLocalString("CSepEnergyImbalance"), su.spmp_heatflow), Format(Converter.ConverterDoSI(su.spmp_heatflow, Me.EnergyImb), FlowSheet.Options.NumberFormat), True, DWSIM.App.GetLocalString("Resultados3"), "", True)
                End If

                If Me.IsSpecAttached = True Then
                    .Item.Add(DWSIM.App.GetLocalString("ObjetoUtilizadopor"), FlowSheet.Collections.ObjectCollection(Me.AttachedSpecId).GraphicObject.Tag, True, DWSIM.App.GetLocalString("Miscelnea2"), "", True)
                    .Item.Add(DWSIM.App.GetLocalString("Utilizadocomo"), Me.SpecVarType, True, DWSIM.App.GetLocalString("Miscelnea3"), "", True)
                End If

                If Not Me.Annotation Is Nothing Then
                    .Item.Add(DWSIM.App.GetLocalString("Anotaes"), Me, "Annotation", False, DWSIM.App.GetLocalString("Outros"), DWSIM.App.GetLocalString("Cliquenobotocomretic"), True)
                    With .Item(.Item.Count - 1)
                        .IsBrowsable = False
                        .CustomEditor = New DWSIM.Editors.Annotation.UIAnnotationEditor
                    End With
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
                    'PROP_FT_0	Energy Balance	
                    value = cv.ConverterDoSI(su.spmp_heatflow, Me.EnergyImb)
                Case 1
                    'PROP_FT_1	Total Filter Area	
                    value = cv.ConverterDoSI(su.area, Me.TotalFilterArea)
                Case 2
                    'PROP_FT_2	Cake Relative Humidity (%)	
                    value = Me.CakeRelativeHumidity
                Case 3
                    'PROP_FT_3	Cycle Time	
                    value = cv.ConverterDoSI(su.time, Me.FilterCycleTime)
                Case 4
                    'PROP_FT_4	Filter Medium Resistance	
                    value = cv.ConverterDoSI(su.mediumresistance, Me.FilterMediumResistance)
                Case 5
                    'PROP_FT_5	Specific Cake Resistance	
                    value = cv.ConverterDoSI(su.cakeresistance, Me.SpecificCakeResistance)
                Case 6
                    'PROP_FT_6	Submerged Area Fraction	
                    value = Me.SubmergedAreaFraction
                Case 7
                    'PROP_FT_7	Total Pressure Drop	
                    value = cv.ConverterDoSI(su.spmp_pressure, Me.PressureDrop)
            End Select

            Return value

        End Function

        Public Overloads Overrides Function GetProperties(ByVal proptype As SimulationObjects_BaseClass.PropertyType) As String()
            Dim i As Integer = 0
            Dim proplist As New ArrayList
            For i = 0 To 7
                proplist.Add("PROP_FT_" + CStr(i))
            Next
            Return proplist.ToArray(GetType(System.String))
            proplist = Nothing
        End Function

        Public Overrides Function SetPropertyValue(ByVal prop As String, ByVal propval As Object, Optional ByVal su As DWSIM.SistemasDeUnidades.Unidades = Nothing) As Object
            If su Is Nothing Then su = New DWSIM.SistemasDeUnidades.UnidadesSI
            Dim cv As New DWSIM.SistemasDeUnidades.Conversor
            Dim propidx As Integer = CInt(prop.Split("_")(2))

            Select Case propidx
                Case 0
                    'PROP_FT_0	Energy Balance	
                Case 1
                    'PROP_FT_1	Total Filter Area	
                    Me.TotalFilterArea = cv.ConverterParaSI(su.area, propval)
                Case 2
                    'PROP_FT_2	Cake Relative Humidity (%)	
                    Me.CakeRelativeHumidity = propval
                Case 3
                    'PROP_FT_3	Cycle Time	
                    Me.FilterCycleTime = cv.ConverterParaSI(su.time, propval)
                Case 4
                    'PROP_FT_4	Filter Medium Resistance	
                    Me.FilterMediumResistance = cv.ConverterParaSI(su.mediumresistance, propval)
                Case 5
                    'PROP_FT_5	Specific Cake Resistance	
                    Me.SpecificCakeResistance = cv.ConverterParaSI(su.cakeresistance, propval)
                Case 6
                    'PROP_FT_6	Submerged Area Fraction	
                    Me.SubmergedAreaFraction = propval
                Case 7
                    'PROP_FT_7	Total Pressure Drop	
                    Me.PressureDrop = cv.ConverterParaSI(su.spmp_deltaP, propval)
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
                    'PROP_FT_0	Energy Balance	
                    value = su.spmp_heatflow
                Case 1
                    'PROP_FT_1	Total Filter Area	
                    value = su.area
                Case 2
                    'PROP_FT_2	Cake Relative Humidity (%)	
                    value = "%"
                Case 3
                    'PROP_FT_3	Cycle Time	
                    value = su.time
                Case 4
                    'PROP_FT_4	Filter Medium Resistance	
                    value = su.mediumresistance
                Case 5
                    'PROP_FT_5	Specific Cake Resistance	
                    value = su.cakeresistance
                Case 6
                    'PROP_FT_6	Submerged Area Fraction	
                    value = ""
                Case 7
                    'PROP_FT_7	Total Pressure Drop	
                    value = su.spmp_deltaP
            End Select

            Return value
        End Function
    End Class

End Namespace

