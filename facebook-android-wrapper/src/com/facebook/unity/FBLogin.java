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

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

import android.text.TextUtils;
import android.util.Log;

import com.facebook.AccessToken;
import com.facebook.AuthenticationToken;
import com.facebook.FacebookCallback;
import com.facebook.FacebookException;
import com.facebook.FacebookSdk;
import com.facebook.login.DeviceLoginManager;
import com.facebook.login.FBLoginSSOLauncher;
import com.facebook.login.LoginBehavior;
import com.facebook.login.LoginManager;
import com.facebook.login.LoginResult;

public class FBLogin {
    public static void loginWithReadPermissions(
            String params,
            final FBUnityLoginActivity activity) {
        login(params, activity, false, false);
    }

    public static void loginWithPublishPermissions(
            String params,
            final FBUnityLoginActivity activity) {
        login(params, activity, true, false);
    }

    public static void loginForTVWithReadPermissions(
            String params,
            final FBUnityLoginActivity activity) {
        login(params, activity, false, true);
    }

    public static void loginForTVWithPublishPermissions(
            String params,
            final FBUnityLoginActivity activity) {
        login(params, activity, true, true);
    }

    public static void loginWithSSO(
            String params,
            final FBUnitySSOActivity activity) {
        if (params == null) {
            // The proxy activity can be recreated by Android after process death (or started
            // without its extra), delivering null params. Fail gracefully rather than letting
            // UnityParams.parse(null) throw and crash the activity.
            Log.w(FB.TAG, "loginWithSSO called with null params; aborting.");
            sendLoginCancelOrErrorMessage(null, "SSO login failed: missing login parameters.");
            activity.finish();
            return;
        }
        if (!FacebookSdk.isInitialized()) {
            Log.w(FB.TAG, "Facebook SDK not initialized. Call init() before calling loginWithSSO()");
            activity.finish();
            return;
        }

        UnityParams unity_params = UnityParams.parse(params,
                "couldn't parse login params: " + params);

        List<String> permissions = new ArrayList<>();
        if (unity_params.hasString("scope")) {
            permissions = new ArrayList<>(
                    Arrays.asList(unity_params.getString("scope").split(",")));
        }

        String callbackIDString = null;
        if (unity_params.has(Constants.CALLBACK_ID_KEY)) {
            callbackIDString = unity_params.getString(Constants.CALLBACK_ID_KEY);
        }
        final String callbackID = callbackIDString;

        FacebookCallback<LoginResult> callback = new FacebookCallback<LoginResult>() {
            @Override
            public void onSuccess(LoginResult loginResult) {
                sendLoginSuccessMessage(
                        loginResult.getAccessToken(),
                        loginResult.getAuthenticationToken(),
                        callbackID);
                activity.finish();
            }

            @Override
            public void onCancel() {
                sendLoginCancelOrErrorMessage(callbackID, null);
                activity.finish();
            }

            @Override
            public void onError(FacebookException e) {
                Log.w(FB.TAG, "SSO login error", e);
                // getMessage() may be null for some FacebookExceptions; fall back to toString()
                // so this is reported as a real error, not mis-routed to the cancel path.
                String message = e.getMessage() != null ? e.getMessage() : e.toString();
                sendLoginCancelOrErrorMessage(callbackID, message);
                activity.finish();
            }
        };

        // Must be constructed during onCreate (registerForActivityResult requirement).
        FBLoginSSOLauncher ssoLauncher = new FBLoginSSOLauncher(activity, callback);
        activity.setSSOLauncher(ssoLauncher);
        boolean launched = ssoLauncher.launch(permissions);
        if (!launched) {
            // launch() returns false (with no callback) when the LoginSSO gatekeeper is
            // disabled, an access token already exists, or no Facebook app capable of SSO is
            // installed. Surface a result and finish so the proxy activity doesn't hang.
            Log.w(FB.TAG, "FBLoginSSOLauncher.launch() returned false; SSO unavailable.");
            sendLoginCancelOrErrorMessage(callbackID,
                    "SSO unavailable: launch() returned false. Ensure the LoginSSO gatekeeper is "
                    + "enabled for this app and a current Facebook app is installed, and that you "
                    + "are not already logged in.");
            activity.finish();
        }
    }

