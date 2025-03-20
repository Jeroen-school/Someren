namespace Someren.Models
{
    public class Activity
    {
        public Activity()
        {
            Date = DateTime.Today;
        }

        public Activity(string activitytype, DateTime date, TimeSpan time)
        {
            Activitytype = activitytype;
            Date = (date < new DateTime(2018, 1, 1)) ? DateTime.Today : date;
            Time = time;
        }

        public string Activitytype { get; set; }
        public DateTime Date { get; set; }

        public TimeSpan Time { get; set; }

        


       


    }
}
