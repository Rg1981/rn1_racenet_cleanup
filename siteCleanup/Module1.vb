Imports System.Data.SqlClient
Imports Microsoft.VisualBasic
Imports System.Net.Mail
Imports System.Net
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

Module Module1

    Private deldate As Date = Now.AddDays(-1)
    Private rootPath As String = "d:/wwwroot/racenet.com.au"
    Private rootPathBreednet As String = "d:/wwwroot/breednet.com.au"
    Private rootPathrnMobile As String = "d:/wwwroot/m.racenet.com.au"
    'Private rootPath As String = "c:/sites/racenet"
    'Private rootPathBreednet As String = "z:/breednet"
    Sub Main(dateVariable As String())
        Try

            If dateVariable.Length > 0 Then
                Try
                    deldate = Convert.ToDateTime(dateVariable(0))
                Catch ex As Exception
                    Dim smtpDate As SmtpClient = racenetgeneral.smtpmailserver()
                    Dim sendemailDate As Mail.MailMessage = New Mail.MailMessage
                    sendemailDate.From = New Mail.MailAddress("cleanUp@racenet.com.au")
                    sendemailDate.To.Add(New Mail.MailAddress("ramy@racenet.com.au"))
                    sendemailDate.CC.Add(New Mail.MailAddress("nathan@racenet.com.au"))
                    sendemailDate.Subject = "Clean Up Error -  Date Issue"
                    sendemailDate.IsBodyHtml = True
                    sendemailDate.Body = "Invalid date - " + dateVariable(0)
                    smtpDate.Send(sendemailDate)
                    sendemailDate.Dispose()
                    smtpDate = Nothing
                End Try
            End If

            Dim textdate As String = deldate.ToString("ddMMyy")
            Dim premiumFormDate As String = deldate.ToString("d/MM/yyyy")
            Dim premiumFormXMLDate As String = deldate.ToString("dMMyyyy")
            Dim dbDate As String = deldate.ToString("yyyy-MM-dd")
            Dim dbFunctionSwitch As Integer = 1

            If dbFunctionSwitch = 1 Then


                Dim conn As SqlConnection = RacenetSql.getConnection
                conn.Open()

                '####################################################################################
                'Form Clean up
                '####################################################################################

                Dim formTablesDelete As SqlCommand = RacenetSql.buildCommand("aap_CleanUpByTextdate", conn)
                formTablesDelete.Parameters.Add(New SqlParameter("@TextDate", textdate))
                formTablesDelete.ExecuteNonQuery()
                formTablesDelete.CommandTimeout = 150
                RacenetSql.disposeCommand(formTablesDelete)

                Dim formPath As String = rootPath + "/breeding/includes/homepage/form.txt"
                Dim formHeadPath As String = rootPath + "/breeding/includes/homepage/formtabs.txt"
                Dim formFootPath As String = rootPath + "/breeding/includes/homepage/formbottom.txt"
                Dim formFootText As String = File.ReadAllText(formFootPath)
                Dim formText As StreamWriter = File.CreateText(formPath)
                Dim formHeadText As StreamWriter = File.CreateText(formHeadPath)

                Dim formOnlineCMD As SqlCommand = RacenetSql.buildCommand("p_RawFormOnline", conn)
                Dim formOnlineReader As SqlDataReader = formOnlineCMD.ExecuteReader
                If formOnlineReader.HasRows Then
                    Dim tabCount As Integer = 1
                    Dim RacenetClub As String = ""
                    Dim currentTextdate As String = ""
                    Dim currentTabIndex As Integer = 0
                    Dim racenetURL As String = ""
                    Dim meetingDateDif As Integer = 0
                    While formOnlineReader.Read
                        RacenetClub = RacenetSql.dbIsNull(formOnlineReader, "Racenet")
                        If RacenetClub = "Yes" Then
                            racenetURL = "rn"
                        Else
                            racenetURL = "nr"
                        End If
                        If meetingDateDif > 6 Then
                            currentTextdate = formOnlineReader("textdate")
                        End If
                        If Not currentTextdate = formOnlineReader("textdate") Then
                            currentTextdate = formOnlineReader("textdate")
                            meetingDateDif = (Convert.ToDateTime(formOnlineReader("mdate")) - Convert.ToDateTime(Now.ToString("yyyy-MM-dd"))).TotalDays
                            'Console.WriteLine(meetingDateDif.ToString)
                            If meetingDateDif = 0 Then
                                formHeadText.WriteLine("<li><a href=""#tabs-" + tabCount.ToString + """>Today</a></li>")
                            End If
                            If meetingDateDif = 1 Then
                                formHeadText.WriteLine("<li><a href=""#tabs-" + tabCount.ToString + """>Tomorrow</a></li>")
                            End If
                            If meetingDateDif > 1 And meetingDateDif < 7 Then
                                formHeadText.WriteLine("<li><a href=""#tabs-" + tabCount.ToString + """>" + Convert.ToDateTime(formOnlineReader("mdate")).ToString("dddd") + "</a></li>")
                            End If
                            If meetingDateDif > 6 Then
                                formHeadText.WriteLine("<li class=""liFuture""><a href=""#tabs-" + tabCount.ToString + """ class=""linkFuture"">Future</a></li>")
                            End If
                            tabCount += 1
                        End If
                        If Not currentTabIndex = tabCount - 1 Then
                            currentTabIndex = tabCount - 1
                            If currentTabIndex = 1 Then
                                formText.WriteLine("<div id=""tabs-" + currentTabIndex.ToString + """>")
                                formText.WriteLine("<table border=""0"" class=""homePageTabs"" cellpadding=""0"" cellspacing=""0"">")
                                formText.WriteLine("<tr>")
                                formText.WriteLine("<td class=""tdHead tdMeeting"">Track</td>")
                                formText.WriteLine("<td class=""tdHead tdTrack"">Conditions</td>")
                                formText.WriteLine("<td class=""tdHead"">Indexes</td>")
                                formText.WriteLine("<td class=""tdHead""></td>")
                                formText.WriteLine("<td class=""tdHead""></td>")
                                formText.WriteLine("<td class=""tdHead""></td>")
                                formText.WriteLine("</tr>")
                            Else
                                formText.WriteLine(formFootText)
                                formText.WriteLine("</table>")
                                formText.WriteLine("</div>")
                                formText.WriteLine("<div id=""tabs-" + currentTabIndex.ToString + """>")
                                formText.WriteLine("<table border=""0"" class=""homePageTabs"" cellpadding=""0"" cellspacing=""0"">")
                                formText.WriteLine("<tr>")
                                formText.WriteLine("<td class=""tdHead tdMeeting"">Track</td>")
                                formText.WriteLine("<td class=""tdHead tdTrack"">Conditions</td>")
                                formText.WriteLine("<td class=""tdHead"">Indexes</td>")
                                formText.WriteLine("<td class=""tdHead""></td>")
                                formText.WriteLine("<td class=""tdHead""></td>")
                                formText.WriteLine("<td class=""tdHead""></td>")
                                formText.WriteLine("</tr>")
                            End If
                        End If
                        Dim racebookLink As String = ""
                        If File.Exists(rootPath + "/racebooks/" + formOnlineReader("trackcode").ToString.Replace(" ", "") + "_" + formOnlineReader("textdate") + ".pdf") Then
                            racebookLink = "<a href=""/racebooks/" + formOnlineReader("trackcode").ToString.Replace(" ", "") + "_" + formOnlineReader("textdate") + ".pdf"" class=""linkBorderLeft link_red_under"">Racebook</a>"
                        End If
                        Dim formStatus As String = formOnlineReader("FormStatus")
                        Dim formStatusText As String = ""
                        If formStatus = "F" Then
                            formStatusText = "Fields"
                        End If
                        If formStatus = "W" Then
                            formStatusText = "Weights"
                        End If
                        If formStatus = "N" Then
                            formStatusText = "Nominations"
                        End If
                        If formStatus = "EN" Then
                            formStatusText = "Nominations"
                        End If
                        If formStatus = "EW" Then
                            formStatusText = "Weights"
                        End If
                        Dim trackCondition As String = RacenetSql.dbIsNull(formOnlineReader, "condition")

                        Dim showHover As Boolean = False
                        If trackCondition.Contains("grade") Then
                            showHover = True
                        End If
                        Dim postponed As Boolean = False
                        If trackCondition.Contains("Postponed") Then
                            postponed = True
                        End If
                        If trackCondition.Contains("Update") Then
                            trackCondition = (Left(trackCondition, trackCondition.IndexOf("Update") - 1))
                        End If
                        If trackCondition.Contains("Finalised") Then
                            trackCondition = (Left(trackCondition, trackCondition.IndexOf("Finalised") - 1))
                        End If
                        If trackCondition.Contains("Official") Then
                            trackCondition = (Left(trackCondition, trackCondition.IndexOf("Official") - 1))
                        End If
                        If trackCondition.Contains("Pen") Then
                            trackCondition = (Left(trackCondition, trackCondition.IndexOf("Pen") - 1))
                        End If
                        If postponed Then
                            trackCondition = "Postponed"
                        End If
                        Dim conditionHover As String = ""
                        If trackCondition.Length > 0 Then
                            trackCondition = trackCondition.Replace("GOOD", "Good ")
                            trackCondition = trackCondition.Replace("SLOW", "Slow ")
                            trackCondition = trackCondition.Replace("DEAD", "Dead ")
                            trackCondition = trackCondition.Replace("FAST", "Fast ")
                            trackCondition = trackCondition.Replace("HEAVY", "Heavy ")
                            trackCondition = trackCondition.Replace("FIRM", "Firm ")
                            trackCondition = trackCondition.Replace("SOFT", "Soft ")

                            Dim trackConRegex As Regex = New Regex("(Slow|Fast|Good|Dead|Heavy|Soft|Firm)(\s)*[0-9]+")
                            Dim trackConditionStart As String = trackConRegex.Match(trackCondition).Value
                            trackConRegex = New Regex("[0-9]+")
                            Dim trackConditionGrading As String = trackConRegex.Match(trackConditionStart).Value

                            Dim locationIndex As Integer = trackConditionStart.IndexOf(trackConditionGrading)
                            If locationIndex > 0 Then
                                If Not trackConditionStart.Chars(locationIndex - 1) = " " Then
                                    trackCondition = trackConditionStart.Substring(0, locationIndex).Trim + " " + trackConditionStart.Substring(locationIndex)
                                End If
                            End If

                            If showHover Then
                                conditionHover = "<span class='info_icon' title='" + formOnlineReader("Condition") + "'>i</span>"
                            End If
                        Else
                            trackCondition = "TBA"
                            conditionHover = ""
                        End If
                        Dim trackState As String = formOnlineReader("state")
                        If trackState = "hk" Then trackState = "hong-kong"
                        If trackState = "nz" Then trackState = "new-zealand"
                        If trackState = "sg" Then trackState = "singapore"
                        Dim trackname As String = formOnlineReader("displayname").ToString.ToLower.Replace(" ", "-")
                        Dim meetingDate As String = Convert.ToDateTime(formOnlineReader("mdate")).ToString("yyyy-MM-dd")
                        formText.WriteLine("<td class=""tdMeeting""><a class=""link_red_under"" href=""/tracks/" + trackState + "/" + trackname + "/"">" + formOnlineReader("DisplayName") + "</a></td>")
                        formText.WriteLine("<td class=""tdTrack SmallForm""><span class=""Heavy"" style='float: left'>" + trackCondition + "</span>" + conditionHover + "</td>")
                        formText.WriteLine("<td class=""tdTrack SmallForm""><a class='indexData' href='/form/gear-changes_new.asp?venue=" + formOnlineReader("DisplayName") + "&mdate=" + meetingDate + "&textdate=" + formOnlineReader("Textdate") + "&trackcode=" + formOnlineReader("Trackcode") + "&tfolder=" + formOnlineReader("Foldername") + "&fn=" + formOnlineReader("Foldername") + "'>G</a><a class='indexData' href='/form/alpha_new.asp?venue=" + formOnlineReader("DisplayName") + "&mdate=" + meetingDate + "&textdate=" + formOnlineReader("Textdate") + "&trackcode=" + formOnlineReader("Trackcode") + "&tfolder=" + formOnlineReader("FolderName") + "&fn=" + formOnlineReader("FolderName") + "'>H</a><a class='indexData' href='/form/jockey-sort_new.asp?venue=" + formOnlineReader("DisplayName") + "&mdate=" + meetingDate + "&textdate=" + formOnlineReader("Textdate") + "&trackcode=" + formOnlineReader("Trackcode") + "&tfolder=" + formOnlineReader("FolderName") + "&fn=" + formOnlineReader("FolderName") + "'>J</a><a class='indexData' href='/form/trainer-sort_new.asp?venue=" + formOnlineReader("DisplayName") + "&mdate=" + meetingDate + "&textdate=" + formOnlineReader("Textdate") + "&trackcode=" + formOnlineReader("Trackcode") + "&tfolder=" + formOnlineReader("FolderName") + "&fn=" + formOnlineReader("FolderName") + "'>T</a></td>")
                        formText.WriteLine("<td class=""tdField right""><a class=""linkBorderLeft link_red_under"" href=""/" + LCase(formOnlineReader("Foldername")) + "/race-fields/" + Replace(formOnlineReader("TrackCode"), " ", "-") + "/" + formOnlineReader("Textdate") + "/" + racenetURL + """>" + formStatusText + "</a></td>")
                        formText.WriteLine("<td class=""tdForm right""><a class=""linkBorderLeft link_red_under"" href=""/" + LCase(formOnlineReader("Foldername")) + "/race-form/" + Replace(formOnlineReader("TrackCode"), " ", "-") + "/" + formOnlineReader("Textdate") + """>Form</a></td>")
                        formText.WriteLine("<td class=""tdTips right""><a class=""linkBorderLeft link_red_under"" href=""/horse-racing-results/" + formOnlineReader("ResultsName") + "/" + meetingDate + """>Results</a></td>")
                        formText.WriteLine("<td class=""tdRacebooks right"">" + racebookLink + "</td>")
                        formText.WriteLine("</tr>")

                    End While
                    formText.WriteLine(formFootText)
                    formText.WriteLine("</table>")
                    formText.WriteLine("</div>")
                Else
                    formText.WriteLine("&nbsp;")
                End If
                RacenetSql.closeReader(formOnlineReader)
                RacenetSql.disposeCommand(formOnlineCMD)
                formHeadText.Close()
                formHeadText.Dispose()
                formHeadText = Nothing
                formText.Close()
                formText.Dispose()
                formText = Nothing

                '####################################################################################
                'Ratings Clean Up
                '####################################################################################

                Dim ratingsCleanUp As SqlCommand = RacenetSql.buildCommand("rn_CleanUpRatingsOff", conn)
                ratingsCleanUp.Parameters.Add(New SqlParameter("@RaceDate", dbDate))
                ratingsCleanUp.ExecuteNonQuery()
                RacenetSql.disposeCommand(ratingsCleanUp)

                Dim ratingsGetCountCMD As SqlCommand = RacenetSql.buildCommand("RA_RatingsOnlineCount", conn)
                Dim ratingsGetCountReader As SqlDataReader = ratingsGetCountCMD.ExecuteReader
                Dim ratingsMeetingsOnline As Integer = 0
                If ratingsGetCountReader.HasRows Then
                    ratingsGetCountReader.Read()
                    ratingsMeetingsOnline = ratingsGetCountReader("RatingsMeetings")
                End If
                RacenetSql.closeReader(ratingsGetCountReader)
                RacenetSql.disposeCommand(ratingsGetCountCMD)

                Dim ratingsSplit As Integer = 0
                If ratingsMeetingsOnline > 2 Then
                    ratingsSplit = Math.Round(ratingsMeetingsOnline / 2)
                Else
                    ratingsSplit = 1000
                End If
                Dim ratingsHomePath As String = rootPath + "/breeding/includes/homepage/ratingsmeetings.txt"
                Dim ratingsHomeFile As StreamWriter = File.CreateText(ratingsHomePath)
                Dim cRatingsCount As Integer = 1
                Dim currentRatingsDate As String = ""

                Dim onlineRatingsMeetingsCMD As SqlCommand = RacenetSql.buildCommand("rn_ratingsOnline", conn)
                Dim onlineRatingsMeetingsReader As SqlDataReader = onlineRatingsMeetingsCMD.ExecuteReader
                If onlineRatingsMeetingsReader.HasRows Then
                    While onlineRatingsMeetingsReader.Read
                        If cRatingsCount = ratingsSplit + 1 Then
                            ratingsHomeFile.WriteLine("</ul>")
                            ratingsHomeFile.WriteLine("</td>")
                            ratingsHomeFile.WriteLine("<td valign=""top"">")
                            ratingsHomeFile.WriteLine("<ul>")
                        End If
                        cRatingsCount += 1
                        If Not currentRatingsDate = onlineRatingsMeetingsReader("RaceDate").ToString Then
                            currentRatingsDate = onlineRatingsMeetingsReader("RaceDate").ToString
                            ratingsHomeFile.WriteLine("<li class=""prod_list_head"">" + Convert.ToDateTime(currentRatingsDate).ToString("dddd") + " - " + Convert.ToDateTime(currentRatingsDate).ToString("d") + " " + Convert.ToDateTime(currentRatingsDate).ToString("MMMM") + "</li>")
                        End If
                        ratingsHomeFile.WriteLine("<li><a class=""link_white"" href=""http://www.racenet.com.au/horse-racing-tips/ratings/"">" + onlineRatingsMeetingsReader("DisplayName").ToString.ToUpper + "</a></li>")
                    End While
                Else
                    ratingsHomeFile.WriteLine("")
                End If
                RacenetSql.closeReader(onlineRatingsMeetingsReader)
                RacenetSql.disposeCommand(onlineRatingsMeetingsCMD)
                ratingsHomeFile.Close()
                ratingsHomeFile.Dispose()
                ratingsHomeFile = Nothing

                '####################################################################################
                'Raceday Clean Up
                '####################################################################################

                Dim racedayCleanup As SqlCommand = RacenetSql.buildCommand("rn_CleanUpRaceDayOff", conn)
                racedayCleanup.Parameters.Add(New SqlParameter("@TextDate", textdate))
                racedayCleanup.ExecuteNonQuery()
                RacenetSql.disposeCommand(racedayCleanup)

                Dim racedayPath As String = rootPath + "/breeding/includes/homepage/racedaymeetings.txt"
                Dim racedayFile As StreamWriter = File.CreateText(racedayPath)

                Dim racedayCurrentDate As String = ""

                Dim racedayMeetingsOnlineCMD As SqlCommand = RacenetSql.buildCommand("rn_RatingsTipsMeetingsOnline", conn)
                Dim racedayMeetingsOnlineReader As SqlDataReader = racedayMeetingsOnlineCMD.ExecuteReader
                If racedayMeetingsOnlineReader.HasRows Then
                    While racedayMeetingsOnlineReader.Read
                        If Not racedayCurrentDate = racedayMeetingsOnlineReader("mdate").ToString Then
                            racedayCurrentDate = racedayMeetingsOnlineReader("mdate").ToString
                            racedayFile.WriteLine("<li class=""prod_list_head"">" + Convert.ToDateTime(racedayCurrentDate).ToString("dddd") + " - " + Convert.ToDateTime(racedayCurrentDate).ToString("d") + " " + Convert.ToDateTime(racedayCurrentDate).ToString("MMMM") + "</li>")
                        End If
                        racedayFile.WriteLine("<li><a class=""link_white"" href=""http://www.racenet.com.au/horse-racing-tips/raceday/"">" + racedayMeetingsOnlineReader("Venue").ToString.ToUpper + "</a></li>")
                    End While
                Else
                    racedayFile.WriteLine("<p>Next meetings available soon.</p>")
                End If
                RacenetSql.closeReader(racedayMeetingsOnlineReader)
                RacenetSql.disposeCommand(racedayMeetingsOnlineCMD)
                racedayFile.Close()
                racedayFile.Dispose()
                racedayFile = Nothing

                '####################################################################################
                'Gear changes Clean Up
                '####################################################################################

                Dim gearChangesCleanup As SqlCommand = RacenetSql.buildCommand("rn_CleanUpDeleteGears", conn)
                gearChangesCleanup.Parameters.Add(New SqlParameter("@TextDate", textdate))
                gearChangesCleanup.ExecuteNonQuery()
                RacenetSql.disposeCommand(gearChangesCleanup)

                '####################################################################################
                'Premium Form Clean Up
                '####################################################################################

                Dim premiumFormCleanup As SqlCommand = RacenetSql.buildCommand("pf_DeleteCleanUp", conn)
                premiumFormCleanup.Parameters.Add(New SqlParameter("@t_date", premiumFormDate))
                premiumFormCleanup.ExecuteNonQuery()
                RacenetSql.disposeCommand(premiumFormCleanup)

                '####################################################################################
                'Ratings Centre Clean Up
                '####################################################################################

                Dim ratingsCentreCleanup As SqlCommand = RacenetSql.buildCommand("RA_RatingsCentreCleanUp", conn)
                ratingsCentreCleanup.CommandTimeout = 500
                ratingsCentreCleanup.ExecuteNonQuery()
                RacenetSql.disposeCommand(ratingsCentreCleanup)

                '####################################################################################
                'Horses To Follow Clean Up
                '####################################################################################

                Dim dateSixMonths As String = Now.AddMonths(-6).ToString("yyyy-MM-dd")
                'Console.WriteLine(dateSixMonths)
                Dim horsesToFollowCleanup As SqlCommand = RacenetSql.buildCommand("rn_vcHorsesToFollowCleanup", conn)
                horsesToFollowCleanup.Parameters.Add(New SqlParameter("@mdate", dateSixMonths))
                horsesToFollowCleanup.CommandTimeout = 120
                horsesToFollowCleanup.ExecuteNonQuery()
                RacenetSql.disposeCommand(horsesToFollowCleanup)

                '####################################################################################
                'Wet Sires Clean Up
                '####################################################################################

                Dim wetSiresCleanup As SqlCommand = RacenetSql.buildCommand("rn_wetsirestatsdailytupdate", conn)
                wetSiresCleanup.ExecuteNonQuery()
                RacenetSql.disposeCommand(wetSiresCleanup)

                '####################################################################################
                'Ratings Dupilicates Clean Up
                '####################################################################################

                'Dim ratingsDupilicatesCleanup As SqlCommand = RacenetSql.buildCommand("p_RatingsDupDelete", conn)
                'ratingsDupilicatesCleanup.CommandTimeout = 500
                'ratingsDupilicatesCleanup.ExecuteNonQuery()
                'RacenetSql.disposeCommand(ratingsDupilicatesCleanup)

                RacenetSql.closeConnection(conn)
            End If


            '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            '+++++++++++++++++++++++++++++++ File Deletion +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            '####################################################################################
            'Form Folders
            '####################################################################################

            Dim formFolderDeletePath As String = rootPath + "/breeding/form"
            Dim formFolders As String() = Directory.GetDirectories(formFolderDeletePath)
            For Each formFolder As String In formFolders
                'Console.WriteLine(formFolder)
                If formFolder.Contains(textdate) Then
                    Directory.Delete(formFolder, True)
                End If
            Next
            formFolders = Nothing

            formFolderDeletePath = rootPath + "/breeding/form/beta"
            formFolders = Directory.GetDirectories(formFolderDeletePath)
            For Each formFolder As String In formFolders
                If formFolder.Contains(textdate) Then
                    Directory.Delete(formFolder, True)
                End If
            Next
            formFolders = Nothing

            '####################################################################################
            'Racebooks Folder
            '####################################################################################

            Dim racebooksDeletePath As String = rootPath + "/racebooks"
            Dim racebooksFiles As String() = Directory.GetFiles(racebooksDeletePath)
            For Each racebookFile As String In racebooksFiles
                If racebookFile.Contains(textdate) Then
                    File.Delete(racebookFile)
                End If
            Next
            racebooksFiles = Nothing

            '####################################################################################
            'Gear Changes Folder
            '####################################################################################

            Dim gearChangesDeletePath As String = rootPath + "/Gear-Changes"
            Dim gearChangesFiles As String() = Directory.GetFiles(gearChangesDeletePath)
            For Each gearChangesFile As String In gearChangesFiles
                If gearChangesFile.Contains(textdate) Then
                    File.Delete(gearChangesFile)
                End If
            Next
            gearChangesFiles = Nothing

            '####################################################################################
            'Racenet Tips Folder
            '####################################################################################

            Dim racenetTipsDeletePath As String = rootPath + "/racenet-tips"
            Dim racenetTipsFiles As String() = Directory.GetFiles(racenetTipsDeletePath)
            For Each racenetTipsFile As String In racenetTipsFiles
                If racenetTipsFile.Contains(textdate) Then
                    File.Delete(racenetTipsFile)
                End If
            Next
            racenetTipsFiles = Nothing

            '####################################################################################
            'Southcoast Folder
            '####################################################################################

            Dim southcoastDeletePath As String = rootPath + "/SouthCoastFlx/Processed"
            Dim southcoastFiles As String() = Directory.GetFiles(southcoastDeletePath)
            For Each southcoastFile As String In southcoastFiles
                'Console.WriteLine(southcoastDeletePath + "/" + southcoastFile)
                Dim southcoastDateModified As Date = File.GetLastWriteTime(southcoastFile)
                If (Now - southcoastDateModified).TotalDays > 7 Then
                    File.Delete(southcoastFile)
                End If
            Next
            southcoastFiles = Nothing

            '####################################################################################
            'Form 7 Days Folder
            '####################################################################################

            Dim oldFormDeletePath As String = rootPath + "/breeding/form"
            Dim oldFormFolders As String() = Directory.GetDirectories(oldFormDeletePath)
            For Each oldFormFolder As String In oldFormFolders
                Dim oldFormDateModified As Date = Directory.GetLastWriteTime(oldFormFolder)
                If (Now - oldFormDateModified).TotalDays > 7 Then
                    Directory.Delete(oldFormFolder, True)
                End If
            Next
            oldFormFolders = Nothing

            oldFormDeletePath = rootPath + "/breeding/form/beta"
            oldFormFolders = Directory.GetDirectories(oldFormDeletePath)
            For Each oldFormFolder As String In oldFormFolders
                Dim oldFormDateModified As Date = File.GetLastWriteTime(oldFormFolder)
                If (Now - oldFormDateModified).TotalDays > 7 Then
                    Directory.Delete(oldFormFolder, True)
                End If
            Next
            oldFormFolders = Nothing

            '####################################################################################
            'Fields 7 Days Folder
            '####################################################################################

            Dim oldFieldsDeletePath As String = rootPath + "/breeding/fields"
            Dim oldFieldsFolders As String() = Directory.GetDirectories(oldFieldsDeletePath)
            For Each oldFieldsFolder As String In oldFieldsFolders
                Dim oldFieldsDateModified As Date = Directory.GetLastWriteTime(oldFieldsFolder)
                If (Now - oldFieldsDateModified).TotalDays > 7 Then
                    Directory.Delete(oldFieldsFolder, True)
                End If
            Next
            oldFieldsFolders = Nothing

            '####################################################################################
            'Sportsbet Odds Folder
            '####################################################################################

            Dim sportsbetOddsDeletePath As String = rootPath + "/breeding/sportsbetodds"
            Dim sportsbetOddsFiles As String() = Directory.GetFiles(sportsbetOddsDeletePath)
            For Each sportsbetOddsFile As String In sportsbetOddsFiles
                Dim sportsbetOddsDateModified As Date = File.GetLastWriteTime(sportsbetOddsFile)
                If (Now - sportsbetOddsDateModified).TotalDays > 7 Then
                    File.Delete(sportsbetOddsFile)
                End If
            Next
            sportsbetOddsFiles = Nothing

            '####################################################################################
            'Ladbrokes Odds Folder
            '####################################################################################

            Dim ladbrokesOddsDeletePath As String = rootPath + "/breeding/ladbrokesodds"
            Dim ladbrokesOddsFiles As String() = Directory.GetFiles(ladbrokesOddsDeletePath)
            For Each ladbrokesOddsFile As String In ladbrokesOddsFiles
                Dim ladbrokesOddsDateModified As Date = File.GetLastWriteTime(ladbrokesOddsFile)
                If (Now - ladbrokesOddsDateModified).TotalDays > 7 Then
                    File.Delete(ladbrokesOddsFile)
                End If
            Next
            ladbrokesOddsFiles = Nothing

            '####################################################################################
            'RSB Processed Folder
            '####################################################################################

            Dim rsbScratchDeletePath As String = rootPath + "/rsbScratchXml/Processed"
            Dim rsbScratchFiles As String() = Directory.GetFiles(rsbScratchDeletePath)
            For Each rsbScratchFile As String In rsbScratchFiles
                Dim rsbScratchDateModified As Date = File.GetLastWriteTime(rsbScratchFile)
                If (Now - rsbScratchDateModified).TotalDays > 7 Then
                    File.Delete(rsbScratchFile)
                End If
            Next
            rsbScratchFiles = Nothing

            '####################################################################################
            'Free Tip Reg Folder
            '####################################################################################

            Dim freeTipsRegDeletePath As String = rootPath + "/breeding/freetipsreg"
            Dim freeTipsRegFiles As String() = Directory.GetFiles(freeTipsRegDeletePath)
            For Each freeTipsRegFile As String In freeTipsRegFiles
                Dim freeTipsRegDateModified As Date = File.GetLastWriteTime(freeTipsRegFile)
                If (Now - freeTipsRegDateModified).TotalDays > 7 Then
                    File.Delete(freeTipsRegFile)
                End If
            Next
            freeTipsRegFiles = Nothing

            '####################################################################################
            'Account Reg Folder
            '####################################################################################

            Dim accountRegDeletePath As String = rootPath + "/breeding/pendingRegistrations"
            Dim accountRegFiles As String() = Directory.GetFiles(accountRegDeletePath)
            For Each accountRegFile As String In accountRegFiles
                Dim accountRegDateModified As Date = File.GetLastWriteTime(accountRegFile)
                If (Now - accountRegDateModified).TotalDays > 7 Then
                    File.Delete(accountRegFile)
                End If
            Next
            accountRegFiles = Nothing

            '####################################################################################
            'Racing Australia Folders
            '####################################################################################

            Dim risaFolders As String() = {"Acceptances", "Calendar", "Final Fields", "Nominations", "NZ Acceptances Gallops", "Rider Update 1.1", "Weights"}

            For i As Integer = 0 To risaFolders.Length - 1 Step 1
                Dim risaFoldersPath As String = rootPath + "/rsbScratchXML/" + risaFolders(i) + "/done"
                Dim risaFoldersFiles As String() = Directory.GetFiles(risaFoldersPath)
                For Each risaFoldersFile As String In risaFoldersFiles
                    Dim risaFoldersDateModified As Date = File.GetLastWriteTime(risaFoldersFile)
                    If (Now - risaFoldersDateModified).TotalDays > 7 Then
                        File.Delete(risaFoldersFile)
                    End If
                Next
                risaFoldersFiles = Nothing
            Next

            '####################################################################################
            'aap Form Folder
            '####################################################################################

            Dim aapFormDeletePath As String = rootPath + "/aap-form/Processed"
            Dim aapFormFiles As String() = Directory.GetFiles(aapFormDeletePath)
            For Each aapFormFile As String In aapFormFiles
                Dim aapFormDateModified As Date = File.GetLastWriteTime(aapFormFile)
                If (Now - aapFormDateModified).TotalDays > 7 Then
                    File.Delete(aapFormFile)
                End If
            Next
            aapFormFiles = Nothing

            '####################################################################################
            'Gear Files Folder
            '####################################################################################

            Dim gearFilesDeletePath As String = rootPath + "/GearsFiles/Processed"
            Dim gearFilesFiles As String() = Directory.GetFiles(gearFilesDeletePath)
            For Each gearFilesFile As String In gearFilesFiles
                Dim gearFilesDateModified As Date = File.GetLastWriteTime(gearFilesFile)
                If (Now - gearFilesDateModified).TotalDays > 14 Then
                    File.Delete(gearFilesFile)
                End If
            Next
            gearFilesFiles = Nothing

            '####################################################################################
            'Interim Results Clean Up
            '####################################################################################

            Dim interimResultsPath As String = rootPath + "/breeding/includes/homepage/interimResults.txt"
            If File.Exists(interimResultsPath) Then
                File.Delete(interimResultsPath)
            End If

            'Console.ReadLine()

            '####################################################################################
            'Breednet PremiumXML Folder
            '####################################################################################

            Dim premiumXmlDeletePath As String = rootPathBreednet + "/PremiumXml"
            Dim premiumXmlFiles As String() = Directory.GetFiles(premiumXmlDeletePath)
            For Each premiumXmlFile As String In premiumXmlFiles
                Dim premiumXmlDateModified As Date = File.GetLastWriteTime(premiumXmlFile)
                If (Now - premiumXmlDateModified).TotalDays > 7 And premiumXmlFile.EndsWith(".xml") Then
                    File.Delete(premiumXmlFile)
                End If
            Next
            premiumXmlFiles = Nothing

            '####################################################################################
            'Breednet premiumXml2 Folder
            '####################################################################################

            Dim premiumXml2DeletePath As String = rootPathBreednet + "/PremiumXml2"
            Dim premiumXml2Files As String() = Directory.GetFiles(premiumXml2DeletePath)
            For Each premiumXml2File As String In premiumXml2Files
                Dim premiumXml2DateModified As Date = File.GetLastWriteTime(premiumXml2File)
                If (Now - premiumXml2DateModified).TotalDays > 14 And premiumXml2File.EndsWith(".xml") Then
                    File.Delete(premiumXml2File)
                End If
            Next
            premiumXml2Files = Nothing

            '####################################################################################
            'Premium Form & Luxbet Cleanup
            '####################################################################################

            Dim request As HttpWebRequest = HttpWebRequest.Create("http://www.premiumform.com.au/CleanUP.asp?pfxml=" + premiumFormXMLDate + "&ctextdate=" + textdate)
            Dim response As WebResponse = request.GetResponse()
            Dim dataStream As Stream = response.GetResponseStream()
            Dim reader As New StreamReader(dataStream)
            Dim responseFromServer As String = reader.ReadToEnd()
            reader.Close()
            response.Close()
            responseFromServer = responseFromServer.Substring(responseFromServer.IndexOf("<body onLoad=""closeWindow();"">") + 31, responseFromServer.IndexOf("</body>") - responseFromServer.IndexOf("<body onLoad=""closeWindow();"">") - 31)

            Dim smtp As SmtpClient = racenetgeneral.smtpmailserver()
            Dim sendemail As Mail.MailMessage = New Mail.MailMessage
            sendemail.From = New Mail.MailAddress("cleanUp@racenet.com.au")
            sendemail.To.Add(New Mail.MailAddress("ramy@racenet.com.au"))
            sendemail.CC.Add(New Mail.MailAddress("nathan@racenet.com.au"))
            sendemail.Subject = "Clean Up Success"
            sendemail.IsBodyHtml = False
            sendemail.Body = "Clean up ran successfully " + textdate + "." + vbNewLine + " Response from Premium Form & Luxbet:" + vbNewLine + responseFromServer
            smtp.Send(sendemail)
            sendemail.Dispose()
            smtp = Nothing

            '####################################################################################
            'Racenet Mobile
            '####################################################################################

            Dim directoryList As String() = Directory.GetDirectories(rootPathrnMobile + "/form/meetings")
            For i As Integer = 0 To directoryList.Length - 1 Step 1
                If (directoryList(i).Contains("_" + deldate.ToString("ddMMyy"))) Then
                    Directory.Delete(directoryList(i), True)
                End If
            Next

            '####################################################################################
            'DONE
            '####################################################################################


        Catch ex As Exception
            Dim smtp As SmtpClient = racenetgeneral.smtpmailserver()
            Dim sendemail As Mail.MailMessage = New Mail.MailMessage
            sendemail.From = New Mail.MailAddress("cleanUp@racenet.com.au")
            sendemail.To.Add(New Mail.MailAddress("ramy@racenet.com.au"))
            sendemail.CC.Add(New Mail.MailAddress("nathan@racenet.com.au"))
            sendemail.Subject = "Clean Up Error"
            sendemail.IsBodyHtml = True
            sendemail.Body = "Clean up error</ br> " + ex.Message + "</br>" + ex.StackTrace
            smtp.Send(sendemail)
            sendemail.Dispose()
            smtp = Nothing
        End Try
    End Sub

End Module
