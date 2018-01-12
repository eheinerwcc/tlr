Imports System.Data
Imports System.Data.SqlClient

Public Class clsWorkSchedule
    Shared Function GetStatus(ByVal strSID As String) As Integer
        Return SqlHelper.ExecuteScalar(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_SELECT_WorkScheduleStatus", _
                                           New SqlParameter("SID", strSID))
    End Function

    Shared Function Exists(ByVal strSID As String) As Boolean
        Dim dr As SqlDataReader = SqlHelper.ExecuteReader(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_SELECT_WorkSchedule", _
                                New SqlParameter("@SID", strSID))
        Return dr.HasRows()
    End Function

    Shared Function GetEmployeeWorkSchedule(ByVal strSID As String) As SqlDataReader
        Return SqlHelper.ExecuteReader(SqlHelper.GetConnString(), CommandType.StoredProcedure, "usp_SELECT_WorkScheduleEntry", _
                                                             New SqlParameter("@SID", strSID))
    End Function
End Class
