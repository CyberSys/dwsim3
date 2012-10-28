﻿'    Equilibrium Reactor Calculation Routines 
'    Copyright 2008-2010 Daniel Wagner O. de Medeiros
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
Imports Ciloci.Flee
Imports System.Math
Imports DWSIM.DWSIM.MathEx.Common
Imports DotNumerics.Optimization
Imports DWSIM.DWSIM.MathEx
Imports DWSIM.DWSIM.Flowsheet.FlowSheetSolver

Namespace DWSIM.SimulationObjects.Reactors

    <System.Serializable()> Public Class Reactor_Equilibrium

        Inherits Reactor

        Protected m_reactionextents As New Dictionary(Of String, Double)
        Private _rex_iest As New ArrayList
        Private _components As New List(Of String)
        Private _initialestimates As New List(Of Double)
        Private _elements As String()
        Private _totalelements As Double()
        Private _ige, _fge As Double

        Dim tmpx As Double(), tmpdx As Double()

        Dim tms As DWSIM.SimulationObjects.Streams.MaterialStream
        Dim N0 As New Dictionary(Of String, Double)
        Dim DN As New Dictionary(Of String, Double)
        Dim N As New Dictionary(Of String, Double)
        Dim T, P, P0, Ninerts, Winerts, E(,) As Double
        Dim r, c, els, comps, i, j As Integer

#Region "Properties"

        Public Property InitialGibbsEnergy() As Double
            Get
                Return _ige
            End Get
            Set(ByVal value As Double)
                _ige = value
            End Set
        End Property

        Public Property FinalGibbsEnergy() As Double
            Get
                Return _fge
            End Get
            Set(ByVal value As Double)
                _fge = value
            End Set
        End Property

        Public ReadOnly Property ReactionExtents() As Dictionary(Of String, Double)
            Get
                Return Me.m_reactionextents
            End Get
        End Property

        Public ReadOnly Property ReactionExtentsEstimates() As ArrayList
            Get
                Return _rex_iest
            End Get
        End Property

        Public Property Elements() As String()
            Get
                Return _elements
            End Get
            Set(ByVal value As String())
                _elements = value
            End Set
        End Property

        Public ReadOnly Property ComponentIDs() As List(Of String)
            Get
                Return _components
            End Get
        End Property

        Public ReadOnly Property InitialEstimates() As List(Of Double)
            Get
                If _initialestimates Is Nothing Then _initialestimates = New List(Of Double)
                Return _initialestimates
            End Get
        End Property

        Public Property TotalElements() As Double()
            Get
                Return _totalelements
            End Get
            Set(ByVal value As Double())
                _totalelements = value
            End Set
        End Property

#End Region

