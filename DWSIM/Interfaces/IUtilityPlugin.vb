﻿'    DWSIM Interface definitions
'    Copyright 2010 Daniel Wagner O. de Medeiros
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

'Revision history:
'27/09/2010 - added new property - Display Mode 
'20/08/2010 - initial release

Namespace Interfaces

    Public Interface IUtilityPlugin

        ReadOnly Property Name() As String
        ReadOnly Property Description() As String
        ReadOnly Property Author() As String
        ReadOnly Property ContactInfo() As String
        ReadOnly Property WebSite() As String
        ReadOnly Property UniqueID() As String

        ReadOnly Property UtilityForm() As Form
        ReadOnly Property CurrentFlowsheet() As FormFlowsheet

        Function SetFlowsheet(ByRef form As FormFlowsheet) As Boolean

        ReadOnly Property DisplayMode() As DispMode

        Enum DispMode
            Modal = 0
            Normal = 1
            Dockable = 2
        End Enum

    End Interface

End Namespace
