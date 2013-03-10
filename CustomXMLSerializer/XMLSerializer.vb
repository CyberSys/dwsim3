﻿'DWSIM Custom XML Serializer
'Copyright 2012 Daniel Wagner O. de Medeiros
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

Imports System.Globalization
Imports System.Reflection
Imports System.Drawing

Public Class XMLSerializer

    ''' <summary>
    ''' Deserializes selected properties of an object from XML.
    ''' </summary>
    ''' <param name="obj">The object to be updated with properties from the XML elements.</param>
    ''' <param name="xmlprops">The list of XML elements to deserialize from.</param>
    ''' <returns>True if successful.</returns>
    ''' <remarks> Properties of type Boolean, String, Double, Single, Integer, Nullable(Of Double), 
    ''' Nullable(Of Single), Nullable(Of Integer), ArrayList, Font, Color, [Enum]
    ''' are supported.</remarks>
    Shared Function Deserialize(ByRef obj As Object, xmlprops As System.Collections.Generic.List(Of XElement), Optional ByVal Fields As Boolean = False) As Boolean

        Dim ci As CultureInfo = CultureInfo.InvariantCulture
        Dim skip As Boolean = False
        If Not Fields Then
            Dim props As PropertyInfo() = obj.GetType.GetProperties()
            For Each prop As PropertyInfo In props
                skip = False
                If prop.CanWrite And prop.CanRead Then
                    Dim propname As String = prop.Name
                    Dim attributes As Object() = obj.GetType.GetProperty(prop.Name).GetCustomAttributes(True)
                    For Each attr As Attribute In attributes
                        If TypeOf attr Is System.Xml.Serialization.XmlIgnoreAttribute Then
                            skip = True
                            Exit For
                        End If
                    Next
                    If Not skip Then
                        If obj.GetType.GetProperty(prop.Name) IsNot Nothing Then
                            If TypeOf obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing) Is Interfaces.ICustomXMLSerialization Then
                                Dim xel As XElement = (From xmlprop In xmlprops Select xmlprop Where xmlprop.Name = propname).SingleOrDefault
                                If Not xel Is Nothing Then
                                    Dim val As List(Of XElement) = xel.Descendants.ToList()
                                    DirectCast(obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing), Interfaces.ICustomXMLSerialization).LoadData(val)
                                End If
                            ElseIf TypeOf obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing) Is Single Then
                                Dim xel As XElement = (From xmlprop In xmlprops Select xmlprop Where xmlprop.Name = propname).SingleOrDefault
                                Dim val As Single = Single.Parse(xel.Value, ci)
                                obj.GetType.GetProperty(prop.Name).SetValue(obj, val, Nothing)
                            ElseIf TypeOf obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing) Is Double Then
                                Dim xel As XElement = (From xmlprop In xmlprops Select xmlprop Where xmlprop.Name = propname).SingleOrDefault
                                If Not xel Is Nothing Then
                                    Dim val As Double = Double.Parse(xel.Value, ci)
                                    obj.GetType.GetProperty(prop.Name).SetValue(obj, val, Nothing)
                                End If
                            ElseIf obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing) Is Nothing Then
                                Dim xel As XElement = (From xmlprop In xmlprops Select xmlprop Where xmlprop.Name = propname).SingleOrDefault
                                If Not xel Is Nothing Then
                                    Dim val As Nullable(Of Double)
                                    If xel.Value <> "" Then val = Double.Parse(xel.Value, ci)
                                    If Not val Is Nothing Then obj.GetType.GetProperty(prop.Name).SetValue(obj, val, Nothing)
                                End If
                            ElseIf TypeOf obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing) Is Integer Then
                                Dim xel As XElement = (From xmlprop In xmlprops Select xmlprop Where xmlprop.Name = propname).SingleOrDefault
                                Dim val As Integer = xel.Value
                                obj.GetType.GetProperty(prop.Name).SetValue(obj, val, Nothing)
                            ElseIf TypeOf obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing) Is Boolean Then
                                Dim xel As XElement = (From xmlprop In xmlprops Select xmlprop Where xmlprop.Name = propname).SingleOrDefault
                                If Not xel Is Nothing Then
                                    Dim val As Boolean = xel.Value
                                    obj.GetType.GetProperty(prop.Name).SetValue(obj, val, Nothing)
                                End If
                            ElseIf TypeOf obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing) Is String Then
                                Dim xel As XElement = (From xmlprop In xmlprops Select xmlprop Where xmlprop.Name = propname).SingleOrDefault
                                If Not xel Is Nothing Then
                                    Dim val As Object = xel.Value
                                    If Not val Is Nothing Then obj.GetType.GetProperty(prop.Name).SetValue(obj, val, Nothing)
                                End If
                            ElseIf TypeOf obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing) Is [Enum] Then
                                Dim xel As XElement = (From xmlprop In xmlprops Select xmlprop Where xmlprop.Name = propname).SingleOrDefault
                                If Not xel Is Nothing Then
                                    Dim val As String = xel.Value
                                    If Not val Is Nothing Then obj.GetType.GetProperty(prop.Name).SetValue(obj, [Enum].Parse(obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing).GetType, val), Nothing)
                                End If
                            ElseIf TypeOf obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing) Is Font Then
                                Dim xel As XElement = (From xmlprop In xmlprops Select xmlprop Where xmlprop.Name = propname).SingleOrDefault
                                Dim val As Font = GetFontByString(xel.Value)
                                obj.GetType.GetProperty(prop.Name).SetValue(obj, val, Nothing)
                            ElseIf TypeOf obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing) Is Color Then
                                Dim xel As XElement = (From xmlprop In xmlprops Select xmlprop Where xmlprop.Name = propname).SingleOrDefault
                                Dim val As Color = ColorTranslator.FromHtml(xel.Value)
                                obj.GetType.GetProperty(prop.Name).SetValue(obj, val, Nothing)
                            ElseIf TypeOf obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing) Is ArrayList Then
                                Dim xel As XElement = (From xmlprop In xmlprops Select xmlprop Where xmlprop.Name = propname).SingleOrDefault
                                Dim val As ArrayList = StringToArray(xel.Value, ci)
                                If Not val Is Nothing Then obj.GetType.GetProperty(prop.Name).SetValue(obj, val, Nothing)
                            End If
                        End If
                    End If
                End If
            Next
        Else
            Dim props As FieldInfo() = obj.GetType.GetFields()
            For Each prop As FieldInfo In props
                skip = False
                Dim propname As String = prop.Name
                Dim attributes As Object() = obj.GetType.GetField(prop.Name).GetCustomAttributes(True)
                For Each attr As Attribute In attributes
                    If TypeOf attr Is System.Xml.Serialization.XmlIgnoreAttribute Then
                        skip = True
                        Exit For
                    End If
                Next
                If Not skip Then
                    If obj.GetType.GetField(prop.Name) IsNot Nothing Then
                        If TypeOf obj.GetType.GetField(prop.Name).GetValue(obj) Is Interfaces.ICustomXMLSerialization Then
                            Dim xel As XElement = (From xmlprop In xmlprops Select xmlprop Where xmlprop.Name = propname).SingleOrDefault
                            If Not xel Is Nothing Then
                                Dim val As List(Of XElement) = xel.Descendants.ToList()
                                DirectCast(obj.GetType.GetField(prop.Name).GetValue(obj), Interfaces.ICustomXMLSerialization).LoadData(val)
                            End If
                        ElseIf TypeOf obj.GetType.GetField(prop.Name).GetValue(obj) Is Single Then
                            Dim xel As XElement = (From xmlprop In xmlprops Select xmlprop Where xmlprop.Name = propname).SingleOrDefault
                            Dim val As Single = Single.Parse(xel.Value, ci)
                            obj.GetType.GetField(prop.Name).SetValue(obj, val)
                        ElseIf TypeOf obj.GetType.GetField(prop.Name).GetValue(obj) Is Double Then
                            Dim xel As XElement = (From xmlprop In xmlprops Select xmlprop Where xmlprop.Name = propname).SingleOrDefault
                            If Not xel Is Nothing Then
                                Dim val As Double = Double.Parse(xel.Value, ci)
                                obj.GetType.GetField(prop.Name).SetValue(obj, val)
                            End If
                        ElseIf obj.GetType.GetField(prop.Name).GetValue(obj) Is Nothing Then
                            Dim xel As XElement = (From xmlprop In xmlprops Select xmlprop Where xmlprop.Name = propname).SingleOrDefault
                            If Not xel Is Nothing Then
                                Dim val As Nullable(Of Double)
                                If xel.Value <> "" Then val = Double.Parse(xel.Value, ci)
                                If Not val Is Nothing Then obj.GetType.GetField(prop.Name).SetValue(obj, val)
                            End If
                        ElseIf TypeOf obj.GetType.GetField(prop.Name).GetValue(obj) Is Integer Then
                            Dim xel As XElement = (From xmlprop In xmlprops Select xmlprop Where xmlprop.Name = propname).SingleOrDefault
                            If Not xel Is Nothing Then
                                Dim val As Integer = xel.Value
                                obj.GetType.GetField(prop.Name).SetValue(obj, val)
                            End If
                        ElseIf TypeOf obj.GetType.GetField(prop.Name).GetValue(obj) Is Boolean Then
                            Dim xel As XElement = (From xmlprop In xmlprops Select xmlprop Where xmlprop.Name = propname).SingleOrDefault
                            If Not xel Is Nothing Then
                                Dim val As Boolean = xel.Value
                                obj.GetType.GetField(prop.Name).SetValue(obj, val)
                            End If
                        ElseIf TypeOf obj.GetType.GetField(prop.Name).GetValue(obj) Is String Then
                            Dim xel As XElement = (From xmlprop In xmlprops Select xmlprop Where xmlprop.Name = propname).SingleOrDefault
                            If Not xel Is Nothing Then
                                Dim val As String = xel.Value
                                obj.GetType.GetField(prop.Name).SetValue(obj, val)
                            End If
                        ElseIf TypeOf obj.GetType.GetField(prop.Name).GetValue(obj) Is [Enum] Then
                            Dim xel As XElement = (From xmlprop In xmlprops Select xmlprop Where xmlprop.Name = propname).SingleOrDefault
                            If Not xel Is Nothing Then
                                Dim val As String = xel.Value
                                If Not val Is Nothing Then obj.GetType.GetField(prop.Name).SetValue(obj, [Enum].Parse(obj.GetType.GetField(prop.Name).GetValue(obj).GetType, val))
                            End If
                        ElseIf TypeOf obj.GetType.GetField(prop.Name).GetValue(obj) Is Font Then
                            Dim xel As XElement = (From xmlprop In xmlprops Select xmlprop Where xmlprop.Name = propname).SingleOrDefault
                            If Not xel Is Nothing Then
                                Dim val As Font = GetFontByString(xel.Value)
                                obj.GetType.GetField(prop.Name).SetValue(obj, val)
                            End If
                        ElseIf TypeOf obj.GetType.GetField(prop.Name).GetValue(obj) Is Color Then
                            Dim xel As XElement = (From xmlprop In xmlprops Select xmlprop Where xmlprop.Name = propname).SingleOrDefault
                            Dim val As Color = ColorTranslator.FromHtml(xel.Value)
                            obj.GetType.GetField(prop.Name).SetValue(obj, val)
                        ElseIf TypeOf obj.GetType.GetField(prop.Name).GetValue(obj) Is ArrayList Then
                            Dim xel As XElement = (From xmlprop In xmlprops Select xmlprop Where xmlprop.Name = propname).SingleOrDefault
                            Dim val As ArrayList = StringToArray(xel.Value, ci)
                            If Not val Is Nothing Then obj.GetType.GetField(prop.Name).SetValue(obj, val)
                        End If
                    End If
                End If
            Next
         End If
        Return True

    End Function

    ''' <summary>
    ''' Serializes selected properties of an object to XML.
    ''' </summary>
    ''' <param name="obj">The object to serialize.</param>
   ''' <returns>True if successful.</returns>
    ''' <remarks> Properties of type String, Double, Single, Integer, Nullable(Of Double), 
    ''' Nullable(Of Single), Nullable(Of Integer), ArrayList, Font, Color, [Enum]
    ''' are supported.</remarks>
    Shared Function Serialize(ByRef obj As Object, Optional ByVal Fields As Boolean = False) As System.Collections.Generic.List(Of XElement)

        Dim elements As New List(Of System.Xml.Linq.XElement)
        Dim ci As CultureInfo = CultureInfo.InvariantCulture
        Dim skip As Boolean = False
        With elements
            .Add(New XElement("Type", obj.GetType.ToString))
            If Not Fields Then
                Dim props As PropertyInfo() = obj.GetType.GetProperties()
                For Each prop As PropertyInfo In props
                    skip = False
                    Dim attributes As Object() = obj.GetType.GetProperty(prop.Name).GetCustomAttributes(True)
                    For Each attr As Attribute In attributes
                        If TypeOf attr Is System.Xml.Serialization.XmlIgnoreAttribute Then
                            skip = True
                            Exit For
                        End If
                    Next
                    If Not skip Then
                        If prop.CanRead Then
                            If TypeOf obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing) Is Interfaces.ICustomXMLSerialization Then
                                .Add(New XElement(prop.Name, DirectCast(obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing), Interfaces.ICustomXMLSerialization).SaveData.ToArray()))
                            ElseIf TypeOf obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing) Is ArrayList Then
                                .Add(New XElement(prop.Name, ArrayToString(obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing), ci)))
                            ElseIf TypeOf obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing) Is Single Then
                                .Add(New XElement(prop.Name, Single.Parse(obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing)).ToString(ci)))
                            ElseIf TypeOf obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing) Is Double Then
                                .Add(New XElement(prop.Name, Double.Parse(obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing)).ToString(ci)))
                            ElseIf TypeOf obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing) Is Nullable(Of Double) Then
                                .Add(New XElement(prop.Name, Double.Parse(obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing)).ToString(ci)))
                            ElseIf TypeOf obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing) Is Nullable(Of Single) Then
                                .Add(New XElement(prop.Name, Single.Parse(obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing)).ToString(ci)))
                            ElseIf TypeOf obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing) Is Nullable(Of Integer) Then
                                .Add(New XElement(prop.Name, obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing)))
                            ElseIf TypeOf obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing) Is Integer Then
                                .Add(New XElement(prop.Name, obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing)))
                            ElseIf TypeOf obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing) Is Boolean Then
                                .Add(New XElement(prop.Name, obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing)))
                            ElseIf TypeOf obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing) Is String Then
                                .Add(New XElement(prop.Name, obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing)))
                            ElseIf TypeOf obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing) Is [Enum] Then
                                .Add(New XElement(prop.Name, obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing)))
                            ElseIf TypeOf obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing) Is Font Then
                                .Add(New XElement(prop.Name, obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing)))
                            ElseIf TypeOf obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing) Is Color Then
                                .Add(New XElement(prop.Name, ColorTranslator.ToHtml(obj.GetType.GetProperty(prop.Name).GetValue(obj, Nothing))))
                            End If
                        End If
                    End If
                Next
            Else
                Dim props As FieldInfo() = obj.GetType.GetFields()
                For Each prop As FieldInfo In props
                    skip = False
                    Dim attributes As Object() = obj.GetType.GetField(prop.Name).GetCustomAttributes(True)
                    For Each attr As Attribute In attributes
                        If TypeOf attr Is System.Xml.Serialization.XmlIgnoreAttribute Then
                            skip = True
                            Exit For
                        End If
                    Next
                    If Not skip Then
                        If TypeOf obj.GetType.GetField(prop.Name).GetValue(obj) Is Interfaces.ICustomXMLSerialization Then
                            .Add(New XElement(prop.Name, DirectCast(obj.GetType.GetField(prop.Name).GetValue(obj), Interfaces.ICustomXMLSerialization).SaveData.ToArray()))
                        ElseIf TypeOf obj.GetType.GetField(prop.Name).GetValue(obj) Is ArrayList Then
                            .Add(New XElement(prop.Name, ArrayToString(obj.GetType.GetField(prop.Name).GetValue(obj), ci)))
                        ElseIf TypeOf obj.GetType.GetField(prop.Name).GetValue(obj) Is Single Then
                            .Add(New XElement(prop.Name, Single.Parse(obj.GetType.GetField(prop.Name).GetValue(obj)).ToString(ci)))
                        ElseIf TypeOf obj.GetType.GetField(prop.Name).GetValue(obj) Is Double Then
                            .Add(New XElement(prop.Name, Double.Parse(obj.GetType.GetField(prop.Name).GetValue(obj)).ToString(ci)))
                        ElseIf TypeOf obj.GetType.GetField(prop.Name).GetValue(obj) Is Nullable(Of Double) Then
                            .Add(New XElement(prop.Name, Double.Parse(obj.GetType.GetField(prop.Name).GetValue(obj)).ToString(ci)))
                        ElseIf TypeOf obj.GetType.GetField(prop.Name).GetValue(obj) Is Nullable(Of Single) Then
                            .Add(New XElement(prop.Name, Single.Parse(obj.GetType.GetField(prop.Name).GetValue(obj)).ToString(ci)))
                        ElseIf TypeOf obj.GetType.GetField(prop.Name).GetValue(obj) Is Nullable(Of Integer) Then
                            .Add(New XElement(prop.Name, obj.GetType.GetField(prop.Name).GetValue(obj)))
                        ElseIf TypeOf obj.GetType.GetField(prop.Name).GetValue(obj) Is Integer Then
                            .Add(New XElement(prop.Name, obj.GetType.GetField(prop.Name).GetValue(obj)))
                        ElseIf TypeOf obj.GetType.GetField(prop.Name).GetValue(obj) Is Boolean Then
                            .Add(New XElement(prop.Name, obj.GetType.GetField(prop.Name).GetValue(obj)))
                        ElseIf TypeOf obj.GetType.GetField(prop.Name).GetValue(obj) Is String Then
                            .Add(New XElement(prop.Name, obj.GetType.GetField(prop.Name).GetValue(obj)))
                        ElseIf TypeOf obj.GetType.GetField(prop.Name).GetValue(obj) Is [Enum] Then
                            .Add(New XElement(prop.Name, obj.GetType.GetField(prop.Name).GetValue(obj)))
                        ElseIf TypeOf obj.GetType.GetField(prop.Name).GetValue(obj) Is Font Then
                            .Add(New XElement(prop.Name, obj.GetType.GetField(prop.Name).GetValue(obj)))
                        ElseIf TypeOf obj.GetType.GetField(prop.Name).GetValue(obj) Is Color Then
                            .Add(New XElement(prop.Name, ColorTranslator.ToHtml(obj.GetType.GetField(prop.Name).GetValue(obj))))
                        End If
                    End If
                Next
            End If
        End With
        Return elements

    End Function

    Public Shared Function GetFontByString(ByVal sFont As String) As Font
        sFont = sFont.Substring(1, sFont.Length - 2)
        sFont = Replace(sFont, ",", vbNullString)
        sFont = Replace(sFont, "Font:", vbNullString)
        Dim sElement() As String = Split(sFont, " ")
        Dim sSingle() As String
        Dim sValue As String
        Dim FontName As String = ""
        Dim FontSize As Single
        Dim FontStyle As FontStyle = Drawing.FontStyle.Regular
        Dim FontUnit As GraphicsUnit = GraphicsUnit.Point
        Dim gdiCharSet As Byte
        Dim gdiVerticalFont As Boolean

        For Each sValue In sElement
            sValue = Trim(sValue)
            sSingle = Split(sValue, "=")
            If sSingle.GetUpperBound(0) > 0 Then
                If sSingle(0) = "Name" Then
                    FontName = sSingle(1)
                ElseIf sSingle(0) = "Size" Then
                    FontSize = CSng(sSingle(1))
                    If FontSize > 100 Then FontSize /= 100
                ElseIf sSingle(0) = "Units" Then
                    FontUnit = CInt(sSingle(1))
                ElseIf sSingle(0) = "GdiCharSet" Then
                    FontName = CByte(sSingle(1))
                ElseIf sSingle(0) = "GdiVerticalFont" Then
                    FontName = CBool(sSingle(1))
                End If
            End If
        Next
        Return New Font(FontName, FontSize, FontStyle, FontUnit, gdiCharSet, gdiVerticalFont)
    End Function

    Public Shared Function ArrayToString(sourcearray As ArrayList, ci As CultureInfo) As String

        Dim sb As String = ""

        If sourcearray.Count > 0 Then

            For Each obj As Object In sourcearray
                If TypeOf obj Is Double Then
                    sb += Double.Parse(obj).ToString(ci) + ","
                Else
                    sb += obj.ToString + ","
                End If
            Next

            sb = sb.Remove(sb.Length - 1)

        End If

        Return sb

    End Function

    Public Shared Function ArrayToString2(sourcearray As Array, ci As CultureInfo) As String

        Dim sb As String = ""

        If Not sourcearray Is Nothing Then
            If sourcearray.Length > 0 Then

                For Each obj As Object In sourcearray
                    If TypeOf obj Is Double Then
                        sb += Double.Parse(obj).ToString(ci) + ","
                    Else
                        sb += obj.ToString + ","
                    End If
                Next

                sb = sb.Remove(sb.Length - 1)

            End If
        End If

        Return sb

    End Function

    Public Shared Function StringToArray(ByVal text As String, ci As CultureInfo) As ArrayList

        If Not text Is Nothing Then
            Dim values() As String = text.Split(",")
            Dim myarr As New ArrayList

            For Each s As String In values
                If Double.TryParse(s, New Double) Then
                    myarr.Add(Double.Parse(s, ci))
                Else
                    myarr.Add(s)
                End If
            Next
            Return myarr
        Else
            Return New ArrayList()
        End If
    End Function

    Public Shared Function StringToArray2(ByVal text As String, ci As CultureInfo, arraytype As Type) As Array

        If Not text Is Nothing Then
            Dim values() As String = text.Split(",")
            Dim myarr As New ArrayList

            For Each s As String In values
                If Double.TryParse(s, New Double) Then
                    myarr.Add(Double.Parse(s, ci))
                Else
                    myarr.Add(s)
                End If
            Next

            Return myarr.ToArray(arraytype)
        Else
            Return New ArrayList().ToArray(arraytype)
        End If

    End Function

End Class

