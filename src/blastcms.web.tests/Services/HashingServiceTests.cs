using blastcms.web.Security;
using NUnit.Framework;
using NUnit.Framework.Legacy;

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
            ClassicAssert.NotNull(results.Item1);
            ClassicAssert.NotNull(results2);
            ClassicAssert.AreEqual(results.Item1, results2);
        }
    }
}
