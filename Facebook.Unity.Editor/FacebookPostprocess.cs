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

namespace Facebook.Unity.Editor
{
    using System.IO;
    using Facebook.Unity;
    using Facebook.Unity.Settings;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;

    public static class XCodePostProcess
    {
        // Only the Objective-C Xcode project type names the app target statically. The Swift
        // project type derives it from the Unity project name.
        private const string ObjectiveCAppTargetName = "Unity-iPhone";

        [PostProcessBuildAttribute(45)]
        private static void PostProcessBuild_iOS(BuildTarget target, string buildPath)
        {
            if (target == BuildTarget.iOS)
            {
                string podFilePath = Path.Combine(buildPath, "Podfile");
                if (File.Exists(podFilePath))
                {
                    string contents = File.ReadAllText(podFilePath);
                    string updated = PodfileEditor.AppendTargetIfMissing(contents, ObjectiveCAppTargetName);
                    if (updated != contents)
                    {
                        File.WriteAllText(podFilePath, updated);
                    }
                }
                else
                {
                    Debug.LogWarning("No podfile created");
                }
            }
        }

        [PostProcessBuild(100)]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
            // If integrating with facebook on any platform, throw a warning if the app id is invalid
            if (!FacebookSettings.IsValidAppId)
            {
                Debug.LogWarning("You didn't specify a Facebook app ID.  Please add one using the Facebook menu in the main Unity editor.");
            }

            // Unity renamed build target from iPhone to iOS in Unity 5, this keeps both versions happy
            if (target.ToString() == "iOS" || target.ToString() == "iPhone")
            {
                UpdatePlistAtPath(Path.Combine(path, "Info.plist"));
                FixupFiles.AddBuildFlag(path);
            }

            if (target == BuildTarget.Android)
            {
                // The default Bundle Identifier for Unity does magical things that causes bad stuff to happen
                var defaultIdentifier = "com.Company.ProductName";

                if (Utility.GetApplicationIdentifier() == defaultIdentifier)
                {
                    Debug.LogError("The default Unity Bundle Identifier (com.Company.ProductName) will not work correctly.");
                }

                if (!FacebookAndroidUtil.SetupProperly)
                {
                    Debug.LogError("Your Android setup is not correct. See Settings in Facebook menu.");
                }

                if (!ManifestMod.CheckManifest())
                {
                    // If something is wrong with the Android Manifest, try to regenerate it to fix it for the next build.
                    ManifestMod.GenerateManifest();
                }
            }
        }

        /// <summary>
        /// Writes the Facebook settings into the exported Info.plist. Takes the full plist path
        /// rather than the build root: only the Objective-C Xcode project type is guaranteed to
        /// emit Info.plist there, so resolving it is the caller's job.
        ///
        /// Renamed from UpdatePlist deliberately. Keeping the old name with a new meaning for its
        /// only parameter would let an external caller keep compiling while silently writing to
        /// the wrong path.
        /// </summary>
        public static void UpdatePlistAtPath(string plistFullPath)
        {
            string appId = FacebookSettings.AppId;
            string clientToken = FacebookSettings.ClientToken;

            if (string.IsNullOrEmpty(appId) || appId.Equals("0"))
            {
                Debug.LogError("You didn't specify a Facebook app ID.  Please add one using the Facebook menu in the main Unity editor.");
                return;
            }

            var facebookParser = new PListParser(plistFullPath);
            facebookParser.UpdateFBSettings(
                appId,
                clientToken,
                FacebookSettings.IosURLSuffix,
                FacebookSettings.AppLinkSchemes[FacebookSettings.SelectedAppIndex].Schemes);
            facebookParser.WriteToFile();
        }
    }
}
