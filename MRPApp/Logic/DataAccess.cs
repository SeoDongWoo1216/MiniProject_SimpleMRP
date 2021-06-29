using MRPApp.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRPApp.Logic
{
    public class DataAccess
    {

        ////////////////////////////////// 설정(Setting) DB //////////////////////////////////////////////////

        
        public static List<Settings> GetSettings()
        {
            // 세팅 테이블에서 데이터 가져오기
            List<Model.Settings> list;

            using (var ctx = new MRPEntities())
                list = ctx.Settings.ToList();  // Settings에있는 DB 데이터를 가져와서 list에 넣어줌.  (SELECT)

            return list;  // DB데이터가 담긴 list를 반환
        }

        internal static int Setsettings(Settings item)  // object -> int로 바꿔줬음
        {
            using (var ctx = new MRPEntities())
            {
                ctx.Settings.AddOrUpdate(item);  // Insert or Update = AddOrUpdate 임. (데이터 삽입)
                return ctx.SaveChanges();        // COMMIT
            }
        }

        internal static int DelSettings(Settings item)
        {
            using(var ctx = new MRPEntities())
            {
                var obj = ctx.Settings.Find(item.BasicCode);  // 삭제할 데이터를 검색해서 그 검색된 데이터를 삭제함
                ctx.Settings.Remove(obj);     // obj를 Delete = Remove (데이터 삭제)
                return ctx.SaveChanges();
            }
        }



        ////////////////////////////////// 공정계획(Schedules) DB //////////////////////////////////////////////////


        internal static List<Schedules> GetSchedules()
        {
            List<Model.Schedules> list;

            using (var ctx = new MRPEntities())
                list = ctx.Schedules.ToList();  // Schdules에있는 DB 데이터를 가져와서 list에 넣어줌. (SELECT)

            return list;  // DB데이터가 담긴 list를 반환
        }

        internal static int SetSchedules(Schedules item)
        {
            using (var ctx = new MRPEntities())
            {
                ctx.Schedules.AddOrUpdate(item);  // Insert or Update = AddOrUpdate 임. (데이터 삽입)
                return ctx.SaveChanges();         // COMMIT
            }
        }


        //////////////////////////////////  프로세스(Process) DB  /////////////////////////////////


        internal static List<Process> GetProcesses()
        {
            List<Model.Process> list;

            using(var ctx = new MRPEntities())
            {
                list = ctx.Process.ToList(); // SELECT
            }
            return list;
        }

        internal static int SetProcesses(Process item)
        {
            using (var ctx = new MRPEntities())
            {
                ctx.Process.AddOrUpdate(item);  // INSERT | UPDATE
                return ctx.SaveChanges();    // COMMIT
            }

        }
    }
}
