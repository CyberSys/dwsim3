﻿Imports System.Xml
Imports DWSIM.DWSIM.SimulationObjects.PropertyPackages
Imports DWSIM.DWSIM.SimulationObjects.Streams
Imports DWSIM.DWSIM.ClassesBasicasTermodinamica
Imports System.Xml.Serialization
Imports System.IO
Imports DWSIM.DWSIM.Flowsheet.FlowsheetSolver
Imports DWSIM.DWSIM.SimulationObjects

'    Command Line Mode Processing Functions
'    Copyright 2009-2014 Daniel Wagner O. de Medeiros
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

<System.Serializable()> Public Class CommandLineProcessor

    Sub New()

    End Sub

    Sub ProcessCommandLineArgs(ByVal fp As FormMain)

        Console.WriteLine("DWSIM - Open Source Process Simulator for Windows")
        Console.WriteLine(My.Application.Info.Copyright)
        Dim dt As DateTime = CType("01/01/2000", DateTime).AddDays(My.Application.Info.Version.Build).AddSeconds(My.Application.Info.Version.Revision * 2)
        Console.WriteLine("Version " & My.Application.Info.Version.Major & "." & My.Application.Info.Version.Minor & _
        ", Build " & My.Application.Info.Version.Build & " (" & Format(dt, "dd/MM/yyyy HH:mm") & ")")
        Console.WriteLine("Microsoft .NET Framework Runtime Version " & System.Runtime.InteropServices.RuntimeEnvironment.GetSystemVersion.ToString())
        Console.WriteLine()

        Dim argcount As Integer = My.Application.CommandLineArgs.Count
        Dim args(My.Application.CommandLineArgs.Count - 1) As String
        My.Application.CommandLineArgs.CopyTo(args, 0)

        '-CommandLine -nosplash -simfile "sim1.dwsim" -input "file1.xml" -output "file2.xml" 

        Dim inputfile As String = ""
        Dim outputfile As String = ""
        Dim simulationfile As String = ""
        Dim savechanges As Boolean = False
        Dim path1 As String = My.Computer.FileSystem.CurrentDirectory + IO.Path.DirectorySeparatorChar
        Dim vlevel As Integer = 0

        Dim i As Integer
        For i = 0 To argcount - 1
            If args(i) = "-input" Then inputfile = args(i + 1).Substring(0, args(i + 1).Length)
            If args(i) = "-output" Then outputfile = args(i + 1).Substring(0, args(i + 1).Length)
            If args(i) = "-simfile" Then simulationfile = args(i + 1).Substring(0, args(i + 1).Length)
            If args(i) = "-show" Then vlevel = args(i + 1)
            If args(i) = "-savechanges" Then savechanges = True
        Next

        Dim simulation As FormFlowsheet
        Dim xmlinput, xmloutput As New Xml.XmlDocument
        Dim reader As XmlReader

        Console.WriteLine("Working directory: " & path1)
        Console.WriteLine("Input XML file: " & inputfile)
        Console.WriteLine("Output XML file: " & outputfile)
        Console.WriteLine("Simulation file: " & simulationfile)
        Console.WriteLine()

        Console.WriteLine("Opening simulation file...")
        Console.WriteLine()

        If simulationfile.Contains(IO.Path.DirectorySeparatorChar) Then
            simulation = fp.LoadFileForCommandLine(simulationfile)
        Else
            simulation = fp.LoadFileForCommandLine(path1 + simulationfile)
        End If

        My.Application.ActiveSimulation = simulation

        If vlevel = 1 Then simulation.FormCL = New FormCLM

        If Not simulation Is Nothing Then
            If inputfile.Contains(IO.Path.DirectorySeparatorChar) Then
                reader = XmlReader.Create(inputfile)
            Else
                reader = XmlReader.Create(path1 + inputfile)
            End If
            reader.Read()
            xmlinput.Load(reader)
        End If

        Dim cult As Globalization.CultureInfo = My.MyApplication._CultureInfo.Clone
        Dim nf As Globalization.NumberFormatInfo = cult.NumberFormat

        Dim pp As DWSIM.SimulationObjects.PropertyPackages.PropertyPackage
        Dim simobj As SimulationObjects_BaseClass
        Dim mstr As MaterialStream
        Dim su As DWSIM.SistemasDeUnidades.Unidades = Nothing
        Dim runs As Integer = 1
        Dim formatted As Boolean = False
        Dim nformat As String = "N4"

        pp = Nothing
        simobj = Nothing
        mstr = Nothing
        If Not simulation Is Nothing Then
            nformat = simulation.Options.NumberFormat
        End If

        Console.WriteLine("Parsing input XML file...")
        Console.WriteLine()

        'node.Attributes("value").Value
        For Each node As XmlNode In xmlinput.ChildNodes(0).ChildNodes
            Select Case node.Name
                Case "Configuration"
                    For Each node2 As XmlNode In node.ChildNodes
                        Select Case node2.Name
                            Case "PureCompoundProperties"
                                For Each node3 As XmlNode In node2.ChildNodes 'compound string ID
                                    Dim conv As New DWSIM.SistemasDeUnidades.Conversor
                                    Dim cp As ConstantProperties = simulation.Options.SelectedComponents(node3.Attributes("Name").Value)
                                    For Each node4 As XmlNode In node3.ChildNodes
                                        With cp
                                            Console.WriteLine("Updating constant property '" & node4.Attributes("Name").Value & "' of compound '" & node3.Attributes("Name").Value & "' to " & node4.Attributes("Value").Value)
                                            Select Case node4.Attributes("Name").Value
                                               Case "Molar_Weight"
                                                    .Molar_Weight = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "Critical_Temperature"
                                                    .Critical_Temperature = conv.ConverterParaSI(node4.Attributes("Unit").Value, node4.Attributes("Value").Value)
                                                Case "Critical_Pressure"
                                                    .Critical_Pressure = conv.ConverterParaSI(node4.Attributes("Unit").Value, node4.Attributes("Value").Value)
                                                Case "Critical_Volume"
                                                    .Critical_Volume = conv.ConverterParaSI(node4.Attributes("Unit").Value, node4.Attributes("Value").Value)
                                                Case "Critical_Compressibility"
                                                    .Critical_Compressibility = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "Acentric_Factor"
                                                    .Acentric_Factor = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "Z_Rackett"
                                                    .Z_Rackett = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "PR_Volume_Translation_Coefficient"
                                                    .PR_Volume_Translation_Coefficient = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "SRK_Volume_Translation_Coefficient"
                                                    .SRK_Volume_Translation_Coefficient = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "CS_Acentric_Factor"
                                                    .Chao_Seader_Acentricity = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "CS_Solubility_Parameter"
                                                    .Chao_Seader_Solubility_Parameter = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "CS_Liquid_Molar_Volume"
                                                    .Chao_Seader_Liquid_Molar_Volume = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "IG_Entropy_of_Formation_25C"
                                                    .IG_Entropy_of_Formation_25C = conv.ConverterParaSI(node4.Attributes("Unit").Value, node4.Attributes("Value").Value)
                                                Case "IG_Enthalpy_of_Formation_25C"
                                                    .IG_Enthalpy_of_Formation_25C = conv.ConverterParaSI(node4.Attributes("Unit").Value, node4.Attributes("Value").Value)
                                                Case "IG_Gibbs_Energy_of_Formation_25C"
                                                    .IG_Gibbs_Energy_of_Formation_25C = conv.ConverterParaSI(node4.Attributes("Unit").Value, node4.Attributes("Value").Value)
                                                Case "Dipole_Moment"
                                                    .Dipole_Moment = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "Vapor_Pressure_Constant_EqNo"
                                                    .VaporPressureEquation = Integer.Parse(node4.Attributes("Value").Value, nf)
                                                Case "Vapor_Pressure_Constant_A"
                                                    .Vapor_Pressure_Constant_A = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "Vapor_Pressure_Constant_B"
                                                    .Vapor_Pressure_Constant_B = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "Vapor_Pressure_Constant_C"
                                                    .Vapor_Pressure_Constant_C = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "Vapor_Pressure_Constant_D"
                                                    .Vapor_Pressure_Constant_D = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "Vapor_Pressure_Constant_E"
                                                    .Vapor_Pressure_Constant_E = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "Ideal_Gas_Heat_Capacity_EqNo"
                                                    .IdealgasCpEquation = Integer.Parse(node4.Attributes("Value").Value, nf)
                                                Case "Ideal_Gas_Heat_Capacity_Const_A"
                                                    .Ideal_Gas_Heat_Capacity_Const_A = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "Ideal_Gas_Heat_Capacity_Const_B"
                                                    .Ideal_Gas_Heat_Capacity_Const_B = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "Ideal_Gas_Heat_Capacity_Const_C"
                                                    .Ideal_Gas_Heat_Capacity_Const_C = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "Ideal_Gas_Heat_Capacity_Const_D"
                                                    .Ideal_Gas_Heat_Capacity_Const_D = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "Ideal_Gas_Heat_Capacity_Const_E"
                                                    .Ideal_Gas_Heat_Capacity_Const_E = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "Liquid_Viscosity_Const_EqNo"
                                                    .LiquidViscosityEquation = Integer.Parse(node4.Attributes("Value").Value, nf)
                                                Case "Liquid_Viscosity_Const_A"
                                                    .Liquid_Viscosity_Const_A = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "Liquid_Viscosity_Const_B"
                                                    .Liquid_Viscosity_Const_B = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "Liquid_Viscosity_Const_C"
                                                    .Liquid_Viscosity_Const_C = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "Liquid_Viscosity_Const_D"
                                                    .Liquid_Viscosity_Const_D = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "Liquid_Viscosity_Const_E"
                                                    .Liquid_Viscosity_Const_E = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "Liquid_Density_Const_EqNo"
                                                    .LiquidDensityEquation = Integer.Parse(node4.Attributes("Value").Value, nf)
                                                Case "Liquid_Density_Const_A"
                                                    .Liquid_Density_Const_A = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "Liquid_Density_Const_B"
                                                    .Liquid_Density_Const_B = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "Liquid_Density_Const_C"
                                                    .Liquid_Density_Const_C = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "Liquid_Density_Const_D"
                                                    .Liquid_Density_Const_D = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "Liquid_Density_Const_E"
                                                    .Liquid_Density_Const_E = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "Normal_Boiling_Point"
                                                    .Normal_Boiling_Point = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "isPf"
                                                    .IsPF = Integer.Parse(node4.Attributes("Value").Value)
                                                Case "isHYP"
                                                    .IsHYPO = Integer.Parse(node4.Attributes("Value").Value)
                                                Case "HVapA"
                                                    .HVap_A = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "HVapB"
                                                    .HVap_B = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "HVapC"
                                                    .HVap_C = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "HVapD"
                                                    .HVap_D = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "HvapTmin"
                                                    .HVap_TMIN = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "HvapTMAX"
                                                    .HVap_TMAX = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "UNIQUAC_r"
                                                    If node4.Attributes("Value").Value <> "" Then .UNIQUAC_R = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "UNIQUAC_q"
                                                    If node4.Attributes("Value").Value <> "" Then .UNIQUAC_Q = Double.Parse(node4.Attributes("Value").Value, nf)
                                                Case "UNIFAC"
                                                    .UNIFACGroups.Collection = New SortedList
                                                    For Each node5 As XmlNode In node4.ChildNodes
                                                        .UNIFACGroups.Collection.Add(node5.Attributes("Name").InnerText, Integer.Parse(node5.Attributes("Value").Value))
                                                    Next
                                                    .MODFACGroups.Collection = New SortedList
                                                    For Each node5 As XmlNode In node4.ChildNodes
                                                        .MODFACGroups.Collection.Add(node5.Attributes("Name").InnerText, Integer.Parse(node5.Attributes("Value").Value))
                                                    Next
                                                Case "Elements"
                                                    .Elements.Collection = New SortedList
                                                    For Each node5 As XmlNode In node4.ChildNodes
                                                        .Elements.Collection.Add(node5.Attributes("Name").InnerText, Integer.Parse(node5.Attributes("Value").Value))
                                                    Next
                                                Case "COSMODBName"
                                                    cp.COSMODBName = node4.Attributes("Value").Value
                                            End Select
                                        End With
                                    Next
                                Next
                                For Each mat As DWSIM.SimulationObjects.Streams.MaterialStream In simulation.Collections.CLCS_MaterialStreamCollection.Values
                                    For Each p As DWSIM.ClassesBasicasTermodinamica.Fase In mat.Fases.Values
                                        For Each subst As DWSIM.ClassesBasicasTermodinamica.Substancia In p.Componentes.Values
                                            subst.ConstantProperties = simulation.Options.SelectedComponents(subst.Nome)
                                        Next
                                    Next
                                Next
                            Case "PropertyPackages"
                                For Each node3 As XmlNode In node2.ChildNodes
                                    Select Case node3.Name
                                        Case "PropertyPackage"
                                            For Each pp0 As PropertyPackage In simulation.Options.PropertyPackages.Values
                                                If pp0.Tag = node3.Attributes("Name").Value Then pp = pp0
                                                Exit For
                                            Next
                                            For Each node4 As XmlNode In node3.ChildNodes
                                                Select Case node4.Name
                                                    Case "Parameter"
                                                        For Each prop As String In pp.Parameters.Keys
                                                            If DWSIM.App.GetLocalString(prop) = node4.Attributes("Name").Value Then
                                                                Console.WriteLine("Setting parameter '" & prop & "' of Property Package '" & pp.Tag & "' to " & node4.Attributes("Value").Value)
                                                                pp.Parameters(prop) = node4.Attributes("Value").Value
                                                            End If
                                                        Next
                                                    Case "InteractionParameters"
                                                        For Each node5 As XmlNode In node4.ChildNodes
                                                            Dim c1 As String = node5.Attributes("Comp1").Value
                                                            Dim c2 As String = node5.Attributes("Comp2").Value
                                                            Console.WriteLine("Setting interaction parameters for compounds '" & c1 & "'/'" & c2 & "' on Property Package '" & pp.Tag & "'")
                                                            Select Case pp.ComponentName
                                                                Case "PC-SAFT"
                                                                    With CType(pp, PCSAFTPropertyPackage).m_pr.InteractionParameters
                                                                        If Not .ContainsKey(pp._availablecomps(c1).CAS_Number) Then
                                                                            .Add(pp._availablecomps(c1).CAS_Number, New Dictionary(Of String, Auxiliary.PCSIP))
                                                                        End If
                                                                        If Not .Item(pp._availablecomps(c1).CAS_Number).ContainsKey(pp._availablecomps(c2).CAS_Number) Then
                                                                            .Item(pp._availablecomps(c1).CAS_Number).Add(pp._availablecomps(c2).CAS_Number, New Auxiliary.PCSIP())
                                                                        End If
                                                                        With .Item(pp._availablecomps(c1).CAS_Number).Item(pp._availablecomps(c2).CAS_Number)
                                                                            .casno1 = pp._availablecomps(c1).CAS_Number
                                                                            .casno2 = pp._availablecomps(c2).CAS_Number
                                                                            .compound1 = c1
                                                                            .compound2 = c2
                                                                            .kij = node5.Attributes("kij").Value
                                                                        End With
                                                                    End With
                                                                Case "Peng-Robinson (PR)"
                                                                    With CType(pp, PengRobinsonPropertyPackage).m_pr.InteractionParameters
                                                                        If Not .ContainsKey(c1) Then .Add(c1, New Dictionary(Of String, Auxiliary.PR_IPData))
                                                                        If Not .Item(c1).ContainsKey(c2) Then .Item(c1).Add(c2, New Auxiliary.PR_IPData())
                                                                        With .Item(c1).Item(c2)
                                                                            .kij = node5.Attributes("kij").Value
                                                                        End With
                                                                    End With
                                                                Case "Peng-Robinson-Stryjek-Vera 2 (PRSV2)", "Peng-Robinson-Stryjek-Vera 2 (PRSV2-M)"
                                                                    With CType(pp, PRSV2PropertyPackage).m_pr.InteractionParameters
                                                                        If Not .ContainsKey(c1) Then .Add(c1.ToLower, New Dictionary(Of String, Auxiliary.PRSV2_IPData))
                                                                        If Not .Item(c1.ToLower).ContainsKey(c2.ToLower) Then .Item(c1.ToLower).Add(c2.ToLower, New Auxiliary.PRSV2_IPData())
                                                                        With .Item(c1.ToLower).Item(c2.ToLower)
                                                                            .kij = node5.Attributes("kij").Value
                                                                            .kji = node5.Attributes("kji").Value
                                                                        End With
                                                                    End With
                                                                Case "Peng-Robinson-Stryjek-Vera 2 (PRSV2-VL)"
                                                                    With CType(pp, PRSV2VLPropertyPackage).m_pr.InteractionParameters
                                                                        If Not .ContainsKey(c1) Then .Add(c1.ToLower, New Dictionary(Of String, Auxiliary.PRSV2_IPData))
                                                                        If Not .Item(c1.ToLower).ContainsKey(c2.ToLower) Then .Item(c1.ToLower).Add(c2.ToLower, New Auxiliary.PRSV2_IPData())
                                                                        With .Item(c1.ToLower).Item(c2.ToLower)
                                                                            .kij = node5.Attributes("kij").Value
                                                                            .kji = node5.Attributes("kji").Value
                                                                        End With
                                                                    End With
                                                                Case "Soave-Redlich-Kwong (SRK)"
                                                                    With CType(pp, SRKPropertyPackage).m_pr.InteractionParameters
                                                                        If Not .ContainsKey(c1) Then .Add(c1, New Dictionary(Of String, Auxiliary.PR_IPData))
                                                                        If Not .Item(c1).ContainsKey(c2) Then .Item(c1).Add(c2, New Auxiliary.PR_IPData())
                                                                        With .Item(c1).Item(c2)
                                                                            .kij = node5.Attributes("kij").Value
                                                                        End With
                                                                    End With
                                                                Case "Peng-Robinson / Lee-Kesler (PR/LK)"
                                                                    With CType(pp, PengRobinsonLKPropertyPackage).m_pr.InteractionParameters
                                                                        If Not .ContainsKey(c1) Then .Add(c1, New Dictionary(Of String, Auxiliary.PR_IPData))
                                                                        If Not .Item(c1).ContainsKey(c2) Then .Item(c1).Add(c2, New Auxiliary.PR_IPData())
                                                                        With .Item(c1).Item(c2)
                                                                            .kij = node5.Attributes("kij").Value
                                                                        End With
                                                                    End With
                                                                Case "UNIFAC"
                                                                    With CType(pp, UNIFACPropertyPackage).m_pr.InteractionParameters
                                                                        If Not .ContainsKey(c1) Then .Add(c1, New Dictionary(Of String, Auxiliary.PR_IPData))
                                                                        If Not .Item(c1).ContainsKey(c2) Then .Item(c1).Add(c2, New Auxiliary.PR_IPData())
                                                                        With .Item(c1).Item(c2)
                                                                            .kij = node5.Attributes("kij").Value
                                                                        End With
                                                                    End With
                                                                Case "UNIFAC-LL"
                                                                    With CType(pp, UNIFACLLPropertyPackage).m_pr.InteractionParameters
                                                                        If Not .ContainsKey(c1) Then .Add(c1, New Dictionary(Of String, Auxiliary.PR_IPData))
                                                                        If Not .Item(c1).ContainsKey(c2) Then .Item(c1).Add(c2, New Auxiliary.PR_IPData())
                                                                        With .Item(c1).Item(c2)
                                                                            .kij = node5.Attributes("kij").Value
                                                                        End With
                                                                    End With
                                                                Case "NRTL"
                                                                    With CType(pp, NRTLPropertyPackage).m_pr.InteractionParameters
                                                                        If Not .ContainsKey(c1) Then .Add(c1, New Dictionary(Of String, Auxiliary.PR_IPData))
                                                                        If Not .Item(c1).ContainsKey(c2) Then .Item(c1).Add(c2, New Auxiliary.PR_IPData())
                                                                        With .Item(c1).Item(c2)
                                                                            .kij = node5.Attributes("kij").Value
                                                                        End With
                                                                    End With
                                                                    With CType(pp, NRTLPropertyPackage).m_uni.InteractionParameters
                                                                        If Not .ContainsKey(c1) Then .Add(c1, New Dictionary(Of String, Auxiliary.NRTL_IPData))
                                                                        If Not .Item(c1).ContainsKey(c2) Then .Item(c1).Add(c2, New Auxiliary.NRTL_IPData())
                                                                        With .Item(c1).Item(c2)
                                                                            .A12 = node5.Attributes("A12").Value
                                                                            .A21 = node5.Attributes("A21").Value
                                                                            .alpha12 = node5.Attributes("alpha12").Value
                                                                        End With
                                                                    End With
                                                                Case "UNIQUAC"
                                                                    With CType(pp, UNIQUACPropertyPackage).m_pr.InteractionParameters
                                                                        If Not .ContainsKey(c1) Then .Add(c1, New Dictionary(Of String, Auxiliary.PR_IPData))
                                                                        If Not .Item(c1).ContainsKey(c2) Then .Item(c1).Add(c2, New Auxiliary.PR_IPData())
                                                                        With .Item(c1).Item(c2)
                                                                            .kij = node5.Attributes("kij").Value
                                                                        End With
                                                                    End With
                                                                    With CType(pp, UNIQUACPropertyPackage).m_uni.InteractionParameters
                                                                        If Not .ContainsKey(c1) Then .Add(c1, New Dictionary(Of String, Auxiliary.UNIQUAC_IPData))
                                                                        If Not .Item(c1).ContainsKey(c2) Then .Item(c1).Add(c2, New Auxiliary.UNIQUAC_IPData())
                                                                        With .Item(c1).Item(c2)
                                                                            .A12 = node5.Attributes("kij").Value
                                                                            .A21 = node5.Attributes("kij").Value
                                                                        End With
                                                                    End With
                                                                Case "Modified UNIFAC (Dortmund)"
                                                                    With CType(pp, MODFACPropertyPackage).m_pr.InteractionParameters
                                                                        If Not .ContainsKey(c1) Then .Add(c1, New Dictionary(Of String, Auxiliary.PR_IPData))
                                                                        If Not .Item(c1).ContainsKey(c2) Then .Item(c1).Add(c2, New Auxiliary.PR_IPData())
                                                                        With .Item(c1).Item(c2)
                                                                            .kij = node5.Attributes("kij").Value
                                                                        End With
                                                                    End With
                                                                Case "Lee-Kesler-Plöcker"
                                                                    With CType(pp, LKPPropertyPackage).m_lk.InteractionParameters
                                                                        If Not .ContainsKey(c1) Then .Add(c1, New Dictionary(Of String, Auxiliary.LKP_IPData))
                                                                        If Not .Item(c1).ContainsKey(c2) Then .Item(c1).Add(c2, New Auxiliary.LKP_IPData())
                                                                        With .Item(c1).Item(c2)
                                                                            .ID1 = c1
                                                                            .ID2 = c2
                                                                            .kij = node5.Attributes("kij").Value
                                                                        End With
                                                                    End With
                                                                Case "COSMO-SAC (JCOSMO)"
                                                                    With CType(pp, COSMOSACPropertyPackage).m_pr.InteractionParameters
                                                                        If Not .ContainsKey(c1) Then .Add(c1, New Dictionary(Of String, Auxiliary.PR_IPData))
                                                                        If Not .Item(c1).ContainsKey(c2) Then .Item(c1).Add(c2, New Auxiliary.PR_IPData())
                                                                        With .Item(c1).Item(c2)
                                                                            .kij = node5.Attributes("kij").Value
                                                                        End With
                                                                    End With
                                                            End Select

                                                        Next
                                                End Select
                                            Next
                                    End Select
                                Next
                            Case "UnitsSystem", "System of Units"
                                If node2.Attributes("Mode").Value = "Default" Then
                                    su = simulation.Options.SelectedUnitSystem
                                Else
                                    For Each s As String In fp.AvailableUnitSystems.Keys
                                        If DWSIM.App.GetLocalString("s") = node2.Attributes("Name").Value Then
                                            su = fp.AvailableUnitSystems.Item(s)
                                        End If
                                    Next
                                End If
                            Case "Simulation"
                                runs = node2.Attributes("NumberOfRuns").Value
                                If node2.Attributes("FormattedOutput").Value = "Yes" Then formatted = True
                        End Select
                    Next
                Case "InputParameters"
                    For Each node2 As XmlNode In node.ChildNodes
                        Select Case node2.Name
                            Case "SimulationObjects"
                                For Each node3 As XmlNode In node2.ChildNodes
                                    Select Case node3.Name
                                        Case "Object"
                                            For Each obj0 As SimulationObjects_BaseClass In simulation.Collections.ObjectCollection.Values
                                                If obj0.GraphicObject.Tag = node3.Attributes("Name").Value Then
                                                    simobj = obj0
                                                    If TypeOf simobj Is MaterialStream Then
                                                        mstr = simobj
                                                    End If
                                                End If
                                            Next
                                            For Each node4 As XmlNode In node3.ChildNodes
                                                Select Case node4.Name
                                                    Case "Property"
                                                        Console.WriteLine("Setting property '" & DWSIM.App.GetPropertyName(node4.Attributes("ID").Value, fp) & "' of object '" & simobj.GraphicObject.Tag & "' to " & Double.Parse(node4.Attributes("Value").Value, nf).ToString)
                                                        simobj.SetPropertyValue(node4.Attributes("ID").Value, Double.Parse(node4.Attributes("Value").Value, nf), su)
                                                    Case "Composition"
                                                        For Each node5 As XmlNode In node4.ChildNodes
                                                            Select Case node5.Name
                                                                Case "Component"
                                                                    For Each comp As Substancia In mstr.Fases(0).Componentes.Values
                                                                        If DWSIM.App.GetComponentName(comp.Nome, fp) = node5.Attributes("Name").Value Then
                                                                            Select Case node4.Attributes("Type").Value
                                                                                Case "MoleFraction"
                                                                                    Console.WriteLine("Setting mole fraction of compound '" & DWSIM.App.GetComponentName(comp.Nome, fp) & "' at Material Stream '" & mstr.GraphicObject.Tag & "' to " & Double.Parse(node5.Attributes("Value").Value, nf).ToString)
                                                                                    comp.FracaoMolar = node5.Attributes("Value").Value
                                                                            End Select
                                                                        End If
                                                                    Next
                                                            End Select
                                                        Next
                                                End Select
                                            Next
                                    End Select
                                Next
                        End Select
                    Next
            End Select
        Next

        Console.WriteLine()

        If vlevel = 1 Then
            With simulation.FormCL
                If inputfile.Contains(IO.Path.DirectorySeparatorChar) Then
                    .LblSimulationFile.Text = simulationfile
                Else
                    .LblSimulationFile.Text = path1 + simulationfile
                End If
                .LblSimulationName.Text = simulation.Options.SimNome
                .Show()
            End With
        End If

        For i = 1 To runs
            If vlevel = 1 Then simulation.FormCL.LblRunNumber.Text = i & "/" & runs
            Console.WriteLine("Running simulation... (run " & i & " of " & runs & ")")
            Console.WriteLine()
            CalculateAll(simulation)
        Next

        If savechanges Then
            Console.WriteLine("Saving changes to simulation file...")
            Console.WriteLine()
            simulation.WriteToLog("Saving changes to file...", Color.Blue, DWSIM.FormClasses.TipoAviso.Informacao)
            Application.DoEvents()
            If simulationfile.Contains(IO.Path.DirectorySeparatorChar) Then
                fp.SaveF(simulationfile, simulation)
            Else
                fp.SaveF(path1 + simulationfile, simulation)
            End If
       End If

        Console.WriteLine("Writing values to output XML file...")
        Console.WriteLine()

        xmloutput = xmlinput.Clone()

        For Each node As XmlNode In xmloutput.ChildNodes(0).ChildNodes
            Select Case node.Name
                Case "OutputParameters"
                    For Each node2 As XmlNode In node.ChildNodes
                        Select Case node2.Name
                            Case "SimulationObjects"
                                For Each node3 As XmlNode In node2.ChildNodes
                                    Select Case node3.Name
                                        Case "Object"
                                            For Each obj0 As SimulationObjects_BaseClass In simulation.Collections.ObjectCollection.Values
                                                If obj0.GraphicObject.Tag = node3.Attributes("Name").Value Then
                                                    simobj = obj0
                                                    If TypeOf simobj Is MaterialStream Then
                                                        mstr = simobj
                                                    End If
                                                End If
                                            Next
                                            For Each node4 As XmlNode In node3.ChildNodes
                                                Select Case node4.Name
                                                    Case "Property"
                                                        node4.Attributes("Name").Value = DWSIM.App.GetPropertyName(node4.Attributes("ID").Value)
                                                        node4.Attributes("Unit").Value = simobj.GetPropertyUnit(node4.Attributes("ID").Value, su)
                                                        If formatted Then
                                                            node4.Attributes("Value").Value = CDbl(simobj.GetPropertyValue(node4.Attributes("ID").Value, su)).ToString(nformat)
                                                        Else
                                                            node4.Attributes("Value").Value = CDbl(simobj.GetPropertyValue(node4.Attributes("ID").Value, su)).ToString(nf)
                                                        End If
                                                    Case "Composition"
                                                        For Each node5 As XmlNode In node4.ChildNodes
                                                            Select Case node5.Name
                                                                Case "Component"
                                                                    For Each comp As Substancia In mstr.Fases(0).Componentes.Values
                                                                        If DWSIM.App.GetComponentName(comp.Nome, fp) = node5.Attributes("Name").Value Then
                                                                            Select Case node4.Attributes("Type").Value
                                                                                Case "MoleFraction"
                                                                                    node5.Attributes("Value").Value = comp.FracaoMolar.GetValueOrDefault.ToString(nf)
                                                                                Case "MassFraction"
                                                                                    node5.Attributes("Value").Value = comp.FracaoMassica.GetValueOrDefault.ToString(nf)
                                                                                Case "VolumetricFraction"
                                                                                    node5.Attributes("Value").Value = comp.VolumetricFraction.GetValueOrDefault.ToString(nf)
                                                                            End Select

                                                                        End If
                                                                    Next
                                                            End Select
                                                        Next
                                                End Select
                                            Next
                                    End Select
                                Next
                        End Select
                    Next
            End Select
        Next

        Console.WriteLine("Saving changes to output XML file...")
        Console.WriteLine("Finished!")
        Console.WriteLine()

        If outputfile.Contains(IO.Path.DirectorySeparatorChar) Then
            xmloutput.Save(outputfile)
        Else
            xmloutput.Save(path1 + outputfile)
        End If

        simulation = Nothing

        Application.Exit()

    End Sub

End Class