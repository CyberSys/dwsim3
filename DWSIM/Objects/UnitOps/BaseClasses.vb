'    Flowsheet Object Base Classes 
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
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Runtime.Serialization
Imports System.IO
Imports System.Linq
Imports DWSIM.Interfaces2
Imports DWSIM.DWSIM
Imports DWSIM.DWSIM.Flowsheet.FlowsheetSolver
Imports Microsoft.Scripting.Hosting
Imports CapeOpen
Imports System.Runtime.Serialization.Formatters
Imports System.Runtime.InteropServices.Marshal
Imports System.Runtime.InteropServices
Imports DWSIM.DWSIM.SimulationObjects
Imports System.Text
Imports DWSIM.DWSIM.SimulationObjects.PropertyPackages
Imports PropertyGridEx

<System.Serializable()> <ComVisible(True)> Public MustInherit Class SimulationObjects_BaseClass

    Implements ICloneable, IDisposable, XMLSerializer.Interfaces.ICustomXMLSerialization

    Protected m_ComponentDescription As String = ""
    Protected m_ComponentName As String = ""

    Public Const ClassId As String = ""

    Protected m_nodeitems As System.Collections.Generic.Dictionary(Of Integer, DWSIM.Outros.NodeItem)
    Protected m_qtnodeitems As System.Collections.Generic.Dictionary(Of Integer, DWSIM.Outros.NodeItem)
    Protected m_table As DWSIM.GraphicObjects.TableGraphic
    Protected m_qtable As DWSIM.GraphicObjects.QuickTableGraphic

    Protected m_IsAdjustAttached As Boolean = False
    Protected m_AdjustId As String = ""
    Protected m_AdjustVarType As DWSIM.SimulationObjects.SpecialOps.Helpers.Adjust.TipoVar = DWSIM.SimulationObjects.SpecialOps.Helpers.Adjust.TipoVar.Nenhum

    Protected m_IsSpecAttached As Boolean = False
    Protected m_SpecId As String = ""
    Protected m_SpecVarType As DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Nenhum

    <System.NonSerialized()> Protected Friend m_flowsheet As FormFlowsheet

    Protected m_showqtable As Boolean = True
    Protected m_errormessage As String = ""

    Public Property Calculated As Boolean = False
    Public Property DebugMode As Boolean = False
    <Xml.Serialization.XmlIgnore> Public Property CreatedWithThreadID As Integer = 0
    <Xml.Serialization.XmlIgnore> Public Property LastUpdated As New Date

    Public MustOverride Sub UpdatePropertyNodes(ByVal su As DWSIM.SistemasDeUnidades.Unidades, ByVal nf As String)

    Public MustOverride Sub PopulatePropertyGrid(ByRef pgrid As PropertyGridEx.PropertyGridEx, ByVal su As DWSIM.SistemasDeUnidades.Unidades)

    Public Sub New()

    End Sub

    Public Overridable Function GetDebugReport() As String
        Return "Error - function not implemented"
    End Function

    ''' <summary>
    ''' Gets or sets the error message regarding the last calculation attempt.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ErrorMessage() As String
        Get
            Return m_errormessage
        End Get
        Set(ByVal value As String)
            m_errormessage = value
        End Set
    End Property

    ''' <summary>
    ''' Checks if a value is valid.
    ''' </summary>
    ''' <param name="val">Value to be checked.</param>
    ''' <param name="onlypositive">Value should be a positive double or not.</param>
    ''' <param name="paramname">Name of the parameter (ex. P, T, W, H etc.)</param>
    ''' <remarks></remarks>
    Public Sub CheckSpec(val As Double, onlypositive As Boolean, paramname As String)

        If Not val.IsValid Then Throw New ArgumentException(DWSIM.App.GetLocalString("ErrorInvalidUOSpecValue") & " (name: " & paramname & ", value: " & val & ")")
        If onlypositive Then If val.IsNegative Then Throw New ArgumentException(DWSIM.App.GetLocalString("ErrorInvalidUOSpecValue") & " (name: " & paramname & ", value: " & val & ")")

    End Sub

    ''' <summary>
    ''' Updates the property selection list to display tables in the flowsheet.
    ''' </summary>
    ''' <param name="NoPropVals"></param>
    ''' <remarks></remarks>
    Public Overridable Sub FillNodeItems(Optional ByVal NoPropVals As Boolean = False)

        If Me.NodeTableItems Is Nothing Then Me.NodeTableItems = New Dictionary(Of Integer, DWSIM.Outros.NodeItem)

        With Me.NodeTableItems

            .Clear()

            Dim props As String() = Me.GetProperties(PropertyType.ALL)

            Dim key As Integer = 0
            For Each prop As String In props
                If Not NoPropVals Then
                    .Add(key, New DWSIM.Outros.NodeItem(prop, GetPropertyValue(prop, FlowSheet.Options.SelectedUnitSystem), GetPropertyUnit(prop, FlowSheet.Options.SelectedUnitSystem), key, 0, ""))
                Else
                    .Add(key, New DWSIM.Outros.NodeItem(prop, "", "", key, 0, ""))
                End If
                key += 1
            Next

        End With

    End Sub

    ''' <summary>
    ''' Updates the values of the variables displayed in the object's tooltip in the flowsheet.
    ''' </summary>
    ''' <remarks></remarks>
    Public MustOverride Sub QTFillNodeItems()

    Protected m_graphicobject As GraphicObject = Nothing

    Protected m_annotation As DWSIM.Outros.Annotation

    Public Enum PropertyType
        RO = 0
        RW = 1
        WR = 2
        ALL = 3
    End Enum

    ''' <summary>
    ''' Get a list of all properties of the object.
    ''' </summary>
    ''' <param name="proptype">Type of the property.</param>
    ''' <returns>A list of property identifiers.</returns>
    ''' <remarks>More details at http://dwsim.inforside.com.br/wiki/index.php?title=Object_Property_Codes </remarks>
    Public MustOverride Function GetProperties(ByVal proptype As PropertyType) As String()
    ''' <summary>
    ''' Gets the value of a property.
    ''' </summary>
    ''' <param name="prop">Property identifier.</param>
    ''' <param name="su">Units system to use. Null to use the default (SI) system.</param>
    ''' <returns>Property value.</returns>
    ''' <remarks>More details at http://dwsim.inforside.com.br/wiki/index.php?title=Object_Property_Codes </remarks>
    Public MustOverride Function GetPropertyValue(ByVal prop As String, Optional ByVal su As DWSIM.SistemasDeUnidades.Unidades = Nothing)
    ''' <summary>
    ''' Gets the units of a property.
    ''' </summary>
    ''' <param name="prop">Property identifier.</param>
    ''' <param name="su">Units system to use. Null to use the default (SI) system.</param>
    ''' <returns>Property units.</returns>
    ''' <remarks>More details at http://dwsim.inforside.com.br/wiki/index.php?title=Object_Property_Codes </remarks>
    Public MustOverride Function GetPropertyUnit(ByVal prop As String, Optional ByVal su As DWSIM.SistemasDeUnidades.Unidades = Nothing)
    ''' <summary>
    ''' Sets the value of a property.
    ''' </summary>
    ''' <param name="prop">Property identifier.</param>
    ''' <param name="propval">Property value to set at the specified units.</param>
    ''' <param name="su">Units system to use. Null to use the default (SI) system.</param>
    ''' <returns></returns>
    ''' <remarks>More details at http://dwsim.inforside.com.br/wiki/index.php?title=Object_Property_Codes </remarks>
    Public MustOverride Function SetPropertyValue(ByVal prop As String, ByVal propval As Object, Optional ByVal su As DWSIM.SistemasDeUnidades.Unidades = Nothing)

    Public Sub HandlePropertyChange(ByVal s As Object, ByVal e As System.Windows.Forms.PropertyValueChangedEventArgs)

        'handle connection updates

        PropertyValueChanged(s, e)

        'handle other property changes

        Dim Conversor = New DWSIM.SistemasDeUnidades.Conversor

        Dim sobj As Microsoft.Msdn.Samples.GraphicObjects.GraphicObject = Me.GraphicObject

        FlowSheet.FormSurface.FlowsheetDesignSurface.SelectedObject = sobj

        If Not sobj Is Nothing Then

            If sobj.TipoObjeto = TipoObjeto.FlowsheetUO Then

                Dim fs As DWSIM.SimulationObjects.UnitOps.Flowsheet = FlowSheet.Collections.CLCS_FlowsheetUOCollection.Item(sobj.Name)

                If e.ChangedItem.PropertyDescriptor.Category.Equals(DWSIM.App.GetLocalString("LinkedInputParms")) Then

                    Dim pkey As String = CType(e.ChangedItem.PropertyDescriptor, CustomProperty.CustomPropertyDescriptor).CustomProperty.Tag

                    fs.Fsheet.Collections.ObjectCollection(fs.InputParams(pkey).ObjectID).SetPropertyValue(fs.InputParams(pkey).ObjectProperty, e.ChangedItem.Value, FlowSheet.Options.SelectedUnitSystem)

                    If FlowSheet.Options.CalculatorActivated Then

                        sobj.Calculated = True
                        FlowSheet.FormProps.HandleObjectStatusChanged(sobj)

                        'Call function to calculate flowsheet
                        Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                        With objargs
                            .Calculado = True
                            .Nome = sobj.Name
                            .Tag = sobj.Tag
                            .Tipo = TipoObjeto.FlowsheetUO
                            .Emissor = "PropertyGrid"
                        End With

                        If fs.IsSpecAttached = True And fs.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then FlowSheet.Collections.CLCS_SpecCollection(fs.AttachedSpecId).Calculate()
                        FlowSheet.CalculationQueue.Enqueue(objargs)

                    End If

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.MaterialStream Then

                Dim ms As DWSIM.SimulationObjects.Streams.MaterialStream = FlowSheet.Collections.CLCS_MaterialStreamCollection.Item(sobj.Name)

                If Not e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Base")) Then

                    Dim T, P, W, Q, QV, HM, SM, VF As Double

                    If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Temperatura")) Then
                        T = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_temperature, e.ChangedItem.Value)
                        ms.Fases(0).SPMProperties.temperature = T
                    ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Presso")) Then
                        P = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_pressure, e.ChangedItem.Value)
                        ms.Fases(0).SPMProperties.pressure = P
                    ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Vazomssica")) Then
                        W = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_massflow, e.ChangedItem.Value)
                        ms.Fases(0).SPMProperties.massflow = W
                        ms.Fases(0).SPMProperties.molarflow = Nothing
                        ms.Fases(0).SPMProperties.volumetric_flow = Nothing
                    ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Vazomolar")) Then
                        Q = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_molarflow, e.ChangedItem.Value)
                        ms.Fases(0).SPMProperties.molarflow = Q
                        ms.Fases(0).SPMProperties.massflow = Nothing
                        ms.Fases(0).SPMProperties.volumetric_flow = Nothing
                    ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Vazovolumtrica")) Then
                        QV = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_volumetricFlow, e.ChangedItem.Value)
                        ms.Fases(0).SPMProperties.volumetric_flow = QV
                        ms.Fases(0).SPMProperties.massflow = Nothing
                        ms.Fases(0).SPMProperties.molarflow = Nothing
                    ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("EntalpiaEspecfica")) Then
                        HM = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_enthalpy, e.ChangedItem.Value)
                        ms.Fases(0).SPMProperties.enthalpy = HM
                    ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("EntropiaEspecfica")) Then
                        SM = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_entropy, e.ChangedItem.Value)
                        ms.Fases(0).SPMProperties.entropy = SM
                    ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Vapor")) Then
                        VF = e.ChangedItem.Value
                        ms.Fases(2).SPMProperties.molarfraction = VF
                    End If

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Nome = sobj.Name
                        .Tag = sobj.Tag
                        .Tipo = TipoObjeto.MaterialStream
                        .Emissor = "PropertyGrid"
                    End With

                    FlowSheet.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.EnergyStream Then

                Dim es As DWSIM.SimulationObjects.Streams.EnergyStream = FlowSheet.Collections.CLCS_EnergyStreamCollection.Item(sobj.Name)

                If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Energia")) Then

                    es.Energia = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_heatflow, e.ChangedItem.Value)

                End If

                If FlowSheet.Options.CalculatorActivated Then

                    sobj.Calculated = True
                    FlowSheet.FormProps.HandleObjectStatusChanged(sobj)

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = True
                        .Nome = sobj.Name
                        .Tag = sobj.Tag
                        .Tipo = TipoObjeto.EnergyStream
                        .Emissor = "PropertyGrid"
                    End With

                    If es.IsSpecAttached = True And es.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then FlowSheet.Collections.CLCS_SpecCollection(es.AttachedSpecId).Calculate()
                    FlowSheet.CalculationQueue.Enqueue(objargs)

                End If


            ElseIf sobj.TipoObjeto = TipoObjeto.NodeOut Then

                Dim sp As DWSIM.SimulationObjects.UnitOps.Splitter = FlowSheet.Collections.CLCS_SplitterCollection.Item(sobj.Name)

                If e.ChangedItem.Label.Contains("[Split Ratio] ") Then

                    If e.ChangedItem.Value < 0.0# Or e.ChangedItem.Value > 1.0# Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))

                    Dim i As Integer = 0
                    Dim j As Integer = 0

                    Dim cp As ConnectionPoint
                    For Each cp In sp.GraphicObject.OutputConnectors
                        If cp.IsAttached Then
                            If e.ChangedItem.Label.Contains(cp.AttachedConnector.AttachedTo.Tag) Then
                                sp.Ratios(i) = e.ChangedItem.Value
                            End If
                        End If
                        i += 1
                    Next

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetPropertyName("PROP_SP_1")) Then

                    Select Case sp.OperationMode
                        Case SimulationObjects.UnitOps.Splitter.OpMode.StreamMassFlowSpec
                            sp.StreamFlowSpec = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_massflow, e.ChangedItem.Value)
                        Case SimulationObjects.UnitOps.Splitter.OpMode.StreamMoleFlowSpec
                            sp.StreamFlowSpec = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_molarflow, e.ChangedItem.Value)
                    End Select

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetPropertyName("PROP_SP_2")) Then

                    Select Case sp.OperationMode
                        Case SimulationObjects.UnitOps.Splitter.OpMode.StreamMassFlowSpec
                            sp.Stream2FlowSpec = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_massflow, e.ChangedItem.Value)
                        Case SimulationObjects.UnitOps.Splitter.OpMode.StreamMoleFlowSpec
                            sp.Stream2FlowSpec = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_molarflow, e.ChangedItem.Value)
                    End Select

                End If

                If FlowSheet.Options.CalculatorActivated Then

                    sobj.Calculated = True

                    FlowSheet.FormProps.HandleObjectStatusChanged(sobj)

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Nome = sobj.Name
                        .Tag = sobj.Tag
                        .Tipo = TipoObjeto.NodeOut
                        .Emissor = "PropertyGrid"
                    End With

                    If sp.IsSpecAttached = True And sp.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then FlowSheet.Collections.CLCS_SpecCollection(sp.AttachedSpecId).Calculate()
                    FlowSheet.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.Pump Then

                Dim bb As DWSIM.SimulationObjects.UnitOps.Pump = FlowSheet.Collections.CLCS_PumpCollection.Item(sobj.Name)

                If e.ChangedItem.Label.Contains("Delta P") Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.DeltaP = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_deltaP, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Pressoajusante")) Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.Pout = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_pressure, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Eficincia")) Then

                    If e.ChangedItem.Value <= 20 Or e.ChangedItem.Value > 100 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))

                End If

                If FlowSheet.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Tag = sobj.Tag
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.Pump
                        .Emissor = "PropertyGrid"
                    End With

                    If bb.IsSpecAttached = True And bb.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then FlowSheet.Collections.CLCS_SpecCollection(bb.AttachedSpecId).Calculate()
                    FlowSheet.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.Valve Then

                Dim bb As DWSIM.SimulationObjects.UnitOps.Valve = FlowSheet.Collections.CLCS_ValveCollection.Item(sobj.Name)

                If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Quedadepresso")) Then

                    bb.DeltaP = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_deltaP, e.ChangedItem.Value)

                    'If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("ValveOutletPressure")) Then

                    bb.OutletPressure = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_pressure, e.ChangedItem.Value)

                End If

                If FlowSheet.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Tag = sobj.Tag
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.Valve
                        .Emissor = "PropertyGrid"
                    End With

                    If bb.IsSpecAttached = True And bb.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then FlowSheet.Collections.CLCS_SpecCollection(bb.AttachedSpecId).Calculate()
                    FlowSheet.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.Filter Then

                Dim ft As DWSIM.SimulationObjects.UnitOps.Filter = FlowSheet.Collections.CLCS_FilterCollection.Item(sobj.Name)

                If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("FilterMediumResistance")) Then
                    ft.FilterMediumResistance = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.mediumresistance, e.ChangedItem.Value)
                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("FilterSpecificCakeResistance")) Then
                    ft.SpecificCakeResistance = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.cakeresistance, e.ChangedItem.Value)
                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("FilterCycleTime")) Then
                    ft.FilterCycleTime = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.time, e.ChangedItem.Value)
                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("FilterPressureDrop")) Then
                    ft.PressureDrop = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_deltaP, e.ChangedItem.Value)
                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("FilterArea")) Then
                    ft.TotalFilterArea = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.area, e.ChangedItem.Value)
                End If

                If FlowSheet.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Tag = sobj.Tag
                        .Nome = sobj.Name
                        .Tipo = sobj.TipoObjeto
                        .Emissor = "PropertyGrid"
                    End With

                    If ft.IsSpecAttached = True And ft.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then FlowSheet.Collections.CLCS_SpecCollection(ft.AttachedSpecId).Calculate()
                    FlowSheet.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.Compressor Then

                Dim bb As DWSIM.SimulationObjects.UnitOps.Compressor = FlowSheet.Collections.CLCS_CompressorCollection.Item(sobj.Name)

                If e.ChangedItem.Label.Contains("Delta P") Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.DeltaP = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_deltaP, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Presso")) Then
                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.POut = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_pressure, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Eficincia")) Then
                    If e.ChangedItem.Value <= 20 Or e.ChangedItem.Value > 100 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))

                End If

                If FlowSheet.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Tag = sobj.Tag
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.Compressor
                        .Emissor = "PropertyGrid"
                    End With

                    If bb.IsSpecAttached = True And bb.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then FlowSheet.Collections.CLCS_SpecCollection(bb.AttachedSpecId).Calculate()
                    FlowSheet.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.Expander Then

                Dim bb As DWSIM.SimulationObjects.UnitOps.Expander = FlowSheet.Collections.CLCS_TurbineCollection.Item(sobj.Name)

                If e.ChangedItem.Label.Contains("Delta P") Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.DeltaP = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_deltaP, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Presso")) Then
                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.POut = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_pressure, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Eficincia")) Then

                    If e.ChangedItem.Value <= 20 Or e.ChangedItem.Value > 100 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))

                End If

                If FlowSheet.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Tag = sobj.Tag
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.Expander
                        .Emissor = "PropertyGrid"
                    End With

                    If bb.IsSpecAttached = True And bb.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then FlowSheet.Collections.CLCS_SpecCollection(bb.AttachedSpecId).Calculate()
                    FlowSheet.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.Pipe Then

                Dim bb As DWSIM.SimulationObjects.UnitOps.Pipe = FlowSheet.Collections.CLCS_PipeCollection.Item(sobj.Name)

                If FlowSheet.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Tag = sobj.Tag
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.Pipe
                        .Emissor = "PropertyGrid"
                    End With

                    If bb.IsSpecAttached = True And bb.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then FlowSheet.Collections.CLCS_SpecCollection(bb.AttachedSpecId).Calculate()
                    FlowSheet.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.Heater Then

                Dim bb As DWSIM.SimulationObjects.UnitOps.Heater = FlowSheet.Collections.CLCS_HeaterCollection.Item(sobj.Name)

                If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Eficincia")) Then

                    If e.ChangedItem.Value <= 20 Or e.ChangedItem.Value > 100 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Calor")) Then

                    bb.DeltaQ = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_heatflow, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Quedadepresso")) Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.DeltaP = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_deltaP, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("HeaterCoolerOutletTemperature")) Then

                    bb.OutletTemperature = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_temperature, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("FraomolardafaseFaseV")) Then

                    bb.OutletVaporFraction = Double.Parse(e.ChangedItem.Value)

                End If

                If FlowSheet.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Tag = sobj.Tag
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.Heater
                        .Emissor = "PropertyGrid"
                    End With

                    If bb.IsSpecAttached = True And bb.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then FlowSheet.Collections.CLCS_SpecCollection(bb.AttachedSpecId).Calculate()
                    FlowSheet.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.Cooler Then

                Dim bb As DWSIM.SimulationObjects.UnitOps.Cooler = FlowSheet.Collections.CLCS_CoolerCollection.Item(sobj.Name)

                If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Eficincia")) Then

                    If e.ChangedItem.Value <= 20 Or e.ChangedItem.Value > 100 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Calor")) Then

                    bb.DeltaQ = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_heatflow, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Quedadepresso")) Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.DeltaP = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_deltaP, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("HeaterCoolerOutletTemperature")) Then

                    bb.OutletTemperature = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_temperature, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("FraomolardafaseFaseV")) Then

                    bb.OutletVaporFraction = Double.Parse(e.ChangedItem.Value)

                End If

                If FlowSheet.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Tag = sobj.Tag
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.Cooler
                        .Emissor = "PropertyGrid"
                    End With

                    If bb.IsSpecAttached = True And bb.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then FlowSheet.Collections.CLCS_SpecCollection(bb.AttachedSpecId).Calculate()
                    FlowSheet.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.Tank Then

                Dim bb As DWSIM.SimulationObjects.UnitOps.Tank = FlowSheet.Collections.CLCS_TankCollection.Item(sobj.Name)

                If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Quedadepresso")) Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.DeltaP = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_deltaP, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("AquecimentoResfriame")) Then

                    bb.DeltaQ = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_heatflow, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("TKVol")) Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.Volume = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.volume, e.ChangedItem.Value)

                End If

                If FlowSheet.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Tag = sobj.Tag
                        .Calculado = False
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.Tank
                        .Emissor = "PropertyGrid"
                    End With

                    If bb.IsSpecAttached = True And bb.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then FlowSheet.Collections.CLCS_SpecCollection(bb.AttachedSpecId).Calculate()
                    FlowSheet.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.OT_Ajuste Then

                Dim adj As DWSIM.SimulationObjects.SpecialOps.Adjust = FlowSheet.Collections.CLCS_AdjustCollection.Item(sobj.Name)

                With adj
                    If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("VarivelControlada")) Then
                        .ControlledObject = FlowSheet.Collections.ObjectCollection(.ControlledObjectData.m_ID)
                        .ControlledVariable = .ControlledObjectData.m_Property
                        CType(FlowSheet.Collections.AdjustCollection(adj.Nome), AdjustGraphic).ConnectedToCv = .ControlledObject.GraphicObject
                        .ReferenceObject = Nothing
                        .ReferenceVariable = Nothing
                        With .ReferencedObjectData
                            .m_ID = ""
                            .m_Name = ""
                            .m_Property = ""
                            .m_Type = ""
                        End With
                    ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("VarivelManipulada")) Then
                        .ManipulatedObject = FlowSheet.Collections.ObjectCollection(.ManipulatedObjectData.m_ID)
                        Dim gr As AdjustGraphic = FlowSheet.Collections.AdjustCollection(adj.Nome)
                        gr.ConnectedToMv = .ManipulatedObject.GraphicObject
                        .ManipulatedVariable = .ManipulatedObjectData.m_Property
                    ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("ObjetoVariveldeRefer")) Then
                        .ReferenceObject = FlowSheet.Collections.ObjectCollection(.ReferencedObjectData.m_ID)
                        .ReferenceVariable = .ReferencedObjectData.m_Property
                        Dim gr As AdjustGraphic = FlowSheet.Collections.AdjustCollection(adj.Nome)
                        gr.ConnectedToRv = .ReferenceObject.GraphicObject
                    ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Valormnimoopcional")) Then
                        adj.MinVal = Conversor.ConverterParaSI(adj.ManipulatedObject.GetPropertyUnit(adj.ManipulatedObjectData.m_Property, FlowSheet.Options.SelectedUnitSystem), e.ChangedItem.Value)
                    ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Valormximoopcional")) Then
                        adj.MaxVal = Conversor.ConverterParaSI(adj.ManipulatedObject.GetPropertyUnit(adj.ManipulatedObjectData.m_Property, FlowSheet.Options.SelectedUnitSystem), e.ChangedItem.Value)
                    ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("ValordeAjusteouOffse")) Then
                        adj.AdjustValue = Conversor.ConverterParaSI(adj.ControlledObject.GetPropertyUnit(adj.ControlledObjectData.m_Property, FlowSheet.Options.SelectedUnitSystem), e.ChangedItem.Value)
                    End If
                End With

            ElseIf sobj.TipoObjeto = TipoObjeto.OT_Especificacao Then

                Dim spec As DWSIM.SimulationObjects.SpecialOps.Spec = FlowSheet.Collections.CLCS_SpecCollection.Item(sobj.Name)

                With spec
                    If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("VarivelDestino")) Then
                        .TargetObject = FlowSheet.Collections.ObjectCollection(.TargetObjectData.m_ID)
                        .TargetVariable = .TargetObjectData.m_Property
                        CType(FlowSheet.Collections.SpecCollection(spec.Nome), SpecGraphic).ConnectedToTv = .TargetObject.GraphicObject
                    ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("VarivelFonte")) Then
                        .SourceObject = FlowSheet.Collections.ObjectCollection(.SourceObjectData.m_ID)
                        Dim gr As SpecGraphic = FlowSheet.Collections.SpecCollection(spec.Nome)
                        gr.ConnectedToSv = .SourceObject.GraphicObject
                        .SourceVariable = .SourceObjectData.m_Property
                    End If
                End With

            ElseIf sobj.TipoObjeto = TipoObjeto.Vessel Then

                Dim vessel As DWSIM.SimulationObjects.UnitOps.Vessel = FlowSheet.Collections.CLCS_VesselCollection.Item(sobj.Name)

                Dim T, P As Double
                If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Temperatura")) Then
                    T = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_temperature, e.ChangedItem.Value)
                    vessel.FlashTemperature = T
                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Presso")) Then
                    P = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_pressure, e.ChangedItem.Value)
                    vessel.FlashPressure = P
                End If

                If FlowSheet.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Tag = sobj.Tag
                        .Calculado = False
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.Vessel
                        .Emissor = "PropertyGrid"
                    End With

                    If vessel.IsSpecAttached = True And vessel.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then FlowSheet.Collections.CLCS_SpecCollection(vessel.AttachedSpecId).Calculate()
                    FlowSheet.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.OT_Reciclo Then

                Dim rec As DWSIM.SimulationObjects.SpecialOps.Recycle = FlowSheet.Collections.CLCS_RecycleCollection.Item(sobj.Name)

                Dim T, P, W As Double
                If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Temperatura")) Then
                    T = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_deltaT, e.ChangedItem.Value)
                    rec.ConvergenceParameters.Temperatura = T
                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Presso")) Then
                    P = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_deltaP, e.ChangedItem.Value)
                    rec.ConvergenceParameters.Pressao = P
                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("mssica")) Then
                    W = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_massflow, e.ChangedItem.Value)
                    rec.ConvergenceParameters.VazaoMassica = W
                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.RCT_Conversion Then

                Dim bb As DWSIM.SimulationObjects.Reactors.Reactor_Conversion = FlowSheet.Collections.CLCS_ReactorConversionCollection.Item(sobj.Name)

                If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Quedadepresso")) Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.DeltaP = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_deltaP, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("HeaterCoolerOutletTemperature")) Then

                    bb.OutletTemperature = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_temperature, e.ChangedItem.Value)

                End If

                If FlowSheet.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Tag = sobj.Tag
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.RCT_Conversion
                        .Emissor = "PropertyGrid"
                    End With

                    If bb.IsSpecAttached = True And bb.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then FlowSheet.Collections.CLCS_SpecCollection(bb.AttachedSpecId).Calculate()
                    FlowSheet.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.RCT_Equilibrium Then

                Dim bb As DWSIM.SimulationObjects.Reactors.Reactor_Equilibrium = FlowSheet.Collections.CLCS_ReactorEquilibriumCollection.Item(sobj.Name)

                If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Quedadepresso")) Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.DeltaP = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_deltaP, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("HeaterCoolerOutletTemperature")) Then

                    bb.OutletTemperature = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_temperature, e.ChangedItem.Value)

                End If

                If FlowSheet.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Tag = sobj.Tag
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.RCT_Equilibrium
                        .Emissor = "PropertyGrid"
                    End With

                    If bb.IsSpecAttached = True And bb.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then FlowSheet.Collections.CLCS_SpecCollection(bb.AttachedSpecId).Calculate()
                    FlowSheet.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.RCT_Gibbs Then

                Dim bb As DWSIM.SimulationObjects.Reactors.Reactor_Gibbs = FlowSheet.Collections.CLCS_ReactorGibbsCollection.Item(sobj.Name)

                If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Quedadepresso")) Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.DeltaP = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_deltaP, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("HeaterCoolerOutletTemperature")) Then

                    bb.OutletTemperature = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_temperature, e.ChangedItem.Value)

                End If

                If FlowSheet.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Tag = sobj.Tag
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.RCT_Gibbs
                        .Emissor = "PropertyGrid"
                    End With

                    If bb.IsSpecAttached = True And bb.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then FlowSheet.Collections.CLCS_SpecCollection(bb.AttachedSpecId).Calculate()
                    FlowSheet.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.RCT_CSTR Then

                Dim bb As DWSIM.SimulationObjects.Reactors.Reactor_CSTR = FlowSheet.Collections.CLCS_ReactorCSTRCollection.Item(sobj.Name)

                If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("RSCTRIsothermalTemperature")) Then

                    bb.IsothermalTemperature = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_temperature, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("HeaterCoolerOutletTemperature")) Then

                    bb.OutletTemperature = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_temperature, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Quedadepresso")) Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.DeltaP = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_deltaP, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("CSTRCatalystAmount")) Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.CatalystAmount = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.mass, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("RCSTRPGridItem1")) Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.Volume = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.volume, e.ChangedItem.Value)

                End If

                If FlowSheet.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Tag = sobj.Tag
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.RCT_CSTR
                        .Emissor = "PropertyGrid"
                    End With

                    If bb.IsSpecAttached = True And bb.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then FlowSheet.Collections.CLCS_SpecCollection(bb.AttachedSpecId).Calculate()
                    FlowSheet.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.RCT_PFR Then

                Dim bb As DWSIM.SimulationObjects.Reactors.Reactor_PFR = FlowSheet.Collections.CLCS_ReactorPFRCollection.Item(sobj.Name)

                If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Quedadepresso")) Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.DeltaP = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_deltaP, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("HeaterCoolerOutletTemperature")) Then

                    bb.OutletTemperature = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_temperature, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("RCSTRPGridItem1")) Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.Volume = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.volume, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("PFRLength")) Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.Length = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.distance, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("PFRCatalystLoading")) Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.CatalystLoading = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_density, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("PFRCatalystParticleDiameter")) Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.CatalystParticleDiameter = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.diameter, e.ChangedItem.Value)

                End If

                If FlowSheet.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Tag = sobj.Tag
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.RCT_PFR
                        .Emissor = "PropertyGrid"
                    End With

                    If bb.IsSpecAttached = True And bb.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then FlowSheet.Collections.CLCS_SpecCollection(bb.AttachedSpecId).Calculate()
                    FlowSheet.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.HeatExchanger Then

                Dim bb As DWSIM.SimulationObjects.UnitOps.HeatExchanger = FlowSheet.Collections.CLCS_HeatExchangerCollection.Item(sobj.Name)

                If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("OverallHeatTranferCoefficient")) Then

                    If e.ChangedItem.Value < 0 Then Throw New InvalidCastException(DWSIM.App.GetLocalString("Ovalorinformadonovli"))
                    bb.OverallCoefficient = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.heat_transf_coeff, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("Area")) Then

                    bb.Area = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.area, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("HeatLoad")) Then

                    bb.Q = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_heatflow, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("HXHotSidePressureDrop")) Then

                    bb.HotSidePressureDrop = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_deltaP, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("HXColdSidePressureDrop")) Then

                    bb.ColdSidePressureDrop = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_deltaP, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("HXTempHotOut")) Then

                    bb.HotSideOutletTemperature = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_temperature, e.ChangedItem.Value)

                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("HXTempColdOut")) Then

                    bb.ColdSideOutletTemperature = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_temperature, e.ChangedItem.Value)

                End If

                If FlowSheet.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Tag = sobj.Tag
                        .Calculado = False
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.HeatExchanger
                        .Emissor = "PropertyGrid"
                    End With

                    If bb.IsSpecAttached = True And bb.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then FlowSheet.Collections.CLCS_SpecCollection(bb.AttachedSpecId).Calculate()
                    FlowSheet.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.ShortcutColumn Then

                Dim sc As DWSIM.SimulationObjects.UnitOps.ShortcutColumn = FlowSheet.Collections.CLCS_ShortcutColumnCollection.Item(sobj.Name)
                Dim Pr, Pc As Double

                If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("SCCondenserType")) Then
                    sc.GraphicObject.Shape = sc.condtype
                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("SCCondenserPressure")) Then
                    Pc = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_pressure, e.ChangedItem.Value)
                    sc.m_condenserpressure = Pc
                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("SCReboilerPressure")) Then
                    Pr = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.spmp_pressure, e.ChangedItem.Value)
                    sc.m_boilerpressure = Pr
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("SCLightKey")) Then
                    sc.m_lightkey = e.ChangedItem.Value
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("SCHeavyKey")) Then
                    sc.m_heavykey = e.ChangedItem.Value
                End If

                If FlowSheet.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Tag = sobj.Tag
                        .Calculado = False
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.ShortcutColumn
                        .Emissor = "PropertyGrid"
                    End With

                    If sc.IsSpecAttached = True And sc.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then FlowSheet.Collections.CLCS_SpecCollection(sc.AttachedSpecId).Calculate()
                    FlowSheet.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.OrificePlate Then

                Dim op As DWSIM.SimulationObjects.UnitOps.OrificePlate = FlowSheet.Collections.CLCS_OrificePlateCollection.Item(sobj.Name)

                If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("OPOrificeDiameter")) Then
                    op.OrificeDiameter = Conversor.ConverterParaSI(FlowSheet.Options.SelectedUnitSystem.diameter, e.ChangedItem.Value)
                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("OPBeta")) Then
                    op.Beta = e.ChangedItem.Value
                ElseIf e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("OPCorrectionFactor")) Then
                    op.CorrectionFactor = e.ChangedItem.Value
                End If

                If FlowSheet.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Tag = sobj.Tag
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.OrificePlate
                        .Emissor = "PropertyGrid"
                    End With

                    If op.IsSpecAttached = True And op.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then FlowSheet.Collections.CLCS_SpecCollection(op.AttachedSpecId).Calculate()
                    FlowSheet.CalculationQueue.Enqueue(objargs)

                End If


            ElseIf sobj.TipoObjeto = TipoObjeto.ExcelUO Then

                Dim eo As DWSIM.SimulationObjects.UnitOps.ExcelUO = FlowSheet.Collections.CLCS_ExcelUOCollection.Item(sobj.Name)
                Dim P1 As Integer
                Dim L As String
                P1 = InStr(1, e.ChangedItem.Label, "(") - 2
                If P1 > 0 Then
                    L = Strings.Left(e.ChangedItem.Label, P1)
                    If eo.InputParams.ContainsKey(L) Then
                        eo.InputParams(L).Value = e.ChangedItem.Value
                    End If
                End If

                If FlowSheet.Options.CalculatorActivated Then

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = False
                        .Tag = sobj.Tag
                        .Nome = sobj.Name
                        .Tipo = TipoObjeto.ExcelUO
                        .Emissor = "PropertyGrid"
                    End With

                    If eo.IsSpecAttached = True And eo.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then FlowSheet.Collections.CLCS_SpecCollection(eo.AttachedSpecId).Calculate()
                    FlowSheet.CalculationQueue.Enqueue(objargs)

                End If

            ElseIf sobj.TipoObjeto = TipoObjeto.DistillationColumn Or sobj.TipoObjeto = TipoObjeto.AbsorptionColumn Or sobj.TipoObjeto = TipoObjeto.ReboiledAbsorber Or
                sobj.TipoObjeto = TipoObjeto.RefluxedAbsorber Or sobj.TipoObjeto = TipoObjeto.CapeOpenUO Then


                If FlowSheet.Options.CalculatorActivated Then

                    sobj.Calculated = True
                    FlowSheet.FormProps.HandleObjectStatusChanged(sobj)

                    'Call function to calculate flowsheet
                    Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
                    With objargs
                        .Calculado = True
                        .Nome = sobj.Name
                        .Tag = sobj.Tag
                        .Tipo = sobj.TipoObjeto
                        .Emissor = "PropertyGrid"
                    End With

                    Dim obj = FlowSheet.Collections.ObjectCollection.Item(sobj.Name)

                    If obj.IsSpecAttached = True And obj.SpecVarType = DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar.Fonte Then FlowSheet.Collections.CLCS_SpecCollection(obj.AttachedSpecId).Calculate()
                    FlowSheet.CalculationQueue.Enqueue(objargs)

                End If

            End If

        End If

        Call FlowSheet.FormSurface.UpdateSelectedObject()
        Call FlowSheet.FormSurface.FlowsheetDesignSurface.Invalidate()

        CalculateAll2(FlowSheet, My.Settings.SolverMode, , True)

    End Sub

    Public Overridable Sub PropertyValueChanged(ByVal s As Object, ByVal e As System.Windows.Forms.PropertyValueChangedEventArgs)

        Dim ChildParent As FormFlowsheet = FlowSheet
        Dim sobj As Microsoft.Msdn.Samples.GraphicObjects.GraphicObject = ChildParent.FormSurface.FlowsheetDesignSurface.SelectedObject

        If Not sobj Is Nothing Then

            'connections
            If sobj.TipoObjeto = TipoObjeto.Cooler Or sobj.TipoObjeto = TipoObjeto.Pipe Or sobj.TipoObjeto = TipoObjeto.Expander Then
                If e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y + sobj.Height / 2 - 10, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesada")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + sobj.Height / 2 - 10, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeenergia")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.EnergyStream, sobj.X + sobj.Width + 40, sobj.Y + sobj.Height + 30, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.EnergyConnector.IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                        End If
                    End If
                End If
            ElseIf sobj.TipoObjeto = TipoObjeto.ExcelUO Then
                If e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada1")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y - 45, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada2")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y - 15, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(1).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(1).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(1).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada3")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y + 15, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(2).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(2).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(2).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada4")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y + 45, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(3).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(3).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(3).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesaida1")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y - 45, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesaida2")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y - 15, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(1).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(1).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(1).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesaida3")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + 15, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(2).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(2).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(2).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesaida4")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + 45, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(3).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(3).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(3).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeenergia")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.EnergyStream, sobj.X - 60, sobj.Y + 75, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(4).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(4).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(4).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                End If
            ElseIf sobj.TipoObjeto = TipoObjeto.Compressor Or sobj.TipoObjeto = TipoObjeto.Heater Or sobj.TipoObjeto = TipoObjeto.Pump Or
                         sobj.TipoObjeto = TipoObjeto.RCT_PFR Or sobj.TipoObjeto = TipoObjeto.RCT_CSTR Then
                If e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y + sobj.Height / 2 - 10, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesada")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + sobj.Height / 2 - 10, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeenergia")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.EnergyStream, sobj.X - 60, sobj.Y + sobj.Height + 20, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(1).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(1).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(1).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                End If
            ElseIf sobj.TipoObjeto = TipoObjeto.Valve Or sobj.TipoObjeto = TipoObjeto.OrificePlate Or sobj.TipoObjeto = TipoObjeto.OT_Reciclo Or sobj.TipoObjeto = TipoObjeto.Tank Then
                If e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y + sobj.Height / 2 - 10, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesada")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + sobj.Height / 2 - 10, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                        End If
                    End If
                End If
            ElseIf sobj.TipoObjeto = TipoObjeto.Vessel Then
                If e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada1")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y - 50, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 0)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada2")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y - 20, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(1).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 1)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(1).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 1)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(1).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada3")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y + 10, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(2).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 2)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(2).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 2)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(2).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada4")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y + 40, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(3).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 3)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(3).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 3)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(3).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada5")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y + 70, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(4).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 4)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(4).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 4)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(4).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada6")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y + 100, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(5).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 5)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(5).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 5)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(5).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label = (DWSIM.App.GetLocalString("Saidadevapor")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y - 20, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 0, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 0, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label = (DWSIM.App.GetLocalString("Saidadelquido")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + 50, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(1).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 1, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(1).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 1, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(1).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label = (DWSIM.App.GetLocalString("Saidadelquido") & " (2)") Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + 100, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(2).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 2, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(2).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 2, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(2).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeenergia")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.EnergyStream, sobj.X - 60, sobj.Y + 130, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(6).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(6).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(6).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                End If
            ElseIf sobj.TipoObjeto = TipoObjeto.FlowsheetUO Then
                If e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada1")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y - 60, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 0)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada2")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y - 30, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(1).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 1)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(1).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 1)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(1).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada3")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y + 0, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(2).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 2)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(2).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 2)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(2).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada4")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y + 30, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(3).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 3)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(3).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 3)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(3).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada5")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y + 60, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(4).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 4)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(4).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 4)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(4).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada6")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y + 90, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(5).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 5)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(5).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 5)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(5).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada7")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y + 120, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(6).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 6)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(6).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 6)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(6).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada8")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y + 150, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(7).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 7)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(7).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 7)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(7).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada9")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y + 180, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(8).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 8)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(8).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 8)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(8).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada10")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y + 210, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(9).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 9)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(9).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 9)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(9).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesaida1")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y - 60, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesaida2")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y - 30, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(1).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(1).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(1).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesaida3")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + 0, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(2).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(2).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(2).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesaida4")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + 30, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(3).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(3).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(3).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesaida5")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + 60, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(4).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(4).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(4).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesaida6")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + 90, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(5).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(5).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(5).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesaida7")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + 120, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(6).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(6).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(6).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesaida8")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + 150, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(7).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(7).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(7).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesaida9")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + 180, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(8).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(8).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(8).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesaida10")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + 210, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(9).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(9).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(9).AttachedConnector.AttachedTo)
                        End If
                    End If
                End If
            ElseIf sobj.TipoObjeto = TipoObjeto.RCT_Conversion Or sobj.TipoObjeto = TipoObjeto.RCT_Equilibrium Or sobj.TipoObjeto = TipoObjeto.RCT_Gibbs Then
                If e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y + sobj.Height / 2 - 10, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label = (DWSIM.App.GetLocalString("Saidadevapor")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + 0.17 * sobj.Height - 10, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 0, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 0, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label = (DWSIM.App.GetLocalString("Saidadelquido")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + sobj.Height * 0.843 - 10, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(1).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 1, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 1, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeenergia")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.EnergyStream, sobj.X - 60, sobj.Y + sobj.Height + 20, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(1).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(1).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(1).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                End If
            ElseIf sobj.TipoObjeto = TipoObjeto.NodeIn Then
                If e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada1")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y - 75, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 0)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada2")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y - 45, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(1).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 1)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(1).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 1)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(1).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada3")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y - 15, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(2).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 2)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(2).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 2)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(2).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada4")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y + 15, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(3).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 3)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(3).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 3)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(3).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada5")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y + 45, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(4).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 4)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(4).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 4)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(4).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada6")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y + 75, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(5).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 5)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(5).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 5)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(5).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Conectadoasada")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                        End If
                    End If
                End If
            ElseIf sobj.TipoObjeto = TipoObjeto.NodeOut Then
                If e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesaida1")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y - 30, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 0, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 0, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesaida2")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(1).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 1, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(1).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 1, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(1).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesaida3")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + 30, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(2).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 2, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(2).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 2, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(2).AttachedConnector.AttachedTo)
                        End If
                    End If
                End If
            ElseIf sobj.TipoObjeto = TipoObjeto.ShortcutColumn Then
                If e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("SCFeed")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y + 0.5 * sobj.Height - 10, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("SCReboilerDuty")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.EnergyStream, sobj.X - 60, sobj.Y + sobj.Height + 20, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(1).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 1)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(1).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 1)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(1).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("SCDistillate")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + 0.3 * sobj.Height - 10, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 0, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 0, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("SCBottoms")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + 0.98 * sobj.Height - 10, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(1).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 1, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(1).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 1, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(1).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("SCCondenserDuty")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.EnergyStream, sobj.X + sobj.Width + 40, sobj.Y + 0.175 * sobj.Height - 30, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.EnergyConnector.IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.EnergyConnector.AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.EnergyConnector.AttachedConnector.AttachedTo)
                        End If
                    End If
                End If
            ElseIf sobj.TipoObjeto = TipoObjeto.HeatExchanger Then
                If e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada1")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y + sobj.Height / 2 - 10, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada2")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y + sobj.Height / 2 - 50, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(1).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(1).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(1).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesaida1")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + sobj.Height / 2 - 10, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 0, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 0, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesaida2")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + sobj.Height / 2 + 30, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(1).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 1, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(1).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 1, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(1).AttachedConnector.AttachedTo)
                        End If
                    End If
                End If
            ElseIf sobj.TipoObjeto = TipoObjeto.OT_EnergyRecycle Then
                If e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.EnergyStream, sobj.X - 60, sobj.Y, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesada")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.EnergyStream, sobj.X + sobj.Width + 40, sobj.Y, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                        End If
                    End If
                End If
            ElseIf sobj.TipoObjeto = TipoObjeto.ComponentSeparator Or sobj.TipoObjeto = TipoObjeto.SolidSeparator Or sobj.TipoObjeto = TipoObjeto.Filter Then
                If e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y + sobj.Height * 0.5 - 10, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("OutletStream1")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + 0.17 * sobj.Height - 10, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 0, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 0, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("OutletStream2")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + sobj.Height * 0.83 - 10, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(1).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 1, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 1, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeenergia")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.EnergyStream, sobj.X + sobj.Width + 40, sobj.Y + sobj.Height + 20, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.EnergyConnector.IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.EnergyConnector.AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.EnergyConnector.AttachedConnector.AttachedTo)
                        End If
                    End If
                End If
            ElseIf sobj.TipoObjeto = TipoObjeto.CustomUO Then
                If e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada1")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y - 65, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 0)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(0).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada2")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y - 35, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(1).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 1)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(1).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 1)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(1).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada3")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y - 5, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(2).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 2)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(2).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 2)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(2).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada4")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y + 25, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(4).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 4)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(4).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 4)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(4).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada5")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y + 55, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(5).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 5)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(5).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 5)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(5).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedeentrada6")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X - 60, sobj.Y + 85, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(6).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 6)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(6).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj, 0, 6)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(6).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("CorrentedeenergiaE")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.EnergyStream, sobj.X - 60, sobj.Y + 115, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).OutputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.InputConnectors(3).IsAttached Then
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        Else
                            ChildParent.DisconnectObject(sobj.InputConnectors(3).AttachedConnector.AttachedFrom, sobj)
                            ChildParent.ConnectObject(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), sobj)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj.InputConnectors(3).AttachedConnector.AttachedFrom, sobj)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesaida1")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y - 65, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(0).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 0, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 0, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(0).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesaida2")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y - 35, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(1).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 1, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(1).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 1, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(1).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesaida3")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y - 5, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(2).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 2, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(2).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 2, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(2).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesaida4")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + 25, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(4).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 4, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(4).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 4, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(4).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesaida5")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + 55, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(5).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 5, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(5).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 5, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(5).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("Correntedesaida6")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.MaterialStream, sobj.X + sobj.Width + 40, sobj.Y + 85, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(6).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 6, 0)
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(6).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), 6, 0)
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(6).AttachedConnector.AttachedTo)
                        End If
                    End If
                ElseIf e.ChangedItem.Label.Equals(DWSIM.App.GetLocalString("CorrentedeenergiaS")) Then
                    If e.ChangedItem.Value <> "" Then
                        If FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface) Is Nothing Then
                            Dim oguid As String = ChildParent.FormSurface.AddObjectToSurface(TipoObjeto.EnergyStream, sobj.X + sobj.Width + 40, sobj.Y + 115, e.ChangedItem.Value)
                        ElseIf CType(FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface), GraphicObject).InputConnectors(0).IsAttached Then
                            MessageBox.Show(DWSIM.App.GetLocalString("Todasasconexespossve"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)
                            Exit Sub
                        End If
                        If Not sobj.OutputConnectors(3).IsAttached Then
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        Else
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(3).AttachedConnector.AttachedTo)
                            ChildParent.ConnectObject(sobj, FormFlowsheet.SearchSurfaceObjectsByTag(e.ChangedItem.Value, ChildParent.FormSurface.FlowsheetDesignSurface))
                        End If
                    Else
                        If e.OldValue.ToString <> "" Then
                            ChildParent.DisconnectObject(sobj, sobj.OutputConnectors(3).AttachedConnector.AttachedTo)
                        End If
                    End If
                End If
            End If
        End If

    End Sub

    ''' <summary>
    ''' Formats a property string, adding its units in parenthesis.
    ''' </summary>
    ''' <param name="prop">Property string</param>
    ''' <param name="unit">Property units</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function FT(ByRef prop As String, ByVal unit As String)
        Return prop & " (" & unit & ")"
    End Function

    ''' <summary>
    ''' Sets the Flowsheet to which this object belongs to.
    ''' </summary>
    ''' <param name="flowsheet">Flowsheet instance.</param>
    ''' <remarks></remarks>
    Public Sub SetFlowsheet(ByVal flowsheet As FormFlowsheet)
        m_flowsheet = flowsheet
    End Sub

    ''' <summary>
    ''' Gets the current flowsheet where this object is.
    ''' </summary>
    ''' <value></value>
    ''' <returns>Flowsheet instance.</returns>
    ''' <remarks></remarks>
    Public Overridable ReadOnly Property FlowSheet() As Global.DWSIM.FormFlowsheet
        Get
            If Not m_flowsheet Is Nothing Then
                Return m_flowsheet
            Else
                Dim frm As FormFlowsheet = My.Application.ActiveSimulation
                If Not frm Is Nothing Then Return frm Else Return Nothing
                If Not My.Application.CAPEOPENMode Then
                    If Not FormMain.ActiveMdiChild Is Nothing Then
                        If TypeOf FormMain.ActiveMdiChild Is FormFlowsheet Then
                            If frm Is Nothing Then frm = FormMain.ActiveMdiChild Else m_flowsheet = frm
                            If frm Is Nothing Then frm = m_flowsheet
                            If Not frm Is Nothing Then Return frm Else Return Nothing
                        Else
                            If FormMain.ActiveMdiChild IsNot Nothing Then
                                If TypeOf FormMain.ActiveMdiChild Is FormFlowsheet Then Return FormMain.ActiveMdiChild Else Return Nothing
                            Else
                                Return Nothing
                            End If
                        End If
                    Else
                        If frm Is Nothing Then frm = m_flowsheet
                        If Not frm Is Nothing Then Return frm Else Return Nothing
                    End If
                Else
                    Return Nothing
                End If

            End If

        End Get
    End Property

    Public Property Annotation() As DWSIM.Outros.Annotation
        Get
            If Me.m_annotation Is Nothing Then Me.m_annotation = New DWSIM.Outros.Annotation
            Return Me.m_annotation
        End Get
        Set(ByVal value As DWSIM.Outros.Annotation)
            Me.m_annotation = value
        End Set
    End Property

    ''' <summary>
    ''' Checks if an Adjust operation is attached to this object.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property IsAdjustAttached() As Boolean
        Get
            Return Me.m_IsAdjustAttached
        End Get
        Set(ByVal value As Boolean)
            Me.m_IsAdjustAttached = value
        End Set
    End Property

    ''' <summary>
    ''' If an Adjust object is attached to this object, returns its ID.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property AttachedAdjustId() As String
        Get
            Return Me.m_AdjustId
        End Get
        Set(ByVal value As String)
            Me.m_AdjustId = value
        End Set
    End Property

    ''' <summary>
    ''' If an Adjust object is attached to this object, returns a variable describing how this object is used by it (manipulated, controlled or reference).
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property AdjustVarType() As DWSIM.SimulationObjects.SpecialOps.Helpers.Adjust.TipoVar
        Get
            Return Me.m_AdjustVarType
        End Get
        Set(ByVal value As DWSIM.SimulationObjects.SpecialOps.Helpers.Adjust.TipoVar)
            Me.m_AdjustVarType = value
        End Set
    End Property

    ''' <summary>
    ''' Checks if an Specification operation is attached to this object.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property IsSpecAttached() As Boolean
        Get
            Return Me.m_IsSpecAttached
        End Get
        Set(ByVal value As Boolean)
            Me.m_IsSpecAttached = value
        End Set
    End Property

    ''' <summary>
    ''' If an Specification object is attached to this object, returns its ID.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property AttachedSpecId() As String
        Get
            Return Me.m_SpecId
        End Get
        Set(ByVal value As String)
            Me.m_SpecId = value
        End Set
    End Property
    ''' <summary>
    ''' If an Specification object is attached to this object, returns a variable describing how this object is used by it (target or source).
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SpecVarType() As DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar
        Get
            Return Me.m_SpecVarType
        End Get
        Set(ByVal value As DWSIM.SimulationObjects.SpecialOps.Helpers.Spec.TipoVar)
            Me.m_SpecVarType = value
        End Set
    End Property
    ''' <summary>
    ''' Gets or sets the graphic object representation of this object in the flowsheet.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property GraphicObject() As GraphicObject
        Get
            Return m_graphicobject
        End Get
        Set(ByVal gObj As GraphicObject)
            m_graphicobject = gObj
        End Set
    End Property

    Public Property NodeTableItems() As System.Collections.Generic.Dictionary(Of Integer, DWSIM.Outros.NodeItem)
        Get
            If m_nodeitems Is Nothing Then m_nodeitems = New Generic.Dictionary(Of Integer, DWSIM.Outros.NodeItem)()
            Return m_nodeitems
        End Get
        Set(ByVal value As System.Collections.Generic.Dictionary(Of Integer, DWSIM.Outros.NodeItem))
            m_nodeitems = value
        End Set
    End Property

    Public Property QTNodeTableItems() As System.Collections.Generic.Dictionary(Of Integer, DWSIM.Outros.NodeItem)
        Get
            If m_qtnodeitems Is Nothing Then m_qtnodeitems = New Generic.Dictionary(Of Integer, DWSIM.Outros.NodeItem)()
            Return m_qtnodeitems
        End Get
        Set(ByVal value As System.Collections.Generic.Dictionary(Of Integer, DWSIM.Outros.NodeItem))
            m_qtnodeitems = value
        End Set
    End Property
    ''' <summary>
    ''' Object's description
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Descricao() As String
        Get
            Return m_ComponentDescription
        End Get
        Set(ByVal value As String)
            m_ComponentDescription = value
        End Set
    End Property

    ''' <summary>
    ''' Object's Unique ID (Name)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>This property is the same as the graphic object 'Name' property.</remarks>
    Public Property Nome() As String
        Get
            Return m_ComponentName
        End Get
        Set(ByVal value As String)
            m_ComponentName = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the flowsheet table object associated with this object.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Tabela() As DWSIM.GraphicObjects.TableGraphic
        Get
            Return m_table
        End Get
        Set(ByVal value As DWSIM.GraphicObjects.TableGraphic)
            m_table = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the tooltip (quick table) object associated with this object.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TabelaRapida() As DWSIM.GraphicObjects.QuickTableGraphic
        Get
            Return m_qtable
        End Get
        Set(ByVal value As DWSIM.GraphicObjects.QuickTableGraphic)
            m_qtable = value
        End Set
    End Property

    Public Property ShowQuickTable() As Boolean
        Get
            Return Me.m_showqtable
        End Get
        Set(ByVal value As Boolean)
            Me.m_showqtable = value
        End Set
    End Property

    Sub CreateNew()
        CreatedWithThreadID = Threading.Thread.CurrentThread.ManagedThreadId
        Me.m_annotation = New DWSIM.Outros.Annotation
        Me.m_nodeitems = New System.Collections.Generic.Dictionary(Of Integer, DWSIM.Outros.NodeItem)
        Me.m_qtnodeitems = New System.Collections.Generic.Dictionary(Of Integer, DWSIM.Outros.NodeItem)
    End Sub

    ''' <summary>
    ''' Clones the current object, returning a new one with identical properties.
    ''' </summary>
    ''' <returns>An object of the same type with the same properties.</returns>
    ''' <remarks>Properties and fields marked with the 'NonSerializable' attribute aren't cloned.</remarks>
    Public Function Clone() As Object Implements System.ICloneable.Clone

        Return ObjectCopy(Me)

    End Function

    Function ObjectCopy(ByVal obj As SimulationObjects_BaseClass) As SimulationObjects_BaseClass

        Dim objMemStream As New MemoryStream(250000)
        Dim objBinaryFormatter As New BinaryFormatter(Nothing, New StreamingContext(StreamingContextStates.Clone))

        objBinaryFormatter.Serialize(objMemStream, obj)

        objMemStream.Seek(0, SeekOrigin.Begin)

        ObjectCopy = objBinaryFormatter.Deserialize(objMemStream)

        objMemStream.Close()

    End Function

    Public disposedValue As Boolean = False        ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: free other state (managed objects).
            End If

            ' TODO: free your own state (unmanaged objects).
            ' TODO: set large fields to null.
        End If
        Me.disposedValue = True
    End Sub

#Region "   IDisposable Support "
    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

    ''' <summary>
    ''' Loads object data stored in a collection of XML elements.
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable Function LoadData(data As System.Collections.Generic.List(Of System.Xml.Linq.XElement)) As Boolean Implements XMLSerializer.Interfaces.ICustomXMLSerialization.LoadData

        XMLSerializer.XMLSerializer.Deserialize(Me, data)

        For Each xel2 As XElement In (From xel As XElement In data Select xel Where xel.Name = "NodeItems").Elements
            Dim text As String = xel2.@Text
            Dim ni2 As DWSIM.Outros.NodeItem = (From ni As DWSIM.Outros.NodeItem In m_nodeitems.Values Select ni Where ni.Text = text).SingleOrDefault
            If Not ni2 Is Nothing Then
                ni2.Checked = True
            End If
        Next

    End Function

    ''' <summary>
    ''' Saves object data in a collection of XML elements.
    ''' </summary>
    ''' <returns>A List of XML elements containing object data.</returns>
    ''' <remarks></remarks>
    Public Overridable Function SaveData() As System.Collections.Generic.List(Of System.Xml.Linq.XElement) Implements XMLSerializer.Interfaces.ICustomXMLSerialization.SaveData

        Dim elements As System.Collections.Generic.List(Of System.Xml.Linq.XElement) = XMLSerializer.XMLSerializer.Serialize(Me)
        Dim ci As Globalization.CultureInfo = Globalization.CultureInfo.InvariantCulture

        With elements

            .Add(New XElement("NodeItems"))
            For Each kvp As KeyValuePair(Of Integer, DWSIM.Outros.NodeItem) In m_nodeitems
                If kvp.Value.Checked Then .Item(.Count - 1).Add(New XElement("NodeItem", New XAttribute("ID", kvp.Key),
                                                                New XAttribute("Checked", kvp.Value.Checked),
                                                                New XAttribute("Text", kvp.Value.Text),
                                                                New XAttribute("Value", kvp.Value.Value),
                                                                New XAttribute("Unit", kvp.Value.Unit)))
            Next

        End With

        Return elements

    End Function
    ''' <summary>
    ''' Copies the object properties to the Clipboard.
    ''' </summary>
    ''' <param name="su">Units system to use.</param>
    ''' <param name="nf">Number format to use.</param>
    ''' <remarks></remarks>
    Public Sub CopyDataToClipboard(su As DWSIM.SistemasDeUnidades.Unidades, nf As String)

        Dim DT As New DataTable
        DT.Columns.Clear()
        DT.Columns.Add(("Propriedade"), GetType(System.String))
        DT.Columns.Add(("Valor"), GetType(System.String))
        DT.Columns.Add(("Unidade"), GetType(System.String))
        DT.Rows.Clear()

        Dim baseobj As SimulationObjects_BaseClass
        Dim properties() As String
        Dim description As String
        Dim objtype As Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto
        Dim propidx, r1, r2, r3, r4, r5, r6 As Integer
        r1 = 5
        r2 = 12
        r3 = 30
        r4 = 48
        r5 = 66
        r6 = 84

        baseobj = Me
        properties = baseobj.GetProperties(SimulationObjects_BaseClass.PropertyType.ALL)
        objtype = baseobj.GraphicObject.TipoObjeto
        description = DWSIM.App.GetLocalString(baseobj.GraphicObject.Description)
        If objtype = Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.MaterialStream Then
            Dim value As String
            For propidx = 0 To r1 - 1
                value = baseobj.GetPropertyValue(properties(propidx), su)
                If Double.TryParse(value, New Double) Then
                    DT.Rows.Add(New String() {DWSIM.App.GetPropertyName(properties(propidx)), Format(Double.Parse(value), nf), baseobj.GetPropertyUnit(properties(propidx), su)})
                Else
                    DT.Rows.Add(New String() {DWSIM.App.GetPropertyName(properties(propidx)), value, baseobj.GetPropertyUnit(properties(propidx), su)})
                End If
            Next
            For propidx = r1 To r2 - 1
                value = baseobj.GetPropertyValue(properties(propidx), su)
                If Double.TryParse(value, New Double) Then
                    DT.Rows.Add(New String() {DWSIM.App.GetPropertyName(properties(propidx)), Format(Double.Parse(value), nf), baseobj.GetPropertyUnit(properties(propidx), su)})
                Else
                    DT.Rows.Add(New String() {DWSIM.App.GetPropertyName(properties(propidx)), value, baseobj.GetPropertyUnit(properties(propidx), su)})
                End If
            Next
            DT.Rows.Add(New String() {DWSIM.App.GetLocalString("FraomolarnaMistura"), "", ""})
            For Each subst As DWSIM.ClassesBasicasTermodinamica.Substancia In CType(Me, Streams.MaterialStream).Fases(0).Componentes.Values
                DT.Rows.Add(New String() {DWSIM.App.GetComponentName(subst.Nome), Format(subst.FracaoMolar.GetValueOrDefault, nf), ""})
            Next
            For propidx = r2 To r3 - 1
                value = baseobj.GetPropertyValue(properties(propidx), su)
                If Double.TryParse(value, New Double) Then
                    DT.Rows.Add(New String() {DWSIM.App.GetPropertyName(properties(propidx)), Format(Double.Parse(value), nf), baseobj.GetPropertyUnit(properties(propidx), su)})
                Else
                    DT.Rows.Add(New String() {DWSIM.App.GetPropertyName(properties(propidx)), value, baseobj.GetPropertyUnit(properties(propidx), su)})
                End If
            Next
            DT.Rows.Add(New String() {DWSIM.App.GetLocalString("FraomolarnaFaseVapor"), "", ""})
            For Each subst As DWSIM.ClassesBasicasTermodinamica.Substancia In CType(Me, Streams.MaterialStream).Fases(2).Componentes.Values
                DT.Rows.Add(New String() {DWSIM.App.GetComponentName(subst.Nome), Format(subst.FracaoMolar.GetValueOrDefault, nf), ""})
            Next
            For propidx = r3 To r4 - 1
                value = baseobj.GetPropertyValue(properties(propidx), su)
                If Double.TryParse(value, New Double) Then
                    DT.Rows.Add(New String() {DWSIM.App.GetPropertyName(properties(propidx)), Format(Double.Parse(value), nf), baseobj.GetPropertyUnit(properties(propidx), su)})
                Else
                    DT.Rows.Add(New String() {DWSIM.App.GetPropertyName(properties(propidx)), value, baseobj.GetPropertyUnit(properties(propidx), su)})
                End If
            Next
            DT.Rows.Add(New String() {DWSIM.App.GetLocalString("FraomolarnaFaseLquid"), "", ""})
            For Each subst As DWSIM.ClassesBasicasTermodinamica.Substancia In CType(Me, Streams.MaterialStream).Fases(1).Componentes.Values
                DT.Rows.Add(New String() {DWSIM.App.GetComponentName(subst.Nome), Format(subst.FracaoMolar.GetValueOrDefault, nf), ""})
            Next
            For propidx = r4 To r5 - 1
                value = baseobj.GetPropertyValue(properties(propidx), su)
                If Double.TryParse(value, New Double) Then
                    DT.Rows.Add(New String() {DWSIM.App.GetPropertyName(properties(propidx)), Format(Double.Parse(value), nf), baseobj.GetPropertyUnit(properties(propidx), su)})
                Else
                    DT.Rows.Add(New String() {DWSIM.App.GetPropertyName(properties(propidx)), value, baseobj.GetPropertyUnit(properties(propidx), su)})
                End If
            Next
            DT.Rows.Add(New String() {DWSIM.App.GetLocalString("FraomolarnaFaseLquid"), "", ""})
            For Each subst As DWSIM.ClassesBasicasTermodinamica.Substancia In CType(Me, Streams.MaterialStream).Fases(3).Componentes.Values
                DT.Rows.Add(New String() {DWSIM.App.GetComponentName(subst.Nome), Format(subst.FracaoMolar.GetValueOrDefault, nf), ""})
            Next
            For propidx = r5 To r6 - 1
                value = baseobj.GetPropertyValue(properties(propidx), su)
                If Double.TryParse(value, New Double) Then
                    DT.Rows.Add(New String() {DWSIM.App.GetPropertyName(properties(propidx)), Format(Double.Parse(value), nf), baseobj.GetPropertyUnit(properties(propidx), su)})
                Else
                    DT.Rows.Add(New String() {DWSIM.App.GetPropertyName(properties(propidx)), value, baseobj.GetPropertyUnit(properties(propidx), su)})
                End If
            Next
            DT.Rows.Add(New String() {DWSIM.App.GetLocalString("FraomolarnaFaseLquid"), "", ""})
            For Each subst As DWSIM.ClassesBasicasTermodinamica.Substancia In CType(Me, Streams.MaterialStream).Fases(4).Componentes.Values
                DT.Rows.Add(New String() {DWSIM.App.GetComponentName(subst.Nome), Format(subst.FracaoMolar.GetValueOrDefault, nf), ""})
            Next
            For propidx = r6 To 101
                value = baseobj.GetPropertyValue(properties(propidx), su)
                If Double.TryParse(value, New Double) Then
                    DT.Rows.Add(New String() {DWSIM.App.GetPropertyName(properties(propidx)), Format(Double.Parse(value), nf), baseobj.GetPropertyUnit(properties(propidx), su)})
                Else
                    DT.Rows.Add(New String() {DWSIM.App.GetPropertyName(properties(propidx)), value, baseobj.GetPropertyUnit(properties(propidx), su)})
                End If
            Next
            DT.Rows.Add(New String() {DWSIM.App.GetLocalString("FraomolarnaFaseLquid"), "", ""})
            For Each subst As DWSIM.ClassesBasicasTermodinamica.Substancia In CType(Me, Streams.MaterialStream).Fases(6).Componentes.Values
                DT.Rows.Add(New String() {DWSIM.App.GetComponentName(subst.Nome), Format(subst.FracaoMolar.GetValueOrDefault, nf), ""})
            Next
        Else
            For Each prop As String In properties
                DT.Rows.Add(New String() {DWSIM.App.GetPropertyName(prop), Format(baseobj.GetPropertyValue(prop, su), nf), baseobj.GetPropertyUnit(prop, su)})
            Next
        End If

        Dim st As New StringBuilder(DWSIM.App.GetLocalString(Me.Descricao) & ": " & Me.GraphicObject.Tag & vbCrLf)
        For Each r As DataRow In DT.Rows
            Dim l As String = ""
            For Each o As Object In r.ItemArray
                l += o.ToString() & vbTab
            Next
            st.AppendLine(l)
        Next

        Clipboard.SetText(st.ToString())

        DT.Clear()
        DT.Dispose()
        DT = Nothing

    End Sub


