Option Strict Off
Option Explicit On

Imports ForecastIO

Public Class IrrigationControl

    Friend WithEvents Event_HistoryTableAdapter As New WatchdogDataSet1TableAdapters.Event_HistoryTableAdapter

    Const MIN_PCT_PRECIP = 60
    Const MIN_TEMP = 40

    Public _SMarduino As Arduino_Net2
    Public WithEvents _VCarduino As Arduino_Net2
    Public tSleep As System.Timers.Timer
    Public SensorValue(7) As Integer
    Dim bWeatherBad, bSoilWet, bExecutionPlan, bOverride_Run, bOverride_Stop As Boolean
    Dim bValveControlActive, bRunning As Boolean
    Dim iZoneTimer, iActiveZone As Integer
    Dim iGallonsUsed, iOldGallonsUsed, iLeakCounter As Integer
    Dim iIrrigationGallons_Start As Integer
    Dim bMasterOn As Boolean

    Private Sub IrrigationControl_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing

        SaveParameters()
        SaveGallonsUsed()
        Event_HistoryTableAdapter1.InsertQuery("9010")
        Irrigation_HistoryTableAdapter.InsertQuery("101")
        _VCarduino.SetDigitalValue(3, 0)  'Turn off Master

    End Sub

    Private Sub IrrigationControl_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Dim port As String
        Dim ports As String() = SerialPort.GetPortNames()
        Dim badChars As Char() = New Char() {"c"}

        Me.Show()
        Event_HistoryTableAdapter1.InsertQuery("9009")
        Irrigation_HistoryTableAdapter.InsertQuery("100")

        For Each port In ports
            ' .NET Framework has a bug where COM ports are
            ' sometimes appended with a 'c' characeter!
            If port.IndexOfAny(badChars) <> -1 Then
                COMControl.cbSM_COMPort.Items.Add(port.Substring(0, port.Length - 1))
                COMControl.cbVC_COMPort.Items.Add(port.Substring(0, port.Length - 1))
            Else
                COMControl.cbSM_COMPort.Items.Add(port)
                COMControl.cbVC_COMPort.Items.Add(port)
            End If
        Next
        If COMControl.cbSM_COMPort.Items.Count = 0 Then
            COMControl.cbSM_COMPort.Text = ""
        Else
            COMControl.cbSM_COMPort.Text = "COM3"
        End If

        If COMControl.cbVC_COMPort.Items.Count = 0 Then
            COMControl.cbVC_COMPort.Text = ""
        Else
            COMControl.cbVC_COMPort.Text = "COM29"
        End If

        'Create Thread-safe timer
        tSleep = New System.Timers.Timer(Int(COMControl.mtxtPollingInterval.Text) * 60 * 1000)
        AddHandler tSleep.Elapsed, AddressOf tSleep_Elapsed

        iActiveZone = 0
        iZoneTimer = 0
        bValveControlActive = True
        bOverride_Run = False
        bOverride_Stop = False
        lblRainToday.Text = "No"

        LoadParameters()

        WaterMeterStartup()

        If StartValveControl() And StartSoilMonitor() Then   'If startups are successful, then proceed
            CheckSoilMoisture()  'Also triggers ExecutionPlanUpdate

            Irrigation_ControlTableAdapter.Reset_New_Requests()
            CheckMonth()
            tDaily.Interval = (((23 - Now.Hour) * 3600000) + ((59 - Now.Minute) * 60000) + ((60 - Now.Second) * 1000))
            tDaily.Start()
            tHourly.Start()
            tCheckDB.Start()
        End If

    End Sub

