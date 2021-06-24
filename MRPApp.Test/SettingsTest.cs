using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace MRPApp.Test
{
    [TestClass]
    public class SettingsTest
    {
        [TestMethod]
        public void IsDuplicateDataTest()
        {
            var inputCode = "PC10001";   // DB상에 있는 값
            var expectVal = true;        // 예상값

            var code = Logic.DataAccess.GetSettings().Where(d => d.BasicCode.Contains(inputCode));
            var readVal = code != null ? true : false;

            Assert.AreEqual(expectVal, readVal);  // 값이 같으면 Pass, 다르면 Fail
        }

        [TestMethod]
        public void IsCodeSearched()
        {
            var expectVal = 2; // 예상값
            var inputCode = "설비";

            var realVal = Logic.DataAccess.GetSettings().Where(d => d.CodeName.Contains(inputCode)).Count();
        }

        [TestMethod]
        public void IsEmailCorrect()
        {
            var inputEmail = "이메일";
            Assert.IsTrue(Commons.IsValidEmail(inputEmail));
        }
    }
}
