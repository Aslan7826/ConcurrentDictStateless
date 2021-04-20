using Stateless;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Model
{
    /// <summary>
    /// 狀態機
    /// </summary>
    public class LightMachine
    {
        public int Id { get; set; }


        public Light LightType { get => _light; }
        private Light _light;
        StateMachine<Light, Trigger> _machine;
        //由外部傳入參數觸發
        //傳入參數有兩個
        StateMachine<Light, Trigger>.TriggerWithParameters<bool, Light> _IsTimeOut;
        //傳入參數為1個
        StateMachine<Light, Trigger>.TriggerWithParameters<bool> _IsTimeOutDic;
        public LightMachine(Light light = Light.Gray)
        {
            _light = light;
            _machine = new StateMachine<Light, Trigger>(() => _light, s => _light = s);
            _IsTimeOut = _machine.SetTriggerParameters<bool, Light>(Trigger.Connect);
            _IsTimeOutDic = _machine.SetTriggerParameters<bool>(Trigger.DisConnect);

            var lightDict = new Dictionary<Light,
                           (Action<StateMachine<Light, Trigger>.StateConfiguration> self,
                            Action<StateMachine<Light, Trigger>.StateConfiguration> outer)>()
            {
                {
                    Light.Gray,
                    (
                        self : o=>o.OnEntry(e=> { }),
                        outer: o=>o.Permit(Trigger.DisEnable,Light.Gray)
                                   .PermitIf(_IsTimeOut,Light.Gray,(t,light)=>light == Light.Gray)
                    )
                },
                {
                    Light.Green,
                    (
                        self : o=>o.OnEntry(e=> { }),
                        outer: o=>o.PermitIf(_IsTimeOut,Light.Green,(t,light)=>t && light == Light.Green)
                    )
                },
                {
                    Light.GreenTimeOut,
                    (
                        self : o=>o.OnEntry(e=> { }),
                        outer: o=>o.PermitIf(_IsTimeOut,Light.GreenTimeOut,(t,light)=>t && light == Light.GreenTimeOut)
                    )
                },
                {
                    Light.Yellow,
                    (
                        self : o=>o.OnEntry(e=> { }),
                        outer: o=>o.PermitIf(_IsTimeOut,Light.Yellow,(t,light)=>t && light == Light.Yellow)
                    )
                },
                {
                    Light.YellowTimeOut,
                    (
                        self : o=>o.OnEntry(e=> { }),
                        outer: o=>o.PermitIf(_IsTimeOut,Light.YellowTimeOut,(t,light)=>t && light == Light.YellowTimeOut)
                    )
                },
                {
                    Light.Red,
                    (
                        self : o=>o.OnEntry(e=> { EventRed?.Invoke(e); }),
                        outer: o=>o.PermitIf(_IsTimeOutDic,Light.Red,(t)=>t )
                    )
                },
                {
                    Light.RedTimeOut,
                    (
                        self : o=>o.OnEntry(e=> { }),
                        outer: o=>o.PermitIf(_IsTimeOutDic,Light.RedTimeOut,(t)=>!t )
                    )
                },
            };

            foreach (var i in lightDict)
            {
                var machine = _machine.Configure(i.Key);
                i.Value.self(machine);
                foreach (var j in lightDict.Where(o => o.Key != i.Key))
                {
                    j.Value.outer(machine);
                }
            }
            //忽略沒設定到的轉換錯誤
            _machine.OnUnhandledTrigger((light, trigger) => { });
        }

        public Action<StateMachine<Light, Trigger>.Transition> EventRed;

        public void TriggerChange(bool isTimeOut, Light light)
        {
            LockTimeOut(_machine, () =>  _machine.Fire(_IsTimeOut, isTimeOut, light));
           
        }
        /// <summary>
        /// 關閉
        /// </summary>
        public void TriggerDisEnable()
        {
            LockTimeOut(_machine, () => _machine.Fire(Trigger.DisEnable));
        }

        private void LockTimeOut(object obj, Action action)
        {
            bool overTime = System.Threading.Monitor.TryEnter(obj, 3000);
            try
            {
                action();
            }
            finally
            {
                if (overTime)
                {
                    System.Threading.Monitor.Exit(obj);
                }
            }
        }
    }
}