End Class

<System.Serializable()> <ComVisible(True)> Public MustInherit Class SimulationObjects_UnitOpBaseClass

    Inherits SimulationObjects_BaseClass

    'CAPE-OPEN Unit Operation Support
    Implements ICapeIdentification, ICapeUnit, ICapeUtilities, ICapeUnitReport

    'CAPE-OPEN Persistence Interface
    Implements IPersistStreamInit

    'CAPE-OPEN Error Interfaces
    Implements ECapeUser, ECapeUnknown, ECapeRoot

    Public ObjectType As Integer
    Private _pp As DWSIM.SimulationObjects.PropertyPackages.PropertyPackage
    Private _ppid As String = ""

    Friend _capeopenmode As Boolean = False

    Private _scripttextEB As String = ""
    Private _scripttextEA As String = ""
    Private _scriptlanguageE As scriptlanguage = scriptlanguage.IronPython
    Private _includesE() As String
    Private _fontnameE As String = "Courier New"
    Private _fontsizeE As Integer = 10
  
    <System.NonSerialized()> Public scope As Microsoft.Scripting.Hosting.ScriptScope
    <System.NonSerialized()> Public engine As Microsoft.Scripting.Hosting.ScriptEngine

    Public Sub New()
        MyBase.CreateNew()
    End Sub

    Public Overrides Function LoadData(data As System.Collections.Generic.List(Of System.Xml.Linq.XElement)) As Boolean

        MyBase.LoadData(data)

        Try
            Me._ppid = (From xel As XElement In data Select xel Where xel.Name = "PropertyPackage").SingleOrDefault.Value
        Catch

        End Try

        Return True

    End Function

    Public Overrides Function SaveData() As System.Collections.Generic.List(Of System.Xml.Linq.XElement)

        Dim elements As List(Of XElement) = MyBase.SaveData()

        Dim ppid As String = ""
        If _ppid <> "" Then
            ppid = _ppid
        ElseIf Not _pp Is Nothing Then
            ppid = _pp.Name
        Else
            ppid = ""
        End If
        elements.Add(New XElement("PropertyPackage", ppid))

        Return elements

    End Function

