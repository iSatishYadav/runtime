// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Speech.AudioFormat;
using System.Speech.Recognition;
using System.Speech.Recognition.SrgsGrammar;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Xml;
using Xunit;

namespace SampleSynthesisTests
{
    public static class SpeechRecognizerTests
    {
        [Fact]
        [OuterLoop] // Pops UI
        public static void SpeechRecognizer()
        {
            if (Thread.CurrentThread.CurrentCulture.ToString() != "en-US")
                return;

            // Sometimes this lingers, causing subsequent tests to fail
            foreach (Process p in Process.GetProcessesByName("sapisvr.exe"))
            {
                p.Kill();
            }

            using (SpeechRecognizer recognizer = new SpeechRecognizer()) // Pops Windows UI that can be ignored but isn't dismissed by the test
            {
                Grammar testGrammar = new Grammar(new GrammarBuilder("test"));
                testGrammar.Name = "Test Grammar";
                recognizer.LoadGrammar(testGrammar);

                var list = new List<string>();

                recognizer.SpeechRecognized += (object sender, SpeechRecognizedEventArgs e) => list.Add("SpeechRecognized");
                recognizer.SpeechDetected += (object sender, SpeechDetectedEventArgs e) => list.Add("SpeechDetected");
                recognizer.SpeechHypothesized += (object sender, SpeechHypothesizedEventArgs e) => list.Add("SpeechHypothesized");
                recognizer.SpeechRecognitionRejected += (object sender, SpeechRecognitionRejectedEventArgs e) => list.Add("SpeechRecognitionRejected");

                recognizer.EmulateRecognizeCompleted += (object sender, EmulateRecognizeCompletedEventArgs e) =>
                {
                    Assert.Equal(EncodingFormat.Pcm, recognizer.AudioFormat.EncodingFormat);
                    Assert.Equal(new TimeSpan(0, 0, 0), recognizer.AudioPosition);
                    Assert.Equal(AudioState.Stopped, recognizer.AudioState);
                    Assert.Equal(RecognizerState.Stopped, recognizer.State);
                    list.Add("EmulateRecognizeCompleted");
                    Assert.Equal(2, list.Count);
                };

                RecognitionResult result = recognizer.EmulateRecognize("test");
                Assert.NotNull(result);
            }
        }
    }
}