#Region "Parameters"
    Private Sub LoadParameters()
        Dim xmlData As DataSet = New DataSet()
        Dim strActivePlan As String

        'Load Parameters
        Try
            xmlData.ReadXml("WDIrrigationControlParameter.xml")
            strActivePlan = xmlData.Tables(0).Rows(0).ItemArray(0)
            If CInt(strActivePlan) = 1 Then
                ValveControl.cb1Active.Checked = True
                ValveControl.cb2Active.Checked = False
                ValveControl.cb3Active.Checked = False
                ValveControl.cb4Active.Checked = False
            End If
            If CInt(strActivePlan) = 2 Then
                ValveControl.cb1Active.Checked = False
                ValveControl.cb2Active.Checked = True
                ValveControl.cb3Active.Checked = False
                ValveControl.cb4Active.Checked = False
            End If
            If CInt(strActivePlan) = 3 Then
                ValveControl.cb1Active.Checked = False
                ValveControl.cb2Active.Checked = False
                ValveControl.cb3Active.Checked = True
                ValveControl.cb4Active.Checked = False
            End If
            If CInt(strActivePlan) = 4 Then
                ValveControl.cb1Active.Checked = False
                ValveControl.cb2Active.Checked = False
                ValveControl.cb3Active.Checked = False
                ValveControl.cb4Active.Checked = True
            End If

            COMControl.mtxtPollingInterval.Text = xmlData.Tables(0).Rows(0).ItemArray(1)

            'Plan 1
            ValveControl.mtxt1StartHour.Text = xmlData.Tables(1).Rows(0).ItemArray(0)
            ValveControl.mtxt1StartMinute.Text = xmlData.Tables(1).Rows(0).ItemArray(1)
            If xmlData.Tables(1).Rows(0).ItemArray(2) = "True" Then
                ValveControl.cb1CheckMoisture.Checked = True
            Else
                ValveControl.cb1CheckMoisture.Checked = False
            End If
            ValveControl.cbo1CheckMoisture.Text = xmlData.Tables(1).Rows(0).ItemArray(3)
            ValveControl.mtxt1Zone1RunTime.Text = xmlData.Tables(1).Rows(0).ItemArray(4)
            If xmlData.Tables(1).Rows(0).ItemArray(5) = "True" Then
                ValveControl.ckb1Zone1Active.Checked = True
            Else
                ValveControl.ckb1Zone1Active.Checked = False
            End If
            ValveControl.mtxt1Zone2RunTime.Text = xmlData.Tables(1).Rows(0).ItemArray(6)
            If xmlData.Tables(1).Rows(0).ItemArray(7) = "True" Then
                ValveControl.ckb1Zone2Active.Checked = True
            Else
                ValveControl.ckb1Zone2Active.Checked = False
            End If
            ValveControl.mtxt1Zone3RunTime.Text = xmlData.Tables(1).Rows(0).ItemArray(8)
            If xmlData.Tables(1).Rows(0).ItemArray(9) = "True" Then
                ValveControl.ckb1Zone3Active.Checked = True
            Else
                ValveControl.ckb1Zone3Active.Checked = False
            End If
            ValveControl.mtxt1Zone4RunTime.Text = xmlData.Tables(1).Rows(0).ItemArray(10)
            If xmlData.Tables(1).Rows(0).ItemArray(11) = "True" Then
                ValveControl.ckb1Zone4Active.Checked = True
            Else
                ValveControl.ckb1Zone4Active.Checked = False
            End If
            ValveControl.mtxt1Zone5RunTime.Text = xmlData.Tables(1).Rows(0).ItemArray(12)
            If xmlData.Tables(1).Rows(0).ItemArray(13) = "True" Then
                ValveControl.ckb1Zone5Active.Checked = True
            Else
                ValveControl.ckb1Zone5Active.Checked = False
            End If
            ValveControl.mtxt1Zone6RunTime.Text = xmlData.Tables(1).Rows(0).ItemArray(14)
            If xmlData.Tables(1).Rows(0).ItemArray(15) = "True" Then
                ValveControl.ckb1Zone6Active.Checked = True
            Else
                ValveControl.ckb1Zone6Active.Checked = False
            End If
            ValveControl.mtxt1Zone7RunTime.Text = xmlData.Tables(1).Rows(0).ItemArray(16)
            If xmlData.Tables(1).Rows(0).ItemArray(17) = "True" Then
                ValveControl.ckb1Zone7Active.Checked = True
            Else
                ValveControl.ckb1Zone7Active.Checked = False
            End If
            ValveControl.mtxt1Zone8RunTime.Text = xmlData.Tables(1).Rows(0).ItemArray(18)
            If xmlData.Tables(1).Rows(0).ItemArray(19) = "True" Then
                ValveControl.ckb1Zone8Active.Checked = True
            Else
                ValveControl.ckb1Zone8Active.Checked = False
            End If
            ValveControl.mtxt1Zone9RunTime.Text = xmlData.Tables(1).Rows(0).ItemArray(20)
            If xmlData.Tables(1).Rows(0).ItemArray(21) = "True" Then
                ValveControl.ckb1Zone9Active.Checked = True
            Else
                ValveControl.ckb1Zone9Active.Checked = False
            End If
            ValveControl.mtxt1Zone10RunTime.Text = xmlData.Tables(1).Rows(0).ItemArray(22)
            If xmlData.Tables(1).Rows(0).ItemArray(23) = "True" Then
                ValveControl.ckb1Zone10Active.Checked = True
            Else
                ValveControl.ckb1Zone10Active.Checked = False
            End If
            ValveControl.mtxt1Zone11RunTime.Text = xmlData.Tables(1).Rows(0).ItemArray(24)
            If xmlData.Tables(1).Rows(0).ItemArray(25) = "True" Then
                ValveControl.ckb1Zone11Active.Checked = True
            Else
                ValveControl.ckb1Zone11Active.Checked = False
            End If
            ValveControl.mtxt1Zone12RunTime.Text = xmlData.Tables(1).Rows(0).ItemArray(26)
            If xmlData.Tables(1).Rows(0).ItemArray(27) = "True" Then
                ValveControl.ckb1Zone12Active.Checked = True
            Else
                ValveControl.ckb1Zone12Active.Checked = False
            End If
            ValveControl.mtxt1Zone13RunTime.Text = xmlData.Tables(1).Rows(0).ItemArray(28)
            If xmlData.Tables(1).Rows(0).ItemArray(29) = "True" Then
                ValveControl.ckb1Zone13Active.Checked = True
            Else
                ValveControl.ckb1Zone13Active.Checked = False
            End If

            'Plan 2
            ValveControl.mtxt2StartHour.Text = xmlData.Tables(2).Rows(0).ItemArray(0)
            ValveControl.mtxt2StartMinute.Text = xmlData.Tables(2).Rows(0).ItemArray(1)
            If xmlData.Tables(2).Rows(0).ItemArray(2) = "True" Then
                ValveControl.cb2CheckMoisture.Checked = True
            Else
                ValveControl.cb2CheckMoisture.Checked = False
            End If
            ValveControl.cbo2CheckMoisture.Text = xmlData.Tables(2).Rows(0).ItemArray(3)
            ValveControl.mtxt2Zone1RunTime.Text = xmlData.Tables(2).Rows(0).ItemArray(4)
            If xmlData.Tables(2).Rows(0).ItemArray(5) = "True" Then
                ValveControl.ckb2Zone1Active.Checked = True
            Else
                ValveControl.ckb2Zone1Active.Checked = False
            End If
            ValveControl.mtxt2Zone2RunTime.Text = xmlData.Tables(2).Rows(0).ItemArray(6)
            If xmlData.Tables(2).Rows(0).ItemArray(7) = "True" Then
                ValveControl.ckb2Zone2Active.Checked = True
            Else
                ValveControl.ckb2Zone2Active.Checked = False
            End If
            ValveControl.mtxt2Zone3RunTime.Text = xmlData.Tables(2).Rows(0).ItemArray(8)
            If xmlData.Tables(2).Rows(0).ItemArray(9) = "True" Then
                ValveControl.ckb2Zone3Active.Checked = True
            Else
                ValveControl.ckb2Zone3Active.Checked = False
            End If
            ValveControl.mtxt2Zone4RunTime.Text = xmlData.Tables(2).Rows(0).ItemArray(10)
            If xmlData.Tables(2).Rows(0).ItemArray(11) = "True" Then
                ValveControl.ckb2Zone4Active.Checked = True
            Else
                ValveControl.ckb2Zone4Active.Checked = False
            End If
            ValveControl.mtxt2Zone5RunTime.Text = xmlData.Tables(2).Rows(0).ItemArray(12)
            If xmlData.Tables(2).Rows(0).ItemArray(13) = "True" Then
                ValveControl.ckb2Zone5Active.Checked = True
            Else
                ValveControl.ckb2Zone5Active.Checked = False
            End If
            ValveControl.mtxt2Zone6RunTime.Text = xmlData.Tables(2).Rows(0).ItemArray(14)
            If xmlData.Tables(2).Rows(0).ItemArray(15) = "True" Then
                ValveControl.ckb2Zone6Active.Checked = True
            Else
                ValveControl.ckb2Zone6Active.Checked = False
            End If
            ValveControl.mtxt2Zone7RunTime.Text = xmlData.Tables(2).Rows(0).ItemArray(16)
            If xmlData.Tables(2).Rows(0).ItemArray(17) = "True" Then
                ValveControl.ckb2Zone7Active.Checked = True
            Else
                ValveControl.ckb2Zone7Active.Checked = False
            End If
            ValveControl.mtxt2Zone8RunTime.Text = xmlData.Tables(2).Rows(0).ItemArray(18)
            If xmlData.Tables(2).Rows(0).ItemArray(19) = "True" Then
                ValveControl.ckb2Zone8Active.Checked = True
            Else
                ValveControl.ckb2Zone8Active.Checked = False
            End If
            ValveControl.mtxt2Zone9RunTime.Text = xmlData.Tables(2).Rows(0).ItemArray(20)
            If xmlData.Tables(2).Rows(0).ItemArray(21) = "True" Then
                ValveControl.ckb2Zone9Active.Checked = True
            Else
                ValveControl.ckb2Zone9Active.Checked = False
            End If
            ValveControl.mtxt2Zone10RunTime.Text = xmlData.Tables(2).Rows(0).ItemArray(22)
            If xmlData.Tables(2).Rows(0).ItemArray(23) = "True" Then
                ValveControl.ckb2Zone10Active.Checked = True
            Else
                ValveControl.ckb2Zone10Active.Checked = False
            End If
            ValveControl.mtxt2Zone11RunTime.Text = xmlData.Tables(2).Rows(0).ItemArray(24)
            If xmlData.Tables(2).Rows(0).ItemArray(25) = "True" Then
                ValveControl.ckb2Zone11Active.Checked = True
            Else
                ValveControl.ckb2Zone11Active.Checked = False
            End If
            ValveControl.mtxt2Zone12RunTime.Text = xmlData.Tables(2).Rows(0).ItemArray(26)
            If xmlData.Tables(2).Rows(0).ItemArray(27) = "True" Then
                ValveControl.ckb2Zone12Active.Checked = True
            Else
                ValveControl.ckb2Zone12Active.Checked = False
            End If
            ValveControl.mtxt2Zone13RunTime.Text = xmlData.Tables(2).Rows(0).ItemArray(28)
            If xmlData.Tables(2).Rows(0).ItemArray(29) = "True" Then
                ValveControl.ckb2Zone13Active.Checked = True
            Else
                ValveControl.ckb2Zone13Active.Checked = False
            End If

            'Plan 3
            ValveControl.mtxt3StartHour.Text = xmlData.Tables(3).Rows(0).ItemArray(0)
            ValveControl.mtxt3StartMinute.Text = xmlData.Tables(3).Rows(0).ItemArray(1)
            If xmlData.Tables(3).Rows(0).ItemArray(2) = "True" Then
                ValveControl.cb3CheckMoisture.Checked = True
            Else
                ValveControl.cb3CheckMoisture.Checked = False
            End If
            ValveControl.cbo3CheckMoisture.Text = xmlData.Tables(3).Rows(0).ItemArray(3)
            ValveControl.mtxt3Zone1RunTime.Text = xmlData.Tables(3).Rows(0).ItemArray(4)
            If xmlData.Tables(3).Rows(0).ItemArray(5) = "True" Then
                ValveControl.ckb3Zone1Active.Checked = True
            Else
                ValveControl.ckb3Zone1Active.Checked = False
            End If
            ValveControl.mtxt3Zone2RunTime.Text = xmlData.Tables(3).Rows(0).ItemArray(6)
            If xmlData.Tables(3).Rows(0).ItemArray(7) = "True" Then
                ValveControl.ckb3Zone2Active.Checked = True
            Else
                ValveControl.ckb3Zone2Active.Checked = False
            End If
            ValveControl.mtxt3Zone3RunTime.Text = xmlData.Tables(3).Rows(0).ItemArray(8)
            If xmlData.Tables(3).Rows(0).ItemArray(9) = "True" Then
                ValveControl.ckb3Zone3Active.Checked = True
            Else
                ValveControl.ckb3Zone3Active.Checked = False
            End If
            ValveControl.mtxt3Zone4RunTime.Text = xmlData.Tables(3).Rows(0).ItemArray(10)
            If xmlData.Tables(3).Rows(0).ItemArray(11) = "True" Then
                ValveControl.ckb3Zone4Active.Checked = True
            Else
                ValveControl.ckb3Zone4Active.Checked = False
            End If
            ValveControl.mtxt3Zone5RunTime.Text = xmlData.Tables(3).Rows(0).ItemArray(12)
            If xmlData.Tables(3).Rows(0).ItemArray(13) = "True" Then
                ValveControl.ckb3Zone5Active.Checked = True
            Else
                ValveControl.ckb3Zone5Active.Checked = False
            End If
            ValveControl.mtxt3Zone6RunTime.Text = xmlData.Tables(3).Rows(0).ItemArray(14)
            If xmlData.Tables(3).Rows(0).ItemArray(15) = "True" Then
                ValveControl.ckb3Zone6Active.Checked = True
            Else
                ValveControl.ckb3Zone6Active.Checked = False
            End If
            ValveControl.mtxt3Zone7RunTime.Text = xmlData.Tables(3).Rows(0).ItemArray(16)
            If xmlData.Tables(3).Rows(0).ItemArray(17) = "True" Then
                ValveControl.ckb3Zone7Active.Checked = True
            Else
                ValveControl.ckb3Zone7Active.Checked = False
            End If
            ValveControl.mtxt3Zone8RunTime.Text = xmlData.Tables(3).Rows(0).ItemArray(18)
            If xmlData.Tables(3).Rows(0).ItemArray(19) = "True" Then
                ValveControl.ckb3Zone8Active.Checked = True
            Else
                ValveControl.ckb3Zone8Active.Checked = False
            End If
            ValveControl.mtxt3Zone9RunTime.Text = xmlData.Tables(3).Rows(0).ItemArray(20)
            If xmlData.Tables(3).Rows(0).ItemArray(21) = "True" Then
                ValveControl.ckb3Zone9Active.Checked = True
            Else
                ValveControl.ckb3Zone9Active.Checked = False
            End If
            ValveControl.mtxt3Zone10RunTime.Text = xmlData.Tables(3).Rows(0).ItemArray(22)
            If xmlData.Tables(3).Rows(0).ItemArray(23) = "True" Then
                ValveControl.ckb3Zone10Active.Checked = True
            Else
                ValveControl.ckb3Zone10Active.Checked = False
            End If
            ValveControl.mtxt3Zone11RunTime.Text = xmlData.Tables(3).Rows(0).ItemArray(24)
            If xmlData.Tables(3).Rows(0).ItemArray(25) = "True" Then
                ValveControl.ckb3Zone11Active.Checked = True
            Else
                ValveControl.ckb3Zone11Active.Checked = False
            End If
            ValveControl.mtxt3Zone12RunTime.Text = xmlData.Tables(3).Rows(0).ItemArray(26)
            If xmlData.Tables(3).Rows(0).ItemArray(27) = "True" Then
                ValveControl.ckb3Zone12Active.Checked = True
            Else
                ValveControl.ckb3Zone12Active.Checked = False
            End If
            ValveControl.mtxt3Zone13RunTime.Text = xmlData.Tables(3).Rows(0).ItemArray(28)
            If xmlData.Tables(3).Rows(0).ItemArray(29) = "True" Then
                ValveControl.ckb3Zone13Active.Checked = True
            Else
                ValveControl.ckb3Zone13Active.Checked = False
            End If


            'Plan 4
            ValveControl.mtxt4StartHour.Text = xmlData.Tables(4).Rows(0).ItemArray(0)
            ValveControl.mtxt4StartMinute.Text = xmlData.Tables(4).Rows(0).ItemArray(1)
            If xmlData.Tables(4).Rows(0).ItemArray(2) = "True" Then
                ValveControl.cb4CheckMoisture.Checked = True
            Else
                ValveControl.cb4CheckMoisture.Checked = False
            End If
            ValveControl.cbo4CheckMoisture.Text = xmlData.Tables(4).Rows(0).ItemArray(3)
            ValveControl.mtxt4Zone1RunTime.Text = xmlData.Tables(4).Rows(0).ItemArray(4)
            If xmlData.Tables(4).Rows(0).ItemArray(5) = "True" Then
                ValveControl.ckb4Zone1Active.Checked = True
            Else
                ValveControl.ckb4Zone1Active.Checked = False
            End If
            ValveControl.mtxt4Zone2RunTime.Text = xmlData.Tables(4).Rows(0).ItemArray(6)
            If xmlData.Tables(4).Rows(0).ItemArray(7) = "True" Then
                ValveControl.ckb4Zone2Active.Checked = True
            Else
                ValveControl.ckb4Zone2Active.Checked = False
            End If
            ValveControl.mtxt4Zone3RunTime.Text = xmlData.Tables(4).Rows(0).ItemArray(8)
            If xmlData.Tables(4).Rows(0).ItemArray(9) = "True" Then
                ValveControl.ckb4Zone3Active.Checked = True
            Else
                ValveControl.ckb4Zone3Active.Checked = False
            End If
            ValveControl.mtxt4Zone4RunTime.Text = xmlData.Tables(4).Rows(0).ItemArray(10)
            If xmlData.Tables(4).Rows(0).ItemArray(11) = "True" Then
                ValveControl.ckb4Zone4Active.Checked = True
            Else
                ValveControl.ckb4Zone4Active.Checked = False
            End If
            ValveControl.mtxt4Zone5RunTime.Text = xmlData.Tables(4).Rows(0).ItemArray(12)
            If xmlData.Tables(4).Rows(0).ItemArray(13) = "True" Then
                ValveControl.ckb4Zone5Active.Checked = True
            Else
                ValveControl.ckb4Zone5Active.Checked = False
            End If
            ValveControl.mtxt4Zone6RunTime.Text = xmlData.Tables(4).Rows(0).ItemArray(14)
            If xmlData.Tables(4).Rows(0).ItemArray(15) = "True" Then
                ValveControl.ckb4Zone6Active.Checked = True
            Else
                ValveControl.ckb4Zone6Active.Checked = False
            End If
            ValveControl.mtxt4Zone7RunTime.Text = xmlData.Tables(4).Rows(0).ItemArray(16)
            If xmlData.Tables(4).Rows(0).ItemArray(17) = "True" Then
                ValveControl.ckb4Zone7Active.Checked = True
            Else
                ValveControl.ckb4Zone7Active.Checked = False
            End If
            ValveControl.mtxt4Zone8RunTime.Text = xmlData.Tables(4).Rows(0).ItemArray(18)
            If xmlData.Tables(4).Rows(0).ItemArray(19) = "True" Then
                ValveControl.ckb4Zone8Active.Checked = True
            Else
                ValveControl.ckb4Zone8Active.Checked = False
            End If
            ValveControl.mtxt4Zone9RunTime.Text = xmlData.Tables(4).Rows(0).ItemArray(20)
            If xmlData.Tables(4).Rows(0).ItemArray(21) = "True" Then
                ValveControl.ckb4Zone9Active.Checked = True
            Else
                ValveControl.ckb4Zone9Active.Checked = False
            End If
            ValveControl.mtxt4Zone10RunTime.Text = xmlData.Tables(4).Rows(0).ItemArray(22)
            If xmlData.Tables(4).Rows(0).ItemArray(23) = "True" Then
                ValveControl.ckb4Zone10Active.Checked = True
            Else
                ValveControl.ckb4Zone10Active.Checked = False
            End If
            ValveControl.mtxt4Zone11RunTime.Text = xmlData.Tables(4).Rows(0).ItemArray(24)
            If xmlData.Tables(4).Rows(0).ItemArray(25) = "True" Then
                ValveControl.ckb4Zone11Active.Checked = True
            Else
                ValveControl.ckb4Zone11Active.Checked = False
            End If
            ValveControl.mtxt4Zone12RunTime.Text = xmlData.Tables(4).Rows(0).ItemArray(26)
            If xmlData.Tables(4).Rows(0).ItemArray(27) = "True" Then
                ValveControl.ckb4Zone12Active.Checked = True
            Else
                ValveControl.ckb4Zone12Active.Checked = False
            End If
            ValveControl.mtxt4Zone13RunTime.Text = xmlData.Tables(4).Rows(0).ItemArray(28)
            If xmlData.Tables(4).Rows(0).ItemArray(29) = "True" Then
                ValveControl.ckb4Zone13Active.Checked = True
            Else
                ValveControl.ckb4Zone13Active.Checked = False
            End If

        Catch ex As Exception
            'ignore
        End Try
    End Sub

    Public Sub SaveParameters()
        Dim FILE_NAME As String = "WDIrrigationControlParameter.xml"
        Dim objWriter As New System.IO.StreamWriter(FILE_NAME)
        Dim strParameter As String

        'Save Parameters
        strParameter = "<Parameter>"

        strParameter = strParameter & "<Active_Plan>"
        If ValveControl.cb1Active.Checked Then
            strParameter = strParameter & "1"
        End If
        If ValveControl.cb2Active.Checked Then
            strParameter = strParameter & "2"
        End If
        If ValveControl.cb3Active.Checked Then
            strParameter = strParameter & "3"
        End If
        If ValveControl.cb4Active.Checked Then
            strParameter = strParameter & "4"
        End If
        strParameter = strParameter & "</Active_Plan>"
        strParameter = strParameter & "<Polling_Interval>" & COMControl.mtxtPollingInterval.Text & "</Polling_Interval>"

        strParameter = strParameter & "<Plan_1>"
        strParameter = strParameter & "<Start_Hour>" & ValveControl.mtxt1StartHour.Text & "</Start_Hour>"
        strParameter = strParameter & "<Start_Minute>" & ValveControl.mtxt1StartMinute.Text & "</Start_Minute>"
        If ValveControl.cb1CheckMoisture.Checked = True Then
            strParameter = strParameter & "<Check_Moisture>True</Check_Moisture>"
        Else
            strParameter = strParameter & "<Check_Moisture>False</Check_Moisture>"
        End If
        strParameter = strParameter & "<Check_Moisture_Val>" & ValveControl.cbo1CheckMoisture.Text & "</Check_Moisture_Val>"
        strParameter = strParameter & "<Zone1RunTime>" & ValveControl.mtxt1Zone1RunTime.Text & "</Zone1RunTime>"
        If ValveControl.ckb1Zone1Active.Checked = True Then
            strParameter = strParameter & "<Zone1Active>True</Zone1Active>"
        Else
            strParameter = strParameter & "<Zone1Active>False</Zone1Active>"
        End If

        strParameter = strParameter & "<Zone2RunTime>" & ValveControl.mtxt1Zone2RunTime.Text & "</Zone2RunTime>"
        If ValveControl.ckb1Zone2Active.Checked = True Then
            strParameter = strParameter & "<Zone2Active>True</Zone2Active>"
        Else
            strParameter = strParameter & "<Zone2Active>False</Zone2Active>"
        End If

        strParameter = strParameter & "<Zone3RunTime>" & ValveControl.mtxt1Zone3RunTime.Text & "</Zone3RunTime>"
        If ValveControl.ckb1Zone3Active.Checked = True Then
            strParameter = strParameter & "<Zone3Active>True</Zone3Active>"
        Else
            strParameter = strParameter & "<Zone3Active>False</Zone3Active>"
        End If

        strParameter = strParameter & "<Zone4RunTime>" & ValveControl.mtxt1Zone4RunTime.Text & "</Zone4RunTime>"
        If ValveControl.ckb1Zone4Active.Checked = True Then
            strParameter = strParameter & "<Zone4Active>True</Zone4Active>"
        Else
            strParameter = strParameter & "<Zone4Active>False</Zone4Active>"
        End If

        strParameter = strParameter & "<Zone5RunTime>" & ValveControl.mtxt1Zone5RunTime.Text & "</Zone5RunTime>"
        If ValveControl.ckb1Zone5Active.Checked = True Then
            strParameter = strParameter & "<Zone5Active>True</Zone5Active>"
        Else
            strParameter = strParameter & "<Zone5Active>False</Zone5Active>"
        End If

        strParameter = strParameter & "<Zone6RunTime>" & ValveControl.mtxt1Zone6RunTime.Text & "</Zone6RunTime>"
        If ValveControl.ckb1Zone6Active.Checked = True Then
            strParameter = strParameter & "<Zone6Active>True</Zone6Active>"
        Else
            strParameter = strParameter & "<Zone6Active>False</Zone6Active>"
        End If

        strParameter = strParameter & "<Zone7RunTime>" & ValveControl.mtxt1Zone7RunTime.Text & "</Zone7RunTime>"
        If ValveControl.ckb1Zone7Active.Checked = True Then
            strParameter = strParameter & "<Zone7Active>True</Zone7Active>"
        Else
            strParameter = strParameter & "<Zone7Active>False</Zone7Active>"
        End If

        strParameter = strParameter & "<Zone8RunTime>" & ValveControl.mtxt1Zone8RunTime.Text & "</Zone8RunTime>"
        If ValveControl.ckb1Zone8Active.Checked = True Then
            strParameter = strParameter & "<Zone8Active>True</Zone8Active>"
        Else
            strParameter = strParameter & "<Zone8Active>False</Zone8Active>"
        End If

        strParameter = strParameter & "<Zone9RunTime>" & ValveControl.mtxt1Zone9RunTime.Text & "</Zone9RunTime>"
        If ValveControl.ckb1Zone9Active.Checked = True Then
            strParameter = strParameter & "<Zone9Active>True</Zone9Active>"
        Else
            strParameter = strParameter & "<Zone9Active>False</Zone9Active>"
        End If

        strParameter = strParameter & "<Zone10RunTime>" & ValveControl.mtxt1Zone10RunTime.Text & "</Zone10RunTime>"
        If ValveControl.ckb1Zone10Active.Checked = True Then
            strParameter = strParameter & "<Zone10Active>True</Zone10Active>"
        Else
            strParameter = strParameter & "<Zone10Active>False</Zone10Active>"
        End If

        strParameter = strParameter & "<Zone11RunTime>" & ValveControl.mtxt1Zone11RunTime.Text & "</Zone11RunTime>"
        If ValveControl.ckb1Zone11Active.Checked = True Then
            strParameter = strParameter & "<Zone11Active>True</Zone11Active>"
        Else
            strParameter = strParameter & "<Zone11Active>False</Zone11Active>"
        End If

        strParameter = strParameter & "<Zone12RunTime>" & ValveControl.mtxt1Zone12RunTime.Text & "</Zone12RunTime>"
        If ValveControl.ckb1Zone12Active.Checked = True Then
            strParameter = strParameter & "<Zone12Active>True</Zone12Active>"
        Else
            strParameter = strParameter & "<Zone12Active>False</Zone12Active>"
        End If

        strParameter = strParameter & "<Zone13RunTime>" & ValveControl.mtxt1Zone13RunTime.Text & "</Zone13RunTime>"
        If ValveControl.ckb1Zone13Active.Checked = True Then
            strParameter = strParameter & "<Zone13Active>True</Zone13Active>"
        Else
            strParameter = strParameter & "<Zone13Active>False</Zone13Active>"
        End If
        strParameter = strParameter & "</Plan_1>"

        strParameter = strParameter & "<Plan_2>"
        strParameter = strParameter & "<Start_Hour>" & ValveControl.mtxt2StartHour.Text & "</Start_Hour>"
        strParameter = strParameter & "<Start_Minute>" & ValveControl.mtxt2StartMinute.Text & "</Start_Minute>"
        If ValveControl.cb2CheckMoisture.Checked = True Then
            strParameter = strParameter & "<Check_Moisture>True</Check_Moisture>"
        Else
            strParameter = strParameter & "<Check_Moisture>False</Check_Moisture>"
        End If
        strParameter = strParameter & "<Check_Moisture_Val>" & ValveControl.cbo2CheckMoisture.Text & "</Check_Moisture_Val>"
        strParameter = strParameter & "<Zone1RunTime>" & ValveControl.mtxt2Zone1RunTime.Text & "</Zone1RunTime>"
        If ValveControl.ckb2Zone1Active.Checked = True Then
            strParameter = strParameter & "<Zone1Active>True</Zone1Active>"
        Else
            strParameter = strParameter & "<Zone1Active>False</Zone1Active>"
        End If

        strParameter = strParameter & "<Zone2RunTime>" & ValveControl.mtxt2Zone2RunTime.Text & "</Zone2RunTime>"
        If ValveControl.ckb2Zone2Active.Checked = True Then
            strParameter = strParameter & "<Zone2Active>True</Zone2Active>"
        Else
            strParameter = strParameter & "<Zone2Active>False</Zone2Active>"
        End If

        strParameter = strParameter & "<Zone3RunTime>" & ValveControl.mtxt2Zone3RunTime.Text & "</Zone3RunTime>"
        If ValveControl.ckb2Zone3Active.Checked = True Then
            strParameter = strParameter & "<Zone3Active>True</Zone3Active>"
        Else
            strParameter = strParameter & "<Zone3Active>False</Zone3Active>"
        End If

        strParameter = strParameter & "<Zone4RunTime>" & ValveControl.mtxt2Zone4RunTime.Text & "</Zone4RunTime>"
        If ValveControl.ckb2Zone4Active.Checked = True Then
            strParameter = strParameter & "<Zone4Active>True</Zone4Active>"
        Else
            strParameter = strParameter & "<Zone4Active>False</Zone4Active>"
        End If

        strParameter = strParameter & "<Zone5RunTime>" & ValveControl.mtxt2Zone5RunTime.Text & "</Zone5RunTime>"
        If ValveControl.ckb2Zone5Active.Checked = True Then
            strParameter = strParameter & "<Zone5Active>True</Zone5Active>"
        Else
            strParameter = strParameter & "<Zone5Active>False</Zone5Active>"
        End If

        strParameter = strParameter & "<Zone6RunTime>" & ValveControl.mtxt2Zone6RunTime.Text & "</Zone6RunTime>"
        If ValveControl.ckb2Zone6Active.Checked = True Then
            strParameter = strParameter & "<Zone6Active>True</Zone6Active>"
        Else
            strParameter = strParameter & "<Zone6Active>False</Zone6Active>"
        End If

        strParameter = strParameter & "<Zone7RunTime>" & ValveControl.mtxt2Zone7RunTime.Text & "</Zone7RunTime>"
        If ValveControl.ckb2Zone7Active.Checked = True Then
            strParameter = strParameter & "<Zone7Active>True</Zone7Active>"
        Else
            strParameter = strParameter & "<Zone7Active>False</Zone7Active>"
        End If

        strParameter = strParameter & "<Zone8RunTime>" & ValveControl.mtxt2Zone8RunTime.Text & "</Zone8RunTime>"
        If ValveControl.ckb2Zone8Active.Checked = True Then
            strParameter = strParameter & "<Zone8Active>True</Zone8Active>"
        Else
            strParameter = strParameter & "<Zone8Active>False</Zone8Active>"
        End If

        strParameter = strParameter & "<Zone9RunTime>" & ValveControl.mtxt2Zone9RunTime.Text & "</Zone9RunTime>"
        If ValveControl.ckb2Zone9Active.Checked = True Then
            strParameter = strParameter & "<Zone9Active>True</Zone9Active>"
        Else
            strParameter = strParameter & "<Zone9Active>False</Zone9Active>"
        End If

        strParameter = strParameter & "<Zone10RunTime>" & ValveControl.mtxt2Zone10RunTime.Text & "</Zone10RunTime>"
        If ValveControl.ckb2Zone10Active.Checked = True Then
            strParameter = strParameter & "<Zone10Active>True</Zone10Active>"
        Else
            strParameter = strParameter & "<Zone10Active>False</Zone10Active>"
        End If

        strParameter = strParameter & "<Zone11RunTime>" & ValveControl.mtxt2Zone11RunTime.Text & "</Zone11RunTime>"
        If ValveControl.ckb2Zone11Active.Checked = True Then
            strParameter = strParameter & "<Zone11Active>True</Zone11Active>"
        Else
            strParameter = strParameter & "<Zone11Active>False</Zone11Active>"
        End If

        strParameter = strParameter & "<Zone12RunTime>" & ValveControl.mtxt2Zone12RunTime.Text & "</Zone12RunTime>"
        If ValveControl.ckb2Zone12Active.Checked = True Then
            strParameter = strParameter & "<Zone12Active>True</Zone12Active>"
        Else
            strParameter = strParameter & "<Zone12Active>False</Zone12Active>"
        End If

        strParameter = strParameter & "<Zone13RunTime>" & ValveControl.mtxt2Zone13RunTime.Text & "</Zone13RunTime>"
        If ValveControl.ckb2Zone13Active.Checked = True Then
            strParameter = strParameter & "<Zone13Active>True</Zone13Active>"
        Else
            strParameter = strParameter & "<Zone13Active>False</Zone13Active>"
        End If
        strParameter = strParameter & "</Plan_2>"

        strParameter = strParameter & "<Plan_3>"
        strParameter = strParameter & "<Start_Hour>" & ValveControl.mtxt3StartHour.Text & "</Start_Hour>"
        strParameter = strParameter & "<Start_Minute>" & ValveControl.mtxt3StartMinute.Text & "</Start_Minute>"
        If ValveControl.cb3CheckMoisture.Checked = True Then
            strParameter = strParameter & "<Check_Moisture>True</Check_Moisture>"
        Else
            strParameter = strParameter & "<Check_Moisture>False</Check_Moisture>"
        End If
        strParameter = strParameter & "<Check_Moisture_Val>" & ValveControl.cbo3CheckMoisture.Text & "</Check_Moisture_Val>"
        strParameter = strParameter & "<Zone1RunTime>" & ValveControl.mtxt3Zone1RunTime.Text & "</Zone1RunTime>"
        If ValveControl.ckb3Zone1Active.Checked = True Then
            strParameter = strParameter & "<Zone1Active>True</Zone1Active>"
        Else
            strParameter = strParameter & "<Zone1Active>False</Zone1Active>"
        End If

        strParameter = strParameter & "<Zone2RunTime>" & ValveControl.mtxt3Zone2RunTime.Text & "</Zone2RunTime>"
        If ValveControl.ckb3Zone2Active.Checked = True Then
            strParameter = strParameter & "<Zone2Active>True</Zone2Active>"
        Else
            strParameter = strParameter & "<Zone2Active>False</Zone2Active>"
        End If

        strParameter = strParameter & "<Zone3RunTime>" & ValveControl.mtxt3Zone3RunTime.Text & "</Zone3RunTime>"
        If ValveControl.ckb3Zone3Active.Checked = True Then
            strParameter = strParameter & "<Zone3Active>True</Zone3Active>"
        Else
            strParameter = strParameter & "<Zone3Active>False</Zone3Active>"
        End If

        strParameter = strParameter & "<Zone4RunTime>" & ValveControl.mtxt3Zone4RunTime.Text & "</Zone4RunTime>"
        If ValveControl.ckb3Zone4Active.Checked = True Then
            strParameter = strParameter & "<Zone4Active>True</Zone4Active>"
        Else
            strParameter = strParameter & "<Zone4Active>False</Zone4Active>"
        End If

        strParameter = strParameter & "<Zone5RunTime>" & ValveControl.mtxt3Zone5RunTime.Text & "</Zone5RunTime>"
        If ValveControl.ckb3Zone5Active.Checked = True Then
            strParameter = strParameter & "<Zone5Active>True</Zone5Active>"
        Else
            strParameter = strParameter & "<Zone5Active>False</Zone5Active>"
        End If

        strParameter = strParameter & "<Zone6RunTime>" & ValveControl.mtxt3Zone6RunTime.Text & "</Zone6RunTime>"
        If ValveControl.ckb3Zone6Active.Checked = True Then
            strParameter = strParameter & "<Zone6Active>True</Zone6Active>"
        Else
            strParameter = strParameter & "<Zone6Active>False</Zone6Active>"
        End If

        strParameter = strParameter & "<Zone7RunTime>" & ValveControl.mtxt3Zone7RunTime.Text & "</Zone7RunTime>"
        If ValveControl.ckb3Zone7Active.Checked = True Then
            strParameter = strParameter & "<Zone7Active>True</Zone7Active>"
        Else
            strParameter = strParameter & "<Zone7Active>False</Zone7Active>"
        End If

        strParameter = strParameter & "<Zone8RunTime>" & ValveControl.mtxt3Zone8RunTime.Text & "</Zone8RunTime>"
        If ValveControl.ckb3Zone8Active.Checked = True Then
            strParameter = strParameter & "<Zone8Active>True</Zone8Active>"
        Else
            strParameter = strParameter & "<Zone8Active>False</Zone8Active>"
        End If

        strParameter = strParameter & "<Zone9RunTime>" & ValveControl.mtxt3Zone9RunTime.Text & "</Zone9RunTime>"
        If ValveControl.ckb3Zone9Active.Checked = True Then
            strParameter = strParameter & "<Zone9Active>True</Zone9Active>"
        Else
            strParameter = strParameter & "<Zone9Active>False</Zone9Active>"
        End If

        strParameter = strParameter & "<Zone10RunTime>" & ValveControl.mtxt3Zone10RunTime.Text & "</Zone10RunTime>"
        If ValveControl.ckb3Zone10Active.Checked = True Then
            strParameter = strParameter & "<Zone10Active>True</Zone10Active>"
        Else
            strParameter = strParameter & "<Zone10Active>False</Zone10Active>"
        End If

        strParameter = strParameter & "<Zone11RunTime>" & ValveControl.mtxt3Zone11RunTime.Text & "</Zone11RunTime>"
        If ValveControl.ckb3Zone11Active.Checked = True Then
            strParameter = strParameter & "<Zone11Active>True</Zone11Active>"
        Else
            strParameter = strParameter & "<Zone11Active>False</Zone11Active>"
        End If

        strParameter = strParameter & "<Zone12RunTime>" & ValveControl.mtxt3Zone12RunTime.Text & "</Zone12RunTime>"
        If ValveControl.ckb3Zone12Active.Checked = True Then
            strParameter = strParameter & "<Zone12Active>True</Zone12Active>"
        Else
            strParameter = strParameter & "<Zone12Active>False</Zone12Active>"
        End If

        strParameter = strParameter & "<Zone13RunTime>" & ValveControl.mtxt3Zone13RunTime.Text & "</Zone13RunTime>"
        If ValveControl.ckb3Zone13Active.Checked = True Then
            strParameter = strParameter & "<Zone13Active>True</Zone13Active>"
        Else
            strParameter = strParameter & "<Zone13Active>False</Zone13Active>"
        End If
        strParameter = strParameter & "</Plan_3>"


        strParameter = strParameter & "<Plan_4>"
        strParameter = strParameter & "<Start_Hour>" & ValveControl.mtxt4StartHour.Text & "</Start_Hour>"
        strParameter = strParameter & "<Start_Minute>" & ValveControl.mtxt4StartMinute.Text & "</Start_Minute>"
        If ValveControl.cb4CheckMoisture.Checked = True Then
            strParameter = strParameter & "<Check_Moisture>True</Check_Moisture>"
        Else
            strParameter = strParameter & "<Check_Moisture>False</Check_Moisture>"
        End If
        strParameter = strParameter & "<Check_Moisture_Val>" & ValveControl.cbo4CheckMoisture.Text & "</Check_Moisture_Val>"
        strParameter = strParameter & "<Zone1RunTime>" & ValveControl.mtxt4Zone1RunTime.Text & "</Zone1RunTime>"
        If ValveControl.ckb4Zone1Active.Checked = True Then
            strParameter = strParameter & "<Zone1Active>True</Zone1Active>"
        Else
            strParameter = strParameter & "<Zone1Active>False</Zone1Active>"
        End If

        strParameter = strParameter & "<Zone2RunTime>" & ValveControl.mtxt4Zone2RunTime.Text & "</Zone2RunTime>"
        If ValveControl.ckb4Zone2Active.Checked = True Then
            strParameter = strParameter & "<Zone2Active>True</Zone2Active>"
        Else
            strParameter = strParameter & "<Zone2Active>False</Zone2Active>"
        End If

        strParameter = strParameter & "<Zone3RunTime>" & ValveControl.mtxt4Zone3RunTime.Text & "</Zone3RunTime>"
        If ValveControl.ckb4Zone3Active.Checked = True Then
            strParameter = strParameter & "<Zone3Active>True</Zone3Active>"
        Else
            strParameter = strParameter & "<Zone3Active>False</Zone3Active>"
        End If

        strParameter = strParameter & "<Zone4RunTime>" & ValveControl.mtxt4Zone4RunTime.Text & "</Zone4RunTime>"
        If ValveControl.ckb4Zone4Active.Checked = True Then
            strParameter = strParameter & "<Zone4Active>True</Zone4Active>"
        Else
            strParameter = strParameter & "<Zone4Active>False</Zone4Active>"
        End If

        strParameter = strParameter & "<Zone5RunTime>" & ValveControl.mtxt4Zone5RunTime.Text & "</Zone5RunTime>"
        If ValveControl.ckb4Zone5Active.Checked = True Then
            strParameter = strParameter & "<Zone5Active>True</Zone5Active>"
        Else
            strParameter = strParameter & "<Zone5Active>False</Zone5Active>"
        End If

        strParameter = strParameter & "<Zone6RunTime>" & ValveControl.mtxt4Zone6RunTime.Text & "</Zone6RunTime>"
        If ValveControl.ckb4Zone6Active.Checked = True Then
            strParameter = strParameter & "<Zone6Active>True</Zone6Active>"
        Else
            strParameter = strParameter & "<Zone6Active>False</Zone6Active>"
        End If

        strParameter = strParameter & "<Zone7RunTime>" & ValveControl.mtxt4Zone7RunTime.Text & "</Zone7RunTime>"
        If ValveControl.ckb4Zone7Active.Checked = True Then
            strParameter = strParameter & "<Zone7Active>True</Zone7Active>"
        Else
            strParameter = strParameter & "<Zone7Active>False</Zone7Active>"
        End If

        strParameter = strParameter & "<Zone8RunTime>" & ValveControl.mtxt4Zone8RunTime.Text & "</Zone8RunTime>"
        If ValveControl.ckb4Zone8Active.Checked = True Then
            strParameter = strParameter & "<Zone8Active>True</Zone8Active>"
        Else
            strParameter = strParameter & "<Zone8Active>False</Zone8Active>"
        End If

        strParameter = strParameter & "<Zone9RunTime>" & ValveControl.mtxt4Zone9RunTime.Text & "</Zone9RunTime>"
        If ValveControl.ckb4Zone9Active.Checked = True Then
            strParameter = strParameter & "<Zone9Active>True</Zone9Active>"
        Else
            strParameter = strParameter & "<Zone9Active>False</Zone9Active>"
        End If

        strParameter = strParameter & "<Zone10RunTime>" & ValveControl.mtxt4Zone10RunTime.Text & "</Zone10RunTime>"
        If ValveControl.ckb4Zone10Active.Checked = True Then
            strParameter = strParameter & "<Zone10Active>True</Zone10Active>"
        Else
            strParameter = strParameter & "<Zone10Active>False</Zone10Active>"
        End If

        strParameter = strParameter & "<Zone11RunTime>" & ValveControl.mtxt4Zone11RunTime.Text & "</Zone11RunTime>"
        If ValveControl.ckb4Zone11Active.Checked = True Then
            strParameter = strParameter & "<Zone11Active>True</Zone11Active>"
        Else
            strParameter = strParameter & "<Zone11Active>False</Zone11Active>"
        End If

        strParameter = strParameter & "<Zone12RunTime>" & ValveControl.mtxt4Zone12RunTime.Text & "</Zone12RunTime>"
        If ValveControl.ckb4Zone12Active.Checked = True Then
            strParameter = strParameter & "<Zone12Active>True</Zone12Active>"
        Else
            strParameter = strParameter & "<Zone12Active>False</Zone12Active>"
        End If

        strParameter = strParameter & "<Zone13RunTime>" & ValveControl.mtxt4Zone13RunTime.Text & "</Zone13RunTime>"
        If ValveControl.ckb4Zone13Active.Checked = True Then
            strParameter = strParameter & "<Zone13Active>True</Zone13Active>"
        Else
            strParameter = strParameter & "<Zone13Active>False</Zone13Active>"
        End If
        strParameter = strParameter & "</Plan_4>"
        strParameter = strParameter & "</Parameter>"

        objWriter.Write(strParameter)
        objWriter.Close()
    End Sub

