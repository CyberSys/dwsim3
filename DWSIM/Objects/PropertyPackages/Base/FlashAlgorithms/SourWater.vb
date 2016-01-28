﻿'    Flash Algorithms for Sour Water simulations
'    Copyright 2016 Daniel Wagner O. de Medeiros
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

Imports System.Math
Imports DWSIM.DWSIM.SimulationObjects
Imports DWSIM.DWSIM.MathEx
Imports DWSIM.DWSIM.MathEx.Common
Imports DWSIM.DWSIM.Flowsheet.FlowsheetSolver
Imports System.Threading.Tasks
Imports DWSIM.DWSIM.ClassesBasicasTermodinamica
Imports System.Linq
Imports System.IO

Namespace DWSIM.SimulationObjects.PropertyPackages.Auxiliary.FlashAlgorithms

    <System.Serializable()> Public Class SourWater

        Inherits FlashAlgorithm

        Dim etol As Double = 0.000001
        Dim itol As Double = 0.000001
        Dim maxit_i As Integer = 100
        Dim maxit_e As Integer = 100
        Dim Hv0, Hvid, Hlid, Hf, Hv, Hl, Hs As Double
        Dim Sv0, Svid, Slid, Sf, Sv, Sl, Ss As Double

        Public Property CompoundProperties As List(Of ConstantProperties)

        Public Property Reactions As List(Of Reaction)

        Sub New()

            Reactions = New List(Of Reaction)

            Dim rfile As String = My.Application.Info.DirectoryPath & Path.DirectorySeparatorChar & "reactions" & Path.DirectorySeparatorChar & "Sour Water Reaction Set.dwrxm"

            Dim xdoc As XDocument = XDocument.Load(rfile)
            Dim data As List(Of XElement) = xdoc.Element("DWSIM_Reaction_Data").Elements.ToList
            For Each xel As XElement In data
                Dim obj As New Reaction()
                obj.LoadData(xel.Elements.ToList)
                Reactions.Add(obj)
            Next

            '   i   Name                            Equation
            '
            '   1   CO2 ionization	                CO2 + H2O <--> H+ + HCO3- 
            '   2   Carbonate production	        HCO3- <--> CO3-2 + H+ 
            '   3   Ammonia ionization	            H+ + NH3 <--> NH4+ 
            '   4   Carbamate production	        HCO3- + NH3 <--> H2NCOO- + H2O 
            '   5   H2S ionization	                H2S <--> HS- + H+ 
            '   6   Sulfide production	            HS- <--> S-2 + H+ 
            '   7   Water self-ionization	        H2O <--> OH- + H+ 
            '   8   Sodium Hydroxide dissociation   NaOH <--> OH- + Na+ 

        End Sub

        Sub Setup(conc As Dictionary(Of String, Double), conc0 As Dictionary(Of String, Double), deltaconc As Dictionary(Of String, Double), id As Dictionary(Of String, Integer))

            conc.Clear()

            conc.Add("H2O", 0.0#)
            conc.Add("H+", 0.0#)
            conc.Add("OH-", 0.0#)
            conc.Add("NH3", 0.0#)
            conc.Add("NH4+", 0.0#)
            conc.Add("CO2", 0.0#)
            conc.Add("HCO3-", 0.0#)
            conc.Add("CO3-2", 0.0#)
            conc.Add("H2NCOO-", 0.0#)
            conc.Add("H2S", 0.0#)
            conc.Add("HS-", 0.0#)
            conc.Add("S-2", 0.0#)
            conc.Add("NaOH", 0.0#)
            conc.Add("Na+", 0.0#)

            deltaconc.Clear()

            deltaconc.Add("H2O", 0.0#)
            deltaconc.Add("H+", 0.0#)
            deltaconc.Add("OH-", 0.0#)
            deltaconc.Add("NH3", 0.0#)
            deltaconc.Add("NH4+", 0.0#)
            deltaconc.Add("CO2", 0.0#)
            deltaconc.Add("HCO3-", 0.0#)
            deltaconc.Add("CO3-2", 0.0#)
            deltaconc.Add("H2NCOO-", 0.0#)
            deltaconc.Add("H2S", 0.0#)
            deltaconc.Add("HS-", 0.0#)
            deltaconc.Add("S-2", 0.0#)
            deltaconc.Add("NaOH", 0.0#)
            deltaconc.Add("Na+", 0.0#)

            conc0.Clear()

            conc0.Add("H2O", 0.0#)
            conc0.Add("H+", 0.0#)
            conc0.Add("OH-", 0.0#)
            conc0.Add("NH3", 0.0#)
            conc0.Add("NH4+", 0.0#)
            conc0.Add("CO2", 0.0#)
            conc0.Add("HCO3-", 0.0#)
            conc0.Add("CO3-2", 0.0#)
            conc0.Add("H2NCOO-", 0.0#)
            conc0.Add("H2S", 0.0#)
            conc0.Add("HS-", 0.0#)
            conc0.Add("S-2", 0.0#)
            conc0.Add("NaOH", 0.0#)
            conc0.Add("Na+", 0.0#)

            Dim wid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.CAS_Number = "7732-18-5").FirstOrDefault)
            Dim co2id As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Name = "Carbon dioxide").FirstOrDefault)
            Dim nh3id As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Name = "Ammonia").FirstOrDefault)
            Dim h2sid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Name = "Hydrogen sulfide").FirstOrDefault)
            Dim naohid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Formula = "NaOH").FirstOrDefault)
            Dim naid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Formula = "Na+").FirstOrDefault)
            Dim ohid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Formula = "OH-").FirstOrDefault)
            Dim hid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Formula = "H+").FirstOrDefault)
            Dim nh4id As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Formula = "NH4+").FirstOrDefault)
            Dim hcoid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Formula = "HCO3-").FirstOrDefault)
            Dim co3id As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Formula = "CO3-2").FirstOrDefault)
            Dim h2nid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Formula = "H2NCOO-").FirstOrDefault)
            Dim hsid As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Formula = "HS-").FirstOrDefault)
            Dim s2id As Integer = CompoundProperties.IndexOf((From c As ConstantProperties In CompoundProperties Select c Where c.Formula = "S-2").FirstOrDefault)

            id.Clear()

            id.Add("H2O", wid)
            id.Add("H+", hid)
            id.Add("OH-", ohid)
            id.Add("NH3", nh3id)
            id.Add("NH4+", nh4id)
            id.Add("CO2", co2id)
            id.Add("HCO3-", hcoid)
            id.Add("CO3-2", co3id)
            id.Add("H2NCOO-", h2nid)
            id.Add("H2S", h2sid)
            id.Add("HS-", hsid)
            id.Add("S-2", s2id)
            id.Add("NaOH", naohid)
            id.Add("Na+", naid)

        End Sub

        Public Overrides Function Flash_PT(ByVal Vz As Double(), ByVal P As Double, ByVal T As Double, ByVal PP As PropertyPackages.PropertyPackage, Optional ByVal ReuseKI As Boolean = False, Optional ByVal PrevKi As Double() = Nothing) As Object

            Return Flash_PT_Internal(Vz, P, T, PP)

        End Function

        Public Function Flash_PT_Internal(ByVal Vz As Double(), ByVal P As Double, ByVal T As Double, ByVal PP As PropertyPackages.PropertyPackage) As Object

            Dim d1, d2 As Date, dt As TimeSpan

            d1 = Date.Now

            etol = CDbl(PP.Parameters("PP_PTFELT"))
            maxit_e = CInt(PP.Parameters("PP_PTFMEI"))
            itol = CDbl(PP.Parameters("PP_PTFILT"))
            maxit_i = CInt(PP.Parameters("PP_PTFMII"))

            Dim n As Integer = CompoundProperties.Count - 1
            Dim nch, pch, pH, pH_old, pH_old0, totalkg, totalkg1, merr, errCN, totalCN, totalCN0, oldCN As Double
            Dim icount, icount0, ecount As Integer

            'Vnf = feed molar amounts (considering 1 mol of feed)
            'Vnl = liquid phase molar amounts
            'Vnv = vapor phase molar amounts
            'Vxl = liquid phase molar fractions
            'Vxv = vapor phase molar fractions
            'V, L = phase molar amounts (F = 1 = V + L)

            Dim Vnf(n), deltaVnf(n), Vnl(n), Vxf(n), Vxl(n), Vxl_ant(n), Vns(n), Vnv(n), Vxv(n), F, V, L, Lold, Vp(n), Ki(n), fx, fx_old, fx_old0, Istr, k1, k5 As Double
            Dim sumN As Double = 0

            Vnf = Vz.Clone
            Vxf = Vz.Clone

            F = 1.0#
            Lold = 0.0#

            'set up concentrations & ids

            Dim conc, conc0, deltaconc As New Dictionary(Of String, Double)
            Dim id As New Dictionary(Of String, Integer)

            Setup(conc, conc0, deltaconc, id)

            'calculate equilibrium constants (f(T))

            Dim kr As New List(Of Double)

            For Each r In Reactions
                kr.Add(r.EvaluateK(T, PP))
            Next

            Dim nl As New DWSIMDefault()

            ecount = 0

            Do

                'calculate the amount of undissociated species from dissociated ones

                If id("NaOH") > -1 Then
                    If id("Na+") > -1 Then Vxf(id("NaOH")) += Vxf(id("Na+"))
                End If

                'If id("NH3") > -1 Then
                '    If id("NH4+") > -1 Then Vxf(id("NH3")) += Vxf(id("NH4+"))
                '    If id("H2NCOO-") > -1 Then Vxf(id("NH3")) += Vxf(id("H2NCOO-"))
                'End If

                'If id("H2S") > -1 Then
                '    If id("HS-") > -1 Then Vxf(id("H2S")) += Vxf(id("HS-"))
                '    If id("S-2") > -1 Then Vxf(id("H2S")) += Vxf(id("S-2"))
                'End If

                'If id("CO2") > -1 Then
                '    If id("HCO3-") > -1 Then Vxf(id("CO2")) += Vxf(id("HCO3-"))
                '    If id("CO3-2") > -1 Then Vxf(id("CO2")) += Vxf(id("CO3-2"))
                '    If id("H2NCOO-") > -1 Then Vxf(id("CO2")) += Vxf(id("H2NCOO-"))
                'End If

                'If id("H+") > -1 Then Vxf(id("H+")) = 0.0#
                'If id("OH-") > -1 Then Vxf(id("OH-")) = 0.0#
                'If id("HCO3-") > -1 Then Vxf(id("HCO3-")) = 0.0#
                'If id("CO3-2") > -1 Then Vxf(id("CO3-2")) = 0.0#
                'If id("H2NCOO-") > -1 Then Vxf(id("H2NCOO-")) = 0.0#
                'If id("NH4+") > -1 Then Vxf(id("NH4+")) = 0.0#
                'If id("HS-") > -1 Then Vxf(id("HS-")) = 0.0#
                'If id("S-2") > -1 Then Vxf(id("S-2")) = 0.0#
                If id("Na+") > -1 Then Vxf(id("Na+")) = 0.0#

                'calculate NH3-H2S-CO2-H2O VLE

                Lold = L

                Dim flashresult = nl.CalculateEquilibrium(FlashSpec.P, FlashSpec.T, P, T, PP, Vxf, Nothing, 0.0#)
                If flashresult.ResultException IsNot Nothing Then Throw flashresult.ResultException

                With flashresult
                    L = .GetLiquidPhase1MoleFraction * F
                    V = .GetVaporPhaseMoleFraction * F
                    Vxl = .GetLiquidPhase1MoleFractions
                    Vxv = .GetVaporPhaseMoleFractions
                    Vnl = Vxl.MultiplyConstY(L)
                    Vnv = Vxv.MultiplyConstY(V)
                End With

                If L = 0.0# Then Exit Do

                If id("H+") > -1 Then Vxv(id("H+")) = 0.0#
                If id("OH-") > -1 Then Vxv(id("OH-")) = 0.0#
                If id("HCO3-") > -1 Then Vxv(id("HCO3-")) = 0.0#
                If id("CO3-2") > -1 Then Vxv(id("CO3-2")) = 0.0#
                If id("H2NCOO-") > -1 Then Vxv(id("H2NCOO-")) = 0.0#
                If id("NH4+") > -1 Then Vxv(id("NH4+")) = 0.0#
                If id("HS-") > -1 Then Vxv(id("HS-")) = 0.0#
                If id("S-2") > -1 Then Vxv(id("S-2")) = 0.0#
                If id("Na+") > -1 Then Vxv(id("Na+")) = 0.0#

                'calculate solution amounts

                totalkg = L * PP.AUX_MMM(Vxl) / 1000 'kg solution

                'calculate concentrations

                If ecount = 0 Then

                    If id("H2O") > -1 Then conc("H2O") = Vnl(id("H2O")) / totalkg
                    If id("CO2") > -1 Then conc("CO2") = Vnl(id("CO2")) / totalkg
                    If id("NH3") > -1 Then conc("NH3") = Vnl(id("NH3")) / totalkg
                    If id("H2S") > -1 Then conc("H2S") = Vnl(id("H2S")) / totalkg
                    If id("NaOH") > -1 Then conc("NaOH") = Vnl(id("NaOH")) / totalkg

                End If

                conc0("H2O") = conc("H2O")
                conc0("CO2") = conc("CO2") + conc("HCO3-") + conc("CO3-2") + conc("H2NCOO-")
                conc0("H2S") = conc("H2S") + conc("HS-") + conc("S-2")
                conc0("NH3") = conc("NH3") + conc("NH4+") + conc("H2NCOO-")
                conc0("NaOH") = conc("NaOH")

                'loop: assume a concentration of H2NCOO- 

                icount0 = 0.0#

                conc("H2NCOO-") = Math.Min(conc0("CO2") / 100, conc0("NH3") / 100)

                totalCN = conc0("CO2") + conc0("NH3")

                Do

                    'loop: pH convergence

                    If conc("H+") > 0.0# Then
                        pH = -Log10(conc("H+"))
                    Else
                        If (conc("NaOH") + conc("Na+") + conc("NH3")) > 0.0# Then pH = 12.0# Else pH = 7.0#
                        conc("H+") = 10 ^ (-pH)
                    End If

                    'calculate ionic strength

                    Istr = 1 ^ 2 * conc("H+") / 2
                    Istr += 1 ^ 2 * conc("OH-") / 2
                    Istr += 1 ^ 2 * conc("HCO3-") / 2
                    Istr += 2 ^ 2 * conc("CO3-2") / 2
                    Istr += 1 ^ 2 * conc("H2NCOO-") / 2
                    Istr += 1 ^ 2 * conc("NH4+") / 2
                    Istr += 1 ^ 2 * conc("HS-") / 2
                    Istr += 2 ^ 2 * conc("S-2") / 2
                    Istr += 1 ^ 2 * conc("Na+") / 2

                    icount = 0

                    Do

                        'calculate liquid phase chemical equilibrium

                        conc("H+") = 10 ^ (-pH)

                        '   1   CO2 ionization	                CO2 + H2O <--> H+ + HCO3- 
                        '   2   Carbonate production	        HCO3- <--> CO3-2 + H+ 

                        ' equilibrium constant ionic strength correction

                        k1 = Exp(Log(kr(0)) - 0.278 * conc("H2S") + (-1.32 + 1558.8 / (T * 1.8)) * Istr ^ 0.4)

                        conc("HCO3-") = k1 * (conc0("CO2") - conc("H2NCOO-")) / (conc("H+") + k1 + 2 * k1 * kr(1) / conc("H+"))
                        deltaconc("HCO3-") = conc("HCO3-") - conc0("HCO3-")

                        conc("CO3-2") = kr(1) * conc("HCO3-") / conc("H+")
                        deltaconc("CO3-2") = conc("CO3-2") - conc0("CO3-2")

                        '   3   Ammonia ionization	            H+ + NH3 <--> NH4+ 
                        '   4   Carbamate production	        HCO3- + NH3 <--> H2NCOO- + H2O 

                        conc("NH4+") = kr(2) * (conc("H+") * conc0("NH3") - conc("H2NCOO-")) / (1 + kr(2) * conc("H+"))
                        deltaconc("NH4+") = conc("NH4+") - conc0("NH4+")

                        '   5   H2S ionization	                H2S <--> HS- + H+ 
                        '   6   Sulfide production	            HS- <--> S-2 + H+ 

                        ' equilibrium constant ionic strength correction

                        k5 = Exp(Log(kr(4)) + 0.427 * conc("CO2"))

                        conc("HS-") = k5 * conc0("H2S") / (conc("H+") + k5 + 2 * k5 * kr(5) / conc("H+"))
                        deltaconc("HS-") = conc("HS-") - conc0("HS-")

                        conc("S-2") = kr(5) * conc("HS-") / conc("H+")
                        deltaconc("S-2") = conc("S-2") - conc0("S-2")

                        '   7   Water self-ionization	        H2O <--> OH- + H+ 
                        '   8   Sodium Hydroxide dissociation   NaOH <--> OH- + Na+ 

                        conc("OH-") = kr(6) / conc("H+") - conc("NaOH")

                        'assume full NaOH dissociation

                        conc("Na+") = conc("NaOH")

                        deltaconc("OH-") = conc("OH-") - conc0("OH-")
                        deltaconc("Na+") = conc("Na+") - conc0("Na+")

                        conc("CO2") = conc0("CO2") - conc("HCO3-") - conc("CO3-2") - conc("H2NCOO-")
                        conc("H2S") = conc0("H2S") - conc("HS-") - conc("S-2")
                        conc("NH3") = conc0("NH3") - conc("NH4+") - conc("H2NCOO-")

                        deltaconc("CO2") = -conc0("CO2") + conc("CO2")
                        deltaconc("H2S") = -conc0("H2S") + conc("H2S")
                        deltaconc("NH3") = -conc0("NH3") + conc("NH3")

                        'neutrality check

                        pch = conc("H+") + conc("NH4+") + conc("Na+")
                        nch = conc("OH-") + conc("HCO3-") + conc("H2NCOO-") + conc("HS-") + 2 * conc("S-2") + 2 * conc("CO3-2")

                        fx_old0 = fx_old
                        fx_old = fx
                        fx = pch - nch

                        If Double.IsNaN(fx) Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashError"))

                        If Abs(fx) * 1000 < itol Then Exit Do

                        pH_old0 = pH_old
                        pH_old = pH

                        If icount <= 2 Then
                            pH += 0.01
                        Else
                            pH = pH - 0.2 * fx * (pH - pH_old0) / (fx - fx_old0)
                            If Double.IsNaN(pH) Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashError"))
                        End If

                        icount += 1

                        If icount > maxit_i * 10 Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashMaxIt2"))

                    Loop

                    If totalCN = 0.0# Then Exit Do

                    oldCN = conc("H2NCOO-")

                    totalCN0 = totalCN
                    totalCN = conc("NH4+") + conc("NH3") + conc("CO2") + conc("HCO3-") + conc("CO3-2") + conc("H2NCOO-")

                    errCN = totalCN - totalCN0

                    If Math.Abs(errCN) < itol Then Exit Do

                    conc("H2NCOO-") *= totalCN / totalCN0

                    icount0 += 1

                    If icount0 > maxit_i * 10 Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashMaxIt2"))

                Loop

                'mass balance

                If id("H+") > -1 Then Vnl(id("H+")) = conc(("H+")) * totalkg
                If id("OH-") > -1 Then Vnl(id("OH-")) = conc(("OH-")) * totalkg
                If id("CO2") > -1 Then Vnl(id("CO2")) = conc(("CO2")) * totalkg
                If id("HCO3-") > -1 Then Vnl(id("HCO3-")) = conc(("HCO3-")) * totalkg
                If id("CO3-2") > -1 Then Vnl(id("CO3-2")) = conc(("CO3-2")) * totalkg
                If id("H2NCOO-") > -1 Then Vnl(id("H2NCOO-")) = conc(("H2NCOO-")) * totalkg
                If id("NH4+") > -1 Then Vnl(id("NH4+")) = conc(("NH4+")) * totalkg
                If id("HS-") > -1 Then Vnl(id("HS-")) = conc(("HS-")) * totalkg
                If id("S-2") > -1 Then Vnl(id("S-2")) = conc(("S-2")) * totalkg
                If id("Na+") > -1 Then Vnl(id("Na+")) = conc(("Na+")) * totalkg

                If id("NaOH") > -1 Then Vnl(id("NaOH")) = conc("NaOH") * totalkg
                If id("NH3") > -1 Then Vnl(id("NH3")) = conc("NH3") * totalkg
                If id("H2S") > -1 Then Vnl(id("H2S")) = conc("H2S") * totalkg
                If id("CO2") > -1 Then Vnl(id("CO2")) = conc("CO2") * totalkg

                Vxl = Vnl.NormalizeY()

                If id("H+") > -1 Then deltaVnf(id("H+")) = deltaconc("H+") * totalkg
                If id("OH-") > -1 Then deltaVnf(id("OH-")) = deltaconc("OH-") * totalkg
                If id("CO2") > -1 Then deltaVnf(id("CO2")) = deltaconc("CO2") * totalkg
                If id("HCO3-") > -1 Then deltaVnf(id("HCO3-")) = deltaconc("HCO3-") * totalkg
                If id("CO3-2") > -1 Then deltaVnf(id("CO3-2")) = deltaconc("CO3-2") * totalkg
                If id("H2NCOO-") > -1 Then deltaVnf(id("H2NCOO-")) = deltaconc("H2NCOO-") * totalkg
                If id("NH4+") > -1 Then deltaVnf(id("NH4+")) = deltaconc("NH4+") * totalkg
                If id("HS-") > -1 Then deltaVnf(id("HS-")) = deltaconc("HS-") * totalkg
                If id("S-2") > -1 Then deltaVnf(id("S-2")) = deltaconc("S-2") * totalkg
                If id("Na+") > -1 Then deltaVnf(id("Na+")) = deltaconc("Na+") * totalkg
                If id("NaOH") > -1 Then deltaVnf(id("NaOH")) = deltaconc("NaOH") * totalkg
                If id("NH3") > -1 Then deltaVnf(id("NH3")) = deltaconc("NH3") * totalkg
                If id("H2S") > -1 Then deltaVnf(id("H2S")) = deltaconc("H2S") * totalkg
                If id("CO2") > -1 Then deltaVnf(id("CO2")) = deltaconc("CO2") * totalkg

                Vnf = Vz.AddY(deltaVnf)

                Vnf(id("H2O")) = Vz(id("H2O")) - deltaconc.Values.Sum * totalkg
                Vxf = Vnf.NormalizeY

                F = Vnf.SumY

                If Double.IsNaN(F) Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashError"))

                If Abs(L - Lold) < etol * 100 And ecount > 1 Then Exit Do

                'check mass conservation

                totalkg1 = L * PP.AUX_MMM(Vxl) / 1000 'kg solution

                merr = (totalkg - totalkg1) / totalkg * 100

                'If merr > 5.0# Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashError"))

                ecount += 1

                If ecount > maxit_e Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashMaxIt2"))

            Loop

            If id("NaOH") > -1 Then Vxl(id("NaOH")) = conc0("NaOH") * totalkg / L
            If id("NH3") > -1 Then Vxl(id("NH3")) = conc0("NH3") * totalkg / L
            If id("H2S") > -1 Then Vxl(id("H2S")) = conc0("H2S") * totalkg / L
            If id("CO2") > -1 Then Vxl(id("CO2")) = conc0("CO2") * totalkg / L

            'return flash calculation results.

            d2 = Date.Now

            dt = d2 - d1

            WriteDebugInfo("PT Flash [Sour Water]: Converged in " & ecount & " iterations. Time taken: " & dt.TotalMilliseconds & " ms. Calculated pH = " & pH)

            Return New Object() {L / F, V / F, Vxl, Vxv, ecount, 0.0#, PP.RET_NullVector, 0.0#, PP.RET_NullVector()}

        End Function

        Public Overrides Function Flash_PH(ByVal Vz As Double(), ByVal P As Double, ByVal H As Double, ByVal Tref As Double, ByVal PP As PropertyPackages.PropertyPackage, Optional ByVal ReuseKI As Boolean = False, Optional ByVal PrevKi As Double() = Nothing) As Object

            Dim doparallel As Boolean = My.Settings.EnableParallelProcessing

            Dim i, j, n, ecount As Integer
            Dim d1, d2 As Date, dt As TimeSpan
            Dim L, V, T, Pf As Double

            d1 = Date.Now

            n = UBound(Vz)

            PP = PP
            Hf = H
            Pf = P

            Dim Vn(n) As String, Vx(n), Vy(n), Vx_ant(n), Vy_ant(n), Vp(n), Ki(n), Ki_ant(n), fi(n) As Double

            Vn = PP.RET_VNAMES()
            fi = Vz.Clone

            Dim maxitINT As Integer = CInt(PP.Parameters("PP_PHFMII"))
            Dim maxitEXT As Integer = CInt(PP.Parameters("PP_PHFMEI"))
            Dim tolINT As Double = CDbl(PP.Parameters("PP_PHFILT"))
            Dim tolEXT As Double = CDbl(PP.Parameters("PP_PHFELT"))

            Dim Tmin, Tmax, epsilon(4), maxDT As Double

            Tmax = 4000.0#
            Tmin = 50.0#
            maxDT = 30.0#

            epsilon(0) = 0.1
            epsilon(1) = 0.01
            epsilon(2) = 0.001
            epsilon(3) = 0.0001
            epsilon(4) = 0.00001

            Dim fx, fx2, dfdx, x1, dx As Double

            Dim cnt As Integer

            If Tref = 0.0# Then Tref = 298.15

            ' New solver 2016 Jan. 11 by Keita. K.
            ' https://sourceforge.net/p/dwsim/discussion/844528/thread/b45e6021

            For j = 0 To 4

                cnt = 0
                x1 = Tref

                Do

                    If My.Settings.EnableParallelProcessing Then
                        My.MyApplication.IsRunningParallelTasks = True
                        Dim task1 = Task.Factory.StartNew(Sub()
                                                              fx = Herror("PT", x1, P, Vz, PP)(0)
                                                          End Sub,
                                                            My.MyApplication.TaskCancellationTokenSource.Token,
                                                            TaskCreationOptions.None,
                                                            My.MyApplication.AppTaskScheduler)
                        Dim task2 = Task.Factory.StartNew(Sub()
                                                              fx2 = Herror("PT", x1 + epsilon(j), P, Vz, PP)(0)
                                                          End Sub,
                                                            My.MyApplication.TaskCancellationTokenSource.Token,
                                                            TaskCreationOptions.None,
                                                            My.MyApplication.AppTaskScheduler)
                        Task.WaitAll(task1, task2)
                        My.MyApplication.IsRunningParallelTasks = False
                    Else
                        fx = Herror("PT", x1, P, Vz, PP)(0)
                        fx2 = Herror("PT", x1 + epsilon(j), P, Vz, PP)(0)
                    End If

                    If Abs(fx) <= tolEXT Then Exit Do

                    dfdx = (fx2 - fx) / epsilon(j)
                    dx = fx / dfdx

                    If Abs(dx) > maxDT Then dx = maxDT * Sign(dx)

                    x1 = x1 - dx
                    maxDT *= 0.95
                    cnt += 1

                Loop Until cnt > maxitEXT Or Double.IsNaN(x1) Or x1 < 0.0#

                T = x1

                If Not Double.IsNaN(T) And Not Double.IsInfinity(T) And Not cnt > maxitEXT Then
                    If T > Tmin And T < Tmax Then Exit For
                End If

            Next

            If Double.IsNaN(T) Or T <= Tmin Or T >= Tmax Or cnt > maxitEXT Or Abs(fx) > tolEXT Then Throw New Exception("PH Flash [Sour Water]: Invalid result: Temperature did not converge.")

            Dim tmp As Object = Flash_PT(Vz, P, T, PP)
            L = tmp(0)
            V = tmp(1)
            Vx = tmp(2)
            Vy = tmp(3)
            ecount = tmp(4)
            For i = 0 To n
                Ki(i) = Vy(i) / Vx(i)
            Next
            d2 = Date.Now
            dt = d2 - d1
            WriteDebugInfo("PH Flash [NL]: Converged in " & ecount & " iterations. Time taken: " & dt.TotalMilliseconds & " ms.")
            Return New Object() {L, V, Vx, Vy, T, ecount, Ki, 0.0#, PP.RET_NullVector, 0.0#, PP.RET_NullVector}

        End Function

        Public Overrides Function Flash_PS(ByVal Vz As Double(), ByVal P As Double, ByVal S As Double, ByVal Tref As Double, ByVal PP As PropertyPackages.PropertyPackage, Optional ByVal ReuseKI As Boolean = False, Optional ByVal PrevKi As Double() = Nothing) As Object

            Dim doparallel As Boolean = My.Settings.EnableParallelProcessing

            Dim Vn(1) As String, Vx(1), Vy(1), Vx_ant(1), Vy_ant(1), Vp(1), Ki(1), Ki_ant(1), fi(1) As Double
            Dim i, j, n, ecount As Integer
            Dim d1, d2 As Date, dt As TimeSpan
            Dim L, V, T, Pf As Double

            d1 = Date.Now

            n = UBound(Vz)

            PP = PP
            Sf = S
            Pf = P

            ReDim Vn(n), Vx(n), Vy(n), Vx_ant(n), Vy_ant(n), Vp(n), Ki(n), fi(n)

            Vn = PP.RET_VNAMES()
            fi = Vz.Clone

            Dim maxitINT As Integer = CInt(PP.Parameters("PP_PSFMII"))
            Dim maxitEXT As Integer = CInt(PP.Parameters("PP_PSFMEI"))
            Dim tolINT As Double = CDbl(PP.Parameters("PP_PSFILT"))
            Dim tolEXT As Double = CDbl(PP.Parameters("PP_PSFELT"))

            Dim Tmin, Tmax, epsilon(4) As Double

            Tmax = 2000.0#
            Tmin = 50.0#

            epsilon(0) = 0.001
            epsilon(1) = 0.01
            epsilon(2) = 0.1
            epsilon(3) = 1
            epsilon(4) = 10

            Dim fx, fx2, dfdx, x1, dx As Double

            Dim cnt As Integer

            If Tref = 0 Then Tref = 298.15

            For j = 0 To 4

                cnt = 0
                x1 = Tref

                Do

                    If My.Settings.EnableParallelProcessing Then
                        My.MyApplication.IsRunningParallelTasks = True
                        If My.Settings.EnableGPUProcessing Then
                            'My.MyApplication.gpu.EnableMultithreading()
                        End If
                        Try
                            Dim task1 = Task.Factory.StartNew(Sub()
                                                                  fx = Serror("PT", x1, P, Vz, PP)(0)
                                                              End Sub,
                                                              My.MyApplication.TaskCancellationTokenSource.Token,
                                                              TaskCreationOptions.None,
                                                              My.MyApplication.AppTaskScheduler)
                            Dim task2 = Task.Factory.StartNew(Sub()
                                                                  fx2 = Serror("PT", x1 + epsilon(j), P, Vz, PP)(0)
                                                              End Sub,
                                                              My.MyApplication.TaskCancellationTokenSource.Token,
                                                              TaskCreationOptions.None,
                                                              My.MyApplication.AppTaskScheduler)
                            Task.WaitAll(task1, task2)
                        Catch ae As AggregateException
                            Throw ae.Flatten().InnerException
                        Finally
                            'If My.Settings.EnableGPUProcessing Then
                            '    My.MyApplication.gpu.DisableMultithreading()
                            '    My.MyApplication.gpu.FreeAll()
                            'End If
                        End Try
                        My.MyApplication.IsRunningParallelTasks = False
                    Else
                        fx = Serror("PT", x1, P, Vz, PP)(0)
                        fx2 = Serror("PT", x1 + epsilon(j), P, Vz, PP)(0)
                    End If

                    If Abs(fx) < tolEXT Then Exit Do

                    dfdx = (fx2 - fx) / epsilon(j)
                    dx = fx / dfdx

                    x1 = x1 - dx

                    cnt += 1

                Loop Until cnt > maxitEXT Or Double.IsNaN(x1)

                T = x1

                If Not Double.IsNaN(T) And Not Double.IsInfinity(T) And Not cnt > maxitEXT Then
                    If T > Tmin And T < Tmax Then Exit For
                End If

            Next

            If Double.IsNaN(T) Or T <= Tmin Or T >= Tmax Then Throw New Exception("PS Flash [Sour Water]: Invalid result: Temperature did not converge.")

            Dim tmp As Object = Flash_PT(Vz, P, T, PP)

            L = tmp(0)
            V = tmp(1)
            Vx = tmp(2)
            Vy = tmp(3)
            ecount = tmp(4)

            For i = 0 To n
                Ki(i) = Vy(i) / Vx(i)
            Next

            d2 = Date.Now

            dt = d2 - d1

            WriteDebugInfo("PS Flash [NL]: Converged in " & ecount & " iterations. Time taken: " & dt.TotalMilliseconds & " ms.")

            Return New Object() {L, V, Vx, Vy, T, ecount, Ki, 0.0#, PP.RET_NullVector, 0.0#, PP.RET_NullVector}

        End Function

        Function OBJ_FUNC_PH_FLASH(ByVal Type As String, ByVal X As Double, ByVal P As Double, ByVal Vz() As Double, ByVal PP As PropertyPackages.PropertyPackage) As Object

            Dim n As Integer = UBound(Vz)
            Dim L, V, Vx(), Vy(), _Hl, _Hv, T As Double

            If Type = "PT" Then
                Dim tmp = Me.Flash_PT(Vz, P, X, PP)
                L = tmp(0)
                V = tmp(1)
                Vx = tmp(2)
                Vy = tmp(3)
                T = X
            Else
                Dim tmp = Me.Flash_PV(Vz, P, X, 0.0#, PP)
                L = tmp(0)
                V = tmp(1)
                Vx = tmp(2)
                Vy = tmp(3)
                T = tmp(4)
            End If

            _Hv = 0.0#
            _Hl = 0.0#

            Dim mmg, mml As Double
            If V > 0 Then _Hv = PP.DW_CalcEnthalpy(Vy, T, P, State.Vapor)
            If L > 0 Then _Hl = PP.DW_CalcEnthalpy(Vx, T, P, State.Liquid)
            mmg = PP.AUX_MMM(Vy)
            mml = PP.AUX_MMM(Vx)

            Dim herr As Double = Hf - (mmg * V / (mmg * V + mml * L)) * _Hv - (mml * L / (mmg * V + mml * L)) * _Hl
            OBJ_FUNC_PH_FLASH = {herr, T, V, L, Vy, Vx}

            WriteDebugInfo("PH Flash [Sour Water]: Current T = " & T & ", Current H Error = " & herr)

            CheckCalculatorStatus()

        End Function

        Function OBJ_FUNC_PS_FLASH(ByVal Type As String, ByVal X As Double, ByVal P As Double, ByVal Vz() As Double, ByVal PP As PropertyPackages.PropertyPackage) As Object

            Dim n = UBound(Vz)
            Dim L, V, Vx(), Vy(), _Sl, _Sv, T As Double

            If Type = "PT" Then
                Dim tmp = Me.Flash_PT(Vz, P, X, PP)
                L = tmp(0)
                V = tmp(1)
                Vx = tmp(2)
                Vy = tmp(3)
                T = X
            Else
                Dim tmp = Me.Flash_PV(Vz, P, X, 0.0#, PP)
                L = tmp(0)
                V = tmp(1)
                Vx = tmp(2)
                Vy = tmp(3)
                T = tmp(4)
            End If

            _Sv = 0.0#
            _Sl = 0.0#
            Dim mmg, mml As Double

            If V > 0 Then _Sv = PP.DW_CalcEntropy(Vy, T, P, State.Vapor)
            If L > 0 Then _Sl = PP.DW_CalcEntropy(Vx, T, P, State.Liquid)
            mmg = PP.AUX_MMM(Vy)
            mml = PP.AUX_MMM(Vx)

            Dim serr As Double = Sf - (mmg * V / (mmg * V + mml * L)) * _Sv - (mml * L / (mmg * V + mml * L)) * _Sl
            OBJ_FUNC_PS_FLASH = {serr, T, V, L, Vy, Vx}

            WriteDebugInfo("PS Flash [Sour Water]: Current T = " & T & ", Current S Error = " & serr)

            CheckCalculatorStatus()

        End Function


        Function Herror(ByVal type As String, ByVal X As Double, ByVal P As Double, ByVal Vz() As Double, ByVal PP As PropertyPackages.PropertyPackage) As Object
            Return OBJ_FUNC_PH_FLASH(type, X, P, Vz, PP)
        End Function

        Function Serror(ByVal type As String, ByVal X As Double, ByVal P As Double, ByVal Vz() As Double, ByVal PP As PropertyPackages.PropertyPackage) As Object
            Return OBJ_FUNC_PS_FLASH(type, X, P, Vz, PP)
        End Function

        Public Overrides Function Flash_TV(ByVal Vz As Double(), ByVal T As Double, ByVal V As Double, ByVal Pref As Double, ByVal PP As PropertyPackages.PropertyPackage, Optional ByVal ReuseKI As Boolean = False, Optional ByVal PrevKi As Double() = Nothing) As Object

            Dim n, ecount, i As Integer
            Dim d1, d2 As Date, dt As TimeSpan

            d1 = Date.Now

            etol = CDbl(PP.Parameters("PP_PTFELT"))
            maxit_e = CInt(PP.Parameters("PP_PTFMEI"))
            itol = CDbl(PP.Parameters("PP_PTFILT"))
            maxit_i = CInt(PP.Parameters("PP_PTFMII"))

            n = UBound(Vz)

            Dim Vx(n), Vy(n), Vp(n), L, Vcalc, Vspec, P, x, x0, x00, fx, fx0, fx00, Pmin, Pmax As Double
            Dim result As Object

            Dim nl As New DWSIMDefault
            Dim flashresult = nl.CalculateEquilibrium(FlashSpec.T, FlashSpec.VAP, T, 0.0#, PP, Vz, Nothing, Pref)
            If flashresult.ResultException IsNot Nothing Then
                Pmax = 0.0#
                For i = 0 To n
                    Pmax += Vz(i) * PP.AUX_PVAPi(i, T)
                Next
            Else
                Pmax = flashresult.CalculatedPressure
            End If
            flashresult = nl.CalculateEquilibrium(FlashSpec.T, FlashSpec.VAP, T, 1.0#, PP, Vz, Nothing, Pref)
            If flashresult.ResultException IsNot Nothing Then
                Pmin = 0.0#
                For i = 0 To n
                    Pmin += Vz(i) * PP.AUX_PVAPi(i, T)
                Next
            Else
                Pmin = flashresult.CalculatedPressure
            End If

            P = Pmin + (1 - V) * (Pmax - Pmin)

            ecount = 0
            Vspec = V
            x = P

            Do

                result = Flash_PT(Vz, x, T, PP)

                If Pmax = Pmin Then
                    x = P
                    Exit Do
                End If

                Vcalc = result(1)

                fx00 = fx0
                fx0 = fx

                fx = Vspec - Vcalc

                If Abs(fx) < etol Then Exit Do

                x00 = x0
                x0 = x

                If ecount <= 1 Then
                    x *= 0.99
                Else
                    x = x - fx * (x - x00) / (fx - fx00)
                    If Double.IsNaN(x) Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashError"))
                End If

                ecount += 1

                If ecount > maxit_e Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashMaxIt2"))

            Loop

            P = x

            L = 1 - V
            Vx = result(2)
            Vy = result(3)

            d2 = Date.Now

            dt = d2 - d1

            WriteDebugInfo("TV Flash [Sour Water]: Converged in " & ecount & " iterations. Time taken: " & dt.TotalMilliseconds & " ms.")

            Return New Object() {L, V, Vx, Vy, P, ecount, Vy.DivideY(Vx), 0.0#, PP.RET_NullVector, 0.0#, PP.RET_NullVector}

        End Function

        Public Overrides Function Flash_PV(ByVal Vz As Double(), ByVal P As Double, ByVal V As Double, ByVal Tref As Double, ByVal PP As PropertyPackages.PropertyPackage, Optional ByVal ReuseKI As Boolean = False, Optional ByVal PrevKi As Double() = Nothing) As Object

            Dim n, ecount As Integer
            Dim d1, d2 As Date, dt As TimeSpan

            d1 = Date.Now

            etol = CDbl(PP.Parameters("PP_PTFELT"))
            maxit_e = CInt(PP.Parameters("PP_PTFMEI"))
            itol = CDbl(PP.Parameters("PP_PTFILT"))
            maxit_i = CInt(PP.Parameters("PP_PTFMII"))

            n = UBound(Vz)

            Dim Vx(n), Vy(n), Vp(n), L, Vcalc, Vspec, T, x, x0, x00, fx, fx0, fx00, Tmin, Tmax As Double
            Dim result As Object

            Dim nl As New DWSIMDefault
            Dim flashresult = nl.CalculateEquilibrium(FlashSpec.P, FlashSpec.VAP, P, 0.0#, PP, Vz, Nothing, 0.0#)
            If flashresult.ResultException IsNot Nothing Then
                Tmin = 230
            Else
                Tmin = flashresult.CalculatedTemperature
            End If
            flashresult = nl.CalculateEquilibrium(FlashSpec.P, FlashSpec.VAP, P, 1.0#, PP, Vz, Nothing, 0.0#)
            If flashresult.ResultException IsNot Nothing Then
                Tmax = PP.AUX_TSATi(P, "Water")
            Else
                Tmax = flashresult.CalculatedTemperature
            End If

            T = Tmin + V * (Tmax - Tmin)

            ecount = 0
            If Vspec = 0.0# Then Vspec = 0.001
            If Vspec = 1.0# Then Vspec = 0.999
            x = T

            Do

                result = Flash_PT(Vz, P, x, PP)

                Vcalc = result(1)

                fx00 = fx0
                fx0 = fx

                fx = Vspec - Vcalc

                If Abs(fx) < etol Then Exit Do

                x00 = x0
                x0 = x

                If ecount <= 1 Then
                    x += 1.0#
                Else
                    x = x - fx * (x - x00) / (fx - fx00)
                    If Double.IsNaN(x) Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashError"))
                End If

                ecount += 1

                If ecount > maxit_e Then Throw New Exception(DWSIM.App.GetLocalString("PropPack_FlashMaxIt2"))

            Loop

            T = x

            L = 1 - V
            Vx = result(2)
            Vy = result(3)

            d2 = Date.Now

            dt = d2 - d1

            WriteDebugInfo("PV Flash [Sour Water]: Converged in " & ecount & " iterations. Time taken: " & dt.TotalMilliseconds & " ms.")

            Return New Object() {L, V, Vx, Vy, T, ecount, Vy.DivideY(Vx), 0.0#, PP.RET_NullVector, 0.0#, PP.RET_NullVector}

        End Function

    End Class

End Namespace