#Region "Auxiliary Functions"

        Private Function FunctionGradient(ByVal x() As Double) As Double()

            Dim epsilon As Double = 0.0001

            Dim f1, f2 As Double
            Dim g(x.Length - 1), x2(x.Length - 1) As Double
            Dim i, j As Integer

            For i = 0 To x.Length - 1
                f1 = FunctionValue(x)
                For j = 0 To x.Length - 1
                    If x(j) = 0 Then
                        If i <> j Then
                            x2(j) = (x(j) + 0.000001)
                        Else
                            x2(j) = (x(j) + 0.000001) * (1 + epsilon)
                        End If
                    Else
                        If i <> j Then
                            x2(j) = x(j)
                        Else
                            x2(j) = x(j) * (1 + epsilon)
                        End If
                    End If
                Next
                f2 = FunctionValue(x2)
                g(i) = (f2 - f1) / (x2(i) - x(i))
            Next

            Return g

        End Function

        Private Function FunctionValue(ByVal x() As Double) As Double

            Dim pp As SimulationObjects.PropertyPackages.PropertyPackage = Me.PropertyPackage

            i = 0
            For Each s As String In N.Keys
                DN(s) = 0
                For j = 0 To r
                    DN(s) += E(i, j) * x(j)
                Next
                i += 1
            Next

            For Each s As String In DN.Keys
                N(s) = N0(s) + DN(s)
            Next

            Dim fw(c), fm(c), sumfm, sum1, sumn, sumw As Double

            N.Values.CopyTo(fm, 0)

            sumfm = Sum(fm) + Ninerts

            sum1 = 0
            sumn = 0
            For Each s As Substancia In tms.Fases(0).Componentes.Values
                If Me.ComponentIDs.Contains(s.Nome) Then
                    s.MolarFlow = N(s.Nome)
                    s.FracaoMolar = N(s.Nome) / sumfm
                    sum1 += N(s.Nome) * s.ConstantProperties.Molar_Weight / 1000
                Else
                    s.FracaoMolar = s.MolarFlow / sumfm
                End If
                sumn += s.MolarFlow
            Next

            tms.Fases(0).SPMProperties.molarflow = sumn

            sumw = 0
            For Each s As Substancia In tms.Fases(0).Componentes.Values
                If Me.ComponentIDs.Contains(s.Nome) Then
                    s.MassFlow = N(s.Nome) * s.ConstantProperties.Molar_Weight / 1000
                End If
                s.FracaoMassica = s.MassFlow / (sum1 + Winerts)
                sumw += s.MassFlow
            Next

            tms.Fases(0).SPMProperties.massflow = sumw

            With pp
                .CurrentMaterialStream = tms
                .DW_CalcEquilibrium(PropertyPackages.FlashSpec.T, PropertyPackages.FlashSpec.P)
                .DW_CalcPhaseProps(PropertyPackages.Fase.Mixture)
                .DW_CalcPhaseProps(PropertyPackages.Fase.Vapor)
                .DW_CalcPhaseProps(PropertyPackages.Fase.Liquid)
                .DW_CalcCompMolarFlow(-1)
                .DW_CalcCompMassFlow(-1)
                .DW_CalcCompVolFlow(-1)
                .DW_CalcOverallProps()
                .DW_CalcTwoPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid, DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                .DW_CalcVazaoVolumetrica()
                .DW_CalcKvalue()
            End With

            Dim fugs(tms.Fases(0).Componentes.Count - 1) As Double
            Dim CP(tms.Fases(0).Componentes.Count - 1) As Double
            Dim prod(x.Length - 1) As Double
            'Dim DGf As Double

            i = 0
            For Each s As Substancia In tms.Fases(2).Componentes.Values
                If s.FracaoMolar > 0.0# Then
                    'DGf = pp.AUX_DELGF_T(298.15, T, s.Nome) * s.ConstantProperties.Molar_Weight
                    fugs(i) = s.FugacityCoeff.GetValueOrDefault
                    CP(i) = (fugs(i) * s.FracaoMolar.GetValueOrDefault * P / P0)
                Else
                    fugs(i) = s.FugacityCoeff.GetValueOrDefault
                    CP(i) = (fugs(i) * 0.0000000001 * P / P0)
                End If
                i += 1
            Next

            For i = 0 To Me.Reactions.Count - 1
                prod(i) = 1
                j = 0
                For Each s As Substancia In tms.Fases(2).Componentes.Values
                    With FlowSheet.Options.Reactions(Me.Reactions(i))
                        If .Components.ContainsKey(s.Nome) Then
                            prod(i) *= CP(j) ^ .Components(s.Nome).StoichCoeff
                        End If
                    End With
                    j += 1
                Next
            Next

            Dim pen_val As Double = ReturnPenaltyValue()

            CheckCalculatorStatus()

            Dim objfunc As Double = 0
            For i = 0 To Me.Reactions.Count - 1
                With FlowSheet.Options.Reactions(Me.Reactions(i))
                    objfunc += Abs(prod(i) - .ConstantKeqValue)
                End With
            Next


            Dim fval As Double
            If Double.IsNaN(objfunc) Or Double.IsInfinity(objfunc) Then
                fval = pen_val
            Else
                fval = objfunc + pen_val
            End If
            Return fval

        End Function

        Private Function FunctionValue2N(ByVal x() As Double) As Double()

            Dim pp As SimulationObjects.PropertyPackages.PropertyPackage = Me.PropertyPackage

            i = 0
            For Each s As String In N.Keys
                DN(s) = 0
                For j = 0 To r
                    DN(s) += E(i, j) * x(j)
                Next
                i += 1
            Next

            For Each s As String In DN.Keys
                N(s) = N0(s) + DN(s)
            Next

            Dim fw(c), fm(c), sumfm, sum1, sumn, sumw As Double

            N.Values.CopyTo(fm, 0)

            sumfm = Sum(fm) + Ninerts

            sum1 = 0
            sumn = 0
            For Each s As Substancia In tms.Fases(0).Componentes.Values
                If Me.ComponentIDs.Contains(s.Nome) Then
                    s.MolarFlow = N(s.Nome)
                    s.FracaoMolar = N(s.Nome) / sumfm
                    sum1 += N(s.Nome) * s.ConstantProperties.Molar_Weight / 1000
                Else
                    s.FracaoMolar = s.MolarFlow / sumfm
                End If
                sumn += s.MolarFlow
            Next

            tms.Fases(0).SPMProperties.molarflow = sumn

            sumw = 0
            For Each s As Substancia In tms.Fases(0).Componentes.Values
                If Me.ComponentIDs.Contains(s.Nome) Then
                    s.MassFlow = N(s.Nome) * s.ConstantProperties.Molar_Weight / 1000
                End If
                s.FracaoMassica = s.MassFlow / (sum1 + Winerts)
                sumw += s.MassFlow
            Next

            tms.Fases(0).SPMProperties.massflow = sumw

            With pp
                .CurrentMaterialStream = tms
                .DW_CalcEquilibrium(PropertyPackages.FlashSpec.T, PropertyPackages.FlashSpec.P)
                .DW_CalcPhaseProps(PropertyPackages.Fase.Mixture)
                .DW_CalcPhaseProps(PropertyPackages.Fase.Vapor)
                .DW_CalcPhaseProps(PropertyPackages.Fase.Liquid)
                .DW_CalcCompMolarFlow(-1)
                .DW_CalcCompMassFlow(-1)
                .DW_CalcCompVolFlow(-1)
                .DW_CalcOverallProps()
                .DW_CalcTwoPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid, DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                .DW_CalcVazaoVolumetrica()
                .DW_CalcKvalue()
            End With

            Dim CP(tms.Fases(0).Componentes.Count - 1) As Double
            Dim f(x.Length - 1) As Double

            Dim fugs(tms.Fases(0).Componentes.Count - 1), prod(x.Length - 1) As Double

            i = 0
            For Each s As Substancia In tms.Fases(2).Componentes.Values
                If s.FracaoMolar > 0.0# Then
                    'DGf = pp.AUX_DELGF_T(298.15, T, s.Nome) * s.ConstantProperties.Molar_Weight
                    fugs(i) = s.FugacityCoeff.GetValueOrDefault
                    CP(i) = (fugs(i) * s.FracaoMolar.GetValueOrDefault * P / P0)
                Else
                    fugs(i) = s.FugacityCoeff.GetValueOrDefault
                    CP(i) = (fugs(i) * 0.01 * P / P0)
                End If
                i += 1
            Next

            For i = 0 To Me.Reactions.Count - 1
                prod(i) = 1
                j = 0
                For Each s As Substancia In tms.Fases(2).Componentes.Values
                    With FlowSheet.Options.Reactions(Me.Reactions(i))
                        If .Components.ContainsKey(s.Nome) Then
                            prod(i) *= CP(j) ^ .Components(s.Nome).StoichCoeff
                        End If
                    End With
                    j += 1
                Next
            Next

            Dim pen_val As Double = ReturnPenaltyValue()

            CheckCalculatorStatus()

            For i = 0 To Me.Reactions.Count - 1
                With FlowSheet.Options.Reactions(Me.Reactions(i))
                    f(i) = prod(i) - .ConstantKeqValue
                    If Double.IsNaN(f(i)) Or Double.IsInfinity(f(i)) Then
                        f(i) = pen_val
                    End If
                End With
            Next

            Return f

        End Function

        Private Function FunctionValue2G(ByVal x() As Double) As Double

            Dim pp As SimulationObjects.PropertyPackages.PropertyPackage = Me.PropertyPackage

            i = 0
            For Each s As String In N.Keys
                DN(s) = 0
                For j = 0 To r
                    DN(s) += E(i, j) * x(j)
                Next
                i += 1
            Next

            For Each s As String In DN.Keys
                N(s) = N0(s) + DN(s)
            Next

            Dim fw(c), fm(c), sumfm, sum1, sumn, sumw As Double

            N.Values.CopyTo(fm, 0)

            sumfm = Sum(fm) + Ninerts

            sum1 = 0
            sumn = 0
            For Each s As Substancia In tms.Fases(0).Componentes.Values
                If Me.ComponentIDs.Contains(s.Nome) Then
                    s.MolarFlow = N(s.Nome)
                    s.FracaoMolar = N(s.Nome) / sumfm
                    sum1 += N(s.Nome) * s.ConstantProperties.Molar_Weight / 1000
                Else
                    s.FracaoMolar = s.MolarFlow / sumfm
                End If
                sumn += s.MolarFlow
            Next

            tms.Fases(0).SPMProperties.molarflow = sumn

            sumw = 0
            For Each s As Substancia In tms.Fases(0).Componentes.Values
                If Me.ComponentIDs.Contains(s.Nome) Then
                    s.MassFlow = N(s.Nome) * s.ConstantProperties.Molar_Weight / 1000
                End If
                s.FracaoMassica = s.MassFlow / (sum1 + Winerts)
                sumw += s.MassFlow
            Next

            tms.Fases(0).SPMProperties.massflow = sumw

            With pp
                .CurrentMaterialStream = tms
                .DW_CalcEquilibrium(PropertyPackages.FlashSpec.T, PropertyPackages.FlashSpec.P)
                .DW_CalcPhaseProps(PropertyPackages.Fase.Mixture)
                .DW_CalcPhaseProps(PropertyPackages.Fase.Vapor)
                .DW_CalcPhaseProps(PropertyPackages.Fase.Liquid)
                .DW_CalcCompMolarFlow(-1)
                .DW_CalcCompMassFlow(-1)
                .DW_CalcCompVolFlow(-1)
                .DW_CalcOverallProps()
                .DW_CalcTwoPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid, DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                .DW_CalcVazaoVolumetrica()
                .DW_CalcKvalue()
            End With

            Dim fugs(tms.Fases(0).Componentes.Count - 1) As Double
            Dim CP(tms.Fases(0).Componentes.Count - 1) As Double
            Dim DGf As Double

            i = 0
            For Each s As Substancia In tms.Fases(2).Componentes.Values
                If s.FracaoMolar <> 0.0# Then
                    DGf = pp.AUX_DELGF_T(298.15, T, s.Nome) * s.ConstantProperties.Molar_Weight
                    fugs(i) = s.FugacityCoeff.GetValueOrDefault
                    CP(i) = s.FracaoMolar * (DGf + Log(fugs(i) * s.FracaoMolar.GetValueOrDefault * P / P0))
                Else
                    CP(i) = 0
                End If
                i += 1
            Next

            Dim pen_val As Double = ReturnPenaltyValue()

            CheckCalculatorStatus()

            Dim gibbs As Double = MathEx.Common.Sum(CP) * sumn * 8.314 * T

            Return gibbs

        End Function

        Private Function FunctionGradient2N(ByVal x() As Double) As Double(,)

            Dim epsilon As Double = 0.0001

            Dim f1(), f2() As Double
            Dim g(x.Length - 1, x.Length - 1), x2(x.Length - 1) As Double
            Dim i, j, k As Integer

            f1 = FunctionValue2N(x)
            For i = 0 To x.Length - 1
                For j = 0 To x.Length - 1
                    If i <> j Then
                        x2(j) = x(j)
                    Else
                        x2(j) = x(j) * (1 + epsilon)
                    End If
                Next
                f2 = FunctionValue2N(x2)
                For k = 0 To x.Length - 1
                    g(k, i) = (f2(k) - f1(k)) / (x2(i) - x(i))
                Next
            Next

            Return g

        End Function

        Public Function MinimizeError(ByVal t As Double) As Double

            Dim tmpx0 As Double() = tmpx.Clone

            For i = 0 To comps + els
                tmpx0(i) -= tmpdx(i) * t
                'If tmpx0(i) < 0 And i <= comps Then tmpx0(i) = 0.000001
            Next

            Dim abssum0 = AbsSum(FunctionValue2N(tmpx0))
            Return abssum0

        End Function

        Private Function ReturnPenaltyValue() As Double

            'calculate penalty functions for constraint variables

            Dim i As Integer
            Dim n As Integer = tms.Fases(0).Componentes.Count - 1

            Dim con_lc(n), con_uc(n), con_val(n) As Double
            Dim pen_val As Double = 0
            Dim delta1, delta2 As Double

            i = 0
            For Each comp As Substancia In tms.Fases(0).Componentes.Values
                con_lc(i) = 0
                con_uc(i) = 1
                con_val(i) = comp.FracaoMolar.GetValueOrDefault
                i += 1
            Next

            pen_val = 0
            For i = 0 To n
                delta1 = con_val(i) - con_lc(i)
                delta2 = con_val(i) - con_uc(i)
                If delta1 < 0 Then
                    pen_val += -delta1 * 100000000000
                ElseIf delta2 > 1 Then
                    pen_val += -delta2 * 100000000000
                Else
                    pen_val += 0
                End If
            Next

            If Double.IsNaN(pen_val) Then pen_val = 0

            Return pen_val

        End Function

