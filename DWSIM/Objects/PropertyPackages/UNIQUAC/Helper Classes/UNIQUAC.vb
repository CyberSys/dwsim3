'    UNIQUAC Property Package 
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

Imports System.Collections.Generic
Imports FileHelpers

Namespace DWSIM.SimulationObjects.PropertyPackages.Auxiliary

    <DelimitedRecord(";")> <IgnoreFirst()> <System.Serializable()> _
    Public Class UNIQUAC_IPData

        Implements ICloneable

        Public ID1 As Integer = -1
        Public ID2 As Integer = -1
        Public A12 As Double = 0
        Public A21 As Double = 0
        Public comment As String = ""
        <FieldIgnored()> Public B12 As Double = 0
        <FieldIgnored()> Public B21 As Double = 0
        <FieldIgnored()> Public C12 As Double = 0
        <FieldIgnored()> Public C21 As Double = 0

        Public Function Clone() As Object Implements System.ICloneable.Clone

            Dim newclass As New UNIQUAC_IPData
            With newclass
                .ID1 = Me.ID1
                .ID2 = Me.ID2
                .A12 = Me.A12
                .A21 = Me.A21
                .B12 = Me.B12
                .B21 = Me.B21
                .C12 = Me.C12
                .C21 = Me.C21
                .comment = Me.comment
            End With
            Return newclass
        End Function

        Public Function CloneToLIQUAC() As LIQUAC2_IPData

            Dim newclass As New LIQUAC2_IPData
            With newclass
                .ID1 = Me.ID1
                .ID2 = Me.ID2
                .Group1 = .ID1
                .Group2 = .ID2
                .A12 = Me.A12
                .A21 = Me.A21
            End With
            Return newclass
        End Function

    End Class

    <System.Serializable()> Public Class UNIQUAC

        Implements IActivityCoefficientBase

        Private _ip As Dictionary(Of String, Dictionary(Of String, UNIQUAC_IPData))

        Public ReadOnly Property InteractionParameters() As Dictionary(Of String, Dictionary(Of String, UNIQUAC_IPData))
            Get
                Return _ip
            End Get
        End Property

        Sub New()

            _ip = New Dictionary(Of String, Dictionary(Of String, UNIQUAC_IPData))

            Dim pathsep As Char = System.IO.Path.DirectorySeparatorChar

            Dim uniquacip As UNIQUAC_IPData
            Dim uniquacipc() As UNIQUAC_IPData
            Dim uniquacipc2() As UNIQUAC_IPData
            Dim fh1 As New FileHelperEngine(Of UNIQUAC_IPData)
            uniquacipc = fh1.ReadFile(My.Application.Info.DirectoryPath & pathsep & "data" & pathsep & "uniquac.dat")
            uniquacipc2 = fh1.ReadFile(My.Application.Info.DirectoryPath & pathsep & "data" & pathsep & "uniquacip.dat")

            Dim csdb As New DWSIM.Databases.ChemSep

            'load UNIQUAC.DAT database interactions
            For Each uniquacip In uniquacipc
                If Me.InteractionParameters.ContainsKey(csdb.GetDWSIMName(uniquacip.ID1)) Then
                    If Not Me.InteractionParameters(csdb.GetDWSIMName(uniquacip.ID1)).ContainsKey(csdb.GetDWSIMName(uniquacip.ID2)) Then
                        Me.InteractionParameters(csdb.GetDWSIMName(uniquacip.ID1)).Add(csdb.GetDWSIMName(uniquacip.ID2), uniquacip.Clone)
                    End If
                    If Not Me.InteractionParameters(csdb.GetDWSIMName(uniquacip.ID1)).ContainsKey(csdb.GetCSName(uniquacip.ID2)) Then
                        Me.InteractionParameters(csdb.GetDWSIMName(uniquacip.ID1)).Add(csdb.GetCSName(uniquacip.ID2), uniquacip.Clone)
                    End If
                Else
                    Me.InteractionParameters.Add(csdb.GetDWSIMName(uniquacip.ID1), New Dictionary(Of String, UNIQUAC_IPData))
                    Me.InteractionParameters(csdb.GetDWSIMName(uniquacip.ID1)).Add(csdb.GetDWSIMName(uniquacip.ID2), uniquacip.Clone)
                    If Not Me.InteractionParameters(csdb.GetDWSIMName(uniquacip.ID1)).ContainsKey(csdb.GetCSName(uniquacip.ID2)) Then
                        Me.InteractionParameters(csdb.GetDWSIMName(uniquacip.ID1)).Add(csdb.GetCSName(uniquacip.ID2), uniquacip.Clone)
                    End If
                End If
            Next
            For Each uniquacip In uniquacipc
                If Me.InteractionParameters.ContainsKey(csdb.GetCSName(uniquacip.ID1)) Then
                    If Not Me.InteractionParameters(csdb.GetCSName(uniquacip.ID1)).ContainsKey(csdb.GetCSName(uniquacip.ID2)) Then
                        Me.InteractionParameters(csdb.GetCSName(uniquacip.ID1)).Add(csdb.GetCSName(uniquacip.ID2), uniquacip.Clone)
                    End If
                    If Not Me.InteractionParameters(csdb.GetCSName(uniquacip.ID1)).ContainsKey(csdb.GetDWSIMName(uniquacip.ID2)) Then
                        Me.InteractionParameters(csdb.GetCSName(uniquacip.ID1)).Add(csdb.GetDWSIMName(uniquacip.ID2), uniquacip.Clone)
                    End If
                Else
                    Me.InteractionParameters.Add(csdb.GetCSName(uniquacip.ID1), New Dictionary(Of String, UNIQUAC_IPData))
                    Me.InteractionParameters(csdb.GetCSName(uniquacip.ID1)).Add(csdb.GetCSName(uniquacip.ID2), uniquacip.Clone)
                    If Not Me.InteractionParameters(csdb.GetCSName(uniquacip.ID1)).ContainsKey(csdb.GetDWSIMName(uniquacip.ID2)) Then
                        Me.InteractionParameters(csdb.GetCSName(uniquacip.ID1)).Add(csdb.GetDWSIMName(uniquacip.ID2), uniquacip.Clone)
                    End If
                End If
            Next

            'load UNIQUACIP.DAT database interactions
            For Each uniquacip In uniquacipc2
                uniquacip.A12 *= 1.98721
                uniquacip.A21 *= 1.98721
            Next

            For Each uniquacip In uniquacipc2
                If Me.InteractionParameters.ContainsKey(csdb.GetDWSIMName(uniquacip.ID1)) Then
                    If Not Me.InteractionParameters(csdb.GetDWSIMName(uniquacip.ID1)).ContainsKey(csdb.GetDWSIMName(uniquacip.ID2)) Then
                        Me.InteractionParameters(csdb.GetDWSIMName(uniquacip.ID1)).Add(csdb.GetDWSIMName(uniquacip.ID2), uniquacip.Clone)
                    End If
                    If Not Me.InteractionParameters(csdb.GetDWSIMName(uniquacip.ID1)).ContainsKey(csdb.GetCSName(uniquacip.ID2)) Then
                        Me.InteractionParameters(csdb.GetDWSIMName(uniquacip.ID1)).Add(csdb.GetCSName(uniquacip.ID2), uniquacip.Clone)
                    End If
                Else
                    Me.InteractionParameters.Add(csdb.GetDWSIMName(uniquacip.ID1), New Dictionary(Of String, UNIQUAC_IPData))
                    Me.InteractionParameters(csdb.GetDWSIMName(uniquacip.ID1)).Add(csdb.GetDWSIMName(uniquacip.ID2), uniquacip.Clone)
                    If Not Me.InteractionParameters(csdb.GetDWSIMName(uniquacip.ID1)).ContainsKey(csdb.GetCSName(uniquacip.ID2)) Then
                        Me.InteractionParameters(csdb.GetDWSIMName(uniquacip.ID1)).Add(csdb.GetCSName(uniquacip.ID2), uniquacip.Clone)
                    End If
                End If
            Next
            For Each uniquacip In uniquacipc2
                If Me.InteractionParameters.ContainsKey(csdb.GetCSName(uniquacip.ID1)) Then
                    If Not Me.InteractionParameters(csdb.GetCSName(uniquacip.ID1)).ContainsKey(csdb.GetCSName(uniquacip.ID2)) Then
                        Me.InteractionParameters(csdb.GetCSName(uniquacip.ID1)).Add(csdb.GetCSName(uniquacip.ID2), uniquacip.Clone)
                    End If
                    If Not Me.InteractionParameters(csdb.GetCSName(uniquacip.ID1)).ContainsKey(csdb.GetDWSIMName(uniquacip.ID2)) Then
                        Me.InteractionParameters(csdb.GetCSName(uniquacip.ID1)).Add(csdb.GetDWSIMName(uniquacip.ID2), uniquacip.Clone)
                    End If
                Else
                    Me.InteractionParameters.Add(csdb.GetCSName(uniquacip.ID1), New Dictionary(Of String, UNIQUAC_IPData))
                    Me.InteractionParameters(csdb.GetCSName(uniquacip.ID1)).Add(csdb.GetCSName(uniquacip.ID2), uniquacip.Clone)
                    If Not Me.InteractionParameters(csdb.GetCSName(uniquacip.ID1)).ContainsKey(csdb.GetDWSIMName(uniquacip.ID2)) Then
                        Me.InteractionParameters(csdb.GetCSName(uniquacip.ID1)).Add(csdb.GetDWSIMName(uniquacip.ID2), uniquacip.Clone)
                    End If
                End If
            Next

            'load user database interactions
            If Not My.Settings.UserInteractionsDatabases Is Nothing Then
                For Each IPDBPath As String In My.Settings.UserInteractionsDatabases

                    Dim Interactions As DWSIM.ClassesBasicasTermodinamica.InteractionParameter()
                    Dim IP As DWSIM.ClassesBasicasTermodinamica.InteractionParameter
                    Try
                        Interactions = DWSIM.Databases.UserIPDB.ReadInteractions(IPDBPath, "UNIQUAC")
                        For Each IP In Interactions
                            Dim IPD As New UNIQUAC_IPData
                            IPD.A12 = IP.Parameters.Item("A12")
                            IPD.A21 = IP.Parameters.Item("A21")
                            IPD.comment = IP.Description
                            If IP.Parameters.ContainsKey("B12") Then IPD.B12 = IP.Parameters.Item("B12")
                            If IP.Parameters.ContainsKey("B21") Then IPD.B21 = IP.Parameters.Item("B21")
                            If IP.Parameters.ContainsKey("C12") Then IPD.C12 = IP.Parameters.Item("C12")
                            If IP.Parameters.ContainsKey("C21") Then IPD.C21 = IP.Parameters.Item("C21")

                            If Me.InteractionParameters.ContainsKey(IP.Comp1) Then
                                If Me.InteractionParameters(IP.Comp1).ContainsKey(IP.Comp2) Then
                                Else
                                    Me.InteractionParameters(IP.Comp1).Add(IP.Comp2, IPD.Clone)
                                End If
                            Else
                                Me.InteractionParameters.Add(IP.Comp1, New Dictionary(Of String, UNIQUAC_IPData))
                                Me.InteractionParameters(IP.Comp1).Add(IP.Comp2, IPD.Clone)
                            End If
                        Next
                    Catch ex As Exception
                        Console.WriteLine(ex.ToString)
                    End Try
                Next
            End If



            uniquacip = Nothing
            uniquacipc = Nothing
            uniquacipc2 = Nothing
            fh1 = Nothing

        End Sub

        Function GAMMA(ByVal T As Double, ByVal Vx As Array, ByVal Vids As Array, ByVal VQ As Array, ByVal VR As Array, ByVal index As Integer)

            Return GAMMA_MR(T, Vx, Vids, VQ, VR)(index)

        End Function

        Function GAMMA_MR(ByVal T, ByVal Vx, ByVal Vids, ByVal VQ, ByVal VR)

            Dim n As Integer = UBound(Vx)

            Dim tau_ij(n, n), tau_ji(n, n), a12(n, n), a21(n, n), b12(n, n), b21(n, n), c12(n, n), c21(n, n) As Double

            Dim i, j As Integer

            i = 0
            Do
                j = 0
                Do
                    If Me.InteractionParameters.ContainsKey(Vids(i)) Then
                        If Me.InteractionParameters(Vids(i)).ContainsKey(Vids(j)) Then
                            a12(i, j) = Me.InteractionParameters(Vids(i))(Vids(j)).A12
                            a21(i, j) = Me.InteractionParameters(Vids(i))(Vids(j)).A21
                            b12(i, j) = Me.InteractionParameters(Vids(i))(Vids(j)).B12
                            b21(i, j) = Me.InteractionParameters(Vids(i))(Vids(j)).B21
                            c12(i, j) = Me.InteractionParameters(Vids(i))(Vids(j)).C12
                            c21(i, j) = Me.InteractionParameters(Vids(i))(Vids(j)).C21
                        Else
                            If Me.InteractionParameters.ContainsKey(Vids(j)) Then
                                If Me.InteractionParameters(Vids(j)).ContainsKey(Vids(i)) Then
                                    a12(i, j) = Me.InteractionParameters(Vids(j))(Vids(i)).A21
                                    a21(i, j) = Me.InteractionParameters(Vids(j))(Vids(i)).A12
                                    b12(i, j) = Me.InteractionParameters(Vids(j))(Vids(i)).B21
                                    b21(i, j) = Me.InteractionParameters(Vids(j))(Vids(i)).B12
                                    c12(i, j) = Me.InteractionParameters(Vids(j))(Vids(i)).C21
                                    c21(i, j) = Me.InteractionParameters(Vids(j))(Vids(i)).C12
                                End If
                            End If
                        End If
                    End If
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                j = 0
                Do
                    tau_ij(i, j) = Math.Exp(-(a12(i, j) + b12(i, j) * T + c12(i, j) * T ^ 2) / (1.98721 * T))
                    tau_ji(j, i) = Math.Exp(-(a21(i, j) + b21(i, j) * T + c21(i, j) * T ^ 2) / (1.98721 * T))
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            Dim r, q As Double

            i = 0
            Do
                r += Vx(i) * VR(i)
                q += Vx(i) * VQ(i)
                i = i + 1
            Loop Until i = n + 1

            Dim teta(n), fi(n), l(n), S(n), lngc(n), lngr(n), lng(n), g(n), sum1(n), sum2 As Double
            Dim z As Double = 10.0#

            i = 0
            Do
                fi(i) = Vx(i) * VR(i) / r
                teta(i) = Vx(i) * VQ(i) / q
                l(i) = z / 2 * (VR(i) - VQ(i)) - (VR(i) - 1)
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                S(i) = 0
                j = 0
                Do
                    S(i) += teta(j) * tau_ji(j, i)
                    j = j + 1
                Loop Until j = n + 1
                i = i + 1
            Loop Until i = n + 1

            i = 0
            Do
                sum1(i) = 0
                j = 0
                Do
                    sum1(i) += teta(j) * tau_ij(i, j) / S(j)
                    j = j + 1
                Loop Until j = n + 1
                sum2 += Vx(i) * l(i)
                i = i + 1
            Loop Until i = n + 1


            i = 0
            Do
                If Vx(i) <> 0.0# Then
                    lngc(i) = 1 - VR(i) / r + Math.Log(VR(i) / r) - z / 2 * VQ(i) * (1 - fi(i) / teta(i) + Math.Log(fi(i) / teta(i)))
                Else
                    lngc(i) = 1 - VR(i) / r
                End If
                lngr(i) = VQ(i) * (1 - Math.Log(S(i)) - sum1(i))
                lng(i) = lngc(i) + lngr(i)
                g(i) = Math.Exp(lng(i))
                i = i + 1
            Loop Until i = n + 1

            Return g


        End Function

        Function DLNGAMMA_DT(ByVal T As Double, ByVal Vx As Array, ByVal Vids As Array, ByVal VQ As Array, ByVal VR As Array) As Array

            Dim gamma1, gamma2 As Double()

            Dim epsilon As Double = 0.001

            gamma1 = GAMMA_MR(T, Vx, Vids, VQ, VR)
            gamma2 = GAMMA_MR(T + epsilon, Vx, Vids, VQ, VR)

            Dim dgamma(gamma1.Length - 1) As Double

            For i As Integer = 0 To Vx.Length - 1
                dgamma(i) = (gamma2(i) - gamma1(i)) / (epsilon)
            Next

            Return dgamma

        End Function

        Function HEX_MIX(ByVal T As Double, ByVal Vx As Array, ByVal Vids As Array, ByVal VQ As Array, ByVal VR As Array) As Double

            Dim dgamma As Double() = DLNGAMMA_DT(T, Vx, Vids, VQ, VR)

            Dim hex As Double = 0.0#

            For i As Integer = 0 To Vx.Length - 1
                hex += -8.314 * T ^ 2 * Vx(i) * dgamma(i)
            Next

            Return hex 'kJ/kmol

        End Function

        Function CPEX_MIX(ByVal T As Double, ByVal Vx As Array, ByVal Vids As Array, ByVal VQ As Array, ByVal VR As Array) As Double

            Dim hex1, hex2, cpex As Double

            Dim epsilon As Double = 0.001

            hex1 = HEX_MIX(T, Vx, Vids, VQ, VR)
            hex2 = HEX_MIX(T + epsilon, Vx, Vids, VQ, VR)

            cpex = (hex2 - hex1) / epsilon

            Return cpex 'kJ/kmol.K

        End Function

        Public Function CalcActivityCoefficients(T As Double, Vx As Array, otherargs As Object) As Array Implements IActivityCoefficientBase.CalcActivityCoefficients

            Return GAMMA_MR(T, Vx, otherargs(0), otherargs(1), otherargs(2))

        End Function

        Public Function CalcExcessEnthalpy(T As Double, Vx As Array, otherargs As Object) As Double Implements IActivityCoefficientBase.CalcExcessEnthalpy

            Return HEX_MIX(T, Vx, otherargs(0), otherargs(1), otherargs(2))

        End Function

        Public Function CalcExcessHeatCapacity(T As Double, Vx As Array, otherargs As Object) As Double Implements IActivityCoefficientBase.CalcExcessHeatCapacity

            Return CPEX_MIX(T, Vx, otherargs(0), otherargs(1), otherargs(2))

        End Function

    End Class

End Namespace
