Public Class ValveControl

    Public ValveTimeArray(14) As Object
    Public ValveActiveArray(14) As Object
    Public CheckMoisture As Object
    Public iCheckMoistureVal As Integer

    Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
        IrrigationControl.SaveParameters()
        IrrigationControl.CheckSoilMoisture()  'Also triggers ExecutionPlanUpdate
        Me.Hide()
    End Sub

    Private Sub ValveControl_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Update1EndTime()
        Update2EndTime()
        Update3EndTime()
        Update4EndTime()

        If cb1Active.Checked Then
            TabControl1.SelectedIndex = 0
            CheckMoisture = cb1CheckMoisture
            iCheckMoistureVal = CInt(cbo1CheckMoisture.Text)
        End If

        If cb2Active.Checked Then
            TabControl1.SelectedIndex = 1
            CheckMoisture = cb2CheckMoisture
            iCheckMoistureVal = CInt(cbo2CheckMoisture.Text)
        End If

        If cb3Active.Checked Then
            TabControl1.SelectedIndex = 2
            CheckMoisture = cb3CheckMoisture
            iCheckMoistureVal = CInt(cbo3CheckMoisture.Text)
        End If

        If cb4Active.Checked Then
            TabControl1.SelectedIndex = 3
            CheckMoisture = cb4CheckMoisture
            iCheckMoistureVal = CInt(cbo4CheckMoisture.Text)
        End If

    End Sub


