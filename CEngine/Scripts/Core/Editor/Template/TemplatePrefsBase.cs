//------------------------------------------------------------------------------
// XenoTemplatePrefs.cs
//
// Copyright 2015 Xenobrain Games LLC 
//
// Created by Habib Loew on 5/21/2015
// Owner: Habib Loew
// 
// Provides access to the various XenoTemplate editor prefs.
//
// Note that when adding new prefs one must update the PrefKey enum in 
// XenoTemplatePrefsBase, add the appropriate properties to XenoTeplatePrefs,
// and update the ValidatePrefs method in XenoTemplatePrefs.
//
// The weird inheritance going on here is to allow for compiler enforcement
// of an enum constraint on several of our methods.  This methodology is based
// upon a comment in the C# 5.0 language spec which indicates that constraint
// inheritance can be used in this way:
//
//      If type parameter S depends on type parameter T then: [...] It is 
//      valid for S to have the value type constraint and T to have the 
//      reference type constraint. Effectively this limits T to the types 
//      System.Object, System.ValueType, System.Enum, and any interface type.
//
// See also: http://stackoverflow.com/a/28527552/4926393
//
//------------------------------------------------------------------------------

using System;
using UnityEngine;
using UnityEditor;

namespace CYM.Template
{

    namespace PrefsInternal {

        //==============================================================================
        //
        // Base class for XenoTemplatePrefs which allows us to enforce enum constraints
        // on some of our generic methods.
        //
        //==============================================================================

        public abstract class TemplatePrefsBase<TClass> where TClass : class {

            //
            // Internal types
            //

            protected enum PrefKey {
                Eol,
            }

            //------------------------------------------------------------------------------
            protected static TEnum GetEnumPref<TEnum> (PrefKey key, TEnum defaultValue) where TEnum : struct, TClass {

                ValidateEnumPref(key, defaultValue);

                String prefValueString = EditorPrefs.GetString(PrefKeyToString(key));
                return (TEnum)Enum.Parse(typeof(TEnum), prefValueString);

            }

            //------------------------------------------------------------------------------
            protected static String PrefKeyToString (PrefKey key) {

                return Enum.GetName(typeof(PrefKey), key);

            }

            //------------------------------------------------------------------------------
            protected static void SetEnumPref<TEnum> (PrefKey key, TEnum value) where TEnum : struct, TClass {

                EditorPrefs.SetString(PrefKeyToString(key), Enum.GetName(typeof(TEnum), value));

            }

            //------------------------------------------------------------------------------
            protected static void ValidateEnumPref<TEnum> (PrefKey key, TEnum defaultValue) where TEnum : struct, TClass {

                String prefKeyString = PrefKeyToString(key);

                // Nuke any existing value that doesn't exist in our enumeration
                if (EditorPrefs.HasKey(prefKeyString) && !Enum.IsDefined(typeof(TEnum), EditorPrefs.GetString(prefKeyString))) {
                    EditorPrefs.DeleteKey(prefKeyString);
                }

                // Set up an initial value if the pref doesn't exist
                if (!EditorPrefs.HasKey(prefKeyString)) {
                    EditorPrefs.SetString(prefKeyString, Enum.GetName(typeof(TEnum), defaultValue));
                }

            }

        }

    } // namespace PrefsInternal


    //==============================================================================
    //
    // Interface to the XenoTemplate preferences stored in the Unity editor prefs
    //
    //==============================================================================

    public abstract class TemplatePrefs : PrefsInternal.TemplatePrefsBase<Enum> {

        //
        // Public properties
        //

        //------------------------------------------------------------------------------
        public static EditorUtil.EolType Eol {
            get {
                return GetEnumPref(PrefKey.Eol, EditorUtil.EolType.Unix);
            }
            set {
                SetEnumPref(PrefKey.Eol, value);
            }
        }


        //
        // Public methods
        //

        //------------------------------------------------------------------------------
        // Makes sure that all prefs have a good value (includes initialization)
        public static void ValidatePrefs () {

            ValidateEnumPref(PrefKey.Eol, EditorUtil.EolType.Unix);

        }

    }

}
