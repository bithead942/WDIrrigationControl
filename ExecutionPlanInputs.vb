Public Class ExecutionPlanInputs

    Private Sub btnRefresh_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRefresh.Click
        btnRefresh.Enabled = False
        IrrigationControl.CheckSoilMoisture()  'Also triggers ExecutionPlanUpdate
        btnRefresh.Enabled = True
    End Sub

    Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
        Me.Hide()

    End Sub

End Class