#End Region

#Region "Valve Control"

    Public Function StartValveControl() As Boolean
        COMControl.lblVCConnectionStatus.Text = "Connecting"
        Try
            _VCarduino = New Arduino_Net2("COM29")
            _VCarduino.ComPort = "COM29"
            COMControl.cbVC_COMPort.Text = "COM29"
            _VCarduino.BaudRate = 9600
            If Not _VCarduino.StartCommunication() Then
                Throw New System.Exception("Failed to Connect")
            End If

            VCConfigPorts()

            COMControl.btnVCConnect.Enabled = False
            COMControl.btnVCDisconnect.Enabled = True
            COMControl.cbVC_COMPort.Enabled = False

            ValveControl.gb1Valves.BackColor = Color.LightGreen
            ValveControl.gb2Valves.BackColor = Color.LightGreen
            ValveControl.gb3Valves.BackColor = Color.LightGreen
            ValveControl.gb4Valves.BackColor = Color.LightGreen
            btnValveControl.BackColor = Color.LightGreen

            COMControl.lblVCConnectionStatus.Text = "Connected"
            tValveControl.Interval = ((60 - Now.Second) * 1000) + 2000
            tValveControl.Start()
            tMeterRefresh.Start()
            Irrigation_HistoryTableAdapter.InsertQuery("104")
            Return True

        Catch ex As Exception
            MsgBox("Valve Control Connect Error")
            _VCarduino.StopCommunication()
            ValveControl.gb1Valves.BackColor = Color.Red
            ValveControl.gb2Valves.BackColor = Color.Red
            ValveControl.gb3Valves.BackColor = Color.Red
            ValveControl.gb4Valves.BackColor = Color.Red
            btnValveControl.BackColor = Color.Red

            bValveControlActive = False
            btnValveActivate.Enabled = True
            btnValveDeactivate.Enabled = False

            Irrigation_HistoryTableAdapter.InsertQuery("105")

            COMControl.lblVCConnectionStatus.Text = "Not Connected"
            COMControl.btnVCConnect.Enabled = True
            COMControl.btnVCDisconnect.Enabled = False
            COMControl.cbVC_COMPort.Enabled = True
            Return False
        End Try
    End Function

    Private Sub VCConfigPorts()
        Dim i As Integer
        'Valve Control Pins

        If _VCarduino.ComIsOpen Then
            For i = 1 To 3
                _VCarduino.EnableDigitalPort(2, True)
                Thread.Sleep(40)
                _VCarduino.EnableDigitalTrigger(2, True)
                Thread.Sleep(40)
                _VCarduino.SetDigitalDirection(2, Arduino_Net2.DigitalDirection.Input)
                Thread.Sleep(40)

                'Master Arm (Valve 14)
                _VCarduino.SetDigitalDirection(3, Arduino_Net2.DigitalDirection.DigitalOutput)
                Thread.Sleep(40)

                _VCarduino.SetDigitalDirection(22, Arduino_Net2.DigitalDirection.DigitalOutput)
                Thread.Sleep(40)
                _VCarduino.SetDigitalDirection(23, Arduino_Net2.DigitalDirection.DigitalOutput)
                Thread.Sleep(40)
                _VCarduino.SetDigitalDirection(24, Arduino_Net2.DigitalDirection.DigitalOutput)
                Thread.Sleep(40)
                _VCarduino.SetDigitalDirection(25, Arduino_Net2.DigitalDirection.DigitalOutput)
                Thread.Sleep(40)
                _VCarduino.SetDigitalDirection(26, Arduino_Net2.DigitalDirection.DigitalOutput)
                Thread.Sleep(40)
                _VCarduino.SetDigitalDirection(27, Arduino_Net2.DigitalDirection.DigitalOutput)
                Thread.Sleep(40)
                _VCarduino.SetDigitalDirection(28, Arduino_Net2.DigitalDirection.DigitalOutput)
                Thread.Sleep(40)
                _VCarduino.SetDigitalDirection(29, Arduino_Net2.DigitalDirection.DigitalOutput)
                Thread.Sleep(40)
                _VCarduino.SetDigitalDirection(30, Arduino_Net2.DigitalDirection.DigitalOutput)
                Thread.Sleep(40)
                _VCarduino.SetDigitalDirection(31, Arduino_Net2.DigitalDirection.DigitalOutput)
                Thread.Sleep(40)
                _VCarduino.SetDigitalDirection(32, Arduino_Net2.DigitalDirection.DigitalOutput)
                Thread.Sleep(40)
                _VCarduino.SetDigitalDirection(33, Arduino_Net2.DigitalDirection.DigitalOutput)
                Thread.Sleep(40)
                _VCarduino.SetDigitalDirection(34, Arduino_Net2.DigitalDirection.DigitalOutput)
                Thread.Sleep(40)
            Next i
        Else
            MsgBox("Connect Error to Valve Control")
            _VCarduino.StopCommunication()
            COMControl.lblVCConnectionStatus.Text = "Not Connected"
            COMControl.btnVCConnect.Enabled = True
            COMControl.btnVCDisconnect.Enabled = False
            COMControl.cbVC_COMPort.Enabled = True
        End If

    End Sub

    Public Sub UpdateExecutionPlan()
        Dim iSensorSum, i, iWetLast2Days As Integer

        If Not bOverride_Run And Not bOverride_Stop And Not bRunning Then

            Try
                tSleep.SynchronizingObject = Me
                Irrigation_HistoryTableAdapter.InsertQuery("309")
                Thread.Sleep(700)  'Wait for soil moisutre check to complete before proceeding
                UpdateScreen()
                If UpdateWeather2() Then
                    ExecutionPlanInputs.gbInternetFeed.BackColor = Color.LightGreen  'Primary source worked
                    btnShowWeatherForecast.BackColor = Color.LightGreen
                Else
                    If CheckAltWeather() Then
                        ExecutionPlanInputs.gbInternetFeed.BackColor = Color.Yellow   'Secondary source worked
                        btnShowWeatherForecast.BackColor = Color.Yellow
                    Else
                        ExecutionPlanInputs.gbInternetFeed.BackColor = Color.Red    'Both sources failed
                        btnShowWeatherForecast.BackColor = Color.Red
                    End If
                End If

            Catch e As Exception
                ' do nothing
            End Try


            iSensorSum = 0
            For i = 0 To 7
                iSensorSum = iSensorSum + SensorValue(i)
            Next

            'if 1 or fewer sensors show moisture, we need to run the sprinklers
            If iSensorSum <= ValveControl.iCheckMoistureVal Then
                bSoilWet = False
            Else
                bSoilWet = True
            End If

            If ValveControl.CheckMoisture.Checked = False Then
                'Ignore soil moisture sensors
                bSoilWet = False
                lblSoilMoisture.Text = "Ignore"
                lblRainToday.Text = "No"
                Irrigation_HistoryTableAdapter.InsertQuery("311")
            ElseIf bSoilWet Then
                lblSoilMoisture.Text = "Yes"
                Irrigation_HistoryTableAdapter.InsertQuery("310")
                If Hour(Now) >= 11 Then
                    'if it's after 10AM and the ground is still wet, cancel watering for today since it either rained, or its been wet for more than 8 hours.
                    lblRainToday.Text = "Yes"
                End If
            Else
                lblSoilMoisture.Text = "No"
                Irrigation_HistoryTableAdapter.InsertQuery("311")
            End If

            If bWeatherBad Then
                lblPrecipForecast.Text = "Yes"
                Irrigation_HistoryTableAdapter.InsertQuery("313")
            Else
                lblPrecipForecast.Text = "No"
                Irrigation_HistoryTableAdapter.InsertQuery("312")
            End If

            iWetLast2Days = Irrigation_HistoryTableAdapter.WetLast2Days()
            If iWetLast2Days > 0 Then
                lblWetInLast2Days.Text = "Yes"
            Else
                lblWetInLast2Days.Text = "No"
            End If

            'Thread.Sleep(100)

            If bWeatherBad Or bSoilWet Then
                lblExecutionPlan.Text = "Don't Run"
                bExecutionPlan = False
                Irrigation_HistoryTableAdapter.InsertQuery("315")
            Else
                lblExecutionPlan.Text = "Run"
                bExecutionPlan = True
                Irrigation_HistoryTableAdapter.InsertQuery("314")
            End If
        End If



    End Sub

    Private Function FindNextActiveZone(ByVal iActiveZone As Integer)
        Select Case (iActiveZone)
            Case 0
                If ValveControl.ValveActiveArray(1).Checked Then
                    iActiveZone = 1
                    iZoneTimer = Int(ValveControl.ValveTimeArray(1).Text)
                ElseIf ValveControl.ValveActiveArray(2).Checked Then
                    iActiveZone = 2
                    iZoneTimer = Int(ValveControl.ValveTimeArray(2).Text)
                ElseIf ValveControl.ValveActiveArray(3).Checked Then
                    iActiveZone = 3
                    iZoneTimer = Int(ValveControl.ValveTimeArray(3).Text)
                ElseIf ValveControl.ValveActiveArray(4).Checked Then
                    iActiveZone = 4
                    iZoneTimer = Int(ValveControl.ValveTimeArray(4).Text)
                ElseIf ValveControl.ValveActiveArray(5).Checked Then
                    iActiveZone = 5
                    iZoneTimer = Int(ValveControl.ValveTimeArray(5).Text)
                ElseIf ValveControl.ValveActiveArray(6).Checked Then
                    iActiveZone = 6
                    iZoneTimer = Int(ValveControl.ValveTimeArray(6).Text)
                ElseIf ValveControl.ValveActiveArray(7).Checked Then
                    iActiveZone = 7
                    iZoneTimer = Int(ValveControl.ValveTimeArray(7).Text)
                ElseIf ValveControl.ValveActiveArray(8).Checked Then
                    iActiveZone = 8
                    iZoneTimer = Int(ValveControl.ValveTimeArray(8).Text)
                ElseIf ValveControl.ValveActiveArray(9).Checked Then
                    iActiveZone = 9
                    iZoneTimer = Int(ValveControl.ValveTimeArray(9).Text)
                ElseIf ValveControl.ValveActiveArray(10).Checked Then
                    iActiveZone = 10
                    iZoneTimer = Int(ValveControl.ValveTimeArray(10).Text)
                ElseIf ValveControl.ValveActiveArray(11).Checked Then
                    iActiveZone = 11
                    iZoneTimer = Int(ValveControl.ValveTimeArray(11).Text)
                ElseIf ValveControl.ValveActiveArray(12).Checked Then
                    iActiveZone = 12
                    iZoneTimer = Int(ValveControl.ValveTimeArray(12).Text)
                ElseIf ValveControl.ValveActiveArray(13).Checked Then
                    iActiveZone = 13
                    iZoneTimer = Int(ValveControl.ValveTimeArray(13).Text)
                Else
                    iActiveZone = 0
                    iZoneTimer = 0
                End If
            Case 1
                If ValveControl.ValveActiveArray(2).Checked Then
                    iActiveZone = 2
                    iZoneTimer = Int(ValveControl.ValveTimeArray(2).Text)
                ElseIf ValveControl.ValveActiveArray(3).Checked Then
                    iActiveZone = 3
                    iZoneTimer = Int(ValveControl.ValveTimeArray(3).Text)
                ElseIf ValveControl.ValveActiveArray(4).Checked Then
                    iActiveZone = 4
                    iZoneTimer = Int(ValveControl.ValveTimeArray(4).Text)
                ElseIf ValveControl.ValveActiveArray(5).Checked Then
                    iActiveZone = 5
                    iZoneTimer = Int(ValveControl.ValveTimeArray(5).Text)
                ElseIf ValveControl.ValveActiveArray(6).Checked Then
                    iActiveZone = 6
                    iZoneTimer = Int(ValveControl.ValveTimeArray(6).Text)
                ElseIf ValveControl.ValveActiveArray(7).Checked Then
                    iActiveZone = 7
                    iZoneTimer = Int(ValveControl.ValveTimeArray(7).Text)
                ElseIf ValveControl.ValveActiveArray(8).Checked Then
                    iActiveZone = 8
                    iZoneTimer = Int(ValveControl.ValveTimeArray(8).Text)
                ElseIf ValveControl.ValveActiveArray(9).Checked Then
                    iActiveZone = 9
                    iZoneTimer = Int(ValveControl.ValveTimeArray(9).Text)
                ElseIf ValveControl.ValveActiveArray(10).Checked Then
                    iActiveZone = 10
                    iZoneTimer = Int(ValveControl.ValveTimeArray(10).Text)
                ElseIf ValveControl.ValveActiveArray(11).Checked Then
                    iActiveZone = 11
                    iZoneTimer = Int(ValveControl.ValveTimeArray(11).Text)
                ElseIf ValveControl.ValveActiveArray(12).Checked Then
                    iActiveZone = 12
                    iZoneTimer = Int(ValveControl.ValveTimeArray(12).Text)
                ElseIf ValveControl.ValveActiveArray(13).Checked Then
                    iActiveZone = 13
                    iZoneTimer = Int(ValveControl.ValveTimeArray(13).Text)
                Else
                    iActiveZone = 0
                    iZoneTimer = 0
                End If
            Case 2
                If ValveControl.ValveActiveArray(3).Checked Then
                    iActiveZone = 3
                    iZoneTimer = Int(ValveControl.ValveTimeArray(3).Text)
                ElseIf ValveControl.ValveActiveArray(4).Checked Then
                    iActiveZone = 4
                    iZoneTimer = Int(ValveControl.ValveTimeArray(4).Text)
                ElseIf ValveControl.ValveActiveArray(5).Checked Then
                    iActiveZone = 5
                    iZoneTimer = Int(ValveControl.ValveTimeArray(5).Text)
                ElseIf ValveControl.ValveActiveArray(6).Checked Then
                    iActiveZone = 6
                    iZoneTimer = Int(ValveControl.ValveTimeArray(6).Text)
                ElseIf ValveControl.ValveActiveArray(7).Checked Then
                    iActiveZone = 7
                    iZoneTimer = Int(ValveControl.ValveTimeArray(7).Text)
                ElseIf ValveControl.ValveActiveArray(8).Checked Then
                    iActiveZone = 8
                    iZoneTimer = Int(ValveControl.ValveTimeArray(8).Text)
                ElseIf ValveControl.ValveActiveArray(9).Checked Then
                    iActiveZone = 9
                    iZoneTimer = Int(ValveControl.ValveTimeArray(9).Text)
                ElseIf ValveControl.ValveActiveArray(10).Checked Then
                    iActiveZone = 10
                    iZoneTimer = Int(ValveControl.ValveTimeArray(10).Text)
                ElseIf ValveControl.ValveActiveArray(11).Checked Then
                    iActiveZone = 11
                    iZoneTimer = Int(ValveControl.ValveTimeArray(11).Text)
                ElseIf ValveControl.ValveActiveArray(12).Checked Then
                    iActiveZone = 12
                    iZoneTimer = Int(ValveControl.ValveTimeArray(12).Text)
                ElseIf ValveControl.ValveActiveArray(13).Checked Then
                    iActiveZone = 13
                    iZoneTimer = Int(ValveControl.ValveTimeArray(13).Text)
                Else
                    iActiveZone = 0
                    iZoneTimer = 0
                End If
            Case 3
                If ValveControl.ValveActiveArray(4).Checked Then
                    iActiveZone = 4
                    iZoneTimer = Int(ValveControl.ValveTimeArray(4).Text)
                ElseIf ValveControl.ValveActiveArray(5).Checked Then
                    iActiveZone = 5
                    iZoneTimer = Int(ValveControl.ValveTimeArray(5).Text)
                ElseIf ValveControl.ValveActiveArray(6).Checked Then
                    iActiveZone = 6
                    iZoneTimer = Int(ValveControl.ValveTimeArray(6).Text)
                ElseIf ValveControl.ValveActiveArray(7).Checked Then
                    iActiveZone = 7
                    iZoneTimer = Int(ValveControl.ValveTimeArray(7).Text)
                ElseIf ValveControl.ValveActiveArray(8).Checked Then
                    iActiveZone = 8
                    iZoneTimer = Int(ValveControl.ValveTimeArray(8).Text)
                ElseIf ValveControl.ValveActiveArray(9).Checked Then
                    iActiveZone = 9
                    iZoneTimer = Int(ValveControl.ValveTimeArray(9).Text)
                ElseIf ValveControl.ValveActiveArray(10).Checked Then
                    iActiveZone = 10
                    iZoneTimer = Int(ValveControl.ValveTimeArray(10).Text)
                ElseIf ValveControl.ValveActiveArray(11).Checked Then
                    iActiveZone = 11
                    iZoneTimer = Int(ValveControl.ValveTimeArray(11).Text)
                ElseIf ValveControl.ValveActiveArray(12).Checked Then
                    iActiveZone = 12
                    iZoneTimer = Int(ValveControl.ValveTimeArray(12).Text)
                ElseIf ValveControl.ValveActiveArray(13).Checked Then
                    iActiveZone = 13
                    iZoneTimer = Int(ValveControl.ValveTimeArray(13).Text)
                Else
                    iActiveZone = 0
                    iZoneTimer = 0
                End If
            Case 4
                If ValveControl.ValveActiveArray(5).Checked Then
                    iActiveZone = 5
                    iZoneTimer = Int(ValveControl.ValveTimeArray(5).Text)
                ElseIf ValveControl.ValveActiveArray(6).Checked Then
                    iActiveZone = 6
                    iZoneTimer = Int(ValveControl.ValveTimeArray(6).Text)
                ElseIf ValveControl.ValveActiveArray(7).Checked Then
                    iActiveZone = 7
                    iZoneTimer = Int(ValveControl.ValveTimeArray(7).Text)
                ElseIf ValveControl.ValveActiveArray(8).Checked Then
                    iActiveZone = 8
                    iZoneTimer = Int(ValveControl.ValveTimeArray(8).Text)
                ElseIf ValveControl.ValveActiveArray(9).Checked Then
                    iActiveZone = 9
                    iZoneTimer = Int(ValveControl.ValveTimeArray(9).Text)
                ElseIf ValveControl.ValveActiveArray(10).Checked Then
                    iActiveZone = 10
                    iZoneTimer = Int(ValveControl.ValveTimeArray(10).Text)
                ElseIf ValveControl.ValveActiveArray(11).Checked Then
                    iActiveZone = 11
                    iZoneTimer = Int(ValveControl.ValveTimeArray(11).Text)
                ElseIf ValveControl.ValveActiveArray(12).Checked Then
                    iActiveZone = 12
                    iZoneTimer = Int(ValveControl.ValveTimeArray(12).Text)
                ElseIf ValveControl.ValveActiveArray(13).Checked Then
                    iActiveZone = 13
                    iZoneTimer = Int(ValveControl.ValveTimeArray(13).Text)
                Else
                    iActiveZone = 0
                    iZoneTimer = 0
                End If
            Case 5
                If ValveControl.ValveActiveArray(6).Checked Then
                    iActiveZone = 6
                    iZoneTimer = Int(ValveControl.ValveTimeArray(6).Text)
                ElseIf ValveControl.ValveActiveArray(7).Checked Then
                    iActiveZone = 7
                    iZoneTimer = Int(ValveControl.ValveTimeArray(7).Text)
                ElseIf ValveControl.ValveActiveArray(8).Checked Then
                    iActiveZone = 8
                    iZoneTimer = Int(ValveControl.ValveTimeArray(8).Text)
                ElseIf ValveControl.ValveActiveArray(9).Checked Then
                    iActiveZone = 9
                    iZoneTimer = Int(ValveControl.ValveTimeArray(9).Text)
                ElseIf ValveControl.ValveActiveArray(10).Checked Then
                    iActiveZone = 10
                    iZoneTimer = Int(ValveControl.ValveTimeArray(10).Text)
                ElseIf ValveControl.ValveActiveArray(11).Checked Then
                    iActiveZone = 11
                    iZoneTimer = Int(ValveControl.ValveTimeArray(11).Text)
                ElseIf ValveControl.ValveActiveArray(12).Checked Then
                    iActiveZone = 12
                    iZoneTimer = Int(ValveControl.ValveTimeArray(12).Text)
                ElseIf ValveControl.ValveActiveArray(13).Checked Then
                    iActiveZone = 13
                    iZoneTimer = Int(ValveControl.ValveTimeArray(13).Text)
                Else
                    iActiveZone = 0
                    iZoneTimer = 0
                End If
            Case 6
                If ValveControl.ValveActiveArray(7).Checked Then
                    iActiveZone = 7
                    iZoneTimer = Int(ValveControl.ValveTimeArray(7).Text)
                ElseIf ValveControl.ValveActiveArray(8).Checked Then
                    iActiveZone = 8
                    iZoneTimer = Int(ValveControl.ValveTimeArray(8).Text)
                ElseIf ValveControl.ValveActiveArray(9).Checked Then
                    iActiveZone = 9
                    iZoneTimer = Int(ValveControl.ValveTimeArray(9).Text)
                ElseIf ValveControl.ValveActiveArray(10).Checked Then
                    iActiveZone = 10
                    iZoneTimer = Int(ValveControl.ValveTimeArray(10).Text)
                ElseIf ValveControl.ValveActiveArray(11).Checked Then
                    iActiveZone = 11
                    iZoneTimer = Int(ValveControl.ValveTimeArray(11).Text)
                ElseIf ValveControl.ValveActiveArray(12).Checked Then
                    iActiveZone = 12
                    iZoneTimer = Int(ValveControl.ValveTimeArray(12).Text)
                ElseIf ValveControl.ValveActiveArray(13).Checked Then
                    iActiveZone = 13
                    iZoneTimer = Int(ValveControl.ValveTimeArray(13).Text)
                Else
                    iActiveZone = 0
                    iZoneTimer = 0
                End If
            Case 7
                If ValveControl.ValveActiveArray(8).Checked Then
                    iActiveZone = 8
                    iZoneTimer = Int(ValveControl.ValveTimeArray(8).Text)
                ElseIf ValveControl.ValveActiveArray(9).Checked Then
                    iActiveZone = 9
                    iZoneTimer = Int(ValveControl.ValveTimeArray(9).Text)
                ElseIf ValveControl.ValveActiveArray(10).Checked Then
                    iActiveZone = 10
                    iZoneTimer = Int(ValveControl.ValveTimeArray(10).Text)
                ElseIf ValveControl.ValveActiveArray(11).Checked Then
                    iActiveZone = 11
                    iZoneTimer = Int(ValveControl.ValveTimeArray(11).Text)
                ElseIf ValveControl.ValveActiveArray(12).Checked Then
                    iActiveZone = 12
                    iZoneTimer = Int(ValveControl.ValveTimeArray(12).Text)
                ElseIf ValveControl.ValveActiveArray(13).Checked Then
                    iActiveZone = 13
                    iZoneTimer = Int(ValveControl.ValveTimeArray(13).Text)
                Else
                    iActiveZone = 0
                    iZoneTimer = 0
                End If
            Case 8
                If ValveControl.ValveActiveArray(9).Checked Then
                    iActiveZone = 9
                    iZoneTimer = Int(ValveControl.ValveTimeArray(9).Text)
                ElseIf ValveControl.ValveActiveArray(10).Checked Then
                    iActiveZone = 10
                    iZoneTimer = Int(ValveControl.ValveTimeArray(10).Text)
                ElseIf ValveControl.ValveActiveArray(11).Checked Then
                    iActiveZone = 11
                    iZoneTimer = Int(ValveControl.ValveTimeArray(11).Text)
                ElseIf ValveControl.ValveActiveArray(12).Checked Then
                    iActiveZone = 12
                    iZoneTimer = Int(ValveControl.ValveTimeArray(12).Text)
                ElseIf ValveControl.ValveActiveArray(13).Checked Then
                    iActiveZone = 13
                    iZoneTimer = Int(ValveControl.ValveTimeArray(13).Text)
                Else
                    iActiveZone = 0
                    iZoneTimer = 0
                End If
            Case 9
                If ValveControl.ValveActiveArray(10).Checked Then
                    iActiveZone = 10
                    iZoneTimer = Int(ValveControl.ValveTimeArray(10).Text)
                ElseIf ValveControl.ValveActiveArray(11).Checked Then
                    iActiveZone = 11
                    iZoneTimer = Int(ValveControl.ValveTimeArray(11).Text)
                ElseIf ValveControl.ValveActiveArray(12).Checked Then
                    iActiveZone = 12
                    iZoneTimer = Int(ValveControl.ValveTimeArray(12).Text)
                ElseIf ValveControl.ValveActiveArray(13).Checked Then
                    iActiveZone = 13
                    iZoneTimer = Int(ValveControl.ValveTimeArray(13).Text)
                Else
                    iActiveZone = 0
                    iZoneTimer = 0
                End If
            Case 10
                If ValveControl.ValveActiveArray(11).Checked Then
                    iActiveZone = 11
                    iZoneTimer = Int(ValveControl.ValveTimeArray(11).Text)
                ElseIf ValveControl.ValveActiveArray(12).Checked Then
                    iActiveZone = 12
                    iZoneTimer = Int(ValveControl.ValveTimeArray(12).Text)
                ElseIf ValveControl.ValveActiveArray(13).Checked Then
                    iActiveZone = 13
                    iZoneTimer = Int(ValveControl.ValveTimeArray(13).Text)
                Else
                    iActiveZone = 0
                    iZoneTimer = 0
                End If
            Case 11
                If ValveControl.ValveActiveArray(12).Checked Then
                    iActiveZone = 12
                    iZoneTimer = Int(ValveControl.ValveTimeArray(12).Text)
                ElseIf ValveControl.ValveActiveArray(13).Checked Then
                    iActiveZone = 13
                    iZoneTimer = Int(ValveControl.ValveTimeArray(13).Text)
                Else
                    iActiveZone = 0
                    iZoneTimer = 0
                End If
            Case 12
                If ValveControl.ValveActiveArray(13).Checked Then
                    iActiveZone = 13
                    iZoneTimer = Int(ValveControl.ValveTimeArray(13).Text)
                Else
                    iActiveZone = 0
                    iZoneTimer = 0
                End If
            Case 13
                iActiveZone = 0
                iZoneTimer = 0
        End Select

        Return iActiveZone

    End Function

    Public Sub Override_ExecutionPlan_Run()
        bOverride_Run = True

        bExecutionPlan = True
        bWeatherBad = False
        bSoilWet = False
        lblExecutionPlan.Text = "O: Run"
        lblSoilMoisture.Text = "Override"
        lblPrecipForecast.Text = "Override"
        btnOverrideRun.Enabled = False
        btnOverrideStop.Enabled = True
        Irrigation_HistoryTableAdapter.InsertQuery("307")
    End Sub

    Private Sub Override_ExecutionPlan_Stop()
        bOverride_Stop = True

        bExecutionPlan = False
        bWeatherBad = False
        bSoilWet = False
        lblExecutionPlan.Text = "O: Don't Run"
        lblSoilMoisture.Text = "Override"
        lblPrecipForecast.Text = "Override"
        btnOverrideStop.Enabled = False
        btnOverrideRun.Enabled = True
        Irrigation_HistoryTableAdapter.InsertQuery("317")
    End Sub
