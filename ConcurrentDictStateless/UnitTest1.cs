using Model;
using NUnit.Framework;
using Service;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConcurrentDictStateless
{
    public class Tests
    {
        LightService _LightService;
        [SetUp]
        public void Setup()
        {
            _LightService = new LightService();

        }

        [Test]
        public void Test1()
        {
            //arrange
            _LightService.AddOrUpdateLight(1, false, Light.Gray);
            _LightService.AddOrUpdateLight(2, false, Light.Gray);
            var tasks = new List<Task>();
            for(var i =0; i < 10000; i++) 
            {
                tasks.Add(
                    Task.Factory.StartNew((index) =>
                    {
                        int selectIndex = ((int)index & 1) == 1 ? 1 : 2;
                        _LightService.AddOrUpdateLight(selectIndex,true,Light.Yellow);
                    }, i));
            }
            var expected = Light.Yellow;
            Task.WaitAll(tasks.ToArray());

            //act
            var act1 = _LightService.GetLightMachineById(1).LightType;
            var act2 = _LightService.GetLightMachineById(2).LightType;
            //assert
            Assert.AreEqual(act2, expected);
            Assert.AreEqual(act1, expected);
        }
    }
}