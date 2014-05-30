'    Shortcut Column Calculation Routines 
'    Copyright 2008-2013 Daniel Wagner O. de Medeiros
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

Imports System.Collections.Generic
Imports Microsoft.MSDN.Samples.GraphicObjects
Imports DWSIM.DWSIM.SimulationObjects
Imports DWSIM.DWSIM.MathEx
Imports System.Math
Imports DWSIM.DWSIM.Flowsheet.FlowSheetSolver

Namespace DWSIM.SimulationObjects.UnitOps

    <System.Serializable()> Public Class ShortcutColumn

        Inherits SimulationObjects_UnitOpBaseClass

        Public Enum CondenserType
            TotalCond = 0
            PartialCond = 1
        End Enum

        Public m_lightkey As String = ""
        Public m_heavykey As String = ""
        Public m_lightkeymolarfrac As Double = 0.001
        Public m_heavykeymolarfrac As Double = 0.001
        Public m_refluxratio As Double = 1.5
        Public m_boilerpressure As Double = 0
        Public m_condenserpressure As Double = 0

        Public m_N, m_Nmin, m_Rmin, m_Tc, m_Tb, m_Qc, m_Qb, L, V, L_, V_, ofs As Double

        Public condtype As CondenserType = CondenserType.TotalCond

        Public Sub New()
            MyBase.New()
        End Sub

        Public Sub New(ByVal nome As String, ByVal descricao As String)

            MyBase.CreateNew()
            m_ComponentName = nome
            m_ComponentDescription = descricao
            FillNodeItems()
            QTFillNodeItems()
            'Define the unitop type for later use.
            ObjectType = TipoObjeto.ShortcutColumn

        End Sub

        Public Overrides Function LoadData(data As System.Collections.Generic.List(Of System.Xml.Linq.XElement)) As Boolean
            MyBase.LoadData(data)
            XMLSerializer.XMLSerializer.Deserialize(Me, data, True)
            Return True
        End Function

        Public Overrides Function SaveData() As System.Collections.Generic.List(Of System.Xml.Linq.XElement)
            Dim elements As System.Collections.Generic.List(Of System.Xml.Linq.XElement) = MyBase.SaveData()
            elements.AddRange(XMLSerializer.XMLSerializer.Serialize(Me, True))
            Return elements
        End Function

        Public Overrides Sub Validate()

            Dim objargs As New DWSIM.Outros.StatusChangeEventArgs

            If Not Me.GraphicObject.InputConnectors(0).IsAttached Then
                'Call function to calculate flowsheet
                With objargs
                    .Calculado = False
                    .Nome = Me.Nome
                    .Tipo = TipoObjeto.Vessel
                End With
                CalculateFlowsheet(FlowSheet, objargs, Nothing)
                Throw New Exception(DWSIM.App.GetLocalString("Nohcorrentedematriac10"))
            ElseIf Not Me.GraphicObject.OutputConnectors(0).IsAttached Then
                'Call function to calculate flowsheet
                With objargs
                    .Calculado = False
                    .Nome = Me.Nome
                    .Tipo = TipoObjeto.Vessel
                End With
                CalculateFlowsheet(FlowSheet, objargs, Nothing)
                Throw New Exception(DWSIM.App.GetLocalString("Nohcorrentedematriac11"))
            ElseIf Not Me.GraphicObject.OutputConnectors(1).IsAttached Then
                'Call function to calculate flowsheet
                With objargs
                    .Calculado = False
                    .Nome = Me.Nome
                    .Tipo = TipoObjeto.Vessel
                End With
                CalculateFlowsheet(FlowSheet, objargs, Nothing)
                Throw New Exception(DWSIM.App.GetLocalString("Nohcorrentedematriac11"))
            ElseIf Not Me.GraphicObject.EnergyConnector.IsAttached Then
                'Call function to calculate flowsheet
                With objargs
                    .Calculado = False
                    .Nome = Me.Nome
                    .Tipo = TipoObjeto.Vessel
                End With
                CalculateFlowsheet(FlowSheet, objargs, Nothing)
                Throw New Exception(DWSIM.App.GetLocalString("Verifiqueasconexesdo"))
            ElseIf Not Me.GraphicObject.InputConnectors(1).IsAttached Then
                'Call function to calculate flowsheet
                With objargs
                    .Calculado = False
                    .Nome = Me.Nome
                    .Tipo = TipoObjeto.Vessel
                End With
                CalculateFlowsheet(FlowSheet, objargs, Nothing)
                Throw New Exception(DWSIM.App.GetLocalString("Verifiqueasconexesdo"))
            End If

        End Sub

        Public Overrides Function Calculate(Optional ByVal args As Object = Nothing) As Integer

            Dim objargs As New DWSIM.Outros.StatusChangeEventArgs

            'Validate unitop status.
            Me.Validate()

            'streams

            Dim feed, distillate, bottoms As Streams.MaterialStream
            Dim cduty, rduty As Streams.EnergyStream

            feed = FlowSheet.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.InputConnectors(0).AttachedConnector.AttachedFrom.Name)
            distillate = FlowSheet.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.OutputConnectors(0).AttachedConnector.AttachedTo.Name)
            bottoms = FlowSheet.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.OutputConnectors(1).AttachedConnector.AttachedTo.Name)
            cduty = FlowSheet.Collections.CLCS_EnergyStreamCollection(Me.GraphicObject.EnergyConnector.AttachedConnector.AttachedTo.Name)
            rduty = FlowSheet.Collections.CLCS_EnergyStreamCollection(Me.GraphicObject.InputConnectors(1).AttachedConnector.AttachedFrom.Name)

            'classify components by relative volatility

            Dim n As Integer = feed.Fases(0).Componentes.Count - 1
            Dim i As Integer = 0

            Dim lnk, dnk, hnk As New ArrayList
            Dim hki, lki As Integer
            Dim K(n), alpha(n), z(n), xb(n), xd(n), F, D, Dant, B, R, q, T, P As Double
            Dim id(n) As String

            F = feed.Fases(0).SPMProperties.molarflow.GetValueOrDefault
            q = feed.Fases(1).SPMProperties.molarfraction.GetValueOrDefault
            T = feed.Fases(0).SPMProperties.temperature.GetValueOrDefault
            P = feed.Fases(0).SPMProperties.pressure.GetValueOrDefault

            i = 0
            For Each comp As DWSIM.ClassesBasicasTermodinamica.Substancia In feed.Fases(0).Componentes.Values
                z(i) = comp.FracaoMolar.GetValueOrDefault
                'K(i) = feed.Fases(2).Componentes(comp.Nome).FracaoMolar.GetValueOrDefault / feed.Fases(1).Componentes(comp.Nome).FracaoMolar.GetValueOrDefault
                id(i) = comp.Nome
                'If Double.IsInfinity(K(i)) Then K(i) = 1.0E+20
                If Me.m_lightkey = comp.Nome Then lki = i
                If Me.m_heavykey = comp.Nome Then hki = i
                i = i + 1
            Next

            feed.PropertyPackage.CurrentMaterialStream = feed

            K = feed.PropertyPackage.DW_CalcKvalue(z, T, P)

            For i = 0 To n
                If Double.IsInfinity(K(i)) Then K(i) = 1.0E+20
            Next

            i = 0
            Do
                alpha(i) = K(i) / K(hki)
                If K(i) > K(lki) Then
                    lnk.Add(i)
                ElseIf K(i) < K(lki) And K(i) > K(hki) Then
                    dnk.Add(i)
                ElseIf K(i) < K(hki) Then
                    hnk.Add(i)
                End If
                i = i + 1
            Loop Until i = n + 1

            'first D estimate
            i = 0
            D = F * z(lki)
            If lnk.Count > 0 Then
                Do
                    D += F * (z(lnk(i)))
                    i = i + 1
                Loop Until i >= lnk.Count
            End If