#End Region

#Region "Soil Moisture"

    Public Function StartSoilMonitor() As Boolean
        COMControl.lblSMConnectionStatus.Text = "Connecting"

        Try
            _SMarduino = New Arduino_Net2(COMControl.cbSM_COMPort.Text)
            _SMarduino.ComPort = COMControl.cbSM_COMPort.Text
            'COMControl.cbSM_COMPort.Text = "COM3"
            _SMarduino.BaudRate = 9600
            If Not _SMarduino.StartCommunication() Then
                Throw New System.Exception("Failed to Connect")
            End If

            SMConfigPorts()

            For i = 0 To 7
                SensorValue(i) = 0
            Next

            COMControl.btnSMConnect.Enabled = False
            COMControl.btnSMDisconnect.Enabled = True
            COMControl.btnStart.Enabled = True
            COMControl.cbSM_COMPort.Enabled = False
            COMControl.lblSMConnectionStatus.Text = "Connected"

            ExecutionPlanInputs.gbSoilMoistureStatus.BackColor = Color.LightGreen
            btnShowWeatherForecast.BackColor = Color.LightGreen

            Thread.Sleep(100)

            If CheckSoilMoisture() = False Then
                Return False
            End If

            Irrigation_HistoryTableAdapter.InsertQuery("102")

            COMControl.lblMonitor.Text = "Monitoring"
            COMControl.btnStop.Enabled = True
            COMControl.btnStart.Enabled = False

            tSleep.Start()
            Return True

        Catch ex As Exception
            MsgBox("Soil Moisture Connect Error")
            COMControl.lblSMConnectionStatus.Text = "Not Connected"
            COMControl.btnSMConnect.Enabled = True
            COMControl.cbSM_COMPort.Enabled = True
            COMControl.btnSMDisconnect.Enabled = False
            COMControl.btnStart.Enabled = False
            COMControl.btnStop.Enabled = False
            COMControl.mtxtPollingInterval.Enabled = True
            _SMarduino.StopCommunication()
            ExecutionPlanInputs.gbSoilMoistureStatus.BackColor = Color.Red
            btnShowWeatherForecast.BackColor = Color.Red
            Irrigation_HistoryTableAdapter.InsertQuery("103")

            Return False
        End Try

    End Function

    Private Sub SMConfigPorts()
        'Moisture Sensors

        If _SMarduino.ComIsOpen Then
            _SMarduino.EnableDigitalPort(2, True)
            _SMarduino.SetDigitalDirection(2, Arduino_Net2.DigitalDirection.DigitalOutput)

            _SMarduino.EnableDigitalPort(3, True)
            _SMarduino.SetDigitalDirection(3, Arduino_Net2.DigitalDirection.Input)

            _SMarduino.EnableDigitalPort(4, True)
            _SMarduino.SetDigitalDirection(4, Arduino_Net2.DigitalDirection.Input)

            _SMarduino.EnableDigitalPort(5, True)
            _SMarduino.SetDigitalDirection(4, Arduino_Net2.DigitalDirection.Input)

            _SMarduino.EnableDigitalPort(6, True)
            _SMarduino.SetDigitalDirection(6, Arduino_Net2.DigitalDirection.Input)

            _SMarduino.EnableDigitalPort(7, True)
            _SMarduino.SetDigitalDirection(7, Arduino_Net2.DigitalDirection.Input)

            _SMarduino.EnableDigitalPort(8, True)
            _SMarduino.SetDigitalDirection(8, Arduino_Net2.DigitalDirection.Input)

            _SMarduino.EnableDigitalPort(9, True)
            _SMarduino.SetDigitalDirection(9, Arduino_Net2.DigitalDirection.Input)

            _SMarduino.EnableDigitalPort(10, True)
            _SMarduino.SetDigitalDirection(10, Arduino_Net2.DigitalDirection.Input)

            AddHandler _SMarduino.DigitalDataReceived, AddressOf Arduino_DigitalDataReceived

            _SMarduino.EnableDigitalTrigger(10, False)
            _SMarduino.EnableDigitalTrigger(9, False)
            _SMarduino.EnableDigitalTrigger(8, False)
            _SMarduino.EnableDigitalTrigger(7, False)
            _SMarduino.EnableDigitalTrigger(6, False)
            _SMarduino.EnableDigitalTrigger(5, False)
            _SMarduino.EnableDigitalTrigger(4, False)
            _SMarduino.EnableDigitalTrigger(3, False)
        End If

    End Sub

    Public Function CheckSoilMoisture() As Boolean
        Try
            If Not _SMarduino.ComIsOpen Then
                If Not StartSoilMonitor() Then
                    Throw New ApplicationException("COM Port Closed")
                End If
            End If
            _SMarduino.SetDigitalValue(2, 1)
            Thread.Sleep(200)
            _SMarduino.GetDigitalValue(3)
            _SMarduino.GetDigitalValue(10)
            _SMarduino.GetDigitalValue(9)
            _SMarduino.GetDigitalValue(8)
            _SMarduino.GetDigitalValue(7)
            _SMarduino.GetDigitalValue(6)
            _SMarduino.GetDigitalValue(5)
            _SMarduino.GetDigitalValue(4)
            _SMarduino.SetDigitalValue(2, 0)
            Thread.Sleep(200)

            UpdateExecutionPlan()
            Return True

        Catch ex As Exception
            'tSleep.Stop()

            COMControl.lblSMConnectionStatus.Text = "Not Connected"
            COMControl.btnSMConnect.Enabled = True
            COMControl.cbSM_COMPort.Enabled = True
            COMControl.btnSMDisconnect.Enabled = False
            COMControl.btnStart.Enabled = False
            COMControl.btnStop.Enabled = False
            COMControl.mtxtPollingInterval.Enabled = True
            _SMarduino.StopCommunication()
            ExecutionPlanInputs.gbSoilMoistureStatus.BackColor = Color.Red
            btnShowWeatherForecast.BackColor = Color.Red
            Irrigation_HistoryTableAdapter.InsertQuery("107")
            COMControl.lblMonitor.Text = "Not Monitoring"
            'MsgBox("Error detected, monitoring disabled")
            Return False
        End Try
    End Function

    Private Sub Arduino_DigitalDataReceived(ByVal DPortNr As Integer, ByVal Value As Integer)

        Try
            If DPortNr = 10 Then
                If SensorValue(0) <> Value Then
                    SensorValue(0) = Value
                    tSleep.SynchronizingObject = Me
                End If
                'Console.Write("Port 1 value = " & Value.ToString & "   " & Now.ToString & Chr(13))
            End If

            If DPortNr = 9 Then
                If SensorValue(1) <> Value Then
                    SensorValue(1) = Value
                    tSleep.SynchronizingObject = Me
                End If
                'Console.Write("Port 2 value = " & Value.ToString & "   " & Now.ToString & Chr(13))
            End If

            If DPortNr = 8 Then
                If SensorValue(2) <> Value Then
                    SensorValue(2) = Value
                    tSleep.SynchronizingObject = Me
                End If
                'Console.Write("Port 3 value = " & Value.ToString & "   " & Now.ToString & Chr(13))
            End If

            If DPortNr = 7 Then
                If SensorValue(3) <> Value Then
                    SensorValue(3) = Value
                    tSleep.SynchronizingObject = Me
                End If
                'Console.Write("Port 4 value = " & Value.ToString & "   " & Now.ToString & Chr(13))
            End If

            If DPortNr = 6 Then
                If SensorValue(4) <> Value Then
                    SensorValue(4) = Value
                    tSleep.SynchronizingObject = Me
                End If
                'Console.Write("Port 5 value = " & Value.ToString & "   " & Now.ToString & Chr(13))
            End If

            If DPortNr = 5 Then
                If SensorValue(5) <> Value Then
                    SensorValue(5) = Value
                    tSleep.SynchronizingObject = Me
                End If
                'Console.Write("Port 6 value = " & Value.ToString & "   " & Now.ToString & Chr(13))
            End If

            If DPortNr = 4 Then
                If SensorValue(6) <> Value Then
                    SensorValue(6) = Value
                    tSleep.SynchronizingObject = Me
                End If
                'Console.Write("Port 7 value = " & Value.ToString & "   " & Now.ToString & Chr(13))
            End If

            If DPortNr = 3 Then
                If SensorValue(7) <> Value Then
                    SensorValue(7) = Value
                    tSleep.SynchronizingObject = Me
                End If
                'Console.Write("Port 8 value = " & Value.ToString & "   " & Now.ToString & Chr(13))
            End If

        Catch ex As Exception
        End Try
    End Sub

    Public Sub UpdateScreen()
        ExecutionPlanInputs.lblZone1SoilMoisture.Text = SensorValue(0).ToString
        ExecutionPlanInputs.lblZone2SoilMoisture.Text = SensorValue(1).ToString
        ExecutionPlanInputs.lblZone3SoilMoisture.Text = SensorValue(2).ToString
        ExecutionPlanInputs.lblZone4SoilMoisture.Text = SensorValue(3).ToString
        ExecutionPlanInputs.lblZone5SoilMoisture.Text = SensorValue(4).ToString
        ExecutionPlanInputs.lblZone6SoilMoisture.Text = SensorValue(5).ToString
        ExecutionPlanInputs.lblZone7SoilMoisture.Text = SensorValue(6).ToString
        ExecutionPlanInputs.lblZone8SoilMoisture.Text = SensorValue(7).ToString

        If SensorValue(0) = 1 Then
            IrrigationMap.MSensor1.Visible = True
        Else
            IrrigationMap.MSensor1.Visible = False
        End If

        If SensorValue(1) = 1 Then
            IrrigationMap.MSensor2.Visible = True
        Else
            IrrigationMap.MSensor2.Visible = False
        End If

        If SensorValue(2) = 1 Then
            IrrigationMap.MSensor3.Visible = True
        Else
            IrrigationMap.MSensor3.Visible = False
        End If

        If SensorValue(3) = 1 Then
            IrrigationMap.MSensor4.Visible = True
        Else
            IrrigationMap.MSensor4.Visible = False
        End If

        If SensorValue(4) = 1 Then
            IrrigationMap.MSensor5.Visible = True
        Else
            IrrigationMap.MSensor5.Visible = False
        End If

        If SensorValue(5) = 1 Then
            IrrigationMap.MSensor6.Visible = True
        Else
            IrrigationMap.MSensor6.Visible = False
        End If

        If SensorValue(6) = 1 Then
            IrrigationMap.MSensor7.Visible = True
        Else
            IrrigationMap.MSensor7.Visible = False
        End If

        If SensorValue(7) = 1 Then
            IrrigationMap.MSensor8.Visible = True
        Else
            IrrigationMap.MSensor8.Visible = False
        End If

        Irrigation_HistoryTableAdapter.InsertQuery("106")  'Due to thread timing, this is moved into this subroutine
        ExecutionPlanInputs.lblSMLastUpdated.Text = Now.ToString
    End Sub

