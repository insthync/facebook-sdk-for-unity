/**
 * Copyright (c) 2014-present, Facebook, Inc. All rights reserved.
 *
 * You are hereby granted a non-exclusive, worldwide, royalty-free license to use,
 * copy, modify, and distribute this software in source code or binary form for use
 * in connection with the web services and APIs provided by Facebook.
 *
 * As with any software that integrates with the Facebook platform, your use of
 * this software is subject to the Facebook Developer Principles and Policies
 * [http://developers.facebook.com/policy/]. This copyright notice shall be
 * included in all copies or substantial portions of the software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

namespace Facebook.Unity.Tests.BuildPostProcess
{
    using Facebook.Unity.Editor;
    using NUnit.Framework;

    [TestFixture]
    public class PodfileEditorTest
    {
        private const string AppTarget = "Unity-iPhone";
        private const string AppTargetBlock = "target 'Unity-iPhone' do\nend\n";
        private const string UnityFrameworkPodfile =
            "platform :ios, '13.0'\ntarget 'UnityFramework' do\n  pod 'FBSDKCoreKit'\nend\n";

        [TestCase("", TestName = "AppendsTarget_EmptyPodfile")]
        [TestCase(UnityFrameworkPodfile, TestName = "AppendsTarget_UnityFrameworkOnly")]
        [TestCase("# Generated for Unity-iPhone\n", TestName = "AppendsTarget_NameOnlyInComment")]
        [TestCase("target 'Unity-iPhone Tests' do\nend\n", TestName = "AppendsTarget_LongerTargetName")]
        [TestCase("target 'UnityFramework' do\nend", TestName = "AppendsTarget_NoTrailingNewline")]
        public void AppendsTargetWhenMissing(string podfile)
        {
            string result = PodfileEditor.AppendTargetIfMissing(podfile, AppTarget);

            Assert.IsTrue(result.StartsWith(podfile), "original contents must be preserved");
            Assert.IsTrue(result.EndsWith(AppTargetBlock), "target block must be appended: " + result);
            Assert.IsFalse(result.Contains("endtarget"), "block must not run into the previous line");
        }

        [TestCase("target 'Unity-iPhone' do\nend\n", TestName = "Unchanged_SingleQuotes")]
        [TestCase("target \"Unity-iPhone\" do\nend\n", TestName = "Unchanged_DoubleQuotes")]
        [TestCase("  target 'Unity-iPhone' do\n  end\n", TestName = "Unchanged_Indented")]
        [TestCase(
            "target 'UnityFramework' do\nend\ntarget 'Unity-iPhone' do\n  pod 'FBSDKLoginKit'\nend\n",
            TestName = "Unchanged_AlreadyHasBothTargets")]
        public void LeavesPodfileUnchangedWhenTargetPresent(string podfile)
        {
            Assert.AreEqual(podfile, PodfileEditor.AppendTargetIfMissing(podfile, AppTarget));
        }

        [Test]
        public void AppendsTheRequestedTargetName()
        {
            // Under the Swift Xcode project type the app target is the sanitized Unity project name.
            string result = PodfileEditor.AppendTargetIfMissing(UnityFrameworkPodfile, "MyGame");

            Assert.IsTrue(result.EndsWith("target 'MyGame' do\nend\n"), result);
            Assert.IsFalse(PodfileEditor.ContainsTarget(result, AppTarget));
        }

        [TestCase(null, AppTarget, TestName = "ContainsTarget_NullContents")]
        [TestCase("", AppTarget, TestName = "ContainsTarget_EmptyContents")]
        [TestCase(UnityFrameworkPodfile, null, TestName = "ContainsTarget_NullTargetName")]
        [TestCase(UnityFrameworkPodfile, "", TestName = "ContainsTarget_EmptyTargetName")]
        public void ContainsTargetIsFalseForMissingInput(string contents, string targetName)
        {
            Assert.IsFalse(PodfileEditor.ContainsTarget(contents, targetName));
        }

        [Test]
        public void TreatsNullContentsAsEmpty()
        {
            Assert.AreEqual(AppTargetBlock, PodfileEditor.AppendTargetIfMissing(null, AppTarget));
        }

        [Test]
        public void LeavesPodfileUnchangedWhenTargetNameIsMissing()
        {
            Assert.AreEqual(
                UnityFrameworkPodfile,
                PodfileEditor.AppendTargetIfMissing(UnityFrameworkPodfile, null));
        }
    }
}
