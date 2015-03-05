﻿Public Class FlowsheetUOViewerForm

    Inherits WeifenLuo.WinFormsUI.Docking.DockContent

    Public fsuo As DWSIM.SimulationObjects.UnitOps.Flowsheet

    Private Sub FlowsheetUOViewerForm_Load(sender As Object, e As EventArgs) Handles Me.Load

        fsuo.Fsheet.FormSurface.Flowsheet = fsuo.Fsheet
        Me.Controls.Add(fsuo.Fsheet.dckPanel)
        fsuo.Fsheet.dckPanel.Invalidate()
        fsuo.Fsheet.FormSurface.FlowsheetDesignSurface.Zoom = 1.0#
        fsuo.Fsheet.FormSurface.FlowsheetDesignSurface.Center()
        fsuo.Fsheet.FormSurface.FlowsheetDesignSurface.Invalidate()
        fsuo.Fsheet.FormSurface.CMS_Sel.Enabled = False
        fsuo.Fsheet.FormSurface.TableLayoutPanel1.RowStyles(1).Height = 0
        fsuo.Fsheet.dckPanel.Invalidate()
        Me.Invalidate()

    End Sub

End Class