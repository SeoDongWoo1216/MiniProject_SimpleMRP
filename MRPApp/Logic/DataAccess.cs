using MRPApp.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
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


        /////////////////////////////  Report와 관련된 DB  //////////////////////////////////////////
        
        
        internal static List<Model.Report> GetReportDatas(string startDate, string endDate, string plantCode)
        {
            // 반환타입을 object -> List<Report>로 변경 
            var connString = ConfigurationManager.ConnectionStrings["MRPConnString"].ToString();
            var list = new List<Report>();
            var lastObj = new Model.Report(); // 추가 : 최종 Report값 담는 변수

            using (var conn = new SqlConnection(connString))
            {
                conn.Open();  // Sql을 사용하기위해 무조건 필요함!

                // 원하는 값을 불러오기위한 쿼리(조인, 서브쿼리 사용)
                var sqlQuery = $@"SELECT sch.SchIdx, sch.PlantCode, sch.SchAmount, prc.PrcDate,
                                           prc.PrcOkAmount, prc.PrcFailAmount
                                    FROM Schedules AS sch
                                  INNER JOIN(
                                                SELECT smr.SchIdx, smr.PrcDate,
                                                       SUM(smr.PrcOK) AS PrcOkAmount, SUM(smr.PrcFail) AS PrcFailAmount

                                                  FROM(
                                                        SELECT p.SchIdx, p.PrcDate,
                                                                CASE p.PrcResult WHEN 1 THEN 1 ELSE 0 END AS PrcOK,
                                                                CASE p.PrcResult WHEN 0 THEN 1 ELSE 0 END AS PrcFail
                                                          FROM Process AS p
                                                       ) AS smr
                                                GROUP BY smr.SchIdx, smr.PrcDate
                                             ) AS prc
                                      ON sch.SchIdx = Prc.SchIdx
                                   WHERE sch.PlantCode = '{plantCode}'
                                     AND prc.PrcDate BETWEEN '{startDate}' AND '{endDate}'";


                SqlCommand cmd = new SqlCommand(sqlQuery, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var tmp = new Report
                    {
                        SchIdx = (int)reader["SchIdx"],
                        PlantCode = reader["PlantCode"].ToString(),
                        PrcDate = DateTime.Parse(reader["PrcDate"].ToString()),
                        SchAmount = (int)reader["SchAmount"],
                        PrcOkAmount = (int)reader["PrcOkAmount"],
                        PrcFailAmount = (int)reader["PrcFailAmount"]
                    };
                    list.Add(tmp);
                    lastObj = tmp; // 마지막 값을 할당
                }

                // 시작일부터 종료일까지 없는 값 만들어주는 로직
                var DtStart = DateTime.Parse(startDate);
                var DtEnd = DateTime.Parse(endDate);
                var DtCurrent = DtStart;

                while (DtCurrent < DtEnd)
                {
                    var count = list.Where(c => c.PrcDate.Equals(DtCurrent)).Count();
                    if (count == 0)
                    {
                        // 새로운 Report(없는 날짜)
                        var tmp = new Report
                        {
                            SchIdx = lastObj.SchIdx,
                            PlantCode = lastObj.PlantCode,
                            PrcDate = DtCurrent,
                            SchAmount = 0,
                            PrcOkAmount = 0,
                            PrcFailAmount = 0
                        };
                        list.Add(tmp);
                    }
                    DtCurrent = DtCurrent.AddDays(1); // 날하루 증가
                }
            }

            list.Sort((reportA, reportB) => reportA.PrcDate.CompareTo(reportB.PrcDate)); // 가장오래된 날짜부터 오름차순 정렬
            return list;
        }
    }
}
