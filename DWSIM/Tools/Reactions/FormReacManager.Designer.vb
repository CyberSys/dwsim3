<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormReacManager
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FormReacManager))
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.GridRSets = New System.Windows.Forms.DataGridView()
        Me.Column1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column3 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ToolStrip2 = New System.Windows.Forms.ToolStrip()
        Me.ToolStripButton1 = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripButton3 = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripButton5 = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripButton2 = New System.Windows.Forms.ToolStripButton()
        Me.GroupBox4 = New System.Windows.Forms.GroupBox()
        Me.GridRxns = New System.Windows.Forms.DataGridView()
        Me.DataGridViewTextBoxColumn1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DataGridViewTextBoxColumn3 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ColumnID = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ToolStrip1 = New System.Windows.Forms.ToolStrip()
        Me.ToolStripDropDownButton1 = New System.Windows.Forms.ToolStripDropDownButton()
        Me.Convers�oToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.Equil�brioToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.Cin�ticaToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.Heterog�neaCatal�ticaToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripButton6 = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripButton7 = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripButton8 = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripButton4 = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripButton9 = New System.Windows.Forms.ToolStripButton()
        Me.OpenFileDialog1 = New System.Windows.Forms.OpenFileDialog()
        Me.SaveFileDialog1 = New System.Windows.Forms.SaveFileDialog()
        Me.GroupBox2.SuspendLayout()
        CType(Me.GridRSets, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ToolStrip2.SuspendLayout()
        Me.GroupBox4.SuspendLayout()
        CType(Me.GridRxns, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ToolStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'GroupBox2
        '
        resources.ApplyResources(Me.GroupBox2, "GroupBox2")
        Me.GroupBox2.Controls.Add(Me.GridRSets)
        Me.GroupBox2.Controls.Add(Me.ToolStrip2)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.TabStop = False
        '
        'GridRSets
        '
        resources.ApplyResources(Me.GridRSets, "GridRSets")
        Me.GridRSets.AllowUserToAddRows = False
        Me.GridRSets.AllowUserToDeleteRows = False
        Me.GridRSets.AllowUserToResizeRows = False
        Me.GridRSets.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.GridRSets.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.GridRSets.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Column1, Me.Column3, Me.Column2})
        Me.GridRSets.MultiSelect = False
        Me.GridRSets.Name = "GridRSets"
        Me.GridRSets.ReadOnly = True
        Me.GridRSets.RowHeadersVisible = False
        Me.GridRSets.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.GridRSets.ShowEditingIcon = False
        Me.GridRSets.ShowRowErrors = False
        '
        'Column1
        '
        resources.ApplyResources(Me.Column1, "Column1")
        Me.Column1.Name = "Column1"
        Me.Column1.ReadOnly = True
        '
        'Column3
        '
        resources.ApplyResources(Me.Column3, "Column3")
        Me.Column3.Name = "Column3"
        Me.Column3.ReadOnly = True
        '
        'Column2
        '
        resources.ApplyResources(Me.Column2, "Column2")
        Me.Column2.Name = "Column2"
        Me.Column2.ReadOnly = True
        '
        'ToolStrip2
        '
        resources.ApplyResources(Me.ToolStrip2, "ToolStrip2")
        Me.ToolStrip2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
        Me.ToolStrip2.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripButton1, Me.ToolStripButton3, Me.ToolStripButton5, Me.ToolStripButton2})
        Me.ToolStrip2.Name = "ToolStrip2"
        '
        'ToolStripButton1
        '
        resources.ApplyResources(Me.ToolStripButton1, "ToolStripButton1")
        Me.ToolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton1.Image = Global.DWSIM.My.Resources.Resources.add
        Me.ToolStripButton1.Name = "ToolStripButton1"
        '
        'ToolStripButton3
        '
        resources.ApplyResources(Me.ToolStripButton3, "ToolStripButton3")
        Me.ToolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton3.Image = Global.DWSIM.My.Resources.Resources.page_white_gear
        Me.ToolStripButton3.Name = "ToolStripButton3"
        '
        'ToolStripButton5
        '
        resources.ApplyResources(Me.ToolStripButton5, "ToolStripButton5")
        Me.ToolStripButton5.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton5.Image = Global.DWSIM.My.Resources.Resources.copy
        Me.ToolStripButton5.Name = "ToolStripButton5"
        '
        'ToolStripButton2
        '
        resources.ApplyResources(Me.ToolStripButton2, "ToolStripButton2")
        Me.ToolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton2.Image = Global.DWSIM.My.Resources.Resources.delete1
        Me.ToolStripButton2.Name = "ToolStripButton2"
        '
        'GroupBox4
        '
        resources.ApplyResources(Me.GroupBox4, "GroupBox4")
        Me.GroupBox4.Controls.Add(Me.GridRxns)
        Me.GroupBox4.Controls.Add(Me.ToolStrip1)
        Me.GroupBox4.Name = "GroupBox4"
        Me.GroupBox4.TabStop = False
        '
        'GridRxns
        '
        resources.ApplyResources(Me.GridRxns, "GridRxns")
        Me.GridRxns.AllowUserToAddRows = False
        Me.GridRxns.AllowUserToDeleteRows = False
        Me.GridRxns.AllowUserToResizeRows = False
        Me.GridRxns.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.GridRxns.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.GridRxns.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.DataGridViewTextBoxColumn1, Me.DataGridViewTextBoxColumn2, Me.DataGridViewTextBoxColumn3, Me.ColumnID})
        Me.GridRxns.MultiSelect = False
        Me.GridRxns.Name = "GridRxns"
        Me.GridRxns.ReadOnly = True
        Me.GridRxns.RowHeadersVisible = False
        Me.GridRxns.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.GridRxns.ShowEditingIcon = False
        Me.GridRxns.ShowRowErrors = False
        '
        'DataGridViewTextBoxColumn1
        '
        resources.ApplyResources(Me.DataGridViewTextBoxColumn1, "DataGridViewTextBoxColumn1")
        Me.DataGridViewTextBoxColumn1.Name = "DataGridViewTextBoxColumn1"
        Me.DataGridViewTextBoxColumn1.ReadOnly = True
        '
        'DataGridViewTextBoxColumn2
        '
        resources.ApplyResources(Me.DataGridViewTextBoxColumn2, "DataGridViewTextBoxColumn2")
        Me.DataGridViewTextBoxColumn2.Name = "DataGridViewTextBoxColumn2"
        Me.DataGridViewTextBoxColumn2.ReadOnly = True
        '
        'DataGridViewTextBoxColumn3
        '
        resources.ApplyResources(Me.DataGridViewTextBoxColumn3, "DataGridViewTextBoxColumn3")
        Me.DataGridViewTextBoxColumn3.Name = "DataGridViewTextBoxColumn3"
        Me.DataGridViewTextBoxColumn3.ReadOnly = True
        '
        'ColumnID
        '
        resources.ApplyResources(Me.ColumnID, "ColumnID")
        Me.ColumnID.Name = "ColumnID"
        Me.ColumnID.ReadOnly = True
        '
        'ToolStrip1
        '
        resources.ApplyResources(Me.ToolStrip1, "ToolStrip1")
        Me.ToolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
        Me.ToolStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripDropDownButton1, Me.ToolStripButton6, Me.ToolStripButton7, Me.ToolStripButton8, Me.ToolStripButton4, Me.ToolStripButton9})
        Me.ToolStrip1.Name = "ToolStrip1"
        '
        'ToolStripDropDownButton1
        '
        resources.ApplyResources(Me.ToolStripDropDownButton1, "ToolStripDropDownButton1")
        Me.ToolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripDropDownButton1.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.Convers�oToolStripMenuItem, Me.Equil�brioToolStripMenuItem, Me.Cin�ticaToolStripMenuItem, Me.Heterog�neaCatal�ticaToolStripMenuItem})
        Me.ToolStripDropDownButton1.Image = Global.DWSIM.My.Resources.Resources.add
        Me.ToolStripDropDownButton1.Name = "ToolStripDropDownButton1"
        Me.ToolStripDropDownButton1.ShowDropDownArrow = False
        '
        'Convers�oToolStripMenuItem
        '
        resources.ApplyResources(Me.Convers�oToolStripMenuItem, "Convers�oToolStripMenuItem")
        Me.Convers�oToolStripMenuItem.Image = Global.DWSIM.My.Resources.Resources.Experiments_Badge
        Me.Convers�oToolStripMenuItem.Name = "Convers�oToolStripMenuItem"
        '
        'Equil�brioToolStripMenuItem
        '
        resources.ApplyResources(Me.Equil�brioToolStripMenuItem, "Equil�brioToolStripMenuItem")
        Me.Equil�brioToolStripMenuItem.Image = Global.DWSIM.My.Resources.Resources.Experiments_Badge
        Me.Equil�brioToolStripMenuItem.Name = "Equil�brioToolStripMenuItem"
        '
        'Cin�ticaToolStripMenuItem
        '
        resources.ApplyResources(Me.Cin�ticaToolStripMenuItem, "Cin�ticaToolStripMenuItem")
        Me.Cin�ticaToolStripMenuItem.Image = Global.DWSIM.My.Resources.Resources.Experiments_Badge
        Me.Cin�ticaToolStripMenuItem.Name = "Cin�ticaToolStripMenuItem"
        '
        'Heterog�neaCatal�ticaToolStripMenuItem
        '
        resources.ApplyResources(Me.Heterog�neaCatal�ticaToolStripMenuItem, "Heterog�neaCatal�ticaToolStripMenuItem")
        Me.Heterog�neaCatal�ticaToolStripMenuItem.Image = Global.DWSIM.My.Resources.Resources.Experiments_Badge
        Me.Heterog�neaCatal�ticaToolStripMenuItem.Name = "Heterog�neaCatal�ticaToolStripMenuItem"
        '
        'ToolStripButton6
        '
        resources.ApplyResources(Me.ToolStripButton6, "ToolStripButton6")
        Me.ToolStripButton6.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton6.Image = Global.DWSIM.My.Resources.Resources.page_white_gear
        Me.ToolStripButton6.Name = "ToolStripButton6"
        '
        'ToolStripButton7
        '
        resources.ApplyResources(Me.ToolStripButton7, "ToolStripButton7")
        Me.ToolStripButton7.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton7.Image = Global.DWSIM.My.Resources.Resources.copy
        Me.ToolStripButton7.Name = "ToolStripButton7"
        '
        'ToolStripButton8
        '
        resources.ApplyResources(Me.ToolStripButton8, "ToolStripButton8")
        Me.ToolStripButton8.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton8.Image = Global.DWSIM.My.Resources.Resources.delete1
        Me.ToolStripButton8.Name = "ToolStripButton8"
        '
        'ToolStripButton4
        '
        resources.ApplyResources(Me.ToolStripButton4, "ToolStripButton4")
        Me.ToolStripButton4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton4.Image = Global.DWSIM.My.Resources.Resources.arrow_left
        Me.ToolStripButton4.Name = "ToolStripButton4"
        '
        'ToolStripButton9
        '
        resources.ApplyResources(Me.ToolStripButton9, "ToolStripButton9")
        Me.ToolStripButton9.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton9.Image = Global.DWSIM.My.Resources.Resources.arrow_right3
        Me.ToolStripButton9.Name = "ToolStripButton9"
        '
        'OpenFileDialog1
        '
        Me.OpenFileDialog1.DefaultExt = "xml"
        resources.ApplyResources(Me.OpenFileDialog1, "OpenFileDialog1")
        Me.OpenFileDialog1.SupportMultiDottedExtensions = True
        '
        'SaveFileDialog1
        '
        Me.SaveFileDialog1.DefaultExt = "xml"
        resources.ApplyResources(Me.SaveFileDialog1, "SaveFileDialog1")
        Me.SaveFileDialog1.FilterIndex = 2
        Me.SaveFileDialog1.RestoreDirectory = True
        Me.SaveFileDialog1.SupportMultiDottedExtensions = True
        '
        'FormReacManager
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.GroupBox4)
        Me.Controls.Add(Me.GroupBox2)
        Me.DoubleBuffered = True
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.Name = "FormReacManager"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        CType(Me.GridRSets, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ToolStrip2.ResumeLayout(False)
        Me.ToolStrip2.PerformLayout()
        Me.GroupBox4.ResumeLayout(False)
        Me.GroupBox4.PerformLayout()
        CType(Me.GridRxns, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ToolStrip1.ResumeLayout(False)
        Me.ToolStrip1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Public WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Public WithEvents GroupBox4 As System.Windows.Forms.GroupBox
    Public WithEvents OpenFileDialog1 As System.Windows.Forms.OpenFileDialog
    Public WithEvents SaveFileDialog1 As System.Windows.Forms.SaveFileDialog
    Public WithEvents GridRSets As System.Windows.Forms.DataGridView
    Public WithEvents Column1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents Column3 As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents Column2 As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents ToolStrip2 As System.Windows.Forms.ToolStrip
    Public WithEvents ToolStripButton1 As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripButton3 As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripButton5 As System.Windows.Forms.ToolStripButton
    Public WithEvents ToolStripButton2 As System.Windows.Forms.ToolStripButton
    Public WithEvents GridRxns As System.Windows.Forms.DataGridView
    Public WithEvents DataGridViewTextBoxColumn1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents DataGridViewTextBoxColumn2 As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents DataGridViewTextBoxColumn3 As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents ColumnID As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents ToolStrip1 As System.Windows.Forms.ToolStrip
    Friend WithEvents ToolStripDropDownButton1 As System.Windows.Forms.ToolStripDropDownButton
    Friend WithEvents Convers�oToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents Equil�brioToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents Cin�ticaToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents Heterog�neaCatal�ticaToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripButton6 As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripButton7 As System.Windows.Forms.ToolStripButton
    Public WithEvents ToolStripButton8 As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripButton4 As System.Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripButton9 As System.Windows.Forms.ToolStripButton
End Class
