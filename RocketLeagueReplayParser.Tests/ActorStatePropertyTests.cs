﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using RocketLeagueReplayParser.NetworkStream;
using System.IO;

namespace RocketLeagueReplayParser.Tests
{
    [TestFixture]
    public class ActorStatePropertyTests
    {
        public IEnumerable<string> ReplayFiles
        {
            get
            {
                var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My Games\Rocket League\TAGame\Demos\");
                return Directory.EnumerateFiles(dir, "*.replay");
            }
        }

        [TestCaseSource("ReplayFiles")]
        public void CameraSettingsTest(string filePath)
        {
            var r = Replay.Deserialize(filePath);
            
            foreach(var p in r.Frames.Where(f=>f.ActorStates != null).SelectMany(x=>x.ActorStates).Where(s=>s.Properties != null).SelectMany(s=>s.Properties).Where(p => p.PropertyName == "TAGame.PRI_TA:CameraSettings"))
            {
                var cs = (CameraSettings)p.Data[0];
                //Console.WriteLine(cs.ToString());

                // FOV: 60 - 110
                // Height: 40 - 200
                // Angle: -45 - 0
                // Distance: 100 - 400
                // Stiffness: 0 - 1
                // Swivel Speed: 1 - 10

                Assert.IsTrue(cs.FieldOfView >= 60 && cs.FieldOfView <= 110);
                Assert.IsTrue(cs.Height >= 40 && cs.Height <= 200);
                Assert.IsTrue(cs.Pitch >= -45 && cs.Pitch <= 0);
                Assert.IsTrue(cs.Distance >= 100 && cs.Distance <= 400);
                Assert.IsTrue(cs.Stiffness >= 0 && cs.Stiffness <= 1);
                Assert.IsTrue(cs.SwivelSpeed >= 1 && cs.SwivelSpeed <= 10);
            }
        }

        [TestCaseSource("ReplayFiles")]
        public void ClientLoadoutTest(string filePath)
        {
            var r = Replay.Deserialize(filePath);

            foreach (var p in r.Frames.Where(f => f.ActorStates != null).SelectMany(x => x.ActorStates).Where(s => s.Properties != null).SelectMany(s => s.Properties).Where(p => p.PropertyName == "TAGame.PRI_TA:ClientLoadout"))
            {
                var cl = (ClientLoadout)p.Data[0];
                Assert.IsTrue(cl.Version == 10);
                Assert.IsTrue(cl.Unknown2 == 0);
            }
        }

        [TestCaseSource("ReplayFiles")]
        public void TeamPaint(string filePath)
        {
            var r = Replay.Deserialize(filePath);

            foreach (var p in r.Frames.Where(f => f.ActorStates != null).SelectMany(x => x.ActorStates).Where(s => s.Properties != null).SelectMany(s => s.Properties).Where(p => p.PropertyName == "TAGame.Car_TA:TeamPaint"))
            {
                var tp = (TeamPaint)p.Data[0];
                //Console.WriteLine(tp.ToString());
                Assert.IsTrue(tp.TeamNumber == 0 || tp.TeamNumber == 1);
                Assert.IsTrue(tp.TeamColorId >= 0 && tp.TeamColorId <= 17);
                Assert.IsTrue(tp.CustomColorId >= 0 && tp.CustomColorId <= 104);
                Assert.IsTrue(tp.TeamFinishId >= 0 && tp.TeamFinishId <= 623);
                Assert.IsTrue(tp.CustomFinishId >= 0 && tp.CustomFinishId <= 623);
            }
        }
    }
}