#End Region

#Region "Buttons"

    Private Sub btnOverrideRun_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOverrideRun.Click
        Override_ExecutionPlan_Run()

    End Sub

    Private Sub btnOverrideStop_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOverrideStop.Click
        Override_ExecutionPlan_Stop()
    End Sub

    Private Sub btnValveActivate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnValveActivate.Click
        If _VCarduino.ComIsOpen Then
            bValveControlActive = True
            btnValveActivate.Enabled = False
            btnValveDeactivate.Enabled = True
            ValveControl.BackColor = Color.LightGreen
            tCheckDB.Start()
        Else
            MsgBox("Error opening Valve Control")
        End If
    End Sub

    Private Sub btnValveDeactivate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnValveDeactivate.Click
        bValveControlActive = False
        btnValveActivate.Enabled = True
        btnValveDeactivate.Enabled = False
        tCheckDB.Stop()
    End Sub

    Private Sub btnShowCOMControls_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnShowCOMControls.Click
        COMControl.ShowDialog()
    End Sub

    Private Sub btnShowWeatherForecast_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnShowWeatherForecast.Click
        ExecutionPlanInputs.ShowDialog()
    End Sub

    Private Sub btnValveControl_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnValveControl.Click
        ValveControl.ShowDialog()
    End Sub

    Private Sub btnShowMap_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnShowMap.Click
        CheckSoilMoisture()  'Also triggers ExecutionPlanUpdate
        IrrigationMap.ShowDialog()
    End Sub

    Private Sub btnMeterStart_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMeterStart.Click
        tMeterRefresh.Start()
        AddHandler _VCarduino.DigitalDataReceived, AddressOf _VCarduino_DigitalDataReceived
        btnMeterStop.Enabled = True
        btnMeterStart.Enabled = False
    End Sub

    Private Sub btnMeterStop_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMeterStop.Click
        tMeterRefresh.Stop()
        RemoveHandler _VCarduino.DigitalDataReceived, AddressOf _VCarduino_DigitalDataReceived
        btnMeterStop.Enabled = False
        btnMeterStart.Enabled = True
    End Sub

#End Region

