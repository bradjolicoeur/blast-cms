using blastcms.web.Security;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blastcms.web.tests.Services
{
    public class HashingServiceTests
    {

        [Test]
        public void GenerateHash_Successful()
        {
            //Arrange

            var sut = new HashingService();

            //Act
            var results = sut.GenerateNewKey();
            var results2 = sut.RegenHash(results.Item2);

            //Assert
            Assert.NotNull(results.Item1);
            Assert.NotNull(results2);
            Assert.AreEqual(results.Item1, results2);
        }
    }
}
