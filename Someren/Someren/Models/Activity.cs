namespace Someren.Models
{
    public class Activity
    {

        public string Activitytype { get; set; }
        public DateTime Date { get; set; }

        public TimeSpan Time { get; set; }

        public Activity()
        {
           
        }


        public Activity(string activitytype, DateTime date, TimeSpan time)
        {
            Activitytype = activitytype;
            Date = date;
            Time = time;
        }


    }
}