#Region "Weather Update"

    Private Function UpdateWeather2() As Boolean
        Dim Request_5DayForecast As System.Net.HttpWebRequest
        Dim Response_5DayForecast As System.Net.HttpWebResponse
        Dim Reader_5DayForecast As System.IO.StreamReader

        Try
            Request_5DayForecast = System.Net.HttpWebRequest.Create("http://dataservice.accuweather.com//forecasts/v1/daily/5day/2191041?apikey=yHx45QCMqBBk7jAGs6QZhtbqHfHCsNxn&details=true")
            With Request_5DayForecast
                .Method = "GET"
                .ContentType = "application/json"
                .Accept = "application/json"
            End With
            Response_5DayForecast = Request_5DayForecast.GetResponse()
            Reader_5DayForecast = New System.IO.StreamReader(Response_5DayForecast.GetResponseStream)

            Dim json As String = Reader_5DayForecast.ReadToEnd
            'Dim json = "{'Headline':{'EffectiveDate':'2019-05-16T19:00:00+04:00','EffectiveEpochDate':1558018800,'Severity':7,'Text':'Warm Thursday night','Category':'heat','EndDate':'2019-05-17T07:00:00+04:00','EndEpochDate':1558062000,'MobileLink':'http://m.accuweather.com/en/ae/al-mateena/323066/extended-weather-forecast/323066?unit=c&lang=en-us','Link':'http://www.accuweather.com/en/ae/al-mateena/323066/daily-weather-forecast/323066?unit=c&lang=en-us'},'DailyForecasts':[{'Date':'2019-05-15T07:00:00+04:00','EpochDate':1557889200,'Sun':{'Rise':'2019-05-15T05:34:00+04:00','EpochRise':1557884040,'Set':'2019-05-15T18:56:00+04:00','EpochSet':1557932160},'Moon':{'Rise':'2019-05-15T15:31:00+04:00','EpochRise':1557919860,'Set':'2019-05-16T03:52:00+04:00','EpochSet':1557964320,'Phase':'WaxingGibbous','Age':11},'Temperature':{'Minimum':{'Value':25.3,'Unit':'C','UnitType':17},'Maximum':{'Value':36.0,'Unit':'C','UnitType':17}},'RealFeelTemperature':{'Minimum':{'Value':25.7,'Unit':'C','UnitType':17},'Maximum':{'Value':38.3,'Unit':'C','UnitType':17}},'RealFeelTemperatureShade':{'Minimum':{'Value':25.7,'Unit':'C','UnitType':17},'Maximum':{'Value':34.3,'Unit':'C','UnitType':17}},'HoursOfSun':13.4,'DegreeDaySummary':{'Heating':{'Value':0.0,'Unit':'C','UnitType':17},'Cooling':{'Value':13.0,'Unit':'C','UnitType':17}},'AirAndPollen':[{'Name':'AirQuality','Value':0,'Category':'Good','CategoryValue':1,'Type':'Ozone'},{'Name':'Grass','Value':0,'Category':'Low','CategoryValue':1},{'Name':'Mold','Value':0,'Category':'Low','CategoryValue':1},{'Name':'Ragweed','Value':0,'Category':'Low','CategoryValue':1},{'Name':'Tree','Value':0,'Category':'Low','CategoryValue':1},{'Name':'UVIndex','Value':12,'Category':'Extreme','CategoryValue':5}],'Day':{'Icon':1,'IconPhrase':'Sunny','ShortPhrase':'Plenty of sunshine','LongPhrase':'Plenty of sunshine','PrecipitationProbability':0,'ThunderstormProbability':0,'RainProbability':0,'SnowProbability':0,'IceProbability':0,'Wind':{'Speed':{'Value':14.8,'Unit':'km/h','UnitType':7},'Direction':{'Degrees':260,'Localized':'W','English':'W'}},'WindGust':{'Speed':{'Value':20.4,'Unit':'km/h','UnitType':7},'Direction':{'Degrees':224,'Localized':'SW','English':'SW'}},'TotalLiquid':{'Value':0.0,'Unit':'mm','UnitType':3},'Rain':{'Value':0.0,'Unit':'mm','UnitType':3},'Snow':{'Value':0.0,'Unit':'cm','UnitType':4},'Ice':{'Value':0.0,'Unit':'mm','UnitType':3},'HoursOfPrecipitation':0.0,'HoursOfRain':0.0,'HoursOfSnow':0.0,'HoursOfIce':0.0,'CloudCover':0},'Night':{'Icon':33,'IconPhrase':'Clear','ShortPhrase':'Clear','LongPhrase':'Clear','PrecipitationProbability':0,'ThunderstormProbability':0,'RainProbability':0,'SnowProbability':0,'IceProbability':0,'Wind':{'Speed':{'Value':3.7,'Unit':'km/h','UnitType':7},'Direction':{'Degrees':202,'Localized':'SSW','English':'SSW'}},'WindGust':{'Speed':{'Value':16.7,'Unit':'km/h','UnitType':7},'Direction':{'Degrees':263,'Localized':'W','English':'W'}},'TotalLiquid':{'Value':0.0,'Unit':'mm','UnitType':3},'Rain':{'Value':0.0,'Unit':'mm','UnitType':3},'Snow':{'Value':0.0,'Unit':'cm','UnitType':4},'Ice':{'Value':0.0,'Unit':'mm','UnitType':3},'HoursOfPrecipitation':0.0,'HoursOfRain':0.0,'HoursOfSnow':0.0,'HoursOfIce':0.0,'CloudCover':0},'Sources':['AccuWeather'],'MobileLink':'http://m.accuweather.com/en/ae/al-mateena/323066/daily-weather-forecast/323066?day=1&unit=c&lang=en-us','Link':'http://www.accuweather.com/en/ae/al-mateena/323066/daily-weather-forecast/323066?day=1&unit=c&lang=en-us'},{'Date':'2019-05-16T07:00:00+04:00','EpochDate':1557975600,'Sun':{'Rise':'2019-05-16T05:34:00+04:00','EpochRise':1557970440,'Set':'2019-05-16T18:57:00+04:00','EpochSet':1558018620},'Moon':{'Rise':'2019-05-16T16:32:00+04:00','EpochRise':1558009920,'Set':'2019-05-17T04:32:00+04:00','EpochSet':1558053120,'Phase':'WaxingGibbous','Age':12},'Temperature':{'Minimum':{'Value':27.1,'Unit':'C','UnitType':17},'Maximum':{'Value':37.0,'Unit':'C','UnitType':17}},'RealFeelTemperature':{'Minimum':{'Value':26.8,'Unit':'C','UnitType':17},'Maximum':{'Value':39.2,'Unit':'C','UnitType':17}},'RealFeelTemperatureShade':{'Minimum':{'Value':26.8,'Unit':'C','UnitType':17},'Maximum':{'Value':35.3,'Unit':'C','UnitType':17}},'HoursOfSun':13.4,'DegreeDaySummary':{'Heating':{'Value':0.0,'Unit':'C','UnitType':17},'Cooling':{'Value':14.0,'Unit':'C','UnitType':17}},'AirAndPollen':[{'Name':'AirQuality','Value':0,'Category':'Good','CategoryValue':1,'Type':'Ozone'},{'Name':'Grass','Value':0,'Category':'Low','CategoryValue':1},{'Name':'Mold','Value':0,'Category':'Low','CategoryValue':1},{'Name':'Ragweed','Value':0,'Category':'Low','CategoryValue':1},{'Name':'Tree','Value':0,'Category':'Low','CategoryValue':1},{'Name':'UVIndex','Value':12,'Category':'Extreme','CategoryValue':5}],'Day':{'Icon':1,'IconPhrase':'Sunny','ShortPhrase':'Plenty of sunshine','LongPhrase':'Plenty of sunshine','PrecipitationProbability':0,'ThunderstormProbability':0,'RainProbability':0,'SnowProbability':0,'IceProbability':0,'Wind':{'Speed':{'Value':9.3,'Unit':'km/h','UnitType':7},'Direction':{'Degrees':272,'Localized':'W','English':'W'}},'WindGust':{'Speed':{'Value':22.2,'Unit':'km/h','UnitType':7},'Direction':{'Degrees':335,'Localized':'NNW','English':'NNW'}},'TotalLiquid':{'Value':0.0,'Unit':'mm','UnitType':3},'Rain':{'Value':0.0,'Unit':'mm','UnitType':3},'Snow':{'Value':0.0,'Unit':'cm','UnitType':4},'Ice':{'Value':0.0,'Unit':'mm','UnitType':3},'HoursOfPrecipitation':0.0,'HoursOfRain':0.0,'HoursOfSnow':0.0,'HoursOfIce':0.0,'CloudCover':0},'Night':{'Icon':33,'IconPhrase':'Clear','ShortPhrase':'Clear; very warm','LongPhrase':'Clear; very warm','PrecipitationProbability':0,'ThunderstormProbability':0,'RainProbability':0,'SnowProbability':0,'IceProbability':0,'Wind':{'Speed':{'Value':5.6,'Unit':'km/h','UnitType':7},'Direction':{'Degrees':71,'Localized':'ENE','English':'ENE'}},'WindGust':{'Speed':{'Value':14.8,'Unit':'km/h','UnitType':7},'Direction':{'Degrees':341,'Localized':'NNW','English':'NNW'}},'TotalLiquid':{'Value':0.0,'Unit':'mm','UnitType':3},'Rain':{'Value':0.0,'Unit':'mm','UnitType':3},'Snow':{'Value':0.0,'Unit':'cm','UnitType':4},'Ice':{'Value':0.0,'Unit':'mm','UnitType':3},'HoursOfPrecipitation':0.0,'HoursOfRain':0.0,'HoursOfSnow':0.0,'HoursOfIce':0.0,'CloudCover':0},'Sources':['AccuWeather'],'MobileLink':'http://m.accuweather.com/en/ae/al-mateena/323066/daily-weather-forecast/323066?day=2&unit=c&lang=en-us','Link':'http://www.accuweather.com/en/ae/al-mateena/323066/daily-weather-forecast/323066?day=2&unit=c&lang=en-us'},{'Date':'2019-05-17T07:00:00+04:00','EpochDate':1558062000,'Sun':{'Rise':'2019-05-17T05:33:00+04:00','EpochRise':1558056780,'Set':'2019-05-17T18:57:00+04:00','EpochSet':1558105020},'Moon':{'Rise':'2019-05-17T17:33:00+04:00','EpochRise':1558099980,'Set':'2019-05-18T05:13:00+04:00','EpochSet':1558141980,'Phase':'WaxingGibbous','Age':13},'Temperature':{'Minimum':{'Value':27.8,'Unit':'C','UnitType':17},'Maximum':{'Value':38.6,'Unit':'C','UnitType':17}},'RealFeelTemperature':{'Minimum':{'Value':27.0,'Unit':'C','UnitType':17},'Maximum':{'Value':40.4,'Unit':'C','UnitType':17}},'RealFeelTemperatureShade':{'Minimum':{'Value':27.0,'Unit':'C','UnitType':17},'Maximum':{'Value':36.9,'Unit':'C','UnitType':17}},'HoursOfSun':12.8,'DegreeDaySummary':{'Heating':{'Value':0.0,'Unit':'C','UnitType':17},'Cooling':{'Value':15.0,'Unit':'C','UnitType':17}},'AirAndPollen':[{'Name':'AirQuality','Value':0,'Category':'Good','CategoryValue':1,'Type':'Ozone'},{'Name':'Grass','Value':0,'Category':'Low','CategoryValue':1},{'Name':'Mold','Value':0,'Category':'Low','CategoryValue':1},{'Name':'Ragweed','Value':0,'Category':'Low','CategoryValue':1},{'Name':'Tree','Value':0,'Category':'Low','CategoryValue':1},{'Name':'UVIndex','Value':12,'Category':'Extreme','CategoryValue':5}],'Day':{'Icon':1,'IconPhrase':'Sunny','ShortPhrase':'Plenty of sunshine; very warm','LongPhrase':'Plenty of sunshine; very warm','PrecipitationProbability':0,'ThunderstormProbability':0,'RainProbability':0,'SnowProbability':0,'IceProbability':0,'Wind':{'Speed':{'Value':13.0,'Unit':'km/h','UnitType':7},'Direction':{'Degrees':9,'Localized':'N','English':'N'}},'WindGust':{'Speed':{'Value':25.9,'Unit':'km/h','UnitType':7},'Direction':{'Degrees':343,'Localized':'NNW','English':'NNW'}},'TotalLiquid':{'Value':0.0,'Unit':'mm','UnitType':3},'Rain':{'Value':0.0,'Unit':'mm','UnitType':3},'Snow':{'Value':0.0,'Unit':'cm','UnitType':4},'Ice':{'Value':0.0,'Unit':'mm','UnitType':3},'HoursOfPrecipitation':0.0,'HoursOfRain':0.0,'HoursOfSnow':0.0,'HoursOfIce':0.0,'CloudCover':1},'Night':{'Icon':34,'IconPhrase':'Mostly clear','ShortPhrase':'Mainly clear; very warm','LongPhrase':'Mainly clear; very warm','PrecipitationProbability':0,'ThunderstormProbability':0,'RainProbability':0,'SnowProbability':0,'IceProbability':0,'Wind':{'Speed':{'Value':9.3,'Unit':'km/h','UnitType':7},'Direction':{'Degrees':79,'Localized':'E','English':'E'}},'WindGust':{'Speed':{'Value':16.7,'Unit':'km/h','UnitType':7},'Direction':{'Degrees':15,'Localized':'NNE','English':'NNE'}},'TotalLiquid':{'Value':0.0,'Unit':'mm','UnitType':3},'Rain':{'Value':0.0,'Unit':'mm','UnitType':3},'Snow':{'Value':0.0,'Unit':'cm','UnitType':4},'Ice':{'Value':0.0,'Unit':'mm','UnitType':3},'HoursOfPrecipitation':0.0,'HoursOfRain':0.0,'HoursOfSnow':0.0,'HoursOfIce':0.0,'CloudCover':16},'Sources':['AccuWeather'],'MobileLink':'http://m.accuweather.com/en/ae/al-mateena/323066/daily-weather-forecast/323066?day=3&unit=c&lang=en-us','Link':'http://www.accuweather.com/en/ae/al-mateena/323066/daily-weather-forecast/323066?day=3&unit=c&lang=en-us'},{'Date':'2019-05-18T07:00:00+04:00','EpochDate':1558148400,'Sun':{'Rise':'2019-05-18T05:33:00+04:00','EpochRise':1558143180,'Set':'2019-05-18T18:58:00+04:00','EpochSet':1558191480},'Moon':{'Rise':'2019-05-18T18:34:00+04:00','EpochRise':1558190040,'Set':'2019-05-19T05:56:00+04:00','EpochSet':1558230960,'Phase':'Full','Age':14},'Temperature':{'Minimum':{'Value':28.7,'Unit':'C','UnitType':17},'Maximum':{'Value':38.2,'Unit':'C','UnitType':17}},'RealFeelTemperature':{'Minimum':{'Value':28.3,'Unit':'C','UnitType':17},'Maximum':{'Value':40.8,'Unit':'C','UnitType':17}},'RealFeelTemperatureShade':{'Minimum':{'Value':28.3,'Unit':'C','UnitType':17},'Maximum':{'Value':36.7,'Unit':'C','UnitType':17}},'HoursOfSun':6.0,'DegreeDaySummary':{'Heating':{'Value':0.0,'Unit':'C','UnitType':17},'Cooling':{'Value':15.0,'Unit':'C','UnitType':17}},'AirAndPollen':[{'Name':'AirQuality','Value':0,'Category':'Good','CategoryValue':1,'Type':'Ozone'},{'Name':'Grass','Value':0,'Category':'Low','CategoryValue':1},{'Name':'Mold','Value':0,'Category':'Low','CategoryValue':1},{'Name':'Ragweed','Value':0,'Category':'Low','CategoryValue':1},{'Name':'Tree','Value':0,'Category':'Low','CategoryValue':1},{'Name':'UVIndex','Value':12,'Category':'Extreme','CategoryValue':5}],'Day':{'Icon':4,'IconPhrase':'Intermittent clouds','ShortPhrase':'Clouds and sun; very warm','LongPhrase':'Times of sun and clouds; very warm','PrecipitationProbability':0,'ThunderstormProbability':0,'RainProbability':0,'SnowProbability':0,'IceProbability':0,'Wind':{'Speed':{'Value':14.8,'Unit':'km/h','UnitType':7},'Direction':{'Degrees':8,'Localized':'N','English':'N'}},'WindGust':{'Speed':{'Value':25.9,'Unit':'km/h','UnitType':7},'Direction':{'Degrees':331,'Localized':'NNW','English':'NNW'}},'TotalLiquid':{'Value':0.0,'Unit':'mm','UnitType':3},'Rain':{'Value':0.0,'Unit':'mm','UnitType':3},'Snow':{'Value':0.0,'Unit':'cm','UnitType':4},'Ice':{'Value':0.0,'Unit':'mm','UnitType':3},'HoursOfPrecipitation':0.0,'HoursOfRain':0.0,'HoursOfSnow':0.0,'HoursOfIce':0.0,'CloudCover':65},'Night':{'Icon':35,'IconPhrase':'Partly cloudy','ShortPhrase':'Partly cloudy; very warm','LongPhrase':'Partly cloudy; very warm','PrecipitationProbability':0,'ThunderstormProbability':0,'RainProbability':0,'SnowProbability':0,'IceProbability':0,'Wind':{'Speed':{'Value':7.4,'Unit':'km/h','UnitType':7},'Direction':{'Degrees':215,'Localized':'SW','English':'SW'}},'WindGust':{'Speed':{'Value':20.4,'Unit':'km/h','UnitType':7},'Direction':{'Degrees':316,'Localized':'NW','English':'NW'}},'TotalLiquid':{'Value':0.0,'Unit':'mm','UnitType':3},'Rain':{'Value':0.0,'Unit':'mm','UnitType':3},'Snow':{'Value':0.0,'Unit':'cm','UnitType':4},'Ice':{'Value':0.0,'Unit':'mm','UnitType':3},'HoursOfPrecipitation':0.0,'HoursOfRain':0.0,'HoursOfSnow':0.0,'HoursOfIce':0.0,'CloudCover':77},'Sources':['AccuWeather'],'MobileLink':'http://m.accuweather.com/en/ae/al-mateena/323066/daily-weather-forecast/323066?day=4&unit=c&lang=en-us','Link':'http://www.accuweather.com/en/ae/al-mateena/323066/daily-weather-forecast/323066?day=4&unit=c&lang=en-us'},{'Date':'2019-05-19T07:00:00+04:00','EpochDate':1558234800,'Sun':{'Rise':'2019-05-19T05:32:00+04:00','EpochRise':1558229520,'Set':'2019-05-19T18:58:00+04:00','EpochSet':1558277880},'Moon':{'Rise':'2019-05-19T19:35:00+04:00','EpochRise':1558280100,'Set':'2019-05-20T06:42:00+04:00','EpochSet':1558320120,'Phase':'WaningGibbous','Age':15},'Temperature':{'Minimum':{'Value':29.0,'Unit':'C','UnitType':17},'Maximum':{'Value':38.0,'Unit':'C','UnitType':17}},'RealFeelTemperature':{'Minimum':{'Value':29.0,'Unit':'C','UnitType':17},'Maximum':{'Value':41.0,'Unit':'C','UnitType':17}},'RealFeelTemperatureShade':{'Minimum':{'Value':29.0,'Unit':'C','UnitType':17},'Maximum':{'Value':36.6,'Unit':'C','UnitType':17}},'HoursOfSun':7.0,'DegreeDaySummary':{'Heating':{'Value':0.0,'Unit':'C','UnitType':17},'Cooling':{'Value':15.0,'Unit':'C','UnitType':17}},'AirAndPollen':[{'Name':'AirQuality','Value':0,'Category':'Good','CategoryValue':1,'Type':'Ozone'},{'Name':'Grass','Value':0,'Category':'Low','CategoryValue':1},{'Name':'Mold','Value':0,'Category':'Low','CategoryValue':1},{'Name':'Ragweed','Value':0,'Category':'Low','CategoryValue':1},{'Name':'Tree','Value':0,'Category':'Low','CategoryValue':1},{'Name':'UVIndex','Value':12,'Category':'Extreme','CategoryValue':5}],'Day':{'Icon':4,'IconPhrase':'Intermittent clouds','ShortPhrase':'Clouds and sun; very warm','LongPhrase':'A blend of sun and clouds; very warm','PrecipitationProbability':0,'ThunderstormProbability':0,'RainProbability':0,'SnowProbability':0,'IceProbability':0,'Wind':{'Speed':{'Value':13.0,'Unit':'km/h','UnitType':7},'Direction':{'Degrees':285,'Localized':'WNW','English':'WNW'}},'WindGust':{'Speed':{'Value':25.9,'Unit':'km/h','UnitType':7},'Direction':{'Degrees':329,'Localized':'NNW','English':'NNW'}},'TotalLiquid':{'Value':0.0,'Unit':'mm','UnitType':3},'Rain':{'Value':0.0,'Unit':'mm','UnitType':3},'Snow':{'Value':0.0,'Unit':'cm','UnitType':4},'Ice':{'Value':0.0,'Unit':'mm','UnitType':3},'HoursOfPrecipitation':0.0,'HoursOfRain':0.0,'HoursOfSnow':0.0,'HoursOfIce':0.0,'CloudCover':59},'Night':{'Icon':35,'IconPhrase':'Partly cloudy','ShortPhrase':'Partly cloudy; very warm','LongPhrase':'Partly cloudy; very warm','PrecipitationProbability':1,'ThunderstormProbability':0,'RainProbability':1,'SnowProbability':0,'IceProbability':0,'Wind':{'Speed':{'Value':9.3,'Unit':'km/h','UnitType':7},'Direction':{'Degrees':25,'Localized':'NNE','English':'NNE'}},'WindGust':{'Speed':{'Value':20.4,'Unit':'km/h','UnitType':7},'Direction':{'Degrees':333,'Localized':'NNW','English':'NNW'}},'TotalLiquid':{'Value':0.0,'Unit':'mm','UnitType':3},'Rain':{'Value':0.0,'Unit':'mm','UnitType':3},'Snow':{'Value':0.0,'Unit':'cm','UnitType':4},'Ice':{'Value':0.0,'Unit':'mm','UnitType':3},'HoursOfPrecipitation':0.0,'HoursOfRain':0.0,'HoursOfSnow':0.0,'HoursOfIce':0.0,'CloudCover':56},'Sources':['AccuWeather'],'MobileLink':'http://m.accuweather.com/en/ae/al-mateena/323066/daily-weather-forecast/323066?day=5&unit=c&lang=en-us','Link':'http://www.accuweather.com/en/ae/al-mateena/323066/daily-weather-forecast/323066?day=5&unit=c&lang=en-us'}]}"
            Dim myAccuWeather As AccuWeather = Newtonsoft.Json.JsonConvert.DeserializeObject(Of AccuWeather)(json)
            Dim TodayLow As Double = myAccuWeather.DailyForecasts(0).Temperature.Minimum.Value
            Dim TomorrowLow As Double = myAccuWeather.DailyForecasts(1).Temperature.Minimum.Value
            Dim TodayPrecipPct As Integer = myAccuWeather.DailyForecasts(0).Night.RainProbability.ToString
            Dim TodayPrecipAmt As Double = myAccuWeather.DailyForecasts(0).Day.Rain.Value + myAccuWeather.DailyForecasts(0).Night.Rain.Value
            Dim TomorrowPrecipPct As Integer = myAccuWeather.DailyForecasts(1).Night.RainProbability.ToString
            Dim TomorrowPrecipAmt As Double = myAccuWeather.DailyForecasts(1).Day.Rain.Value + myAccuWeather.DailyForecasts(1).Night.Rain.Value

            ExecutionPlanInputs.lblTodayLow.Text = TodayLow.ToString
            ExecutionPlanInputs.lblTomorrowLow.Text = TomorrowLow.ToString

            ExecutionPlanInputs.lblPrecipTodayPct.Text = TodayPrecipPct.ToString
            ExecutionPlanInputs.lblPrecipTodayAmt.Text = TodayPrecipAmt.ToString
            ExecutionPlanInputs.lblPrecipTomorrowPct.Text = TomorrowPrecipPct.ToString
            ExecutionPlanInputs.lblPrecipTomorrowAmt.Text = TomorrowPrecipAmt.ToString

            Irrigation_HistoryTableAdapter.InsertQuery("108")
            ExecutionPlanInputs.lblWLastUpdated.Text = Now.ToString

            '''''''''''''''
            'Check weather to determine on/off
            If TodayPrecipPct >= MIN_PCT_PRECIP Or TomorrowPrecipPct >= MIN_PCT_PRECIP Or TodayLow < MIN_TEMP Or TomorrowLow < MIN_TEMP Then
                If TodayPrecipAmt >= 0.25 Or TomorrowPrecipAmt >= 0.25 Then
                    bWeatherBad = True
                Else
                    bWeatherBad = False
                End If
            Else
                bWeatherBad = False
            End If


            Return True
        Catch e As Exception
            'On error, use last known weather report - do not update bWeatherBad
            Irrigation_HistoryTableAdapter.InsertQuery("109")
            'MsgBox("AccuWeather error: " + e.Message)

            Return False
        End Try

    End Function
    Private Function UpdateWeather() As Boolean

        Try
            Dim request = New ForecastIORequest("95a75eee24600535ad7515bbad576c89", 39.367003, -84.475208, Unit.us)
            Dim response = request.Get()
            'tSleep.SynchronizingObject = Me

            'MsgBox(Now.ToString)
            ExecutionPlanInputs.lblTodayLow.Text = response.daily.data.Item(0).temperatureMin
            ExecutionPlanInputs.lblTomorrowLow.Text = response.daily.data.Item(1).temperatureMin

            ExecutionPlanInputs.lblPrecipTodayPct.Text = response.daily.data.Item(0).precipProbability * 100
            ExecutionPlanInputs.lblPrecipTomorrowPct.Text = response.daily.data.Item(1).precipProbability * 100
            'Throw New System.Exception("Test")

            Irrigation_HistoryTableAdapter.InsertQuery("108")
            ExecutionPlanInputs.lblWLastUpdated.Text = Now.ToString

            'MsgBox(Now.ToString)

            '''''''''''''''
            'Check weather to determine on/off
            If Int(ExecutionPlanInputs.lblPrecipTodayPct.Text) >= MIN_PCT_PRECIP Or Int(ExecutionPlanInputs.lblPrecipTomorrowPct.Text) >= MIN_PCT_PRECIP Or Int(ExecutionPlanInputs.lblTodayLow.Text) < MIN_TEMP Or Int(ExecutionPlanInputs.lblTomorrowLow.Text) < MIN_TEMP Then
                bWeatherBad = True
            Else
                bWeatherBad = False
            End If


            Return True
        Catch e As Exception
            'On error, use last known weather report - do not update bWeatherBad
            Irrigation_HistoryTableAdapter.InsertQuery("109")
            'MsgBox("Forecast.io error")

            Return False

        End Try

    End Function

    Function CheckAltWeather()
        Dim strPrecipToday, strPrecipTomorrow, strData As String
        Dim strLowToday, strLowTomorrow As String
        Dim iIndex As Integer = 0
        Dim iTempIndex As Integer = 0


        Try
            Dim xmlData As New System.Net.WebClient
            xmlData.Headers(HttpRequestHeader.UserAgent) = "Greg/1.0"
            strData = xmlData.DownloadString("http://forecast.weather.gov/MapClick.php?lat=39.3665&lon=-84.4743&unit=0&lg=english&FcstType=dwml")

            ''''''
            ' Low Temp
            ''''''
            strLowToday = ""
            strLowTomorrow = ""

            iIndex = strData.IndexOf("<temperature type=""minimum")
            iIndex = strData.IndexOf("<value>", iIndex)
            iIndex = iIndex + 7
            iTempIndex = strData.IndexOf("</value>", iIndex)

            For i As Integer = iIndex To iTempIndex - 1
                strLowToday = strLowToday & strData(i)
            Next i

            iIndex = iTempIndex

            iIndex = strData.IndexOf("<value>", iIndex)
            iIndex = iIndex + 7
            iTempIndex = strData.IndexOf("</value>", iIndex)

            For i As Integer = iIndex To iTempIndex - 1
                strLowTomorrow = strLowTomorrow & strData(i)
            Next i

            ExecutionPlanInputs.lblTodayLow.Text = strLowToday
            ExecutionPlanInputs.lblTomorrowLow.Text = strLowTomorrow

            iIndex = iTempIndex

            ''''''
            ' Precipitation
            ''''''
            strPrecipToday = ""
            strPrecipTomorrow = ""

            iIndex = strData.IndexOf("probability-of-precipitation")
            iIndex = strData.IndexOf("<value", iIndex)
            iIndex = iIndex + 6
            If strData(iIndex + 1) = "x" And strData(iIndex + 2) = "s" And strData(iIndex + 3) = "i" Then   'Zero value = <value xsi:nil="true"/>
                strPrecipToday = "0"
                iTempIndex = strData.IndexOf("</value>", iIndex)
            Else
                iIndex = iIndex + 1
                iTempIndex = strData.IndexOf("</value>", iIndex)

                For i As Integer = iIndex To iTempIndex - 1
                    strPrecipToday = strPrecipToday & strData(i)
                Next i
            End If

            iIndex = iTempIndex

            iIndex = strData.IndexOf("<value", iIndex)
            iIndex = iIndex + 6
            If strData(iIndex + 1) = "x" And strData(iIndex + 2) = "s" And strData(iIndex + 3) = "i" Then   'Zero value = <value xsi:nil="true"/>
                strPrecipTomorrow = "0"
                iTempIndex = strData.IndexOf("</value>", iIndex)
            Else
                iIndex = iIndex + 1
                iTempIndex = strData.IndexOf("</value>", iIndex)

                For i As Integer = iIndex To iTempIndex - 1
                    strPrecipTomorrow = strPrecipTomorrow & strData(i)
                Next i
            End If

            ExecutionPlanInputs.lblPrecipTodayPct.Text = strPrecipToday
            ExecutionPlanInputs.lblPrecipTomorrowPct.Text = strPrecipTomorrow

            Irrigation_HistoryTableAdapter.InsertQuery("110")
            '''''''''''''''
            'Check weather to determine on/off
            If Int(ExecutionPlanInputs.lblPrecipTodayPct.Text) >= MIN_PCT_PRECIP Or Int(ExecutionPlanInputs.lblPrecipTomorrowPct.Text) >= MIN_PCT_PRECIP Or Int(ExecutionPlanInputs.lblTodayLow.Text) < MIN_TEMP Or Int(ExecutionPlanInputs.lblTomorrowLow.Text) < MIN_TEMP Then
                bWeatherBad = True
            Else
                bWeatherBad = False
            End If

            ExecutionPlanInputs.lblWLastUpdated.Text = Now.ToString
            Return True

        Catch e As Exception
            'On error, use last known weather report - do not update bWeatherBad
            Irrigation_HistoryTableAdapter.InsertQuery("111")

            Return False

        End Try

    End Function

#End Region

#Region "Water Meter"
    Private Sub _VCarduino_DigitalDataReceived(ByVal DPortNr As Integer, ByVal Value As Integer) Handles _VCarduino.DigitalDataReceived
        If DPortNr = 50 And Value = 49 Then
            iGallonsUsed = iGallonsUsed + 1
            'My.Application.DoEvents()
        End If
    End Sub

    Private Sub SaveGallonsUsed()
        Dim dtLastUpdate As Date
        Dim tabMostRecentValue As WatchdogDataSet1.Water_Usage_HistoryDataTable

        tabMostRecentValue = New WatchdogDataSet1.Water_Usage_HistoryDataTable
        tabMostRecentValue = Water_Usage_HistoryTableAdapter.GetDataBy_MostRecentValue()
        If tabMostRecentValue.Rows.Count = 1 Then  'If the table is not empty
            dtLastUpdate = tabMostRecentValue.Rows(0).ItemArray(0)

            If DatePart(DateInterval.Day, dtLastUpdate) = DatePart(DateInterval.Day, Now) And Month(dtLastUpdate) = Month(Now) And Year(dtLastUpdate) = Year(Now) Then
                Water_Usage_HistoryTableAdapter.UpdateGallonsUsed(iGallonsUsed, dtLastUpdate)
            Else
                Water_Usage_HistoryTableAdapter.InsertGallonsUsed(iGallonsUsed)
            End If
        Else  'the table is empty
            Water_Usage_HistoryTableAdapter.InsertGallonsUsed(iGallonsUsed)
        End If
    End Sub

    Private Sub SaveIrrigationResult(iIrrigationGallons, strIrrigationCode)
        Dim dtLastUpdate As Date
        Dim tabMostRecentValue As WatchdogDataSet1.Water_Usage_HistoryDataTable

        tabMostRecentValue = New WatchdogDataSet1.Water_Usage_HistoryDataTable
        tabMostRecentValue = Water_Usage_HistoryTableAdapter.GetDataBy_MostRecentValue()
        If tabMostRecentValue.Rows.Count = 1 Then  'If the table is not empty
            dtLastUpdate = tabMostRecentValue.Rows(0).ItemArray(0)
            Water_Usage_HistoryTableAdapter.UpdateIrrigationResults(iIrrigationGallons, strIrrigationCode, dtLastUpdate)
        End If
    End Sub

    Private Function CalcIrrigationGallons(iStart, iEnd) As Integer
        Dim tabYesterdayValue As WatchdogDataSet1.Water_Usage_HistoryDataTable
        Dim iYesterdayTotal As Integer

        If iEnd >= iStart Then
            Return iEnd - iStart
        Else
            'iGallons reset in the middle of the run
            tabYesterdayValue = New WatchdogDataSet1.Water_Usage_HistoryDataTable
            tabYesterdayValue = Water_Usage_HistoryTableAdapter.FillBy_YesterdayGallons()
            If tabYesterdayValue.Rows.Count = 2 Then  'If the table is not empty
                iYesterdayTotal = tabYesterdayValue.Rows(1).ItemArray(1)
                Return (iYesterdayTotal - iStart) + iEnd
            Else
                'iYesterdayTotal = 0
                Return iEnd
            End If

        End If
    End Function

    Private Sub WaterMeterStartup()
        Dim dtLastUpdate As Date
        Dim tabMostRecentValue As WatchdogDataSet1.Water_Usage_HistoryDataTable

        tabMostRecentValue = New WatchdogDataSet1.Water_Usage_HistoryDataTable
        tabMostRecentValue = Water_Usage_HistoryTableAdapter.GetDataBy_MostRecentValue()
        If tabMostRecentValue.Rows.Count = 1 Then
            dtLastUpdate = tabMostRecentValue.Rows(0).ItemArray(0)
            If DatePart(DateInterval.Day, dtLastUpdate) = DatePart(DateInterval.Day, Now) And Month(dtLastUpdate) = Month(Now) And Year(dtLastUpdate) = Year(Now) Then
                iGallonsUsed = tabMostRecentValue.Rows(0).ItemArray(1)
            Else
                iGallonsUsed = 0
            End If
        Else
            iGallonsUsed = 0
        End If
        iOldGallonsUsed = iGallonsUsed
        iLeakCounter = 0
        pbLeakDetected.Value = 0
        lblPeakLeakDetected.Text = 0
        tLeakCheck.Start()
    End Sub

#End Region

#Region "Timers"

    Private Sub tSleep_Elapsed()
        'Frequency set by user in mtxtPolling Interval

        If bValveControlActive Then
            CheckSoilMoisture()  'Also triggers ExecutionPlanUpdate
        Else
            IrrigationMap.MSensor1.Visible = False
            IrrigationMap.MSensor2.Visible = False
            IrrigationMap.MSensor3.Visible = False
            IrrigationMap.MSensor4.Visible = False
            IrrigationMap.MSensor5.Visible = False
            IrrigationMap.MSensor6.Visible = False
            IrrigationMap.MSensor7.Visible = False
            IrrigationMap.MSensor8.Visible = False
        End If
    End Sub

    Private Sub SendEmail(strMessage As String)
        Dim mySMTPClient As SmtpClient = New SmtpClient
        Dim Message As MailMessage

        Message = New MailMessage("Watchdog@gmail.com", "gregspata@gmail.com")
        Message.Subject = "Irrigation Error"
        Message.Priority = MailPriority.High
        mySMTPClient.Host = "smtp-server.cinci.rr.com"
        mySMTPClient.Port = 25
        Message.Body = strMessage
        mySMTPClient.Send(Message)
    End Sub

    Private Function TestConnection() As Boolean
        Dim bSoilMositureCheck, bValveControlCheck, bWeatherCheck, bDatabaseCheck As Boolean
        Dim dtSMLastUpdated, dtVLastUpdated, dtDBLastUpdated, dtWLastUpdated As Date

        If bValveControlActive Then
            'Check soil moisture (updated every 120 minutes)
            dtSMLastUpdated = CDate(ExecutionPlanInputs.lblSMLastUpdated.Text)
            If dtSMLastUpdated.Year = Now.Year And dtSMLastUpdated.Month = Now.Month And dtSMLastUpdated.Day = Now.Day And dtSMLastUpdated.Hour >= (Now.Hour - 8) Then
                bSoilMositureCheck = True
            Else
                bSoilMositureCheck = False
            End If

            'Check Weather (updated every 120 minutes)
            dtWLastUpdated = CDate(ExecutionPlanInputs.lblWLastUpdated.Text)
            If dtWLastUpdated.Year = Now.Year And dtWLastUpdated.Month = Now.Month And dtWLastUpdated.Day = Now.Day And dtWLastUpdated.Hour >= (Now.Hour - 8) Then
                bWeatherCheck = True
            Else
                bWeatherCheck = False
            End If

            'Check Database (updated every 6 seconds)
            dtDBLastUpdated = CDate(lblLastUpdated.Text)
            If dtDBLastUpdated.Year = Now.Year And dtDBLastUpdated.Month = Now.Month And dtDBLastUpdated.Day = Now.Day And dtDBLastUpdated.Hour = Now.Hour And dtDBLastUpdated.Minute >= (Now.Minute - 2) Then
                bDatabaseCheck = True
            Else
                bDatabaseCheck = False
            End If
        Else
            bSoilMositureCheck = True
            bWeatherCheck = True
            bDatabaseCheck = True
        End If

        'Check valve control (updated every 1 minutes)
        dtVLastUpdated = CDate(ValveControl.lblLastChecked.Text)
        If dtVLastUpdated.Year = Now.Year And dtVLastUpdated.Month = Now.Month And dtVLastUpdated.Day = Now.Day And dtVLastUpdated.Hour = Now.Hour And dtVLastUpdated.Minute >= (Now.Minute - 2) Then
            bValveControlCheck = True
        Else
            bValveControlCheck = False
        End If

        If bSoilMositureCheck And bValveControlCheck And bWeatherCheck And bDatabaseCheck Then
            Return True
        Else
            Return False
        End If

    End Function


    Private Sub tHourly_Tick(sender As Object, e As EventArgs) Handles tHourly.Tick
        'Runs once per hour

        If Now.Hour = 10 Then
            If Not TestConnection() Then
                SendEmail("WDIrrigationControl Error Detected - manual intervention required.")
                Irrigation_HistoryTableAdapter.InsertQuery("998")
                Event_HistoryTableAdapter1.InsertQuery("9916")
            End If
        End If
    End Sub

    Private Sub tDaily_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tDaily.Tick
        tDaily.Interval = 86400000  '24 hrs x 60 min x 60 sec x 1000 msec
        CheckMonth()

        'Reset Peak Leak Detected monthly
        If DatePart(DateInterval.Day, Now) = 1 Then
            lblPeakLeakDetected.Text = "0"
        End If

    End Sub

    Private Sub tMeterRefresh_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tMeterRefresh.Tick
        'Timer ticks every 6 seconds

        'At midnight, reset total
        If Hour(Now) = 0 And Minute(Now) = 0 And Second(Now) >= 0 And Second(Now) <= 5 Then
            iGallonsUsed = 0
            iOldGallonsUsed = 0
        End If

        'Save every hour on the hour
        If Minute(Now) = 0 And Second(Now) >= 0 And Second(Now) <= 5 Then
            SaveGallonsUsed()
        End If

        lblGallonsUsed.Text = iGallonsUsed.ToString
    End Sub

    Private Sub tValveControl_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tValveControl.Tick
        'Runs once per minute
        tValveControl.Interval = 60000

        Try
            ValveControl.lblLastChecked.Text = Now.ToString
            If lblStartTime.Text.Substring(2, 1) <> ":" Then
                lblStartTime.Text = "0" & lblStartTime.Text
            End If
            If Hour(Now) = Int(lblStartTime.Text.Substring(0, 2)) And Minute(Now) = Int(lblStartTime.Text.Substring(3, 2)) Then
                'It's time to start
                If bValveControlActive Then
                    CheckSoilMoisture()  'Also triggers ExecutionPlanUpdate

                    If bOverride_Stop Then  'Manual Stop
                        bRunning = False
                        Irrigation_HistoryTableAdapter.InsertQuery("318")  'Override - Stop
                        SaveIrrigationResult(0, "318")
                        bOverride_Run = False
                        bOverride_Stop = False
                        btnOverrideRun.Enabled = True
                        btnOverrideStop.Enabled = True
                        lblRainToday.Text = "No"
                        Irrigation_HistoryTableAdapter.InsertQuery("308")  'Override Disabled
                    ElseIf bOverride_Run Then  'Manual Run
                        bRunning = True
                        Irrigation_HistoryTableAdapter.InsertQuery("307")  'Override - Run
                        iActiveZone = FindNextActiveZone(0)
                        Irrigation_HistoryTableAdapter.InsertQuery("298")  'Start Irrigation Cycle
                        Irrigation_ControlTableAdapter.Request_State_Change(1, 0)  'Master On
                        iIrrigationGallons_Start = iGallonsUsed
                    ElseIf lblRainToday.Text = "Yes" Then  'Automatic Stop
                        'Soil was wet today, so no need to run
                        Irrigation_HistoryTableAdapter.InsertQuery("319")  'Ground Wet
                        SaveIrrigationResult(0, "319")
                        bRunning = False
                        bOverride_Run = False
                        bOverride_Stop = False
                        btnOverrideRun.Enabled = True
                        btnOverrideStop.Enabled = True
                        lblRainToday.Text = "No"
                    ElseIf Not (bExecutionPlan) And lblWetInLast2Days.Text = "No" And Int(ExecutionPlanInputs.lblPrecipTodayPct.Text) <= 80 And Int(ExecutionPlanInputs.lblTodayLow.Text) > 40 And Int(ExecutionPlanInputs.lblTomorrowLow.Text) > 40 Then  'Automatic Run
                        'It's been dry for 2 days - need to water badly, so increase weather threshold and override if necessary
                        Irrigation_HistoryTableAdapter.InsertQuery("307")  'Override - Run
                        bRunning = True
                        iActiveZone = FindNextActiveZone(0)
                        Irrigation_HistoryTableAdapter.InsertQuery("298")  'Start Irrigation Cycle
                        Irrigation_ControlTableAdapter.Request_State_Change(1, 0)  'Master On
                        iIrrigationGallons_Start = iGallonsUsed
                    ElseIf bExecutionPlan Then  'No override exceptions - perform normal evaluation
                        'Not running, Execution Plans says its ok to run, and system is Activated
                        bRunning = True
                        iActiveZone = FindNextActiveZone(0)
                        Irrigation_HistoryTableAdapter.InsertQuery("298")  'Start Irrigation Cycle
                        Irrigation_ControlTableAdapter.Request_State_Change(1, 0)  'Master On
                        iIrrigationGallons_Start = iGallonsUsed
                    Else
                        bRunning = False
                        Irrigation_HistoryTableAdapter.InsertQuery("302")  'Irrigation Cancelled - Weather
                        SaveIrrigationResult(0, "302")
                        bOverride_Run = False
                        bOverride_Stop = False
                        btnOverrideRun.Enabled = True
                        btnOverrideStop.Enabled = True
                        lblRainToday.Text = "No"
                    End If
                Else
                    Irrigation_HistoryTableAdapter.InsertQuery("301")  'Irrigation Cancelled - Deactivated
                    SaveIrrigationResult(0, "301")
                    bRunning = False
                    bOverride_Run = False
                    bOverride_Stop = False
                    btnOverrideRun.Enabled = True
                    btnOverrideStop.Enabled = True
                    lblRainToday.Text = "No"
                End If
            End If

            If bRunning Then
                'Running
                Select Case (iActiveZone)
                    Case 0  'Run complete
                        bRunning = False

                        Irrigation_ControlTableAdapter.Request_State_Change(0, 0)  'Master Off

                        lblIrrigationStatus.Text = "Stopped"
                        Irrigation_HistoryTableAdapter.InsertQuery("299")  'Irrigation Cycle Stopped
                        If bOverride_Run Then
                            Irrigation_HistoryTableAdapter.InsertQuery("308")  'Override Disabled
                            bOverride_Run = False
                        End If
                        SaveIrrigationResult(CalcIrrigationGallons(iIrrigationGallons_Start, iGallonsUsed), "900")
                        Irrigation_HistoryTableAdapter.InsertQuery("310") 'Need to force moisture found event to stop Override tomorrow
                        bOverride_Stop = False
                        btnOverrideRun.Enabled = True
                        btnOverrideStop.Enabled = True
                        lblRainToday.Text = "No"
                        lblExecutionPlan.Text = "Pending"
                        lblPrecipForecast.Text = "---"
                        lblSoilMoisture.Text = "---"
                    Case 1
                        If iZoneTimer = Int(ValveControl.ValveTimeArray(1).Text) Then
                            'Just started - send commands
                            Irrigation_ControlTableAdapter.Request_State_Change(1, 1)  'Zone 1 On
                            iZoneTimer = iZoneTimer - 1
                        ElseIf iZoneTimer > 0 Then
                            iZoneTimer = iZoneTimer - 1
                        Else
                            Irrigation_ControlTableAdapter.Request_State_Change(0, 1)  'Zone 1 Off
                            iActiveZone = FindNextActiveZone(iActiveZone)
                        End If
                    Case 2
                        If iZoneTimer = Int(ValveControl.ValveTimeArray(2).Text) Then
                            'Just started - send commands
                            Irrigation_ControlTableAdapter.Request_State_Change(1, 2)  'Zone 2 On
                            iZoneTimer = iZoneTimer - 1
                        ElseIf iZoneTimer > 0 Then
                            iZoneTimer = iZoneTimer - 1
                        Else
                            Irrigation_ControlTableAdapter.Request_State_Change(0, 2)  'Zone 2 Off

                            iActiveZone = FindNextActiveZone(iActiveZone)
                        End If
                    Case 3
                        If iZoneTimer = Int(ValveControl.ValveTimeArray(3).Text) Then
                            'Just started - send commands
                            Irrigation_ControlTableAdapter.Request_State_Change(1, 3)  'Zone 3 On
                            iZoneTimer = iZoneTimer - 1
                        ElseIf iZoneTimer > 0 Then
                            iZoneTimer = iZoneTimer - 1
                        Else
                            Irrigation_ControlTableAdapter.Request_State_Change(0, 3)  'Zone 3 Off
                            iActiveZone = FindNextActiveZone(iActiveZone)
                        End If
                    Case 4
                        If iZoneTimer = Int(ValveControl.ValveTimeArray(4).Text) Then
                            'Just started - send commands
                            Irrigation_ControlTableAdapter.Request_State_Change(1, 4)  'Zone 4 On
                            iZoneTimer = iZoneTimer - 1
                        ElseIf iZoneTimer > 0 Then
                            iZoneTimer = iZoneTimer - 1
                        Else
                            Irrigation_ControlTableAdapter.Request_State_Change(0, 4)  'Zone 4 Off
                            iActiveZone = FindNextActiveZone(iActiveZone)
                        End If
                    Case 5
                        If iZoneTimer = Int(ValveControl.ValveTimeArray(5).Text) Then
                            'Just started - send commands
                            Irrigation_ControlTableAdapter.Request_State_Change(1, 5)  'Zone 5 On
                            iZoneTimer = iZoneTimer - 1
                        ElseIf iZoneTimer > 0 Then
                            iZoneTimer = iZoneTimer - 1
                        Else
                            Irrigation_ControlTableAdapter.Request_State_Change(0, 5)  'Zone 5 Off
                            iActiveZone = FindNextActiveZone(iActiveZone)
                        End If
                    Case 6
                        If iZoneTimer = Int(ValveControl.ValveTimeArray(6).Text) Then
                            'Just started - send commands
                            Irrigation_ControlTableAdapter.Request_State_Change(1, 6)  'Zone 6 On
                            iZoneTimer = iZoneTimer - 1
                        ElseIf iZoneTimer > 0 Then
                            iZoneTimer = iZoneTimer - 1
                        Else
                            Irrigation_ControlTableAdapter.Request_State_Change(0, 6)  'Zone 6 Off
                            iActiveZone = FindNextActiveZone(iActiveZone)
                        End If
                    Case 7
                        If iZoneTimer = Int(ValveControl.ValveTimeArray(7).Text) Then
                            'Just started - send commands
                            Irrigation_ControlTableAdapter.Request_State_Change(1, 7)  'Zone 7 On
                            iZoneTimer = iZoneTimer - 1
                        ElseIf iZoneTimer > 0 Then
                            iZoneTimer = iZoneTimer - 1
                        Else
                            Irrigation_ControlTableAdapter.Request_State_Change(0, 7)  'Zone 7 Off
                            iActiveZone = FindNextActiveZone(iActiveZone)
                        End If
                    Case 8
                        If iZoneTimer = Int(ValveControl.ValveTimeArray(8).Text) Then
                            'Just started - send commands
                            Irrigation_ControlTableAdapter.Request_State_Change(1, 8)  'Zone 8 On
                            iZoneTimer = iZoneTimer - 1
                        ElseIf iZoneTimer > 0 Then
                            iZoneTimer = iZoneTimer - 1
                        Else
                            Irrigation_ControlTableAdapter.Request_State_Change(0, 8)  'Zone 8 Off
                            iActiveZone = FindNextActiveZone(iActiveZone)
                        End If
                    Case 9
                        If iZoneTimer = Int(ValveControl.ValveTimeArray(9).Text) Then
                            'Just started - send commands
                            Irrigation_ControlTableAdapter.Request_State_Change(1, 9)  'Zone 9 On
                            iZoneTimer = iZoneTimer - 1
                        ElseIf iZoneTimer > 0 Then
                            iZoneTimer = iZoneTimer - 1
                        Else
                            Irrigation_ControlTableAdapter.Request_State_Change(0, 9)  'Zone 9 Off
                            iActiveZone = FindNextActiveZone(iActiveZone)
                        End If
                    Case 10
                        If iZoneTimer = Int(ValveControl.ValveTimeArray(10).Text) Then
                            'Just started - send commands
                            Irrigation_ControlTableAdapter.Request_State_Change(1, 10)  'Zone 10 On
                            iZoneTimer = iZoneTimer - 1
                        ElseIf iZoneTimer > 0 Then
                            iZoneTimer = iZoneTimer - 1
                        Else
                            Irrigation_ControlTableAdapter.Request_State_Change(0, 10)  'Zone 10 Off
                            iActiveZone = FindNextActiveZone(iActiveZone)
                        End If
                    Case 11
                        If iZoneTimer = Int(ValveControl.ValveTimeArray(11).Text) Then
                            'Just started - send commands
                            Irrigation_ControlTableAdapter.Request_State_Change(1, 11)  'Zone 11 On
                            iZoneTimer = iZoneTimer - 1
                        ElseIf iZoneTimer > 0 Then
                            iZoneTimer = iZoneTimer - 1
                        Else
                            Irrigation_ControlTableAdapter.Request_State_Change(0, 11)  'Zone 11 Off
                            iActiveZone = FindNextActiveZone(iActiveZone)
                        End If
                    Case 12
                        If iZoneTimer = Int(ValveControl.ValveTimeArray(12).Text) Then
                            'Just started - send commands
                            Irrigation_ControlTableAdapter.Request_State_Change(1, 12)  'Zone 12 On
                            iZoneTimer = iZoneTimer - 1
                        ElseIf iZoneTimer > 0 Then
                            iZoneTimer = iZoneTimer - 1
                        Else
                            Irrigation_ControlTableAdapter.Request_State_Change(0, 12)  'Zone 12 Off
                            iActiveZone = FindNextActiveZone(iActiveZone)
                        End If
                    Case 13
                        If iZoneTimer = Int(ValveControl.ValveTimeArray(13).Text) Then
                            'Just started - send commands
                            Irrigation_ControlTableAdapter.Request_State_Change(1, 13)  'Zone 13 On
                            iZoneTimer = iZoneTimer - 1
                        ElseIf iZoneTimer > 0 Then
                            iZoneTimer = iZoneTimer - 1
                        Else
                            Irrigation_ControlTableAdapter.Request_State_Change(0, 13)  'Zone 13 On
                            iActiveZone = FindNextActiveZone(iActiveZone)
                        End If
                End Select
            End If

        Catch ex As Exception
            Irrigation_HistoryTableAdapter.InsertQuery("999")
            Event_HistoryTableAdapter1.InsertQuery("9915")
        End Try

    End Sub

    Private Sub tLeakCheck_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tLeakCheck.Tick
        'Timer ticks every 10 minutes

        If iOldGallonsUsed < iGallonsUsed Then
            iLeakCounter = iLeakCounter + 1
            iOldGallonsUsed = iGallonsUsed
            If iLeakCounter > 12 Then
                iLeakCounter = 12
            End If
        Else
            iLeakCounter = 0
        End If

        If bRunning Then
            iLeakCounter = 0
        End If

        pbLeakDetected.Value = iLeakCounter
        If iLeakCounter > Int(lblPeakLeakDetected.Text) Then
            lblPeakLeakDetected.Text = iLeakCounter.ToString
        End If

        'Continuous water usage after 2 hours = Leak Detected
        If iLeakCounter = 12 Then
            Event_HistoryTableAdapter1.InsertQuery("5034")
            Event_Current_StateTableAdapter.AlertLeakDetected()
            iLeakCounter = 0
        End If

    End Sub

    Private Sub tCheckDB_Tick(sender As Object, e As EventArgs) Handles tCheckDB.Tick
        'Runs once every 6 seconds

        Dim tabNewRequests As WatchdogDataSet1.Irrigation_ControlDataTable
        Dim tabActiveZones As WatchdogDataSet1.Irrigation_ControlDataTable
        Dim i, j, k As Integer

        lblLastUpdated.Text = Now.ToString
        Try
            tabNewRequests = Irrigation_ControlTableAdapter.Get_New_Requests
            If tabNewRequests.Rows.Count > 0 Then
                tabActiveZones = Irrigation_ControlTableAdapter.Get_Active_Zones
                If tabActiveZones.Rows.Count > 0 Then
                    'Turn off all active zones (except for master)
                    For j = 0 To tabActiveZones.Rows.Count - 1
                        For i = 1 To 3
                            _VCarduino.SetDigitalValue(21 + CInt(tabActiveZones.Rows(j).Item(0)), 0)
                            Thread.Sleep(40)
                        Next i
                        Irrigation_ControlTableAdapter.Update_Current_State(0, CInt(tabActiveZones.Rows(j).Item(0)))
                        LogEvent(CInt(tabActiveZones.Rows(j).Item(0)), 0)
                    Next j
                End If

                'Fulfill request
                For k = 0 To tabNewRequests.Rows.Count - 1
                    If CInt(tabNewRequests.Rows(k).Item(0)) = 0 Then   'Special case to change the state of the Master
                        For i = 1 To 3
                            _VCarduino.SetDigitalValue(3, CInt(tabNewRequests.Rows(0).Item(3)))
                            Thread.Sleep(40)
                        Next i
                        If CInt(tabNewRequests.Rows(k).Item(3)) = 1 Then
                            For i = 1 To 3
                                _VCarduino.SetDigitalValue(3, 1)
                                Thread.Sleep(40)
                            Next i
                            Irrigation_ControlTableAdapter.Update_Current_State(1, 0)
                            bMasterOn = True
                            LogEvent(0, 1)
                        Else
                            For i = 1 To 3
                                _VCarduino.SetDigitalValue(3, 0)
                                Thread.Sleep(40)
                            Next i
                            Irrigation_ControlTableAdapter.Update_Current_State(0, 0)
                            bMasterOn = False
                            LogEvent(0, 0)
                        End If
                        Irrigation_ControlTableAdapter.Update_Current_State(CInt(tabNewRequests.Rows(k).Item(3)), 0)
                        LogEvent(0, CInt(tabNewRequests.Rows(k).Item(3)))
                    ElseIf CInt(tabNewRequests.Rows(k).Item(3)) = 1 Then   'Already turn all valves off, so ignore if 0
                        If Not bMasterOn Then
                            For i = 1 To 3
                                _VCarduino.SetDigitalValue(3, 1)
                                Thread.Sleep(40)
                            Next i
                            Irrigation_ControlTableAdapter.Update_Current_State(1, 0)
                            bMasterOn = True
                            LogEvent(0, 1)
                            Thread.Sleep(500)
                        End If
                        For i = 1 To 3
                            _VCarduino.SetDigitalValue(21 + CInt(tabNewRequests.Rows(k).Item(0)), CInt(tabNewRequests.Rows(k).Item(3)))
                            Thread.Sleep(40)
                        Next i
                        Irrigation_ControlTableAdapter.Update_Current_State(CInt(tabNewRequests.Rows(k).Item(3)), CInt(tabNewRequests.Rows(k).Item(0)))
                        LogEvent(CInt(tabNewRequests.Rows(k).Item(0)), CInt(tabNewRequests.Rows(k).Item(3)))

                        'Restart Fail Safe to turn off all valves after 30 minutes
                        tFailSafe.Stop()
                        tFailSafe.Start()
                    End If
                Next k
                'Update database - reset all requests to 0
                Irrigation_ControlTableAdapter.Reset_New_Requests()

            End If
        Catch ex As Exception
            Irrigation_HistoryTableAdapter.InsertQuery("999")
            Event_HistoryTableAdapter1.InsertQuery("9915")
        End Try
    End Sub

    Private Sub tFailSafe_Tick(sender As Object, e As EventArgs) Handles tFailSafe.Tick
        'Runs once every 30 minutes

        Dim tabActiveZones As WatchdogDataSet1.Irrigation_ControlDataTable
        Dim i, j As Integer

        'Turn off Master
        For i = 1 To 3
            _VCarduino.SetDigitalValue(3, 0)
            Thread.Sleep(40)
        Next i
        Irrigation_ControlTableAdapter.Update_Current_State(0, 0)
        LogEvent(0, 0)
        bMasterOn = False

        'Turn off all active zones 
        tabActiveZones = Irrigation_ControlTableAdapter.Get_Active_Zones
        If tabActiveZones.Rows.Count > 0 Then
            For j = 0 To tabActiveZones.Rows.Count - 1
                For i = 1 To 3
                    _VCarduino.SetDigitalValue(21 + CInt(tabActiveZones.Rows(j).Item(0)), 0)
                    Thread.Sleep(40)
                Next i
                Irrigation_ControlTableAdapter.Update_Current_State(0, CInt(tabActiveZones.Rows(j).Item(0)))
                LogEvent(CInt(tabActiveZones.Rows(j).Item(0)), 0)
            Next j
        End If
        tFailSafe.Stop()
        lblIrrigationStatus.Text = "Stopped"
    End Sub

#End Region

    Private Sub CheckMonth()
        Select Case Month(Now)
            Case 11, 12, 1, 2, 3
                'Off season, keep the water off.
                bValveControlActive = False
                btnValveActivate.Enabled = True
                btnValveDeactivate.Enabled = False
                lblIrrigationStatus.Text = "Disabled"
                'Case Else
                '    bValveControlActive = True
                '    btnValveActivate.Enabled = False
                '    btnValveDeactivate.Enabled = True
                '    lblIrrigationStatus.Text = "Stopped"
        End Select
    End Sub

    Private Sub LogEvent(ByVal iZone As Integer, ByVal iState As Integer)
        Select Case iZone
            Case 0
                If iState = 1 Then
                    Irrigation_HistoryTableAdapter.InsertQuery("227")
                Else
                    Irrigation_HistoryTableAdapter.InsertQuery("228")
                End If
            Case 1
                If iState = 1 Then
                    IrrigationMap.Spout1a.Visible = True
                    IrrigationMap.Spout1b.Visible = True
                    IrrigationMap.Spout1c.Visible = True

                    lblIrrigationStatus.Text = "Zone 1 Running"
                    Irrigation_HistoryTableAdapter.InsertQuery("201")
                Else
                    lblIrrigationStatus.Text = "Zone 1 Stopped"
                    IrrigationMap.Spout1a.Visible = False
                    IrrigationMap.Spout1b.Visible = False
                    IrrigationMap.Spout1c.Visible = False

                    Irrigation_HistoryTableAdapter.InsertQuery("202")
                End If
            Case 2
                If iState = 1 Then
                    IrrigationMap.Spout2a.Visible = True
                    IrrigationMap.Spout2b.Visible = True
                    IrrigationMap.Spout2c.Visible = True
                    IrrigationMap.Spout2d.Visible = True
                    IrrigationMap.Spout2e.Visible = True

                    lblIrrigationStatus.Text = "Zone 2 Running"
                    Irrigation_HistoryTableAdapter.InsertQuery("203")
                Else
                    lblIrrigationStatus.Text = "Zone 2 Stopped"
                    IrrigationMap.Spout2a.Visible = False
                    IrrigationMap.Spout2b.Visible = False
                    IrrigationMap.Spout2c.Visible = False
                    IrrigationMap.Spout2d.Visible = False
                    IrrigationMap.Spout2e.Visible = False
                    Irrigation_HistoryTableAdapter.InsertQuery("204")
                End If
            Case 3
                If iState = 1 Then
                    IrrigationMap.Spout3a.Visible = True
                    IrrigationMap.Spout3b.Visible = True
                    IrrigationMap.Spout3c.Visible = True
                    IrrigationMap.Spout3d.Visible = True

                    lblIrrigationStatus.Text = "Zone 3 Running"
                    Irrigation_HistoryTableAdapter.InsertQuery("205")
                Else
                    lblIrrigationStatus.Text = "Zone 3 Stopped"
                    IrrigationMap.Spout3a.Visible = False
                    IrrigationMap.Spout3b.Visible = False
                    IrrigationMap.Spout3c.Visible = False
                    IrrigationMap.Spout3d.Visible = False
                    Irrigation_HistoryTableAdapter.InsertQuery("206")

                End If
            Case 4
                If iState = 1 Then
                    IrrigationMap.Spout4a.Visible = True
                    IrrigationMap.Spout4b.Visible = True
                    IrrigationMap.Spout4c.Visible = True
                    IrrigationMap.Spout4d.Visible = True
                    IrrigationMap.Spout4e.Visible = True

                    lblIrrigationStatus.Text = "Zone 4 Running"
                    Irrigation_HistoryTableAdapter.InsertQuery("207")
                Else
                    lblIrrigationStatus.Text = "Zone 4 Stopped"
                    IrrigationMap.Spout4a.Visible = False
                    IrrigationMap.Spout4b.Visible = False
                    IrrigationMap.Spout4c.Visible = False
                    IrrigationMap.Spout4d.Visible = False
                    IrrigationMap.Spout4e.Visible = False
                    Irrigation_HistoryTableAdapter.InsertQuery("208")
                End If
            Case 5
                If iState = 1 Then
                    IrrigationMap.Spout5a.Visible = True
                    IrrigationMap.Spout5b.Visible = True
                    IrrigationMap.Spout5c.Visible = True
                    IrrigationMap.Spout5d.Visible = True

                    lblIrrigationStatus.Text = "Zone 5 Running"
                    Irrigation_HistoryTableAdapter.InsertQuery("209")
                Else
                    lblIrrigationStatus.Text = "Zone 5 Stopped"
                    IrrigationMap.Spout5a.Visible = False
                    IrrigationMap.Spout5b.Visible = False
                    IrrigationMap.Spout5c.Visible = False
                    IrrigationMap.Spout5d.Visible = False

                    Irrigation_HistoryTableAdapter.InsertQuery("210")
                End If
            Case 6
                If iState = 1 Then
                    IrrigationMap.Spout6a.Visible = True
                    IrrigationMap.Spout6b.Visible = True
                    IrrigationMap.Spout6c.Visible = True
                    IrrigationMap.Spout6d.Visible = True
                    IrrigationMap.Spout6e.Visible = True

                    lblIrrigationStatus.Text = "Zone 6 Running"
                    Irrigation_HistoryTableAdapter.InsertQuery("211")
                Else
                    lblIrrigationStatus.Text = "Zone 6 Stopped"
                    IrrigationMap.Spout6a.Visible = False
                    IrrigationMap.Spout6b.Visible = False
                    IrrigationMap.Spout6c.Visible = False
                    IrrigationMap.Spout6d.Visible = False
                    IrrigationMap.Spout6e.Visible = False

                    Irrigation_HistoryTableAdapter.InsertQuery("212")
                End If
            Case 7
                If iState = 1 Then
                    IrrigationMap.Spout7b.Visible = True
                    IrrigationMap.Spout7c.Visible = True
                    IrrigationMap.Spout7d.Visible = True
                    IrrigationMap.Spout7e.Visible = True

                    lblIrrigationStatus.Text = "Zone 7 Running"
                    Irrigation_HistoryTableAdapter.InsertQuery("213")
                Else
                    lblIrrigationStatus.Text = "Zone 7 Stopped"
                    IrrigationMap.Spout7b.Visible = False
                    IrrigationMap.Spout7c.Visible = False
                    IrrigationMap.Spout7d.Visible = False
                    IrrigationMap.Spout7e.Visible = False

                    Irrigation_HistoryTableAdapter.InsertQuery("214")
                End If
            Case 8
                If iState = 1 Then
                    IrrigationMap.Spout8a.Visible = True
                    IrrigationMap.Spout8b.Visible = True
                    IrrigationMap.Spout8c.Visible = True

                    lblIrrigationStatus.Text = "Zone 8 Running"
                    Irrigation_HistoryTableAdapter.InsertQuery("215")
                Else
                    lblIrrigationStatus.Text = "Zone 8 Stopped"
                    IrrigationMap.Spout8a.Visible = False
                    IrrigationMap.Spout8b.Visible = False
                    IrrigationMap.Spout8c.Visible = False

                    Irrigation_HistoryTableAdapter.InsertQuery("216")
                End If
            Case 9
                If iState = 1 Then
                    IrrigationMap.Spout9a.Visible = True
                    IrrigationMap.Spout9b.Visible = True
                    IrrigationMap.Spout9c.Visible = True
                    IrrigationMap.Spout9d.Visible = True

                    lblIrrigationStatus.Text = "Zone 9 Running"
                    Irrigation_HistoryTableAdapter.InsertQuery("217")
                Else
                    lblIrrigationStatus.Text = "Zone 9 Stopped"
                    IrrigationMap.Spout9a.Visible = False
                    IrrigationMap.Spout9b.Visible = False
                    IrrigationMap.Spout9c.Visible = False
                    IrrigationMap.Spout9d.Visible = False

                    Irrigation_HistoryTableAdapter.InsertQuery("218")
                End If
            Case 10
                If iState = 1 Then
                    IrrigationMap.Spout10a.Visible = True
                    IrrigationMap.Spout10b.Visible = True
                    IrrigationMap.Spout10c.Visible = True
                    IrrigationMap.Spout10d.Visible = True
                    IrrigationMap.Spout10e.Visible = True

                    lblIrrigationStatus.Text = "Zone 10 Running"
                    Irrigation_HistoryTableAdapter.InsertQuery("219")
                Else
                    lblIrrigationStatus.Text = "Zone 10 Stopped"
                    IrrigationMap.Spout10a.Visible = False
                    IrrigationMap.Spout10b.Visible = False
                    IrrigationMap.Spout10c.Visible = False
                    IrrigationMap.Spout10d.Visible = False
                    IrrigationMap.Spout10e.Visible = False

                    Irrigation_HistoryTableAdapter.InsertQuery("220")
                End If
            Case 11
                If iState = 1 Then
                    IrrigationMap.Spout11a.Visible = True
                    IrrigationMap.Spout11b.Visible = True
                    IrrigationMap.Spout11c.Visible = True
                    IrrigationMap.Spout11d.Visible = True

                    lblIrrigationStatus.Text = "Zone 11 Running"
                    Irrigation_HistoryTableAdapter.InsertQuery("221")
                Else
                    lblIrrigationStatus.Text = "Zone 11 Stopped"
                    IrrigationMap.Spout11a.Visible = False
                    IrrigationMap.Spout11b.Visible = False
                    IrrigationMap.Spout11c.Visible = False
                    IrrigationMap.Spout11d.Visible = False

                    Irrigation_HistoryTableAdapter.InsertQuery("222")
                End If
            Case 12
                If iState = 1 Then
                    IrrigationMap.Spout12a.Visible = True
                    IrrigationMap.Spout12b.Visible = True
                    IrrigationMap.Spout12c.Visible = True

                    lblIrrigationStatus.Text = "Zone 12 Running"
                    Irrigation_HistoryTableAdapter.InsertQuery("223")
                Else
                    lblIrrigationStatus.Text = "Zone 12 Stopped"
                    IrrigationMap.Spout12a.Visible = False
                    IrrigationMap.Spout12b.Visible = False
                    IrrigationMap.Spout12c.Visible = False

                    Irrigation_HistoryTableAdapter.InsertQuery("224")
                End If
            Case 13
                If iState = 1 Then
                    IrrigationMap.Spout13a.Visible = True
                    IrrigationMap.Spout13b.Visible = True
                    IrrigationMap.Spout13c.Visible = True
                    IrrigationMap.Spout13d.Visible = True

                    lblIrrigationStatus.Text = "Zone 13 Running"
                    Irrigation_HistoryTableAdapter.InsertQuery("225")
                Else
                    lblIrrigationStatus.Text = "Zone 13 Stopped"
                    IrrigationMap.Spout13a.Visible = False
                    IrrigationMap.Spout13b.Visible = False
                    IrrigationMap.Spout13c.Visible = False
                    IrrigationMap.Spout13d.Visible = False

                    Irrigation_HistoryTableAdapter.InsertQuery("226")
                End If
        End Select

    End Sub

End Class
