﻿/*
 * Copyright (c) 2015 Mehrzad Chehraz (mehrzady@gmail.com)
 * Released under the MIT License
 * http://chehraz.ir/mit_license
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:

 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.

 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
namespace Atf.ScreenRecorder.UI.View {
   using Atf.ScreenRecorder.Recording;
   using Atf.ScreenRecorder.Screen;
   using Atf.ScreenRecorder.UI.Presentation;
   using Atf.ScreenRecorder.Util;

   using System;
   using System.Collections.Generic;
   using System.ComponentModel;
   using System.Drawing;
   using System.Reflection;
   using System.Windows.Forms;

   public partial class frmMain : Form, IMainView {
      #region Fields
      private static readonly string cancelMessage = "Are you sure to want to cancel recording?";
      private static readonly string errorMessageTitle = "Error";
      private static readonly string hotKeyWarningMessage = "Failed to register one or more hot keys.";
      private static readonly int notifyErrorDelay = 20000;
      private static readonly int notifyWarningDelay = 10000;
      private static readonly string stopMessage = "Are you sure to want to stop recording?";
      private static readonly string warningMessageTitle = "Warning";

      private bool isSelectingTracker;
      private MainPresenter presenter;
      private TrackingType prevTrackingType;
      private TimeSpan recordingDuration;
      private TrackingType trackingType;
      private WindowFinder windowFinder;
      #endregion

      #region Properties
      public string AssemblyProduct {
         get {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute),
                                                                                      false);
            if (attributes.Length == 0) {
               return string.Empty;
            }
            return ((AssemblyProductAttribute)attributes[0]).Product;
         }
      }
      #endregion

      #region Constructors
      public frmMain() {
         InitializeComponent();
         this.notifyIcon.Text = this.AssemblyProduct;
         this.Text = this.AssemblyProduct;
         this.presenter = new MainPresenter(this);
         this.windowFinder = new WindowFinder();

      }
      #endregion

      #region Methods
      private void btnCancel_Click(object sender, EventArgs e) {
         this.OnCancel(EventArgs.Empty);
      }
      private void btnOpenFolder_Click(object sender, EventArgs e) {
         this.OnOpenFolder(EventArgs.Empty);
      }
      private void btnPause_Click(object sender, EventArgs e) {
         this.OnPause(EventArgs.Empty);
      }
      private void btnPlay_Click(object sender, EventArgs e) {
         this.OnPlay(EventArgs.Empty);
      }
      private void btnRecord_Click(object sender, EventArgs e) {
         this.OnRecord(EventArgs.Empty);
      }
      private void btnStop_Click(object sender, EventArgs e) {
         this.OnStop(EventArgs.Empty);
      }     
      private void ctsmiRestore_Click(object sender, EventArgs e) {
         this.OnRestore();
      }
      protected override void Dispose(bool disposing) {
         if (disposing && (components != null)) {
            components.Dispose();            
            this.windowFinder.Dispose();
         }
         base.Dispose(disposing);
      }
      private void frmMain_FormClosing(object sender, FormClosingEventArgs e) {
         CancelEventArgs ea = new CancelEventArgs(e.Cancel);
         this.OnViewClosing(ea);
         e.Cancel = ea.Cancel;
      }
      private void frmMain_Resize(object sender, EventArgs e) {
         if (this.WindowState == FormWindowState.Minimized) {
            this.Hide();
            this.notifyIcon.Visible = true;
         }
         else {
            this.notifyIcon.Visible = false;
         }
      }
      private void notifyIcon_BalloonTipClicked(object sender, EventArgs e) {
         if (!this.Visible) {
            this.OnRestore();
         }
      }
      private void notifyIcon_DoubleClick(object sender, EventArgs e) {
         this.OnRestore();
      }
      private void OnAbout() {
         frmAboutBox frmAboutBox = new frmAboutBox();
         frmAboutBox.ShowDialog(this);
      }
      public void OnCancel(EventArgs e) {
         if (this.Cancel != null) {
            this.Cancel(this, e);
         }
      }     
      private void OnExit() {
         this.Close();
      }
      private void OnOpenFolder(EventArgs e) {
         if (this.OpenFolder != null) {
            this.OpenFolder(this, e);
         }
      }
      private void OnOptions() {
         if (this.Options != null) {
            this.Options(this, EventArgs.Empty);
         }
      }
      private void OnPause(EventArgs e) {
         if (this.Pause != null) {
            this.Pause(this, e);
         }
      }
      private void OnPlay(EventArgs e) {
         if (this.Play != null) {
            this.Play(this, e);
         }
      }
      private void OnRecord(EventArgs e) {
         if (this.Record != null) {
            this.Record(this, e);
         }
      }
      private void OnRestore() {
         this.Show();
         this.WindowState = FormWindowState.Normal;
      }
      private void OnStop(EventArgs e) {
         if (this.Stop != null) {
            this.Stop(this, e);
         }
      }
      private void OnTrackerChanged(TrackerChangedEventArgs ea) {
         BoundsTracker tracker = ea.BoundsTracker;
         this.TrackingBounds = tracker.Bounds;
         this.TrackingType = tracker.Type;   
         if (this.TrackerChanged != null) {
            this.TrackerChanged(this, ea);
         }
      }
      private void OnUpdate(EventArgs e) {
         if (this.Update != null) {
            this.Update(this, e);
         }
      }
      private void OnViewClosed(EventArgs e) {
         if (this.ViewClosed != null) {
            this.ViewClosed(this, e);
         }
      }
      private void OnViewClosing(CancelEventArgs ea) {
         if (this.ViewClosing != null) {
            this.ViewClosing(this, ea);
         }
      }
      private void rdoFixed_Click(object sender, EventArgs e) {
         frmSelectBounds selectBoundsView = new frmSelectBounds();
         this.isSelectingTracker = true;
         if (selectBoundsView.ShowDialog()) {
            this.isSelectingTracker = false;
            BoundsTracker tracker = new BoundsTracker(selectBoundsView.SelectedBounds);
            TrackerChangedEventArgs ea = new TrackerChangedEventArgs(tracker);
            this.OnTrackerChanged(ea);
         }
      }
      private void rdoFull_Click(object sender, EventArgs e) {
         BoundsTracker tracker = new BoundsTracker();
         TrackerChangedEventArgs ea = new TrackerChangedEventArgs(tracker);
         this.OnTrackerChanged(ea);
      }
      private void rdoWindow_MouseDown(object sender, MouseEventArgs e) {
         if (!this.windowFinder.IsFinding && e.Button == MouseButtons.Left) {
            this.windowFinder.BeginFind();
            // Keep current tracking type in case of cancellation.
            this.prevTrackingType = this.TrackingType;
            // Update radio buttons state
            this.TrackingType = TrackingType.Window;
            // Change cursor
            this.Cursor = Cursors.Cross;
         }
      }
      private void rdoWindow_MouseMove(object sender, MouseEventArgs e) {
         if ((e.Button & MouseButtons.Left) == MouseButtons.Left &&
             this.windowFinder.IsFinding) {
            this.windowFinder.Find();
         }
      }
      private void rdoWindow_MouseUp(object sender, MouseEventArgs e) {
         if (this.windowFinder.IsFinding) {
            this.Cursor = Cursors.Default;
            IntPtr hWnd = this.windowFinder.EndFind();
            if (e.Button == MouseButtons.Left) {
               if (hWnd != IntPtr.Zero) {
                  BoundsTracker tracker = new BoundsTracker(hWnd);
                  TrackerChangedEventArgs ea = new TrackerChangedEventArgs(tracker);
                  this.OnTrackerChanged(ea);
               }
               else {                  
                  this.TrackingType = this.prevTrackingType;
               }
            }
            else {               
               this.TrackingType = this.prevTrackingType;
            }
         }
      }
      private void tmrUpdate_Tick(object sender, EventArgs e) {
         this.OnUpdate(EventArgs.Empty);
      }
      private void tsmiAbout_Click(object sender, EventArgs e) {
         this.OnAbout();
      }
      private void tsmiCancel_Click(object sender, EventArgs e) {
         this.OnCancel(EventArgs.Empty);
      }
      private void tsmiExit_Click(object sender, EventArgs e) {
         this.OnExit();
      }
      private void tsmiOptions_Click(object sender, EventArgs e) {
         this.OnOptions();
      }
      private void tsmiPause_Click(object sender, EventArgs e) {
         this.OnPause(EventArgs.Empty);
      }
      private void tsmiPlay_Click(object sender, EventArgs e) {
         this.OnPlay(EventArgs.Empty);
      }
      private void tsmiRecord_Click(object sender, EventArgs e) {
         this.OnRecord(EventArgs.Empty);
      }
      private void tsmiStop_Click(object sender, EventArgs e) {
         this.OnStop(EventArgs.Empty);
      }
      #endregion

      #region IMainView Members
      public event EventHandler Cancel;
      public event EventHandler Options;
      public event EventHandler OpenFolder;
      public event EventHandler Pause;
      public event EventHandler Play;
      public event EventHandler Record;
      public event EventHandler Stop;
      public event TrackerChangedEventHandler TrackerChanged;
      public new event EventHandler Update;
      public event EventHandler ViewClosed;
      public event CancelEventHandler ViewClosing;

      public bool AllowCancel {
         set {
            this.btnCancel.Enabled = value;
            this.ctsmiCancel.Enabled = value;
            this.tsmiCancel.Enabled = value;
         }
      }
      public bool AllowOptions {
         set {
            this.ctsmiOptions.Enabled = value;
            this.tsmiOptions.Enabled = value;
         }
      }
      public bool AllowChangeTrackingType {
         set {
            this.pnlTrackingType.Enabled = value;
         }
      }
      public bool AllowPause {
         set {
            this.btnPause.Enabled = value;
            this.ctsmiPause.Enabled = value;
            this.tsmiPause.Enabled = value;
         }
      }
      public bool AllowPlay {
         set {
            this.btnPlay.Enabled = value;
            this.ctsmiPlay.Enabled = value;
            this.tsmiPlay.Enabled = value;
         }
      }
      public bool AllowRecord {
         set {
            this.btnRecord.Enabled = value;
            this.ctsmiRecord.Enabled = value;
            this.tsmiRecord.Enabled = value;
         }
      }
      public bool AllowStop {
         set {
            this.btnStop.Enabled = value;
            this.ctsmiStop.Enabled = value;
            this.tsmiStop.Enabled = value;
         }
      }
      public bool AllowUpdate {
         set {
            this.tmrUpdate.Enabled = value;
         }
      }
      public Keys CancelHotKey {
         set {
            this.tsmiCancel.ShortcutKeys = value;
         }
      }
      public bool Minimized {
         get {
            return this.WindowState == FormWindowState.Minimized;
         }
         set {
            if (value) {
               this.WindowState = FormWindowState.Minimized;
            }
            else {
               this.OnRestore();
            }
         }
      }
      public Keys PauseHotKey {
         set {
            this.tsmiPause.ShortcutKeys = value;
         }
      }
      public Keys RecordHotKey {
         set {
            this.tsmiRecord.ShortcutKeys = value;          
         }
      }
      public RecordingState RecordingState {
         set {
            Image statusImage;
            Icon notifyIconIcon;
            switch (value) {
               case RecordingState.Paused:
                  notifyIconIcon = Properties.Resources.icon_paused;
                  statusImage = Properties.Resources.playback_pause;
                  break;
               case RecordingState.Recording:
                  bool blink = Math.Ceiling(this.recordingDuration.TotalSeconds) % 2 == 1;
                  if (blink) {
                     notifyIconIcon = Properties.Resources.icon_rec;
                     statusImage = Properties.Resources.primitive_dot_red;
                  }
                  else {
                     notifyIconIcon = Properties.Resources.icon_16;
                     statusImage = Properties.Resources.primitive_dot;
                  }
                  break;
               default:
                  notifyIconIcon = Properties.Resources.icon_16;
                  statusImage = null;
                  break;
            }
            string status = value.ToString();
            this.lblStatus.Image = statusImage;
            this.lblStatus.Text = status;
            this.notifyIcon.Icon = notifyIconIcon;
            string notifyText = string.Format("{0} ({0})", this.AssemblyProduct, status);
            if (!string.Equals(this.notifyIcon.Text, notifyText)) {
               this.notifyIcon.Text = string.Format("{0} ({0})", status);
            }
         }
      }
      public TimeSpan RecordDuration {
         set {
            if (value != TimeSpan.MinValue) {
               this.lblDuration.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
               this.lblDuration.Text = string.Format("{0:00}:{1:00}:{2:00}", value.Hours, value.Minutes, value.Seconds);
            }
            else {
               this.lblDuration.DisplayStyle = ToolStripItemDisplayStyle.None;
               this.lblDuration.Text = string.Empty;
            }
            this.recordingDuration = value;
         }
      }
      public Keys StopHotKey {
         set {
            this.tsmiStop.ShortcutKeys = value;                          
         }
      }
      public Rectangle TrackingBounds {
         set {
            if (!this.isSelectingTracker) {
               this.lblCaptureOrigin.Text = string.Format("({0}, {1})", value.Left, value.Top);
               this.lblCaptureSize.Text = string.Format("{0}x{1}", value.Width, value.Height);
            }
         }
      }
      public TrackingType TrackingType {
         get {
            return this.trackingType;
         }
         set {
            if (!this.isSelectingTracker) {
               this.trackingType = value;
               switch (value) {
                  case TrackingType.Full:
                     rdoFull.Checked = true;
                     rdoPartial.Checked = false;
                     rdoWindow.Checked = false;
                     break;
                  case TrackingType.Fixed:
                     rdoFull.Checked = false;
                     rdoPartial.Checked = true;
                     rdoWindow.Checked = false;
                     break;
                  case TrackingType.Window:
                     rdoFull.Checked = false;
                     rdoPartial.Checked = false;
                     rdoWindow.Checked = true;
                     break;
               }
            }
         }
      }
      public bool ShowCancelMessage() {
         if (!this.Visible) {
            this.OnRestore();
         }
         DialogResult result = MessageBox.Show(this, cancelMessage, this.AssemblyProduct,
                                               MessageBoxButtons.YesNo,
                                               MessageBoxIcon.None, MessageBoxDefaultButton.Button2);
         return (result == DialogResult.Yes);
      }
      public void ShowError(string message) {
         if (this.InvokeRequired) {
            this.BeginInvoke((MethodInvoker)delegate {
               ShowError(message);
            });
         }
         else {
            if (this.Visible) {
               MessageBox.Show(this, message, errorMessageTitle, MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
            }
            else {
               this.notifyIcon.ShowBalloonTip(notifyErrorDelay, errorMessageTitle, message, 
                                              ToolTipIcon.Error);
            }
         }
      }
      public void ShowHotKeyRegisterWarning() {
         if (this.Visible) {
            MessageBox.Show(this, hotKeyWarningMessage, errorMessageTitle, MessageBoxButtons.OK,
                            MessageBoxIcon.Exclamation);
         }
         else {
            this.notifyIcon.ShowBalloonTip(notifyWarningDelay, errorMessageTitle, warningMessageTitle,
                                           ToolTipIcon.Warning);

         }
      }
      public bool ShowStopMessage() {
         if (!this.Visible) {
            this.OnRestore();
         }
         DialogResult result = MessageBox.Show(this, stopMessage, this.AssemblyProduct,
                                               MessageBoxButtons.YesNo,
                                               MessageBoxIcon.None, MessageBoxDefaultButton.Button2);
         return (result == DialogResult.Yes);
      }
      //public void ShowWindowInaccessibleWarning() {
      //   if (this.Visible) {
      //      MessageBox.Show(this, windowInaccessibleMessage, warningMessageTitle,
      //                      MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      //   }
      //   else {
      //      this.notifyIcon.ShowBalloonTip(notifyWarningDelay, warningMessageTitle, windowInaccessibleMessage,
      //                                     ToolTipIcon.Warning);
      //   }
      //}

      #endregion

      #region IView Members
      public bool Result {
         get;
         set;
      }
      public new bool ShowDialog() {
         base.ShowDialog();
         return this.Result;
      }
      public bool ShowDialog(IView owner) {
         base.ShowDialog((IWin32Window)owner);
         return this.Result;
      }
      #endregion
   }
}