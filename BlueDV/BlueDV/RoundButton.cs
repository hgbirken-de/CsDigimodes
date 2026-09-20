using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BlueDV
{
	// Token: 0x02000048 RID: 72
	public class RoundButton : Button
	{
		// Token: 0x170000AA RID: 170
		// (get) Token: 0x06000561 RID: 1377 RVA: 0x0002E7A1 File Offset: 0x0002C9A1
		// (set) Token: 0x06000562 RID: 1378 RVA: 0x0002E7A9 File Offset: 0x0002C9A9
		[Category("Appearance")]
		[Description("The color to use for the top portion of the gradient fill of the component.")]
		[DefaultValue(typeof(Color), "0x2C55B1")]
		public Color GradientTop
		{
			get
			{
				return this.gradientTop;
			}
			set
			{
				this.gradientTop = value;
				this.SetPaintColors();
				base.Invalidate();
			}
		}

		// Token: 0x170000AB RID: 171
		// (get) Token: 0x06000563 RID: 1379 RVA: 0x0002E7BE File Offset: 0x0002C9BE
		// (set) Token: 0x06000564 RID: 1380 RVA: 0x0002E7C6 File Offset: 0x0002C9C6
		[Category("Appearance")]
		[Description("The color to use for the bottom portion of the gradient fill of the component.")]
		[DefaultValue(typeof(Color), "0x99C6F1")]
		public Color GradientBottom
		{
			get
			{
				return this.gradientBottom;
			}
			set
			{
				this.gradientBottom = value;
				this.SetPaintColors();
				base.Invalidate();
			}
		}

		// Token: 0x170000AC RID: 172
		// (get) Token: 0x06000565 RID: 1381 RVA: 0x0002E7DB File Offset: 0x0002C9DB
		// (set) Token: 0x06000566 RID: 1382 RVA: 0x0002E7E3 File Offset: 0x0002C9E3
		public override Color ForeColor
		{
			get
			{
				return base.ForeColor;
			}
			set
			{
				base.ForeColor = value;
				this.SetPaintColors();
				base.Invalidate();
			}
		}

		// Token: 0x06000567 RID: 1383 RVA: 0x0002E7F8 File Offset: 0x0002C9F8
		protected override void OnCreateControl()
		{
			base.SuspendLayout();
			this.SetControlSizes();
			this.SetPaintColors();
			this.InitializeTimers();
			base.OnCreateControl();
			base.ResumeLayout();
		}

		// Token: 0x06000568 RID: 1384 RVA: 0x0002E81E File Offset: 0x0002CA1E
		protected override void OnResize(EventArgs e)
		{
			this.SetControlSizes();
			base.Invalidate();
			base.OnResize(e);
		}

		// Token: 0x06000569 RID: 1385 RVA: 0x0002E834 File Offset: 0x0002CA34
		private void SetControlSizes()
		{
			int num = Math.Min(base.ClientRectangle.Width, base.ClientRectangle.Height);
			this.buttonRect = new Rectangle(base.ClientRectangle.X, base.ClientRectangle.Y, base.ClientRectangle.Width - 1, base.ClientRectangle.Height - 1);
			this.rectCornerRadius = Math.Max(1, num / 10);
			this.rectOutlineWidth = (float)Math.Max(1, num / 50);
			this.highlightRect = new Rectangle(base.ClientRectangle.X, base.ClientRectangle.Y, base.ClientRectangle.Width - 1, (base.ClientRectangle.Height - 1) / 2);
			this.highlightRectOffset = Math.Max(1, num / 35);
			this.defaultHighlightOffset = Math.Max(1, num / 35);
		}

		// Token: 0x0600056A RID: 1386 RVA: 0x0002E934 File Offset: 0x0002CB34
		protected override void OnEnabledChanged(EventArgs e)
		{
			if (!base.Enabled)
			{
				this.animateButtonHighlightedTimer.Stop();
				this.animateResumeNormalTimer.Stop();
			}
			this.SetPaintColors();
			base.Invalidate();
			base.OnEnabledChanged(e);
		}

		// Token: 0x0600056B RID: 1387 RVA: 0x0002E968 File Offset: 0x0002CB68
		private void SetPaintColors()
		{
			if (base.Enabled)
			{
				if (SystemInformation.HighContrast)
				{
					this.paintGradientTop = Color.Black;
					this.paintGradientBottom = Color.Black;
					this.paintForeColor = Color.White;
					return;
				}
				this.paintGradientTop = this.gradientTop;
				this.paintGradientBottom = this.gradientBottom;
				this.paintForeColor = this.ForeColor;
				return;
			}
			else
			{
				if (SystemInformation.HighContrast)
				{
					this.paintGradientTop = Color.Gray;
					this.paintGradientBottom = Color.White;
					this.paintForeColor = Color.Black;
					return;
				}
				int num = (int)(this.gradientTop.GetBrightness() * 255f);
				this.paintGradientTop = Color.FromArgb(num, num, num);
				int num2 = (int)(this.gradientBottom.GetBrightness() * 255f);
				this.paintGradientBottom = Color.FromArgb(num2, num2, num2);
				int num3 = (int)(this.ForeColor.GetBrightness() * 255f);
				if (num3 > 127)
				{
					num3 -= 60;
				}
				else
				{
					num3 += 60;
				}
				this.paintForeColor = Color.FromArgb(num3, num3, num3);
				return;
			}
		}

		// Token: 0x0600056C RID: 1388 RVA: 0x0002EA6C File Offset: 0x0002CC6C
		private void InitializeTimers()
		{
			this.animateButtonHighlightedTimer.Interval = 20;
			this.animateButtonHighlightedTimer.Tick += this.animateButtonHighlightedTimer_Tick;
			this.animateResumeNormalTimer.Interval = 5;
			this.animateResumeNormalTimer.Tick += this.animateResumeNormalTimer_Tick;
		}

		// Token: 0x0600056D RID: 1389 RVA: 0x0002EAC0 File Offset: 0x0002CCC0
		protected override void OnPaint(PaintEventArgs pevent)
		{
			Graphics graphics = pevent.Graphics;
			ButtonRenderer.DrawParentBackground(graphics, base.ClientRectangle, this);
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			using (GraphicsPath graphicsPath = RoundButton.RoundedRectangle(this.buttonRect, this.rectCornerRadius, 0))
			{
				using (LinearGradientBrush linearGradientBrush = new LinearGradientBrush(this.buttonRect, this.paintGradientTop, this.paintGradientBottom, LinearGradientMode.Vertical))
				{
					graphics.FillPath(linearGradientBrush, graphicsPath);
				}
				using (Pen pen = new Pen(this.paintGradientTop, this.rectOutlineWidth))
				{
					pen.Alignment = PenAlignment.Inset;
					graphics.DrawPath(pen, graphicsPath);
				}
			}
			if (base.IsDefault)
			{
				using (GraphicsPath graphicsPath2 = new GraphicsPath())
				{
					graphicsPath2.AddPath(RoundButton.RoundedRectangle(this.buttonRect, this.rectCornerRadius, 0), false);
					graphicsPath2.AddPath(RoundButton.RoundedRectangle(this.buttonRect, this.rectCornerRadius, this.defaultHighlightOffset), false);
					using (PathGradientBrush pathGradientBrush = new PathGradientBrush(graphicsPath2))
					{
						pathGradientBrush.CenterColor = Color.FromArgb(50, Color.White);
						pathGradientBrush.SurroundColors = new Color[] { Color.FromArgb(100, Color.White) };
						graphics.FillPath(pathGradientBrush, graphicsPath2);
					}
				}
			}
			using (GraphicsPath graphicsPath3 = RoundButton.RoundedRectangle(this.highlightRect, this.rectCornerRadius, this.highlightRectOffset))
			{
				using (LinearGradientBrush linearGradientBrush2 = new LinearGradientBrush(this.highlightRect, Color.FromArgb(this.highlightAlphaTop, Color.White), Color.FromArgb(this.highlightAlphaBottom, Color.White), LinearGradientMode.Vertical))
				{
					graphics.FillPath(linearGradientBrush2, graphicsPath3);
				}
			}
			TextRenderer.DrawText(graphics, this.Text, this.Font, this.buttonRect, this.paintForeColor, Color.Transparent, TextFormatFlags.EndEllipsis | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
		}

		// Token: 0x0600056E RID: 1390 RVA: 0x0002ECF8 File Offset: 0x0002CEF8
		private static GraphicsPath RoundedRectangle(Rectangle boundingRect, int cornerRadius, int margin)
		{
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddArc(boundingRect.X + margin, boundingRect.Y + margin, cornerRadius * 2, cornerRadius * 2, 180f, 90f);
			graphicsPath.AddArc(boundingRect.X + boundingRect.Width - margin - cornerRadius * 2, boundingRect.Y + margin, cornerRadius * 2, cornerRadius * 2, 270f, 90f);
			graphicsPath.AddArc(boundingRect.X + boundingRect.Width - margin - cornerRadius * 2, boundingRect.Y + boundingRect.Height - margin - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 0f, 90f);
			graphicsPath.AddArc(boundingRect.X + margin, boundingRect.Y + boundingRect.Height - margin - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 90f, 90f);
			graphicsPath.AddLine(boundingRect.X + margin, boundingRect.Y + boundingRect.Height - margin - cornerRadius * 2, boundingRect.X + margin, boundingRect.Y + margin + cornerRadius);
			graphicsPath.CloseFigure();
			return graphicsPath;
		}

		// Token: 0x0600056F RID: 1391 RVA: 0x0002EE18 File Offset: 0x0002D018
		protected override void OnMouseEnter(EventArgs e)
		{
			this.HighlightButton();
			base.OnMouseEnter(e);
		}

		// Token: 0x06000570 RID: 1392 RVA: 0x0002EE27 File Offset: 0x0002D027
		protected override void OnGotFocus(EventArgs e)
		{
			this.HighlightButton();
			base.OnGotFocus(e);
		}

		// Token: 0x06000571 RID: 1393 RVA: 0x0002EE36 File Offset: 0x0002D036
		private void HighlightButton()
		{
			if (base.Enabled)
			{
				this.animateResumeNormalTimer.Stop();
				this.animateButtonHighlightedTimer.Start();
			}
		}

		// Token: 0x06000572 RID: 1394 RVA: 0x0002EE58 File Offset: 0x0002D058
		private void animateButtonHighlightedTimer_Tick(object sender, EventArgs e)
		{
			if (this.increasingAlpha)
			{
				if (100 <= this.highlightAlphaBottom)
				{
					this.increasingAlpha = false;
				}
				else
				{
					this.highlightAlphaBottom += 5;
				}
			}
			else if (0 >= this.highlightAlphaBottom)
			{
				this.increasingAlpha = true;
			}
			else
			{
				this.highlightAlphaBottom -= 5;
			}
			base.Invalidate();
		}

		// Token: 0x06000573 RID: 1395 RVA: 0x0002EEB6 File Offset: 0x0002D0B6
		protected override void OnMouseLeave(EventArgs e)
		{
			this.ResumeNormalButton();
			base.OnMouseLeave(e);
		}

		// Token: 0x06000574 RID: 1396 RVA: 0x0002EEC5 File Offset: 0x0002D0C5
		protected override void OnLostFocus(EventArgs e)
		{
			this.ResumeNormalButton();
			base.OnLostFocus(e);
		}

		// Token: 0x06000575 RID: 1397 RVA: 0x0002EED4 File Offset: 0x0002D0D4
		private void ResumeNormalButton()
		{
			if (base.Enabled)
			{
				this.animateButtonHighlightedTimer.Stop();
				this.animateResumeNormalTimer.Start();
			}
		}

		// Token: 0x06000576 RID: 1398 RVA: 0x0002EEF4 File Offset: 0x0002D0F4
		private void animateResumeNormalTimer_Tick(object sender, EventArgs e)
		{
			bool flag = false;
			if (this.highlightAlphaBottom > 0)
			{
				this.highlightAlphaBottom -= 5;
				flag = true;
			}
			if (this.highlightAlphaTop < 255)
			{
				this.highlightAlphaTop += 5;
				flag = true;
			}
			if (!flag)
			{
				this.animateResumeNormalTimer.Stop();
			}
			base.Invalidate();
		}

		// Token: 0x06000577 RID: 1399 RVA: 0x0002EF4D File Offset: 0x0002D14D
		protected override void OnMouseDown(MouseEventArgs mevent)
		{
			this.PressButton();
			base.OnMouseDown(mevent);
		}

		// Token: 0x06000578 RID: 1400 RVA: 0x0002EF5C File Offset: 0x0002D15C
		protected override void OnKeyDown(KeyEventArgs kevent)
		{
			if (kevent.KeyCode == Keys.Space || kevent.KeyCode == Keys.Return)
			{
				this.PressButton();
			}
			base.OnKeyDown(kevent);
		}

		// Token: 0x06000579 RID: 1401 RVA: 0x0002EF80 File Offset: 0x0002D180
		private void PressButton()
		{
			if (base.Enabled)
			{
				this.animateButtonHighlightedTimer.Stop();
				this.animateResumeNormalTimer.Stop();
				this.highlightRect.Location = new Point(0, base.ClientRectangle.Height / 2);
				this.highlightAlphaTop = 0;
				this.highlightAlphaBottom = 200;
				base.Invalidate();
			}
		}

		// Token: 0x0600057A RID: 1402 RVA: 0x0002EFE4 File Offset: 0x0002D1E4
		protected override void OnMouseUp(MouseEventArgs mevent)
		{
			this.ReleaseButton();
			if (this.DisplayRectangle.Contains(mevent.Location))
			{
				this.HighlightButton();
			}
			base.OnMouseUp(mevent);
		}

		// Token: 0x0600057B RID: 1403 RVA: 0x0002F01A File Offset: 0x0002D21A
		protected override void OnKeyUp(KeyEventArgs kevent)
		{
			if (kevent.KeyCode == Keys.Space || kevent.KeyCode == Keys.Return)
			{
				this.ReleaseButton();
				if (base.IsDefault)
				{
					this.HighlightButton();
				}
			}
			base.OnKeyUp(kevent);
		}

		// Token: 0x0600057C RID: 1404 RVA: 0x0002F04C File Offset: 0x0002D24C
		protected override void OnMouseMove(MouseEventArgs mevent)
		{
			if (base.Enabled && (mevent.Button & MouseButtons.Left) == MouseButtons.Left && !base.ClientRectangle.Contains(mevent.Location))
			{
				this.ReleaseButton();
			}
			base.OnMouseMove(mevent);
		}

		// Token: 0x0600057D RID: 1405 RVA: 0x0002F097 File Offset: 0x0002D297
		private void ReleaseButton()
		{
			if (base.Enabled)
			{
				this.highlightRect.Location = new Point(0, 0);
				this.highlightAlphaTop = 255;
				this.highlightAlphaBottom = 0;
			}
		}

		// Token: 0x0400037B RID: 891
		private Color gradientTop = Color.FromArgb(255, 44, 85, 177);

		// Token: 0x0400037C RID: 892
		private Color gradientBottom = Color.FromArgb(255, 153, 198, 241);

		// Token: 0x0400037D RID: 893
		private Color paintGradientTop;

		// Token: 0x0400037E RID: 894
		private Color paintGradientBottom;

		// Token: 0x0400037F RID: 895
		private Color paintForeColor;

		// Token: 0x04000380 RID: 896
		private Rectangle buttonRect;

		// Token: 0x04000381 RID: 897
		private Rectangle highlightRect;

		// Token: 0x04000382 RID: 898
		private int rectCornerRadius;

		// Token: 0x04000383 RID: 899
		private float rectOutlineWidth;

		// Token: 0x04000384 RID: 900
		private int highlightRectOffset;

		// Token: 0x04000385 RID: 901
		private int defaultHighlightOffset;

		// Token: 0x04000386 RID: 902
		private int highlightAlphaTop = 255;

		// Token: 0x04000387 RID: 903
		private int highlightAlphaBottom;

		// Token: 0x04000388 RID: 904
		private Timer animateButtonHighlightedTimer = new Timer();

		// Token: 0x04000389 RID: 905
		private Timer animateResumeNormalTimer = new Timer();

		// Token: 0x0400038A RID: 906
		private bool increasingAlpha;
	}
}
