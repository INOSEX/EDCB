﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using CtrlCmdCLI;
using CtrlCmdCLI.Def;
using EpgTimer.EpgView;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;

namespace EpgTimer.EpgView
{
    /// <summary>
    /// ProgramView.xaml の相互作用ロジック
    /// </summary>
    public partial class ProgramView : UserControl
    {
        public delegate void ProgramViewClickHandler(object sender, Point cursorPos);
        public event ScrollChangedEventHandler ScrollChanged = null;
        public event ProgramViewClickHandler LeftDoubleClick = null;
        public event ProgramViewClickHandler RightClick = null;
        private List<Rectangle> reserveBorder = new List<Rectangle>();
        private Dictionary<UInt64, EpgServiceItem> serviceList = new Dictionary<UInt64, EpgServiceItem>();

        private Point lastDownMousePos;
        private double lastDownHOffset;
        private double lastDownVOffset;
        private bool isDrag = false;
        private bool isDragMoved = false;

        private Point lastPopupPos;
        private ProgramViewItem lastPopupInfo;
        private Rectangle popupReserve = null;
        private double popupReserveWidth = 0;
        private double popupReserveHeight = 0;
        private double popupReserveLeftPos = 0;

        public ProgramView()
        {
            InitializeComponent();

        }

        protected void PopupItem()
        {
            ProgramViewItem info = null;

            if (epgViewPanel.Items != null)
            {
                //if (MainWindow.GetWindow(this).IsActive == false)
                //{
                //    return;
                //}
                Point cursorPos2 = Mouse.GetPosition(scrollViewer);
                if (cursorPos2.X < 0 || cursorPos2.Y < 0 ||
                    scrollViewer.ViewportWidth < cursorPos2.X || scrollViewer.ViewportHeight < cursorPos2.Y)
                {
                    return;
                }
                Point cursorPos = Mouse.GetPosition(epgViewPanel);
                foreach (ProgramViewItem item in epgViewPanel.Items)
                {
                    if (item.LeftPos <= cursorPos.X && cursorPos.X < item.LeftPos + item.Width)
                    {
                        if (item.TopPos <= cursorPos.Y && cursorPos.Y < item.TopPos + item.Height)
                        {
                            if (item == lastPopupInfo) return;

                            info = item;
                            lastPopupInfo = info;
                            break;
                        }
                    }
                }
            }

            if (popupReserve != null)
            {
                popupReserve.Width = popupReserveWidth;
                popupReserve.Height = popupReserveHeight;
                Canvas.SetLeft(popupReserve, popupReserveLeftPos);
                Canvas.SetZIndex(popupReserve, 10);
                popupReserve = null;
            }

            if (info == null)
            {
                popupItem.Visibility = System.Windows.Visibility.Hidden;
                lastPopupInfo = null;
                return;
            }

            UInt64 sidKey = CommonManager.Create64Key(info.EventInfo.original_network_id, info.EventInfo.transport_stream_id, info.EventInfo.service_id);

            double sizeNormal = Settings.Instance.FontSize;
            double sizeTitle = Settings.Instance.FontSizeTitle;

            FontFamily fontNormal = null;
            FontFamily fontTitle = null;
            try
            {
                if (Settings.Instance.FontName.Length > 0)
                {
                    fontNormal = new FontFamily(Settings.Instance.FontName);
                }
                if (Settings.Instance.FontNameTitle.Length > 0)
                {
                    fontTitle = new FontFamily(Settings.Instance.FontNameTitle);
                }

                if (fontNormal == null)
                {
                    fontNormal = new FontFamily("MS UI Gothic");
                }
                if (fontTitle == null)
                {
                    fontTitle =new FontFamily("MS UI Gothic");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }

            popupItem.Background = info.ContentColor;
            Canvas.SetTop(popupItem, info.TopPos);
            popupItem.MinHeight = info.Height;
            if (serviceList.ContainsKey(sidKey))
            {
                Canvas.SetLeft(popupItem, serviceList[serviceList[sidKey].GroupID].LeftPos);
                popupItem.Width = serviceList[serviceList[sidKey].GroupID].GroupWidth;
            }
            else
            {
                Canvas.SetLeft(popupItem, info.LeftPos);
                popupItem.Width = info.Width;
            }

            FontWeight titleWeight = Settings.Instance.FontBoldTitle ? FontWeights.Bold : FontWeights.Normal;

            string min;
            if (info.EventInfo.StartTimeFlag == 1)
            {
                min = info.EventInfo.start_time.Minute.ToString("d02");
            }
            else
            {
                min = "未定";
            }

            minText.Text = min;
            minText.FontFamily = fontTitle;
            minText.FontSize = sizeTitle - 0.5;
            minText.FontWeight = titleWeight;
            minText.Foreground = CommonManager.Instance.CustTitle1Color;
            minText.Margin = new Thickness(0.5, -0.5, 0, 0);

            minGrid.Width = new GridLength(sizeNormal * 1.7 + 1);

            if (info.EventInfo.ShortInfo != null)
            {
                titleText.Text = info.EventInfo.ShortInfo.event_name;
                titleText.FontFamily = fontTitle;
                titleText.FontSize = sizeTitle;
                titleText.FontWeight = titleWeight;
                titleText.Foreground = CommonManager.Instance.CustTitle1Color;
                titleText.Margin = new Thickness(1, 1.5, 5.5, sizeNormal / 2);
                titleText.LineHeight = sizeTitle + 2;

                infoText.Text = info.EventInfo.ShortInfo.text_char;
                infoText.FontFamily = fontNormal;
                infoText.FontSize = sizeNormal;
                infoText.FontWeight = FontWeights.Normal;
                infoText.Foreground = CommonManager.Instance.CustTitle2Color;
                infoText.Margin = new Thickness(1, 0, 9.5, 1);
                infoText.LineHeight = sizeNormal + 2;
            }
            else
            {
                titleText.Text = null;
                infoText.Text = null;
            }

            popupItem.Visibility = System.Windows.Visibility.Visible;

            foreach (Rectangle rect in reserveBorder)
            {
                if (Canvas.GetLeft(rect) == info.LeftPos && Canvas.GetTop(rect) == info.TopPos)
                {
                    popupReserve = rect;
                    popupReserveWidth = rect.Width;
                    popupReserveHeight = rect.Height;
                    popupReserveLeftPos = Canvas.GetLeft(popupReserve);
                    popupItem.UpdateLayout();
                    rect.Width = popupItem.ActualWidth;
                    rect.Height = popupItem.ActualHeight;
                    if (serviceList.ContainsKey(sidKey))
                    {
                        Canvas.SetLeft(rect, serviceList[serviceList[sidKey].GroupID].LeftPos);
                    }
                    Canvas.SetZIndex(rect, 30);
                    break;
                }
            }
        }

        public void ClearInfo()
        {
            lastPopupInfo = null;
            popupReserve = null;
            popupItem.Visibility = System.Windows.Visibility.Hidden;

            foreach (Rectangle info in reserveBorder)
            {
                canvas.Children.Remove(info);
            }
            reserveBorder.Clear();
            reserveBorder = null;
            reserveBorder = new List<Rectangle>();

            epgViewPanel.ReleaseMouseCapture();
            isDrag = false;

            epgViewPanel.Items = null;
            epgViewPanel.Height = 0;
            epgViewPanel.Width = 0;
            canvas.Height = 0;
            canvas.Width = 0;

            serviceList.Clear();
            serviceList = null;
            serviceList = new Dictionary<ulong, EpgServiceItem>();
        }

        public void SetReserveList(List<ReserveViewItem> reserveList)
        {
            try
            {
                popupReserve = null;
                foreach (Rectangle info in reserveBorder)
                {
                    canvas.Children.Remove(info);
                }
                reserveBorder.Clear();

                foreach (ReserveViewItem info in reserveList)
                {
                    Rectangle rect = new Rectangle();
                    rect.Opacity = 0.3;
                    rect.Effect = new System.Windows.Media.Effects.DropShadowEffect() { BlurRadius = 6 };

                    Brush color;
                    if (info.ReserveInfo.RecSetting.RecMode == 5)
                    {
                        color = CommonManager.Instance.CustContentColorList[0x12];
                    }
                    else if (info.ReserveInfo.OverlapMode == 2)
                    {
                        color = CommonManager.Instance.CustContentColorList[0x13];
                    }
                    else if (info.ReserveInfo.OverlapMode == 1)
                    {
                        color = CommonManager.Instance.CustContentColorList[0x14];
                    }
                    else
                    {
                        color = CommonManager.Instance.CustContentColorList[0x11];
                    }

                    if (Settings.Instance.ReserveRectBackground == false)
                    {
                        rect.Fill = System.Windows.Media.Brushes.Transparent;
                        rect.StrokeThickness = 3;

                        rect.Stroke = color;
                    }
                    else
                    {
                        rect.Fill = color;
                    }
                    rect.Width = info.Width;
                    rect.Height = info.Height;
                    rect.IsHitTestVisible = false;

                    Canvas.SetLeft(rect, info.LeftPos);
                    Canvas.SetTop(rect, info.TopPos);
                    Canvas.SetZIndex(rect, 10);
                    canvas.Children.Add(rect);
                    reserveBorder.Add(rect);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        public void SetService(Dictionary<UInt64, EpgServiceItem> servicelist)
        {
            serviceList = servicelist;
        }

        public void SetProgramList(List<ProgramViewItem> programList, double width, double height)
        {
            try
            {
                canvas.Height = Math.Ceiling(height);
                canvas.Width = Math.Ceiling(width);
                epgViewPanel.Height = Math.Ceiling(height);
                epgViewPanel.Width = Math.Ceiling(width);
                epgViewPanel.IsTitleIndent = Settings.Instance.EpgTitleIndent;
                epgViewPanel.Items = programList;
                epgViewPanel.InvalidateVisual();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void epgViewPanel_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (sender.GetType() == typeof(EpgViewPanel))
                {
                    if (e.LeftButton == MouseButtonState.Pressed && isDrag == true)
                    {
                        isDragMoved = true;

                        Point CursorPos = Mouse.GetPosition(null);
                        double MoveX = lastDownMousePos.X - CursorPos.X;
                        double MoveY = lastDownMousePos.Y - CursorPos.Y;

                        double OffsetH = 0;
                        double OffsetV = 0;
                        MoveX *= Settings.Instance.DragScroll;
                        MoveY *= Settings.Instance.DragScroll;
                        OffsetH = lastDownHOffset + MoveX;
                        OffsetV = lastDownVOffset + MoveY;
                        if (OffsetH < 0)
                        {
                            OffsetH = 0;
                        }
                        if (OffsetV < 0)
                        {
                            OffsetV = 0;
                        }

                        scrollViewer.ScrollToHorizontalOffset(Math.Floor(OffsetH));
                        scrollViewer.ScrollToVerticalOffset(Math.Floor(OffsetV));
                    }
                    else
                    {
                        Point CursorPos = Mouse.GetPosition(epgViewPanel);
                        if (lastPopupPos != CursorPos)
                        {
                            PopupItem();
                            lastPopupPos = CursorPos;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void epgViewPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                lastDownMousePos = Mouse.GetPosition(null);
                lastDownHOffset = scrollViewer.HorizontalOffset;
                lastDownVOffset = scrollViewer.VerticalOffset;
                epgViewPanel.CaptureMouse();
                isDrag = true;
                isDragMoved = false;

                if (e.ClickCount == 2)
                {
                    Point cursorPos = Mouse.GetPosition(epgViewPanel);
                    if (LeftDoubleClick != null)
                    {
                        LeftDoubleClick(sender, cursorPos);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void epgViewPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                epgViewPanel.ReleaseMouseCapture();
                isDrag = false;
                if (isDragMoved == false)
                {
                    if (Settings.Instance.EpgInfoSingleClick == true)
                    {
                        Point cursorPos = Mouse.GetPosition(epgViewPanel);
                        if (LeftDoubleClick != null)
                        {
                            LeftDoubleClick(sender, cursorPos);
                        }
                    }
                }
                isDragMoved = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void scrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (ScrollChanged != null)
            {
                scrollViewer.ScrollToHorizontalOffset(Math.Floor(scrollViewer.HorizontalOffset));
                scrollViewer.ScrollToVerticalOffset(Math.Floor(scrollViewer.VerticalOffset));
                ScrollChanged(this, e);
            }
        }

        private void epgViewPanel_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            epgViewPanel.ReleaseMouseCapture();
            isDrag = false; 
            lastDownMousePos = Mouse.GetPosition(null);
            lastDownHOffset = scrollViewer.HorizontalOffset;
            lastDownVOffset = scrollViewer.VerticalOffset;
            if (e.ClickCount == 1)
            {
                Point cursorPos = Mouse.GetPosition(epgViewPanel);
                if (RightClick != null)
                {
                    RightClick(sender, cursorPos);
                }
            }
        }

        private void epgViewPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            popupItem.Visibility = System.Windows.Visibility.Hidden;
            lastPopupInfo = null;
            lastPopupPos = new Point(-1, -1);
            if (popupReserve != null)
            {
                popupReserve.Width = popupReserveWidth;
                popupReserve.Height = popupReserveHeight;
                Canvas.SetLeft(popupReserve, popupReserveLeftPos);
                Canvas.SetZIndex(popupReserve, 10);
                popupReserve = null;
            }
        }
    }
}
