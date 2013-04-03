﻿Imports DWSIM.DWSIM.ClassesBasicasTermodinamica
Imports DWSIM.DWSIM.SimulationObjects.Streams

Public Class CompositionEditorForm
    Inherits System.Windows.Forms.Form
    Public Componentes As Dictionary(Of String, Substancia)
    Public Q, W As Double
    Public SU As DWSIM.SistemasDeUnidades.Unidades
    Public NF As String = ""
    Private loaded As Boolean = False

    Private Sub CompositionEditorForm_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Dim comp As DWSIM.ClassesBasicasTermodinamica.Substancia
        GridComp.Rows.Clear()
        For Each comp In Me.Componentes.Values
            If Me.RadioButton1.Checked Then
                GridComp.Rows.Add(New Object() {comp.FracaoMolar})
                GridComp.Rows(GridComp.Rows.Count - 1).HeaderCell.Value = DWSIM.App.GetComponentName(comp.Nome)
            Else
                GridComp.Rows.Add(New Object() {comp.FracaoMassica})
                GridComp.Rows(GridComp.Rows.Count - 1).HeaderCell.Value = DWSIM.App.GetComponentName(comp.Nome)
            End If
            GridComp.Rows(GridComp.Rows.Count - 1).HeaderCell.Tag = comp.Nome
        Next
        Try
            Dim v As Double = 0
            For Each r As DataGridViewRow In Me.GridComp.Rows
                v += CDbl(r.Cells(0).Value)
            Next
            Me.Label3.Text = Format(v, "#0.0000")
            Me.Label3.ForeColor = Color.SlateBlue
            If Math.Abs(1 - v) < 0.0001 Then
                Me.Label2.Text = "OK"
                Me.Label2.ForeColor = Color.Green
            End If
        Catch ex As Exception
            Me.Label3.Text = DWSIM.App.GetLocalString("indefinido")
            Me.Label3.ForeColor = Color.Red
        End Try
        loaded = True
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles KButton1.Click
        Dim row As DataGridViewRow
        For Each row In GridComp.Rows
            row.Cells(0).Value = 0
        Next
    End Sub

    Private Sub Button23_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles KButton23.Click
        Dim total As Double = 0
        Dim row As DataGridViewRow
        If My.Computer.Keyboard.ShiftKeyDown Then
            For Each row In GridComp.Rows
                row.Cells(0).Value = 1 / GridComp.Rows.Count
            Next
        Else
            For Each row In GridComp.Rows
                total += row.Cells(0).Value
            Next
            For Each row In GridComp.Rows
                row.Cells(0).Value = row.Cells(0).Value / total
            Next
        End If
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles KButton3.Click
        Me.Close()
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles KButton2.Click

        Call Me.ValidateData()

        If Not Me.Label2.Text = DWSIM.App.GetLocalString("Erro") Then

            Dim comp As DWSIM.ClassesBasicasTermodinamica.Substancia
            Dim row As DataGridViewRow
            Dim mmtotal As Double = 0
            Dim mtotal As Double = 0
            If Me.RadioButton1.Checked Then
                Call Me.Button23_Click(sender, e)
                For Each row In Me.GridComp.Rows
                    Me.Componentes(row.HeaderCell.Tag).FracaoMolar = row.Cells(0).Value
                Next
                For Each comp In Me.Componentes.Values
                    mtotal += comp.FracaoMolar.GetValueOrDefault * comp.ConstantProperties.Molar_Weight
                Next
                For Each comp In Me.Componentes.Values
                    comp.FracaoMassica = comp.FracaoMolar.GetValueOrDefault * comp.ConstantProperties.Molar_Weight / mtotal
                Next
            ElseIf Me.RadioButton2.Checked Then
                Call Me.Button23_Click(sender, e)
                For Each row In Me.GridComp.Rows
                    Me.Componentes(row.HeaderCell.Tag).FracaoMassica = row.Cells(0).Value
                Next
                For Each comp In Me.Componentes.Values
                    mmtotal += comp.FracaoMassica.GetValueOrDefault / comp.ConstantProperties.Molar_Weight
                Next
                For Each comp In Me.Componentes.Values
                    comp.FracaoMolar = comp.FracaoMassica.GetValueOrDefault / comp.ConstantProperties.Molar_Weight / mmtotal
                Next
            ElseIf Me.RadioButton3.Checked Then
                Dim total As Double = 0
                For Each row In GridComp.Rows
                    total += row.Cells(0).Value
                Next
                Dim cv As New DWSIM.SistemasDeUnidades.Conversor
                Q = cv.ConverterParaSI(SU.spmp_molarflow, total)
                For Each row In Me.GridComp.Rows
                    Me.Componentes(row.HeaderCell.Tag).FracaoMolar = row.Cells(0).Value / total
                Next
                For Each comp In Me.Componentes.Values
                    mtotal += comp.FracaoMolar.GetValueOrDefault * comp.ConstantProperties.Molar_Weight
                Next
                For Each comp In Me.Componentes.Values
                    comp.FracaoMassica = comp.FracaoMolar.GetValueOrDefault * comp.ConstantProperties.Molar_Weight / mtotal
                Next
            ElseIf Me.RadioButton4.Checked Then
                Dim total As Double = 0
                For Each row In GridComp.Rows
                    total += row.Cells(0).Value
                Next
                Dim cv As New DWSIM.SistemasDeUnidades.Conversor
                W = cv.ConverterParaSI(SU.spmp_massflow, total)
                For Each row In Me.GridComp.Rows
                    Me.Componentes(row.HeaderCell.Tag).FracaoMassica = row.Cells(0).Value / total
                Next
                For Each comp In Me.Componentes.Values
                    mmtotal += comp.FracaoMassica.GetValueOrDefault / comp.ConstantProperties.Molar_Weight
                Next
                For Each comp In Me.Componentes.Values
                    comp.FracaoMolar = comp.FracaoMassica.GetValueOrDefault / comp.ConstantProperties.Molar_Weight / mmtotal
                Next
            End If

        End If

    End Sub

    Sub ValidateData()

        Me.Label2.Text = "OK"
        Me.Label2.ForeColor = Color.Green
        Dim row As DataGridViewRow
        For Each row In Me.GridComp.Rows
            If Not Double.TryParse(row.Cells(0).Value, New Double) Then
                Me.Label2.Text = DWSIM.App.GetLocalString("Erro")
                Me.Label2.ForeColor = Color.Red
                Exit Sub
            End If
        Next

    End Sub

    Private Sub GridComp_CellValueChanged(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles GridComp.CellValueChanged

        If loaded Then
            Try
                Dim v As Double = 0
                For Each r As DataGridViewRow In Me.GridComp.Rows
                    v += CDbl(r.Cells(0).Value)
                Next
                If Me.RadioButton1.Checked Or Me.RadioButton2.Checked Then
                    Me.Label3.Text = Format(v, "#0.0000")
                ElseIf Me.RadioButton3.Checked Then
                    Me.Label3.Text = Format(v, NF) & " " & SU.spmp_molarflow
                Else
                    Me.Label3.Text = Format(v, NF) & " " & SU.spmp_massflow
                End If
                Me.Label3.ForeColor = Color.SlateBlue
            Catch ex As Exception
                Me.Label3.Text = DWSIM.App.GetLocalString("indefinido")
                Me.Label3.ForeColor = Color.Red
            End Try
        End If


    End Sub

    Private Sub RadioButton_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButton1.CheckedChanged,
                                                                                                                RadioButton2.CheckedChanged,
                                                                                                                RadioButton3.CheckedChanged,
                                                                                                                RadioButton4.CheckedChanged

        If Me.loaded Then

            Call Me.ValidateData()

            If Not Me.Label2.Text = DWSIM.App.GetLocalString("Erro") Then

                Call Me.Button23_Click(sender, e)

                Dim row As DataGridViewRow
                If Me.RadioButton1.Checked Then
                    For Each row In Me.GridComp.Rows
                        row.Cells(0).Value = Me.Componentes(row.HeaderCell.Tag).FracaoMolar
                    Next
                ElseIf Me.RadioButton2.Checked Then
                    For Each row In Me.GridComp.Rows
                        row.Cells(0).Value = Me.Componentes(row.HeaderCell.Tag).FracaoMassica
                    Next
                ElseIf Me.RadioButton3.Checked Then
                    Dim cv As New DWSIM.SistemasDeUnidades.Conversor
                    For Each row In Me.GridComp.Rows
                        row.Cells(0).Value = cv.ConverterDoSI(SU.spmp_molarflow, Me.Componentes(row.HeaderCell.Tag).FracaoMolar.GetValueOrDefault * Q)
                    Next
                ElseIf Me.RadioButton4.Checked Then
                    Dim cv As New DWSIM.SistemasDeUnidades.Conversor
                    For Each row In Me.GridComp.Rows
                        row.Cells(0).Value = cv.ConverterDoSI(SU.spmp_massflow, Me.Componentes(row.HeaderCell.Tag).FracaoMassica.GetValueOrDefault * W)
                    Next
                End If

            End If

        End If

    End Sub

    Private Sub GridComp_KeyDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles GridComp.KeyDown
        If e.KeyCode = Keys.V And e.Modifiers = Keys.Control Then PasteData(GridComp)
    End Sub

    Public Sub PasteData(ByRef dgv As DataGridView)
        Dim tArr() As String
        Dim arT() As String
        Dim i, ii As Integer
        Dim c, cc, r As Integer

        tArr = Clipboard.GetText().Split(Environment.NewLine)

        If dgv.SelectedCells.Count > 0 Then
            r = dgv.SelectedCells(0).RowIndex
            c = dgv.SelectedCells(0).ColumnIndex
        Else
            r = 0
            c = 0
        End If
        For i = 0 To tArr.Length - 2
            If tArr(i) <> "" Then
                arT = tArr(i).Split(vbTab)
                For ii = 0 To arT.Length - 1
                    If r > dgv.Rows.Count - 1 Then
                        dgv.Rows(0).Cells(0).Selected = True
                    End If
                Next
                r = r + 1
            End If
        Next
        If dgv.SelectedCells.Count > 0 Then
            r = dgv.SelectedCells(0).RowIndex
            c = dgv.SelectedCells(0).ColumnIndex
        Else
            r = 0
            c = 0
        End If
        For i = 0 To tArr.Length - 2
            If tArr(i) <> "" Then
                arT = tArr(i).Split(vbTab)
                cc = c
                For ii = 0 To arT.Length - 1
                    cc = 0
                    If cc > dgv.ColumnCount - 1 Then Exit For
                    dgv.Item(cc, r).Value = arT(ii).TrimStart
                    cc = cc + 1
                Next
                r = r + 1
            End If
        Next

    End Sub

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub
End Class