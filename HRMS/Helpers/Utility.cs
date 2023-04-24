namespace HRMS.Helpers
{
    public class Utility
    {

        public static DateTime GetMMTimeNow()
        {
            var serverTime = DateTime.Now;
            var mmTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(serverTime,
                                        TimeZoneInfo.Local.Id,
                                        "Myanmar Standard Time");
            return mmTime;
        }
    }
}
