﻿<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class AboutBoxMONO
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
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(AboutBoxMONO))
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.Version = New System.Windows.Forms.Label()
        Me.Copyright = New System.Windows.Forms.Label()
        Me.FaTabStrip1 = New FarsiLibrary.Win.FATabStrip()
        Me.FaTabStripItem1 = New FarsiLibrary.Win.FATabStripItem()
        Me.Lblcpusimd = New System.Windows.Forms.Label()
        Me.Lblcpuinfo = New System.Windows.Forms.Label()
        Me.Label13 = New System.Windows.Forms.Label()
        Me.Label12 = New System.Windows.Forms.Label()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.LinkLabel1 = New System.Windows.Forms.LinkLabel()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.LinkLabel2 = New System.Windows.Forms.LinkLabel()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.LblCLRInfo = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.LabelLicense = New System.Windows.Forms.Label()
        Me.LblOSInfo = New System.Windows.Forms.Label()
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.FaTabStripItem2 = New FarsiLibrary.Win.FATabStripItem()
        Me.DataGridView1 = New System.Windows.Forms.DataGridView()
        Me.FaTabStripItem3 = New FarsiLibrary.Win.FATabStripItem()
        Me.AssemblyInfoListView = New System.Windows.Forms.ListView()
        Me.colAssemblyName = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.colAssemblyVersion = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.colAssemblyBuilt = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.colAssemblyCodeBase = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.FaTabStripItem4 = New FarsiLibrary.Win.FATabStripItem()
        Me.AssemblyDetailsListView = New System.Windows.Forms.ListView()
        Me.ColumnHeader1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader2 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.AssemblyNamesComboBox = New System.Windows.Forms.ComboBox()
        Me.Column1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column3 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column4 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column5 = New System.Windows.Forms.DataGridViewLinkColumn()
        Me.Column6 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column7 = New System.Windows.Forms.DataGridViewLinkColumn()
        CType(Me.FaTabStrip1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.FaTabStrip1.SuspendLayout()
        Me.FaTabStripItem1.SuspendLayout()
        Me.FaTabStripItem2.SuspendLayout()
        CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.FaTabStripItem3.SuspendLayout()
        Me.FaTabStripItem4.SuspendLayout()
        Me.SuspendLayout()
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.BackColor = System.Drawing.Color.Transparent
        Me.Label1.Name = "Label1"
        '
        'Button1
        '
        resources.ApplyResources(Me.Button1, "Button1")
        Me.Button1.Name = "Button1"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'Version
        '
        resources.ApplyResources(Me.Version, "Version")
        Me.Version.BackColor = System.Drawing.Color.Transparent
        Me.Version.Name = "Version"
        '
        'Copyright
        '
        resources.ApplyResources(Me.Copyright, "Copyright")
        Me.Copyright.BackColor = System.Drawing.Color.Transparent
        Me.Copyright.Name = "Copyright"
        '
        'FaTabStrip1
        '
        resources.ApplyResources(Me.FaTabStrip1, "FaTabStrip1")
        Me.FaTabStrip1.AlwaysShowClose = False
        Me.FaTabStrip1.AlwaysShowMenuGlyph = False
        Me.FaTabStrip1.Items.AddRange(New FarsiLibrary.Win.FATabStripItem() {Me.FaTabStripItem1, Me.FaTabStripItem2, Me.FaTabStripItem3, Me.FaTabStripItem4})
        Me.FaTabStrip1.Name = "FaTabStrip1"
        Me.FaTabStrip1.SelectedItem = Me.FaTabStripItem1
        '
        'FaTabStripItem1
        '
        resources.ApplyResources(Me.FaTabStripItem1, "FaTabStripItem1")
        Me.FaTabStripItem1.CanClose = False
        Me.FaTabStripItem1.Controls.Add(Me.Lblcpusimd)
        Me.FaTabStripItem1.Controls.Add(Me.Lblcpuinfo)
        Me.FaTabStripItem1.Controls.Add(Me.Label13)
        Me.FaTabStripItem1.Controls.Add(Me.Label12)
        Me.FaTabStripItem1.Controls.Add(Me.Label9)
        Me.FaTabStripItem1.Controls.Add(Me.LinkLabel1)
        Me.FaTabStripItem1.Controls.Add(Me.Label10)
        Me.FaTabStripItem1.Controls.Add(Me.Label2)
        Me.FaTabStripItem1.Controls.Add(Me.Label8)
        Me.FaTabStripItem1.Controls.Add(Me.LinkLabel2)
        Me.FaTabStripItem1.Controls.Add(Me.Label3)
        Me.FaTabStripItem1.Controls.Add(Me.Label5)
        Me.FaTabStripItem1.Controls.Add(Me.LblCLRInfo)
        Me.FaTabStripItem1.Controls.Add(Me.Label6)
        Me.FaTabStripItem1.Controls.Add(Me.Label4)
        Me.FaTabStripItem1.Controls.Add(Me.LabelLicense)
        Me.FaTabStripItem1.Controls.Add(Me.LblOSInfo)
        Me.FaTabStripItem1.Controls.Add(Me.TextBox1)
        Me.FaTabStripItem1.Controls.Add(Me.Label7)
        Me.FaTabStripItem1.IsDrawn = True
        Me.FaTabStripItem1.Name = "FaTabStripItem1"
        Me.FaTabStripItem1.Selected = True
        '
        'Lblcpusimd
        '
        resources.ApplyResources(Me.Lblcpusimd, "Lblcpusimd")
        Me.Lblcpusimd.BackColor = System.Drawing.Color.Transparent
        Me.Lblcpusimd.Name = "Lblcpusimd"
        '
        'Lblcpuinfo
        '
        resources.ApplyResources(Me.Lblcpuinfo, "Lblcpuinfo")
        Me.Lblcpuinfo.BackColor = System.Drawing.Color.Transparent
        Me.Lblcpuinfo.Name = "Lblcpuinfo"
        '
        'Label13
        '
        resources.ApplyResources(Me.Label13, "Label13")
        Me.Label13.BackColor = System.Drawing.Color.Transparent
        Me.Label13.Name = "Label13"
        '
        'Label12
        '
        resources.ApplyResources(Me.Label12, "Label12")
        Me.Label12.BackColor = System.Drawing.Color.Transparent
        Me.Label12.Name = "Label12"
        '
        'Label9
        '
        resources.ApplyResources(Me.Label9, "Label9")
        Me.Label9.BackColor = System.Drawing.Color.Transparent
        Me.Label9.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label9.Name = "Label9"
        '
        'LinkLabel1
        '
        resources.ApplyResources(Me.LinkLabel1, "LinkLabel1")
        Me.LinkLabel1.BackColor = System.Drawing.Color.Transparent
        Me.LinkLabel1.ForeColor = System.Drawing.SystemColors.ControlText
        Me.LinkLabel1.Name = "LinkLabel1"
        Me.LinkLabel1.TabStop = True
        '
        'Label10
        '
        resources.ApplyResources(Me.Label10, "Label10")
        Me.Label10.BackColor = System.Drawing.Color.Transparent
        Me.Label10.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label10.Name = "Label10"
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.BackColor = System.Drawing.Color.Transparent
        Me.Label2.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label2.Name = "Label2"
        '
        'Label8
        '
        resources.ApplyResources(Me.Label8, "Label8")
        Me.Label8.BackColor = System.Drawing.Color.Transparent
        Me.Label8.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label8.Name = "Label8"
        '
        'LinkLabel2
        '
        resources.ApplyResources(Me.LinkLabel2, "LinkLabel2")
        Me.LinkLabel2.BackColor = System.Drawing.Color.Transparent
        Me.LinkLabel2.ForeColor = System.Drawing.SystemColors.ControlText
        Me.LinkLabel2.Name = "LinkLabel2"
        Me.LinkLabel2.TabStop = True
        '
        'Label3
        '
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.BackColor = System.Drawing.Color.Transparent
        Me.Label3.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label3.Name = "Label3"
        '
        'Label5
        '
        resources.ApplyResources(Me.Label5, "Label5")
        Me.Label5.BackColor = System.Drawing.Color.Transparent
        Me.Label5.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label5.Name = "Label5"
        '
        'LblCLRInfo
        '
        resources.ApplyResources(Me.LblCLRInfo, "LblCLRInfo")
        Me.LblCLRInfo.BackColor = System.Drawing.Color.Transparent
        Me.LblCLRInfo.ForeColor = System.Drawing.SystemColors.ControlText
        Me.LblCLRInfo.Name = "LblCLRInfo"
        '
        'Label6
        '
        resources.ApplyResources(Me.Label6, "Label6")
        Me.Label6.BackColor = System.Drawing.Color.Transparent
        Me.Label6.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label6.Name = "Label6"
        '
        'Label4
        '
        resources.ApplyResources(Me.Label4, "Label4")
        Me.Label4.BackColor = System.Drawing.Color.Transparent
        Me.Label4.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label4.Name = "Label4"
        '
        'LabelLicense
        '
        resources.ApplyResources(Me.LabelLicense, "LabelLicense")
        Me.LabelLicense.BackColor = System.Drawing.Color.Transparent
        Me.LabelLicense.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.LabelLicense.ForeColor = System.Drawing.SystemColors.ControlText
        Me.LabelLicense.Name = "LabelLicense"
        '
        'LblOSInfo
        '
        resources.ApplyResources(Me.LblOSInfo, "LblOSInfo")
        Me.LblOSInfo.BackColor = System.Drawing.Color.Transparent
        Me.LblOSInfo.ForeColor = System.Drawing.SystemColors.ControlText
        Me.LblOSInfo.Name = "LblOSInfo"
        '
        'TextBox1
        '
        resources.ApplyResources(Me.TextBox1, "TextBox1")
        Me.TextBox1.BackColor = System.Drawing.Color.White
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.ReadOnly = True
        '
        'Label7
        '
        resources.ApplyResources(Me.Label7, "Label7")
        Me.Label7.BackColor = System.Drawing.Color.Transparent
        Me.Label7.ForeColor = System.Drawing.SystemColors.ControlText
        Me.Label7.Name = "Label7"
        '
        'FaTabStripItem2
        '
        resources.ApplyResources(Me.FaTabStripItem2, "FaTabStripItem2")
        Me.FaTabStripItem2.CanClose = False
        Me.FaTabStripItem2.Controls.Add(Me.DataGridView1)
        Me.FaTabStripItem2.IsDrawn = True
        Me.FaTabStripItem2.Name = "FaTabStripItem2"
        '
        'DataGridView1
        '
        resources.ApplyResources(Me.DataGridView1, "DataGridView1")
        Me.DataGridView1.AllowUserToAddRows = False
        Me.DataGridView1.AllowUserToDeleteRows = False
        Me.DataGridView1.AllowUserToResizeRows = False
        Me.DataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells
        Me.DataGridView1.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells
        Me.DataGridView1.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None
        Me.DataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridView1.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Column1, Me.Column2, Me.Column3, Me.Column4, Me.Column5, Me.Column6, Me.Column7})
        Me.DataGridView1.Name = "DataGridView1"
        Me.DataGridView1.ReadOnly = True
        Me.DataGridView1.RowHeadersVisible = False
        Me.DataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        '
        'FaTabStripItem3
        '
        resources.ApplyResources(Me.FaTabStripItem3, "FaTabStripItem3")
        Me.FaTabStripItem3.CanClose = False
        Me.FaTabStripItem3.Controls.Add(Me.AssemblyInfoListView)
        Me.FaTabStripItem3.IsDrawn = True
        Me.FaTabStripItem3.Name = "FaTabStripItem3"
        '
        'AssemblyInfoListView
        '
        resources.ApplyResources(Me.AssemblyInfoListView, "AssemblyInfoListView")
        Me.AssemblyInfoListView.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.colAssemblyName, Me.colAssemblyVersion, Me.colAssemblyBuilt, Me.colAssemblyCodeBase})
        Me.AssemblyInfoListView.FullRowSelect = True
        Me.AssemblyInfoListView.MultiSelect = False
        Me.AssemblyInfoListView.Name = "AssemblyInfoListView"
        Me.AssemblyInfoListView.Sorting = System.Windows.Forms.SortOrder.Ascending
        Me.AssemblyInfoListView.UseCompatibleStateImageBehavior = False
        Me.AssemblyInfoListView.View = System.Windows.Forms.View.Details
        '
        'colAssemblyName
        '
        resources.ApplyResources(Me.colAssemblyName, "colAssemblyName")
        '
        'colAssemblyVersion
        '
        resources.ApplyResources(Me.colAssemblyVersion, "colAssemblyVersion")
        '
        'colAssemblyBuilt
        '
        resources.ApplyResources(Me.colAssemblyBuilt, "colAssemblyBuilt")
        '
        'colAssemblyCodeBase
        '
        resources.ApplyResources(Me.colAssemblyCodeBase, "colAssemblyCodeBase")
        '
        'FaTabStripItem4
        '
        resources.ApplyResources(Me.FaTabStripItem4, "FaTabStripItem4")
        Me.FaTabStripItem4.CanClose = False
        Me.FaTabStripItem4.Controls.Add(Me.AssemblyDetailsListView)
        Me.FaTabStripItem4.Controls.Add(Me.AssemblyNamesComboBox)
        Me.FaTabStripItem4.IsDrawn = True
        Me.FaTabStripItem4.Name = "FaTabStripItem4"
        '
        'AssemblyDetailsListView
        '
        resources.ApplyResources(Me.AssemblyDetailsListView, "AssemblyDetailsListView")
        Me.AssemblyDetailsListView.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader1, Me.ColumnHeader2})
        Me.AssemblyDetailsListView.FullRowSelect = True
        Me.AssemblyDetailsListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable
        Me.AssemblyDetailsListView.Name = "AssemblyDetailsListView"
        Me.AssemblyDetailsListView.Sorting = System.Windows.Forms.SortOrder.Ascending
        Me.AssemblyDetailsListView.UseCompatibleStateImageBehavior = False
        Me.AssemblyDetailsListView.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader1
        '
        resources.ApplyResources(Me.ColumnHeader1, "ColumnHeader1")
        '
        'ColumnHeader2
        '
        resources.ApplyResources(Me.ColumnHeader2, "ColumnHeader2")
        '
        'AssemblyNamesComboBox
        '
        resources.ApplyResources(Me.AssemblyNamesComboBox, "AssemblyNamesComboBox")
        Me.AssemblyNamesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.AssemblyNamesComboBox.Name = "AssemblyNamesComboBox"
        Me.AssemblyNamesComboBox.Sorted = True
        '
        'Column1
        '
        resources.ApplyResources(Me.Column1, "Column1")
        Me.Column1.Name = "Column1"
        Me.Column1.ReadOnly = True
        '
        'Column2
        '
        resources.ApplyResources(Me.Column2, "Column2")
        Me.Column2.Name = "Column2"
        Me.Column2.ReadOnly = True
        '
        'Column3
        '
        resources.ApplyResources(Me.Column3, "Column3")
        Me.Column3.Name = "Column3"
        Me.Column3.ReadOnly = True
        '
        'Column4
        '
        resources.ApplyResources(Me.Column4, "Column4")
        Me.Column4.Name = "Column4"
        Me.Column4.ReadOnly = True
        '
        'Column5
        '
        resources.ApplyResources(Me.Column5, "Column5")
        Me.Column5.Name = "Column5"
        Me.Column5.ReadOnly = True
        Me.Column5.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.Column5.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        '
        'Column6
        '
        resources.ApplyResources(Me.Column6, "Column6")
        Me.Column6.Name = "Column6"
        Me.Column6.ReadOnly = True
        '
        'Column7
        '
        resources.ApplyResources(Me.Column7, "Column7")
        Me.Column7.Name = "Column7"
        Me.Column7.ReadOnly = True
        Me.Column7.Resizable = System.Windows.Forms.DataGridViewTriState.[True]
        Me.Column7.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        '
        'AboutBoxMONO
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ControlBox = False
        Me.Controls.Add(Me.FaTabStrip1)
        Me.Controls.Add(Me.Copyright)
        Me.Controls.Add(Me.Version)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.Label1)
        Me.DoubleBuffered = True
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Name = "AboutBoxMONO"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        CType(Me.FaTabStrip1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.FaTabStrip1.ResumeLayout(False)
        Me.FaTabStripItem1.ResumeLayout(False)
        Me.FaTabStripItem1.PerformLayout()
        Me.FaTabStripItem2.ResumeLayout(False)
        CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.FaTabStripItem3.ResumeLayout(False)
        Me.FaTabStripItem4.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Public WithEvents Label1 As System.Windows.Forms.Label
    Public WithEvents Button1 As System.Windows.Forms.Button
    Public WithEvents Version As System.Windows.Forms.Label
    Public WithEvents Copyright As System.Windows.Forms.Label
    Friend WithEvents FaTabStrip1 As FarsiLibrary.Win.FATabStrip
    Friend WithEvents FaTabStripItem1 As FarsiLibrary.Win.FATabStripItem
    Public WithEvents Label9 As System.Windows.Forms.Label
    Public WithEvents LinkLabel1 As System.Windows.Forms.LinkLabel
    Public WithEvents Label10 As System.Windows.Forms.Label
    Public WithEvents Label2 As System.Windows.Forms.Label
    Public WithEvents Label8 As System.Windows.Forms.Label
    Public WithEvents LinkLabel2 As System.Windows.Forms.LinkLabel
    Public WithEvents Label3 As System.Windows.Forms.Label
    Public WithEvents Label5 As System.Windows.Forms.Label
    Public WithEvents LblCLRInfo As System.Windows.Forms.Label
    Public WithEvents Label6 As System.Windows.Forms.Label
    Public WithEvents Label4 As System.Windows.Forms.Label
    Public WithEvents LabelLicense As System.Windows.Forms.Label
    Public WithEvents LblOSInfo As System.Windows.Forms.Label
    Public WithEvents TextBox1 As System.Windows.Forms.TextBox
    Public WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents FaTabStripItem2 As FarsiLibrary.Win.FATabStripItem
    Friend WithEvents DataGridView1 As System.Windows.Forms.DataGridView
    Friend WithEvents FaTabStripItem3 As FarsiLibrary.Win.FATabStripItem
    Friend WithEvents AssemblyInfoListView As System.Windows.Forms.ListView
    Friend WithEvents colAssemblyName As System.Windows.Forms.ColumnHeader
    Friend WithEvents colAssemblyVersion As System.Windows.Forms.ColumnHeader
    Friend WithEvents colAssemblyBuilt As System.Windows.Forms.ColumnHeader
    Friend WithEvents colAssemblyCodeBase As System.Windows.Forms.ColumnHeader
    Friend WithEvents FaTabStripItem4 As FarsiLibrary.Win.FATabStripItem
    Friend WithEvents AssemblyDetailsListView As System.Windows.Forms.ListView
    Friend WithEvents ColumnHeader1 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader2 As System.Windows.Forms.ColumnHeader
    Friend WithEvents AssemblyNamesComboBox As System.Windows.Forms.ComboBox
    Public WithEvents Lblcpusimd As System.Windows.Forms.Label
    Public WithEvents Lblcpuinfo As System.Windows.Forms.Label
    Public WithEvents Label13 As System.Windows.Forms.Label
    Public WithEvents Label12 As System.Windows.Forms.Label
    Friend WithEvents Column1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Column2 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Column3 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Column4 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Column5 As System.Windows.Forms.DataGridViewLinkColumn
    Friend WithEvents Column6 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents Column7 As System.Windows.Forms.DataGridViewLinkColumn
End Class