#Region "Plan 1"
    Private Sub Update1EndTime()
        Dim dtRunTime As Date
        Dim iRunTime As Double = 0
        Dim iCount As Integer = 0

        Try
            If mtxt1StartHour.Text = "" Or mtxt1StartMinute.Text = "" Then
                dtRunTime = "00:00"
            Else
                dtRunTime = CDate(mtxt1StartHour.Text & ":" & mtxt1StartMinute.Text)
            End If
        Catch ex As Exception
            dtRunTime = "00:00"
        End Try


        If ckb1Zone1Active.Checked Then
            If mtxt1Zone1RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt1Zone1RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb1Zone2Active.Checked Then
            If mtxt1Zone2RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt1Zone2RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb1Zone3Active.Checked Then
            If mtxt1Zone3RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt1Zone3RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb1Zone4Active.Checked Then
            If mtxt1Zone4RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt1Zone4RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb1Zone5Active.Checked Then
            If mtxt1Zone5RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt1Zone5RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb1Zone6Active.Checked Then
            If mtxt1Zone6RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt1Zone6RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb1Zone7Active.Checked Then
            If mtxt1Zone7RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt1Zone7RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb1Zone8Active.Checked Then
            If mtxt1Zone8RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt1Zone8RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb1Zone9Active.Checked Then
            If mtxt1Zone9RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt1Zone9RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb1Zone10Active.Checked Then
            If mtxt1Zone10RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt1Zone10RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb1Zone11Active.Checked Then
            If mtxt1Zone11RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt1Zone11RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb1Zone12Active.Checked Then
            If mtxt1Zone12RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt1Zone12RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb1Zone13Active.Checked Then
            If mtxt1Zone13RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt1Zone13RunTime.Text)
                iCount = iCount + 1
            End If
        End If

        If iRunTime > 0 Then
            iRunTime = iRunTime + iCount  'One minute after each zone
        Else
            iRunTime = 0
        End If

        dtRunTime = dtRunTime.AddMinutes(iRunTime)
        If DatePart(DateInterval.Hour, dtRunTime) >= 10 Then
            lbl1EndTime.Text = DatePart(DateInterval.Hour, dtRunTime)

        ElseIf DatePart(DateInterval.Hour, dtRunTime) < 10 And DatePart(DateInterval.Hour, dtRunTime) > 0 Then
            lbl1EndTime.Text = "0" & DatePart(DateInterval.Hour, dtRunTime)
        Else
            lbl1EndTime.Text = "00"
        End If

        If DatePart(DateInterval.Minute, dtRunTime) >= 10 Then
            lbl1EndTime.Text = lbl1EndTime.Text & ":" & DatePart(DateInterval.Minute, dtRunTime)

        ElseIf DatePart(DateInterval.Minute, dtRunTime) < 10 And DatePart(DateInterval.Minute, dtRunTime) > 0 Then
            lbl1EndTime.Text = lbl1EndTime.Text & ":0" & DatePart(DateInterval.Minute, dtRunTime)
        Else
            lbl1EndTime.Text = lbl1EndTime.Text & ":00"
        End If

        If cb1Active.Checked Then
            IrrigationControl.lblStartTime.Text = mtxt1StartHour.Text & ":" & mtxt1StartMinute.Text
            IrrigationControl.lblEndTime.Text = lbl1EndTime.Text
        End If
    End Sub

    Private Sub cb1CheckMoisture_CheckedChanged(sender As Object, e As EventArgs) Handles cb1CheckMoisture.CheckedChanged
        If cb1CheckMoisture.Checked Then
            cbo1CheckMoisture.Enabled = True
        Else
            cbo1CheckMoisture.Enabled = False
        End If
    End Sub

    Private Sub cbo1CheckMoisture_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbo1CheckMoisture.SelectedIndexChanged
        iCheckMoistureVal = CInt(cbo1CheckMoisture.Text)
    End Sub

    Private Sub mtxt1Zone1RunTime_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles mtxt1Zone1RunTime.TextChanged
        Update1EndTime()
    End Sub
    Private Sub mtxt1Zone2RunTime_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles mtxt1Zone2RunTime.TextChanged
        Update1EndTime()
    End Sub
    Private Sub mtxt1Zone3RunTime_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles mtxt1Zone3RunTime.TextChanged
        Update1EndTime()
    End Sub
    Private Sub mtxt1Zone4RunTime_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles mtxt1Zone4RunTime.TextChanged
        Update1EndTime()
    End Sub
    Private Sub mtxt1Zone5RunTime_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles mtxt1Zone5RunTime.TextChanged
        Update1EndTime()
    End Sub
    Private Sub mtxt1Zone6RunTime_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles mtxt1Zone6RunTime.TextChanged
        Update1EndTime()
    End Sub
    Private Sub mtxt1Zone7RunTime_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles mtxt1Zone7RunTime.TextChanged
        Update1EndTime()
    End Sub
    Private Sub mtxt1Zone8RunTime_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles mtxt1Zone8RunTime.TextChanged
        Update1EndTime()
    End Sub
    Private Sub mtxt1Zone9RunTime_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles mtxt1Zone9RunTime.TextChanged
        Update1EndTime()
    End Sub
    Private Sub mtxt1Zone10RunTime_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles mtxt1Zone10RunTime.TextChanged
        Update1EndTime()
    End Sub
    Private Sub mtxt1Zone11RunTime_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles mtxt1Zone11RunTime.TextChanged
        Update1EndTime()
    End Sub
    Private Sub mtxt1Zone12RunTime_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles mtxt1Zone12RunTime.TextChanged
        Update1EndTime()
    End Sub
    Private Sub mtxt1Zone13RunTime_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles mtxt1Zone13RunTime.TextChanged
        Update1EndTime()
    End Sub


    Private Sub ckb1Zone1Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb1Zone1Active.CheckedChanged
        Update1EndTime()
    End Sub
    Private Sub ckb1Zone2Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb1Zone2Active.CheckedChanged
        Update1EndTime()
    End Sub
    Private Sub ckb1Zone3Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb1Zone3Active.CheckedChanged
        Update1EndTime()
    End Sub
    Private Sub ckb1Zone4Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb1Zone4Active.CheckedChanged
        Update1EndTime()
    End Sub
    Private Sub ckb1Zone5Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb1Zone5Active.CheckedChanged
        Update1EndTime()
    End Sub
    Private Sub ckb1Zone6Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb1Zone6Active.CheckedChanged
        Update1EndTime()
    End Sub
    Private Sub ckb1Zone7Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb1Zone7Active.CheckedChanged
        Update1EndTime()
    End Sub
    Private Sub ckb1Zone8Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb1Zone8Active.CheckedChanged
        Update1EndTime()
    End Sub
    Private Sub ckb1Zone9Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb1Zone9Active.CheckedChanged
        Update1EndTime()
    End Sub
    Private Sub ckb1Zone10Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb1Zone10Active.CheckedChanged
        Update1EndTime()
    End Sub
    Private Sub ckb1Zone11Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb1Zone11Active.CheckedChanged
        Update1EndTime()
    End Sub
    Private Sub ckb1Zone12Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb1Zone12Active.CheckedChanged
        Update1EndTime()
    End Sub
    Private Sub ckb1Zone13Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb1Zone13Active.CheckedChanged
        Update1EndTime()
    End Sub

    Private Sub mtxt1StartHour_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles mtxt1StartHour.TextChanged
        Update1EndTime()
    End Sub
    Private Sub mtxt1StartMinute_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles mtxt1StartMinute.TextChanged
        Update1EndTime()
    End Sub

    Private Sub cb1Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cb1Active.CheckedChanged
        If cb1Active.Checked Then
            cb2Active.Checked = False
            cb3Active.Checked = False
            cb4Active.Checked = False

            ValveTimeArray(1) = mtxt1Zone1RunTime
            ValveActiveArray(1) = ckb1Zone1Active
            ValveTimeArray(2) = mtxt1Zone2RunTime
            ValveActiveArray(2) = ckb1Zone2Active
            ValveTimeArray(3) = mtxt1Zone3RunTime
            ValveActiveArray(3) = ckb1Zone3Active
            ValveTimeArray(4) = mtxt1Zone4RunTime
            ValveActiveArray(4) = ckb1Zone4Active
            ValveTimeArray(5) = mtxt1Zone5RunTime
            ValveActiveArray(5) = ckb1Zone5Active
            ValveTimeArray(6) = mtxt1Zone6RunTime
            ValveActiveArray(6) = ckb1Zone6Active
            ValveTimeArray(7) = mtxt1Zone7RunTime
            ValveActiveArray(7) = ckb1Zone7Active
            ValveTimeArray(8) = mtxt1Zone8RunTime
            ValveActiveArray(8) = ckb1Zone8Active
            ValveTimeArray(9) = mtxt1Zone9RunTime
            ValveActiveArray(9) = ckb1Zone9Active
            ValveTimeArray(10) = mtxt1Zone10RunTime
            ValveActiveArray(10) = ckb1Zone10Active
            ValveTimeArray(11) = mtxt1Zone11RunTime
            ValveActiveArray(11) = ckb1Zone11Active
            ValveTimeArray(12) = mtxt1Zone12RunTime
            ValveActiveArray(12) = ckb1Zone12Active
            ValveTimeArray(13) = mtxt1Zone13RunTime
            ValveActiveArray(13) = ckb1Zone13Active

            CheckMoisture = cb1CheckMoisture
            iCheckMoistureVal = CInt(cbo1CheckMoisture.Text)
        End If
    End Sub

    Private Sub btn1RunNow_Click(sender As Object, e As EventArgs) Handles btn1RunNow.Click
        mtxt1StartHour.Text = Hour(Now).ToString
        If Minute(Now) = 59 Then
            mtxt1StartMinute.Text = "00"
            mtxt1StartHour.Text = Hour(DateAdd(DateInterval.Hour, 1, Now)).ToString
        ElseIf Minute(Now) + 1 < 10 Then
            mtxt1StartMinute.Text = "0" & Minute(DateAdd(DateInterval.Minute, 1, Now)).ToString
        Else
            mtxt1StartMinute.Text = Minute(DateAdd(DateInterval.Minute, 1, Now)).ToString
        End If

        IrrigationControl.CheckSoilMoisture()  'Also triggers ExecutionPlanUpdate
        IrrigationControl.Override_ExecutionPlan_Run()
        Me.Hide()
    End Sub