restart:    B = F - D

            xd(hki) = Me.m_heavykeymolarfrac
            xb(lki) = Me.m_lightkeymolarfrac

            xb(hki) = (F * z(hki) - D * xd(hki)) / (F - D)
            xd(lki) = (F * z(lki) - (F - D) * xb(lki)) / D

            'Nmin calculation (Fenske)

            Dim S As Double

            S = (xd(lki) / xd(hki)) * (xb(hki) / xb(lki))
            m_Nmin = Math.Log(S) / Math.Log(alpha(lki) / alpha(hki))

            'calculate nonkeys distribution

            Dim C, cte(n) As Double

            C = (Log10(alpha(lki)) * Log10(xd(hki) / xb(hki)) - Log10(alpha(hki)) * Log10(xd(lki) / xb(lki))) / (Log10(alpha(lki)) - Log10(alpha(hki)))

            i = 0
            Do
                If i <> lki And i <> hki Then
                    cte(i) = 10 ^ (m_Nmin * Math.Log10(alpha(i)) + C)
                    xb(i) = F * z(i) / (B + D * cte(i))
                    xd(i) = xb(i) * cte(i)
                End If
                i = i + 1
            Loop Until i = n + 1

            Dant = D

            i = 0
            D = 0
            Do
                If z(i) <> 0 Then D += Dant * xd(i)
                i = i + 1
            Loop Until i = n + 1

            If Double.IsNaN(D) Or D = 0.0# Then Throw New ArgumentOutOfRangeException("D", "Invalid value for Distillate Rate: " & D)

            CheckCalculatorStatus()

            If Not Math.Abs((D - Dant) / D) < 0.0001 Then GoTo restart

            R = m_refluxratio
            L = R * D
            L_ = L + q * F
            V_ = L_ - B
            V = D + L

            'calculate minimum reflux by Underwood's method

            Dim brentsolver As New BrentOpt.Brent
            brentsolver.DefineFuncDelegate(AddressOf rminfunc)

            Dim mode2 As Boolean = False
            Dim count As Integer = 0
            Dim indexes As New ArrayList
            Dim Dr(n) As Double
            i = 0
            Do
                Dr(i) = (alpha(i) - 1) / (alpha(lki) - 1) * D * xd(lki) / (F * z(lki)) + (alpha(lki) - alpha(i)) / (alpha(lki) - 1) * D * xd(hki) / (F * z(hki))
                If Dr(i) > 0 And Dr(i) < 1 And z(i) <> 0 And i <> lki And i <> hki Then
                    mode2 = True
                    count += 1
                    indexes.Add(i)
                End If
                i = i + 1
            Loop Until i = n + 1

            If mode2 = False Then

                Dim teta, L_Dmin, sum As Double

                teta = brentsolver.BrentOpt(alpha(hki) * 1.01, alpha(lki), 10, 0.0000001, 100, New Object() {alpha, z, q, n})

                sum = 0
                i = 0
                Do
                    If z(i) <> 0 Then sum += alpha(i) * xd(i) / (alpha(i) - teta)
                    i = i + 1
                Loop Until i = n + 1

                L_Dmin = sum - 1

                m_Rmin = L_Dmin

            Else

                Dim teta(count), xdm(count - 1) As Double

                i = 0
                Do
                    If i = 0 Then
                        teta(i) = brentsolver.BrentOpt(alpha(lki), alpha(indexes(i)), 10, 0.0001, 100, New Object() {alpha, z, q, n})
                    ElseIf i = count Then
                        teta(i) = brentsolver.BrentOpt(alpha(indexes(i - 1)), alpha(hki), 10, 0.0001, 100, New Object() {alpha, z, q, n})
                    Else
                        teta(i) = brentsolver.BrentOpt(alpha(indexes(i - 1)), alpha(indexes(i)), 10, 0.0001, 100, New Object() {alpha, z, q, n})
                    End If
                    i = i + 1
                Loop Until i = count + 1

                Dim MA As New Mapack.Matrix(count, count)
                Dim MB As New Mapack.Matrix(count, 1)
                Dim MX As New Mapack.Matrix(count, count)

                Dim j As Integer = 0
                i = 0
                Do
                    MB(i, 0) = 0
                    j = 0
                    Do
                        If j = 0 Then
                            MA(i, j) = 1 'L/D min
                        Else
                            MA(i, j) = -alpha(indexes(j)) / (alpha(indexes(j)) - teta(i))
                        End If
                        j = j + 1
                    Loop Until j = count
                    j = 0
                    Do
                        If j <> indexes(j) Then
                            MB(i, 0) += alpha(j) * xd(j) / (alpha(j) - teta(i))
                        End If
                        j = j + 1
                    Loop Until j >= count
                    MB(i, 0) -= 1
                    i = i + 1
                Loop Until i >= count

                MX = MA.Solve(MB)

                m_Rmin = MX(0, 0)

            End If

            'actual number of stages by Gilliland's method

            Dim xx, yy As Double
            xx = (R - m_Rmin) / (R + 1)
            yy = 0.75 * (1 - xx ^ 0.5668)
            m_N = (yy + m_Nmin) / (1 - yy)

            'temperatures and heat duties - copy compositions

            Dim Dmw, Bmw As Double

            i = 0
            Dmw = 0
            For Each comp As DWSIM.ClassesBasicasTermodinamica.Substancia In distillate.Fases(0).Componentes.Values
                If Double.IsNaN(xd(i)) = False Then comp.FracaoMolar = xd(i) Else comp.FracaoMolar = 0
                Dmw += comp.FracaoMolar.GetValueOrDefault * comp.ConstantProperties.Molar_Weight
                i = i + 1
            Next
            With distillate.Fases(0)
                .SPMProperties.pressure = Me.m_condenserpressure
                .SPMProperties.molarflow = D
                .SPMProperties.massflow = Dmw * D / 1000
            End With

            i = 0
            Bmw = 0
            For Each comp As DWSIM.ClassesBasicasTermodinamica.Substancia In bottoms.Fases(0).Componentes.Values
                If Double.IsNaN(xb(i)) = False Then comp.FracaoMolar = xb(i) Else comp.FracaoMolar = 0
                Bmw += comp.FracaoMolar.GetValueOrDefault * comp.ConstantProperties.Molar_Weight
                i = i + 1
            Next
            With bottoms.Fases(0)
                .SPMProperties.pressure = Me.m_boilerpressure
                .SPMProperties.molarflow = B
                .SPMProperties.massflow = Bmw * B / 1000
            End With

            Dim result As Object
            Dim TD, TB, TF, HF, HD, HD0, HB, HL As Double
            Dim pp As DWSIM.SimulationObjects.PropertyPackages.PropertyPackage = Me.PropertyPackage

            TF = feed.Fases(0).SPMProperties.temperature
            HF = feed.Fases(0).SPMProperties.enthalpy.GetValueOrDefault * feed.Fases(0).SPMProperties.molecularWeight.GetValueOrDefault

            pp.CurrentMaterialStream = distillate

            For Each comp As DWSIM.ClassesBasicasTermodinamica.Substancia In distillate.Fases(0).Componentes.Values
                comp.FracaoMassica = pp.AUX_CONVERT_MOL_TO_MASS(comp.Nome, 0)
            Next

            If Me.condtype = CondenserType.PartialCond Then
                result = pp.DW_CalcEquilibrio_ISOL(PropertyPackages.FlashSpec.P, PropertyPackages.FlashSpec.VAP, m_condenserpressure, 1, 0)
                TD = result(2)
            ElseIf Me.condtype = CondenserType.TotalCond Then
                result = pp.DW_CalcEquilibrio_ISOL(PropertyPackages.FlashSpec.P, PropertyPackages.FlashSpec.VAP, m_condenserpressure, 0, 0)
                TD = result(2)
            End If
            With distillate.Fases(0)
                .SPMProperties.temperature = TD
            End With
            CalculateMaterialStream(FlowSheet, distillate)

            HD = distillate.Fases(0).SPMProperties.enthalpy.GetValueOrDefault * distillate.Fases(0).SPMProperties.molecularWeight.GetValueOrDefault

            pp.CurrentMaterialStream = bottoms

            For Each comp As DWSIM.ClassesBasicasTermodinamica.Substancia In bottoms.Fases(0).Componentes.Values
                comp.FracaoMassica = pp.AUX_CONVERT_MOL_TO_MASS(comp.Nome, 0)
            Next

            result = pp.DW_CalcEquilibrio_ISOL(PropertyPackages.FlashSpec.P, PropertyPackages.FlashSpec.VAP, m_boilerpressure, 0.001, 0)
            TB = result(2)
            With bottoms.Fases(0)
                .SPMProperties.temperature = TB
            End With
            CalculateMaterialStream(FlowSheet, bottoms)

            HB = bottoms.Fases(0).SPMProperties.enthalpy.GetValueOrDefault * bottoms.Fases(0).SPMProperties.molecularWeight.GetValueOrDefault

            pp.CurrentMaterialStream = distillate
            If Me.condtype = CondenserType.PartialCond Then
                result = pp.DW_CalcEquilibrio_ISOL(PropertyPackages.FlashSpec.P, PropertyPackages.FlashSpec.VAP, m_condenserpressure, 0, 0)
                HL = result(4) * distillate.Fases(0).SPMProperties.molecularWeight.GetValueOrDefault
                m_Qc = -(HL - HD) * L / 1000
            ElseIf Me.condtype = CondenserType.TotalCond Then
                result = pp.DW_CalcEquilibrio_ISOL(PropertyPackages.FlashSpec.P, PropertyPackages.FlashSpec.VAP, m_condenserpressure, 1, 0)
                HD0 = result(4) * distillate.Fases(0).SPMProperties.molecularWeight.GetValueOrDefault
                result = pp.DW_CalcEquilibrio_ISOL(PropertyPackages.FlashSpec.P, PropertyPackages.FlashSpec.VAP, m_condenserpressure, 0, 0)
                HL = result(4) * distillate.Fases(0).SPMProperties.molecularWeight.GetValueOrDefault
                m_Qc = -(HL - HD0) * (L + D) / 1000
            End If

            m_Qb = D / 1000 * HD + B / 1000 * HB + m_Qc - F / 1000 * HF

            'optimum feed stage by Fenske's method

            Dim NminS, Ss, Ns As Double

            Ss = z(lki) / z(hki) * xb(hki) / xb(lki)
            'alpha_s = bottoms.Fases(2).Componentes(m_lightkey).FracaoMolar.GetValueOrDefault / bottoms.Fases(1).Componentes(m_lightkey).FracaoMolar.GetValueOrDefault
            'alpha_s = alpha_s / (bottoms.Fases(2).Componentes(m_heavykey).FracaoMolar.GetValueOrDefault / bottoms.Fases(1).Componentes(m_heavykey).FracaoMolar.GetValueOrDefault)
            NminS = Log(Ss) / Log(alpha(lki))
            Ns = NminS * m_N / m_Nmin
            ofs = Ns

            'update exchanger duties

            With cduty
                .Energia = m_Qc
                .GraphicObject.Calculated = True
            End With

            With rduty
                .Energia = m_Qb
                .GraphicObject.Calculated = True
            End With

            'call the flowsheet calculation routine

            With objargs
                .Calculado = True
                .Nome = Me.Nome
                .Tipo = Me.GraphicObject.TipoObjeto
            End With

