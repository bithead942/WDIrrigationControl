
Public Class AccuWeather
    Public Property Headline As Headline
    Public Property DailyForecasts As List(Of Dailyforecast)
End Class

Public Class Headline
    Public Property EffectiveDate As Date
    Public Property EffectiveEpochDate As Integer
    Public Property Severity As Integer
    Public Property Text As String
    Public Property Category As String
    Public Property EndDate As Date
    Public Property EndEpochDate As Integer
    Public Property MobileLink As String
    Public Property Link As String
End Class

Public Class Dailyforecast
    Public Property _Date As Date
    Public Property EpochDate As Integer
    Public Property Sun As Sun
    Public Property Moon As Moon
    Public Property Temperature As Temperature
    Public Property RealFeelTemperature As Realfeeltemperature
    Public Property RealFeelTemperatureShade As Realfeeltemperatureshade
    Public Property HoursOfSun As Single
    Public Property DegreeDaySummary As Degreedaysummary
    Public Property AirAndPollen As List(Of Airandpollen)
    Public Property Day As Day
    Public Property Night As Night
    Public Property Sources As List(Of String)
    Public Property MobileLink As String
    Public Property Link As String
End Class

Public Class Sun
    Public Property Rise As Date
    Public Property EpochRise As Integer
    Public Property _Set As Date
    Public Property EpochSet As Integer
End Class

Public Class Moon
    Public Property Rise As Date
    Public Property EpochRise As Integer
    Public Property _Set As Date
    Public Property EpochSet As Integer
    Public Property Phase As String
    Public Property Age As Integer
End Class

Public Class Temperature
    Public Property Minimum As Minimum
    Public Property Maximum As Maximum
End Class

Public Class Minimum
    Public Property Value As Double
    Public Property Unit As String
    Public Property UnitType As Integer
End Class

Public Class Maximum
    Public Property Value As Double
    Public Property Unit As String
    Public Property UnitType As Integer
End Class

Public Class Realfeeltemperature
    Public Property Minimum As Minimum1
    Public Property Maximum As Maximum1
End Class

Public Class Minimum1
    Public Property Value As Double
    Public Property Unit As String
    Public Property UnitType As Integer
End Class

Public Class Maximum1
    Public Property Value As Double
    Public Property Unit As String
    Public Property UnitType As Integer
End Class

Public Class Realfeeltemperatureshade
    Public Property Minimum As Minimum2
    Public Property Maximum As Maximum2
End Class

Public Class Minimum2
    Public Property Value As Double
    Public Property Unit As String
    Public Property UnitType As Integer
End Class

Public Class Maximum2
    Public Property Value As Double
    Public Property Unit As String
    Public Property UnitType As Integer
End Class

Public Class Degreedaysummary
    Public Property Heating As Heating
    Public Property Cooling As Cooling
End Class

Public Class Heating
    Public Property Value As Double
    Public Property Unit As String
    Public Property UnitType As Integer
End Class

Public Class Cooling
    Public Property Value As Double
    Public Property Unit As String
    Public Property UnitType As Integer
End Class

Public Class Day
    Public Property Icon As Integer
    Public Property IconPhrase As String
    Public Property HasPrecipitation As Boolean
    Public Property ShortPhrase As String
    Public Property LongPhrase As String
    Public Property PrecipitationProbability As Integer
    Public Property ThunderstormProbability As Integer
    Public Property RainProbability As Integer
    Public Property SnowProbability As Integer
    Public Property IceProbability As Integer
    Public Property Wind As Wind
    Public Property WindGust As Windgust
    Public Property TotalLiquid As Totalliquid
    Public Property Rain As Rain
    Public Property Snow As Snow
    Public Property Ice As Ice
    Public Property HoursOfPrecipitation As Double
    Public Property HoursOfRain As Double
    Public Property HoursOfSnow As Double
    Public Property HoursOfIce As Double
    Public Property CloudCover As Integer
    Public Property PrecipitationType As String
    Public Property PrecipitationIntensity As String
End Class

Public Class Wind
    Public Property Speed As Speed
    Public Property Direction As Direction
End Class

Public Class Speed
    Public Property Value As Double
    Public Property Unit As String
    Public Property UnitType As Integer
End Class

Public Class Direction
    Public Property Degrees As Double
    Public Property Localized As String
    Public Property English As String
End Class

Public Class Windgust
    Public Property Speed As Speed1
    Public Property Direction As Direction1
End Class

Public Class Speed1
    Public Property Value As Double
    Public Property Unit As String
    Public Property UnitType As Integer
End Class

Public Class Direction1
    Public Property Degrees As Double
    Public Property Localized As String
    Public Property English As String
End Class

Public Class Totalliquid
    Public Property Value As Double
    Public Property Unit As String
    Public Property UnitType As Integer
End Class

Public Class Rain
    Public Property Value As Double
    Public Property Unit As String
    Public Property UnitType As Integer
End Class

Public Class Snow
    Public Property Value As Double
    Public Property Unit As String
    Public Property UnitType As Integer
End Class

Public Class Ice
    Public Property Value As Double
    Public Property Unit As String
    Public Property UnitType As Integer
End Class

Public Class Night
    Public Property Icon As Integer
    Public Property IconPhrase As String
    Public Property HasPrecipitation As Boolean
    Public Property ShortPhrase As String
    Public Property LongPhrase As String
    Public Property PrecipitationProbability As Integer
    Public Property ThunderstormProbability As Integer
    Public Property RainProbability As Integer
    Public Property SnowProbability As Integer
    Public Property IceProbability As Integer
    Public Property Wind As Wind1
    Public Property WindGust As Windgust1
    Public Property TotalLiquid As Totalliquid1
    Public Property Rain As Rain1
    Public Property Snow As Snow1
    Public Property Ice As Ice1
    Public Property HoursOfPrecipitation As Double
    Public Property HoursOfRain As Double
    Public Property HoursOfSnow As Double
    Public Property HoursOfIce As Double
    Public Property CloudCover As Integer
End Class

Public Class Wind1
    Public Property Speed As Speed2
    Public Property Direction As Direction2
End Class

Public Class Speed2
    Public Property Value As Double
    Public Property Unit As String
    Public Property UnitType As Integer
End Class

Public Class Direction2
    Public Property Degrees As Double
    Public Property Localized As String
    Public Property English As String
End Class

Public Class Windgust1
    Public Property Speed As Speed3
    Public Property Direction As Direction3
End Class

Public Class Speed3
    Public Property Value As Double
    Public Property Unit As String
    Public Property UnitType As Integer
End Class

Public Class Direction3
    Public Property Degrees As Double
    Public Property Localized As String
    Public Property English As String
End Class

Public Class Totalliquid1
    Public Property Value As Double
    Public Property Unit As String
    Public Property UnitType As Integer
End Class

Public Class Rain1
    Public Property Value As Double
    Public Property Unit As String
    Public Property UnitType As Integer
End Class

Public Class Snow1
    Public Property Value As Double
    Public Property Unit As String
    Public Property UnitType As Integer
End Class

Public Class Ice1
    Public Property Value As Double
    Public Property Unit As String
    Public Property UnitType As Integer
End Class

Public Class Airandpollen
    Public Property Name As String
    Public Property Value As Integer
    Public Property Category As String
    Public Property CategoryValue As Integer
    Public Property Type As String
End Class