#End Region

        Public Sub New()
            MyBase.New()
        End Sub

        Public Sub New(ByVal nome As String, ByVal descricao As String)

            MyBase.new()
            Me.m_ComponentName = nome
            Me.m_ComponentDescription = descricao
            Me.FillNodeItems()
            Me.QTFillNodeItems()
            Me.ShowQuickTable = False
            Me._rex_iest = New ArrayList()
            Me._components = New List(Of String)

        End Sub

        Public Overrides Sub Validate()

            'MyBase.Validate()
            Dim objargs As New DWSIM.Outros.StatusChangeEventArgs

            If Not Me.GraphicObject.InputConnectors(0).IsAttached Then
                'Call function to calculate flowsheet
                With objargs
                    .Calculado = False
                    .Nome = Me.Nome
                    .Tipo = TipoObjeto.RCT_Equilibrium
                End With
                CalculateFlowsheet(FlowSheet, objargs, Nothing)
                Throw New Exception(DWSIM.App.GetLocalString("Nohcorrentedematriac10"))
            ElseIf Not Me.GraphicObject.OutputConnectors(0).IsAttached Then
                'Call function to calculate flowsheet
                With objargs
                    .Calculado = False
                    .Nome = Me.Nome
                    .Tipo = TipoObjeto.RCT_Equilibrium
                End With
                CalculateFlowsheet(FlowSheet, objargs, Nothing)
                Throw New Exception(DWSIM.App.GetLocalString("Nohcorrentedematriac11"))
            ElseIf Not Me.GraphicObject.OutputConnectors(1).IsAttached Then
                'Call function to calculate flowsheet
                With objargs
                    .Calculado = False
                    .Nome = Me.Nome
                    .Tipo = TipoObjeto.RCT_Equilibrium
                End With
                CalculateFlowsheet(FlowSheet, objargs, Nothing)
                Throw New Exception(DWSIM.App.GetLocalString("Nohcorrentedematriac11"))
            ElseIf Not Me.GraphicObject.OutputConnectors(0).IsAttached Then
                'Call function to calculate flowsheet
                With objargs
                    .Calculado = False
                    .Nome = Me.Nome
                    .Tipo = TipoObjeto.RCT_Equilibrium
                End With
                CalculateFlowsheet(FlowSheet, objargs, Nothing)
                Throw New Exception(DWSIM.App.GetLocalString("Verifiqueasconexesdo"))
            ElseIf Not Me.GraphicObject.OutputConnectors(1).IsAttached Then
                'Call function to calculate flowsheet
                With objargs
                    .Calculado = False
                    .Nome = Me.Nome
                    .Tipo = TipoObjeto.RCT_Equilibrium
                End With
                CalculateFlowsheet(FlowSheet, objargs, Nothing)
                Throw New Exception(DWSIM.App.GetLocalString("Verifiqueasconexesdo"))
            ElseIf Not Me.GraphicObject.InputConnectors(0).IsAttached Then
                'Call function to calculate flowsheet
                With objargs
                    .Calculado = False
                    .Nome = Me.Nome
                    .Tipo = TipoObjeto.RCT_Equilibrium
                End With
                CalculateFlowsheet(FlowSheet, objargs, Nothing)
                Throw New Exception(DWSIM.App.GetLocalString("Verifiqueasconexesdo"))
            End If

        End Sub

        Public Overrides Function Calculate(Optional ByVal args As Object = Nothing) As Integer

            If Me.Conversions Is Nothing Then Me.m_conversions = New Dictionary(Of String, Double)
            If Me.ReactionExtents Is Nothing Then Me.m_reactionextents = New Dictionary(Of String, Double)
            If Me.ReactionExtentsEstimates Is Nothing Then Me._rex_iest = New ArrayList
            If Me.ComponentConversions Is Nothing Then Me.m_componentconversions = New Dictionary(Of String, Double)

            Me.Validate()

            Dim form As FormFlowsheet = My.Application.ActiveSimulation

            Dim objargs As New DWSIM.Outros.StatusChangeEventArgs

            Me.Reactions.Clear()
            Me.ReactionExtents.Clear()
            Me.Conversions.Clear()
            Me.ComponentConversions.Clear()
            Me.DeltaQ = 0
            Me.DeltaT = 0

            Dim rx As Reaction
            Dim ims As DWSIM.SimulationObjects.Streams.MaterialStream = form.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.InputConnectors(0).AttachedConnector.AttachedFrom.Name).Clone
            Dim pp As DWSIM.SimulationObjects.PropertyPackages.PropertyPackage = Me.PropertyPackage
            Dim ppr As New DWSIM.SimulationObjects.PropertyPackages.RaoultPropertyPackage()

            'Reactants Enthalpy (kJ/kg * kg/s = kW) (ISOTHERMIC)
            Dim Hr0 As Double
            Hr0 = ims.Fases(0).SPMProperties.enthalpy.GetValueOrDefault * ims.Fases(0).SPMProperties.massflow.GetValueOrDefault

            Dim tmp As Object
            'Copy results to upstream MS
            Dim xl, xv, H, S, wtotalx, wtotaly As Double
            pp.CurrentMaterialStream = ims

            T = ims.Fases(0).SPMProperties.temperature.GetValueOrDefault
            P = ims.Fases(0).SPMProperties.pressure.GetValueOrDefault
            P0 = 101325

            Dim rxn As Reaction

            'check active reactions (equilibrium only) in the reaction set
            For Each rxnsb As ReactionSetBase In form.Options.ReactionSets(Me.ReactionSetID).Reactions.Values
                If form.Options.Reactions(rxnsb.ReactionID).ReactionType = ReactionType.Equilibrium And rxnsb.IsActive Then
                    Me.Reactions.Add(rxnsb.ReactionID)
                    Me.ReactionExtents.Add(rxnsb.ReactionID, 0)

                    rxn = form.Options.Reactions(rxnsb.ReactionID)

                    'equilibrium constant calculation
                    Select Case rxn.KExprType
                        Case Reaction.KOpt.Constant
                            'rxn.ConstantKeqValue = rxn.ConstantKeqValue
                        Case Reaction.KOpt.Expression
                            If rxn.ExpContext Is Nothing Then
                                rxn.ExpContext = New Ciloci.Flee.ExpressionContext
                                With rxn.ExpContext
                                    .Imports.ImportStaticMembers(GetType(System.Math))
                                    .Variables.DefineVariable("T", GetType(Double))
                                End With
                            End If
                            rxn.ExpContext.Variables.SetVariableValue("T", T)
                            rxn.Expr = ExpressionFactory.CreateGeneric(Of Double)(rxn.Expression, rxn.ExpContext)
                            rxn.ConstantKeqValue = Exp(rxn.Expr.Evaluate)
                        Case Reaction.KOpt.Gibbs
                            Dim id(rxn.Components.Count - 1) As String
                            Dim stcoef(rxn.Components.Count - 1) As Double
                            Dim bcidx As Integer = 0
                            j = 0
                            For Each sb As ReactionStoichBase In rxn.Components.Values
                                id(j) = sb.CompName
                                stcoef(j) = sb.StoichCoeff
                                If sb.IsBaseReactant Then bcidx = j
                                j += 1
                            Next
                            Dim DelG_RT = pp.AUX_DELGig_RT(298.15, T, id, stcoef, bcidx)
                            rxn.ConstantKeqValue = Exp(-DelG_RT)
                    End Select

                End If
            Next

            T = ims.Fases(0).SPMProperties.temperature.GetValueOrDefault
            P = ims.Fases(0).SPMProperties.pressure.GetValueOrDefault
            P0 = 101325

            pp.CurrentMaterialStream = ims
            ppr.CurrentMaterialStream = ims

            'initial estimates for reaction extents

            tms = ims.Clone()

            Me.ComponentConversions.Clear()
            Me.ComponentIDs.Clear()

            'r: number of reactions
            'c: number of components
            'i,j: iterators

            i = 0
            For Each rxid As String In Me.Reactions
                rx = FlowSheet.Options.Reactions(rxid)
                j = 0
                For Each comp As ReactionStoichBase In rx.Components.Values
                    If Not Me.ComponentIDs.Contains(comp.CompName) Then
                        Me.ComponentIDs.Add(comp.CompName)
                        Me.ComponentConversions.Add(comp.CompName, 0)
                    End If
                    j += 1
                Next
                i += 1
            Next

            r = Me.Reactions.Count - 1
            c = Me.ComponentIDs.Count - 1

            ReDim E(c, r)


            'E: matrix of stoichometric coefficients
            i = 0
            For Each rxid As String In Me.Reactions
                rx = FlowSheet.Options.Reactions(rxid)
                j = 0
                For Each cname As String In Me.ComponentIDs
                    If rx.Components.ContainsKey(cname) Then
                        E(j, i) = rx.Components(cname).StoichCoeff
                    Else
                        E(j, i) = 0
                    End If
                    j += 1
                Next
                i += 1
            Next

            Dim fm0(c), N0tot, W0tot, wm0 As Double

            N0.Clear()
            DN.Clear()
            N.Clear()

            For Each cname As String In Me.ComponentIDs
                N0.Add(cname, ims.Fases(0).Componentes(cname).MolarFlow.GetValueOrDefault)
                DN.Add(cname, 0)
                N.Add(cname, ims.Fases(0).Componentes(cname).MolarFlow.GetValueOrDefault)
                wm0 += ims.Fases(0).Componentes(cname).MassFlow.GetValueOrDefault
            Next

            N0.Values.CopyTo(fm0, 0)

            N0tot = ims.Fases(0).SPMProperties.molarflow.GetValueOrDefault
            W0tot = ims.Fases(0).SPMProperties.massflow.GetValueOrDefault

            Ninerts = N0tot - Sum(fm0)
            Winerts = W0tot - wm0

            Dim lbound(Me.ReactionExtents.Count - 1) As Double
            Dim ubound(Me.ReactionExtents.Count - 1) As Double
            Dim var1 As Double

            i = 0
            For Each rxid As String In Me.Reactions
                rx = FlowSheet.Options.Reactions(rxid)
                j = 0
                For Each comp As ReactionStoichBase In rx.Components.Values
                    var1 = -N0(comp.CompName) / comp.StoichCoeff
                    If j = 0 Then
                        lbound(i) = var1
                        ubound(i) = var1
                    Else
                        If var1 < lbound(i) Then lbound(i) = var1
                        If var1 > ubound(i) Then ubound(i) = var1
                    End If
                    j += 1
                Next
                i += 1
            Next

            Dim REx(r) As Double

            For i = 0 To r
                REx(i) = (lbound(i) + ubound(i)) / 2 'Me.ReactionExtentsEstimates(i)
            Next

            Dim g0, g1 As Double

            Dim REx0(REx.Length - 1) As Double

            g0 = FunctionValue2G(REx0)

            Me.InitialGibbsEnergy = g0

            'solve using newton's method

            Dim fx(r), dfdx(r, r), dx(r), x(r), df, fval As Double
            Dim brentsolver As New BrentOpt.BrentMinimize
            brentsolver.DefineFuncDelegate(AddressOf MinimizeError)

            Dim niter As Integer

            x = REx
            niter = 0
            Do

                fx = Me.FunctionValue2N(x)
                dfdx = Me.FunctionGradient2N(x)

                Dim success As Boolean
                success = MathEx.SysLin.rsolve.rmatrixsolve(dfdx, fx, r + 1, dx)

                tmpx = x
                tmpdx = dx
                df = 1
                fval = brentsolver.brentoptimize(0.000000000001, 1.5, 0.0001, df)

                For i = 0 To r
                    x(i) -= dx(i) * df
                Next

                niter += 1

            Loop Until AbsSum(fx) < 0.001 Or niter > 99

            If niter > 99 Then
                Throw New Exception("Maximum number of iterations reached.")
            End If
            'reevaluate function

            g1 = FunctionValue2G(REx)

            Me.FinalGibbsEnergy = g1

            i = 0
            For Each r As String In Me.Reactions
                Me.ReactionExtents(r) = REx(i)
                i += 1
            Next

            Dim DHr, Hid_r, Hid_p, Hp As Double

            DHr = 0

            i = 0
            Do
                'process reaction i
                rx = FlowSheet.Options.Reactions(Me.Reactions(i))

                Dim id(rx.Components.Count - 1) As String
                Dim stcoef(rx.Components.Count - 1) As Double
                Dim bcidx As Integer = 0
                j = 0
                For Each sb As ReactionStoichBase In rx.Components.Values
                    id(j) = sb.CompName
                    stcoef(j) = sb.StoichCoeff
                    If sb.IsBaseReactant Then bcidx = j
                    j += 1
                Next

                'Heat released (or absorbed) (kJ/s = kW) (Ideal Gas)
                DHr += rx.ReactionHeat * Me.ReactionExtents(Me.Reactions(i)) * rx.Components(rx.BaseReactant).StoichCoeff / 1000
                'DHfT = pp.AUX_DELHig_RT(298.15, 298.15, id, stcoef, bcidx)
                'DHr += DHfT * Me.ReactionExtents(Me.Reactions(i)) * rx.Components(rx.BaseReactant).StoichCoeff / 1000
                i += 1
            Loop Until i = Me.Reactions.Count

            'Ideal Gas Reactants Enthalpy (kJ/kg * kg/s = kW)
            Hid_r += 0 'ppr.RET_Hid(298.15, ims.Fases(0).SPMProperties.temperature.GetValueOrDefault, PropertyPackages.Fase.Mixture) * ims.Fases(0).SPMProperties.massflow.GetValueOrDefault

            ' comp. conversions
            For Each sb As Substancia In ims.Fases(0).Componentes.Values
                If Me.ComponentConversions.ContainsKey(sb.Nome) Then
                    Me.ComponentConversions(sb.Nome) = -DN(sb.Nome) / N0(sb.Nome)
                End If
            Next

            'Check to see if are negative molar fractions.
            Dim sum1 As Double = 0
            For Each subst As Substancia In tms.Fases(0).Componentes.Values
                If subst.FracaoMolar.GetValueOrDefault < 0 Then
                    subst.MolarFlow = 0
                Else
                    sum1 += subst.MolarFlow.GetValueOrDefault
                End If
            Next
            For Each subst As Substancia In tms.Fases(0).Componentes.Values
                subst.FracaoMolar = subst.MolarFlow.GetValueOrDefault / sum1
            Next

            ims = tms.Clone

            Select Case Me.ReactorOperationMode

                Case OperationMode.Adiabatic

                    Me.DeltaQ = form.Collections.CLCS_EnergyStreamCollection(Me.GraphicObject.InputConnectors(1).AttachedConnector.AttachedFrom.Name).Energia.GetValueOrDefault

                    'Products Enthalpy (kJ/kg * kg/s = kW)
                    Hp = Me.DeltaQ.GetValueOrDefault + Hr0 + Hid_p - Hid_r + DHr

                    tmp = Me.PropertyPackage.DW_CalcEquilibrio_ISOL(PropertyPackages.FlashSpec.P, PropertyPackages.FlashSpec.H, P, Hp / ims.Fases(0).SPMProperties.massflow.GetValueOrDefault, 0)
                    Dim Tout As Double = tmp(2)

                    Me.DeltaT = Tout - T
                    ims.Fases(0).SPMProperties.temperature = Tout
                    T = ims.Fases(0).SPMProperties.temperature.GetValueOrDefault

                    With pp
                        .CurrentMaterialStream = ims
                        'Calcular corrente de matéria com T e P
                        '.DW_CalcVazaoMolar()
                        .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.T, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P)
                        If ims.Fases(1).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                            .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid)
                        Else
                            .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid)
                        End If
                        If ims.Fases(2).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                            .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                        Else
                            .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                        End If
                        .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Mixture)
                        .DW_CalcOverallProps()
                        .DW_CalcTwoPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid, DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                        .DW_CalcCompMassFlow(-1)
                        .DW_CalcCompMolarFlow(-1)
                        .DW_CalcCompVolFlow(-1)
                        .DW_CalcVazaoVolumetrica()

                    End With

                Case OperationMode.Isothermic

                    With pp
                        .CurrentMaterialStream = ims
                        'Calcular corrente de matéria com T e P
                        '.DW_CalcVazaoMolar()
                        .DW_CalcEquilibrium(DWSIM.SimulationObjects.PropertyPackages.FlashSpec.T, DWSIM.SimulationObjects.PropertyPackages.FlashSpec.P)
                        If ims.Fases(1).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                            .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid)
                        Else
                            .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid)
                        End If
                        If ims.Fases(2).SPMProperties.molarfraction.GetValueOrDefault > 0 Then
                            .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                        Else
                            .DW_ZerarPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                        End If
                        .DW_CalcPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Mixture)
                        .DW_CalcOverallProps()
                        .DW_CalcTwoPhaseProps(DWSIM.SimulationObjects.PropertyPackages.Fase.Liquid, DWSIM.SimulationObjects.PropertyPackages.Fase.Vapor)
                        .DW_CalcCompMassFlow(-1)
                        .DW_CalcCompMolarFlow(-1)
                        .DW_CalcCompVolFlow(-1)
                        .DW_CalcVazaoVolumetrica()

                    End With

                    'Products Enthalpy (kJ/kg * kg/s = kW)
                    Hp = ims.Fases(0).SPMProperties.enthalpy.GetValueOrDefault * ims.Fases(0).SPMProperties.massflow.GetValueOrDefault

                    'Heat (kW)
                    Me.DeltaQ = Me.DeltaQ.GetValueOrDefault - DHr + Hid_r + Hp - Hr0 - Hid_p

                    Me.DeltaT = 0

            End Select

            Dim W As Double = ims.Fases(0).SPMProperties.massflow.GetValueOrDefault

            'do a flash calc (calculate final temperature/enthalpy)
            tmp = pp.DW_CalcEquilibrio_ISOL(PropertyPackages.FlashSpec.T, PropertyPackages.FlashSpec.P, ims.Fases(0).SPMProperties.temperature.GetValueOrDefault, ims.Fases(0).SPMProperties.pressure.GetValueOrDefault, 0)

            'Return New Object() {xl, xv, T, P, H, S, 1, 1, Vx, Vy}
            Dim Vx(ims.Fases(0).Componentes.Count - 1), Vy(ims.Fases(0).Componentes.Count - 1), Vwx(ims.Fases(0).Componentes.Count - 1), Vwy(ims.Fases(0).Componentes.Count - 1) As Double
            xl = tmp(0)
            xv = tmp(1)
            T = tmp(2)
            P = tmp(3)
            H = tmp(4)
            S = tmp(5)
            Vx = tmp(8)
            Vy = tmp(9)

            Dim ms As DWSIM.SimulationObjects.Streams.MaterialStream
            Dim cp As ConnectionPoint
            cp = Me.GraphicObject.InputConnectors(0)
            If cp.IsAttached Then
                ms = form.Collections.CLCS_MaterialStreamCollection(cp.AttachedConnector.AttachedFrom.Name)
                Dim comp As DWSIM.ClassesBasicasTermodinamica.Substancia
                i = 0
                For Each comp In ms.Fases(0).Componentes.Values
                    wtotalx += Vx(i) * comp.ConstantProperties.Molar_Weight
                    wtotaly += Vy(i) * comp.ConstantProperties.Molar_Weight
                    i += 1
                Next
                i = 0
                For Each comp In ms.Fases(0).Componentes.Values
                    Vwx(i) = Vx(i) * comp.ConstantProperties.Molar_Weight / wtotalx
                    Vwy(i) = Vy(i) * comp.ConstantProperties.Molar_Weight / wtotaly
                    i += 1
                Next
            End If

            cp = Me.GraphicObject.OutputConnectors(0)
            If cp.IsAttached Then
                ms = form.Collections.CLCS_MaterialStreamCollection(cp.AttachedConnector.AttachedTo.Name)
                With ms
                    .Fases(0).SPMProperties.temperature = T
                    .Fases(0).SPMProperties.pressure = P
                    .Fases(0).SPMProperties.enthalpy = H * (wtotaly * xv / (wtotaly * xv + wtotalx * xl))
                    Dim comp As DWSIM.ClassesBasicasTermodinamica.Substancia
                    j = 0
                    For Each comp In .Fases(0).Componentes.Values
                        comp.FracaoMolar = Vy(j)
                        comp.FracaoMassica = Vwy(j)
                        j += 1
                    Next
                    j = 0
                    For Each comp In .Fases(2).Componentes.Values
                        comp.FracaoMolar = Vy(j)
                        comp.FracaoMassica = Vwy(j)
                        j += 1
                    Next
                    .Fases(0).SPMProperties.massflow = W * (wtotaly * xv / (wtotaly * xv + wtotalx * xl))
                    .Fases(0).SPMProperties.massfraction = (wtotaly * xv / (wtotaly * xv + wtotalx * xl))
                    .Fases(0).SPMProperties.molarfraction = 1
                    .Fases(3).SPMProperties.massfraction = 0
                    .Fases(3).SPMProperties.molarfraction = 0
                    .Fases(2).SPMProperties.massfraction = 1
                    .Fases(2).SPMProperties.molarfraction = 1
                End With
            End If

            cp = Me.GraphicObject.OutputConnectors(1)
            If cp.IsAttached Then
                ms = form.Collections.CLCS_MaterialStreamCollection(cp.AttachedConnector.AttachedTo.Name)
                With ms
                    .Fases(0).SPMProperties.temperature = T
                    .Fases(0).SPMProperties.pressure = P
                    .Fases(0).SPMProperties.enthalpy = H * (wtotalx * xl / (wtotaly * xv + wtotalx * xl))
                    Dim comp As DWSIM.ClassesBasicasTermodinamica.Substancia
                    j = 0
                    For Each comp In .Fases(0).Componentes.Values
                        comp.FracaoMolar = Vx(j)
                        comp.FracaoMassica = Vwx(j)
                        j += 1
                    Next
                    j = 0
                    For Each comp In .Fases(3).Componentes.Values
                        comp.FracaoMolar = Vx(j)
                        comp.FracaoMassica = Vwx(j)
                        j += 1
                    Next
                    .Fases(0).SPMProperties.massflow = W * (wtotalx * xl / (wtotaly * xv + wtotalx * xl))
                    .Fases(0).SPMProperties.massfraction = (wtotalx * xl / (wtotaly * xv + wtotalx * xl))
                    .Fases(0).SPMProperties.molarfraction = 1
                    .Fases(3).SPMProperties.massfraction = 1
                    .Fases(3).SPMProperties.molarfraction = 1
                    .Fases(2).SPMProperties.massfraction = 0
                    .Fases(2).SPMProperties.molarfraction = 0
                End With
            End If

            'Corrente de energia - atualizar valor da potência (kJ/s)
            With form.Collections.CLCS_EnergyStreamCollection(Me.GraphicObject.InputConnectors(1).AttachedConnector.AttachedFrom.Name)
                .Energia = Me.DeltaQ.GetValueOrDefault
                .GraphicObject.Calculated = True
            End With

            'Call function to calculate flowsheet
            With objargs
                .Calculado = True
                .Nome = Me.Nome
                .Tag = Me.GraphicObject.Tag
                .Tipo = TipoObjeto.RCT_Equilibrium
            End With

            form.CalculationQueue.Enqueue(objargs)

        End Function

        Public Overrides Function DeCalculate() As Integer

            'If Not Me.GraphicObject.InputConnectors(0).IsAttached Then Throw New Exception(DWSIM.App.GetLocalString("Nohcorrentedematriac10"))
            'If Not Me.GraphicObject.OutputConnectors(0).IsAttached Then Throw New Exception(DWSIM.App.GetLocalString("Nohcorrentedematriac11"))
            'If Not Me.GraphicObject.OutputConnectors(1).IsAttached Then Throw New Exception(DWSIM.App.GetLocalString("Nohcorrentedematriac11"))

            Dim form As Global.DWSIM.FormFlowsheet = My.Application.ActiveSimulation

            'Dim ems As DWSIM.SimulationObjects.Streams.MaterialStream = form.Collections.CLCS_MaterialStreamCollection(Me.GraphicObject.InputConnectors(0).AttachedConnector.AttachedFrom.Name)
            'Dim W As Double = ems.Fases(0).SPMProperties.massflow.GetValueOrDefault
            'Dim j As Integer = 0

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
                .Tipo = TipoObjeto.RCT_Equilibrium
            End With

            form.CalculationQueue.Enqueue(objargs)

        End Function

        Public Overrides Sub QTFillNodeItems()

        End Sub

        Public Overrides Sub UpdatePropertyNodes(ByVal su As SistemasDeUnidades.Unidades, ByVal nf As String)

        End Sub

        Public Overrides Sub PopulatePropertyGrid(ByRef pgrid As PropertyGridEx.PropertyGridEx, ByVal su As SistemasDeUnidades.Unidades)

            Dim Conversor As New DWSIM.SistemasDeUnidades.Conversor

            With pgrid

                .PropertySort = PropertySort.Categorized
                .ShowCustomProperties = True
                .Item.Clear()

                MyBase.PopulatePropertyGrid(pgrid, su)

                Dim ent, saida1, saida2, energ As String
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
                    energ = Me.GraphicObject.InputConnectors(1).AttachedConnector.AttachedFrom.Tag
                Else
                    energ = ""
                End If

                .Item.Add(DWSIM.App.GetLocalString("Correntedeentrada"), ent, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
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

                .Item.Add(DWSIM.App.GetLocalString("Correntedeenergia"), energ, False, DWSIM.App.GetLocalString("Conexes1"), "", True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .CustomEditor = New DWSIM.Editors.Streams.UIInputESSelector
                End With

                .Item.Add(DWSIM.App.GetLocalString("RConvPGridItem1"), FlowSheet.Options.ReactionSets(Me.ReactionSetID).Name, False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("RConvPGridItem1Help"), True)
                With .Item(.Item.Count - 1)
                    .CustomEditor = New DWSIM.Editors.Reactors.UIReactionSetSelector
                    .IsDropdownResizable = True
                End With

                .Item.Add(DWSIM.App.GetLocalString("RConvPGridItem2"), Me, "ReactorOperationMode", False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("RConvPGridItem2Help"), True)
                With .Item(.Item.Count - 1)
                    .IsBrowsable = False
                End With

                Dim valor = Format(Conversor.ConverterDoSI(su.spmp_deltaP, Me.DeltaP.GetValueOrDefault), FlowSheet.Options.NumberFormat)
                .Item.Add(FT(DWSIM.App.GetLocalString("Quedadepresso"), su.spmp_deltaP), valor, False, DWSIM.App.GetLocalString("Parmetrosdeclculo2"), DWSIM.App.GetLocalString("Quedadepressoaplicad6"), True)
                With .Item(.Item.Count - 1)
                    .DefaultValue = Nothing
                    .DefaultType = GetType(Nullable(Of Double))
                End With

                If Me.GraphicObject.Calculated Then

                    .Item.Add(FT(DWSIM.App.GetLocalString("DeltaT2"), su.spmp_deltaT), Format(Conversor.ConverterDoSI(su.spmp_deltaT, Me.DeltaT.GetValueOrDefault), FlowSheet.Options.NumberFormat), True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("Diferenadetemperatur"), True)
                    With .Item(.Item.Count - 1)
                        .DefaultValue = Nothing
                        .DefaultType = GetType(Nullable(Of Double))
                    End With

                    .Item.Add(FT(DWSIM.App.GetLocalString("RConvPGridItem3"), su.spmp_heatflow), Format(Conversor.ConverterDoSI(su.spmp_heatflow, Me.DeltaQ.GetValueOrDefault), FlowSheet.Options.NumberFormat), True, DWSIM.App.GetLocalString("Resultados3"), "", True)
                    With .Item(.Item.Count - 1)
                        .DefaultValue = Nothing
                        .DefaultType = GetType(Nullable(Of Double))
                    End With

                    .Item.Add(FT(DWSIM.App.GetLocalString("RGInitialG"), su.spmp_heatflow), Format(Conversor.ConverterDoSI(su.molar_enthalpy, Me.InitialGibbsEnergy), FlowSheet.Options.NumberFormat), True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("RGInitialG_description"), True)
                    .Item.Add(FT(DWSIM.App.GetLocalString("RGFinalG"), su.spmp_heatflow), Format(Conversor.ConverterDoSI(su.molar_enthalpy, Me.FinalGibbsEnergy), FlowSheet.Options.NumberFormat), True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("RGFinalG_description"), True)

                    'CustomPropertyCollection
                    Dim m As New PropertyGridEx.CustomPropertyCollection()
                    For Each dbl As KeyValuePair(Of String, Double) In Me.ComponentConversions
                        valor = Format(dbl.Value * 100, FlowSheet.Options.NumberFormat)
                        m.Add(DWSIM.App.GetComponentName(dbl.Key), valor, False, DWSIM.App.GetLocalString("ComponentesConversoes"), DWSIM.App.GetLocalString("RCSTRPGridItem3Help"), True)
                        m.Item(m.Count - 1).IsReadOnly = True
                        m.Item(m.Count - 1).DefaultValue = Nothing
                        m.Item(m.Count - 1).DefaultType = GetType(Nullable(Of Double))
                    Next

                    .Item.Add(DWSIM.App.GetLocalString("ComponentesConversoes"), m, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("RCSTRPGridItem2Help"), True)
                    With .Item(.Item.Count - 1)
                        .IsReadOnly = True
                        .IsBrowsable = True
                        .BrowsableLabelStyle = PropertyGridEx.BrowsableTypeConverter.LabelStyle.lsEllipsis
                        .CustomEditor = New System.Drawing.Design.UITypeEditor
                    End With

                    'CustomPropertyCollection
                    Dim m2 As New PropertyGridEx.CustomPropertyCollection()
                    For Each dbl As KeyValuePair(Of String, Double) In Me.ReactionExtents
                        valor = Format(dbl.Value, FlowSheet.Options.NumberFormat)
                        m2.Add(FlowSheet.Options.Reactions(dbl.Key).Name, valor, False, DWSIM.App.GetLocalString("CoordenadasReacoes"), DWSIM.App.GetLocalString("REqPGridItem1Help"), True)
                        m2.Item(m2.Count - 1).IsReadOnly = True
                        m2.Item(m2.Count - 1).DefaultValue = Nothing
                        m2.Item(m2.Count - 1).DefaultType = GetType(Nullable(Of Double))
                    Next

                    .Item.Add(DWSIM.App.GetLocalString("CoordenadasReacoes"), m2, True, DWSIM.App.GetLocalString("Resultados3"), DWSIM.App.GetLocalString("REqPGridItem2Help"), True)
                    With .Item(.Item.Count - 1)
                        .IsReadOnly = True
                        .IsBrowsable = True
                        .BrowsableLabelStyle = PropertyGridEx.BrowsableTypeConverter.LabelStyle.lsEllipsis
                        .CustomEditor = New System.Drawing.Design.UITypeEditor
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
                    'PROP_HT_0	Pressure Drop
                    value = cv.ConverterDoSI(su.spmp_deltaP, Me.DeltaP.GetValueOrDefault)

            End Select

            Return value
        End Function

        Public Overloads Overrides Function GetProperties(ByVal proptype As SimulationObjects_BaseClass.PropertyType) As String()
            Dim i As Integer = 0
            Dim proplist As New ArrayList
            Select Case proptype
                Case PropertyType.RW
                    For i = 0 To 0
                        proplist.Add("PROP_EQ_" + CStr(i))
                    Next
                Case PropertyType.WR
                    For i = 0 To 0
                        proplist.Add("PROP_EQ_" + CStr(i))
                    Next
                Case PropertyType.ALL
                    For i = 0 To 0
                        proplist.Add("PROP_EQ_" + CStr(i))
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
                    'PROP_HT_0	Pressure Drop
                    Me.DeltaP = cv.ConverterParaSI(su.spmp_deltaP, propval)

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
                    'PROP_HT_0	Pressure Drop
                    value = su.spmp_deltaP

            End Select

            Return value
        End Function

    End Class

End Namespace


