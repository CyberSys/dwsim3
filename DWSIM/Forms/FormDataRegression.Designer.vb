﻿<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormDataRegression
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FormDataRegression))
        Dim DataGridViewCellStyle1 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle2 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle3 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle5 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Dim DataGridViewCellStyle4 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.tbRegResults = New System.Windows.Forms.TextBox()
        Me.btnDoReg = New System.Windows.Forms.Button()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.chkTS = New System.Windows.Forms.CheckBox()
        Me.chkTL = New System.Windows.Forms.CheckBox()
        Me.chkIdealVaporPhase = New System.Windows.Forms.CheckBox()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.gridInEst = New System.Windows.Forms.DataGridView()
        Me.colpar = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colval = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.cf = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.btnCancel = New System.Windows.Forms.Button()
        Me.cbPunit = New System.Windows.Forms.ComboBox()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.cbTunit = New System.Windows.Forms.ComboBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.chkIncludeSD = New System.Windows.Forms.CheckBox()
        Me.btnCalcOnce = New System.Windows.Forms.Button()
        Me.cbObjFunc = New System.Windows.Forms.ComboBox()
        Me.LabelWithDivider12 = New System.Windows.Forms.LabelWithDivider()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.cbRegMethod = New System.Windows.Forms.ComboBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.cbCompound1 = New System.Windows.Forms.ComboBox()
        Me.LabelWithDivider3 = New System.Windows.Forms.LabelWithDivider()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.GridExpData = New System.Windows.Forms.DataGridView()
        Me.ContextMenuStrip1 = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.ToolStripMenuItem1 = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem2 = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem3 = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem4 = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem5 = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem6 = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripMenuItem7 = New System.Windows.Forms.ToolStripMenuItem()
        Me.cbCompound2 = New System.Windows.Forms.ComboBox()
        Me.cbDataType = New System.Windows.Forms.ComboBox()
        Me.cbModel = New System.Windows.Forms.ComboBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.LabelWithDivider1 = New System.Windows.Forms.LabelWithDivider()
        Me.LabelWithDivider2 = New System.Windows.Forms.LabelWithDivider()
        Me.graph = New ZedGraph.ZedGraphControl()
        Me.GroupBox4 = New System.Windows.Forms.GroupBox()
        Me.tbDescription = New System.Windows.Forms.TextBox()
        Me.Label11 = New System.Windows.Forms.Label()
        Me.tbTitle = New System.Windows.Forms.TextBox()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.FaTabStrip1 = New FarsiLibrary.Win.FATabStrip()
        Me.FaTabStripItem1 = New FarsiLibrary.Win.FATabStripItem()
        Me.FaTabStripItem2 = New FarsiLibrary.Win.FATabStripItem()
        Me.graph2 = New ZedGraph.ZedGraphControl()
        Me.FaTabStripItem3 = New FarsiLibrary.Win.FATabStripItem()
        Me.gridstats = New System.Windows.Forms.DataGridView()
        Me.x1l1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.x1l1calc = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.x1l2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.x1l2calc = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.y1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.y1c = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.t = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.tcalc = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.p = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.pcalc = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.dy = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.dyy = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.dyp = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.dp = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.dpp = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.dppp = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.dt = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.dtt = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.dttp = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.dx1l1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.dxx1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.dxxp = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.dx2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.dxx2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.dxx2p = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.coltl_error = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colts_error = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.check = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.colx1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colx2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.coly1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colT = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.coltl = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colts = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.colP = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.GroupBox1.SuspendLayout()
        Me.GroupBox2.SuspendLayout()
        Me.Panel1.SuspendLayout()
        CType(Me.gridInEst, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.GridExpData, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ContextMenuStrip1.SuspendLayout()
        Me.GroupBox4.SuspendLayout()
        CType(Me.FaTabStrip1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.FaTabStrip1.SuspendLayout()
        Me.FaTabStripItem1.SuspendLayout()
        Me.FaTabStripItem2.SuspendLayout()
        Me.FaTabStripItem3.SuspendLayout()
        CType(Me.gridstats, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'GroupBox1
        '
        resources.ApplyResources(Me.GroupBox1, "GroupBox1")
        Me.GroupBox1.Controls.Add(Me.tbRegResults)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.TabStop = False
        '
        'tbRegResults
        '
        resources.ApplyResources(Me.tbRegResults, "tbRegResults")
        Me.tbRegResults.Name = "tbRegResults"
        Me.tbRegResults.ReadOnly = True
        '
        'btnDoReg
        '
        resources.ApplyResources(Me.btnDoReg, "btnDoReg")
        Me.btnDoReg.Name = "btnDoReg"
        Me.btnDoReg.UseVisualStyleBackColor = True
        '
        'GroupBox2
        '
        resources.ApplyResources(Me.GroupBox2, "GroupBox2")
        Me.GroupBox2.Controls.Add(Me.Panel1)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.TabStop = False
        '
        'Panel1
        '
        resources.ApplyResources(Me.Panel1, "Panel1")
        Me.Panel1.Controls.Add(Me.chkTS)
        Me.Panel1.Controls.Add(Me.chkTL)
        Me.Panel1.Controls.Add(Me.chkIdealVaporPhase)
        Me.Panel1.Controls.Add(Me.Button2)
        Me.Panel1.Controls.Add(Me.Button1)
        Me.Panel1.Controls.Add(Me.Label9)
        Me.Panel1.Controls.Add(Me.gridInEst)
        Me.Panel1.Controls.Add(Me.btnCancel)
        Me.Panel1.Controls.Add(Me.cbPunit)
        Me.Panel1.Controls.Add(Me.Label8)
        Me.Panel1.Controls.Add(Me.cbTunit)
        Me.Panel1.Controls.Add(Me.Label1)
        Me.Panel1.Controls.Add(Me.chkIncludeSD)
        Me.Panel1.Controls.Add(Me.btnCalcOnce)
        Me.Panel1.Controls.Add(Me.cbObjFunc)
        Me.Panel1.Controls.Add(Me.LabelWithDivider12)
        Me.Panel1.Controls.Add(Me.Label7)
        Me.Panel1.Controls.Add(Me.Label2)
        Me.Panel1.Controls.Add(Me.cbRegMethod)
        Me.Panel1.Controls.Add(Me.btnDoReg)
        Me.Panel1.Controls.Add(Me.Label3)
        Me.Panel1.Controls.Add(Me.Label6)
        Me.Panel1.Controls.Add(Me.cbCompound1)
        Me.Panel1.Controls.Add(Me.LabelWithDivider3)
        Me.Panel1.Controls.Add(Me.Label4)
        Me.Panel1.Controls.Add(Me.GridExpData)
        Me.Panel1.Controls.Add(Me.cbCompound2)
        Me.Panel1.Controls.Add(Me.cbDataType)
        Me.Panel1.Controls.Add(Me.cbModel)
        Me.Panel1.Controls.Add(Me.Label5)
        Me.Panel1.Controls.Add(Me.LabelWithDivider1)
        Me.Panel1.Controls.Add(Me.LabelWithDivider2)
        Me.Panel1.Name = "Panel1"
        '
        'chkTS
        '
        resources.ApplyResources(Me.chkTS, "chkTS")
        Me.chkTS.Name = "chkTS"
        Me.chkTS.UseVisualStyleBackColor = True
        '
        'chkTL
        '
        resources.ApplyResources(Me.chkTL, "chkTL")
        Me.chkTL.Name = "chkTL"
        Me.chkTL.UseVisualStyleBackColor = True
        '
        'chkIdealVaporPhase
        '
        resources.ApplyResources(Me.chkIdealVaporPhase, "chkIdealVaporPhase")
        Me.chkIdealVaporPhase.Name = "chkIdealVaporPhase"
        Me.chkIdealVaporPhase.UseVisualStyleBackColor = True
        '
        'Button2
        '
        resources.ApplyResources(Me.Button2, "Button2")
        Me.Button2.Name = "Button2"
        Me.Button2.UseVisualStyleBackColor = True
        '
        'Button1
        '
        resources.ApplyResources(Me.Button1, "Button1")
        Me.Button1.Name = "Button1"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'Label9
        '
        resources.ApplyResources(Me.Label9, "Label9")
        Me.Label9.Name = "Label9"
        '
        'gridInEst
        '
        Me.gridInEst.AllowUserToAddRows = False
        Me.gridInEst.AllowUserToDeleteRows = False
        Me.gridInEst.AllowUserToResizeRows = False
        DataGridViewCellStyle1.BackColor = System.Drawing.Color.WhiteSmoke
        Me.gridInEst.AlternatingRowsDefaultCellStyle = DataGridViewCellStyle1
        Me.gridInEst.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.gridInEst.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.gridInEst.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.colpar, Me.colval, Me.cf})
        resources.ApplyResources(Me.gridInEst, "gridInEst")
        Me.gridInEst.Name = "gridInEst"
        Me.gridInEst.RowHeadersVisible = False
        '
        'colpar
        '
        Me.colpar.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        resources.ApplyResources(Me.colpar, "colpar")
        Me.colpar.Name = "colpar"
        Me.colpar.ReadOnly = True
        Me.colpar.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'colval
        '
        Me.colval.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        DataGridViewCellStyle2.Format = "N4"
        Me.colval.DefaultCellStyle = DataGridViewCellStyle2
        resources.ApplyResources(Me.colval, "colval")
        Me.colval.Name = "colval"
        Me.colval.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
        '
        'cf
        '
        Me.cf.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill
        Me.cf.FillWeight = 20.0!
        resources.ApplyResources(Me.cf, "cf")
        Me.cf.Name = "cf"
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.UseVisualStyleBackColor = True
        '
        'cbPunit
        '
        Me.cbPunit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbPunit.FormattingEnabled = True
        Me.cbPunit.Items.AddRange(New Object() {resources.GetString("cbPunit.Items"), resources.GetString("cbPunit.Items1"), resources.GetString("cbPunit.Items2"), resources.GetString("cbPunit.Items3"), resources.GetString("cbPunit.Items4"), resources.GetString("cbPunit.Items5"), resources.GetString("cbPunit.Items6"), resources.GetString("cbPunit.Items7"), resources.GetString("cbPunit.Items8"), resources.GetString("cbPunit.Items9"), resources.GetString("cbPunit.Items10"), resources.GetString("cbPunit.Items11"), resources.GetString("cbPunit.Items12"), resources.GetString("cbPunit.Items13"), resources.GetString("cbPunit.Items14"), resources.GetString("cbPunit.Items15"), resources.GetString("cbPunit.Items16"), resources.GetString("cbPunit.Items17"), resources.GetString("cbPunit.Items18")})
        resources.ApplyResources(Me.cbPunit, "cbPunit")
        Me.cbPunit.Name = "cbPunit"
        '
        'Label8
        '
        resources.ApplyResources(Me.Label8, "Label8")
        Me.Label8.Name = "Label8"
        '
        'cbTunit
        '
        Me.cbTunit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbTunit.FormattingEnabled = True
        Me.cbTunit.Items.AddRange(New Object() {resources.GetString("cbTunit.Items"), resources.GetString("cbTunit.Items1"), resources.GetString("cbTunit.Items2"), resources.GetString("cbTunit.Items3")})
        resources.ApplyResources(Me.cbTunit, "cbTunit")
        Me.cbTunit.Name = "cbTunit"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'chkIncludeSD
        '
        resources.ApplyResources(Me.chkIncludeSD, "chkIncludeSD")
        Me.chkIncludeSD.Name = "chkIncludeSD"
        Me.chkIncludeSD.UseVisualStyleBackColor = True
        '
        'btnCalcOnce
        '
        resources.ApplyResources(Me.btnCalcOnce, "btnCalcOnce")
        Me.btnCalcOnce.Name = "btnCalcOnce"
        Me.btnCalcOnce.UseVisualStyleBackColor = True
        '
        'cbObjFunc
        '
        resources.ApplyResources(Me.cbObjFunc, "cbObjFunc")
        Me.cbObjFunc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbObjFunc.FormattingEnabled = True
        Me.cbObjFunc.Items.AddRange(New Object() {resources.GetString("cbObjFunc.Items"), resources.GetString("cbObjFunc.Items1"), resources.GetString("cbObjFunc.Items2"), resources.GetString("cbObjFunc.Items3"), resources.GetString("cbObjFunc.Items4"), resources.GetString("cbObjFunc.Items5")})
        Me.cbObjFunc.Name = "cbObjFunc"
        '
        'LabelWithDivider12
        '
        resources.ApplyResources(Me.LabelWithDivider12, "LabelWithDivider12")
        Me.LabelWithDivider12.Gap = 5
        Me.LabelWithDivider12.Name = "LabelWithDivider12"
        '
        'Label7
        '
        resources.ApplyResources(Me.Label7, "Label7")
        Me.Label7.Name = "Label7"
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'cbRegMethod
        '
        resources.ApplyResources(Me.cbRegMethod, "cbRegMethod")
        Me.cbRegMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbRegMethod.FormattingEnabled = True
        Me.cbRegMethod.Items.AddRange(New Object() {resources.GetString("cbRegMethod.Items"), resources.GetString("cbRegMethod.Items1"), resources.GetString("cbRegMethod.Items2"), resources.GetString("cbRegMethod.Items3")})
        Me.cbRegMethod.Name = "cbRegMethod"
        '
        'Label3
        '
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.Name = "Label3"
        '
        'Label6
        '
        resources.ApplyResources(Me.Label6, "Label6")
        Me.Label6.Name = "Label6"
        '
        'cbCompound1
        '
        Me.cbCompound1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbCompound1.FormattingEnabled = True
        resources.ApplyResources(Me.cbCompound1, "cbCompound1")
        Me.cbCompound1.Name = "cbCompound1"
        '
        'LabelWithDivider3
        '
        resources.ApplyResources(Me.LabelWithDivider3, "LabelWithDivider3")
        Me.LabelWithDivider3.Gap = 5
        Me.LabelWithDivider3.Name = "LabelWithDivider3"
        '
        'Label4
        '
        resources.ApplyResources(Me.Label4, "Label4")
        Me.Label4.Name = "Label4"
        '
        'GridExpData
        '
        DataGridViewCellStyle3.BackColor = System.Drawing.Color.WhiteSmoke
        Me.GridExpData.AlternatingRowsDefaultCellStyle = DataGridViewCellStyle3
        resources.ApplyResources(Me.GridExpData, "GridExpData")
        Me.GridExpData.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.GridExpData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.GridExpData.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.check, Me.colx1, Me.colx2, Me.coly1, Me.colT, Me.coltl, Me.colts, Me.colP})
        Me.GridExpData.ContextMenuStrip = Me.ContextMenuStrip1
        Me.GridExpData.Name = "GridExpData"
        Me.GridExpData.RowHeadersVisible = False
        '
        'ContextMenuStrip1
        '
        Me.ContextMenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripMenuItem1, Me.ToolStripMenuItem2, Me.ToolStripMenuItem3, Me.ToolStripMenuItem4, Me.ToolStripMenuItem5, Me.ToolStripMenuItem6, Me.ToolStripMenuItem7})
        Me.ContextMenuStrip1.Name = "ContextMenuStrip1"
        resources.ApplyResources(Me.ContextMenuStrip1, "ContextMenuStrip1")
        '
        'ToolStripMenuItem1
        '
        Me.ToolStripMenuItem1.Image = Global.DWSIM.My.Resources.Resources.lightning
        Me.ToolStripMenuItem1.Name = "ToolStripMenuItem1"
        resources.ApplyResources(Me.ToolStripMenuItem1, "ToolStripMenuItem1")
        '
        'ToolStripMenuItem2
        '
        Me.ToolStripMenuItem2.Image = Global.DWSIM.My.Resources.Resources.lightning
        Me.ToolStripMenuItem2.Name = "ToolStripMenuItem2"
        resources.ApplyResources(Me.ToolStripMenuItem2, "ToolStripMenuItem2")
        '
        'ToolStripMenuItem3
        '
        Me.ToolStripMenuItem3.Image = Global.DWSIM.My.Resources.Resources.lightning
        Me.ToolStripMenuItem3.Name = "ToolStripMenuItem3"
        resources.ApplyResources(Me.ToolStripMenuItem3, "ToolStripMenuItem3")
        '
        'ToolStripMenuItem4
        '
        Me.ToolStripMenuItem4.Image = Global.DWSIM.My.Resources.Resources.lightning
        Me.ToolStripMenuItem4.Name = "ToolStripMenuItem4"
        resources.ApplyResources(Me.ToolStripMenuItem4, "ToolStripMenuItem4")
        '
        'ToolStripMenuItem5
        '
        Me.ToolStripMenuItem5.Image = Global.DWSIM.My.Resources.Resources.lightning
        Me.ToolStripMenuItem5.Name = "ToolStripMenuItem5"
        resources.ApplyResources(Me.ToolStripMenuItem5, "ToolStripMenuItem5")
        '
        'ToolStripMenuItem6
        '
        Me.ToolStripMenuItem6.Image = Global.DWSIM.My.Resources.Resources.lightning
        Me.ToolStripMenuItem6.Name = "ToolStripMenuItem6"
        resources.ApplyResources(Me.ToolStripMenuItem6, "ToolStripMenuItem6")
        '
        'ToolStripMenuItem7
        '
        Me.ToolStripMenuItem7.Image = Global.DWSIM.My.Resources.Resources.lightning
        Me.ToolStripMenuItem7.Name = "ToolStripMenuItem7"
        resources.ApplyResources(Me.ToolStripMenuItem7, "ToolStripMenuItem7")
        '
        'cbCompound2
        '
        Me.cbCompound2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbCompound2.FormattingEnabled = True
        resources.ApplyResources(Me.cbCompound2, "cbCompound2")
        Me.cbCompound2.Name = "cbCompound2"
        '
        'cbDataType
        '
        Me.cbDataType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbDataType.FormattingEnabled = True
        Me.cbDataType.Items.AddRange(New Object() {resources.GetString("cbDataType.Items"), resources.GetString("cbDataType.Items1"), resources.GetString("cbDataType.Items2"), resources.GetString("cbDataType.Items3"), resources.GetString("cbDataType.Items4"), resources.GetString("cbDataType.Items5"), resources.GetString("cbDataType.Items6"), resources.GetString("cbDataType.Items7")})
        resources.ApplyResources(Me.cbDataType, "cbDataType")
        Me.cbDataType.Name = "cbDataType"
        '
        'cbModel
        '
        Me.cbModel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbModel.FormattingEnabled = True
        Me.cbModel.Items.AddRange(New Object() {resources.GetString("cbModel.Items"), resources.GetString("cbModel.Items1"), resources.GetString("cbModel.Items2"), resources.GetString("cbModel.Items3"), resources.GetString("cbModel.Items4"), resources.GetString("cbModel.Items5"), resources.GetString("cbModel.Items6"), resources.GetString("cbModel.Items7")})
        resources.ApplyResources(Me.cbModel, "cbModel")
        Me.cbModel.Name = "cbModel"
        '
        'Label5
        '
        resources.ApplyResources(Me.Label5, "Label5")
        Me.Label5.Name = "Label5"
        '
        'LabelWithDivider1
        '
        resources.ApplyResources(Me.LabelWithDivider1, "LabelWithDivider1")
        Me.LabelWithDivider1.Gap = 5
        Me.LabelWithDivider1.Name = "LabelWithDivider1"
        '
        'LabelWithDivider2
        '
        resources.ApplyResources(Me.LabelWithDivider2, "LabelWithDivider2")
        Me.LabelWithDivider2.Gap = 5
        Me.LabelWithDivider2.Name = "LabelWithDivider2"
        '
        'graph
        '
        resources.ApplyResources(Me.graph, "graph")
        Me.graph.Name = "graph"
        Me.graph.ScrollGrace = 0.0R
        Me.graph.ScrollMaxX = 0.0R
        Me.graph.ScrollMaxY = 0.0R
        Me.graph.ScrollMaxY2 = 0.0R
        Me.graph.ScrollMinX = 0.0R
        Me.graph.ScrollMinY = 0.0R
        Me.graph.ScrollMinY2 = 0.0R
        '
        'GroupBox4
        '
        resources.ApplyResources(Me.GroupBox4, "GroupBox4")
        Me.GroupBox4.Controls.Add(Me.tbDescription)
        Me.GroupBox4.Controls.Add(Me.Label11)
        Me.GroupBox4.Controls.Add(Me.tbTitle)
        Me.GroupBox4.Controls.Add(Me.Label10)
        Me.GroupBox4.Name = "GroupBox4"
        Me.GroupBox4.TabStop = False
        '
        'tbDescription
        '
        resources.ApplyResources(Me.tbDescription, "tbDescription")
        Me.tbDescription.Name = "tbDescription"
        '
        'Label11
        '
        resources.ApplyResources(Me.Label11, "Label11")
        Me.Label11.Name = "Label11"
        '
        'tbTitle
        '
        resources.ApplyResources(Me.tbTitle, "tbTitle")
        Me.tbTitle.Name = "tbTitle"
        '
        'Label10
        '
        resources.ApplyResources(Me.Label10, "Label10")
        Me.Label10.Name = "Label10"
        '
        'FaTabStrip1
        '
        Me.FaTabStrip1.AlwaysShowClose = False
        Me.FaTabStrip1.AlwaysShowMenuGlyph = False
        resources.ApplyResources(Me.FaTabStrip1, "FaTabStrip1")
        Me.FaTabStrip1.Items.AddRange(New FarsiLibrary.Win.FATabStripItem() {Me.FaTabStripItem1, Me.FaTabStripItem2, Me.FaTabStripItem3})
        Me.FaTabStrip1.Name = "FaTabStrip1"
        Me.FaTabStrip1.SelectedItem = Me.FaTabStripItem1
        '
        'FaTabStripItem1
        '
        Me.FaTabStripItem1.CanClose = False
        Me.FaTabStripItem1.Controls.Add(Me.graph)
        Me.FaTabStripItem1.IsDrawn = True
        Me.FaTabStripItem1.Name = "FaTabStripItem1"
        Me.FaTabStripItem1.Selected = True
        resources.ApplyResources(Me.FaTabStripItem1, "FaTabStripItem1")
        '
        'FaTabStripItem2
        '
        Me.FaTabStripItem2.CanClose = False
        Me.FaTabStripItem2.Controls.Add(Me.graph2)
        Me.FaTabStripItem2.IsDrawn = True
        Me.FaTabStripItem2.Name = "FaTabStripItem2"
        resources.ApplyResources(Me.FaTabStripItem2, "FaTabStripItem2")
        '
        'graph2
        '
        resources.ApplyResources(Me.graph2, "graph2")
        Me.graph2.Name = "graph2"
        Me.graph2.ScrollGrace = 0.0R
        Me.graph2.ScrollMaxX = 0.0R
        Me.graph2.ScrollMaxY = 0.0R
        Me.graph2.ScrollMaxY2 = 0.0R
        Me.graph2.ScrollMinX = 0.0R
        Me.graph2.ScrollMinY = 0.0R
        Me.graph2.ScrollMinY2 = 0.0R
        '
        'FaTabStripItem3
        '
        Me.FaTabStripItem3.CanClose = False
        Me.FaTabStripItem3.Controls.Add(Me.gridstats)
        Me.FaTabStripItem3.IsDrawn = True
        Me.FaTabStripItem3.Name = "FaTabStripItem3"
        resources.ApplyResources(Me.FaTabStripItem3, "FaTabStripItem3")
        '
        'gridstats
        '
        DataGridViewCellStyle5.BackColor = System.Drawing.Color.WhiteSmoke
        Me.gridstats.AlternatingRowsDefaultCellStyle = DataGridViewCellStyle5
        Me.gridstats.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.gridstats.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText
        Me.gridstats.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.gridstats.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.x1l1, Me.x1l1calc, Me.x1l2, Me.x1l2calc, Me.y1, Me.y1c, Me.t, Me.tcalc, Me.p, Me.pcalc, Me.dy, Me.dyy, Me.dyp, Me.dp, Me.dpp, Me.dppp, Me.dt, Me.dtt, Me.dttp, Me.dx1l1, Me.dxx1, Me.dxxp, Me.dx2, Me.dxx2, Me.dxx2p, Me.coltl_error, Me.colts_error})
        Me.gridstats.ContextMenuStrip = Me.ContextMenuStrip1
        resources.ApplyResources(Me.gridstats, "gridstats")
        Me.gridstats.Name = "gridstats"
        Me.gridstats.ReadOnly = True
        Me.gridstats.RowHeadersVisible = False
        '
        'x1l1
        '
        resources.ApplyResources(Me.x1l1, "x1l1")
        Me.x1l1.Name = "x1l1"
        Me.x1l1.ReadOnly = True
        '
        'x1l1calc
        '
        resources.ApplyResources(Me.x1l1calc, "x1l1calc")
        Me.x1l1calc.Name = "x1l1calc"
        Me.x1l1calc.ReadOnly = True
        '
        'x1l2
        '
        resources.ApplyResources(Me.x1l2, "x1l2")
        Me.x1l2.Name = "x1l2"
        Me.x1l2.ReadOnly = True
        '
        'x1l2calc
        '
        resources.ApplyResources(Me.x1l2calc, "x1l2calc")
        Me.x1l2calc.Name = "x1l2calc"
        Me.x1l2calc.ReadOnly = True
        '
        'y1
        '
        resources.ApplyResources(Me.y1, "y1")
        Me.y1.Name = "y1"
        Me.y1.ReadOnly = True
        '
        'y1c
        '
        resources.ApplyResources(Me.y1c, "y1c")
        Me.y1c.Name = "y1c"
        Me.y1c.ReadOnly = True
        '
        't
        '
        resources.ApplyResources(Me.t, "t")
        Me.t.Name = "t"
        Me.t.ReadOnly = True
        '
        'tcalc
        '
        resources.ApplyResources(Me.tcalc, "tcalc")
        Me.tcalc.Name = "tcalc"
        Me.tcalc.ReadOnly = True
        '
        'p
        '
        resources.ApplyResources(Me.p, "p")
        Me.p.Name = "p"
        Me.p.ReadOnly = True
        '
        'pcalc
        '
        resources.ApplyResources(Me.pcalc, "pcalc")
        Me.pcalc.Name = "pcalc"
        Me.pcalc.ReadOnly = True
        '
        'dy
        '
        resources.ApplyResources(Me.dy, "dy")
        Me.dy.Name = "dy"
        Me.dy.ReadOnly = True
        '
        'dyy
        '
        resources.ApplyResources(Me.dyy, "dyy")
        Me.dyy.Name = "dyy"
        Me.dyy.ReadOnly = True
        '
        'dyp
        '
        resources.ApplyResources(Me.dyp, "dyp")
        Me.dyp.Name = "dyp"
        Me.dyp.ReadOnly = True
        '
        'dp
        '
        resources.ApplyResources(Me.dp, "dp")
        Me.dp.Name = "dp"
        Me.dp.ReadOnly = True
        '
        'dpp
        '
        resources.ApplyResources(Me.dpp, "dpp")
        Me.dpp.Name = "dpp"
        Me.dpp.ReadOnly = True
        '
        'dppp
        '
        resources.ApplyResources(Me.dppp, "dppp")
        Me.dppp.Name = "dppp"
        Me.dppp.ReadOnly = True
        '
        'dt
        '
        resources.ApplyResources(Me.dt, "dt")
        Me.dt.Name = "dt"
        Me.dt.ReadOnly = True
        '
        'dtt
        '
        resources.ApplyResources(Me.dtt, "dtt")
        Me.dtt.Name = "dtt"
        Me.dtt.ReadOnly = True
        '
        'dttp
        '
        resources.ApplyResources(Me.dttp, "dttp")
        Me.dttp.Name = "dttp"
        Me.dttp.ReadOnly = True
        '
        'dx1l1
        '
        resources.ApplyResources(Me.dx1l1, "dx1l1")
        Me.dx1l1.Name = "dx1l1"
        Me.dx1l1.ReadOnly = True
        '
        'dxx1
        '
        resources.ApplyResources(Me.dxx1, "dxx1")
        Me.dxx1.Name = "dxx1"
        Me.dxx1.ReadOnly = True
        '
        'dxxp
        '
        resources.ApplyResources(Me.dxxp, "dxxp")
        Me.dxxp.Name = "dxxp"
        Me.dxxp.ReadOnly = True
        '
        'dx2
        '
        resources.ApplyResources(Me.dx2, "dx2")
        Me.dx2.Name = "dx2"
        Me.dx2.ReadOnly = True
        '
        'dxx2
        '
        resources.ApplyResources(Me.dxx2, "dxx2")
        Me.dxx2.Name = "dxx2"
        Me.dxx2.ReadOnly = True
        '
        'dxx2p
        '
        resources.ApplyResources(Me.dxx2p, "dxx2p")
        Me.dxx2p.Name = "dxx2p"
        Me.dxx2p.ReadOnly = True
        '
        'coltl_error
        '
        resources.ApplyResources(Me.coltl_error, "coltl_error")
        Me.coltl_error.Name = "coltl_error"
        Me.coltl_error.ReadOnly = True
        '
        'colts_error
        '
        resources.ApplyResources(Me.colts_error, "colts_error")
        Me.colts_error.Name = "colts_error"
        Me.colts_error.ReadOnly = True
        '
        'check
        '
        DataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        Me.check.DefaultCellStyle = DataGridViewCellStyle4
        Me.check.FalseValue = ""
        Me.check.FillWeight = 60.9137!
        Me.check.HeaderText = ""
        Me.check.Name = "check"
        Me.check.Resizable = System.Windows.Forms.DataGridViewTriState.[False]
        Me.check.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic
        Me.check.TrueValue = ""
        '
        'colx1
        '
        Me.colx1.FillWeight = 107.8173!
        resources.ApplyResources(Me.colx1, "colx1")
        Me.colx1.Name = "colx1"
        '
        'colx2
        '
        Me.colx2.FillWeight = 107.8173!
        resources.ApplyResources(Me.colx2, "colx2")
        Me.colx2.Name = "colx2"
        '
        'coly1
        '
        Me.coly1.FillWeight = 107.8173!
        resources.ApplyResources(Me.coly1, "coly1")
        Me.coly1.Name = "coly1"
        '
        'colT
        '
        Me.colT.FillWeight = 107.8173!
        resources.ApplyResources(Me.colT, "colT")
        Me.colT.Name = "colT"
        '
        'coltl
        '
        resources.ApplyResources(Me.coltl, "coltl")
        Me.coltl.Name = "coltl"
        '
        'colts
        '
        resources.ApplyResources(Me.colts, "colts")
        Me.colts.Name = "colts"
        '
        'colP
        '
        Me.colP.FillWeight = 107.8173!
        resources.ApplyResources(Me.colP, "colP")
        Me.colP.Name = "colP"
        '
        'FormDataRegression
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.FaTabStrip1)
        Me.Controls.Add(Me.GroupBox4)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.GroupBox1)
        Me.Name = "FormDataRegression"
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.GroupBox2.ResumeLayout(False)
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        CType(Me.gridInEst, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.GridExpData, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ContextMenuStrip1.ResumeLayout(False)
        Me.GroupBox4.ResumeLayout(False)
        Me.GroupBox4.PerformLayout()
        CType(Me.FaTabStrip1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.FaTabStrip1.ResumeLayout(False)
        Me.FaTabStripItem1.ResumeLayout(False)
        Me.FaTabStripItem2.ResumeLayout(False)
        Me.FaTabStripItem3.ResumeLayout(False)
        CType(Me.gridstats, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents btnDoReg As System.Windows.Forms.Button
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents cbCompound2 As System.Windows.Forms.ComboBox
    Friend WithEvents cbCompound1 As System.Windows.Forms.ComboBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents cbModel As System.Windows.Forms.ComboBox
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents cbDataType As System.Windows.Forms.ComboBox
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Public WithEvents LabelWithDivider2 As System.Windows.Forms.LabelWithDivider
    Public WithEvents LabelWithDivider1 As System.Windows.Forms.LabelWithDivider
    Public WithEvents LabelWithDivider12 As System.Windows.Forms.LabelWithDivider
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents cbObjFunc As System.Windows.Forms.ComboBox
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents cbRegMethod As System.Windows.Forms.ComboBox
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Public WithEvents LabelWithDivider3 As System.Windows.Forms.LabelWithDivider
    Public WithEvents GridExpData As System.Windows.Forms.DataGridView
    Friend WithEvents graph As ZedGraph.ZedGraphControl
    Friend WithEvents tbRegResults As System.Windows.Forms.TextBox
    Friend WithEvents btnCalcOnce As System.Windows.Forms.Button
    Friend WithEvents chkIncludeSD As System.Windows.Forms.CheckBox
    Friend WithEvents cbPunit As System.Windows.Forms.ComboBox
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents cbTunit As System.Windows.Forms.ComboBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents btnCancel As System.Windows.Forms.Button
    Public WithEvents gridInEst As System.Windows.Forms.DataGridView
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents GroupBox4 As System.Windows.Forms.GroupBox
    Friend WithEvents tbTitle As System.Windows.Forms.TextBox
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents tbDescription As System.Windows.Forms.TextBox
    Friend WithEvents Label11 As System.Windows.Forms.Label
    Friend WithEvents Button2 As System.Windows.Forms.Button
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents ContextMenuStrip1 As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents ToolStripMenuItem1 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem2 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem3 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem4 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem5 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem6 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripMenuItem7 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents FaTabStrip1 As FarsiLibrary.Win.FATabStrip
    Friend WithEvents FaTabStripItem1 As FarsiLibrary.Win.FATabStripItem
    Friend WithEvents FaTabStripItem2 As FarsiLibrary.Win.FATabStripItem
    Friend WithEvents graph2 As ZedGraph.ZedGraphControl
    Friend WithEvents FaTabStripItem3 As FarsiLibrary.Win.FATabStripItem
    Public WithEvents gridstats As System.Windows.Forms.DataGridView
    Friend WithEvents chkIdealVaporPhase As System.Windows.Forms.CheckBox
    Friend WithEvents colpar As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colval As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents cf As System.Windows.Forms.DataGridViewCheckBoxColumn
    Friend WithEvents x1l1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents x1l1calc As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents x1l2 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents x1l2calc As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents y1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents y1c As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents t As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents tcalc As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents p As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents pcalc As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents dy As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents dyy As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents dyp As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents dp As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents dpp As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents dppp As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents dt As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents dtt As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents dttp As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents dx1l1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents dxx1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents dxxp As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents dx2 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents dxx2 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents dxx2p As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents coltl_error As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colts_error As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents chkTS As System.Windows.Forms.CheckBox
    Friend WithEvents chkTL As System.Windows.Forms.CheckBox
    Friend WithEvents check As System.Windows.Forms.DataGridViewCheckBoxColumn
    Friend WithEvents colx1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colx2 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents coly1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colT As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents coltl As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colts As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents colP As System.Windows.Forms.DataGridViewTextBoxColumn
End Class
