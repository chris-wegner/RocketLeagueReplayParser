﻿using NUnit.Framework;
using RocketLeagueReplayParser.NetworkStream;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketLeagueReplayParser.Tests
{
    [TestFixture]
    public class SerializeTests
    {
        public IEnumerable<string> ReplayFiles
        {
            get
            {
				var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My Games\Rocket League\TAGame\Demos\");
                return Directory.EnumerateFiles(dir, "*.replay").OrderByDescending(x => File.GetCreationTime(x));
            }
        }

        [TestCaseSource("ReplayFiles")]
        public void TestRoundTripSerialization(string filePath)
        {
            var replay = Replay.Deserialize(filePath);

            var originalBytes = File.ReadAllBytes(filePath);

            byte[] newBytes;
            using (MemoryStream stream = new MemoryStream())
            {
                replay.Serialize(stream);

                newBytes = stream.ToArray();
            }
            
            Assert.AreEqual(originalBytes.Length, newBytes.Length);

            for (int i = 0; i < newBytes.Length; ++i)
            {
                Assert.AreEqual(originalBytes[i], newBytes[i]);
            }

        }

        [Test]
        public void TestRandomUIntMaxRoundTripSerialization()
        {
            var r = new Random();

            for (int n = 0; n < 1000; ++n)
            {
                int max = 0;
                while (max < 100) max = r.Next();

                int val = r.Next(max);

                TestUIntMaxRoundTripSerialization((UInt32)val, (UInt32)max);
            }
        }

        [TestCase(1U, 64U)]
        [TestCase(3U, 5U)]
        [TestCase(887977537U, 1173576471U)]
        [Test]
        public void TestUIntMaxRoundTripSerialization(UInt32 value, UInt32 max)
        {
            for (var i = 0; i < 2; ++i)
            {
                var bw = new BitWriter(32);
                bw.Write(value, max);
                var br = new BitReader(bw.GetBits(0, bw.Length).ToArray());
                var val2 = br.ReadUInt32Max((int)max);
                Assert.AreEqual(value, val2);
                Assert.AreEqual(bw.Length, br.Position);
                value = val2;
            }
        }

        [TestCase(UInt32.MaxValue, 32)]
        [TestCase(UInt32.MinValue, 32)]
        [Test]
        public void TestUIntFixedRoundTripSerialization(UInt32 value, int numBits)
        {
            for (var i = 0; i < 2; ++i)
            {
                var bw = new BitWriter(32);
                bw.WriteFixedBitCount(value, numBits);
                var br = new BitReader(bw.GetBits(0, bw.Length).ToArray());
                var val2 = br.ReadUInt32FromBits(numBits);
                Assert.AreEqual(value, val2);
                Assert.AreEqual(bw.Length, br.Position);
                value = val2;
            }
        }

        [Test]
        public void TestRandomFixedFloatRoundTripSerialization()
        {
            var r = new Random();

            for (int n = 0; n < 1000; ++n)
            {

                var f = (float)((r.NextDouble() * 2) - 1);

                for (var i = 0; i < 2; ++i)
                {
                    var bw = new BitWriter(32);
                    bw.WriteFixedCompressedFloat(f, 1, 16);
                    var br = new BitReader(bw.GetBits(0, bw.Length).ToArray());
                    var f2 = br.ReadFixedCompressedFloat(1, 16);

                    if (i == 0)
                    {
                        // We're generating floats that are probably going to lose precision when serialized.
                        // So the first time around just check to see if we're close.
                        Assert.IsTrue(Math.Abs(f - f2) < 0.0001);
                    }
                    else
                    {
                        Assert.AreEqual(f, f2);
                    }
                    Assert.AreEqual(bw.Length, br.Position);
                    f = f2;
                }
            }
        }

        [TestCase(0f, 0f, 99f)]
        [Test]
        public void TestVectorRoundTripSerialization(float x, float y, float z)
        {
            var v = new Vector3D(x, y, z);
            for (var i = 0; i < 2; ++i)
            {
                var bw = new BitWriter(32);
                v.Serialize(bw);
                var br = new BitReader(bw.GetBits(0, bw.Length).ToArray());
                var v2 = Vector3D.Deserialize(br);

                if (i == 0)
                {
                    // We're generating floats even though these are serialized as ints
                    // So the first time around just check to see if we're at the nearest int
                    Assert.IsTrue(Math.Abs(v.X - v2.X) <= 0.5);
                    Assert.IsTrue(Math.Abs(v.Y - v2.Y) <= 0.5);
                    Assert.IsTrue(Math.Abs(v.Z - v2.Z) <= 0.5);
                }
                else
                {
                    Assert.AreEqual(v.X, v2.X);
                    Assert.AreEqual(v.Y, v2.Y);
                    Assert.AreEqual(v.Z, v2.Z);
                }
                Assert.AreEqual(bw.Length, br.Position);
                v = v2;
            }
        }

        [TestCase("0110000000010000000111000111")] // 0,0,99
        [TestCase("1101000000000000100000000000110000110110001")] // 0, 2048, 432
        [Test]
        public void TestVectorRoundTripSerializationFromBinary(string binary)
        {
            var br = new BitReader(binary);
            var v = Vector3D.Deserialize(br);

            var bw = new BitWriter(32);
            v.Serialize(bw);
            var writtenBits = bw.GetBits(0, bw.Length).ToBinaryString();

            Assert.AreEqual(binary, writtenBits);
        }

        [Test]
        public void TestRandomVectorRoundTripSerialization()
        {
            var r = new Random();

            for (int n = 0; n < 1000; ++n)
            {
                var x = (float)((r.NextDouble() * 200000) - 100000);
                var y = (float)((r.NextDouble() * 200000) - 100000);
                var z = (float)((r.NextDouble() * 200000) - 100000);

                TestVectorRoundTripSerialization(x, y, z);
            }
        }
    }
}