final:      FlowSheet.CalculationQueue.Enqueue(objargs)

        End Function

        Function rminfunc(ByVal x As Double, ByVal otherargs As Object) As Double

            If Double.IsNaN(x) Then Exit Function

            Dim alpha As Object = otherargs(0)
            Dim z As Object = otherargs(1)
            Dim q As Double = otherargs(2)
            Dim n As Integer = otherargs(3)

            Dim value As Double
            Dim j As Integer = 0
            Do
                If z(j) <> 0 Then value += (alpha(j) * z(j)) / (alpha(j) - x)
                j = j + 1
            Loop Until j = n + 1

            CheckCalculatorStatus()

            Return value - 1 + q

        End Function

        Public Overrides Function DeCalculate() As Integer

            If Me.GraphicObject.OutputConnectors(0).IsAttached Then

                'Zerar valores da corrente de matéria conectada a jusante
                FlowSheet.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.OutputConnectors(0).AttachedConnector.AttachedTo.Name).Clear()

            End If

            If Me.GraphicObject.OutputConnectors(1).IsAttached Then

                'Zerar valores da corrente de matéria conectada a jusante
                FlowSheet.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.OutputConnectors(1).AttachedConnector.AttachedTo.Name).Clear()

            End If

            If Me.GraphicObject.EnergyConnector.IsAttached Then

                Dim cduty As SimulationObjects.Streams.EnergyStream = FlowSheet.Collections.CLCS_EnergyStreamCollection(Me.GraphicObject.EnergyConnector.AttachedConnector.AttachedTo.Name)

                With cduty
                    .Energia = Nothing
                    .GraphicObject.Calculated = False
                End With

            End If

            'Call function to calculate flowsheet
            Dim objargs As New DWSIM.Outros.StatusChangeEventArgs
            With objargs
                .Calculado = False
                .Nome = Me.Nome
                .Tag = Me.GraphicObject.Tag
                .Tipo = Me.GraphicObject.TipoObjeto
            End With

            FlowSheet.CalculationQueue.Enqueue(objargs)

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

                If Me.m_Qc Then
                    valor = Format(Conversor.ConverterDoSI(su.spmp_heatflow, Me.m_Qc), nf)
                Else
                    valor = DWSIM.App.GetLocalString("NC")
                End If
                .Item(0).Value = valor
                .Item(0).Unit = su.spmp_heatflow

                If Me.m_Qb Then
                    valor = Format(Conversor.ConverterDoSI(su.spmp_heatflow, Me.m_Qb), nf)
                Else
                    valor = DWSIM.App.GetLocalString("NC")
                End If
                .Item(1).Value = valor
                .Item(1).Unit = su.spmp_heatflow

            End With

        End Sub

        Public Overrides Sub QTFillNodeItems()

            With Me.QTNodeTableItems

                .Clear()

                .Add(0, New DWSIM.Outros.NodeItem("QC", "", "", 0, 0, ""))
                .Add(1, New DWSIM.Outros.NodeItem("QR", "", "", 1, 0, ""))

            End With

        End Sub

        Public Overrides Sub PopulatePropertyGrid(ByRef pgrid As PropertyGridEx.PropertyGridEx, ByVal su As SistemasDeUnidades.Unidades)
            Dim Conversor As New DWSIM.SistemasDeUnidades.Conversor

            With pgrid

                .PropertySort = PropertySort.Categorized
                .ShowCustomProperties = True
                .Item.Clear()

                MyBase.PopulatePropertyGrid(pgrid, su)

                Dim ent, saida1, saida2, ec, er As String
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
                If Me.GraphicObject.InputConnectors(1).IsAttached = True Then
                    er = Me.GraphicObject.InputConnectors(1).AttachedConnector.AttachedFrom.Tag
                Else
                    er = ""
                End If
                If Me.GraphicObject.EnergyConnector.IsAttached = True Then
                    ec = Me.GraphicObject.EnergyConnector.AttachedConnector.AttachedTo.Tag
                Else
                    ec = ""
                End If

                .Item.Add(DWSIM.App.GetLocalString("SCFeed"), ent, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIInputMSSelector
                End With

                .Item.Add(DWSIM.App.GetLocalString("SCDistillate"), saida1, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIOutputMSSelector
                End With

                .Item.Add(DWSIM.App.GetLocalString("SCBottoms"), saida2, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIOutputMSSelector
                End With

                .Item.Add(DWSIM.App.GetLocalString("SCCondenserDuty"), ec, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIOutputESSelector
                End With

                .Item.Add(DWSIM.App.GetLocalString("SCReboilerDuty"), er, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIInputESSelector
                End With

                .Item.Add(DWSIM.App.GetLocalString("SCCondenserType"), Me, "condtype", False, DWSIM.App.GetLocalString("Parmetros2"), DWSIM.App.GetLocalString("SCCondenserType"), True)
                .Item.Add(DWSIM.App.GetLocalString("SCRefluxRatio"), Me, "m_refluxratio", False, DWSIM.App.GetLocalString("Parmetros2"), DWSIM.App.GetLocalString("SCRefluxRatio"), True)
                .Item.Add(DWSIM.App.GetLocalString("SCLightKey"), DWSIM.App.GetComponentName(Me.m_lightkey), False, DWSIM.App.GetLocalString("Parmetros2"), DWSIM.App.GetLocalString("SCLightKeyMF"), True)
                With .Item(.Item.Count - 1)
                    .IsBrowsable = False
                    .CustomEditor = New DWSIM.Editors.Components.UIComponentSelector
                    .DefaultValue = ""
                    .DefaultType = GetType(String)
                End With
                .Item.Add(DWSIM.App.GetLocalString("SCLightKeyMF"), Me, "m_lightkeymolarfrac", False, DWSIM.App.GetLocalString("Parmetros2"), DWSIM.App.GetLocalString("SCLightKeyMF"), True)
                .Item.Add(DWSIM.App.GetLocalString("SCHeavyKey"), DWSIM.App.GetComponentName(Me.m_heavykey), False, DWSIM.App.GetLocalString("Parmetros2"), DWSIM.App.GetLocalString("SCHeavyKey"), True)
                With .Item(.Item.Count - 1)
                    .IsBrowsable = False
                    .CustomEditor = New DWSIM.Editors.Components.UIComponentSelector
                    .DefaultValue = ""
                    .DefaultType = GetType(String)
                End With
                .Item.Add(DWSIM.App.GetLocalString("SCHeavyKeyMF"), Me, "m_heavykeymolarfrac", False, DWSIM.App.GetLocalString("Parmetros2"), DWSIM.App.GetLocalString("SCHeavyKeyMF"), True)
                Dim valor = Format(Conversor.ConverterDoSI(su.spmp_pressure, Me.m_condenserpressure), FlowSheet.Options.NumberFormat)
                .Item.Add(FT(DWSIM.App.GetLocalString("SCCondenserPressure"), su.spmp_pressure), valor, False, DWSIM.App.GetLocalString("Parmetros2"), DWSIM.App.GetLocalString("SCCondenserPressure"), True)
                With .Item(.Item.Count - 1)
                    .Tag = New Object() {FlowSheet.Options.NumberFormat, su.spmp_pressure, "P"}
                    .CustomEditor = New DWSIM.Editors.Generic.UIUnitConverter
                End With
                valor = Format(Conversor.ConverterDoSI(su.spmp_pressure, Me.m_boilerpressure), FlowSheet.Options.NumberFormat)
                .Item.Add(FT(DWSIM.App.GetLocalString("SCReboilerPressure"), su.spmp_pressure), valor, False, DWSIM.App.GetLocalString("Parmetros2"), DWSIM.App.GetLocalString("SCReboilerPressure"), True)
                With .Item(.Item.Count - 1)
                    .Tag = New Object() {FlowSheet.Options.NumberFormat, su.spmp_pressure, "P"}
                    .CustomEditor = New DWSIM.Editors.Generic.UIUnitConverter
                End With

                If Me.GraphicObject.Calculated Then

                    .Item.Add(DWSIM.App.GetLocalString("SCMinimumRefluxRatio"), Format(Me.m_Rmin, FlowSheet.Options.NumberFormat), True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("SCMinimumRefluxRatio"), True)
                    With .Item(.Item.Count - 1)
                        .DefaultValue = Nothing
                        .DefaultType = GetType(Nullable(Of Double))
                    End With
                    .Item.Add(DWSIM.App.GetLocalString("SCNminstages"), Format(Me.m_Nmin, FlowSheet.Options.NumberFormat), True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("SCNminstages"), True)
                    With .Item(.Item.Count - 1)
                        .DefaultValue = Nothing
                        .DefaultType = GetType(Nullable(Of Double))
                    End With
                    .Item.Add(DWSIM.App.GetLocalString("SCNstages"), Format(Me.m_N, FlowSheet.Options.NumberFormat), True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("SCNstages"), True)
                    With .Item(.Item.Count - 1)
                        .DefaultValue = Nothing
                        .DefaultType = GetType(Nullable(Of Double))
                    End With
                    .Item.Add(DWSIM.App.GetLocalString("SCOptimalFeedStage"), Format(Me.ofs, FlowSheet.Options.NumberFormat), True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("SCOptimalFeedStage"), True)
                    With .Item(.Item.Count - 1)
                        .DefaultValue = Nothing
                        .DefaultType = GetType(Nullable(Of Double))
                    End With
                    .Item.Add(FT(DWSIM.App.GetLocalString("SCStrippingLiquid"), su.spmp_molarflow), Format(Conversor.ConverterDoSI(su.spmp_molarflow, Me.L_), FlowSheet.Options.NumberFormat), True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("SCStrippingLiquid"), True)
                    With .Item(.Item.Count - 1)
                        .DefaultValue = Nothing
                        .DefaultType = GetType(Nullable(Of Double))
                    End With
                    .Item.Add(FT(DWSIM.App.GetLocalString("SCRectifyLiquid"), su.spmp_molarflow), Format(Conversor.ConverterDoSI(su.spmp_molarflow, Me.L), FlowSheet.Options.NumberFormat), True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("SCRectifyLiquid"), True)
                    With .Item(.Item.Count - 1)
                        .DefaultValue = Nothing
                        .DefaultType = GetType(Nullable(Of Double))
                    End With
                    .Item.Add(FT(DWSIM.App.GetLocalString("SCStrippingVapor"), su.spmp_molarflow), Format(Conversor.ConverterDoSI(su.spmp_molarflow, Me.V_), FlowSheet.Options.NumberFormat), True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("SCStrippingVapor"), True)
                    With .Item(.Item.Count - 1)
                        .DefaultValue = Nothing
                        .DefaultType = GetType(Nullable(Of Double))
                    End With
                    .Item.Add(FT(DWSIM.App.GetLocalString("SCRectifyVapor"), su.spmp_molarflow), Format(Conversor.ConverterDoSI(su.spmp_molarflow, Me.V), FlowSheet.Options.NumberFormat), True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("SCRectifyVapor"), True)
                    With .Item(.Item.Count - 1)
                        .DefaultValue = Nothing
                        .DefaultType = GetType(Nullable(Of Double))
                    End With
                    .Item.Add(FT(DWSIM.App.GetLocalString("SCCondenserDuty"), su.spmp_heatflow), Format(Conversor.ConverterDoSI(su.spmp_heatflow, Me.m_Qc), FlowSheet.Options.NumberFormat), True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("SCCondenserDuty"), True)
                    With .Item(.Item.Count - 1)
                        .DefaultValue = Nothing
                        .DefaultType = GetType(Nullable(Of Double))
                    End With
                    .Item.Add(FT(DWSIM.App.GetLocalString("SCReboilerDuty"), su.spmp_heatflow), Format(Conversor.ConverterDoSI(su.spmp_heatflow, Me.m_Qb), FlowSheet.Options.NumberFormat), True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("SCReboilerDuty"), True)
                    With .Item(.Item.Count - 1)
                        .DefaultValue = Nothing
                        .DefaultType = GetType(Nullable(Of Double))
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
                    'PROP_SC_0	Reflux Ratio
                    value = Me.m_refluxratio
                Case 1
                    'PROP_SC_1	Heavy Key Molar Fraction
                    value = Me.m_heavykeymolarfrac
                Case 2
                    'PROP_SC_2	Light Key Molar Fraction
                    value = Me.m_lightkeymolarfrac
                Case 3
                    'PROP_SC_3	Condenser Pressure
                    value = cv.ConverterDoSI(su.spmp_pressure, Me.m_condenserpressure)
                Case 4
                    'PROP_SC_4	Reboiler Pressure
                    value = cv.ConverterDoSI(su.spmp_pressure, Me.m_boilerpressure)
                Case 5
                    'PROP_SC_5	Minimun Reflux Ratio
                    value = Me.m_Rmin
                Case 6
                    'PROP_SC_6	Minimum Stages
                    value = Me.m_Nmin
                Case 7
                    'PROP_SC_7	Optimal Feed Stage
                    value = Me.ofs
                Case 8
                    'PROP_SC_8	Stripping Liquid Molar Flow
                    value = cv.ConverterDoSI(su.spmp_molarflow, Me.L_)
                Case 9
                    'PROP_SC_9	Rectify Liquid Molar Flow
                    value = cv.ConverterDoSI(su.spmp_molarflow, Me.L)
                Case 10
                    'PROP_SC_10	Stripping Vapor Molar Flow
                    value = cv.ConverterDoSI(su.spmp_molarflow, Me.V_)
                Case 11
                    'PROP_SC_11	Rectify Vapor Molar Flow
                    value = cv.ConverterDoSI(su.spmp_molarflow, Me.V)
                Case 12
                    'PROP_SC_12	Condenser Duty
                    value = cv.ConverterDoSI(su.spmp_heatflow, Me.m_Qc)
                Case 13
                    'PROP_SC_13	Reboiler Duty
                    value = cv.ConverterDoSI(su.spmp_heatflow, Me.m_Qb)
            End Select

            Return value

        End Function


        Public Overloads Overrides Function GetProperties(ByVal proptype As SimulationObjects_BaseClass.PropertyType) As String()
            Dim i As Integer = 0
            Dim proplist As New ArrayList
            Select Case proptype
                Case PropertyType.RO
                    For i = 5 To 13
                        proplist.Add("PROP_SC_" + CStr(i))
                    Next
                Case PropertyType.RW
                    For i = 0 To 13
                        proplist.Add("PROP_SC_" + CStr(i))
                    Next
                Case PropertyType.WR
                    For i = 0 To 4
                        proplist.Add("PROP_SC_" + CStr(i))
                    Next
                Case PropertyType.ALL
                    For i = 0 To 13
                        proplist.Add("PROP_SC_" + CStr(i))
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
                    'PROP_SC_0	Reflux Ratio
                    Me.m_refluxratio = propval
                Case 1
                    'PROP_SC_1	Heavy Key Molar Fraction
                    Me.m_heavykeymolarfrac = propval
                Case 2
                    'PROP_SC_2	Light Key Molar Fraction
                    Me.m_lightkeymolarfrac = propval
                Case 3
                    'PROP_SC_3	Condenser Pressure
                    Me.m_condenserpressure = cv.ConverterParaSI(su.spmp_pressure, propval)
                Case 4
                    'PROP_SC_4	Reboiler Pressure
                    Me.m_boilerpressure = cv.ConverterParaSI(su.spmp_pressure, propval)

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
                    'PROP_SC_0	Reflux Ratio
                    value = ""
                Case 1
                    'PROP_SC_1	Heavy Key Molar Fraction
                    value = ""
                Case 2
                    'PROP_SC_2	Light Key Molar Fraction
                    value = ""
                Case 3
                    'PROP_SC_3	Condenser Pressure
                    value = su.spmp_pressure
                Case 4
                    'PROP_SC_4	Reboiler Pressure
                    value = su.spmp_pressure
                Case 5
                    'PROP_SC_5	Minimun Reflux Ratio
                    value = ""
                Case 6
                    'PROP_SC_6	Minimum Stages
                    value = ""
                Case 7
                    'PROP_SC_7	Optimal Feed Stage
                    value = ""
                Case 8
                    'PROP_SC_8	Stripping Liquid Molar Flow
                    value = su.spmp_molarflow
                Case 9
                    'PROP_SC_9	Rectify Liquid Molar Flow
                    value = su.spmp_molarflow
                Case 10
                    'PROP_SC_10	Stripping Vapor Molar Flow
                    value = su.spmp_molarflow
                Case 11
                    'PROP_SC_11	Rectify Vapor Molar Flow
                    value = su.spmp_molarflow
                Case 12
                    'PROP_SC_12	Condenser Duty
                    value = su.spmp_heatflow
                Case 13
                    'PROP_SC_13	Reboiler Duty
                    value = su.spmp_heatflow
            End Select

            Return value
        End Function
    End Class

End Namespace



