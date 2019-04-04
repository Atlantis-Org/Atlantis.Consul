using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Atlantis.Consul.Utilies
{
    public class Timer
    {
        public static Timer Instance = new Timer();

        private readonly IDictionary<string, Schedule> _schedules;
        private int _isrunning = 0;
        private Task _timer;
        private readonly int _scanPreiod = 800;//util: ms

        private Timer()
        {
            _schedules = new Dictionary<string, Schedule>();
        }

        public Timer Register(Schedule schedule)
        {
            TryRunning();

            if (_schedules.ContainsKey(schedule.Name))
            {
                _schedules[schedule.Name] = schedule;
            }
            else
            {
                _schedules.Add(schedule.Name, schedule);
            }

            return this;
        }

        private void TryRunning()
        {
            if (!EnterRunning()) return;

            _timer = new Task(Running, TaskCreationOptions.LongRunning);
            _timer.Start();
        }

        private void Running()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(_scanPreiod);
                var schedules = _schedules.Values
                    .Where(p => p.NextExecuteTime <= DateTime.UtcNow)
                    .ToList();
                if (schedules == null || schedules.Count == 0)
                    continue;

                foreach (var schedule in schedules)
                {
                    schedule.Task.Invoke();
                    ExpandNextTime(schedule);
                }

            }
        }

        private void ExpandNextTime(Schedule schedule)
        {
            if (schedule.NextExecuteTime == DateTime.MinValue)
                schedule.NextExecuteTime = DateTime.UtcNow;

            schedule.NextExecuteTime = BuildTimeForCron(
                schedule.NextExecuteTime, schedule.CronTime);
            // _log.LogInformation($"Next execute {schedule.Name} time: {schedule.NextExecuteTime.ToString("yyyy-MM-dd HH:mm:ss")}");
        }


        private bool EnterRunning()
        {
            return Interlocked.CompareExchange(
                ref _isrunning, 1, 0) == 0;
        }

        private void ExitRunning()
        {
            Interlocked.Exchange(ref _isrunning, 0);
        }

        public static DateTime BuildTimeForCron(DateTime startTime,string cron)
        {
            var cronArr=cron.Split(' ');
            var seconds=Decode(cronArr[0], 60);
            if(seconds[0]==1)
            {
                return startTime.AddSeconds(seconds[2]);
            }
            
            var minutes=Decode(cronArr[1], 60);
            if(minutes[0]==1)
            {
                startTime= startTime.AddMinutes(minutes[2]);
                return new DateTime(startTime.Year,startTime.Month,startTime.Day,
                                           startTime.Hour,startTime.Minute,seconds[1]);
            }
            
            var hours=Decode(cronArr[2], 24);
            if(hours[0]==1)
            {
                startTime= startTime.AddHours(hours[2]);
                return new DateTime(startTime.Year,startTime.Month,startTime.Day,
                                           startTime.Hour,minutes[1],seconds[1]);
            }
            
            var days=Decode(cronArr[3], 31);
            if(days[0]==1)
            {
                startTime= startTime.AddDays(days[2]);
                return new DateTime(startTime.Year,startTime.Month,startTime.Day,
                                           hours[1],minutes[1],seconds[1]);
            }
            
            var months=Decode(cronArr[4], 13);
            if(months[0]==1)
            {
                startTime= startTime.AddMonths(months[2]);
                return new DateTime(startTime.Year,startTime.Month,days[1],
                                           hours[1],minutes[1],seconds[1]);
            }
            
            var weeks=Decode(cronArr[5], 8);
            var years=Decode(cronArr[6], 2099);
            if(minutes[0]==1)
            {
                startTime= startTime.AddYears(years[2]);
                return new DateTime(startTime.Year,months[1],days[1],
                                           hours[1],minutes[1],seconds[1]);
            }
            return startTime;
        }

        private static IList<int> Decode(string cronItem,int frequecy)
        {
            var values=new List<int>();
            if(cronItem=="*")
            {
                values.Add(0);
                values.Add(0);
                values.Add(0);
                return values;
            }
            if(cronItem.Contains("/"))
            {
                var itemArr=cronItem.Split('/');
                values.Add(1);
                values.Add(int.Parse(itemArr[0]));
                values.Add(int.Parse(itemArr[1]));
                return values;
            }

            var value=0;
            int.TryParse(cronItem, out value);
            values.Add(3);
            values.Add(value);
            return values;
        }

    }

    public class Schedule
    {
        public string Name { get; set; }

        public Func<Task> Task { get; set; }

        public string CronTime { get; set; }

        public DateTime NextExecuteTime { get; set; }
    }
}
