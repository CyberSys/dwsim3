﻿'    Flow Package Base Class
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

Namespace DWSIM.FlowPackages

    Public MustInherit Class FPBaseClass

        Sub New()

        End Sub

        Public MustOverride Function CalculateDeltaP(ByVal D, ByVal L, ByVal deltaz, ByVal k, ByVal qv, ByVal ql, ByVal muv, ByVal mul, ByVal rhov, ByVal rhol, ByVal surft) As Object

    End Class

End Namespace