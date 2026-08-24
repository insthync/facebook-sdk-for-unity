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
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Text transforms for the Podfile that the External Dependency Manager generates during an
    /// iOS export. Deliberately free of UnityEditor APIs: the app target name is only a static
    /// "Unity-iPhone" under the Objective-C Xcode project type, so the caller has to resolve it
    /// and pass it in.
    /// </summary>
    public static class PodfileEditor
    {
        /// <summary>
        /// Whether the Podfile already declares a target block for <paramref name="targetName"/>.
        /// </summary>
        public static bool ContainsTarget(string contents, string targetName)
        {
            if (string.IsNullOrEmpty(contents) || string.IsNullOrEmpty(targetName))
            {
                return false;
            }

            // Anchor on an actual `target '<name>'` declaration. A bare substring search matches
            // comments, workspace paths, and longer target names such as "Unity-iPhone Tests".
            string pattern = @"^[ \t]*target[ \t]+(['""])" + Regex.Escape(targetName) + @"\1";
            return Regex.IsMatch(contents, pattern, RegexOptions.Multiline);
        }

        /// <summary>
        /// Appends an empty target block for <paramref name="targetName"/> unless one is already
        /// declared. Without it CocoaPods links the pods against UnityFramework only, and the app
        /// target fails to link with undefined FBSDK symbols.
        /// </summary>
        public static string AppendTargetIfMissing(string contents, string targetName)
        {
            string existing = contents ?? string.Empty;

            if (string.IsNullOrEmpty(targetName) || ContainsTarget(existing, targetName))
            {
                return existing;
            }

            var builder = new StringBuilder(existing);

            // A Podfile that does not end in a newline would otherwise produce `endtarget '...' do`.
            if (existing.Length > 0 && !existing.EndsWith("\n"))
            {
                builder.Append("\n");
            }

            builder.Append("target '").Append(targetName).Append("' do\n");
            builder.Append("end\n");

            return builder.ToString();
        }
    }
}
