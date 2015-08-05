﻿'    UNIFAC-LL Property Package 
'    Copyright 2008-2011 Daniel Wagner O. de Medeiros
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

'Imports CAPEOPEN_PD.CAPEOPEN
'Imports DWSIM.SimulationObjects
Imports DWSIM.DWSIM.SimulationObjects.PropertyPackages
Imports System.Math
Imports DWSIM.DWSIM.ClassesBasicasTermodinamica

Namespace DWSIM.SimulationObjects.PropertyPackages

    <System.Runtime.InteropServices.Guid(UNIFACLLPropertyPackage.ClassId)> _
<System.Serializable()> Public Class UNIFACLLPropertyPackage

        Inherits DWSIM.SimulationObjects.PropertyPackages.ActivityCoefficientPropertyPackage

        Public Shadows Const ClassId As String = "E48B580B-82E4-4bd7-841A-647A24A447B5"

        Public Property m_uni As Auxiliary.UnifacLL
            Get
                Return m_act
            End Get
            Set(value As Auxiliary.UnifacLL)
                m_act = m_uni
            End Set
        End Property


        Public Sub New(ByVal comode As Boolean)
            MyBase.New(comode)
            Me.m_act = New Auxiliary.UnifacLL
        End Sub

        Public Sub New()

            MyBase.New(False)

            Me.IsConfigurable = True
            Me.ConfigForm = New FormConfigPP
            Me._packagetype = PropertyPackages.PackageType.ActivityCoefficient

            Me.m_act = New Auxiliary.UnifacLL

        End Sub

        Public Overrides Sub ReconfigureConfigForm()
            MyBase.ReconfigureConfigForm()
            Me.ConfigForm = New FormConfigPP
        End Sub

#Region "    Auxiliary Functions"

        Function RET_VN(ByVal subst As DWSIM.ClassesBasicasTermodinamica.Substancia) As Object

            Return Me.m_uni.RET_VN(subst.ConstantProperties)

        End Function

        Function RET_VQ() As Object

            Dim subst As DWSIM.ClassesBasicasTermodinamica.Substancia
            Dim VQ(Me.CurrentMaterialStream.Fases(0).Componentes.Count - 1) As Double
            Dim i As Integer = 0
            Dim sum As Double = 0

            For Each subst In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                VQ(i) = Me.m_uni.RET_Qi(Me.RET_VN(subst))
                i += 1
            Next

            Return VQ

        End Function

        Function RET_VR() As Object

            Dim subst As DWSIM.ClassesBasicasTermodinamica.Substancia
            Dim VR(Me.CurrentMaterialStream.Fases(0).Componentes.Count - 1) As Double
            Dim i As Integer = 0
            Dim sum As Double = 0

            For Each subst In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                VR(i) = Me.m_uni.RET_Ri(Me.RET_VN(subst))
                i += 1
            Next

            Return VR

        End Function

        Function RET_VEKI() As List(Of Dictionary(Of Integer, Double))

            Dim subst As DWSIM.ClassesBasicasTermodinamica.Substancia
            Dim VEKI As New List(Of Dictionary(Of Integer, Double))
            Dim i As Integer = 0
            Dim sum As Double
            For Each subst In Me.CurrentMaterialStream.Fases(0).Componentes.Values
                sum = 0
                For Each s As String In subst.ConstantProperties.MODFACGroups.Collection.Keys
                    sum += subst.ConstantProperties.MODFACGroups.Collection(s) * Me.m_uni.UnifGroups.Groups(s).Q
                Next
                Dim obj = Me.m_uni.RET_EKI(Me.RET_VN(subst), sum)
                VEKI.Add(obj)
            Next

            Return VEKI

        End Function

#End Region

    End Class

End Namespace

