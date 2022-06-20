using blastcms.web.Handlers;
using blastcms.web.Security;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blastcms.web.tests.Handlers
{
    public class GenerateApiKeyHandlers
    {

        [Test]
        public async Task GenerateApiKey_Successful()
        {
            //Arrange

            var hashingService = new Mock<IHashingService>();
            hashingService.Setup(m => m.GenerateNewKey()).Returns(new Tuple<string, string>("hash", "key"));

            var sut = new GenerateApiKeyHandler.Handler(Tests.SessionFactory, hashingService.Object);

            //Act
            var result = await sut.Handle(new GenerateApiKeyHandler.Command(), new System.Threading.CancellationToken());


            //Assert
            hashingService.Verify(m => m.GenerateNewKey(), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Key);
            Assert.AreEqual("key", result.Key);


        }
    }
}
