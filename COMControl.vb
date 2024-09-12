Public Class COMControl


#Region "Buttons"
    Private Sub btnVCConnect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnVCConnect.Click
        lblVCConnectionStatus.Text = "Connecting"

        btnVCConnect.Enabled = False
        btnVCDisconnect.Enabled = True
        cbVC_COMPort.Enabled = False

        If cbVC_COMPort.Text = "" Then
            MsgBox("Specify COM Port")
            lblVCConnectionStatus.Text = "Not Connected"
            Exit Sub
        End If

        IrrigationControl.StartValveControl()

    End Sub

    Private Sub btnVCDisconnect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnVCDisconnect.Click
        btnVCConnect.Enabled = True
        btnVCDisconnect.Enabled = False
        cbVC_COMPort.Enabled = True
        IrrigationControl._VCarduino.StopCommunication()

        lblVCConnectionStatus.Text = "Not Connected"

    End Sub


    Private Sub btnSMConnect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSMConnect.Click
        lblSMConnectionStatus.Text = "Connecting"

        If cbSM_COMPort.Text = "" Then
            MsgBox("Connect Error")
            lblSMConnectionStatus.Text = "Not Connected"
            Exit Sub
        End If

        IrrigationControl.StartSoilMonitor()

    End Sub

    Private Sub btnSMDisconnect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSMDisconnect.Click
        IrrigationControl.tSleep.Stop()
        btnSMConnect.Enabled = True
        btnSMDisconnect.Enabled = False
        btnStart.Enabled = False
        btnStop.Enabled = False
        cbSM_COMPort.Enabled = True
        mtxtPollingInterval.Enabled = True
        IrrigationControl._SMarduino.StopCommunication()

        lblSMConnectionStatus.Text = "Not Connected"

    End Sub

    Private Sub btnStart_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnStart.Click
        lblMonitor.Text = "Monitoring"
        btnStop.Enabled = True
        btnStart.Enabled = False
        mtxtPollingInterval.Enabled = False

        IrrigationControl.Event_HistoryTableAdapter1.InsertQuery("9015")
        IrrigationControl.CheckSoilMoisture()

        IrrigationControl.tSleep.Interval = CInt(mtxtPollingInterval.Text) * 60000  'convert to milliseconds
        IrrigationControl.tSleep.Start()

    End Sub

    Private Sub btnStop_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnStop.Click
        IrrigationControl.tSleep.Stop()

        IrrigationControl.Event_HistoryTableAdapter1.InsertQuery("9016")
        lblMonitor.Text = "Not Monitoring"
        btnStart.Enabled = True
        btnStop.Enabled = False
        mtxtPollingInterval.Enabled = True
    End Sub
#End Region

    Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
        IrrigationControl.SaveParameters()
        Me.Hide()
    End Sub

End Class