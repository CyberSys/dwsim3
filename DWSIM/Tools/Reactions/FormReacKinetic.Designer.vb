<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormReacKinetic
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FormReacKinetic))
        Me.KryptonButton4 = New System.Windows.Forms.Button()
        Me.KryptonButton3 = New System.Windows.Forms.Button()
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.KryptonLabel18 = New System.Windows.Forms.Label()
        Me.KryptonLabel17 = New System.Windows.Forms.Label()
        Me.tbRevE = New System.Windows.Forms.TextBox()
        Me.KryptonLabel15 = New System.Windows.Forms.Label()
        Me.tbRevA = New System.Windows.Forms.TextBox()
        Me.KryptonLabel16 = New System.Windows.Forms.Label()
        Me.tbFwdE = New System.Windows.Forms.TextBox()
        Me.KryptonLabel14 = New System.Windows.Forms.Label()
        Me.tbFwdA = New System.Windows.Forms.TextBox()
        Me.KryptonLabel13 = New System.Windows.Forms.Label()
        Me.KryptonLabel11 = New System.Windows.Forms.Label()
        Me.cbConcUnit = New System.Windows.Forms.ComboBox()
        Me.KryptonLabel12 = New System.Windows.Forms.Label()
        Me.cbVelUnit = New System.Windows.Forms.ComboBox()
        Me.KryptonLabel10 = New System.Windows.Forms.Label()
        Me.KryptonLabel5 = New System.Windows.Forms.Label()
        Me.tbCompBase = New System.Windows.Forms.TextBox()
        Me.KryptonLabel9 = New System.Windows.Forms.Label()
        Me.tbTmax = New System.Windows.Forms.TextBox()
        Me.tbTmin = New System.Windows.Forms.TextBox()
        Me.KryptonLabel8 = New System.Windows.Forms.Label()
        Me.KryptonLabel7 = New System.Windows.Forms.Label()
        Me.KryptonLabel4 = New System.Windows.Forms.Label()
        Me.cbBase = New System.Windows.Forms.ComboBox()
        Me.KryptonLabel6 = New System.Windows.Forms.Label()
        Me.tbPhase = New System.Windows.Forms.ComboBox()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.tbEquation = New System.Windows.Forms.TextBox()
        Me.KryptonLabel3 = New System.Windows.Forms.Label()
        Me.KryptonButton2 = New System.Windows.Forms.Button()
        Me.tbReacHeat = New System.Windows.Forms.TextBox()
        Me.KryptonLabel2 = New System.Windows.Forms.Label()
        Me.tbStoich = New System.Windows.Forms.TextBox()
        Me.KryptonDataGridView1 = New System.Windows.Forms.DataGridView()
        Me.Column2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column3 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column4 = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.Column5 = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.Column6 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column7 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column8 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.Column1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.KryptonLabel1 = New System.Windows.Forms.Label()
        Me.GroupBox4 = New System.Windows.Forms.GroupBox()
        Me.KryptonLabel19 = New System.Windows.Forms.Label()
        Me.KryptonLabel20 = New System.Windows.Forms.Label()
        Me.tbName = New System.Windows.Forms.TextBox()
        Me.tbDesc = New System.Windows.Forms.TextBox()
        Me.GroupBox2.SuspendLayout()
        Me.GroupBox3.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        CType(Me.KryptonDataGridView1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox4.SuspendLayout()
        Me.SuspendLayout()
        '
        'KryptonButton4
        '
        Me.KryptonButton4.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        resources.ApplyResources(Me.KryptonButton4, "KryptonButton4")
        Me.KryptonButton4.Name = "KryptonButton4"
        '
        'KryptonButton3
        '
        Me.KryptonButton3.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        resources.ApplyResources(Me.KryptonButton3, "KryptonButton3")
        Me.KryptonButton3.Name = "KryptonButton3"
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.GroupBox3)
        Me.GroupBox2.Controls.Add(Me.tbCompBase)
        Me.GroupBox2.Controls.Add(Me.KryptonLabel9)
        Me.GroupBox2.Controls.Add(Me.tbTmax)
        Me.GroupBox2.Controls.Add(Me.tbTmin)
        Me.GroupBox2.Controls.Add(Me.KryptonLabel8)
        Me.GroupBox2.Controls.Add(Me.KryptonLabel7)
        Me.GroupBox2.Controls.Add(Me.KryptonLabel4)
        Me.GroupBox2.Controls.Add(Me.cbBase)
        Me.GroupBox2.Controls.Add(Me.KryptonLabel6)
        Me.GroupBox2.Controls.Add(Me.tbPhase)
        resources.ApplyResources(Me.GroupBox2, "GroupBox2")
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.TabStop = False
        '
        'GroupBox3
        '
        Me.GroupBox3.Controls.Add(Me.KryptonLabel18)
        Me.GroupBox3.Controls.Add(Me.KryptonLabel17)
        Me.GroupBox3.Controls.Add(Me.tbRevE)
        Me.GroupBox3.Controls.Add(Me.KryptonLabel15)
        Me.GroupBox3.Controls.Add(Me.tbRevA)
        Me.GroupBox3.Controls.Add(Me.KryptonLabel16)
        Me.GroupBox3.Controls.Add(Me.tbFwdE)
        Me.GroupBox3.Controls.Add(Me.KryptonLabel14)
        Me.GroupBox3.Controls.Add(Me.tbFwdA)
        Me.GroupBox3.Controls.Add(Me.KryptonLabel13)
        Me.GroupBox3.Controls.Add(Me.KryptonLabel11)
        Me.GroupBox3.Controls.Add(Me.cbConcUnit)
        Me.GroupBox3.Controls.Add(Me.KryptonLabel12)
        Me.GroupBox3.Controls.Add(Me.cbVelUnit)
        Me.GroupBox3.Controls.Add(Me.KryptonLabel10)
        Me.GroupBox3.Controls.Add(Me.KryptonLabel5)
        resources.ApplyResources(Me.GroupBox3, "GroupBox3")
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.TabStop = False
        '
        'KryptonLabel18
        '
        resources.ApplyResources(Me.KryptonLabel18, "KryptonLabel18")
        Me.KryptonLabel18.Name = "KryptonLabel18"
        '
        'KryptonLabel17
        '
        resources.ApplyResources(Me.KryptonLabel17, "KryptonLabel17")
        Me.KryptonLabel17.Name = "KryptonLabel17"
        '
        'tbRevE
        '
        resources.ApplyResources(Me.tbRevE, "tbRevE")
        Me.tbRevE.Name = "tbRevE"
        '
        'KryptonLabel15
        '
        resources.ApplyResources(Me.KryptonLabel15, "KryptonLabel15")
        Me.KryptonLabel15.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.KryptonLabel15.Name = "KryptonLabel15"
        '
        'tbRevA
        '
        resources.ApplyResources(Me.tbRevA, "tbRevA")
        Me.tbRevA.Name = "tbRevA"
        '
        'KryptonLabel16
        '
        resources.ApplyResources(Me.KryptonLabel16, "KryptonLabel16")
        Me.KryptonLabel16.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.KryptonLabel16.Name = "KryptonLabel16"
        '
        'tbFwdE
        '
        resources.ApplyResources(Me.tbFwdE, "tbFwdE")
        Me.tbFwdE.Name = "tbFwdE"
        '
        'KryptonLabel14
        '
        resources.ApplyResources(Me.KryptonLabel14, "KryptonLabel14")
        Me.KryptonLabel14.Name = "KryptonLabel14"
        '
        'tbFwdA
        '
        resources.ApplyResources(Me.tbFwdA, "tbFwdA")
        Me.tbFwdA.Name = "tbFwdA"
        '
        'KryptonLabel13
        '
        resources.ApplyResources(Me.KryptonLabel13, "KryptonLabel13")
        Me.KryptonLabel13.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.KryptonLabel13.Name = "KryptonLabel13"
        '
        'KryptonLabel11
        '
        resources.ApplyResources(Me.KryptonLabel11, "KryptonLabel11")
        Me.KryptonLabel11.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.KryptonLabel11.Name = "KryptonLabel11"
        '
        'cbConcUnit
        '
        Me.cbConcUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbConcUnit.DropDownWidth = 121
        resources.ApplyResources(Me.cbConcUnit, "cbConcUnit")
        Me.cbConcUnit.Name = "cbConcUnit"
        '
        'KryptonLabel12
        '
        resources.ApplyResources(Me.KryptonLabel12, "KryptonLabel12")
        Me.KryptonLabel12.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.KryptonLabel12.Name = "KryptonLabel12"
        '
        'cbVelUnit
        '
        Me.cbVelUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbVelUnit.DropDownWidth = 121
        resources.ApplyResources(Me.cbVelUnit, "cbVelUnit")
        Me.cbVelUnit.Name = "cbVelUnit"
        '
        'KryptonLabel10
        '
        resources.ApplyResources(Me.KryptonLabel10, "KryptonLabel10")
        Me.KryptonLabel10.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.KryptonLabel10.Name = "KryptonLabel10"
        '
        'KryptonLabel5
        '
        resources.ApplyResources(Me.KryptonLabel5, "KryptonLabel5")
        Me.KryptonLabel5.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.KryptonLabel5.Name = "KryptonLabel5"
        '
        'tbCompBase
        '
        resources.ApplyResources(Me.tbCompBase, "tbCompBase")
        Me.tbCompBase.Name = "tbCompBase"
        Me.tbCompBase.ReadOnly = True
        '
        'KryptonLabel9
        '
        Me.KryptonLabel9.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        resources.ApplyResources(Me.KryptonLabel9, "KryptonLabel9")
        Me.KryptonLabel9.Name = "KryptonLabel9"
        '
        'tbTmax
        '
        resources.ApplyResources(Me.tbTmax, "tbTmax")
        Me.tbTmax.Name = "tbTmax"
        '
        'tbTmin
        '
        resources.ApplyResources(Me.tbTmin, "tbTmin")
        Me.tbTmin.Name = "tbTmin"
        '
        'KryptonLabel8
        '
        resources.ApplyResources(Me.KryptonLabel8, "KryptonLabel8")
        Me.KryptonLabel8.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.KryptonLabel8.Name = "KryptonLabel8"
        '
        'KryptonLabel7
        '
        resources.ApplyResources(Me.KryptonLabel7, "KryptonLabel7")
        Me.KryptonLabel7.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.KryptonLabel7.Name = "KryptonLabel7"
        '
        'KryptonLabel4
        '
        resources.ApplyResources(Me.KryptonLabel4, "KryptonLabel4")
        Me.KryptonLabel4.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.KryptonLabel4.Name = "KryptonLabel4"
        '
        'cbBase
        '
        Me.cbBase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbBase.DropDownWidth = 121
        resources.ApplyResources(Me.cbBase, "cbBase")
        Me.cbBase.Items.AddRange(New Object() {resources.GetString("cbBase.Items"), resources.GetString("cbBase.Items1"), resources.GetString("cbBase.Items2"), resources.GetString("cbBase.Items3"), resources.GetString("cbBase.Items4"), resources.GetString("cbBase.Items5"), resources.GetString("cbBase.Items6")})
        Me.cbBase.Name = "cbBase"
        '
        'KryptonLabel6
        '
        resources.ApplyResources(Me.KryptonLabel6, "KryptonLabel6")
        Me.KryptonLabel6.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.KryptonLabel6.Name = "KryptonLabel6"
        '
        'tbPhase
        '
        Me.tbPhase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.tbPhase.DropDownWidth = 121
        Me.tbPhase.Items.AddRange(New Object() {resources.GetString("tbPhase.Items"), resources.GetString("tbPhase.Items1"), resources.GetString("tbPhase.Items2")})
        resources.ApplyResources(Me.tbPhase, "tbPhase")
        Me.tbPhase.Name = "tbPhase"
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.tbEquation)
        Me.GroupBox1.Controls.Add(Me.KryptonLabel3)
        Me.GroupBox1.Controls.Add(Me.KryptonButton2)
        Me.GroupBox1.Controls.Add(Me.tbReacHeat)
        Me.GroupBox1.Controls.Add(Me.KryptonLabel2)
        Me.GroupBox1.Controls.Add(Me.tbStoich)
        Me.GroupBox1.Controls.Add(Me.KryptonDataGridView1)
        Me.GroupBox1.Controls.Add(Me.KryptonLabel1)
        resources.ApplyResources(Me.GroupBox1, "GroupBox1")
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.TabStop = False
        '
        'tbEquation
        '
        resources.ApplyResources(Me.tbEquation, "tbEquation")
        Me.tbEquation.Name = "tbEquation"
        Me.tbEquation.ReadOnly = True
        '
        'KryptonLabel3
        '
        Me.KryptonLabel3.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        resources.ApplyResources(Me.KryptonLabel3, "KryptonLabel3")
        Me.KryptonLabel3.Name = "KryptonLabel3"
        '
        'KryptonButton2
        '
        resources.ApplyResources(Me.KryptonButton2, "KryptonButton2")
        Me.KryptonButton2.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        Me.KryptonButton2.Name = "KryptonButton2"
        '
        'tbReacHeat
        '
        resources.ApplyResources(Me.tbReacHeat, "tbReacHeat")
        Me.tbReacHeat.Name = "tbReacHeat"
        Me.tbReacHeat.ReadOnly = True
        '
        'KryptonLabel2
        '
        Me.KryptonLabel2.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        resources.ApplyResources(Me.KryptonLabel2, "KryptonLabel2")
        Me.KryptonLabel2.Name = "KryptonLabel2"
        '
        'tbStoich
        '
        resources.ApplyResources(Me.tbStoich, "tbStoich")
        Me.tbStoich.Name = "tbStoich"
        Me.tbStoich.ReadOnly = True
        '
        'KryptonDataGridView1
        '
        Me.KryptonDataGridView1.AllowUserToAddRows = False
        Me.KryptonDataGridView1.AllowUserToDeleteRows = False
        Me.KryptonDataGridView1.AllowUserToResizeRows = False
        Me.KryptonDataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.KryptonDataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.KryptonDataGridView1.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.Column2, Me.Column3, Me.Column4, Me.Column5, Me.Column6, Me.Column7, Me.Column8, Me.Column1})
        resources.ApplyResources(Me.KryptonDataGridView1, "KryptonDataGridView1")
        Me.KryptonDataGridView1.MultiSelect = False
        Me.KryptonDataGridView1.Name = "KryptonDataGridView1"
        Me.KryptonDataGridView1.RowHeadersVisible = False
        Me.KryptonDataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect
        '
        'Column2
        '
        Me.Column2.FillWeight = 40.0!
        resources.ApplyResources(Me.Column2, "Column2")
        Me.Column2.Name = "Column2"
        Me.Column2.ReadOnly = True
        Me.Column2.ToolTipText = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        '
        'Column3
        '
        Me.Column3.FillWeight = 20.0!
        resources.ApplyResources(Me.Column3, "Column3")
        Me.Column3.Name = "Column3"
        Me.Column3.ReadOnly = True
        Me.Column3.ToolTipText = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        '
        'Column4
        '
        Me.Column4.FillWeight = 10.0!
        resources.ApplyResources(Me.Column4, "Column4")
        Me.Column4.Name = "Column4"
        Me.Column4.ToolTipText = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        '
        'Column5
        '
        Me.Column5.FillWeight = 10.0!
        resources.ApplyResources(Me.Column5, "Column5")
        Me.Column5.Name = "Column5"
        '
        'Column6
        '
        Me.Column6.FillWeight = 20.0!
        resources.ApplyResources(Me.Column6, "Column6")
        Me.Column6.Name = "Column6"
        Me.Column6.ToolTipText = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        '
        'Column7
        '
        Me.Column7.FillWeight = 5.0!
        resources.ApplyResources(Me.Column7, "Column7")
        Me.Column7.Name = "Column7"
        '
        'Column8
        '
        Me.Column8.FillWeight = 5.0!
        resources.ApplyResources(Me.Column8, "Column8")
        Me.Column8.Name = "Column8"
        '
        'Column1
        '
        resources.ApplyResources(Me.Column1, "Column1")
        Me.Column1.Name = "Column1"
        Me.Column1.ReadOnly = True
        Me.Column1.ToolTipText = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        '
        'KryptonLabel1
        '
        Me.KryptonLabel1.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        resources.ApplyResources(Me.KryptonLabel1, "KryptonLabel1")
        Me.KryptonLabel1.Name = "KryptonLabel1"
        '
        'GroupBox4
        '
        Me.GroupBox4.Controls.Add(Me.KryptonLabel19)
        Me.GroupBox4.Controls.Add(Me.KryptonLabel20)
        Me.GroupBox4.Controls.Add(Me.tbName)
        Me.GroupBox4.Controls.Add(Me.tbDesc)
        resources.ApplyResources(Me.GroupBox4, "GroupBox4")
        Me.GroupBox4.Name = "GroupBox4"
        Me.GroupBox4.TabStop = False
        '
        'KryptonLabel19
        '
        Me.KryptonLabel19.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        resources.ApplyResources(Me.KryptonLabel19, "KryptonLabel19")
        Me.KryptonLabel19.Name = "KryptonLabel19"
        '
        'KryptonLabel20
        '
        Me.KryptonLabel20.ImageKey = Global.DWSIM.My.Resources.DWSIM.NewVersionAvailable
        resources.ApplyResources(Me.KryptonLabel20, "KryptonLabel20")
        Me.KryptonLabel20.Name = "KryptonLabel20"
        '
        'tbName
        '
        resources.ApplyResources(Me.tbName, "tbName")
        Me.tbName.Name = "tbName"
        '
        'tbDesc
        '
        resources.ApplyResources(Me.tbDesc, "tbDesc")
        Me.tbDesc.Name = "tbDesc"
        '
        'FormReacKinetic
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.GroupBox4)
        Me.Controls.Add(Me.KryptonButton4)
        Me.Controls.Add(Me.KryptonButton3)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.GroupBox1)
        Me.DoubleBuffered = True
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.Name = "FormReacKinetic"
        Me.ShowInTaskbar = False
        Me.TopMost = True
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox3.PerformLayout()
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        CType(Me.KryptonDataGridView1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox4.ResumeLayout(False)
        Me.GroupBox4.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Public WithEvents KryptonButton4 As System.Windows.Forms.Button
    Public WithEvents KryptonButton3 As System.Windows.Forms.Button
    Public WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Public WithEvents GroupBox3 As System.Windows.Forms.GroupBox
    Public WithEvents tbCompBase As System.Windows.Forms.TextBox
    Public WithEvents KryptonLabel9 As System.Windows.Forms.Label
    Public WithEvents tbTmax As System.Windows.Forms.TextBox
    Public WithEvents tbTmin As System.Windows.Forms.TextBox
    Public WithEvents KryptonLabel8 As System.Windows.Forms.Label
    Public WithEvents KryptonLabel7 As System.Windows.Forms.Label
    Public WithEvents KryptonLabel4 As System.Windows.Forms.Label
    Public WithEvents cbBase As System.Windows.Forms.ComboBox
    Public WithEvents KryptonLabel6 As System.Windows.Forms.Label
    Public WithEvents tbPhase As System.Windows.Forms.ComboBox
    Public WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Public WithEvents tbEquation As System.Windows.Forms.TextBox
    Public WithEvents KryptonLabel3 As System.Windows.Forms.Label
    Public WithEvents KryptonButton2 As System.Windows.Forms.Button
    Public WithEvents tbReacHeat As System.Windows.Forms.TextBox
    Public WithEvents KryptonLabel2 As System.Windows.Forms.Label
    Public WithEvents tbStoich As System.Windows.Forms.TextBox
    Public WithEvents KryptonDataGridView1 As System.Windows.Forms.DataGridView
    Public WithEvents KryptonLabel1 As System.Windows.Forms.Label
    Public WithEvents KryptonLabel11 As System.Windows.Forms.Label
    Public WithEvents cbConcUnit As System.Windows.Forms.ComboBox
    Public WithEvents KryptonLabel12 As System.Windows.Forms.Label
    Public WithEvents cbVelUnit As System.Windows.Forms.ComboBox
    Public WithEvents KryptonLabel10 As System.Windows.Forms.Label
    Public WithEvents KryptonLabel5 As System.Windows.Forms.Label
    Public WithEvents KryptonLabel18 As System.Windows.Forms.Label
    Public WithEvents KryptonLabel17 As System.Windows.Forms.Label
    Public WithEvents tbRevE As System.Windows.Forms.TextBox
    Public WithEvents KryptonLabel15 As System.Windows.Forms.Label
    Public WithEvents tbRevA As System.Windows.Forms.TextBox
    Public WithEvents KryptonLabel16 As System.Windows.Forms.Label
    Public WithEvents tbFwdE As System.Windows.Forms.TextBox
    Public WithEvents KryptonLabel14 As System.Windows.Forms.Label
    Public WithEvents tbFwdA As System.Windows.Forms.TextBox
    Public WithEvents KryptonLabel13 As System.Windows.Forms.Label
    Public WithEvents GroupBox4 As System.Windows.Forms.GroupBox
    Public WithEvents KryptonLabel19 As System.Windows.Forms.Label
    Public WithEvents KryptonLabel20 As System.Windows.Forms.Label
    Public WithEvents tbName As System.Windows.Forms.TextBox
    Public WithEvents tbDesc As System.Windows.Forms.TextBox
    Public WithEvents Column2 As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents Column3 As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents Column4 As System.Windows.Forms.DataGridViewCheckBoxColumn
    Public WithEvents Column5 As System.Windows.Forms.DataGridViewCheckBoxColumn
    Public WithEvents Column6 As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents Column7 As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents Column8 As System.Windows.Forms.DataGridViewTextBoxColumn
    Public WithEvents Column1 As System.Windows.Forms.DataGridViewTextBoxColumn
End Class
