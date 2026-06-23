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

package com.facebook.unity;

import android.os.Bundle;

import androidx.annotation.Nullable;
import androidx.fragment.app.FragmentActivity;

import com.facebook.login.FBLoginSSOLauncher;

/**
 * Proxy activity that hosts the AndroidX-based Facebook SSO ("Login with
 * Facebook") app-switch flow.
 *
 * Unlike {@link FBUnityLoginActivity} (which extends the plain {@link BaseActivity}),
 * the SSO launcher requires an AndroidX {@code ComponentActivity} so it can call
 * {@code registerForActivityResult} during {@code onCreate}, and its "no Facebook
 * app installed" fallback dialog requires a {@link FragmentActivity}. The login
 * result is delivered back to Unity through the existing {@code OnLoginComplete}
 * message, identical to every other login path.
 */
public class FBUnitySSOActivity extends FragmentActivity {
    public static final String LOGIN_PARAMS = "login_params";

    // Held so the launcher (and its registered ActivityResult callback) is not
    // garbage collected while the SSO flow is in progress.
    private FBLoginSSOLauncher ssoLauncher;

    @Override
    protected void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        String loginParams = getIntent().getStringExtra(LOGIN_PARAMS);
        // Must run during onCreate: FBLoginSSOLauncher registers for an activity result.
        FBLogin.loginWithSSO(loginParams, this);
    }

    void setSSOLauncher(FBLoginSSOLauncher launcher) {
        this.ssoLauncher = launcher;
    }
}
