//==================================================================================
// Copyright © 2018 Ivo Salmre.  ALL RIGHTS RESERVED.  NO WARRANTY OF ANY KIND.
// No part of this code may be used, copied or modified in any way without explicit
// written permission.
//==================================================================================
using System;
using System.Text;
using System.Collections.Generic;
//using System.Web;
using System.Configuration;
using Microsoft.Win32;
using Xamarin.Forms.Shapes;

/// <summary>
/// Global settings for the application
/// </summary>
internal static class AppSettings
{

    /// <summary>
    /// TRUE: We want to use simulated data instead of loading live ata
    /// FALSE: Connect to Cloudbeds and get real data
    /// </summary>
    public static bool UseSimulatedGuestData
    {
        get
        {
            return s_useSimulatedGuestData;
        }
        set
        {
            s_useSimulatedGuestData = value;
        }
    }

    /// <summary>
    /// Load the user's preferred path to the File provisioning file
    /// </summary>
    /// <param name="text"></param>
    internal static string LoadPreference_PathAppSecretsConfig()
    {
        return System.IO.Path.Combine(
            FileIOHelper.MobileDevice_WorkingDirectoryForApp,
            "Cloudbeds_AppConfig.xml");
    }

    internal static string LoadPreference_PathUserAccessTokens()
    {
        return System.IO.Path.Combine(
            FileIOHelper.MobileDevice_WorkingDirectoryForApp,
            "Cloudbeds_AccessTokens.xml");
    }



    /// <summary>
    /// TRUE: Don't connect to Cloudbeds (use simulated data)
    /// FALSE: Connect to Cloudbeds and get real data
    /// </summary>
    private static bool s_useSimulatedGuestData = true;

    /// <summary>
    /// TRUE: We want to write assert contents into files
    /// </summary>
    public static bool DiagnosticsWriteDebugOutputToFile
    {
        get
        {
            return false; 
            
            /*GetAppSettingIntegerBoolean(
                "iwsDiagnosticsWriteDebugOutputToFile",
                false); //Default to not logging user actions
            */
        }
    }

    /// <summary>
    /// Returns the local file system path for the application
    /// </summary>
    public static string LocalFileSystemPath
    {
        get
        {
            //return AppDomain.CurrentDomain.GetData("APPBASE").ToString();
            return System.IO.Directory.GetCurrentDirectory();
        }
    }

    /// <summary>
    /// Returns the local file system path for photo storage
    /// </summary>
    public static string LocalFileSystemPath_Diagnostics
    {
        get
        {
            var fullPathToPhotoDirectory =
                System.IO.Path.Combine(AppSettings.LocalFileSystemPath, @"App_Data\iwsPrivateContent\Diagnostics");

            return fullPathToPhotoDirectory;
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    public static bool DiagnosticsWriteAssertsToFile
    {
        get
        {
            return false;
        }
    }

}