    private static void sendLoginCancelOrErrorMessage(String callbackID, String error) {
        UnityMessage unityMessage = new UnityMessage("OnLoginComplete");
        unityMessage.put("key_hash", FB.getKeyHash());
        if (callbackID != null && !callbackID.isEmpty()) {
            unityMessage.put(Constants.CALLBACK_ID_KEY, callbackID);
        }
        if (error != null) {
            unityMessage.sendError(error);
        } else {
            unityMessage.putCancelled();
            unityMessage.send();
        }
    }

    public static void sendLoginSuccessMessage(AccessToken accessToken, AuthenticationToken authenticationToken, String callbackID) {
        UnityMessage unityMessage = new UnityMessage("OnLoginComplete");
        FBLogin.addLoginParametersToMessage(unityMessage, accessToken, authenticationToken, callbackID);
        unityMessage.send();
    }

    public static void addLoginParametersToMessage(
            UnityMessage unityMessage,
            AccessToken accessToken,
            AuthenticationToken authenticationToken,
            String callbackID) {
        unityMessage.put("key_hash", FB.getKeyHash());
        unityMessage.put("opened", true);
        unityMessage.put("access_token", accessToken.getToken());
        if (authenticationToken != null) {
          unityMessage.put("auth_token_string", authenticationToken.getToken());
          unityMessage.put("auth_nonce", authenticationToken.getExpectedNonce());
        }
        Long expiration = accessToken.getExpires().getTime() / 1000;
        unityMessage.put("expiration_timestamp", expiration.toString());
        unityMessage.put("user_id", accessToken.getUserId());
        unityMessage.put("permissions",
                TextUtils.join(",", accessToken.getPermissions()));
        unityMessage.put("declined_permissions",
                TextUtils.join(",", accessToken.getDeclinedPermissions()));
        unityMessage.put("graph_domain", accessToken.getGraphDomain() != null ? accessToken.getGraphDomain() : "facebook");

        if (accessToken.getLastRefresh() != null) {
            Long lastRefresh = accessToken.getLastRefresh().getTime() / 1000;
            unityMessage.put("last_refresh", lastRefresh.toString());
        }

        if (callbackID != null && !callbackID.isEmpty()) {
            unityMessage.put(Constants.CALLBACK_ID_KEY, callbackID);
        }
    }

    private static void login(
            String params,
            final FBUnityLoginActivity activity,
            boolean isPublishPermLogin,
            boolean isDeviceAuthLogin) {
        if (!FacebookSdk.isInitialized()) {
            Log.w(FB.TAG, "Facebook SDK not initialized. Call init() before calling login()");
            return;
        }

        final UnityMessage unityMessage = new UnityMessage("OnLoginComplete");
        unityMessage.put("key_hash", FB.getKeyHash());
        UnityParams unity_params = UnityParams.parse(params,
                "couldn't parse login params: " + params);

        List<String> permissions = null;
        if (unity_params.hasString("scope")) {
            permissions = new ArrayList<>(
                    Arrays.asList(unity_params.getString("scope").split(",")));
        }

        String callbackIDString = null;
        if (unity_params.has(Constants.CALLBACK_ID_KEY)) {
            callbackIDString = unity_params.getString(Constants.CALLBACK_ID_KEY);
            unityMessage.put(Constants.CALLBACK_ID_KEY, callbackIDString);
        }

        final String callbackID = callbackIDString;

        // For now only web login
        LoginManager.getInstance().registerCallback(
                activity.getCallbackManager(),
                new FacebookCallback<LoginResult>() {
                    @Override
                    public void onSuccess(LoginResult loginResult) {
                        sendLoginSuccessMessage(loginResult.getAccessToken(), loginResult.getAuthenticationToken(), callbackID);
                    }

                    @Override
                    public void onCancel() {
                        unityMessage.putCancelled();
                        unityMessage.send();
                    }

                    @Override
                    public void onError(FacebookException e) {
                        Log.w(FB.TAG, "Error occurred, ", e);
                        unityMessage.sendError(e.getMessage());
                    }
                });

        LoginManager loginManager;
        if (isDeviceAuthLogin) {
            loginManager = DeviceLoginManager.getInstance();
        } else {
            loginManager = LoginManager.getInstance();
        }

        if (isPublishPermLogin) {
            loginManager.logInWithPublishPermissions(activity, permissions);
        } else {
            loginManager.logInWithReadPermissions(activity, permissions);
        }
    }
}
