using System;
using Plugin.Connectivity;

namespace Bloggy.Helpers
{
    public static class Constant
    {
        public static string AdminRole = "Admin";
        public static string UserRole = "User";

        public static bool CheckConnectivity()
        {
            return CrossConnectivity.Current.IsConnected;
        }

        public static string GetTimeMessage(DateTime startdate)
        {
            string time_message = "";
            DateTime enddate = DateTime.Now;
            TimeSpan diff = enddate.Subtract(startdate);

            if ((enddate.Year - startdate.Year) >= 1)
            {
                if ((enddate.Year - startdate.Year) == 1)
                    time_message += (enddate.Year - startdate.Year).ToString() + " year ago.";
                else
                    time_message += (enddate.Year - startdate.Year).ToString() + " years ago.";
            }
            else if ((enddate.Month - startdate.Month) >= 1)
            {
                if ((enddate.Month - startdate.Month) == 1)
                    time_message += (enddate.Month - startdate.Month).ToString() + " month ago.";
                else
                    time_message += (enddate.Month - startdate.Month).ToString() + " months ago.";
            }
            else if ((enddate.Day - startdate.Day) >= 1)
            {
                if ((enddate.Day - startdate.Day) == 1)
                    time_message += (enddate.Day - startdate.Day).ToString() + " day ago.";
                else
                    time_message += (enddate.Day - startdate.Day).ToString() + " days ago.";
            }
            else if ((enddate.Hour - startdate.Hour) >= 1)
            {
                if ((enddate.Hour - startdate.Hour) == 1)
                    time_message += (enddate.Hour - startdate.Hour).ToString() + " hour ago.";
                else
                    time_message += (enddate.Hour - startdate.Hour).ToString() + " hours ago.";
            }
            else if ((enddate.Minute - startdate.Minute) >= 1)
            {
                if ((enddate.Minute - startdate.Minute) == 1)
                    time_message += (enddate.Minute - startdate.Minute).ToString() + " minute ago.";
                else
                    time_message += (enddate.Minute - startdate.Minute).ToString() + " minutes ago.";
            }
            else if ((enddate.Second - startdate.Second) >= 1)
            {
                if ((enddate.Second - startdate.Second) == 1)
                    time_message += (enddate.Second - startdate.Second).ToString() + " just now.";
                else
                    time_message += (enddate.Second - startdate.Second).ToString() + " seconds ago.";
            }
            return time_message;
        }
    }
}
