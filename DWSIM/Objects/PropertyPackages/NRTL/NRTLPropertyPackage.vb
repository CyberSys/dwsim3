'    NRTL Property Package 
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

Imports DWSIM.DWSIM.SimulationObjects.PropertyPackages

Namespace DWSIM.SimulationObjects.PropertyPackages

    <System.Runtime.InteropServices.Guid(NRTLPropertyPackage.ClassId)> _
   <System.Serializable()> Public Class NRTLPropertyPackage

        Inherits DWSIM.SimulationObjects.PropertyPackages.ActivityCoefficientPropertyPackage

        Public Shadows Const ClassId As String = "D42F0157-5750-4c89-A94E-634A04701568"

        Public Property m_uni As Auxiliary.NRTL
            Get
                Return m_act
            End Get
            Set(value As Auxiliary.NRTL)
                m_act = m_uni
            End Set
        End Property

        Public Sub New(ByVal comode As Boolean)

            MyBase.New(comode)

            Me.m_act = New Auxiliary.NRTL

        End Sub

        Public Sub New()

            MyBase.New(False)

            Me.m_act = New Auxiliary.NRTL

            Me.IsConfigurable = True
            Me.ConfigForm = New FormConfigNRTL
            Me._packagetype = PropertyPackages.PackageType.ActivityCoefficient

        End Sub

        Public Overrides Sub ReconfigureConfigForm()

            MyBase.ReconfigureConfigForm()
            Me.ConfigForm = New FormConfigNRTL

        End Sub

    End Class

End Namespace


