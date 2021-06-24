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
        // 세팅 테이블에서 데이터 가져오기
        public static List<Settings> GetSettings()
        {
            List<Model.Settings> settings;

            using (var ctx = new MRPEntities())
                settings = ctx.Settings.ToList();  // 세팅즈에있는 DB 데이터를 가져와서 리스트로 만들겠다. (SELECT)

            return settings;  // DB데이터가 담긴 리스트(settings)를 반환
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
    }
}