#End Region

#Region "Plan 2"
    Sub Update2EndTime()
        Dim dtRunTime As Date
        Dim iRunTime As Double = 0
        Dim iCount As Integer = 0

        Try
            If mtxt2StartHour.Text = "" Or mtxt2StartMinute.Text = "" Then
                dtRunTime = "00:00"
            Else
                dtRunTime = CDate(mtxt2StartHour.Text & ":" & mtxt2StartMinute.Text)
            End If
        Catch ex As Exception
            dtRunTime = "00:00"
        End Try


        If ckb2Zone1Active.Checked Then
            If mtxt2Zone1RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt2Zone1RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb2Zone2Active.Checked Then
            If mtxt2Zone2RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt2Zone2RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb2Zone3Active.Checked Then
            If mtxt2Zone3RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt2Zone3RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb2Zone4Active.Checked Then
            If mtxt2Zone4RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt2Zone4RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb2Zone5Active.Checked Then
            If mtxt2Zone5RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt2Zone5RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb2Zone6Active.Checked Then
            If mtxt2Zone6RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt2Zone6RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb2Zone7Active.Checked Then
            If mtxt2Zone7RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt2Zone7RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb2Zone8Active.Checked Then
            If mtxt2Zone8RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt2Zone8RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb2Zone9Active.Checked Then
            If mtxt2Zone9RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt2Zone9RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb2Zone10Active.Checked Then
            If mtxt2Zone10RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt2Zone10RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb2Zone11Active.Checked Then
            If mtxt2Zone11RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt2Zone11RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb2Zone12Active.Checked Then
            If mtxt2Zone12RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt2Zone12RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb2Zone13Active.Checked Then
            If mtxt2Zone13RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt2Zone13RunTime.Text)
                iCount = iCount + 1
            End If
        End If

        If iRunTime > 0 Then
            iRunTime = iRunTime + iCount  'One minute after each zone
        Else
            iRunTime = 0
        End If

        dtRunTime = dtRunTime.AddMinutes(iRunTime)
        If DatePart(DateInterval.Hour, dtRunTime) >= 10 Then
            lbl2EndTime.Text = DatePart(DateInterval.Hour, dtRunTime)

        ElseIf DatePart(DateInterval.Hour, dtRunTime) < 10 And DatePart(DateInterval.Hour, dtRunTime) > 0 Then
            lbl2EndTime.Text = "0" & DatePart(DateInterval.Hour, dtRunTime)
        Else
            lbl2EndTime.Text = "00"
        End If

        If DatePart(DateInterval.Minute, dtRunTime) >= 10 Then
            lbl2EndTime.Text = lbl2EndTime.Text & ":" & DatePart(DateInterval.Minute, dtRunTime)

        ElseIf DatePart(DateInterval.Minute, dtRunTime) < 10 And DatePart(DateInterval.Minute, dtRunTime) > 0 Then
            lbl2EndTime.Text = lbl2EndTime.Text & ":0" & DatePart(DateInterval.Minute, dtRunTime)
        Else
            lbl2EndTime.Text = lbl2EndTime.Text & ":00"
        End If

        If cb2Active.Checked Then
            IrrigationControl.lblStartTime.Text = mtxt2StartHour.Text & ":" & mtxt2StartMinute.Text
            IrrigationControl.lblEndTime.Text = lbl2EndTime.Text
        End If
    End Sub

    Private Sub cb2CheckMoisture_CheckedChanged(sender As Object, e As EventArgs) Handles cb2CheckMoisture.CheckedChanged
        If cb2CheckMoisture.Checked Then
            cbo2CheckMoisture.Enabled = True
        Else
            cbo2CheckMoisture.Enabled = False
        End If
    End Sub

    Private Sub cbo2CheckMoisture_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbo2CheckMoisture.SelectedIndexChanged
        iCheckMoistureVal = CInt(cbo2CheckMoisture.Text)
    End Sub

    Private Sub mtxt2StartHour_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt2StartHour.TextChanged
        Update2EndTime()
    End Sub

    Private Sub mtxt2StartMinute_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt2StartMinute.TextChanged
        Update2EndTime()
    End Sub

    Private Sub mtxt2Zone1RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt2Zone1RunTime.TextChanged
        Update2EndTime()
    End Sub

    Private Sub mtxt2Zone2RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt2Zone2RunTime.TextChanged
        Update2EndTime()
    End Sub

    Private Sub mtxt2Zone3RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt2Zone3RunTime.TextChanged
        Update2EndTime()
    End Sub

    Private Sub mtxt2Zone4RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt2Zone4RunTime.TextChanged
        Update2EndTime()
    End Sub

    Private Sub mtxt2Zone5RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt2Zone5RunTime.TextChanged
        Update2EndTime()
    End Sub

    Private Sub mtxt2Zone6RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt2Zone6RunTime.TextChanged
        Update2EndTime()
    End Sub

    Private Sub mtxt2Zone7RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt2Zone7RunTime.TextChanged
        Update2EndTime()
    End Sub

    Private Sub mtxt2Zone8RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt2Zone8RunTime.TextChanged
        Update2EndTime()
    End Sub

    Private Sub mtxt2Zone9RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt2Zone9RunTime.TextChanged
        Update2EndTime()
    End Sub

    Private Sub mtxt2Zone10RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt2Zone10RunTime.TextChanged
        Update2EndTime()
    End Sub

    Private Sub mtxt2Zone11RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt2Zone11RunTime.TextChanged
        Update2EndTime()
    End Sub

    Private Sub mtxt2Zone12RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt2Zone12RunTime.TextChanged
        Update2EndTime()
    End Sub

    Private Sub mtxt2Zone13RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt2Zone13RunTime.TextChanged
        Update2EndTime()
    End Sub


    Private Sub ckb2Zone1Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb2Zone1Active.CheckedChanged
        Update2EndTime()
    End Sub

    Private Sub ckb2Zone2Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb2Zone2Active.CheckedChanged
        Update2EndTime()
    End Sub

    Private Sub ckb2Zone3Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb2Zone3Active.CheckedChanged
        Update2EndTime()
    End Sub

    Private Sub ckb2Zone4Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb2Zone4Active.CheckedChanged
        Update2EndTime()
    End Sub

    Private Sub ckb2Zone5Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb2Zone5Active.CheckedChanged
        Update2EndTime()
    End Sub

    Private Sub ckb2Zone6Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb2Zone6Active.CheckedChanged
        Update2EndTime()
    End Sub

    Private Sub ckb2Zone7Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb2Zone7Active.CheckedChanged
        Update2EndTime()
    End Sub

    Private Sub ckb2Zone8Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb2Zone8Active.CheckedChanged
        Update2EndTime()
    End Sub

    Private Sub ckb2Zone9Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb2Zone9Active.CheckedChanged
        Update2EndTime()
    End Sub

    Private Sub ckb2Zone10Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb2Zone10Active.CheckedChanged
        Update2EndTime()
    End Sub

    Private Sub ckb2Zone11Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb2Zone11Active.CheckedChanged
        Update2EndTime()
    End Sub

    Private Sub ckb2Zone12Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb2Zone12Active.CheckedChanged
        Update2EndTime()
    End Sub

    Private Sub ckb2Zone13Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb2Zone13Active.CheckedChanged
        Update2EndTime()
    End Sub

    Private Sub cb2Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cb2Active.CheckedChanged
        If cb2Active.Checked Then
            cb1Active.Checked = False
            cb3Active.Checked = False
            cb4Active.Checked = False

            ValveTimeArray(1) = mtxt2Zone1RunTime
            ValveActiveArray(1) = ckb2Zone1Active
            ValveTimeArray(2) = mtxt2Zone2RunTime
            ValveActiveArray(2) = ckb2Zone2Active
            ValveTimeArray(3) = mtxt2Zone3RunTime
            ValveActiveArray(3) = ckb2Zone3Active
            ValveTimeArray(4) = mtxt2Zone4RunTime
            ValveActiveArray(4) = ckb2Zone4Active
            ValveTimeArray(5) = mtxt2Zone5RunTime
            ValveActiveArray(5) = ckb2Zone5Active
            ValveTimeArray(6) = mtxt2Zone6RunTime
            ValveActiveArray(6) = ckb2Zone6Active
            ValveTimeArray(7) = mtxt2Zone7RunTime
            ValveActiveArray(7) = ckb2Zone7Active
            ValveTimeArray(8) = mtxt2Zone8RunTime
            ValveActiveArray(8) = ckb2Zone8Active
            ValveTimeArray(9) = mtxt2Zone9RunTime
            ValveActiveArray(9) = ckb2Zone9Active
            ValveTimeArray(10) = mtxt2Zone10RunTime
            ValveActiveArray(10) = ckb2Zone10Active
            ValveTimeArray(11) = mtxt2Zone11RunTime
            ValveActiveArray(11) = ckb2Zone11Active
            ValveTimeArray(12) = mtxt2Zone12RunTime
            ValveActiveArray(12) = ckb2Zone12Active
            ValveTimeArray(13) = mtxt2Zone13RunTime
            ValveActiveArray(13) = ckb2Zone13Active

            CheckMoisture = cb2CheckMoisture
            iCheckMoistureVal = CInt(cbo2CheckMoisture.Text)
        End If
    End Sub


    Private Sub btn2RunNow_Click(sender As Object, e As EventArgs) Handles btn2RunNow.Click
        mtxt2StartHour.Text = Hour(Now).ToString
        If Minute(Now) = 59 Then
            mtxt2StartMinute.Text = "00"
            mtxt2StartHour.Text = Hour(DateAdd(DateInterval.Hour, 1, Now)).ToString
        ElseIf Minute(Now) + 1 < 10 Then
            mtxt2StartMinute.Text = "0" & Minute(DateAdd(DateInterval.Minute, 1, Now)).ToString
        Else
            mtxt2StartMinute.Text = Minute(DateAdd(DateInterval.Minute, 1, Now)).ToString
        End If

        IrrigationControl.CheckSoilMoisture()  'Also triggers ExecutionPlanUpdate
        IrrigationControl.Override_ExecutionPlan_Run()
        Me.Hide()
    End Sub

