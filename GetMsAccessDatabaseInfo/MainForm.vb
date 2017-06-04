Imports System.Data.OleDb
Imports System.IO
Imports Microsoft.Office.Interop

Public Class MainForm




#Region "Variables"

    Public O_Access As Object = Nothing
        Dim filepath As String
        Dim oledbCon As New OleDbConnection
        Dim strGetAccessInfosCommand As String = "SELECT Name, DateCreate, DateUpdate, " &
                                                 " " &
                                                 "iif(LEFT(Name, 4) = 'MSys','System Table'," &
                                                 "iif(type = 2,'System Object', " &
                                                 "iif(type = 3,'System Object'," &
                                                 "iif(type = 8,'System Object', " &
                                                 "iif(type = 4,'Linked Table (ODBC)'," &
                                                 "iif(type = 1,'Table', " &
                                                 "iif(type = 6, 'Linked Table (MsAccess/MsExcel)'," &
                                                 "iif(type = 5,'Query', " &
                                                 "iif(type = -32768,'Form'," &
                                                 "iif(type = -32764,'Report', " &
                                                 "iif(type=-32766,'Macro'," &
                                                 "iif(type = -32761,'Module', " &
                                                 "iif(type = -32756,'Page', " &
                                                 "iif(type = -32758,'User','Unknown')))))))))))))) as ObjectType" &
                                                 " FROM MSysObjects WHERE LEFT(Name, 1) <> '~' "
        'WHERE Type IN (1, 5, 2, 3, 4, 6, 8, -32768, -32764, -32766, -32761)" & _
        Dim lstObjects As New List(Of AccessObject)

#End Region

#Region "Form_Events"
        Private Sub btnDataSource_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBrowse.Click
            Dim ofd As New OpenFileDialog
            ofd.Filter = "Access 2000-2003 database (*.mdb)|*.mdb|Access 2007+ database (*.accdb)|*.accdb|All files (*.*)|*.*"
            If ofd.ShowDialog = Windows.Forms.DialogResult.OK Then
                filepath = ofd.FileName
                '      txtPath.Text = filepath

                If Not TypeOf O_Access Is Access.Application Then
                    O_Access = CreateObject("Access.Application")
                End If
                O_Access.OpenCurrentDatabase(filepath)

                Try
                    O_Access.Visible = Boolean.FalseString
                Catch ex As Exception

                End Try



            Try
                'Get File Infos
                lblCreation.Text = String.Empty
                lblLastModified.Text = String.Empty
                lblLastAccessed.Text = String.Empty
                lblSize.Text = String.Empty

                Dim AccessFile As New FileInfo(filepath)
                lblCreation.Text = AccessFile.CreationTime.ToString
                lblLastModified.Text = AccessFile.LastWriteTime.ToString
                lblLastAccessed.Text = AccessFile.LastAccessTime.ToString
                Dim FileSize As Decimal = 0
                FileSize = ((AccessFile.Length / 1024) / (1024))
                FileSize = Decimal.Round(FileSize, 4)
                lblSize.Text = FileSize.ToString & " MB"

                'Get Database infos
                Dim dbs As dao.Database
                Dim Rec As dao.Recordset
                dbs = O_Access.CurrentDb
                If Not chkSysObjects.Checked Then
                    Rec = dbs.OpenRecordset(strGetAccessInfosCommand & " AND LEFT(Name, 4) <> 'MSys' AND" &
                                            " Type IN (1, 5, 4, 6,  -32768, -32764, -32766, -32761,-32756,-32758)",
                                            dao.RecordsetTypeEnum.dbOpenDynaset)

                Else
                    Rec = dbs.OpenRecordset(strGetAccessInfosCommand,
                                            dao.RecordsetTypeEnum.dbOpenDynaset)
                End If

                lstObjects.Clear()

                While Not Rec.EOF
                    Dim accObject As New AccessObject


                    If IsDBNull(Rec("Name").Value) = False Then
                        accObject.ObjectName = Rec("Name").Value.ToString
                    End If

                    If IsDBNull(Rec("DateCreate").Value) = False Then
                        accObject.DateCreated = Rec("DateCreate").Value.ToString()
                    End If

                    If IsDBNull(Rec("DateUpdate").Value) = False Then
                        accObject.DateModified = Rec("DateUpdate").Value.ToString()
                    End If

                    If IsDBNull(Rec("ObjectType").Value) = False Then
                        accObject.ObjectType = Rec("ObjectType").Value.ToString()
                    End If

                    lstObjects.Add(accObject)
                    Rec.MoveNext()
                End While
                Rec.Close()
                dbs.Close()
                Rec = Nothing
                dbs = Nothing
                O_Access.CloseCurrentDatabase()


            Catch ex As Exception
                MsgBox(ex.Message)
            Finally
                oledbCon.Close()
                'O_Access.Quit()
                'O_Access = Nothing
            End Try

            DataGridView1.DataSource = lstObjects.OrderBy(Function(x) x.ObjectType).ToList
            DataGridView1.Refresh()
        End If


        End Sub
#End Region


    End Class