#Region "   DWSIM Specific"

    Public Enum scriptlanguage
        IronPython = 2
        Lua = 4
    End Enum

    Public Property ScriptExt_FontName() As String
        Get
            Return _fontnameE
        End Get
        Set(ByVal value As String)
            _fontnameE = value
        End Set
    End Property

    Public Property ScriptExt_FontSize() As Integer
        Get
            Return _fontsizeE
        End Get
        Set(ByVal value As Integer)
            _fontsizeE = value
        End Set
    End Property

    Public Property ScriptExt_Includes() As String()
        Get
            Return _includesE
        End Get
        Set(ByVal value As String())
            _includesE = value
        End Set
    End Property

    Public Property ScriptExt_ScriptTextB() As String
        Get
            If _scripttextEB IsNot Nothing Then Return _scripttextEB Else Return ""
        End Get
        Set(ByVal value As String)
            _scripttextEB = value
        End Set
    End Property

    Public Property ScriptExt_ScriptTextA() As String
        Get
            If _scripttextEA IsNot Nothing Then Return _scripttextEA Else Return ""
        End Get
        Set(ByVal value As String)
            _scripttextEA = value
        End Set
    End Property

    Public Property ScriptExt_Language() As scriptlanguage
        Get
            Return _scriptlanguageE
        End Get
        Set(ByVal value As scriptlanguage)
            _scriptlanguageE = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the property package associated with this object.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Xml.Serialization.XmlIgnore()> Property PropertyPackage() As PropertyPackage
        Get
            If Not _pp Is Nothing Then Return _pp
            If _ppid Is Nothing Then _ppid = ""
            If FlowSheet.Options.PropertyPackages.ContainsKey(_ppid) Then
                Return FlowSheet.Options.PropertyPackages(_ppid)
            Else
                For Each pp As DWSIM.SimulationObjects.PropertyPackages.PropertyPackage In Me.FlowSheet.Options.PropertyPackages.Values
                    _ppid = pp.UniqueID
                    Return pp
                    Exit For
                Next
            End If
            Return Nothing
        End Get
        Set(ByVal value As PropertyPackage)
            If value IsNot Nothing Then
                _ppid = value.UniqueID
                _pp = value
            Else
                _pp = Nothing
            End If
        End Set
    End Property

    ''' <summary>
    ''' Calculates the object.
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    ''' <remarks>Use 'Solve()' to calculate the object instead.</remarks>
    Public Overridable Function Calculate(Optional ByVal args As Object = Nothing) As Integer
        Return Nothing
    End Function

    ''' <summary>
    ''' Calculates the object.
    ''' </summary>
    ''' <param name="args"></param>
    ''' <remarks></remarks>
    Public Sub Solve(Optional ByVal args As Object = Nothing)

        Calculated = False

        Calculate(args)

        Calculated = True
        LastUpdated = Date.Now

    End Sub

    ''' <summary>
    ''' Decalculates the object.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable Function DeCalculate() As Integer
        Return Nothing
    End Function

    ''' <summary>
    ''' Decalculates the object.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Unsolve()

        DeCalculate()

        Calculated = False

    End Sub

    ''' <summary>
    ''' Validates the object, checking its connections and other parameters.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overridable Sub Validate()

        Dim vForm As Global.DWSIM.FormFlowsheet = FlowSheet
        Dim vEventArgs As New DWSIM.Outros.StatusChangeEventArgs
        Dim vCon As ConnectionPoint

        With vEventArgs
            .Calculado = False
            .Nome = Me.Nome
            .Tipo = Me.ObjectType
        End With

        'Validate input connections.
        For Each vCon In Me.GraphicObject.InputConnectors
            If Not vCon.IsAttached Then
                CalculateFlowsheet(FlowSheet, vEventArgs, Nothing)
                Throw New Exception(DWSIM.App.GetLocalString("Verifiqueasconexesdo"))
            End If
        Next

        'Validate output connections.
        For Each vCon In Me.GraphicObject.OutputConnectors
            If Not vCon.IsAttached Then
                CalculateFlowsheet(vForm, vEventArgs, Nothing)
                Throw New Exception(DWSIM.App.GetLocalString("Verifiqueasconexesdo"))
            End If
        Next

    End Sub

    ''' <summary>
    ''' Populates the Property Grid with properties from this object.
    ''' </summary>
    ''' <param name="pgrid"></param>
    ''' <param name="su"></param>
    ''' <remarks></remarks>
    Public Overrides Sub PopulatePropertyGrid(ByRef pgrid As PropertyGridEx.PropertyGridEx, ByVal su As DWSIM.SistemasDeUnidades.Unidades)
        With pgrid
            '.Item.Add(DWSIM.App.GetLocalString("UO_ScriptLanguage"), Me, "ScriptExt_Language", False, DWSIM.App.GetLocalString("UO_ScriptExtension"), "", True)
            '.Item.Add(DWSIM.App.GetLocalString("UO_ScriptText_Before"), Me, "ScriptExt_ScriptTextB", False, DWSIM.App.GetLocalString("UO_ScriptExtension"), DWSIM.App.GetLocalString("Cliquenobotocomretic"), True)
            'With .Item(.Item.Count - 1)
            '    .CustomEditor = New DWSIM.Editors.CustomUO.UIScriptEditor
            '    .Tag = "B"
            'End With
            '.Item.Add(DWSIM.App.GetLocalString("UO_ScriptText_After"), Me, "ScriptExt_ScriptTextA", False, DWSIM.App.GetLocalString("UO_ScriptExtension"), DWSIM.App.GetLocalString("Cliquenobotocomretic"), True)
            'With .Item(.Item.Count - 1)
            '    .CustomEditor = New DWSIM.Editors.CustomUO.UIScriptEditor
            '    .Tag = "A"
            'End With
            .Item.Add(DWSIM.App.GetLocalString("UOPropertyPackage"), Me.PropertyPackage.Tag, False, DWSIM.App.GetLocalString("UOPropertyPackage0"), "", True)
            With .Item(.Item.Count - 1)
                .CustomEditor = New DWSIM.Editors.PropertyPackages.UIPPSelector
            End With
            If Not Me.GraphicObject Is Nothing Then
                .Item.Add(DWSIM.App.GetLocalString("Ativo"), Me.GraphicObject, "Active", False, DWSIM.App.GetLocalString("Miscelnea4"), "", True)
            End If
            If Me.IsSpecAttached = True Then
                .Item.Add(DWSIM.App.GetLocalString("ObjetoUtilizadopor"), FlowSheet.Collections.ObjectCollection(Me.AttachedSpecId).GraphicObject.Tag, True, DWSIM.App.GetLocalString("Miscelnea4"), "", True)
                Select Case Me.SpecVarType
                    Case SpecialOps.Helpers.Spec.TipoVar.Destino
                        .Item.Add(DWSIM.App.GetLocalString("Utilizadocomo"), DWSIM.App.GetLocalString(Me.SpecVarType.ToString), True, DWSIM.App.GetLocalString("Miscelnea4"), "", True)
                    Case SpecialOps.Helpers.Spec.TipoVar.Fonte
                        .Item.Add(DWSIM.App.GetLocalString("Utilizadocomo"), DWSIM.App.GetLocalString("SpecSource"), True, DWSIM.App.GetLocalString("Miscelnea4"), "", True)
                End Select
            End If
            If Not Me.Annotation Is Nothing Then
                .Item.Add(DWSIM.App.GetLocalString("Anotaes"), Me, "Annotation", False, DWSIM.App.GetLocalString("Outros"), DWSIM.App.GetLocalString("Cliquenobotocomretic"), True)
                With .Item(.Item.Count - 1)
                    .IsBrowsable = False
                    .CustomEditor = New DWSIM.Editors.Annotation.UIAnnotationEditor
                End With
            End If
            .Item.Add("ID", Me.Nome, True, DWSIM.App.GetLocalString("Outros"), "", True)
            .Item.Add(DWSIM.App.GetLocalString("LastUpdatedOn"), Me.LastUpdated.ToString("O"), True, DWSIM.App.GetLocalString("Outros"), "", True)
        End With
    End Sub

    Public Overrides Sub PropertyValueChanged(ByVal s As Object, ByVal e As System.Windows.Forms.PropertyValueChangedEventArgs)

        MyBase.PropertyValueChanged(s, e)

        If e.ChangedItem.Label.Contains(DWSIM.App.GetLocalString("UOPropertyPackage")) Then
            If e.ChangedItem.Value <> "" Then
                If FlowSheet.Options.PropertyPackages.ContainsKey(e.ChangedItem.Value) Then
                    Me.PropertyPackage = FlowSheet.Options.PropertyPackages(e.ChangedItem.Value)
                End If
            End If
        End If

    End Sub

    Sub CreateCOPorts()
        _ports = New CapeOpen.PortCollection()
        For Each c As ConnectionPoint In Me.GraphicObject.InputConnectors
            Select Case c.Type
                Case ConType.ConIn
                    _ports.Add(New UnitPort("Inlet" + Me.GraphicObject.InputConnectors.IndexOf(c).ToString, "", CapePortDirection.CAPE_INLET, CapePortType.CAPE_MATERIAL))
                    With _ports(_ports.Count - 1)
                        If c.IsAttached Then
                            .Connect(Me.FlowSheet.GetFlowsheetSimulationObject(c.AttachedConnector.AttachedFrom.Tag))
                        End If
                    End With
                Case ConType.ConEn
                    _ports.Add(New UnitPort("Inlet" + Me.GraphicObject.InputConnectors.IndexOf(c).ToString, "", CapePortDirection.CAPE_INLET, CapePortType.CAPE_ENERGY))
                    With _ports(_ports.Count - 1)
                        If c.IsAttached Then
                            .Connect(Me.FlowSheet.GetFlowsheetSimulationObject(c.AttachedConnector.AttachedFrom.Tag))
                        End If
                    End With
            End Select
        Next
        For Each c As ConnectionPoint In Me.GraphicObject.OutputConnectors
            Select Case c.Type
                Case ConType.ConOut
                    _ports.Add(New UnitPort("Outlet" + Me.GraphicObject.OutputConnectors.IndexOf(c).ToString, "", CapePortDirection.CAPE_OUTLET, CapePortType.CAPE_MATERIAL))
                    With _ports(_ports.Count - 1)
                        If c.IsAttached Then
                            .Connect(Me.FlowSheet.GetFlowsheetSimulationObject(c.AttachedConnector.AttachedTo.Tag))
                        End If
                    End With
                Case ConType.ConEn
                    _ports.Add(New UnitPort("Outlet" + Me.GraphicObject.OutputConnectors.IndexOf(c).ToString, "", CapePortDirection.CAPE_OUTLET, CapePortType.CAPE_ENERGY))
                    With _ports(_ports.Count - 1)
                        If c.IsAttached Then
                            .Connect(Me.FlowSheet.GetFlowsheetSimulationObject(c.AttachedConnector.AttachedTo.Tag))
                        End If
                    End With
            End Select
        Next
    End Sub

    Sub CreateCOParameters()
        _parameters = New CapeOpen.ParameterCollection
        Dim props() = Me.GetProperties(PropertyType.ALL)
        For Each s As String In props
            _parameters.Add(New CapeOpen.RealParameter(s, Me.GetPropertyValue(s), 0.0#, Me.GetPropertyUnit(s)))
            With _parameters.Item(_parameters.Count - 1)
                .Mode = CapeParamMode.CAPE_OUTPUT
            End With
        Next
    End Sub

#End Region

#Region "   CAPE-OPEN Unit Operation internal support"

    Friend _ports As CapeOpen.PortCollection
    Friend _parameters As CapeOpen.ParameterCollection
    Friend _simcontext As Object = Nothing

    ''' <summary>
    ''' Gets the description of the component.
    ''' </summary>
    ''' <value></value>
    ''' <returns>CapeString</returns>
    ''' <remarks>Implements CapeOpen.ICapeIdentification.ComponentName</remarks>
    Public Property ComponentDescription() As String Implements CapeOpen.ICapeIdentification.ComponentDescription
        Get
            Return Me.m_ComponentDescription
        End Get
        Set(ByVal value As String)
            Me.m_ComponentDescription = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the name of the component.
    ''' </summary>
    ''' <value></value>
    ''' <returns>CapeString</returns>
    ''' <remarks>Implements CapeOpen.ICapeIdentification.ComponentDescription</remarks>
    Public Property ComponentName() As String Implements CapeOpen.ICapeIdentification.ComponentName
        Get
            Return Me.m_ComponentName
        End Get
        Set(ByVal value As String)
            Me.m_ComponentName = value
        End Set
    End Property

    ''' <summary>
    ''' Calculates the Unit Operation.
    ''' </summary>
    ''' <remarks>The Flowsheet Unit performs its calculation, that is, computes the variables that are missing at
    ''' this stage in the complete description of the input and output streams and computes any public
    ''' parameter value that needs to be displayed. Calculate will be able to do progress monitoring
    ''' and checks for interrupts as required using the simulation context. At present, there are no
    ''' standards agreed for this.
    ''' It is recommended that Flowsheet Units perform a suitable flash calculation on all output
    ''' streams. In some cases a Simulation Executive will be able to perform a flash calculation but
    ''' the writer of a Flowsheet Unit is in the best position to decide the correct flash to use.
    ''' Before performing the calculation, this method should perform any final validation tests that
    ''' are required. For example, at this point the validity of Material Objects connected to ports can
    ''' be checked.
    ''' There are no input or output arguments for this method.</remarks>
    Public Overridable Sub Calculate1() Implements CapeOpen.ICapeUnit.Calculate
        'do CAPE calculation here
    End Sub

    ''' <summary>
    ''' Return an interface to a collection containing the list of unit ports (e.g. ICapeUnitCollection).
    ''' </summary>
    ''' <value></value>
    ''' <returns>A reference to the interface on the collection containing the specified ports</returns>
    ''' <remarks>Return the collection of unit ports (i.e. ICapeUnitCollection). These are delivered as a
    ''' collection of elements exposing the interfaces ICapeUnitPort</remarks>
    <Xml.Serialization.XmlIgnore()> Public ReadOnly Property ports() As Object Implements CapeOpen.ICapeUnit.ports
        Get
            If Not Me._capeopenmode Then
                If Not Me.GraphicObject.TipoObjeto = TipoObjeto.CapeOpenUO Then
                    _ports = New CapeOpen.PortCollection
                    For Each c As ConnectionPoint In Me.GraphicObject.InputConnectors
                        If c.Type = ConType.ConIn Then
                            _ports.Add(New UnitPort(c.ConnectorName, "", CapePortDirection.CAPE_INLET, CapePortType.CAPE_MATERIAL))
                        ElseIf c.Type = ConType.ConEn Then
                            _ports.Add(New UnitPort(c.ConnectorName, "", CapePortDirection.CAPE_INLET, CapePortType.CAPE_ENERGY))
                        End If
                        With _ports(_ports.Count - 1)
                            If c.IsAttached And Not c.AttachedConnector Is Nothing Then .Connect(Me.FlowSheet.Collections.ObjectCollection(c.AttachedConnector.AttachedFrom.Name))
                        End With
                    Next
                    For Each c As ConnectionPoint In Me.GraphicObject.OutputConnectors
                        If c.Type = ConType.ConOut Then
                            _ports.Add(New UnitPort(c.ConnectorName, "", CapePortDirection.CAPE_OUTLET, CapePortType.CAPE_MATERIAL))
                        ElseIf c.Type = ConType.ConEn Then
                            _ports.Add(New UnitPort(c.ConnectorName, "", CapePortDirection.CAPE_OUTLET, CapePortType.CAPE_ENERGY))
                        End If
                        With _ports(_ports.Count - 1)
                            If c.IsAttached And Not c.AttachedConnector Is Nothing Then .Connect(Me.FlowSheet.Collections.ObjectCollection(c.AttachedConnector.AttachedTo.Name))
                        End With
                    Next
                End If
            End If
            Return _ports
        End Get
    End Property

    ''' <summary>Validates the Unit Operation.</summary>
    ''' <param name="message">An optional message describing the cause of the validation failure.</param>
    ''' <returns>TRUE if the Unit is valid</returns>
    ''' <remarks>Sets the flag that indicates whether the Flowsheet Unit is valid by validating the ports and
    ''' parameters of the Flowsheet Unit. For example, this method could check that all mandatory
    ''' ports have connections and that the values of all parameters are within bounds.
    ''' Note that the Simulation Executive can call the Validate routine at any time, in particular it
    ''' may be called before the executive is ready to call the Calculate method. This means that
    ''' Material Objects connected to unit ports may not be correctly configured when Validate is
    ''' called. The recommended approach is for this method to validate parameters and ports but not
    ''' Material Object configuration. A second level of validation to check Material Objects can be
    ''' implemented as part of Calculate, when it is reasonable to expect that the Material Objects
    ''' connected to ports will be correctly configured.</remarks>
    Public Overridable Function Validate1(ByRef message As String) As Boolean Implements CapeOpen.ICapeUnit.Validate
        'do CAPE validation here
        message = "Validation OK"
        Return True
    End Function

    ''' <summary>
    ''' Get the flag that indicates whether the Flowsheet Unit is valid (e.g. some parameter values
    ''' have changed but they have not been validated by using Validate). It has three possible
    ''' values:
    ''' ·  notValidated(CAPE_NOT_VALIDATED): the unit’s validate() method has not been
    ''' called since the last operation that could have changed the validation status of the unit, for
    ''' example an update to a parameter value of a connection to a port.
    ''' ·  invalid(CAPE_INVALID): the last time the unit’s validate() method was called it returned
    ''' false.
    ''' ·  valid(CAPE_VALID): the last time the unit’s validate() method was called it returned true.
    ''' </summary>
    ''' <value>CAPE_VALID meaning the Validate method returned success; CAPE_INVALID meaing the Validate 
    ''' method returned failure; CAPE_NOT_VALIDATED meaning that the Validate method needs to be called 
    ''' to determine whether the unit is valid or not.</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overridable ReadOnly Property ValStatus() As CapeOpen.CapeValidationStatus Implements CapeOpen.ICapeUnit.ValStatus
        Get
            Return CapeValidationStatus.CAPE_VALID
        End Get
    End Property

    ''' <summary>
    ''' The PMC displays its user interface and allows the Flowsheet User to interact with it. If no user interface is
    ''' available it returns an error.</summary>
    ''' <remarks></remarks>
    Public Overridable Sub Edit1() Implements CapeOpen.ICapeUtilities.Edit
        Throw New CapeNoImplException("ICapeUtilities.Edit() Method not implemented.")
    End Sub

    ''' <summary>
    ''' Initially, this method was only present in the ICapeUnit interface. Since ICapeUtilities.Initialize is now
    ''' available for any kind of PMC, ICapeUnit. Initialize is deprecated.
    ''' The PME will order the PMC to get initialized through this method. Any initialisation that could fail must be
    ''' placed here. Initialize is guaranteed to be the first method called by the client (except low level methods such
    ''' as class constructors or initialization persistence methods). Initialize has to be called once when the PMC is
    ''' instantiated in a particular flowsheet.
    ''' When the initialization fails, before signalling an error, the PMC must free all the resources that were
    ''' allocated before the failure occurred. When the PME receives this error, it may not use the PMC anymore.
    ''' The method terminate of the current interface must not either be called. Hence, the PME may only release
    ''' the PMC through the middleware native mechanisms.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overridable Sub Initialize() Implements CapeOpen.ICapeUtilities.Initialize
        'do CAPE Initialization here
        CreateCOPorts()
        CreateCOParameters()
    End Sub

    ''' <summary>
    ''' Returns an ICapeCollection interface.
    ''' </summary>
    ''' <value></value>
    ''' <returns>CapeInterface (ICapeCollection)</returns>
    ''' <remarks>This interface will contain a collection of ICapeParameter interfaces.
    ''' This method allows any client to access all the CO Parameters exposed by a PMC. Initially, this method was
    ''' only present in the ICapeUnit interface. Since ICapeUtilities.GetParameters is now available for any kind of
    ''' PMC, ICapeUnit.GetParameters is deprecated. Consult the “Open Interface Specification: Parameter
    ''' Common Interface” document for more information about parameter. Consult the “Open Interface
    ''' Specification: Collection Common Interface” document for more information about collection.
    ''' If the PMC does not support exposing its parameters, it should raise the ECapeNoImpl error, instead of
    ''' returning a NULL reference or an empty Collection. But if the PMC supports parameters but has for this call
    ''' no parameters, it should return a valid ICapeCollection reference exposing zero parameters.</remarks>
    <Xml.Serialization.XmlIgnore()> Public ReadOnly Property parameters() As Object Implements CapeOpen.ICapeUtilities.parameters
        Get
            Return _parameters
        End Get
    End Property

    ''' <summary>
    ''' Allows the PME to convey the PMC a reference to the former’s simulation context. 
    ''' </summary>
    ''' <value>The reference to the PME’s simulation context class. For the PMC to
    ''' use this class, this reference will have to be converted to each of the
    ''' defined CO Simulation Context interfaces.</value>
    ''' <remarks>The simulation context
    ''' will be PME objects which will expose a given set of CO interfaces. Each of these interfaces will allow the
    ''' PMC to call back the PME in order to benefit from its exposed services (such as creation of material
    ''' templates, diagnostics or measurement unit conversion). If the PMC does not support accessing the
    ''' simulation context, it is recommended to raise the ECapeNoImpl error.
    ''' Initially, this method was only present in the ICapeUnit interface. Since ICapeUtilities.SetSimulationContext
    ''' is now available for any kind of PMC, ICapeUnit. SetSimulationContext is deprecated.</remarks>
    <Xml.Serialization.XmlIgnore()> Public WriteOnly Property simulationContext() As Object Implements CapeOpen.ICapeUtilities.simulationContext
        Set(ByVal value As Object)
            _simcontext = value
        End Set
    End Property

    ''' <summary>
    ''' Initially, this method was only present in the ICapeUnit interface. Since ICapeUtilities.Terminate is now
    ''' available for any kind of PMC, ICapeUnit.Terminate is deprecated.
    ''' The PME will order the PMC to get destroyed through this method. Any uninitialization that could fail must
    ''' be placed here. ‘Terminate’ is guaranteed to be the last method called by the client (except low level methods
    ''' such as class destructors). ‘Terminate’ may be called at any time, but may be only called once.
    ''' When this method returns an error, the PME should report the user. However, after that the PME is not
    ''' allowed to use the PMC anymore.
    ''' The Unit specification stated that “Terminate may check if the data has been saved and return an error if
    ''' not.” It is suggested not to follow this recommendation, since it’s the PME responsibility to save the state of
    ''' the PMC before terminating it. In the case that a user wants to close a simulation case without saving it, it’s
    ''' better to leave the PME to handle the situation instead of each PMC providing a different implementation.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overridable Sub Terminate1() Implements CapeOpen.ICapeUtilities.Terminate
        If Not _simcontext Is Nothing Then
            If IsComObject(_simcontext) Then
                ReleaseComObject(_simcontext)
            Else
                _simcontext = Nothing
            End If
        End If
        Me.Dispose()
    End Sub

#End Region

#Region "   CAPE-OPEN Persistence Implementation"

    Friend m_dirty As Boolean = True

    Public Sub GetClassID(ByRef pClassID As System.Guid) Implements IPersistStreamInit.GetClassID
        pClassID = New Guid(SimulationObjects_UnitOpBaseClass.ClassId)
    End Sub

    Public Sub GetSizeMax(ByRef pcbSize As Long) Implements IPersistStreamInit.GetSizeMax
        pcbSize = 1024 * 1024
    End Sub

    Public Sub InitNew() Implements IPersistStreamInit.InitNew
        'do nothing
    End Sub

    Public Function IsDirty() As Integer Implements IPersistStreamInit.IsDirty
        Return m_dirty
    End Function

    Public Overridable Sub Load(ByVal pStm As System.Runtime.InteropServices.ComTypes.IStream) Implements IPersistStreamInit.Load

        ' Read the length of the string  
        Dim arrLen As Byte() = New [Byte](3) {}
        pStm.Read(arrLen, arrLen.Length, IntPtr.Zero)

        ' Calculate the length  
        Dim cb As Integer = BitConverter.ToInt32(arrLen, 0)

        ' Read the stream to get the string    
        Dim bytes As Byte() = New Byte(cb - 1) {}
        Dim pcb As New IntPtr()
        pStm.Read(bytes, bytes.Length, pcb)
        If System.Runtime.InteropServices.Marshal.IsComObject(pStm) Then System.Runtime.InteropServices.Marshal.ReleaseComObject(pStm)

        ' Deserialize byte array    

        Dim memoryStream As New System.IO.MemoryStream(bytes)

        Try

            Dim domain As AppDomain = AppDomain.CurrentDomain
            AddHandler domain.AssemblyResolve, New ResolveEventHandler(AddressOf MyResolveEventHandler)

            Dim myarr As ArrayList

            Dim mySerializer As Binary.BinaryFormatter = New Binary.BinaryFormatter(Nothing, New System.Runtime.Serialization.StreamingContext())
            myarr = mySerializer.Deserialize(memoryStream)

            Me._parameters = myarr(0)
            Me._ports = myarr(1)

            myarr = Nothing
            mySerializer = Nothing

            RemoveHandler domain.AssemblyResolve, New ResolveEventHandler(AddressOf MyResolveEventHandler)

        Catch p_Ex As System.Exception

            System.Windows.Forms.MessageBox.Show(p_Ex.ToString())

        End Try

        memoryStream.Close()

    End Sub

    Public Overridable Sub Save(ByVal pStm As System.Runtime.InteropServices.ComTypes.IStream, ByVal fClearDirty As Boolean) Implements IPersistStreamInit.Save

        Dim props As New ArrayList

        With props

            .Add(_parameters)
            .Add(_ports)

        End With

        Dim mySerializer As Binary.BinaryFormatter = New Binary.BinaryFormatter(Nothing, New System.Runtime.Serialization.StreamingContext())
        Dim mstr As New MemoryStream
        mySerializer.Serialize(mstr, props)
        Dim bytes As Byte() = mstr.ToArray()
        mstr.Close()

        ' construct length (separate into two separate bytes)    

        Dim arrLen As Byte() = BitConverter.GetBytes(bytes.Length)
        Try

            ' Save the array in the stream    
            pStm.Write(arrLen, arrLen.Length, IntPtr.Zero)
            pStm.Write(bytes, bytes.Length, IntPtr.Zero)
            If System.Runtime.InteropServices.Marshal.IsComObject(pStm) Then System.Runtime.InteropServices.Marshal.ReleaseComObject(pStm)

        Catch p_Ex As System.Exception

            System.Windows.Forms.MessageBox.Show(p_Ex.ToString())

        End Try

        If fClearDirty Then
            m_dirty = False
        End If

    End Sub

    Friend Function MyResolveEventHandler(ByVal sender As Object, ByVal args As ResolveEventArgs) As System.Reflection.Assembly
        Return Me.[GetType]().Assembly
    End Function

#End Region

#Region "   CAPE-OPEN Error Interfaces"

    Sub ThrowCAPEException(ByRef ex As Exception, ByVal name As String, ByVal description As String, ByVal interf As String, ByVal moreinfo As String, ByVal operation As String, ByVal scope As String, ByVal code As Integer)

        _code = code
        _description = description
        _interfacename = interf
        _moreinfo = moreinfo
        _operation = operation
        _scope = scope

        Throw ex

    End Sub

    Private _name, _description, _interfacename, _moreinfo, _operation, _scope As String, _code As Integer

    Public ReadOnly Property Name() As String Implements CapeOpen.ECapeRoot.Name
        Get
            Return Me.Nome
        End Get
    End Property

    Public ReadOnly Property code() As Integer Implements CapeOpen.ECapeUser.code
        Get
            Return _code
        End Get
    End Property

    Public ReadOnly Property description() As String Implements CapeOpen.ECapeUser.description
        Get
            Return _description
        End Get
    End Property

    Public ReadOnly Property interfaceName() As String Implements CapeOpen.ECapeUser.interfaceName
        Get
            Return _interfacename
        End Get
    End Property

    Public ReadOnly Property moreInfo() As String Implements CapeOpen.ECapeUser.moreInfo
        Get
            Return _moreinfo
        End Get
    End Property

    Public ReadOnly Property operation() As String Implements CapeOpen.ECapeUser.operation
        Get
            Return _operation
        End Get
    End Property

    Public ReadOnly Property scope1() As String Implements CapeOpen.ECapeUser.scope
        Get
            Return _scope
        End Get
    End Property

#End Region

#Region "   CAPE-OPEN Reports"

    Friend _reports As String() = New String() {"log", "last run", "validation results"}
    Friend _selreport As String = ""
    Friend _calclog As String = ""
    Friend _lastrun As String = ""
    Friend _valres As String = ""

    Public Sub ProduceReport(ByRef message As String) Implements CapeOpen.ICapeUnitReport.ProduceReport
        Select Case _selreport
            Case "log"
                message = _calclog
            Case "last run"
                message = _lastrun
            Case "validation results"
                message = _valres
        End Select
    End Sub

    Public ReadOnly Property reports() As Object Implements CapeOpen.ICapeUnitReport.reports
        Get
            Return _reports
        End Get
    End Property

    Public Property selectedReport() As String Implements CapeOpen.ICapeUnitReport.selectedReport
        Get
            Return _selreport
        End Get
        Set(ByVal value As String)
            _selreport = value
        End Set
    End Property

#End Region

End Class

<System.Serializable()> Public MustInherit Class SimulationObjects_SpecialOpBaseClass

    Inherits SimulationObjects_BaseClass

    Public Overridable Function Calculate() As Integer
        Return Nothing
    End Function

    Public Overridable Function DeCalculate() As Integer
        Return Nothing
    End Function

    Public Sub New()
        MyBase.CreateNew()
    End Sub

End Class
