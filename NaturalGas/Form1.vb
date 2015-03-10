'Natural Gas Properties Plugin for DWSIM
'Copyright 2010-2014 Daniel Medeiros

Imports FileHelpers
Imports Microsoft.Msdn.Samples.GraphicObjects
Imports DWSIM
Imports System.Windows.Forms
Imports System.Linq
Imports DWSIM.DWSIM.ClassesBasicasTermodinamica
Imports System.Threading.Tasks
Imports DWSIM.DWSIM.SimulationObjects.PropertyPackages

Public Class Form1

    Inherits WeifenLuo.WinFormsUI.Docking.DockContent

    'flowsheet reference
    Public fsheet As DWSIM.FormFlowsheet

    'collection of component mass heating values
    Public dmc As New Dictionary(Of String, datamass)

    'collection of component volumetric heating values
    Public dvc As New Dictionary(Of String, datavol)

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing

        'remove SelectedObjectChanged event handler
        RemoveHandler fsheet.FormSurface.FlowsheetDesignSurface.MouseUp, AddressOf SelectedObjectChanged

    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        'add a handler to the SelectedObjectChanged event, triggered when the flowsheet's selected object changes
        AddHandler fsheet.FormSurface.FlowsheetDesignSurface.MouseUp, AddressOf SelectedObjectChanged

        'read data from text files
        Dim engine As New FileHelperEngine(Of datamass)()
        Dim compsm As datamass() = engine.ReadString(My.Resources.pc_massico)
        dmc = New Dictionary(Of String, datamass)
        For Each d In compsm
            If d.dbname <> "" Then dmc.Add(d.dbname, d)
        Next
        Dim engine2 As New FileHelperEngine(Of datavol)()
        Dim compsv As datavol() = engine2.ReadString(My.Resources.pc_volumetrico)
        dvc = New Dictionary(Of String, datavol)
        For Each d In compsv
            If d.dbname <> "" Then dvc.Add(d.dbname, d)
        Next

    End Sub

    Sub SelectedObjectChanged(ByVal sender As Object, ByVal e As MouseEventArgs)

        Me.lblStream.Text = ""
        Me.lblCalcd.Text = ""
        Me.lblVapOnly.Text = ""

        Me.pg.ShowCustomProperties = True
        Me.pg.Item.Clear()
        Me.Invalidate()

        'check if we have a selected object.
        If Not fsheet.FormSurface.FlowsheetDesignSurface.SelectedObject Is Nothing Then

            'check if the selected object is a material stream.
            If fsheet.FormSurface.FlowsheetDesignSurface.SelectedObject.TipoObjeto = Microsoft.Msdn.Samples.GraphicObjects.TipoObjeto.MaterialStream Then

                'get a reference to the material stream graphic object.
                Dim gobj As GraphicObject = fsheet.FormSurface.FlowsheetDesignSurface.SelectedObject

                Me.lblStream.Text = gobj.Tag
                If gobj.Calculated Then
                    Me.lblCalcd.Text = "Yes"
                Else
                    Me.lblCalcd.Text = "No"
                End If

                'get a reference to the material stream base class.
                Dim dobj As DWSIM.DWSIM.SimulationObjects.Streams.MaterialStream = fsheet.Collections.CLCS_MaterialStreamCollection(gobj.Name)

                'check if the stream is vapor only.
                If dobj.Fases(2).SPMProperties.molarfraction = 1 Then
                    Me.lblVapOnly.Text = "Yes"
                Else
                    Me.lblVapOnly.Text = "No"
                End If

                'declare heating value variables.
                Dim hhv25m As Double = 0
                Dim hhv20m As Double = 0
                Dim hhv15m As Double = 0
                Dim hhv0m As Double = 0
                Dim lhv25m As Double = 0
                Dim lhv20m As Double = 0
                Dim lhv15m As Double = 0
                Dim lhv0m As Double = 0
                Dim hhv1515v As Double = 0
                Dim hhv00v As Double = 0
                Dim hhv2020v As Double = 0
                Dim lhv1515v As Double = 0
                Dim lhv00v As Double = 0
                Dim lhv2020v As Double = 0
                Dim hhv1515vr As Double = 0
                Dim hhv00vr As Double = 0
                Dim hhv2020vr As Double = 0
                Dim lhv1515vr As Double = 0
                Dim lhv00vr As Double = 0
                Dim lhv2020vr As Double = 0

                'declare wobbe index variables.
                Dim iw0 As Double = 0
                Dim iw15 As Double = 0
                Dim iw20 As Double = 0
                Dim iw0r As Double = 0
                Dim iw15r As Double = 0
                Dim iw20r As Double = 0

                'methane number variables
                Dim mon, mn, xc1, xc2, xc3, xc4, xco2, xn2 As Double
                Dim c1, c2, c3, ic4, nc4, co2, n2 As Substancia

                'molecular weight
                Dim mw As Double = dobj.Fases(0).SPMProperties.molecularWeight

                'declare a temporary material stream so we can do calculations without messing with the simulation.
                Dim tmpms As New DWSIM.DWSIM.SimulationObjects.Streams.MaterialStream("", "")
                tmpms = dobj.Clone
                tmpms.PropertyPackage = dobj.PropertyPackage.Clone
                tmpms.PropertyPackage.CurrentMaterialStream = tmpms

                'get the current composition, check if there is water and create a new, "dry" composition
                'vx = current composition
                'vxnw = dry composition
                Dim vx(dobj.Fases(0).Componentes.Count - 1), vxnw(dobj.Fases(0).Componentes.Count - 1), vxw(dobj.Fases(0).Componentes.Count - 1) As Double
                Dim i As Integer = 0
                Dim iw As Integer = -1
                For Each c As DWSIM.DWSIM.ClassesBasicasTermodinamica.Substancia In dobj.Fases(0).Componentes.Values
                    vx(i) = c.FracaoMolar
                    If c.ConstantProperties.CAS_Number = "7732-18-5" Then
                        iw = i
                    End If
                    i += 1
                Next

                If iw <> -1 Then
                    If vx(iw) <> 0.0# Then
                        'water is present
                        i = 0
                        For Each c As DWSIM.DWSIM.ClassesBasicasTermodinamica.Substancia In dobj.Fases(0).Componentes.Values
                            If i <> iw Then
                                vxnw(i) = vx(i) / (1 - vx(iw))
                            Else
                                vxnw(i) = 0.0#
                                vxw(i) = 1.0#
                            End If
                            i += 1
                        Next
                    End If
                Else
                    'if there is no water, clone the current composition.
                    vxnw = vx.Clone
                End If

                Me.pg.ShowCustomProperties = True
                Me.pg.Item.Clear()
                Me.pg.Item.Add("Calculating", "please wait...", True, "Natural Gas Properties", "", True)
                Me.pg.PropertySort = PropertySort.Categorized
                Me.pg.ShowCustomProperties = True

                'wdp    =   Water dew point (real, not reliable)
                'hdp    =   Hydrocarbon dew point
                '           Calculated using the dry composition and a normal PV-Flash.
                'iwdp   =   Ideal water dew point
                '           Calculated based on the Raoult's law:
                '           xiPisat = yiP => Pisat = yiP/xi
                '           After calculating Pisat (water partial vapor pressure), use the AUX_TSATi function 
                '           to return the saturation temperature (dew point).
                Dim wdp, hdp, iwdp, wc0, wc15, wc20, wcb, wdp1, iwdp1, hdp1 As Double
                Dim fa As New DWSIM.DWSIM.SimulationObjects.PropertyPackages.Auxiliary.FlashAlgorithms.NestedLoops3PV2
                Dim fa2 As New DWSIM.DWSIM.SimulationObjects.PropertyPackages.Auxiliary.FlashAlgorithms.NestedLoops3PV2
                fa.StabSearchCompIDs = New String() {"Agua", "Water"}
                fa.StabSearchSeverity = 0

                Dim pp1 As PropertyPackage = tmpms.PropertyPackage
                Dim pp2 As PropertyPackage = tmpms.PropertyPackage.Clone

                pp1.CurrentMaterialStream = tmpms
                pp2.CurrentMaterialStream = tmpms.Clone

                Try
                    If iw <> -1 Then
                        If vx(iw) <> 0.0# Then
                            Dim t1 As Task = Task.Factory.StartNew(Sub()
                                                                       Dim result As Object = pp1.FlashBase.Flash_PV(vx, tmpms.Fases(0).SPMProperties.pressure, 1, 273.15, pp1)
                                                                       wdp = fa.Flash_PV_3P(vx, result(1), result(0), 0, result(3), result(2), vxw, tmpms.Fases(0).SPMProperties.pressure, 1, result(4), pp1)(4)
                                                                       iwdp = pp1.AUX_TSATi(vx(iw) * tmpms.Fases(0).SPMProperties.pressure.GetValueOrDefault, iw)
                                                                       hdp = pp1.FlashBase.Flash_PV(vxnw, tmpms.Fases(0).SPMProperties.pressure, 1, result(4), pp1)(4)
                                                                   End Sub)
                            Dim t2 As Task = Task.Factory.StartNew(Sub()
                                                                       Dim result As Object = pp2.FlashBase.Flash_PV(vx, 101325, 1, 253.15, pp2)
                                                                       wdp1 = fa2.Flash_PV_3P(vx, result(1), result(0), 0, result(3), result(2), vxw, 101325, 1, result(4), pp2)(4)
                                                                       iwdp1 = pp2.AUX_TSATi(vx(iw) * 101325, iw)
                                                                       hdp1 = pp2.FlashBase.Flash_PV(vxnw, 101325, 1, result(4), pp2)(4)
                                                                   End Sub)
                            Threading.Thread.Sleep(500)
                            While t1.Status = TaskStatus.Running Or t2.Status = TaskStatus.Running
                                Threading.Thread.Sleep(500)
                                Application.DoEvents()
                            End While
                        End If
                    Else
                        wdp = -1.0E+20
                        iwdp = -1.0E+20
                    End If
                Catch ex As Exception
                    fsheet.WriteToLog(ex.ToString, Drawing.Color.Red, DWSIM.DWSIM.FormClasses.TipoAviso.Erro)
                Finally
                    fa = Nothing
                    fa2 = Nothing
                    pp2.Dispose()
                    pp2.CurrentMaterialStream = Nothing
                    pp2 = Nothing
                End Try

                'set stream pressure
                tmpms.Fases(0).SPMProperties.pressure = 101325

                'compressibility factors and specific gravities
                Dim z0, z15, z20, d0, d15, d20, d As Double

                'ideal gas specific gravity
                d = mw / 28.9626

                tmpms.Fases(0).SPMProperties.temperature = 273.15 + 0
                tmpms.PropertyPackage.DW_CalcPhaseProps(DWSIM.DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                z0 = tmpms.Fases(2).SPMProperties.compressibilityFactor
                d0 = d / z0

                tmpms.Fases(0).SPMProperties.temperature = 273.15 + 15.56
                tmpms.PropertyPackage.DW_CalcPhaseProps(DWSIM.DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                z15 = tmpms.Fases(2).SPMProperties.compressibilityFactor
                d15 = d / z15

                tmpms.Fases(0).SPMProperties.temperature = 273.15 + 20
                tmpms.PropertyPackage.DW_CalcPhaseProps(DWSIM.DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                z20 = tmpms.Fases(2).SPMProperties.compressibilityFactor
                d20 = d / z20

                If iw <> -1 Then
                    If vx(iw) <> 0.0# Then
                        'water content in mg/m3
                        If tmpms.Fases(0).Componentes.ContainsKey("Agua") Then
                            wcb = vx(iw) * tmpms.Fases(0).Componentes("Agua").ConstantProperties.Molar_Weight * 1000 * 1000
                        ElseIf tmpms.Fases(0).Componentes.ContainsKey("Water") Then
                            wcb = vx(iw) * tmpms.Fases(0).Componentes("Water").ConstantProperties.Molar_Weight * 1000 * 1000
                        End If
                        wc0 = wcb / (1 * z0 * 8314.47 * (273.15 + 0) / 101325)
                        wc15 = wcb / (1 * z15 * 8314.47 * (273.15 + 15.56) / 101325)
                        wc20 = wcb / (1 * z20 * 8314.47 * (273.15 + 20) / 101325)
                    End If
                End If

                'calculation of heating values at various conditions
                For Each c As DWSIM.DWSIM.ClassesBasicasTermodinamica.Substancia In dobj.Fases(0).Componentes.Values
                    If dmc.ContainsKey(c.ConstantProperties.CAS_Number) Then
                        hhv25m += c.FracaoMolar * c.ConstantProperties.Molar_Weight / mw * dmc(c.ConstantProperties.CAS_Number).sup25 * 1000
                        hhv20m += c.FracaoMolar * c.ConstantProperties.Molar_Weight / mw * dmc(c.ConstantProperties.CAS_Number).sup20 * 1000
                        hhv15m += c.FracaoMolar * c.ConstantProperties.Molar_Weight / mw * dmc(c.ConstantProperties.CAS_Number).sup15 * 1000
                        hhv0m += c.FracaoMolar * c.ConstantProperties.Molar_Weight / mw * dmc(c.ConstantProperties.CAS_Number).sup0 * 1000
                        lhv25m += c.FracaoMolar * c.ConstantProperties.Molar_Weight / mw * dmc(c.ConstantProperties.CAS_Number).inf25 * 1000
                        lhv20m += c.FracaoMolar * c.ConstantProperties.Molar_Weight / mw * dmc(c.ConstantProperties.CAS_Number).inf20 * 1000
                        lhv15m += c.FracaoMolar * c.ConstantProperties.Molar_Weight / mw * dmc(c.ConstantProperties.CAS_Number).inf15 * 1000
                        lhv0m += c.FracaoMolar * c.ConstantProperties.Molar_Weight / mw * dmc(c.ConstantProperties.CAS_Number).inf0 * 1000
                    End If
                    If dvc.ContainsKey(c.ConstantProperties.CAS_Number) Then
                        hhv1515v += c.FracaoMolar * dvc(c.ConstantProperties.CAS_Number).sup1515 * 1000
                        hhv00v += c.FracaoMolar * dvc(c.ConstantProperties.CAS_Number).sup00 * 1000
                        hhv2020v += c.FracaoMolar * dvc(c.ConstantProperties.CAS_Number).sup2020 * 1000
                        lhv1515v += c.FracaoMolar * dvc(c.ConstantProperties.CAS_Number).inf1515 * 1000
                        lhv00v += c.FracaoMolar * dvc(c.ConstantProperties.CAS_Number).inf00 * 1000
                        lhv2020v += c.FracaoMolar * dvc(c.ConstantProperties.CAS_Number).inf2020 * 1000
                    End If
                Next

                'real gas heating values
                hhv1515vr = hhv1515v / z15
                hhv00vr = hhv00v / z0
                hhv2020vr = hhv2020v / z20
                lhv1515vr = lhv1515v / z15
                lhv00vr = lhv00v / z0
                lhv2020vr = lhv2020v / z20

                'ideal gas wobbe indexes
                iw0 = hhv00v / d ^ 0.5
                iw15 = hhv1515v / d ^ 0.5
                iw20 = hhv2020v / d ^ 0.5

                'real gas wobbe indexes
                iw0r = hhv00vr / d0 ^ 0.5
                iw15r = hhv1515vr / d15 ^ 0.5
                iw20r = hhv2020vr / d20 ^ 0.5

                'methane number
                c1 = (From c As Substancia In dobj.Fases(0).Componentes.Values Select c Where c.ConstantProperties.CAS_Number = "74-82-8").FirstOrDefault
                c2 = (From c As Substancia In dobj.Fases(0).Componentes.Values Select c Where c.ConstantProperties.CAS_Number = "74-84-0").FirstOrDefault
                c3 = (From c As Substancia In dobj.Fases(0).Componentes.Values Select c Where c.ConstantProperties.CAS_Number = "74-98-6").FirstOrDefault
                nc4 = (From c As Substancia In dobj.Fases(0).Componentes.Values Select c Where c.ConstantProperties.CAS_Number = "106-97-8").FirstOrDefault
                ic4 = (From c As Substancia In dobj.Fases(0).Componentes.Values Select c Where c.ConstantProperties.CAS_Number = "75-28-5").FirstOrDefault
                co2 = (From c As Substancia In dobj.Fases(0).Componentes.Values Select c Where c.ConstantProperties.CAS_Number = "124-38-9").FirstOrDefault
                n2 = (From c As Substancia In dobj.Fases(0).Componentes.Values Select c Where c.ConstantProperties.CAS_Number = "7727-37-9").FirstOrDefault

                If Not c1 Is Nothing Then xc1 = c1.FracaoMolar.GetValueOrDefault
                If Not c2 Is Nothing Then xc2 = c2.FracaoMolar.GetValueOrDefault
                If Not c3 Is Nothing Then xc3 = c3.FracaoMolar.GetValueOrDefault
                If Not nc4 Is Nothing Then xc4 = nc4.FracaoMolar.GetValueOrDefault
                If Not ic4 Is Nothing Then xc4 += ic4.FracaoMolar.GetValueOrDefault
                If Not co2 Is Nothing Then xco2 = co2.FracaoMolar.GetValueOrDefault
                If Not n2 Is Nothing Then xn2 = n2.FracaoMolar.GetValueOrDefault

                mon = 137.78 * xc1 + 29.948 * xc2 - 18.193 * xc3 - 167.062 * xc4 + 181.233 * xco2 + 26.994 * xn2
                mn = 1.445 * mon - 103.42

                'get a reference to the current number format.
                Dim nf As String = fsheet.Options.NumberFormat

                'get a reference to the current unit system.
                Dim su As DWSIM.DWSIM.SistemasDeUnidades.Unidades = fsheet.Options.SelectedUnitSystem

                'declare a new unit conversor to convert (some) calculated values to the current unit system.
                Dim cv As New DWSIM.DWSIM.SistemasDeUnidades.Conversor()

                'populate property grid with calculated values.

                With Me.pg

                    .Item.Clear()
                    .Item.Add("Molar Weight", Format(mw, nf), True, "Natural Gas Properties", "", True)
                    .Item.Add("Ideal Gas Specific Gravity", Format(d, nf), True, "Natural Gas Properties", "", True)
                    .Item.Add("Compressibility Factor @ NC", Format(z0, nf), True, "Natural Gas Properties", "NC = Normal Conditions (T = 0 °C, P = 1 atm)", True)
                    .Item.Add("Compressibility Factor @ SC", Format(z15, nf), True, "Natural Gas Properties", "SC = Standard Conditions (T = 15.56 °C, P = 1 atm)", True)
                    .Item.Add("Compressibility Factor @ BR", Format(z20, nf), True, "Natural Gas Properties", "BR = CNTP (T = 20 °C, P = 1 atm)", True)
                    .Item.Add("Specific Gravity @ NC", Format(d0, nf), True, "Natural Gas Properties", "NC = Normal Conditions (T = 0 °C, P = 1 atm)", True)
                    .Item.Add("Specific Gravity @ SC", Format(d15, nf), True, "Natural Gas Properties", "SC = Standard Conditions (T = 15.56 °C, P = 1 atm)", True)
                    .Item.Add("Specific Gravity @ BR", Format(d20, nf), True, "Natural Gas Properties", "BR = CNTP (T = 20 °C, P = 1 atm)", True)
                    .Item.Add("Mass LHV @ 0 °C (" & su.spmp_enthalpy & ")", Format(cv.ConverterDoSI(su.spmp_enthalpy, lhv0m), nf), True, "Natural Gas Properties", "LHV = Lower Heating Value", True)
                    .Item.Add("Mass LHV @ 15 °C (" & su.spmp_enthalpy & ")", Format(cv.ConverterDoSI(su.spmp_enthalpy, lhv15m), nf), True, "Natural Gas Properties", "LHV = Lower Heating Value", True)
                    .Item.Add("Mass LHV @ 20 °C (" & su.spmp_enthalpy & ")", Format(cv.ConverterDoSI(su.spmp_enthalpy, lhv20m), nf), True, "Natural Gas Properties", "LHV = Lower Heating Value", True)
                    .Item.Add("Mass LHV @ 25 °C (" & su.spmp_enthalpy & ")", Format(cv.ConverterDoSI(su.spmp_enthalpy, lhv25m), nf), True, "Natural Gas Properties", "LHV = Lower Heating Value", True)
                    .Item.Add("Mass HHV @ 0 °C (" & su.spmp_enthalpy & ")", Format(cv.ConverterDoSI(su.spmp_enthalpy, hhv0m), nf), True, "Natural Gas Properties", "HHV = Higher Heating Value", True)
                    .Item.Add("Mass HHV @ 15 °C (" & su.spmp_enthalpy & ")", Format(cv.ConverterDoSI(su.spmp_enthalpy, hhv15m), nf), True, "Natural Gas Properties", "HHV = Higher Heating Value", True)
                    .Item.Add("Mass HHV @ 20 °C (" & su.spmp_enthalpy & ")", Format(cv.ConverterDoSI(su.spmp_enthalpy, hhv20m), nf), True, "Natural Gas Properties", "HHV = Higher Heating Value", True)
                    .Item.Add("Mass HHV @ 25 °C (" & su.spmp_enthalpy & ")", Format(cv.ConverterDoSI(su.spmp_enthalpy, hhv25m), nf), True, "Natural Gas Properties", "HHV = Higher Heating Value", True)
                    .Item.Add("Molar LHV @ 0 °C (" & su.molar_enthalpy & ")", Format(cv.ConverterDoSI(su.molar_enthalpy, lhv0m * mw), nf), True, "Natural Gas Properties", "LHV = Lower Heating Value", True)
                    .Item.Add("Molar LHV @ 15 °C (" & su.molar_enthalpy & ")", Format(cv.ConverterDoSI(su.molar_enthalpy, lhv15m * mw), nf), True, "Natural Gas Properties", "LHV = Lower Heating Value", True)
                    .Item.Add("Molar LHV @ 20 °C (" & su.molar_enthalpy & ")", Format(cv.ConverterDoSI(su.molar_enthalpy, lhv20m * mw), nf), True, "Natural Gas Properties", "LHV = Lower Heating Value", True)
                    .Item.Add("Molar LHV @ 25 °C (" & su.molar_enthalpy & ")", Format(cv.ConverterDoSI(su.molar_enthalpy, lhv25m * mw), nf), True, "Natural Gas Properties", "LHV = Lower Heating Value", True)
                    .Item.Add("Molar HHV @ 0 °C (" & su.molar_enthalpy & ")", Format(cv.ConverterDoSI(su.molar_enthalpy, hhv0m * mw), nf), True, "Natural Gas Properties", "HHV = Higher Heating Value", True)
                    .Item.Add("Molar HHV @ 15 °C (" & su.molar_enthalpy & ")", Format(cv.ConverterDoSI(su.molar_enthalpy, hhv15m * mw), nf), True, "Natural Gas Properties", "HHV = Higher Heating Value", True)
                    .Item.Add("Molar HHV @ 20 °C (" & su.molar_enthalpy & ")", Format(cv.ConverterDoSI(su.molar_enthalpy, hhv20m * mw), nf), True, "Natural Gas Properties", "HHV = Higher Heating Value", True)
                    .Item.Add("Molar HHV @ 25 °C (" & su.molar_enthalpy & ")", Format(cv.ConverterDoSI(su.molar_enthalpy, hhv25m * mw), nf), True, "Natural Gas Properties", "HHV = Higher Heating Value", True)
                    .Item.Add("Ideal Gas Vol. LHV @ NC (kJ/m3)", Format(lhv00v, nf), True, "Natural Gas Properties", "NC = Normal Conditions (T = 0 °C, P = 1 atm)", True)
                    .Item.Add("Ideal Gas Vol. LHV @ SC (kJ/m3)", Format(lhv1515v, nf), True, "Natural Gas Properties", "SC = Standard Conditions (T = 15.56 °C, P = 1 atm)", True)
                    .Item.Add("Ideal Gas Vol. LHV @ BR (kJ/m3)", Format(lhv2020v, nf), True, "Natural Gas Properties", "BR = CNTP (T = 20 °C, P = 1 atm)", True)
                    .Item.Add("Ideal Gas Vol. HHV @ NC (kJ/m3)", Format(hhv00v, nf), True, "Natural Gas Properties", "NC = Normal Conditions (T = 0 °C, P = 1 atm)", True)
                    .Item.Add("Ideal Gas Vol. HHV @ SC (kJ/m3)", Format(hhv1515v, nf), True, "Natural Gas Properties", "SC = Standard Conditions (T = 15.56 °C, P = 1 atm)", True)
                    .Item.Add("Ideal Gas Vol. HHV @ BR (kJ/m3)", Format(hhv2020v, nf), True, "Natural Gas Properties", "BR = CNTP (T = 20 °C, P = 1 atm)", True)
                    .Item.Add("Vol. LHV @ NC (kJ/m3)", Format(lhv00vr, nf), True, "Natural Gas Properties", "NC = Normal Conditions (T = 0 °C, P = 1 atm)", True)
                    .Item.Add("Vol. LHV @ SC (kJ/m3)", Format(lhv1515vr, nf), True, "Natural Gas Properties", "SC = Standard Conditions (T = 15.56 °C, P = 1 atm)", True)
                    .Item.Add("Vol. LHV @ BR (kJ/m3)", Format(lhv2020vr, nf), True, "Natural Gas Properties", "BR = CNTP (T = 20 °C, P = 1 atm)", True)
                    .Item.Add("Vol. HHV @ NC (kJ/m3)", Format(hhv00vr, nf), True, "Natural Gas Properties", "NC = Normal Conditions (T = 0 °C, P = 1 atm)", True)
                    .Item.Add("Vol. HHV @ SC (kJ/m3)", Format(hhv1515vr, nf), True, "Natural Gas Properties", "SC = Standard Conditions (T = 15.56 °C, P = 1 atm)", True)
                    .Item.Add("Vol. HHV @ BR (kJ/m3)", Format(hhv2020vr, nf), True, "Natural Gas Properties", "BR = CNTP (T = 20 °C, P = 1 atm)", True)
                    .Item.Add("Ideal Gas Wobbe Index @ NC (kJ/m3)", Format(iw0, nf), True, "Natural Gas Properties", "NC = Normal Conditions (T = 0 °C, P = 1 atm)", True)
                    .Item.Add("Ideal Gas Wobbe Index @ SC (kJ/m3)", Format(iw15, nf), True, "Natural Gas Properties", "SC = Standard Conditions (T = 15.56 °C, P = 1 atm)", True)
                    .Item.Add("Ideal Gas Wobbe Index @ BR (kJ/m3)", Format(iw20, nf), True, "Natural Gas Properties", "BR = CNTP (T = 20 °C, P = 1 atm)", True)
                    .Item.Add("Wobbe Index @ NC (kJ/m3)", Format(iw0r, nf), True, "Natural Gas Properties", "NC = Normal Conditions (T = 0 °C, P = 1 atm)", True)
                    .Item.Add("Wobbe Index @ SC (kJ/m3)", Format(iw15r, nf), True, "Natural Gas Properties", "SC = Standard Conditions (T = 15.56 °C, P = 1 atm)", True)
                    .Item.Add("Wobbe Index @ BR (kJ/m3)", Format(iw20r, nf), True, "Natural Gas Properties", "BR = CNTP (T = 20 °C, P = 1 atm)", True)
                    .Item.Add("Motor Octane Number (MON)", Format(mon, nf), True, "Natural Gas Properties", "Motor Octane Number", True)
                    .Item.Add("Methane Number (MN)", Format(mn, nf), True, "Natural Gas Properties", "Methane Number", True)
                    .Item.Add("Water Dew Point @ P (" & su.spmp_temperature & ")", Format(cv.ConverterDoSI(su.spmp_temperature, wdp), nf), True, "Natural Gas Properties", "", True)
                    .Item.Add("Water Dew Point @ 1 atm (" & su.spmp_temperature & ")", Format(cv.ConverterDoSI(su.spmp_temperature, wdp1), nf), True, "Natural Gas Properties", "", True)
                    .Item.Add("Water Dew Point (Ideal) @ P (" & su.spmp_temperature & ")", Format(cv.ConverterDoSI(su.spmp_temperature, iwdp), nf), True, "Natural Gas Properties", "Water Dew Point at System Pressure, calculated using Raoult's Law and Water's Vapor Pressure experimental curve.", True)
                    .Item.Add("Water Dew Point (Ideal) @ 1 atm (" & su.spmp_temperature & ")", Format(cv.ConverterDoSI(su.spmp_temperature, iwdp1), nf), True, "Natural Gas Properties", "Water Dew Point at System Pressure, calculated using Raoult's Law and Water's Vapor Pressure experimental curve.", True)
                    .Item.Add("HC Dew Point @ P (" & su.spmp_temperature & ")", Format(cv.ConverterDoSI(su.spmp_temperature, hdp), nf), True, "Natural Gas Properties", "Hydrocarbon Dew Point at System Pressure", True)
                    .Item.Add("HC Dew Point @ 1 atm (" & su.spmp_temperature & ")", Format(cv.ConverterDoSI(su.spmp_temperature, hdp1), nf), True, "Natural Gas Properties", "Hydrocarbon Dew Point at System Pressure", True)
                    .Item.Add("Water Content @ NC (mg/m3)", Format(wc0, nf), True, "Natural Gas Properties", "Water concentration at NC = Normal Conditions (T = 0 °C, P = 1 atm)", True)
                    .Item.Add("Water Content @ SC (mg/m3)", Format(wc15, nf), True, "Natural Gas Properties", "Water concentration at SC = Standard Conditions (T = 15.56 °C, P = 1 atm)", True)
                    .Item.Add("Water Content @ BR (mg/m3)", Format(wc20, nf), True, "Natural Gas Properties", "Water concentration at BR = CNTP (T = 20 °C, P = 1 atm)", True)

                    .PropertySort = PropertySort.Categorized
                    .ShowCustomProperties = True

                End With

            End If

            fsheet.FormSurface.FlowsheetDesignSurface.Focus()

        End If

    End Sub

End Class
