using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using PluginCore.DockPanelSuite;
using PluginCore.Helpers;

namespace WeifenLuo.WinFormsUI.Docking
{
    internal class VS2005DockPaneStrip : DockPaneStripBase
    {
        class TabVS2005 : Tab
        {
            public TabVS2005(IDockContent content)
                : base(content)
            {
            }

            public int TabX { get; set; }
            public int TabWidth { get; set; }
            public int MaxWidth { get; set; }
            protected internal bool Flag { get; set; }
        }

        protected internal override Tab CreateTab(IDockContent content) => new TabVS2005(content);

        sealed class InertButton : InertButtonBase
        {
            readonly Bitmap m_image0;
            readonly Bitmap m_image1;

            public InertButton(Bitmap image0, Bitmap image1)
            {
                m_image0 = image0;
                m_image1 = image1;
            }

            int m_imageCategory;
            public int ImageCategory
            {
                get => m_imageCategory;
                set
                {
                    if (value == m_imageCategory) return;
                    m_imageCategory = value;
                    Invalidate();
                }
            }

            public override Bitmap Image => ImageCategory == 0 ? m_image0 : m_image1;

            protected override void OnRefreshChanges()
            {
                if (ImageColor != ForeColor)
                {
                    ForeColor = ImageColor;
                    Invalidate();
                }
            }
        }

        #region consts
        // CHANGED - NICK
        const int _ToolWindowStripGapTop = 0;
        const int _ToolWindowStripGapBottom = 1;
        const int _ToolWindowStripGapLeft = 0;
        const int _ToolWindowStripGapRight = 0;
        const int _ToolWindowImageHeight = 16;
        const int _ToolWindowImageWidth = 16;
        const int _ToolWindowImageGapTop = 3;
        const int _ToolWindowImageGapBottom = 1;
        const int _ToolWindowImageGapLeft = 4;
        const int _ToolWindowImageGapRight = 0;
        const int _ToolWindowTextGapRight = 2;
        const int _ToolWindowTabSeperatorGapTop = 3;
        const int _ToolWindowTabSeperatorGapBottom = 3;

        const int _DocumentStripGapTop = 0;
        const int _DocumentStripGapBottom = 0;
        const int _DocumentTabMaxWidth = 200;
        const int _DocumentButtonGapTop = 2;
        const int _DocumentButtonGapBottom = 3;
        const int _DocumentButtonGapBetween = 0;
        const int _DocumentButtonGapRight = 2;
        const int _DocumentTabGapTop = 2;
        const int _DocumentTabGapLeft = 0;
        const int _DocumentTabGapRight = 3;
        const int _DocumentIconGapBottom = 2;
        const int _DocumentIconGapLeft = 8;
        const int _DocumentIconGapRight = 0;
        const int _DocumentIconHeight = 16;
        const int _DocumentIconWidth = 16;
        const int _DocumentTextGapRight = 3;
        #endregion

        static Bitmap _imageButtonClose;

        static Bitmap ImageButtonClose => _imageButtonClose ??= ScaleHelper.Scale(Resources.DockPane_Close);

        InertButton m_buttonClose;

        InertButton ButtonClose
        {
            get
            {
                if (m_buttonClose is null)
                {
                    m_buttonClose = new InertButton(ImageButtonClose, ImageButtonClose);
                    m_toolTip.SetToolTip(m_buttonClose, ToolTipClose);
                    m_buttonClose.Click += Close_Click;
                    Controls.Add(m_buttonClose);
                }
                return m_buttonClose;
            }
        }

        static Bitmap _imageButtonWindowList;

        static Bitmap ImageButtonWindowList => _imageButtonWindowList ??= ScaleHelper.Scale(Resources.DockPane_Option);

        static Bitmap _imageButtonWindowListOverflow;

        static Bitmap ImageButtonWindowListOverflow => _imageButtonWindowListOverflow ??= ScaleHelper.Scale(Resources.DockPane_OptionOverflow);

        InertButton m_buttonWindowList;

        InertButton ButtonWindowList
        {
            get
            {
                if (m_buttonWindowList is null)
                {
                    m_buttonWindowList = new InertButton(ImageButtonWindowList, ImageButtonWindowListOverflow);
                    m_toolTip.SetToolTip(m_buttonWindowList, ToolTipSelect);
                    m_buttonWindowList.Click += WindowList_Click;
                    Controls.Add(m_buttonWindowList);
                }

                return m_buttonWindowList;
            }
        }

        static GraphicsPath GraphicsPath => VS2005AutoHideStrip.GraphicsPath;

        readonly ToolTip m_toolTip;
        IContainer Components { get; }

        #region Customizable Properties

        static int ToolWindowStripGapTop => ScaleHelper.Scale(_ToolWindowStripGapTop);

        static int ToolWindowStripGapBottom => ScaleHelper.Scale(_ToolWindowStripGapBottom);

        static int ToolWindowStripGapLeft => ScaleHelper.Scale(_ToolWindowStripGapLeft);

        static int ToolWindowStripGapRight => ScaleHelper.Scale(_ToolWindowStripGapRight);

        static int ToolWindowImageHeight => ScaleHelper.Scale(_ToolWindowImageHeight);

        static int ToolWindowImageWidth => ScaleHelper.Scale(_ToolWindowImageWidth);

        static int ToolWindowImageGapTop => ScaleHelper.Scale(_ToolWindowImageGapTop);

        static int ToolWindowImageGapBottom => ScaleHelper.Scale(_ToolWindowImageGapBottom);

        static int ToolWindowImageGapLeft => ScaleHelper.Scale(_ToolWindowImageGapLeft);

        static int ToolWindowImageGapRight => ScaleHelper.Scale(_ToolWindowImageGapRight);

        static int ToolWindowTextGapRight => ScaleHelper.Scale(_ToolWindowTextGapRight);

        static int ToolWindowTabSeperatorGapTop => ScaleHelper.Scale(_ToolWindowTabSeperatorGapTop);

        static int ToolWindowTabSeperatorGapBottom => ScaleHelper.Scale(_ToolWindowTabSeperatorGapBottom);

        static string _toolTipClose;

        static string ToolTipClose
        {
            get
            {   
                if (_toolTipClose is null)
                    // HACK: _toolTipClose = Strings.DockPaneStrip_ToolTipClose;
                    _toolTipClose = PluginCore.Localization.TextHelper.GetString("PluginCore.Docking.ToolTipClose");
                return _toolTipClose;
            }
        }

        static string _toolTipSelect;

        static string ToolTipSelect
        {
            get
            {   
                if (_toolTipSelect is null)
                    // HACK: _toolTipSelect = Strings.DockPaneStrip_ToolTipWindowList;
                    _toolTipSelect = PluginCore.Localization.TextHelper.GetString("PluginCore.Docking.ToolTipWindowList");
                return _toolTipSelect;
            }
        }

