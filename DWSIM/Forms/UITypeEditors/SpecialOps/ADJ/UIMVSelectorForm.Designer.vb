<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class UIMVSelectorForm
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(UIMVSelectorForm))
        Me.GroupBox1 = New System.Windows.Forms.GroupBox
        Me.TreeView3 = New System.Windows.Forms.TreeView
        Me.TreeView2 = New System.Windows.Forms.TreeView
        Me.TreeView1 = New System.Windows.Forms.TreeView
        Me.Label3 = New System.Windows.Forms.Label
        Me.Label2 = New System.Windows.Forms.Label
        Me.Label1 = New System.Windows.Forms.Label
        Me.KryptonButton1 = New System.Windows.Forms.Button
        Me.KryptonButton2 = New System.Windows.Forms.Button
        Me.GroupBox1.SuspendLayout()
        Me.SuspendLayout()
        '
        'GroupBox1
        '
        Me.GroupBox1.AccessibleDescription = Nothing
        Me.GroupBox1.AccessibleName = Nothing
        resources.ApplyResources(Me.GroupBox1, "GroupBox1")
        Me.GroupBox1.BackgroundImage = Nothing
        Me.GroupBox1.Controls.Add(Me.TreeView3)
        Me.GroupBox1.Controls.Add(Me.TreeView2)
        Me.GroupBox1.Controls.Add(Me.TreeView1)
        Me.GroupBox1.Controls.Add(Me.Label3)
        Me.GroupBox1.Controls.Add(Me.Label2)
        Me.GroupBox1.Controls.Add(Me.Label1)
        Me.GroupBox1.Font = Nothing
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.TabStop = False
        '
        'TreeView3
        '
        Me.TreeView3.AccessibleDescription = Nothing
        Me.TreeView3.AccessibleName = Nothing
        resources.ApplyResources(Me.TreeView3, "TreeView3")
        Me.TreeView3.BackgroundImage = Nothing
        Me.TreeView3.Font = Nothing
        Me.TreeView3.FullRowSelect = True
        Me.TreeView3.HideSelection = False
        Me.TreeView3.Name = "TreeView3"
        Me.TreeView3.ShowLines = False
        Me.TreeView3.ShowPlusMinus = False
        Me.TreeView3.ShowRootLines = False
        '
        'TreeView2
        '
        Me.TreeView2.AccessibleDescription = Nothing
        Me.TreeView2.AccessibleName = Nothing
        resources.ApplyResources(Me.TreeView2, "TreeView2")
        Me.TreeView2.BackgroundImage = Nothing
        Me.TreeView2.Font = Nothing
        Me.TreeView2.FullRowSelect = True
        Me.TreeView2.HideSelection = False
        Me.TreeView2.Name = "TreeView2"
        Me.TreeView2.ShowLines = False
        Me.TreeView2.ShowPlusMinus = False
        Me.TreeView2.ShowRootLines = False
        '
        'TreeView1
        '
        Me.TreeView1.AccessibleDescription = Nothing
        Me.TreeView1.AccessibleName = Nothing
        resources.ApplyResources(Me.TreeView1, "TreeView1")
        Me.TreeView1.BackgroundImage = Nothing
        Me.TreeView1.Font = Nothing
        Me.TreeView1.FullRowSelect = True
        Me.TreeView1.HideSelection = False
        Me.TreeView1.Name = "TreeView1"
        Me.TreeView1.ShowLines = False
        Me.TreeView1.ShowPlusMinus = False
        Me.TreeView1.ShowRootLines = False
        '
        'Label3
        '
        Me.Label3.AccessibleDescription = Nothing
        Me.Label3.AccessibleName = Nothing
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.Font = Nothing
        Me.Label3.Name = "Label3"
        '
        'Label2
        '
        Me.Label2.AccessibleDescription = Nothing
        Me.Label2.AccessibleName = Nothing
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Font = Nothing
        Me.Label2.Name = "Label2"
        '
        'Label1
        '
        Me.Label1.AccessibleDescription = Nothing
        Me.Label1.AccessibleName = Nothing
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Font = Nothing
        Me.Label1.Name = "Label1"
        '
        'KryptonButton1
        '
        Me.KryptonButton1.AccessibleDescription = Nothing
        Me.KryptonButton1.AccessibleName = Nothing
        resources.ApplyResources(Me.KryptonButton1, "KryptonButton1")
        Me.KryptonButton1.BackgroundImage = Nothing
        Me.KryptonButton1.Font = Nothing
        Me.KryptonButton1.Name = "KryptonButton1"











        '
        'KryptonButton2
        '
        Me.KryptonButton2.AccessibleDescription = Nothing
        Me.KryptonButton2.AccessibleName = Nothing
        resources.ApplyResources(Me.KryptonButton2, "KryptonButton2")
        Me.KryptonButton2.BackgroundImage = Nothing
        Me.KryptonButton2.Font = Nothing
        Me.KryptonButton2.Name = "KryptonButton2"











        '
        'UIMVSelectorForm
        '
        Me.AccessibleDescription = Nothing
        Me.AccessibleName = Nothing
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font

        Me.BackgroundImage = Nothing
        Me.Controls.Add(Me.KryptonButton2)
        Me.Controls.Add(Me.KryptonButton1)
        Me.Controls.Add(Me.GroupBox1)
        Me.Font = Nothing
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.Icon = Nothing
        Me.Name = "UIMVSelectorForm"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Public WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Public WithEvents KryptonButton1 As System.Windows.Forms.Button
    Public WithEvents KryptonButton2 As System.Windows.Forms.Button
    Public WithEvents Label3 As System.Windows.Forms.Label
    Public WithEvents Label2 As System.Windows.Forms.Label
    Public WithEvents Label1 As System.Windows.Forms.Label
    Public WithEvents TreeView1 As System.Windows.Forms.TreeView
    Public WithEvents TreeView3 As System.Windows.Forms.TreeView
    Public WithEvents TreeView2 As System.Windows.Forms.TreeView
End Class
