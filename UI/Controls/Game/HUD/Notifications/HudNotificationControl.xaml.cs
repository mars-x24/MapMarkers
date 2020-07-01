﻿namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Notifications
{
  using AtomicTorch.CBND.CoreMod.SoundPresets;
  using AtomicTorch.CBND.CoreMod.Systems.Cursor;
  using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Notifications.Data;
  using AtomicTorch.CBND.GameApi.Resources;
  using AtomicTorch.CBND.GameApi.Scripting;
  using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
  using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
  using System;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Input;
  using System.Windows.Media;
  using System.Windows.Media.Animation;

  public partial class HudNotificationControl : BaseUserControl, IHudNotificationControl
  {
    public Action CallbackOnRightClickHide;

    public SoundResource soundToPlay;

    private Border outerBorder;

    private Border root;

    private Storyboard storyboardFadeOut;

    private Storyboard storyboardHide;

    private Storyboard storyboardShow;

    private ViewModelHudNotificationControl viewModel;

    public bool IsAutoHide { get; private set; }

    public bool IsHiding { get; private set; }

    public string Message => this.viewModel?.Message;

    public string Title => this.viewModel?.Title;

    public static event NotificationEventHandler NewNotification;
    public delegate void NotificationEventHandler(NotificationEventArgs e);

    public static HudNotificationControl Create(
            string title,
            string message,
            Brush brushBackground,
            Brush brushForeground,
            ITextureResource icon,
            Action onClick,
            bool autoHide,
            SoundResource soundToPlay)
    {

      NewNotification?.Invoke(new NotificationEventArgs(title, message));

      var iconBrush = icon != null
                                ? Api.Client.UI.GetTextureBrush(icon)
                                : null;

      return new HudNotificationControl()
      {
        viewModel = new ViewModelHudNotificationControl(
              title,
              message,
              brushBackground,
              brushForeground,
              iconBrush,
              onClick),
        IsAutoHide = autoHide,
        soundToPlay = soundToPlay
      };
    }

    public void Hide(bool quick)
    {
      if (quick)
      {
        this.storyboardFadeOut.SpeedRatio = 6.5;
      }

      if (this.IsHiding)
      {
        // already hiding
        return;
      }

      this.IsHiding = true;

      if (!this.isLoaded)
      {
        this.RemoveControl();
        return;
      }

      this.storyboardShow.Stop();
      this.storyboardFadeOut.Begin();
    }

    public void HideAfterDelay(double delaySeconds)
    {
      this.IsAutoHide = false;

      // hide the notification control after delay
      ClientTimersSystem.AddAction(
          delaySeconds,
          () => this.Hide(quick: false));
    }

    public bool IsSame(IHudNotificationControl other)
    {
      return other is HudNotificationControl otherControl
             && this.viewModel.IsSame(otherControl.viewModel);
    }

    public void SetMessage(string message)
    {
      if (this.viewModel is null)
      {
        return;
      }

      this.viewModel.Message = message;
      this.ResetNotificationSize();
    }

    public void SetTitle(string title)
    {
      if (this.viewModel is null)
      {
        return;
      }

      this.viewModel.Title = title;
      this.ResetNotificationSize();
    }

    public void SetupAutoHideChecker(Func<bool> checker)
    {
      Api.Client.Scene.CreateSceneObject("Notification auto hide checker")
         .AddComponent<ClientComponentNotificationAutoHideChecker>()
         .Setup(this, checker);
    }

    protected override void InitControl()
    {
      if (IsDesignTime)
      {
        this.viewModel = new ViewModelHudNotificationControl();
        return;
      }

      this.storyboardShow = this.GetResource<Storyboard>("StoryboardShow");
      this.storyboardHide = this.GetResource<Storyboard>("StoryboardHide");
      this.storyboardFadeOut = this.GetResource<Storyboard>("StoryboardFadeOut");
      this.outerBorder = this.GetByName<Border>("OuterBorder");
      this.root = this.GetByName<Border>("Border");
      this.DataContext = this.viewModel;
    }

    protected override void OnLoaded()
    {
      this.ResetNotificationSize();

      if (IsDesignTime)
      {
        return;
      }

      this.storyboardFadeOut.Completed += this.StoryboardFadeOutCompletedHandler;
      this.storyboardHide.Completed += this.StoryboardHideCompletedHandler;
      this.root.MouseLeftButtonDown += this.RootMouseButtonLeftHandler;
      this.root.MouseRightButtonDown += this.RootMouseButtonRightHandler;
      this.root.MouseEnter += this.RootMouseEnterHandler;
      this.root.MouseLeave += this.RootMouseLeaveHandler;

      this.storyboardShow.Begin();

      if (this.soundToPlay != null)
      {
        Api.Client.Audio.PlayOneShot(this.soundToPlay,
                                     SoundConstants.VolumeUINotifications);
      }
    }

    protected override void OnUnloaded()
    {
      if (IsDesignTime)
      {
        return;
      }

      this.DataContext = null;
      this.viewModel.Dispose();
      this.viewModel = null;

      this.storyboardFadeOut.Completed -= this.StoryboardFadeOutCompletedHandler;
      this.storyboardHide.Completed -= this.StoryboardHideCompletedHandler;
      this.root.MouseLeftButtonDown -= this.RootMouseButtonLeftHandler;
      this.root.MouseRightButtonDown -= this.RootMouseButtonRightHandler;
      this.root.MouseEnter -= this.RootMouseEnterHandler;
      this.root.MouseLeave -= this.RootMouseLeaveHandler;

      // to ensure that the control has a hiding flag (used for ClientComponentNotificationAutoHideChecker)
      this.IsHiding = true;

      this.RemoveControl();
    }

    private void RemoveControl()
    {
      var parent = this.Parent as Panel;
      parent?.Children.Remove(this);
    }

    private void ResetNotificationSize()
    {
      this.UpdateLayout();
      this.root.Measure(new Size(this.outerBorder.ActualWidth, 1000));
      this.viewModel.RequiredHeight = (float)(this.root.DesiredSize.Height
                                              + this.outerBorder.Padding.Top
                                              + this.outerBorder.Padding.Bottom);
    }

    private void RootMouseButtonLeftHandler(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      this.Hide(quick: true);
      this.viewModel.CommandClick?.Execute(null);
    }

    private void RootMouseButtonRightHandler(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      this.CallbackOnRightClickHide?.Invoke();
      this.Hide(quick: true);
    }

    private void RootMouseEnterHandler(object sender, MouseEventArgs e)
    {
      ClientCursorSystem.CurrentCursorId = this.viewModel.Cursor;
    }

    private void RootMouseLeaveHandler(object sender, MouseEventArgs e)
    {
      ClientCursorSystem.CurrentCursorId = CursorId.Default;
    }

    private void StoryboardFadeOutCompletedHandler(object sender, EventArgs e)
    {
      this.storyboardHide.Begin();
    }

    private void StoryboardHideCompletedHandler(object sender, EventArgs e)
    {
      this.RemoveControl();
    }

    private class ClientComponentNotificationAutoHideChecker : ClientComponent
    {
      private Func<bool> checker;

      private HudNotificationControl control;

      public void Setup(HudNotificationControl control, Func<bool> checker)
      {
        this.control = control;
        this.checker = checker;
      }

      public override void Update(double deltaTime)
      {
        if (this.control.IsHiding)
        {
          // checker is not required anymore
          this.SceneObject.Destroy();
          return;
        }

        if (!this.checker())
        {
          return;
        }

        // auto hide check success - hide the notification
        this.control.Hide(quick: false);
        this.SceneObject.Destroy();
      }
    }
  }
}