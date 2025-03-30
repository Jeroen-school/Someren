namespace Someren.Models
{
    public class ActivityParticipant
    {
        public string ActivityType { get; set; }
        public int StudentNum { get; set; }

        public ActivityParticipant() { }

        public ActivityParticipant(string activityType, int studentNum)
        {
            ActivityType = activityType;
            StudentNum = studentNum;
        }
    }
}