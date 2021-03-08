using Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Service
{

    /// <summary>
    /// 狀態機資料
    /// </summary>
    public class LightService
    {
        /// <summary>
        /// 靜態暫存
        /// </summary>
        private static ConcurrentDictionary<int, LightMachine> LightMachineDict = new ConcurrentDictionary<int, LightMachine>();


        public ICollection<LightMachine> GetLightMachines
        {
            get => LightMachineDict.Values;
        }

        public LightMachine GetLightMachineById(int id)
        {
            if (!LightMachineDict.TryGetValue(id, out LightMachine lightMachine))
            {
                return new LightMachine() { Id = id };
            }
            return lightMachine;
        }

        public LightMachine AddOrUpdateLight(int id, bool timeOut, Light light)
        {
            var result = LightMachineDict.AddOrUpdate(id,
                          new LightMachine(light) { Id = id },
                          (id, lightMachine) =>
                          {
                              lightMachine.TriggerChange(timeOut, light);
                              return lightMachine;
                          });
            return result;
        }

        public void DisEnable(int id) 
        {
            if (LightMachineDict.TryGetValue(id, out LightMachine lightMachine)) 
            {
                lightMachine.TriggerDisEnable();
            }
        }
    }
}
