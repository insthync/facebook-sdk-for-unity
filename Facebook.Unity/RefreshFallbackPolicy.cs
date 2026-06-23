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

namespace Facebook.Unity
{
    /// <summary>
    /// Controls how <see cref="FB.Mobile.RefreshLimitedLogin"/> refreshes the
    /// Limited Login profile data (iOS only).
    /// </summary>
    /// <remarks>
    /// The integer values MUST stay in sync with the native iOS
    /// <c>FBSDKRefreshFallbackPolicy</c> enum raw values, because the value is
    /// marshalled across the P/Invoke boundary as an int and cast straight back
    /// with <c>(FBSDKRefreshFallbackPolicy)value</c>. See
    /// <c>FBUnityInterface.mm</c> / <c>IOSWrapper.IOSFBRefreshLimitedLogin</c>.
    ///
    /// Verified matching the native enum: Automatic=0, SilentOnly=1,
    /// ExplicitOnly=2, DirectOnly=3. Note the native cases are positional
    /// (only <c>automatic = 0</c> is explicit there), so reordering or inserting
    /// a case in the native enum would silently renumber these. Keep both ends
    /// in the same order.
    /// </remarks>
    public enum RefreshFallbackPolicy
    {
        /// <summary>
        /// Try the direct (DPoP) refresh, then silent (prompt=none), then the
        /// explicit re-login flow. This is the recommended default and degrades
        /// gracefully when DPoP key material or a Facebook session is unavailable.
        /// </summary>
        Automatic = 0,

        /// <summary>
        /// Only attempt the silent (prompt=none) refresh. Fails if no active
        /// Facebook session is available rather than prompting the user.
        /// </summary>
        SilentOnly = 1,

        /// <summary>
        /// Only attempt the explicit refresh, which re-runs the Limited Login
        /// flow and may show a confirmation to the user.
        /// </summary>
        ExplicitOnly = 2,

        /// <summary>
        /// Only attempt the direct (DPoP) refresh. Fails with a "not DPoP bound"
        /// error if the current token was not bound at login time.
        /// </summary>
        DirectOnly = 3,
    }
}