#End Region

#Region "Plan 3"
    Sub Update3EndTime()
        Dim dtRunTime As Date
        Dim iRunTime As Double = 0
        Dim iCount As Integer = 0

        Try
            If mtxt3StartHour.Text = "" Or mtxt3StartMinute.Text = "" Then
                dtRunTime = "00:00"
            Else
                dtRunTime = CDate(mtxt3StartHour.Text & ":" & mtxt3StartMinute.Text)
            End If
        Catch ex As Exception
            dtRunTime = "00:00"
        End Try


        If ckb3Zone1Active.Checked Then
            If mtxt3Zone1RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt3Zone1RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb3Zone2Active.Checked Then
            If mtxt3Zone2RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt3Zone2RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb3Zone3Active.Checked Then
            If mtxt3Zone3RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt3Zone3RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb3Zone4Active.Checked Then
            If mtxt3Zone4RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt3Zone4RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb3Zone5Active.Checked Then
            If mtxt3Zone5RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt3Zone5RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb3Zone6Active.Checked Then
            If mtxt3Zone6RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt3Zone6RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb3Zone7Active.Checked Then
            If mtxt3Zone7RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt3Zone7RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb3Zone8Active.Checked Then
            If mtxt3Zone8RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt3Zone8RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb3Zone9Active.Checked Then
            If mtxt3Zone9RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt3Zone9RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb3Zone10Active.Checked Then
            If mtxt3Zone10RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt3Zone10RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb3Zone11Active.Checked Then
            If mtxt3Zone11RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt3Zone11RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb3Zone12Active.Checked Then
            If mtxt3Zone12RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt3Zone12RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb3Zone13Active.Checked Then
            If mtxt3Zone13RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt3Zone13RunTime.Text)
                iCount = iCount + 1
            End If
        End If

        If iRunTime > 0 Then
            iRunTime = iRunTime + iCount  'One minute after each zone
        Else
            iRunTime = 0
        End If

        dtRunTime = dtRunTime.AddMinutes(iRunTime)
        If DatePart(DateInterval.Hour, dtRunTime) >= 10 Then
            lbl3EndTime.Text = DatePart(DateInterval.Hour, dtRunTime)

        ElseIf DatePart(DateInterval.Hour, dtRunTime) < 10 And DatePart(DateInterval.Hour, dtRunTime) > 0 Then
            lbl3EndTime.Text = "0" & DatePart(DateInterval.Hour, dtRunTime)
        Else
            lbl3EndTime.Text = "00"
        End If

        If DatePart(DateInterval.Minute, dtRunTime) >= 10 Then
            lbl3EndTime.Text = lbl3EndTime.Text & ":" & DatePart(DateInterval.Minute, dtRunTime)

        ElseIf DatePart(DateInterval.Minute, dtRunTime) < 10 And DatePart(DateInterval.Minute, dtRunTime) > 0 Then
            lbl3EndTime.Text = lbl3EndTime.Text & ":0" & DatePart(DateInterval.Minute, dtRunTime)
        Else
            lbl3EndTime.Text = lbl3EndTime.Text & ":00"
        End If
        If cb3Active.Checked Then
            IrrigationControl.lblStartTime.Text = mtxt3StartHour.Text & ":" & mtxt3StartMinute.Text
            IrrigationControl.lblEndTime.Text = lbl3EndTime.Text
        End If
    End Sub

    Private Sub cb3CheckMoisture_CheckedChanged(sender As Object, e As EventArgs) Handles cb3CheckMoisture.CheckedChanged
        If cb3CheckMoisture.Checked Then
            cbo3CheckMoisture.Enabled = True
        Else
            cbo3CheckMoisture.Enabled = False
        End If
    End Sub

    Private Sub cbo3CheckMoisture_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbo3CheckMoisture.SelectedIndexChanged
        iCheckMoistureVal = CInt(cbo3CheckMoisture.Text)
    End Sub

    Private Sub mtxt3StartHour_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt3StartHour.TextChanged
        Update3EndTime()
    End Sub

    Private Sub mtxt3StartMinute_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt3StartMinute.TextChanged
        Update3EndTime()
    End Sub

    Private Sub mtxt3Zone1RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt3Zone1RunTime.TextChanged
        Update3EndTime()
    End Sub

    Private Sub mtxt3Zone2RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt3Zone2RunTime.TextChanged
        Update3EndTime()
    End Sub

    Private Sub mtxt3Zone3RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt3Zone3RunTime.TextChanged
        Update3EndTime()
    End Sub

    Private Sub mtxt3Zone4RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt3Zone4RunTime.TextChanged
        Update3EndTime()
    End Sub

    Private Sub mtxt3Zone5RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt3Zone5RunTime.TextChanged
        Update3EndTime()
    End Sub

    Private Sub mtxt3Zone6RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt3Zone6RunTime.TextChanged
        Update3EndTime()
    End Sub

    Private Sub mtxt3Zone7RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt3Zone7RunTime.TextChanged
        Update3EndTime()
    End Sub

    Private Sub mtxt3Zone8RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt3Zone8RunTime.TextChanged
        Update3EndTime()
    End Sub

    Private Sub mtxt3Zone9RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt3Zone9RunTime.TextChanged
        Update3EndTime()
    End Sub

    Private Sub mtxt3Zone10RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt3Zone10RunTime.TextChanged
        Update3EndTime()
    End Sub

    Private Sub mtxt3Zone11RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt3Zone11RunTime.TextChanged
        Update3EndTime()
    End Sub

    Private Sub mtxt3Zone12RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt3Zone12RunTime.TextChanged
        Update3EndTime()
    End Sub

    Private Sub mtxt3Zone13RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt3Zone13RunTime.TextChanged
        Update3EndTime()
    End Sub


    Private Sub ckb3Zone1Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb3Zone1Active.CheckedChanged
        Update3EndTime()
    End Sub

    Private Sub ckb3Zone2Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb3Zone2Active.CheckedChanged
        Update3EndTime()
    End Sub

    Private Sub ckb3Zone3Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb3Zone3Active.CheckedChanged
        Update3EndTime()
    End Sub

    Private Sub ckb3Zone4Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb3Zone4Active.CheckedChanged
        Update3EndTime()
    End Sub

    Private Sub ckb3Zone5Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb3Zone5Active.CheckedChanged
        Update3EndTime()
    End Sub

    Private Sub ckb3Zone6Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb3Zone6Active.CheckedChanged
        Update3EndTime()
    End Sub

    Private Sub ckb3Zone7Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb3Zone7Active.CheckedChanged
        Update3EndTime()
    End Sub

    Private Sub ckb3Zone8Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb3Zone8Active.CheckedChanged
        Update3EndTime()
    End Sub

    Private Sub ckb3Zone9Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb3Zone9Active.CheckedChanged
        Update3EndTime()
    End Sub

    Private Sub ckb3Zone10Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb3Zone10Active.CheckedChanged
        Update3EndTime()
    End Sub

    Private Sub ckb3Zone11Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb3Zone11Active.CheckedChanged
        Update3EndTime()
    End Sub

    Private Sub ckb3Zone12Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb3Zone12Active.CheckedChanged
        Update3EndTime()
    End Sub

    Private Sub ckb3Zone13Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb3Zone13Active.CheckedChanged
        Update3EndTime()
    End Sub

    Private Sub cb3Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cb3Active.CheckedChanged
        If cb3Active.Checked Then
            cb1Active.Checked = False
            cb2Active.Checked = False
            cb4Active.Checked = False

            ValveTimeArray(1) = mtxt3Zone1RunTime
            ValveActiveArray(1) = ckb3Zone1Active
            ValveTimeArray(2) = mtxt3Zone2RunTime
            ValveActiveArray(2) = ckb3Zone2Active
            ValveTimeArray(3) = mtxt3Zone3RunTime
            ValveActiveArray(3) = ckb3Zone3Active
            ValveTimeArray(4) = mtxt3Zone4RunTime
            ValveActiveArray(4) = ckb3Zone4Active
            ValveTimeArray(5) = mtxt3Zone5RunTime
            ValveActiveArray(5) = ckb3Zone5Active
            ValveTimeArray(6) = mtxt3Zone6RunTime
            ValveActiveArray(6) = ckb3Zone6Active
            ValveTimeArray(7) = mtxt3Zone7RunTime
            ValveActiveArray(7) = ckb3Zone7Active
            ValveTimeArray(8) = mtxt3Zone8RunTime
            ValveActiveArray(8) = ckb3Zone8Active
            ValveTimeArray(9) = mtxt3Zone9RunTime
            ValveActiveArray(9) = ckb3Zone9Active
            ValveTimeArray(10) = mtxt3Zone10RunTime
            ValveActiveArray(10) = ckb3Zone10Active
            ValveTimeArray(11) = mtxt3Zone11RunTime
            ValveActiveArray(11) = ckb3Zone11Active
            ValveTimeArray(12) = mtxt3Zone12RunTime
            ValveActiveArray(12) = ckb3Zone12Active
            ValveTimeArray(13) = mtxt3Zone13RunTime
            ValveActiveArray(13) = ckb3Zone13Active

            CheckMoisture = cb3CheckMoisture
            iCheckMoistureVal = CInt(cbo3CheckMoisture.Text)
        End If
    End Sub


    Private Sub btn3RunNow_Click(sender As Object, e As EventArgs) Handles btn3RunNow.Click
        mtxt3StartHour.Text = Hour(Now).ToString
        If Minute(Now) = 59 Then
            mtxt3StartMinute.Text = "00"
            mtxt3StartHour.Text = Hour(DateAdd(DateInterval.Hour, 1, Now)).ToString
        ElseIf Minute(Now) + 1 < 10 Then
            mtxt3StartMinute.Text = "0" & Minute(DateAdd(DateInterval.Minute, 1, Now)).ToString
        Else
            mtxt3StartMinute.Text = Minute(DateAdd(DateInterval.Minute, 1, Now)).ToString
        End If

        IrrigationControl.CheckSoilMoisture()  'Also triggers ExecutionPlanUpdate
        IrrigationControl.Override_ExecutionPlan_Run()
        Me.Hide()
    End Sub

