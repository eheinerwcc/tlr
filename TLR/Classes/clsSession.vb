Public Class clsSession
    Shared Property userSID() As String
        Get
            Return HttpContext.Current.Session("userSID")
        End Get
        Set(ByVal Value As String)
            HttpContext.Current.Session("userSID") = Value
        End Set
    End Property

    Shared Property userFirstName() As String
        Get
            Return HttpContext.Current.Session("userFirstName")
        End Get
        Set(ByVal Value As String)
            HttpContext.Current.Session("userFirstName") = Value
        End Set
    End Property

    Shared Property userLastName() As String
        Get
            Return HttpContext.Current.Session("userLastName")
        End Get
        Set(ByVal Value As String)
            HttpContext.Current.Session("userLastName") = Value
        End Set
    End Property

    Shared Property userDisplayName() As String
        Get
            Return HttpContext.Current.Session("userDisplayName")
        End Get
        Set(ByVal Value As String)
            HttpContext.Current.Session("userDisplayName") = Value
        End Set
    End Property


    Shared Property userWorkEmail() As String
        Get
            Return HttpContext.Current.Session("userWorkEmail")
        End Get
        Set(ByVal Value As String)
            HttpContext.Current.Session("userWorkEmail") = Value
        End Set
    End Property

    Shared Property userFullPartIndicator() As String
        'Full time ("F") or part time ("P") employee
        Get
            Return HttpContext.Current.Session("userFullPartIndicator")
        End Get
        Set(ByVal Value As String)
            HttpContext.Current.Session("userFullPartIndicator") = Value
        End Set
    End Property

    Shared Property userEmploymentStatus() As String
        'Active ("A") or separated ("S")
        Get
            Return HttpContext.Current.Session("userEmploymentStatus")
        End Get
        Set(ByVal Value As String)
            HttpContext.Current.Session("userEmploymentStatus") = Value
        End Set
    End Property

    Shared Property userEmployeeType() As String
        Get
            Return HttpContext.Current.Session("userEmployeeType")
        End Get
        Set(ByVal Value As String)
            HttpContext.Current.Session("userEmployeeType") = Value
        End Set
    End Property

    Shared Property userLeaveExpirationMonth() As String
        'Number of month, in which leave balances expire every year
        Get
            Return HttpContext.Current.Session("userLeaveExpirationMonth")
        End Get
        Set(ByVal Value As String)
            HttpContext.Current.Session("userLeaveExpirationMonth") = Value
        End Set
    End Property

    Shared Property userLeaveEligible() As Boolean
        'Number of month, in which leave balances expire every year
        Get
            Return HttpContext.Current.Session("userLeaveEligible")
        End Get
        Set(ByVal Value As Boolean)
            HttpContext.Current.Session("userLeaveEligible") = Value
        End Set
    End Property

    Shared Property userIsSupervisor() As Boolean
        'Defined by having a record for a give user in vw_Supervisor
        Get
            Return HttpContext.Current.Session("userIsSupervisor")
        End Get
        Set(ByVal Value As Boolean)
            HttpContext.Current.Session("userIsSupervisor") = Value
        End Set
    End Property

    Shared Property userIsPayrollAdmin() As Boolean
        'Defined by having a record for a given user in PayrollAdmin table
        Get
            Return HttpContext.Current.Session("userIsPayrollAdmin")
        End Get
        Set(ByVal Value As Boolean)
            HttpContext.Current.Session("userIsPayrollAdmin") = Value
        End Set
    End Property

    Shared Property userIsFinAidAdmin() As Boolean
        'Defined by having a record for a given user in PayrollAdmin table
        Get
            Return HttpContext.Current.Session("userIsFinAidAdmin")
        End Get
        Set(ByVal Value As Boolean)
            HttpContext.Current.Session("userIsFinAidAdmin") = Value
        End Set
    End Property

    Shared Property userIsWorkScheduleEligible() As Boolean
        'Defined by a value from My.Settings 
        Get
            Return HttpContext.Current.Session("userIsWorkScheduleEligible")
        End Get
        Set(ByVal Value As Boolean)
            HttpContext.Current.Session("userIsWorkScheduleEligible") = Value
        End Set
    End Property

    Shared Property userIsFinanceAdmin() As Boolean
        'Defined by having a record for a given user in PayrollAdmin table
        Get
            Return HttpContext.Current.Session("userIsFinanceAdmin")
        End Get
        Set(ByVal Value As Boolean)
            HttpContext.Current.Session("userIsFinanceAdmin") = Value
        End Set
    End Property
    Shared Property userIsHRAdmin() As Boolean
        'Defined by having a record for a given user in PayrollAdmin table
        Get
            Return HttpContext.Current.Session("userIsHRAdmin")
        End Get
        Set(ByVal Value As Boolean)
            HttpContext.Current.Session("userIsHRAdmin") = Value
        End Set
    End Property

    Shared Property userIsDelegatedTimesheetManager() As Boolean
        'represents whether a primary supervisor has been given rights to manage timesheets on behalf of an employee
        Get
            Return HttpContext.Current.Session("userIsDelegatedTimesheetManager")
        End Get
        Set(ByVal Value As Boolean)
            HttpContext.Current.Session("userIsDelegatedTimesheetManager") = Value
        End Set
    End Property
End Class