        TextFormatFlags ToolWindowTextFormat
        {
            get
            {   
                const TextFormatFlags textFormat = TextFormatFlags.EndEllipsis
                                                   | TextFormatFlags.HorizontalCenter
                                                   | TextFormatFlags.SingleLine
                                                   | TextFormatFlags.VerticalCenter;
                if (RightToLeft == RightToLeft.Yes) return textFormat | TextFormatFlags.RightToLeft | TextFormatFlags.Right;
                return textFormat;
            }
        }

        static int DocumentStripGapTop => _DocumentStripGapTop;

        static int DocumentStripGapBottom => _DocumentStripGapBottom;

        TextFormatFlags DocumentTextFormat
        {
            get
            {   
                 const TextFormatFlags textFormat = TextFormatFlags.SingleLine
                                                    | TextFormatFlags.VerticalCenter
                                                    | TextFormatFlags.PreserveGraphicsClipping
                                                    | TextFormatFlags.HorizontalCenter;
                 if (RightToLeft == RightToLeft.Yes) return textFormat | TextFormatFlags.RightToLeft;
                 return textFormat;
            }
        }

        static int DocumentTabMaxWidth => ScaleHelper.Scale(_DocumentTabMaxWidth);

        static int DocumentButtonGapTop => ScaleHelper.Scale(_DocumentButtonGapTop);

        static int DocumentButtonGapBottom => ScaleHelper.Scale(_DocumentButtonGapBottom);

        static int DocumentButtonGapBetween => ScaleHelper.Scale(_DocumentButtonGapBetween);

        static int DocumentButtonGapRight => ScaleHelper.Scale(_DocumentButtonGapRight);

        // HACK
        static int DocumentTabGapTop
        {
            get
            {
                return PluginCore.PluginBase.MainForm.GetThemeValue("VS2005DockPaneStrip.TabSize") switch
                {
                    "Large" => ScaleHelper.Scale(_DocumentTabGapTop - 2),
                    _ => ScaleHelper.Scale(_DocumentTabGapTop)
                };
            }
        }

        static int DocumentTabGapLeft => ScaleHelper.Scale(_DocumentTabGapLeft);

        static int DocumentTabGapRight => ScaleHelper.Scale(_DocumentTabGapRight);

        static int DocumentIconGapBottom => ScaleHelper.Scale(_DocumentIconGapBottom);

        // HACK
        static int DocumentIconGapLeft
        {
            get
            {
                return PluginCore.PluginBase.MainForm.GetThemeValue("VS2005DockPaneStrip.TabStyle") switch
                {
                    "Rect" => ScaleHelper.Scale(_DocumentIconGapLeft - 4),
                    _ => ScaleHelper.Scale(_DocumentIconGapLeft)
                };
            }
        }

        static int DocumentIconGapRight => ScaleHelper.Scale(_DocumentIconGapRight);

        static int DocumentIconWidth => ScaleHelper.Scale(_DocumentIconWidth);

        static int DocumentIconHeight => ScaleHelper.Scale(_DocumentIconHeight);

        static int DocumentTextGapRight => ScaleHelper.Scale(_DocumentTextGapRight);

        static Pen PenToolWindowTabActiveBorder
        {
            get
            {
                var color = PluginCore.PluginBase.MainForm.GetThemeColor("VS2005DockPaneStrip.ToolActiveBorderColor");
                return color != Color.Empty ? new Pen(color) : SystemPens.ControlDark;
            }
        }

        static Pen PenToolWindowTabInactiveBorder
        {
            get 
            {
                var color = PluginCore.PluginBase.MainForm.GetThemeColor("VS2005DockPaneStrip.ToolBorderColor");
                return color != Color.Empty ? new Pen(color) : Pens.Transparent;
            }
        }

        // HACK
        static Pen PenDocumentTabActiveBorder
        {
            get 
            {
                var color = PluginCore.PluginBase.MainForm.GetThemeColor("VS2005DockPaneStrip.DocTabActiveBorder");
                return color != Color.Empty ? new Pen(color) : SystemPens.ControlDark;
            }
        }

        // HACK
        static Pen PenDocumentTabInactiveBorder
        {
            get 
            {
                var color = PluginCore.PluginBase.MainForm.GetThemeColor("VS2005DockPaneStrip.DocTabInactiveBorder");
                return color != Color.Empty ? new Pen(color) : SystemPens.ControlDark;
            }
        }

        // HACK
        static Brush BrushToolWindowActiveBackground
        {
            get 
            {
                var color = PluginCore.PluginBase.MainForm.GetThemeColor("VS2005DockPaneStrip.ToolActiveBackColor");
                if (color != Color.Empty) return new SolidBrush(color);
                return PluginCore.PluginBase.Settings.UseSystemColors ? SystemBrushes.Control : Brushes.White;
            }
        }

        // HACK
        static Brush BrushDocumentActiveBackground
        {
            get 
            {
                var color = PluginCore.PluginBase.MainForm.GetThemeColor("VS2005DockPaneStrip.DocActiveBackColor");
                if (color != Color.Empty) return new SolidBrush(color);
                return PluginCore.PluginBase.Settings.UseSystemColors ? SystemBrushes.ControlLightLight : Brushes.White;
            }
        }

        static Brush BrushDocumentInactiveBackground => SystemBrushes.ControlLight;

        static Color ColorToolWindowActiveText
        {
            get 
            {
                Color color = PluginCore.PluginBase.MainForm.GetThemeColor("VS2005DockPaneStrip.ToolActiveForeColor");
                if (color == Color.Empty) color = PluginCore.PluginBase.MainForm.GetThemeColor("VS2005DockPaneStrip.ForeColor");
                if (color != Color.Empty) return color;
                return SystemColors.ControlText;
            }
        }

        static Color ColorDocumentActiveText
        {
            get 
            {
                Color color = PluginCore.PluginBase.MainForm.GetThemeColor("VS2005DockPaneStrip.DocTabActiveForeColor");
                if (color == Color.Empty) color = PluginCore.PluginBase.MainForm.GetThemeColor("VS2005DockPaneStrip.TabForeColor");
                if (color != Color.Empty) return color;
                return SystemColors.ControlText;
            }
        }

        static Color ColorToolWindowInactiveText
        {
            get 
            {
                Color color = PluginCore.PluginBase.MainForm.GetThemeColor("VS2005DockPaneStrip.ForeColor");
                if (color != Color.Empty) return color;
                return SystemColors.ControlDarkDark;
            }
        }

        static Color ColorDocumentInactiveText
        {
            get 
            {
                Color color = PluginCore.PluginBase.MainForm.GetThemeColor("VS2005DockPaneStrip.TabForeColor");
                if (color != Color.Empty) return color;
                return SystemColors.ControlText;
            }
        }

