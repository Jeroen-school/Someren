namespace Someren.Models
{
    public class ActivitySupervisorsViewModel
    {
        //fields and properties
        public Activity Activity {  get; set; }
        public List<Lecturer> SupervisingLecturers { get; set; }

        public List<Lecturer> OtherLecturers { get; set; }

        //constructors
        public ActivitySupervisorsViewModel(Activity activity, List<Lecturer> supervisingLecturers, List<Lecturer> otherLecturers)
        {
            this.Activity = activity;
            this.SupervisingLecturers = supervisingLecturers;
            this.OtherLecturers = otherLecturers;
        }


        //methods

    }
}
