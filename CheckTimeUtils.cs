using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace _7D2D_DecayMod
{
    public static class CheckTimeUtils
    {
        //Check Day Changed
        public static int _lastDay = -1;
        public static float _lastDayCheckTime = 0f;
        public static float _dayCheckCooldown = 15f; //Cooldown For Interval Check (ex. 15f = wait 15 Second before check again)
        public static void CheckDayChanged()
        {
            if (Time.time - _lastDayCheckTime < _dayCheckCooldown) return;
            _lastDayCheckTime = Time.time;

            //Same Function at DiscordRPCManager.GetDayCount()
            int currentDay = DiscordRPCManager.GetDayCount();

            if (currentDay != _lastDay)
            {
                Log.Out($"[DiscordRPCMod]: Day changed to {currentDay}");
                _lastDay = currentDay;
                DiscordRPCManager._dayChanged = true;
            }
        }

        public static void OnDayChanged(int day)
        {
            //May be Update In Future.
            Log.Out($"[DiscordRPCMod]: Day changed to {day}");
        }
    }
}
