﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.TestCode;
using ttt;

namespace TestGenerator.Tests
{
    [TestClass()]
    public class DecomposeCodeTests
    {
        DecomposeCode decompose;
        string text;

        [TestInitialize]
        public void Init()
        {
            decompose = new DecomposeCode();
            text = A.t;
        }

        [TestMethod()]
        public void CountOfCreatedContextTypes()
        {
            var result = decompose.DecomposeType(text);
            Console.WriteLine(result.Count());
            Assert.AreEqual(result.Count(), 8);
        }
    }
}