        static Color ImageColor
        {
            get
            {
                Color color = PluginCore.PluginBase.MainForm.GetThemeColor("VS2005DockPaneStrip.ImageColor");
                if (color != Color.Empty) return color;
                return ColorDocumentActiveText;
            }
        }

        #endregion

        public VS2005DockPaneStrip(DockPane pane) : base(pane)
        {
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            SuspendLayout();

            Font = PluginCore.PluginBase.Settings.DefaultFont;
            
            Components = new Container();
            m_toolTip = new ToolTip(Components);
            m_selectMenu = new ContextMenuStrip(Components);
            m_selectMenu.Font = PluginCore.PluginBase.Settings.DefaultFont;
            m_selectMenu.ImageScalingSize = ScaleHelper.Scale(new Size(16, 16));
            m_selectMenu.Renderer = new DockPanelStripRenderer(false);
            m_selectMenu.MouseWheel += ContextMenu_MouseWheel;

            ResumeLayout();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) Components.Dispose();
            base.Dispose (disposing);
        }

        Font m_boldFont;

        Font BoldFont => m_boldFont ??= new Font(Font, FontStyle.Bold);

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            if (m_boldFont != null)
            {
                m_boldFont.Dispose();
                m_boldFont = null;
            }
        }

        int FirstDisplayingTab { get; set; }

        int m_startDisplayingTab;

        int StartDisplayingTab
        {
            get => m_startDisplayingTab;
            set
            {
                m_startDisplayingTab = value;
                Invalidate();
            }
        }

        int EndDisplayingTab { get; set; }

        bool m_documentTabsOverflow;

        bool DocumentTabsOverflow
        {
            set
            {
                if (m_documentTabsOverflow == value) return;
                m_documentTabsOverflow = value;
                ButtonWindowList.ImageCategory = value ? 1 : 0;
            }
        }

        protected internal override int MeasureHeight()
        {
            if (Appearance == DockPane.AppearanceStyle.ToolWindow)
                return MeasureHeight_ToolWindow();
            return MeasureHeight_Document();
        }

        int MeasureHeight_ToolWindow()
        {
            if (DockPane.IsAutoHide || Tabs.Count <= 1)
                return 0;

            int height = Math.Max(Font.Height, ToolWindowImageHeight + ToolWindowImageGapTop + ToolWindowImageGapBottom)
                + ToolWindowStripGapTop + ToolWindowStripGapBottom;

            return height;
        }

        int MeasureHeight_Document()
        {
            int height = Math.Max(Font.Height + DocumentTabGapTop,
                ButtonClose.Height + DocumentButtonGapTop + DocumentButtonGapBottom)
                + DocumentStripGapBottom + DocumentStripGapTop;

            return height;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Appearance == DockPane.AppearanceStyle.Document)
            {
               Color color = PluginCore.PluginBase.MainForm.GetThemeColor("VS2005DockPaneStrip.DocBackColor");
               if (color != Color.Empty)  BackColor = color;
               else if (PluginCore.PluginBase.Settings.UseSystemColors)
               {
                   if (BackColor != SystemColors.ControlLight)
                   {
                       BackColor = SystemColors.ControlLight;
                   }
               }
               else if (BackColor != Color.FromArgb(228, 226, 213))
               {
                   BackColor = Color.FromArgb(228, 226, 213);
               }
            }
            else
            {
               Color color = PluginCore.PluginBase.MainForm.GetThemeColor("VS2005DockPaneStrip.ToolBackColor");
               if (color != Color.Empty) BackColor = color;
               else if (BackColor != SystemColors.Control)
               {
                   BackColor = SystemColors.Control;
               }
            }
            base.OnPaint(e);
            CalculateTabs();
            if (Appearance == DockPane.AppearanceStyle.Document && DockPane.ActiveContent != null)
            {
                if (EnsureDocumentTabVisible(DockPane.ActiveContent, false)) CalculateTabs();
            }
            DrawTabStrip(e.Graphics);
        }

        protected override void OnRefreshChanges()
        {
            SetInertButtons();
            Invalidate();
        }

        protected internal override GraphicsPath GetOutline(int index)
        {

            if (Appearance == DockPane.AppearanceStyle.Document)
                return GetOutline_Document(index);
            return GetOutline_ToolWindow(index);

        }

        GraphicsPath GetOutline_Document(int index)
        {
            Rectangle rectTab = GetTabRectangle(index);
            rectTab.X -= rectTab.Height / 2;
            rectTab.Intersect(TabsRectangle);
            rectTab = RectangleToScreen(DrawHelper.RtlTransform(this, rectTab));
            Rectangle rectPaneClient = DockPane.RectangleToScreen(DockPane.ClientRectangle);

            GraphicsPath path = new GraphicsPath();
            GraphicsPath pathTab = GetTabOutline_Document(Tabs[index], true, true, true);
            path.AddPath(pathTab, true);
            path.AddLine(rectTab.Right, rectTab.Bottom, rectPaneClient.Right, rectTab.Bottom);
            path.AddLine(rectPaneClient.Right, rectTab.Bottom, rectPaneClient.Right, rectPaneClient.Bottom);
            path.AddLine(rectPaneClient.Right, rectPaneClient.Bottom, rectPaneClient.Left, rectPaneClient.Bottom);
            path.AddLine(rectPaneClient.Left, rectPaneClient.Bottom, rectPaneClient.Left, rectTab.Bottom);
            path.AddLine(rectPaneClient.Left, rectTab.Bottom, rectTab.Right, rectTab.Bottom);
            return path;
        }

        GraphicsPath GetOutline_ToolWindow(int index)
        {
            Rectangle rectTab = GetTabRectangle(index);
            rectTab.Intersect(TabsRectangle);
            rectTab = RectangleToScreen(DrawHelper.RtlTransform(this, rectTab));
            Rectangle rectPaneClient = DockPane.RectangleToScreen(DockPane.ClientRectangle);

            GraphicsPath path = new GraphicsPath();
            GraphicsPath pathTab = GetTabOutline(Tabs[index], true, true);
            path.AddPath(pathTab, true);
            path.AddLine(rectTab.Left, rectTab.Top, rectPaneClient.Left, rectTab.Top);
            path.AddLine(rectPaneClient.Left, rectTab.Top, rectPaneClient.Left, rectPaneClient.Top);
            path.AddLine(rectPaneClient.Left, rectPaneClient.Top, rectPaneClient.Right, rectPaneClient.Top);
            path.AddLine(rectPaneClient.Right, rectPaneClient.Top, rectPaneClient.Right, rectTab.Top);
            path.AddLine(rectPaneClient.Right, rectTab.Top, rectTab.Right, rectTab.Top);
            return path;
        }

        void CalculateTabs()
        {
            if (Appearance == DockPane.AppearanceStyle.ToolWindow)
                CalculateTabs_ToolWindow();
            else
                CalculateTabs_Document();
        }

        void CalculateTabs_ToolWindow()
        {
            if (Tabs.Count <= 1 || DockPane.IsAutoHide)
                return;

            Rectangle rectTabStrip = TabStripRectangle;

            // Calculate tab widths
            int countTabs = Tabs.Count;
            foreach (TabVS2005 tab in Tabs)
            {
                tab.MaxWidth = GetMaxTabWidth(Tabs.IndexOf(tab));
                tab.Flag = false;
            }

            // Set tab whose max width less than average width
            int totalWidth = rectTabStrip.Width - ToolWindowStripGapLeft - ToolWindowStripGapRight;
            int totalAllocatedWidth = 0;
            int averageWidth = totalWidth / countTabs;
            int remainedTabs = countTabs;
            bool anyWidthWithinAverage;
            for (anyWidthWithinAverage=true; anyWidthWithinAverage && remainedTabs>0;)
            {
                anyWidthWithinAverage = false;
                foreach (TabVS2005 tab in Tabs)
                {
                    if (tab.Flag)
                        continue;

                    if (tab.MaxWidth <= averageWidth)
                    {
                        tab.Flag = true;
                        tab.TabWidth = tab.MaxWidth;
                        totalAllocatedWidth += tab.TabWidth;
                        anyWidthWithinAverage = true;
                        remainedTabs--;
                    }
                }
                if (remainedTabs != 0)
                    averageWidth = (totalWidth - totalAllocatedWidth) / remainedTabs;
            }

            // If any tab width not set yet, set it to the average width
            if (remainedTabs > 0)
            {
                int roundUpWidth = (totalWidth - totalAllocatedWidth) - (averageWidth * remainedTabs);
                foreach (TabVS2005 tab in Tabs)
                {
                    if (tab.Flag)
                        continue;

                    tab.Flag = true;
                    if (roundUpWidth > 0)
                    {
                        tab.TabWidth = averageWidth + 1;
                        roundUpWidth --;
                    }
                    else
                        tab.TabWidth = averageWidth;
                }
            }

            // Set the X position of the tabs
            int x = rectTabStrip.X + ToolWindowStripGapLeft;
            foreach (TabVS2005 tab in Tabs)
            {
                tab.TabX = x;
                x += tab.TabWidth;
            }
        }

        bool CalculateDocumentTab(Rectangle rectTabStrip, ref int x, int index)
        {
            bool overflow = false;

            TabVS2005 tab = Tabs[index] as TabVS2005;
            tab.MaxWidth = GetMaxTabWidth(index);
            int width = Math.Min(tab.MaxWidth, DocumentTabMaxWidth);
            if (x + width < rectTabStrip.Right || index == StartDisplayingTab)
            {
                tab.TabX = x;
                tab.TabWidth = width;
                EndDisplayingTab = index;
            }
            else
            {
                tab.TabX = 0;
                tab.TabWidth = 0;
                overflow = true;
            }
            x += width;

            return overflow;
        }

        /// <summary>
        /// Calculate which tabs are displayed and in what order.
        /// </summary>
        void CalculateTabs_Document()
        {
            if (m_startDisplayingTab >= Tabs.Count)
                m_startDisplayingTab = 0;

            int x = 0;
            Rectangle rectTabStrip = TabsRectangle;

            string tabStyle = PluginCore.PluginBase.MainForm.GetThemeValue("VS2005DockPaneStrip.TabStyle");

            // HACK
            if (tabStyle == "Rect") x = rectTabStrip.X;
            else x = rectTabStrip.X + rectTabStrip.Height / 2;

            bool overflow = false;

            // Originally all new documents that were considered overflow
            // (not enough pane strip space to show all tabs) were added to
            // the far left (assuming not right to left) and the tabs on the
            // right were dropped from view. If StartDisplayingTab is not 0
            // then we are dealing with making sure a specific tab is kept in focus.
            if (m_startDisplayingTab > 0)
            {
                int tempX = x;
                TabVS2005 tab = Tabs[m_startDisplayingTab] as TabVS2005;
                tab.MaxWidth = GetMaxTabWidth(m_startDisplayingTab);

                // Add the active tab and tabs to the left
                for (int i = StartDisplayingTab; i >= 0; i--)
                    CalculateDocumentTab(rectTabStrip, ref tempX, i);

                // Store which tab is the first one displayed so that it
                // will be drawn correctly (without part of the tab cut off)
                FirstDisplayingTab = EndDisplayingTab;

                tempX = x; // Reset X location because we are starting over

                // Start with the first tab displayed - name is a little misleading.
                // Loop through each tab and set its location. If there is not enough
                // room for all of them overflow will be returned.
                for (int i = EndDisplayingTab; i < Tabs.Count; i++)
                    overflow = CalculateDocumentTab(rectTabStrip, ref tempX, i);

                // If not all tabs are shown then we have an overflow.
                if (FirstDisplayingTab != 0)
                    overflow = true;
            }
            else
            {
                for (int i = StartDisplayingTab; i < Tabs.Count; i++)
                    overflow = CalculateDocumentTab(rectTabStrip, ref x, i);
                for (int i = 0; i < StartDisplayingTab; i++)
                    overflow = CalculateDocumentTab(rectTabStrip, ref x, i);

                FirstDisplayingTab = StartDisplayingTab;
            }

            if (!overflow)
            {
                m_startDisplayingTab = 0;
                FirstDisplayingTab = 0;

                // HACK
                if (tabStyle == "Rect") x = rectTabStrip.X;
                else x = rectTabStrip.X + rectTabStrip.Height / 2;

                foreach (TabVS2005 tab in Tabs)
                {
                    tab.TabX = x;
                    x += tab.TabWidth;
                }
            }
            DocumentTabsOverflow = overflow;
        }

        protected internal override void EnsureTabVisible(IDockContent content)
        {
            if (Appearance != DockPane.AppearanceStyle.Document || !Tabs.Contains(content))
                return;

            CalculateTabs();
            EnsureDocumentTabVisible(content, true);
        }

        bool EnsureDocumentTabVisible(IDockContent content, bool repaint)
        {
            int index = Tabs.IndexOf(content);
            TabVS2005 tab = Tabs[index] as TabVS2005;
            if (tab.TabWidth != 0)
                return false;

            StartDisplayingTab = index;
            if (repaint)
                Invalidate();

            return true;
        }

        int GetMaxTabWidth(int index)
        {
            if (Appearance == DockPane.AppearanceStyle.ToolWindow)
                return GetMaxTabWidth_ToolWindow(index);
            return GetMaxTabWidth_Document(index);
        }

        int GetMaxTabWidth_ToolWindow(int index)
        {
            IDockContent content = Tabs[index].Content;
            Size sizeString = TextRenderer.MeasureText(content.DockHandler.TabText, Font);
            return ToolWindowImageWidth + sizeString.Width + ToolWindowImageGapLeft
                + ToolWindowImageGapRight + ToolWindowTextGapRight;
        }

        int GetMaxTabWidth_Document(int index)
        {
            IDockContent content = Tabs[index].Content;

            int height = GetTabRectangle_Document(index).Height;

            Size sizeText = TextRenderer.MeasureText(content.DockHandler.TabText, BoldFont, new Size(DocumentTabMaxWidth, height), DocumentTextFormat);

            if (DockPane.DockPanel.ShowDocumentIcon)
                return sizeText.Width + DocumentIconWidth + DocumentIconGapLeft + DocumentIconGapRight + DocumentTextGapRight;
            return sizeText.Width + DocumentIconGapLeft + DocumentTextGapRight;
        }

        void DrawTabStrip(Graphics g)
        {
            if (Appearance == DockPane.AppearanceStyle.Document)
                DrawTabStrip_Document(g);
            else
                DrawTabStrip_ToolWindow(g);
        }

        void DrawTabStrip_Document(Graphics g)
        {
            int count = Tabs.Count;
            if (count == 0)
                return;

            Rectangle rectTabStrip = TabStripRectangle;

            // Draw the tabs
            Rectangle rectTabOnly = TabsRectangle;
            Rectangle rectTab = Rectangle.Empty;
            TabVS2005 tabActive = null;
            g.SetClip(DrawHelper.RtlTransform(this, rectTabOnly));
            for (int i=0; i<count; i++)
            {
                rectTab = GetTabRectangle(i);
                if (Tabs[i].Content == DockPane.ActiveContent)
                {
                    tabActive = Tabs[i] as TabVS2005;
                    continue;
                }
                if (rectTab.IntersectsWith(rectTabOnly))
                    DrawTab(g, Tabs[i] as TabVS2005, rectTab);
            }

            g.SetClip(rectTabStrip);
            g.DrawLine(PenDocumentTabActiveBorder, rectTabStrip.Left, rectTabStrip.Bottom - 1,
                rectTabStrip.Right, rectTabStrip.Bottom - 1);
            g.SetClip(DrawHelper.RtlTransform(this, rectTabOnly));
            if (tabActive != null)
            {
                rectTab = GetTabRectangle(Tabs.IndexOf(tabActive));
                if (rectTab.IntersectsWith(rectTabOnly))
                    DrawTab(g, tabActive, rectTab);
            }
        }

        void DrawTabStrip_ToolWindow(Graphics g)
        {
            Rectangle rectTabStrip = TabStripRectangle;

            g.DrawLine(PenToolWindowTabActiveBorder, rectTabStrip.Left, rectTabStrip.Top,
                rectTabStrip.Right, rectTabStrip.Top);

            for (int i=0; i<Tabs.Count; i++)
                DrawTab(g, Tabs[i] as TabVS2005, GetTabRectangle(i));
        }

        Rectangle GetTabRectangle(int index)
        {
            if (Appearance == DockPane.AppearanceStyle.ToolWindow)
                return GetTabRectangle_ToolWindow(index);
            return GetTabRectangle_Document(index);
        }

        Rectangle GetTabRectangle_ToolWindow(int index)
        {
            Rectangle rectTabStrip = TabStripRectangle;

            TabVS2005 tab = (TabVS2005)(Tabs[index]);
            return new Rectangle(tab.TabX, rectTabStrip.Y, tab.TabWidth, rectTabStrip.Height);
        }

        Rectangle GetTabRectangle_Document(int index)
        {
            Rectangle rectTabStrip = TabStripRectangle;
            TabVS2005 tab = (TabVS2005)Tabs[index];

            return new Rectangle(tab.TabX, rectTabStrip.Y + DocumentTabGapTop, tab.TabWidth, rectTabStrip.Height - DocumentTabGapTop);
        }

        void DrawTab(Graphics g, TabVS2005 tab, Rectangle rect)
        {
            if (Appearance == DockPane.AppearanceStyle.ToolWindow)
                DrawTab_ToolWindow(g, tab, rect);
            else
                DrawTab_Document(g, tab, rect);
        }

        GraphicsPath GetTabOutline(Tab tab, bool rtlTransform, bool toScreen)
        {
            if (Appearance == DockPane.AppearanceStyle.ToolWindow)
                return GetTabOutline_ToolWindow(tab, rtlTransform, toScreen);
            return GetTabOutline_Document(tab, rtlTransform, toScreen, false);
        }

        GraphicsPath GetTabOutline_ToolWindow(Tab tab, bool rtlTransform, bool toScreen)
        {
            Rectangle rect = GetTabRectangle(Tabs.IndexOf(tab));
            if (rtlTransform)
                rect = DrawHelper.RtlTransform(this, rect);
            if (toScreen)
                rect = RectangleToScreen(rect);

            DrawHelper.GetRoundedCornerTab(GraphicsPath, rect, false);
            return GraphicsPath;
        }

        GraphicsPath GetTabOutline_Document(Tab tab, bool rtlTransform, bool toScreen, bool full)
        {
            int curveSize = 6;

            string tabStyle = PluginCore.PluginBase.MainForm.GetThemeValue("VS2005DockPaneStrip.TabStyle");
            if (tabStyle == "Block")
            {
                curveSize = 1;
            }

            GraphicsPath.Reset();
            Rectangle rect = GetTabRectangle(Tabs.IndexOf(tab));
            if (rtlTransform)
                rect = DrawHelper.RtlTransform(this, rect);
            if (toScreen)
                rect = RectangleToScreen(rect);

            if (tabStyle == "Rect")
            {
                GraphicsPath.AddRectangle(rect);
                return GraphicsPath;
            }

            // Draws the full angle piece for active content (or first tab)
            if (tab.Content == DockPane.ActiveContent || full || Tabs.IndexOf(tab) == FirstDisplayingTab)
            {
                if (RightToLeft == RightToLeft.Yes)
                {
                    GraphicsPath.AddLine(rect.Right, rect.Bottom, rect.Right + rect.Height / 2, rect.Bottom);
                    GraphicsPath.AddLine(rect.Right + rect.Height / 2, rect.Bottom, rect.Right - rect.Height / 2 + curveSize / 2, rect.Top + curveSize / 2);
                }
                else
                {
                    GraphicsPath.AddLine(rect.Left, rect.Bottom, rect.Left - rect.Height / 2, rect.Bottom);
                    GraphicsPath.AddLine(rect.Left - rect.Height / 2, rect.Bottom, rect.Left + rect.Height / 2 - curveSize / 2, rect.Top + curveSize / 2);
                }
            }
            else
            {
                if (RightToLeft == RightToLeft.Yes)
                {
                    GraphicsPath.AddLine(rect.Right, rect.Bottom, rect.Right, rect.Bottom - rect.Height / 2);
                    GraphicsPath.AddLine(rect.Right, rect.Bottom - rect.Height / 2, rect.Right - rect.Height / 2 + curveSize / 2, rect.Top + curveSize / 2);
                }
                else
                {
                    GraphicsPath.AddLine(rect.Left, rect.Bottom, rect.Left, rect.Bottom - rect.Height / 2);
                    GraphicsPath.AddLine(rect.Left, rect.Bottom - rect.Height / 2, rect.Left + rect.Height / 2 - curveSize / 2, rect.Top + curveSize / 2);
                }
            }

            if (RightToLeft == RightToLeft.Yes)
            {
                GraphicsPath.AddLine(rect.Right - rect.Height / 2 - curveSize / 2, rect.Top, rect.Left + curveSize / 2, rect.Top);
                GraphicsPath.AddArc(new Rectangle(rect.Left, rect.Top, curveSize, curveSize), 180, 90);
            }
            else
            {
                GraphicsPath.AddLine(rect.Left + rect.Height / 2 + curveSize / 2, rect.Top, rect.Right - curveSize / 2, rect.Top);
                GraphicsPath.AddArc(new Rectangle(rect.Right - curveSize, rect.Top, curveSize, curveSize), -90, 90);
            }

            if (Tabs.IndexOf(tab) != EndDisplayingTab &&
                (Tabs.IndexOf(tab) != Tabs.Count - 1 && Tabs[Tabs.IndexOf(tab) + 1].Content == DockPane.ActiveContent)
                && !full)
            {
                if (RightToLeft == RightToLeft.Yes)
                {
                    GraphicsPath.AddLine(rect.Left, rect.Top + curveSize / 2, rect.Left, rect.Top + rect.Height / 2);
                    GraphicsPath.AddLine(rect.Left, rect.Top + rect.Height / 2, rect.Left + rect.Height / 2, rect.Bottom);
                }
                else
                {
                    GraphicsPath.AddLine(rect.Right, rect.Top + curveSize / 2, rect.Right, rect.Top + rect.Height / 2);
                    GraphicsPath.AddLine(rect.Right, rect.Top + rect.Height / 2, rect.Right - rect.Height / 2, rect.Bottom);
                }
            }
            else
            {
                if (RightToLeft == RightToLeft.Yes)
                    GraphicsPath.AddLine(rect.Left, rect.Top + curveSize / 2, rect.Left, rect.Bottom);
                else
                    GraphicsPath.AddLine(rect.Right, rect.Top + curveSize / 2, rect.Right, rect.Bottom);
            }

            return GraphicsPath;
        }

        void DrawTab_ToolWindow(Graphics g, TabVS2005 tab, Rectangle rect)
        {
            Rectangle rectIcon = new Rectangle(
                rect.X + ToolWindowImageGapLeft,
                rect.Y + rect.Height - 1 - ToolWindowImageGapBottom - ToolWindowImageHeight,
                ToolWindowImageWidth, ToolWindowImageHeight);
            Rectangle rectText = rectIcon;
            rectText.X += rectIcon.Width + ToolWindowImageGapRight;
            rectText.Width = rect.Width - rectIcon.Width - ToolWindowImageGapLeft - 
                ToolWindowImageGapRight - ToolWindowTextGapRight;

            Rectangle rectTab = DrawHelper.RtlTransform(this, rect);
            rectText = DrawHelper.RtlTransform(this, rectText);
            rectIcon = DrawHelper.RtlTransform(this, rectIcon);
            GraphicsPath path = GetTabOutline(tab, true, false);
            if (DockPane.ActiveContent == tab.Content)
            {
                g.FillPath(BrushToolWindowActiveBackground, path);
                g.DrawPath(PenToolWindowTabActiveBorder, path);

                // NICK: eliminate line between tab and content
                RectangleF r = path.GetBounds();
                Color color = PluginCore.PluginBase.MainForm.GetThemeColor("VS2005DockPaneStrip.ToolSeparatorColor");
                if (color == Color.Empty) color = Color.FromArgb(240, 239, 243);
                using (Pen pen = new Pen(color)) g.DrawLine(pen, r.Left + 1, r.Top, r.Right - 1, r.Top);

                TextRenderer.DrawText(g, tab.Content.DockHandler.TabText, Font, rectText, ColorToolWindowActiveText, ToolWindowTextFormat);
            }
            else
            {
                // NICK: remove separators, too busy for FD
                /*if (Tabs.IndexOf(DockPane.ActiveContent) != Tabs.IndexOf(tab) + 1)
                {
                    Point pt1 = new Point(rect.Right, rect.Top + ToolWindowTabSeperatorGapTop);
                    Point pt2 = new Point(rect.Right, rect.Bottom - ToolWindowTabSeperatorGapBottom); 
                    g.DrawLine(PenToolWindowTabBorder, DrawHelper.RtlTransform(this, pt1), DrawHelper.RtlTransform(this, pt2));
                }*/
                g.DrawPath(PenToolWindowTabInactiveBorder, path);
                TextRenderer.DrawText(g, tab.Content.DockHandler.TabText, Font, rectText, ColorToolWindowInactiveText, ToolWindowTextFormat);
            }

            if (rectTab.Contains(rectIcon))
                g.DrawIcon(tab.Content.DockHandler.Icon, rectIcon);
        }

        void DrawTab_Document(Graphics g, TabVS2005 tab, Rectangle rect)
        {
            if (tab.TabWidth == 0)
                return;

            Rectangle rectIcon = new Rectangle(
                rect.X + DocumentIconGapLeft,
                rect.Y + rect.Height - 1 - DocumentIconGapBottom - DocumentIconHeight,
                DocumentIconWidth, DocumentIconHeight);
            Rectangle rectText = rectIcon;

            string tabStyle = PluginCore.PluginBase.MainForm.GetThemeValue("VS2005DockPaneStrip.TabStyle");

            // Adjust text
            double scale = ScaleHelper.GetScale();
            if (scale >= 1.5)
            {
                string tabSize = PluginCore.PluginBase.MainForm.GetThemeValue("VS2005DockPaneStrip.TabSize");
                if (tabSize == "Default") rectText.Y += ScaleHelper.Scale(1);
            }
            else rectText.Y += ScaleHelper.Scale(2);
            if (Font.SizeInPoints <= 8F) rectText.Y -= ScaleHelper.Scale(1);

            if (DockPane.DockPanel.ShowDocumentIcon)
            {
                rectText.X += rectIcon.Width + DocumentIconGapRight;
                rectText.Y = rect.Y;
                rectText.Width = rect.Width - rectIcon.Width - DocumentIconGapLeft -
                    DocumentIconGapRight - DocumentTextGapRight;
                rectText.Height = rect.Height;
            }
            else
                rectText.Width = rect.Width - DocumentIconGapLeft - DocumentTextGapRight;

            Rectangle rectTab = DrawHelper.RtlTransform(this, rect);
            rectText = DrawHelper.RtlTransform(this, rectText);
            rectIcon = DrawHelper.RtlTransform(this, rectIcon);
            GraphicsPath path = GetTabOutline(tab, true, false);

            Color stripColor = tab.Content.DockHandler.TabColor;
            if (stripColor != Color.Transparent)
            {
                Color temp = PluginCore.PluginBase.MainForm.GetThemeColor("VS2005DockPaneStrip.TabStripColor");
                if (temp != Color.Empty) stripColor = temp;
            }

            string tabText = tab.MaxWidth >= rect.Width ? PathHelper.Ellipsis.Compact(tab.Content.DockHandler.TabText, BoldFont, rectText.Width,
                PathHelper.Ellipsis.EllipsisFormat.Path | PathHelper.Ellipsis.EllipsisFormat.Middle) : tab.Content.DockHandler.TabText;
            if (DockPane.ActiveContent == tab.Content)
            {
                if (tabStyle == "Rect") g.FillRectangle(BrushDocumentActiveBackground, rectTab);
                else g.FillPath(BrushDocumentActiveBackground, path);

                // Change by Mika: add color strip to tabs
                SolidBrush stripBrush = new SolidBrush(stripColor);
                Rectangle stripRect = rectTab;
                stripRect.X = stripRect.X + stripRect.Width - 2;
                stripRect.Height -= 1;
                stripRect.Width = 2;
                stripRect.Y += 1;
                g.FillRectangle(stripBrush, stripRect);

                if (tabStyle == "Rect") g.DrawRectangle(PenDocumentTabActiveBorder, rectTab);
                else g.DrawPath(PenDocumentTabActiveBorder, path);

                Color sepColor = PluginCore.PluginBase.MainForm.GetThemeColor("VS2005DockPaneStrip.TabSeparatorColor");

                // CHANGED to eliminate line between selected tab and content - NICK
                RectangleF r = path.GetBounds();
                float right = stripColor == Color.Transparent ? r.Right - 1 : r.Right - 3;

                // Choose color
                if (sepColor == Color.Empty)
                {
                    if (PluginCore.PluginBase.Settings.UseSystemColors)
                    {
                        sepColor = SystemColors.ControlLight;
                    }
                    else sepColor = Color.FromArgb(240, 239, 243);
                }
                
                // Draw separator
                using (Pen pen = new Pen(sepColor))
                {
                    if (tabStyle == "Rect") g.DrawLine(pen, rectTab.Left + 2, rectTab.Bottom - 1, right, rectTab.Bottom - 1);
                    else g.DrawLine(pen, r.Left + 2, r.Bottom - 1, right, r.Bottom - 1);
                }

                if (DockPane.IsActiveDocumentPane)
                {
                    TextRenderer.DrawText(g, tabText, BoldFont, rectText, ColorDocumentActiveText, DocumentTextFormat);
                }
                else TextRenderer.DrawText(g, tabText, Font, rectText, ColorDocumentInactiveText, DocumentTextFormat);
            }
            else
            {
                // CHANGE by NICK: emulate VS-style inactive tab gradient
                Brush tabBrush = new LinearGradientBrush(rectTab, SystemColors.ControlLightLight, SystemColors.ControlLight, LinearGradientMode.Vertical);

                // Choose color
                Color color = PluginCore.PluginBase.MainForm.GetThemeColor("VS2005DockPaneStrip.DocInactiveBackColor");
                if (color != Color.Empty)
                {
                    tabBrush = new SolidBrush(color);
                }

                //g.FillPath(BrushDocumentInactiveBackground, path);
                if (tabStyle == "Rect") g.FillRectangle(tabBrush, rectTab);
                else g.FillPath(tabBrush, path);

                // Change by Mika: add color strip to tabs
                SolidBrush stripBrush = new SolidBrush(stripColor);
                Rectangle stripRect = rectTab;
                stripRect.X = stripRect.X + stripRect.Width - 2;
                stripRect.Height -= 2;
                stripRect.Width = 2;
                stripRect.Y += 1;
                g.FillRectangle(stripBrush, stripRect);

                if (tabStyle == "Rect") g.DrawRectangle(PenDocumentTabInactiveBorder, rectTab);
                else g.DrawPath(PenDocumentTabInactiveBorder, path);

                TextRenderer.DrawText(g, tabText, Font, rectText, ColorDocumentInactiveText, DocumentTextFormat);
            }

            if (rectTab.Contains(rectIcon) && DockPane.DockPanel.ShowDocumentIcon)
                g.DrawIcon(tab.Content.DockHandler.Icon, rectIcon);
        }

        Rectangle TabStripRectangle
        {
            get
            {
                if (Appearance == DockPane.AppearanceStyle.Document)
                    return TabStripRectangle_Document;
                return TabStripRectangle_ToolWindow;
            }
        }

        Rectangle TabStripRectangle_ToolWindow
        {
            get
            {
                Rectangle rect = ClientRectangle;
                return new Rectangle(rect.X, rect.Top + ToolWindowStripGapTop, rect.Width, rect.Height - ToolWindowStripGapTop - ToolWindowStripGapBottom);
            }
        }

        Rectangle TabStripRectangle_Document
        {
            get
            {
                Rectangle rect = ClientRectangle;
                // NICK: don't use ToolStripGap here!!
                return new Rectangle(rect.X, rect.Top + DocumentStripGapTop, rect.Width, rect.Height - DocumentStripGapTop);// - ToolWindowStripGapBottom);
            }
        }

        Rectangle TabsRectangle
        {
            get 
            {
                if (Appearance == DockPane.AppearanceStyle.ToolWindow)
                    return TabStripRectangle;

                Rectangle rectWindow = TabStripRectangle;
                int x = rectWindow.X;
                int y = rectWindow.Y;
                int width = rectWindow.Width;
                int height = rectWindow.Height;

                x += DocumentTabGapLeft;
                width -= DocumentTabGapLeft + 
                        DocumentTabGapRight +
                        DocumentButtonGapRight +
                        ButtonClose.Width +
                        ButtonWindowList.Width +
                        2 * DocumentButtonGapBetween;

                return new Rectangle(x, y, width, height);
            }
        }

        readonly ContextMenuStrip m_selectMenu;
        ContextMenuStrip SelectMenu => m_selectMenu;

        void WindowList_Click(object sender, EventArgs e)
        {
            int x = 0;
            int y = ButtonWindowList.Location.Y + ButtonWindowList.Height;

            List<Tab> tabs = new List<Tab>(Tabs);
            tabs.Sort((a, b) => string.CompareOrdinal(a.Content.DockHandler.TabText, b.Content.DockHandler.TabText));
            
            SelectMenu.Items.Clear();
            foreach (TabVS2005 tab in tabs)
            {
                IDockContent content = tab.Content;
                ToolStripItem item = SelectMenu.Items.Add(content.DockHandler.TabText, content.DockHandler.Icon.ToBitmap());
                item.Tag = tab.Content;
                item.MouseUp += ContextMenuItem_Up;
            }
            SelectMenu.Show(ButtonWindowList, x, y);
        }

        static readonly Action<ToolStrip, int> ScrollInternal
            = (Action<ToolStrip, int>)Delegate.CreateDelegate(typeof(Action<ToolStrip, int>),
                typeof(ToolStrip).GetMethod("ScrollInternal",
                    System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance));

        static readonly Action<ToolStripDropDownMenu> UpdateScrollButtonStatus
            = (Action<ToolStripDropDownMenu>)Delegate.CreateDelegate(typeof(Action<ToolStripDropDownMenu>),
                typeof(ToolStripDropDownMenu).GetMethod("UpdateScrollButtonStatus",
                    System.Reflection.BindingFlags.NonPublic
                    | System.Reflection.BindingFlags.Instance));

        void ContextMenu_MouseWheel(object sender, MouseEventArgs e)
        {
            /* Default size, can it be changed? if so we can get it with:
            ((ToolStripItem)typeof(ToolStripDropDownMenu).GetProperty ("DownScrollButton", 
                 System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance ).GetValue(ts, null)).
                 GetPreferredSize(Size.Empty).Height
            */
            const int scrollButtonHeight = 9;

            var ts = (ContextMenuStrip)sender;
            int delta = e.Delta;
            if (ts.Items.Count == 0)
                return;
            var firstItem = ts.Items[0];
            var lastItem = ts.Items[ts.Items.Count - 1];
            if (lastItem.Bounds.Bottom < ts.Height && firstItem.Bounds.Top > 0)
                return;
            delta = delta switch
            {
                < 0 when firstItem.Bounds.Top - delta > scrollButtonHeight => firstItem.Bounds.Top - scrollButtonHeight,
                > 0 when delta > lastItem.Bounds.Bottom - ts.Height + scrollButtonHeight * 2 => lastItem.Bounds.Bottom -
                    ts.Height + scrollButtonHeight * 2,
                _ => -4
            };
            if (delta != 0)
            {
                ScrollInternal(ts, delta);
                UpdateScrollButtonStatus(ts);
            }
        }

        void ContextMenuItem_Up(object sender, MouseEventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item != null)
            {
                IDockContent content = (IDockContent)item.Tag;
                if (e.Button == MouseButtons.Middle) content.DockHandler.Close();
                else DockPane.ActiveContent = content;
                SelectMenu.Hide();
            }
        }

        void SetInertButtons()
        {
            if (Appearance == DockPane.AppearanceStyle.ToolWindow)
            {
                if (m_buttonClose != null)
                    m_buttonClose.Left = -m_buttonClose.Width;

                if (m_buttonWindowList != null)
                    m_buttonWindowList.Left = -m_buttonWindowList.Width;
            }
            else
            {
                bool showCloseButton = DockPane.ActiveContent?.DockHandler.CloseButton ?? true;
                ButtonClose.Enabled = showCloseButton;
                ButtonClose.RefreshChanges();
                ButtonWindowList.RefreshChanges();
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            if (Appearance != DockPane.AppearanceStyle.Document)
            {
                base.OnLayout(levent);
                return;
            }

            Rectangle rectTabStrip = TabStripRectangle;

            // Set position and size of the buttons
            int buttonWidth = ButtonClose.Image.Width;
            int buttonHeight = ButtonClose.Image.Height;
            int height = rectTabStrip.Height - DocumentButtonGapTop - DocumentButtonGapBottom;
            if (buttonHeight < height)
            {
                buttonWidth *= (height / buttonHeight);
                buttonHeight = height;
            }
            Size buttonSize = new Size(buttonWidth, buttonHeight);

            int x = rectTabStrip.X + rectTabStrip.Width - DocumentTabGapLeft
                - DocumentButtonGapRight - buttonWidth;
            int y = rectTabStrip.Y + DocumentButtonGapTop;

            // HACK - Mika
            Point point = new Point(x - 1, y);
            ButtonClose.Bounds = DrawHelper.RtlTransform(this, new Rectangle(point, buttonSize));
            point.Offset(-(DocumentButtonGapBetween + buttonWidth), 0);
            ButtonWindowList.Bounds = DrawHelper.RtlTransform(this, new Rectangle(point, buttonSize));

            OnRefreshChanges();

            base.OnLayout (levent);
        }

        void Close_Click(object sender, EventArgs e)
        {
            DockPane.CloseActiveContent();
        }

        protected internal override int HitTest(Point ptMouse)
        {
            if (!TabsRectangle.Contains(ptMouse))
                return -1;

            foreach (Tab tab in Tabs)
            {
                GraphicsPath path = GetTabOutline(tab, true, false);
                if (path.IsVisible(ptMouse))
                    return Tabs.IndexOf(tab);
            }
            return -1;
        }

        protected override void OnMouseHover(EventArgs e)
        {
            int index = HitTest(PointToClient(MousePosition));
            string toolTip = string.Empty;

            base.OnMouseHover(e);

            if (index != -1)
            {
                TabVS2005 tab = Tabs[index] as TabVS2005;
                if (!string.IsNullOrEmpty(tab.Content.DockHandler.ToolTipText))
                    toolTip = tab.Content.DockHandler.ToolTipText;
                else if (tab.MaxWidth > tab.TabWidth)
                    toolTip = tab.Content.DockHandler.TabText;
            }

            if (m_toolTip.GetToolTip(this) != toolTip)
            {
                m_toolTip.Active = false;
                m_toolTip.SetToolTip(this, toolTip);
                m_toolTip.Active = true;
            }

            // requires further tracking of mouse hover behavior,
            ResetMouseEventArgs();
        }

        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            PerformLayout();
        }

        // HACK: middle click document closing
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == (int)Win32.Msgs.WM_MBUTTONDOWN)
            {
                int index = HitTest();
                if (index != -1)
                {
                    if (Tabs[index].Content.DockHandler.Content.GetType().ToString() == "FlashDevelop.Docking.TabbedDocument")
                    {
                        Tabs[index].Content.DockHandler.Close();
                        return;
                    }
                }
            }
            base.WndProc(ref m);
        }
    }
}
