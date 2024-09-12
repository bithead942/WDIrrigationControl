<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class COMControl
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
        Me.GroupBox2 = New System.Windows.Forms.GroupBox()
        Me.cbVC_COMPort = New System.Windows.Forms.ComboBox()
        Me.Label36 = New System.Windows.Forms.Label()
        Me.btnVCDisconnect = New System.Windows.Forms.Button()
        Me.btnVCConnect = New System.Windows.Forms.Button()
        Me.lblVCConnectionStatus = New System.Windows.Forms.Label()
        Me.Label38 = New System.Windows.Forms.Label()
        Me.gbMonitor = New System.Windows.Forms.GroupBox()
        Me.Label22 = New System.Windows.Forms.Label()
        Me.mtxtPollingInterval = New System.Windows.Forms.MaskedTextBox()
        Me.Label20 = New System.Windows.Forms.Label()
        Me.lblMonitor = New System.Windows.Forms.Label()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.btnStop = New System.Windows.Forms.Button()
        Me.btnStart = New System.Windows.Forms.Button()
        Me.cbSM_COMPort = New System.Windows.Forms.ComboBox()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.btnSMDisconnect = New System.Windows.Forms.Button()
        Me.btnSMConnect = New System.Windows.Forms.Button()
        Me.lblSMConnectionStatus = New System.Windows.Forms.Label()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.btnClose = New System.Windows.Forms.Button()
        Me.GroupBox2.SuspendLayout()
        Me.gbMonitor.SuspendLayout()
        Me.SuspendLayout()
        '
        'GroupBox2
        '
        Me.GroupBox2.Controls.Add(Me.cbVC_COMPort)
        Me.GroupBox2.Controls.Add(Me.Label36)
        Me.GroupBox2.Controls.Add(Me.btnVCDisconnect)
        Me.GroupBox2.Controls.Add(Me.btnVCConnect)
        Me.GroupBox2.Controls.Add(Me.lblVCConnectionStatus)
        Me.GroupBox2.Controls.Add(Me.Label38)
        Me.GroupBox2.Location = New System.Drawing.Point(12, 204)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(189, 102)
        Me.GroupBox2.TabIndex = 94
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Valve Control Comm"
        '
        'cbVC_COMPort
        '
        Me.cbVC_COMPort.FormattingEnabled = True
        Me.cbVC_COMPort.Location = New System.Drawing.Point(68, 13)
        Me.cbVC_COMPort.Name = "cbVC_COMPort"
        Me.cbVC_COMPort.Size = New System.Drawing.Size(60, 21)
        Me.cbVC_COMPort.TabIndex = 70
        Me.cbVC_COMPort.Text = "COM1"
        '
        'Label36
        '
        Me.Label36.AutoSize = True
        Me.Label36.Location = New System.Drawing.Point(5, 17)
        Me.Label36.Name = "Label36"
        Me.Label36.Size = New System.Drawing.Size(56, 13)
        Me.Label36.TabIndex = 69
        Me.Label36.Text = "COM Port:"
        '
        'btnVCDisconnect
        '
        Me.btnVCDisconnect.BackColor = System.Drawing.SystemColors.Control
        Me.btnVCDisconnect.Enabled = False
        Me.btnVCDisconnect.Location = New System.Drawing.Point(99, 60)
        Me.btnVCDisconnect.Name = "btnVCDisconnect"
        Me.btnVCDisconnect.Size = New System.Drawing.Size(74, 34)
        Me.btnVCDisconnect.TabIndex = 68
        Me.btnVCDisconnect.Text = "Disconnect"
        Me.btnVCDisconnect.UseVisualStyleBackColor = False
        '
        'btnVCConnect
        '
        Me.btnVCConnect.BackColor = System.Drawing.SystemColors.Control
        Me.btnVCConnect.Location = New System.Drawing.Point(16, 60)
        Me.btnVCConnect.Name = "btnVCConnect"
        Me.btnVCConnect.Size = New System.Drawing.Size(74, 34)
        Me.btnVCConnect.TabIndex = 67
        Me.btnVCConnect.Text = "Connect"
        Me.btnVCConnect.UseVisualStyleBackColor = False
        '
        'lblVCConnectionStatus
        '
        Me.lblVCConnectionStatus.AutoSize = True
        Me.lblVCConnectionStatus.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblVCConnectionStatus.Location = New System.Drawing.Point(95, 41)
        Me.lblVCConnectionStatus.Name = "lblVCConnectionStatus"
        Me.lblVCConnectionStatus.Size = New System.Drawing.Size(80, 12)
        Me.lblVCConnectionStatus.TabIndex = 66
        Me.lblVCConnectionStatus.Text = "Not Connected"
        '
        'Label38
        '
        Me.Label38.AutoSize = True
        Me.Label38.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label38.Location = New System.Drawing.Point(5, 41)
        Me.Label38.Name = "Label38"
        Me.Label38.Size = New System.Drawing.Size(72, 12)
        Me.Label38.TabIndex = 65
        Me.Label38.Text = "Connect Status:"
        '
        'gbMonitor
        '
        Me.gbMonitor.Controls.Add(Me.Label22)
        Me.gbMonitor.Controls.Add(Me.mtxtPollingInterval)
        Me.gbMonitor.Controls.Add(Me.Label20)
        Me.gbMonitor.Controls.Add(Me.lblMonitor)
        Me.gbMonitor.Controls.Add(Me.Label7)
        Me.gbMonitor.Controls.Add(Me.btnStop)
        Me.gbMonitor.Controls.Add(Me.btnStart)
        Me.gbMonitor.Controls.Add(Me.cbSM_COMPort)
        Me.gbMonitor.Controls.Add(Me.Label10)
        Me.gbMonitor.Controls.Add(Me.btnSMDisconnect)
        Me.gbMonitor.Controls.Add(Me.btnSMConnect)
        Me.gbMonitor.Controls.Add(Me.lblSMConnectionStatus)
        Me.gbMonitor.Controls.Add(Me.Label8)
        Me.gbMonitor.Location = New System.Drawing.Point(12, 12)
        Me.gbMonitor.Name = "gbMonitor"
        Me.gbMonitor.Size = New System.Drawing.Size(189, 186)
        Me.gbMonitor.TabIndex = 93
        Me.gbMonitor.TabStop = False
        Me.gbMonitor.Text = "Soil Moisture Comm"
        '
        'Label22
        '
        Me.Label22.AutoSize = True
        Me.Label22.Location = New System.Drawing.Point(134, 166)
        Me.Label22.Name = "Label22"
        Me.Label22.Size = New System.Drawing.Size(43, 13)
        Me.Label22.TabIndex = 89
        Me.Label22.Text = "minutes"
        '
        'mtxtPollingInterval
        '
        Me.mtxtPollingInterval.Enabled = False
        Me.mtxtPollingInterval.Location = New System.Drawing.Point(97, 160)
        Me.mtxtPollingInterval.Mask = "999"
        Me.mtxtPollingInterval.Name = "mtxtPollingInterval"
        Me.mtxtPollingInterval.Size = New System.Drawing.Size(34, 20)
        Me.mtxtPollingInterval.TabIndex = 88
        Me.mtxtPollingInterval.Text = "120"
        '
        'Label20
        '
        Me.Label20.AutoSize = True
        Me.Label20.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label20.Location = New System.Drawing.Point(7, 164)
        Me.Label20.Name = "Label20"
        Me.Label20.Size = New System.Drawing.Size(69, 12)
        Me.Label20.TabIndex = 75
        Me.Label20.Text = "Polling Interval:"
        '
        'lblMonitor
        '
        Me.lblMonitor.AutoSize = True
        Me.lblMonitor.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMonitor.Location = New System.Drawing.Point(95, 143)
        Me.lblMonitor.Name = "lblMonitor"
        Me.lblMonitor.Size = New System.Drawing.Size(80, 12)
        Me.lblMonitor.TabIndex = 74
        Me.lblMonitor.Text = "Not Monitoring"
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label7.Location = New System.Drawing.Point(6, 143)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(81, 12)
        Me.Label7.TabIndex = 73
        Me.Label7.Text = "Monitoring Status:"
        '
        'btnStop
        '
        Me.btnStop.BackColor = System.Drawing.SystemColors.Control
        Me.btnStop.Enabled = False
        Me.btnStop.Location = New System.Drawing.Point(99, 100)
        Me.btnStop.Name = "btnStop"
        Me.btnStop.Size = New System.Drawing.Size(74, 34)
        Me.btnStop.TabIndex = 72
        Me.btnStop.Text = "Stop Monitoring"
        Me.btnStop.UseVisualStyleBackColor = False
        '
        'btnStart
        '
        Me.btnStart.BackColor = System.Drawing.SystemColors.Control
        Me.btnStart.Enabled = False
        Me.btnStart.Location = New System.Drawing.Point(16, 100)
        Me.btnStart.Name = "btnStart"
        Me.btnStart.Size = New System.Drawing.Size(74, 34)
        Me.btnStart.TabIndex = 71
        Me.btnStart.Text = "Start Monitoring"
        Me.btnStart.UseVisualStyleBackColor = False
        '
        'cbSM_COMPort
        '
        Me.cbSM_COMPort.FormattingEnabled = True
        Me.cbSM_COMPort.Location = New System.Drawing.Point(68, 13)
        Me.cbSM_COMPort.Name = "cbSM_COMPort"
        Me.cbSM_COMPort.Size = New System.Drawing.Size(60, 21)
        Me.cbSM_COMPort.TabIndex = 70
        Me.cbSM_COMPort.Text = "COM1"
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Location = New System.Drawing.Point(5, 17)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(56, 13)
        Me.Label10.TabIndex = 69
        Me.Label10.Text = "COM Port:"
        '
        'btnSMDisconnect
        '
        Me.btnSMDisconnect.BackColor = System.Drawing.SystemColors.Control
        Me.btnSMDisconnect.Enabled = False
        Me.btnSMDisconnect.Location = New System.Drawing.Point(99, 60)
        Me.btnSMDisconnect.Name = "btnSMDisconnect"
        Me.btnSMDisconnect.Size = New System.Drawing.Size(74, 34)
        Me.btnSMDisconnect.TabIndex = 68
        Me.btnSMDisconnect.Text = "Disconnect"
        Me.btnSMDisconnect.UseVisualStyleBackColor = False
        '
        'btnSMConnect
        '
        Me.btnSMConnect.BackColor = System.Drawing.SystemColors.Control
        Me.btnSMConnect.Location = New System.Drawing.Point(16, 60)
        Me.btnSMConnect.Name = "btnSMConnect"
        Me.btnSMConnect.Size = New System.Drawing.Size(74, 34)
        Me.btnSMConnect.TabIndex = 67
        Me.btnSMConnect.Text = "Connect"
        Me.btnSMConnect.UseVisualStyleBackColor = False
        '
        'lblSMConnectionStatus
        '
        Me.lblSMConnectionStatus.AutoSize = True
        Me.lblSMConnectionStatus.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSMConnectionStatus.Location = New System.Drawing.Point(95, 41)
        Me.lblSMConnectionStatus.Name = "lblSMConnectionStatus"
        Me.lblSMConnectionStatus.Size = New System.Drawing.Size(80, 12)
        Me.lblSMConnectionStatus.TabIndex = 66
        Me.lblSMConnectionStatus.Text = "Not Connected"
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label8.Location = New System.Drawing.Point(5, 41)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(72, 12)
        Me.Label8.TabIndex = 65
        Me.Label8.Text = "Connect Status:"
        '
        'btnClose
        '
        Me.btnClose.Location = New System.Drawing.Point(73, 313)
        Me.btnClose.Name = "btnClose"
        Me.btnClose.Size = New System.Drawing.Size(67, 23)
        Me.btnClose.TabIndex = 95
        Me.btnClose.Text = "Close"
        Me.btnClose.UseVisualStyleBackColor = True
        '
        'COMControl
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(213, 344)
        Me.ControlBox = False
        Me.Controls.Add(Me.btnClose)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.gbMonitor)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "COMControl"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "COMControl"
        Me.GroupBox2.ResumeLayout(False)
        Me.GroupBox2.PerformLayout()
        Me.gbMonitor.ResumeLayout(False)
        Me.gbMonitor.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents cbVC_COMPort As System.Windows.Forms.ComboBox
    Friend WithEvents Label36 As System.Windows.Forms.Label
    Friend WithEvents btnVCDisconnect As System.Windows.Forms.Button
    Friend WithEvents btnVCConnect As System.Windows.Forms.Button
    Friend WithEvents lblVCConnectionStatus As System.Windows.Forms.Label
    Friend WithEvents Label38 As System.Windows.Forms.Label
    Friend WithEvents gbMonitor As System.Windows.Forms.GroupBox
    Friend WithEvents Label22 As System.Windows.Forms.Label
    Friend WithEvents mtxtPollingInterval As System.Windows.Forms.MaskedTextBox
    Friend WithEvents Label20 As System.Windows.Forms.Label
    Friend WithEvents lblMonitor As System.Windows.Forms.Label
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents btnStop As System.Windows.Forms.Button
    Friend WithEvents btnStart As System.Windows.Forms.Button
    Friend WithEvents cbSM_COMPort As System.Windows.Forms.ComboBox
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents btnSMDisconnect As System.Windows.Forms.Button
    Friend WithEvents btnSMConnect As System.Windows.Forms.Button
    Friend WithEvents lblSMConnectionStatus As System.Windows.Forms.Label
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents btnClose As System.Windows.Forms.Button
End Class
