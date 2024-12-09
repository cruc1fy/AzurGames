//
//  MaxMediationGoogleUtils.cs
//  AppLovin MAX Unity Plugin
//
//  Created by Santosh Bagadi on 11/7/19.
//  Copyright © 2019 AppLovin. All rights reserved.
//

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// An Utils class containing shared convenience methods.
/// </summary>
public static class MaxMediationGoogleUtils
{
    /// <summary>
    /// Loads the AppLovin Settings asset if it is available and returns the value for the given property name.
    /// </summary>
    /// <param name="property">The name of the property for which to get the value of from <c>AppLovinSettings.asset</c> file.</param>
    /// <returns>The string value of the property if found.</returns>
    public static string GetAppIdFromAppLovinSettings(string property)
    {
        var settingsFileName = Path.Combine("Assets/MaxSdk/Resources", "AppLovinSettings.asset");
        if (!File.Exists(settingsFileName))
        {
            Debug.LogError("[AppLovin MAX] The current plugin version is incompatible with the AdMob adapter. Please update the AppLovin MAX plugin to version 2.4.0 or higher.");
            return null;
        }

        var instance = AssetDatabase.LoadAssetAtPath(settingsFileName, Type.GetType("AppLovinSettings"));
        if (instance == null) return null;

        var adMobAppIdProperty = instance.GetType().GetProperty(property);
        return adMobAppIdProperty == null ? null : adMobAppIdProperty.GetValue(instance, null).ToString();
    }
}
