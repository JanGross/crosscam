﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CrossCam.Model;
using CrossCam.Page;
using CrossCam.Wrappers;
using FreshMvvm;
using Xamarin.Forms;

namespace CrossCam.ViewModel
{
    public class SettingsViewModel : FreshBasePageModel
    {
        public Settings Settings { get; set; }
        public Command ResetToDefaults { get; set; }

        // ReSharper disable once MemberCanBeMadeStatic.Global
        public IEnumerable<int> PositiveIntegers => Enumerable.Range(0, 1000).ToList();
        public IEnumerable<string> BorderColors => Enum.GetNames(typeof(BorderColor)).ToList();

        public SettingsViewModel()
        {
            ResetToDefaults = new Command(() =>
            {
                Settings.ResetToDefaults();
            });
        }

        public override void Init(object initData)
        {
            base.Init(initData);
            Settings = (Settings) initData;
            Settings.PropertyChanged += SaveSettings;
        }

        protected override void ViewIsDisappearing(object sender, EventArgs e)
        {
            base.ViewIsDisappearing(sender, e);
            SaveSettings(null, null);
            Settings.PropertyChanged -= SaveSettings;
        }

        private void SaveSettings(object sender, PropertyChangedEventArgs e)
        {
            PersistentStorage.Save(PersistentStorage.SETTINGS_KEY, Settings);
        }
    }
}