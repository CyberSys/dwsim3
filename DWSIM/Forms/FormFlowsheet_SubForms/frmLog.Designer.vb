<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmLog
    Inherits WeifenLuo.WinFormsUI.Docking.DockContent

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmLog))
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.ToolStripContainer1 = New System.Windows.Forms.ToolStripContainer()
        Me.Grid1 = New System.Windows.Forms.DataGridView()
        Me.Imagem = New System.Windows.Forms.DataGridViewImageColumn()
        Me.Indice = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Data = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Tipo = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Mensagem = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ToolStrip1 = New System.Windows.Forms.ToolStrip()
        Me.ToolStripButton5 = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripButton3 = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripButton1 = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator()
        Me.ToolStripButton2 = New System.Windows.Forms.ToolStripButton()
        Me.dckMenu = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.FloatToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DockLeftToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DockRightToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DockTopToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DockBottomToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DockLeftAutoHideToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DockRightAutoHideToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DockTopAutoHideToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DockBottomAutoHideToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DocumentToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripContainer1.ContentPanel.SuspendLayout()
        Me.ToolStripContainer1.TopToolStripPanel.SuspendLayout()
        Me.ToolStripContainer1.SuspendLayout()
        CType(Me.Grid1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ToolStrip1.SuspendLayout()
        Me.dckMenu.SuspendLayout()
        Me.SuspendLayout()
        '
        'ToolStripContainer1
        '
        '
        'ToolStripContainer1.ContentPanel
        '
        Me.ToolStripContainer1.ContentPanel.Controls.Add(Me.Grid1)
        resources.ApplyResources(Me.ToolStripContainer1.ContentPanel, "ToolStripContainer1.ContentPanel")
        resources.ApplyResources(Me.ToolStripContainer1, "ToolStripContainer1")
        Me.ToolStripContainer1.Name = "ToolStripContainer1"
        '
        'ToolStripContainer1.TopToolStripPanel
        '
        Me.ToolStripContainer1.TopToolStripPanel.Controls.Add(Me.ToolStrip1)
        '
        'Grid1
        '
        Me.Grid1.AllowUserToAddRows = False
        Me.Grid1.AllowUserToDeleteRows = False
        Me.Grid1.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells
        Me.Grid1.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal
        Me.Grid1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.Grid1.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Imagem, Me.Indice, Me.Data, Me.Tipo, Me.Mensagem})
        resources.ApplyResources(Me.Grid1, "Grid1")
        Me.Grid1.Name = "Grid1"
        Me.Grid1.ReadOnly = True
        Me.Grid1.RowHeadersVisible = False
        Me.Grid1.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders
        Me.Grid1.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        Me.Grid1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        '
        'Imagem
        '
        Me.Imagem.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells
        Me.Imagem.Description = " "
        resources.ApplyResources(Me.Imagem, "Imagem")
        Me.Imagem.Name = "Imagem"
        Me.Imagem.ReadOnly = True
        Me.Imagem.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.Imagem.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        '
        'Indice
        '
        resources.ApplyResources(Me.Indice, "Indice")
        Me.Indice.Name = "Indice"
        Me.Indice.ReadOnly = True
        '
        'Data
        '
        Me.Data.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells
        Me.Data.FillWeight = 40.0!
        resources.ApplyResources(Me.Data, "Data")
        Me.Data.Name = "Data"
        Me.Data.ReadOnly = True
        '
        'Tipo
        '
        Me.Tipo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells
        Me.Tipo.FillWeight = 30.0!
        resources.ApplyResources(Me.Tipo, "Tipo")
        Me.Tipo.Name = "Tipo"
        Me.Tipo.ReadOnly = True
        Me.Tipo.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        '
        'Mensagem
        '
        Me.Mensagem.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        DataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        Me.Mensagem.DefaultCellStyle = DataGridViewCellStyle1
        resources.ApplyResources(Me.Mensagem, "Mensagem")
        Me.Mensagem.Name = "Mensagem"
        Me.Mensagem.ReadOnly = True
        '
        'ToolStrip1
        '
        resources.ApplyResources(Me.ToolStrip1, "ToolStrip1")
        Me.ToolStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripButton5, Me.ToolStripButton3, Me.ToolStripButton1, Me.ToolStripSeparator1, Me.ToolStripButton2})
        Me.ToolStrip1.Name = "ToolStrip1"
        Me.ToolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional
        Me.ToolStrip1.Stretch = True
        '
        'ToolStripButton5
        '
        Me.ToolStripButton5.Checked = True
        Me.ToolStripButton5.CheckOnClick = True
        Me.ToolStripButton5.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ToolStripButton5.Image = Global.DWSIM.My.Resources.Resources.information
        resources.ApplyResources(Me.ToolStripButton5, "ToolStripButton5")
        Me.ToolStripButton5.Name = "ToolStripButton5"
        '
        'ToolStripButton3
        '
        Me.ToolStripButton3.Checked = True
        Me.ToolStripButton3.CheckOnClick = True
        Me.ToolStripButton3.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ToolStripButton3.Image = Global.DWSIM.My.Resources.Resources.exclamation
        resources.ApplyResources(Me.ToolStripButton3, "ToolStripButton3")
        Me.ToolStripButton3.Name = "ToolStripButton3"
        '
        'ToolStripButton1
        '
        Me.ToolStripButton1.Checked = True
        Me.ToolStripButton1.CheckOnClick = True
        Me.ToolStripButton1.CheckState = System.Windows.Forms.CheckState.Checked
        Me.ToolStripButton1.Image = Global.DWSIM.My.Resources.Resources._error
        resources.ApplyResources(Me.ToolStripButton1, "ToolStripButton1")
        Me.ToolStripButton1.Name = "ToolStripButton1"
        '
        'ToolStripSeparator1
        '
        Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
        resources.ApplyResources(Me.ToolStripSeparator1, "ToolStripSeparator1")
        '
        'ToolStripButton2
        '
        Me.ToolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButton2.Image = Global.DWSIM.My.Resources.Resources.cross
        resources.ApplyResources(Me.ToolStripButton2, "ToolStripButton2")
        Me.ToolStripButton2.Name = "ToolStripButton2"
        '
        'dckMenu
        '
        Me.dckMenu.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.FloatToolStripMenuItem, Me.DockLeftToolStripMenuItem, Me.DockRightToolStripMenuItem, Me.DockTopToolStripMenuItem, Me.DockBottomToolStripMenuItem, Me.DockLeftAutoHideToolStripMenuItem, Me.DockRightAutoHideToolStripMenuItem, Me.DockTopAutoHideToolStripMenuItem, Me.DockBottomAutoHideToolStripMenuItem, Me.DocumentToolStripMenuItem})
        Me.dckMenu.Name = "dckMenu"
        resources.ApplyResources(Me.dckMenu, "dckMenu")
        '
        'FloatToolStripMenuItem
        '
        Me.FloatToolStripMenuItem.Name = "FloatToolStripMenuItem"
        resources.ApplyResources(Me.FloatToolStripMenuItem, "FloatToolStripMenuItem")
        '
        'DockLeftToolStripMenuItem
        '
        Me.DockLeftToolStripMenuItem.Name = "DockLeftToolStripMenuItem"
        resources.ApplyResources(Me.DockLeftToolStripMenuItem, "DockLeftToolStripMenuItem")
        '
        'DockRightToolStripMenuItem
        '
        Me.DockRightToolStripMenuItem.Name = "DockRightToolStripMenuItem"
        resources.ApplyResources(Me.DockRightToolStripMenuItem, "DockRightToolStripMenuItem")
        '
        'DockTopToolStripMenuItem
        '
        Me.DockTopToolStripMenuItem.Name = "DockTopToolStripMenuItem"
        resources.ApplyResources(Me.DockTopToolStripMenuItem, "DockTopToolStripMenuItem")
        '
        'DockBottomToolStripMenuItem
        '
        Me.DockBottomToolStripMenuItem.Name = "DockBottomToolStripMenuItem"
        resources.ApplyResources(Me.DockBottomToolStripMenuItem, "DockBottomToolStripMenuItem")
        '
        'DockLeftAutoHideToolStripMenuItem
        '
        Me.DockLeftAutoHideToolStripMenuItem.Name = "DockLeftAutoHideToolStripMenuItem"
        resources.ApplyResources(Me.DockLeftAutoHideToolStripMenuItem, "DockLeftAutoHideToolStripMenuItem")
        '
        'DockRightAutoHideToolStripMenuItem
        '
        Me.DockRightAutoHideToolStripMenuItem.Name = "DockRightAutoHideToolStripMenuItem"
        resources.ApplyResources(Me.DockRightAutoHideToolStripMenuItem, "DockRightAutoHideToolStripMenuItem")
        '
        'DockTopAutoHideToolStripMenuItem
        '
        Me.DockTopAutoHideToolStripMenuItem.Name = "DockTopAutoHideToolStripMenuItem"
        resources.ApplyResources(Me.DockTopAutoHideToolStripMenuItem, "DockTopAutoHideToolStripMenuItem")
        '
        'DockBottomAutoHideToolStripMenuItem
        '
        Me.DockBottomAutoHideToolStripMenuItem.Name = "DockBottomAutoHideToolStripMenuItem"
        resources.ApplyResources(Me.DockBottomAutoHideToolStripMenuItem, "DockBottomAutoHideToolStripMenuItem")
        '
        'DocumentToolStripMenuItem
        '
        Me.DocumentToolStripMenuItem.Name = "DocumentToolStripMenuItem"
        resources.ApplyResources(Me.DocumentToolStripMenuItem, "DocumentToolStripMenuItem")
        '
        'frmLog
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.CloseButton = False
        Me.CloseButtonVisible = False
        Me.Controls.Add(Me.ToolStripContainer1)
        Me.Name = "frmLog"
        Me.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockBottom
        Me.TabPageContextMenuStrip = Me.dckMenu
        Me.ToolStripContainer1.ContentPanel.ResumeLayout(False)
        Me.ToolStripContainer1.TopToolStripPanel.ResumeLayout(False)
        Me.ToolStripContainer1.TopToolStripPanel.PerformLayout()
        Me.ToolStripContainer1.ResumeLayout(False)
        Me.ToolStripContainer1.PerformLayout()
        CType(Me.Grid1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ToolStrip1.ResumeLayout(False)
        Me.ToolStrip1.PerformLayout()
        Me.dckMenu.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Public WithEvents ToolStrip1 As System.Windows.Forms.ToolStrip
    Public WithEvents ToolStripContainer1 As System.Windows.Forms.ToolStripContainer
    Public WithEvents ToolStripButton3 As System.Windows.Forms.ToolStripButton
    Public WithEvents ToolStripButton5 As System.Windows.Forms.ToolStripButton
    Public WithEvents Grid1 As System.Windows.Forms.DataGridView
    Public WithEvents ToolStripButton1 As System.Windows.Forms.ToolStripButton
    Public WithEvents ToolStripSeparator1 As System.Windows.Forms.ToolStripSeparator
    Public WithEvents ToolStripButton2 As System.Windows.Forms.ToolStripButton
    Friend WithEvents Imagem As System.Windows.Forms.DataGridViewImageColumn
    Friend WithEvents Indice As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Data As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Tipo As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Mensagem As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents dckMenu As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents FloatToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents DockLeftToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents DockRightToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents DockTopToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents DockBottomToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents DockLeftAutoHideToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents DockRightAutoHideToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents DockTopAutoHideToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents DockBottomAutoHideToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents DocumentToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem

End Class