#End Region

#Region "Plan 4"
    Sub Update4EndTime()
        Dim dtRunTime As Date
        Dim iRunTime As Double = 0
        Dim iCount As Integer = 0

        Try
            If mtxt4StartHour.Text = "" Or mtxt4StartMinute.Text = "" Then
                dtRunTime = "00:00"
            Else
                dtRunTime = CDate(mtxt4StartHour.Text & ":" & mtxt4StartMinute.Text)
            End If
        Catch ex As Exception
            dtRunTime = "00:00"
        End Try


        If ckb4Zone1Active.Checked Then
            If mtxt4Zone1RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt4Zone1RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb4Zone2Active.Checked Then
            If mtxt4Zone2RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt4Zone2RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb4Zone3Active.Checked Then
            If mtxt4Zone3RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt4Zone3RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb4Zone4Active.Checked Then
            If mtxt4Zone4RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt4Zone4RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb4Zone5Active.Checked Then
            If mtxt4Zone5RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt4Zone5RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb4Zone6Active.Checked Then
            If mtxt4Zone6RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt4Zone6RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb4Zone7Active.Checked Then
            If mtxt4Zone7RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt4Zone7RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb4Zone8Active.Checked Then
            If mtxt4Zone8RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt4Zone8RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb4Zone9Active.Checked Then
            If mtxt4Zone9RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt4Zone9RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb4Zone10Active.Checked Then
            If mtxt4Zone10RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt4Zone10RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb4Zone11Active.Checked Then
            If mtxt4Zone11RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt4Zone11RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb4Zone12Active.Checked Then
            If mtxt4Zone12RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt4Zone12RunTime.Text)
                iCount = iCount + 1
            End If
        End If
        If ckb4Zone13Active.Checked Then
            If mtxt4Zone13RunTime.Text <> "" Then
                iRunTime = iRunTime + CInt(mtxt4Zone13RunTime.Text)
                iCount = iCount + 1
            End If
        End If

        If iRunTime > 0 Then
            iRunTime = iRunTime + iCount  'One minute after each zone
        Else
            iRunTime = 0
        End If

        dtRunTime = dtRunTime.AddMinutes(iRunTime)
        If DatePart(DateInterval.Hour, dtRunTime) >= 10 Then
            lbl4EndTime.Text = DatePart(DateInterval.Hour, dtRunTime)

        ElseIf DatePart(DateInterval.Hour, dtRunTime) < 10 And DatePart(DateInterval.Hour, dtRunTime) > 0 Then
            lbl4EndTime.Text = "0" & DatePart(DateInterval.Hour, dtRunTime)
        Else
            lbl4EndTime.Text = "00"
        End If

        If DatePart(DateInterval.Minute, dtRunTime) >= 10 Then
            lbl4EndTime.Text = lbl4EndTime.Text & ":" & DatePart(DateInterval.Minute, dtRunTime)

        ElseIf DatePart(DateInterval.Minute, dtRunTime) < 10 And DatePart(DateInterval.Minute, dtRunTime) > 0 Then
            lbl4EndTime.Text = lbl4EndTime.Text & ":0" & DatePart(DateInterval.Minute, dtRunTime)
        Else
            lbl4EndTime.Text = lbl4EndTime.Text & ":00"
        End If

        If cb4Active.Checked Then
            IrrigationControl.lblStartTime.Text = mtxt4StartHour.Text & ":" & mtxt4StartMinute.Text
            IrrigationControl.lblEndTime.Text = lbl4EndTime.Text
        End If
    End Sub

    Private Sub cb4CheckMoisture_CheckedChanged(sender As Object, e As EventArgs) Handles cb4CheckMoisture.CheckedChanged
        If cb4CheckMoisture.Checked Then
            cbo4CheckMoisture.Enabled = True
        Else
            cbo4CheckMoisture.Enabled = False
        End If
    End Sub

    Private Sub cbo4CheckMoisture_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbo4CheckMoisture.SelectedIndexChanged
        iCheckMoistureVal = CInt(cbo4CheckMoisture.Text)
    End Sub

    Private Sub mtxt4StartHour_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt4StartHour.TextChanged
        Update4EndTime()
    End Sub

    Private Sub mtxt4StartMinute_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt4StartMinute.TextChanged
        Update4EndTime()
    End Sub

    Private Sub mtxt4Zone1RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt4Zone1RunTime.TextChanged
        Update4EndTime()
    End Sub

    Private Sub mtxt4Zone2RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt4Zone2RunTime.TextChanged
        Update4EndTime()
    End Sub

    Private Sub mtxt4Zone3RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt4Zone3RunTime.TextChanged
        Update4EndTime()
    End Sub

    Private Sub mtxt4Zone4RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt4Zone4RunTime.TextChanged
        Update4EndTime()
    End Sub

    Private Sub mtxt4Zone5RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt4Zone5RunTime.TextChanged
        Update4EndTime()
    End Sub

    Private Sub mtxt4Zone6RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt4Zone6RunTime.TextChanged
        Update4EndTime()
    End Sub

    Private Sub mtxt4Zone7RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt4Zone7RunTime.TextChanged
        Update4EndTime()
    End Sub

    Private Sub mtxt4Zone8RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt4Zone8RunTime.TextChanged
        Update4EndTime()
    End Sub

    Private Sub mtxt4Zone9RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt4Zone9RunTime.TextChanged
        Update4EndTime()
    End Sub

    Private Sub mtxt4Zone10RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt4Zone10RunTime.TextChanged
        Update4EndTime()
    End Sub

    Private Sub mtxt4Zone11RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt4Zone11RunTime.TextChanged
        Update4EndTime()
    End Sub

    Private Sub mtxt4Zone12RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt4Zone12RunTime.TextChanged
        Update4EndTime()
    End Sub

    Private Sub mtxt4Zone13RunTime_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mtxt4Zone13RunTime.TextChanged
        Update4EndTime()
    End Sub



    Private Sub ckb4Zone1Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb4Zone1Active.CheckedChanged
        Update4EndTime()
    End Sub

    Private Sub ckb4Zone2Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb4Zone2Active.CheckedChanged
        Update4EndTime()
    End Sub

    Private Sub ckb4Zone3Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb4Zone3Active.CheckedChanged
        Update4EndTime()
    End Sub

    Private Sub ckb4Zone4Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb4Zone4Active.CheckedChanged
        Update4EndTime()
    End Sub

    Private Sub ckb4Zone5Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb4Zone5Active.CheckedChanged
        Update4EndTime()
    End Sub

    Private Sub ckb4Zone6Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb4Zone6Active.CheckedChanged
        Update4EndTime()
    End Sub

    Private Sub ckb4Zone7Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb4Zone7Active.CheckedChanged
        Update4EndTime()
    End Sub

    Private Sub ckb4Zone8Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb4Zone8Active.CheckedChanged
        Update4EndTime()
    End Sub

    Private Sub ckb4Zone9Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb4Zone9Active.CheckedChanged
        Update4EndTime()
    End Sub

    Private Sub ckb4Zone10Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb4Zone10Active.CheckedChanged
        Update4EndTime()
    End Sub

    Private Sub ckb4Zone11Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb4Zone11Active.CheckedChanged
        Update4EndTime()
    End Sub

    Private Sub ckb4Zone12Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb4Zone12Active.CheckedChanged
        Update4EndTime()
    End Sub

    Private Sub ckb4Zone13Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ckb4Zone13Active.CheckedChanged
        Update4EndTime()
    End Sub

    Private Sub cb4Active_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cb4Active.CheckedChanged
        If cb4Active.Checked Then
            cb1Active.Checked = False
            cb2Active.Checked = False
            cb3Active.Checked = False

            ValveTimeArray(1) = mtxt4Zone1RunTime
            ValveActiveArray(1) = ckb4Zone1Active
            ValveTimeArray(2) = mtxt4Zone2RunTime
            ValveActiveArray(2) = ckb4Zone2Active
            ValveTimeArray(3) = mtxt4Zone3RunTime
            ValveActiveArray(3) = ckb4Zone3Active
            ValveTimeArray(4) = mtxt4Zone4RunTime
            ValveActiveArray(4) = ckb4Zone4Active
            ValveTimeArray(5) = mtxt4Zone5RunTime
            ValveActiveArray(5) = ckb4Zone5Active
            ValveTimeArray(6) = mtxt4Zone6RunTime
            ValveActiveArray(6) = ckb4Zone6Active
            ValveTimeArray(7) = mtxt4Zone7RunTime
            ValveActiveArray(7) = ckb4Zone7Active
            ValveTimeArray(8) = mtxt4Zone8RunTime
            ValveActiveArray(8) = ckb4Zone8Active
            ValveTimeArray(9) = mtxt4Zone9RunTime
            ValveActiveArray(9) = ckb4Zone9Active
            ValveTimeArray(10) = mtxt4Zone10RunTime
            ValveActiveArray(10) = ckb4Zone10Active
            ValveTimeArray(11) = mtxt4Zone11RunTime
            ValveActiveArray(11) = ckb4Zone11Active
            ValveTimeArray(12) = mtxt4Zone12RunTime
            ValveActiveArray(12) = ckb4Zone12Active
            ValveTimeArray(13) = mtxt4Zone13RunTime
            ValveActiveArray(13) = ckb4Zone13Active

            CheckMoisture = cb4CheckMoisture
            iCheckMoistureVal = CInt(cbo4CheckMoisture.Text)
        End If
    End Sub


    Private Sub btn4RunNow_Click(sender As Object, e As EventArgs) Handles btn4RunNow.Click
        mtxt4StartHour.Text = Hour(Now).ToString
        If Minute(Now) = 59 Then
            mtxt4StartMinute.Text = "00"
            mtxt4StartHour.Text = Hour(DateAdd(DateInterval.Hour, 1, Now)).ToString
        ElseIf Minute(Now) + 1 < 10 Then
            mtxt4StartMinute.Text = "0" & Minute(DateAdd(DateInterval.Minute, 1, Now)).ToString
        Else
            mtxt4StartMinute.Text = Minute(DateAdd(DateInterval.Minute, 1, Now)).ToString
        End If

        IrrigationControl.CheckSoilMoisture()  'Also triggers ExecutionPlanUpdate
        IrrigationControl.Override_ExecutionPlan_Run()
        Me.Hide()
    End Sub

#End Region

End Class