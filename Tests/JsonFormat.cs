using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Alteracia.Web;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class JsonFormat
    {
        [Test]
        public void ReferenceTest()
        {
            string json = @"{""stringValue"":""true""}"; // TODO Add All levels
            RootObject dto = JsonUtility.FromJson<RootObject>(json.FormatJsonText(typeof(RootObject)));
            Assert.AreEqual("true", dto.stringValue);
        }
        
        [Test]
        public void InherentTest()
        {
            string json = @"{""stringValue"":""true"",""fatherStringValue"":""true""}";
            Sun dto = JsonUtility.FromJson<Sun>(json.FormatJsonText(typeof(Sun)));
            Assert.AreEqual("true", dto.stringValue);
            Assert.AreEqual("true", dto.fatherStringValue);
        }
        
        [Test]
        public void ReferenceInInherentTest()
        {
            string json = @"{""sunStringValue"":""true""}"; // TODO Add All levels
            SunRootObject dto = JsonUtility.FromJson<SunRootObject>(json.FormatJsonText(typeof(SunRootObject)));
            Assert.AreEqual("true", dto.sunStringValue);
        }
        
        [Test]
        public void StructReferenceTest()
        {
            string json = @"{""stringValue"":""true""}"; // TODO Add All levels
            StructReferenceObject dto = JsonUtility.FromJson<StructReferenceObject>(json.FormatJsonText(typeof(StructReferenceObject)));
            Assert.AreEqual("true", dto.stringValue);
        }
        
        [Test]
        public void DateTimeTest()
        {
            string json = @"{""stringValue"":""true""}"; // TODO Add All levels
            DateTimeObject dto = JsonUtility.FromJson<DateTimeObject>(json); // OK 
            //DateTimeObject dto = JsonUtility.FromJson<DateTimeObject>(json.FormatJsonText(typeof(DateTimeObject))); // KO
            Assert.AreEqual("true", dto.stringValue);
        }
    }
}
