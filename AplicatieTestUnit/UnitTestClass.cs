using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SQLManager;

namespace AplicatieTestUnit
{
    [TestClass]
    public class UnitTestClass
    {
        private SQLManager.SQLManager _sqlManager;

        [ClassInitialize]
        public void Init()
        {
            _sqlManager = SQLManager.SQLManager.GetInstance();
        }

        [TestMethod]
        public void SQLManager_Test()
        {

        }
    }
}
