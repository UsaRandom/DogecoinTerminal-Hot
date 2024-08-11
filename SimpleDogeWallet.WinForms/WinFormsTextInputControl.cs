using Microsoft.Xna.Framework;
using SharpDX.Direct2D1;
using SimpleDogeWallet.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace SimpleDogeWallet.WinForms
{
	public class WinFormsTextInputControl : TextInputControl
	{
		private bool _selected = false;
		private bool _isPassword = false;

		private TextBox _textBox;

		

		public WinFormsTextInputControl(XElement element)
			: base(element)
		{
			PlaceholderTextStringDef = element.Attribute(nameof(PlaceholderTextStringDef))?.Value;

			var isPassText = element.Attribute("Password")?.Value ?? "false";

			_isPassword = isPassText.ToLower() == "true";
        }

		private string PlaceholderTextStringDef { get; set; }

		public TextBox TextBoxControl
		{
			get
			{
				if(_textBox == null)
				{
					_textBox = new TextBox();
					_textBox.Font = new Font("Comic Sans MS", 4.5f * TextSize, FontStyle.Bold, GraphicsUnit.Point);
					((TextBoxBase)_textBox).AutoSize = false;			
				}
				return _textBox;
			}
		}

		public override string Text
		{
			get
			{
				return TextBoxControl.Text;
			}
			set
			{
				if(TextBoxControl.InvokeRequired)
				{
					TextBoxControl.Invoke(new MethodInvoker(delegate
					{
						TextBoxControl.Text = value;
					}));
				}
				else
                {
                    TextBoxControl.Text = value;
                }
			}
		}



		public override void Draw(GameTime time, IServiceProvider services)
		{

		}

		private int lastHeight = -1;

		public override void Update(GameTime time, IServiceProvider services)
		{
			var virtualScreen = services.GetService<VirtualScreen>();
			var screenCordStart = virtualScreen.VirtualCoordToWindowCoord(StartPosition);
			var screenCordEnd = virtualScreen.VirtualCoordToWindowCoord(EndPosition);


			if(TextBoxControl.PlaceholderText == string.Empty && !string.IsNullOrWhiteSpace(PlaceholderTextStringDef))
			{
				TextBoxControl.PlaceholderText = services.GetService<Strings>().GetString(PlaceholderTextStringDef);
			}

			TextBoxControl.Width = Math.Abs(screenCordEnd.X - screenCordStart.X);
			TextBoxControl.Height = Math.Abs(screenCordEnd.Y - screenCordStart.Y);
			
			TextBoxControl.Location = new System.Drawing.Point(Math.Min(screenCordStart.X, screenCordEnd.X), Math.Min(screenCordStart.Y, screenCordEnd.Y));

			if(lastHeight != TextBoxControl.Height)
			{
				TextBoxControl.Font = new Font("Comic Sans MS", TextBoxControl.Height/2 - 2, FontStyle.Bold, GraphicsUnit.Pixel);
			}

			lastHeight = TextBoxControl.Height;
		}



		public override void OnControlHidden(IServiceProvider services)
		{
			var form = services.GetService<Form>();
			if (form.InvokeRequired)
			{
				form.Invoke((MethodInvoker)delegate
				{
					form.Controls.Remove(TextBoxControl);
				});
			}
			else
			{
				form.Controls.Remove(TextBoxControl);
			}
		}

		public override void OnControlShown(IServiceProvider services)
		{
			var form = services.GetService<Form>();

			var add = (MethodInvoker)delegate
			{

				var virtualScreen = services.GetService<VirtualScreen>();
				var screenCordStart = virtualScreen.VirtualCoordToWindowCoord(StartPosition);
				var screenCordEnd = virtualScreen.VirtualCoordToWindowCoord(EndPosition);


				if (TextBoxControl.PlaceholderText == string.Empty && !string.IsNullOrWhiteSpace(PlaceholderTextStringDef))
				{
					TextBoxControl.PlaceholderText = services.GetService<Strings>().GetString(PlaceholderTextStringDef);
				}

				TextBoxControl.ForeColor = System.Drawing.Color.FromArgb(ForegroundColor.Color.A,
																		ForegroundColor.Color.R,
																		ForegroundColor.Color.G,
																		ForegroundColor.Color.B);
				TextBoxControl.BackColor = System.Drawing.Color.FromArgb(BackgroundColor.Color.A,
																		BackgroundColor.Color.R,
																		BackgroundColor.Color.G,
																		BackgroundColor.Color.B);

                if (_isPassword)
                {
                    TextBoxControl.PasswordChar = '●';
                }

                TextBoxControl.ReadOnly = !Editable;
				TextBoxControl.Width = Math.Abs(screenCordEnd.X - screenCordStart.X);
				TextBoxControl.Height = Math.Abs(screenCordEnd.Y - screenCordStart.Y);

				TextBoxControl.Location = new System.Drawing.Point(Math.Min(screenCordStart.X, screenCordEnd.X), Math.Min(screenCordStart.Y, screenCordEnd.Y));

				if (lastHeight != TextBoxControl.Height)
				{
					TextBoxControl.Font = new Font("Comic Sans MS", TextBoxControl.Height / 2 - 2, FontStyle.Bold, GraphicsUnit.Pixel);
				}

				lastHeight = TextBoxControl.Height;

				form.Controls.Add(TextBoxControl);
			};

			if (form.InvokeRequired)
			{
				form.Invoke(add);
			}
			else
			{
				add();
			}
		}
	}
}
