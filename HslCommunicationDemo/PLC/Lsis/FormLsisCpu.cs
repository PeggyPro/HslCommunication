﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HslCommunication.Profinet;
using HslCommunication;
using HslCommunication.Profinet.LSIS;
using System.Threading;
using System.IO.Ports;
using System.Xml.Linq;
using HslCommunicationDemo.PLC.Common;
using HslCommunicationDemo.DemoControl;

namespace HslCommunicationDemo
{
	public partial class FormLsisCpu : HslFormContent
	{
		public FormLsisCpu( )
		{
			InitializeComponent( );
		}


		private LSCpu xGBCpu = null;
		private AddressExampleControl addressExampleControl;
		private CodeExampleControl codeExampleControl;

		private void FormSiemens_Load( object sender, EventArgs e )
		{
			Language( Program.Language );

			addressExampleControl = new AddressExampleControl( );
			addressExampleControl.SetAddressExample( HslCommunicationDemo.PLC.Lsis.Helper.GetLsisCnetAddress( ) );
			userControlReadWriteDevice1.AddSpecialFunctionTab( addressExampleControl, false, DeviceAddressExample.GetTitle( ) );

			codeExampleControl = new CodeExampleControl( );
			userControlReadWriteDevice1.AddSpecialFunctionTab( codeExampleControl, false, CodeExampleControl.GetTitle( ) );
			userControlReadWriteDevice1.SetEnable( false );
		}


		private void Language( int language )
		{
			if (language == 2)
			{
				Text = "Lsis Cpu Read Demo";
				label21.Text = "station";
				button1.Text = "Connect";
				button2.Text = "Disconnect";
			}
		}

		private void FormSiemens_FormClosing( object sender, FormClosingEventArgs e )
		{

		}
		
		#region Connect And Close

		private void button1_Click( object sender, EventArgs e )
		{
			if (!byte.TryParse(textBox15.Text,out byte station))
			{
				DemoUtils.ShowMessage( "Station input wrong！" );
				return;
			}

			xGBCpu?.Close( );
			xGBCpu = new LSCpu( );
			xGBCpu.Station = station;
			xGBCpu.LogNet = LogNet;

			try
			{
				this.pipeSelectControl1.IniPipe( xGBCpu );
				OperateResult open = DeviceConnectPLC( xGBCpu );

				if (open.IsSuccess)
				{
					button2.Enabled = true;
					button1.Enabled = false;
					userControlReadWriteDevice1.SetEnable( true );

					// MB100;MB200;MW300

					// 设置子控件的读取能力
					userControlReadWriteDevice1.SetReadWriteNet( xGBCpu, "MB100", false );
					// 设置批量读取
					userControlReadWriteDevice1.BatchRead.SetReadWriteNet( xGBCpu, "MB100", string.Empty );

					// 设置报文读取
					userControlReadWriteDevice1.MessageRead.SetReadSourceBytes( m => xGBCpu.ReadFromCoreServer( m, true, false ), string.Empty, string.Empty );

					// 设置代码示例
					this.userControlReadWriteDevice1.SetDeviceVariableName( DemoUtils.PlcDeviceName );
					codeExampleControl.SetCodeText( xGBCpu, nameof( xGBCpu.Station ) );
				}
				else
				{
					DemoUtils.ShowMessage( StringResources.Language.ConnectedFailed + open.Message + Environment.NewLine +
						"Error: " + open.ErrorCode );
				}
			}
			catch (Exception ex)
			{
				DemoUtils.ShowMessage( ex.Message );
			}
		}

		private void button2_Click( object sender, EventArgs e )
		{
			// 断开连接
			xGBCpu.Close( );
			button2.Enabled = false;
			button1.Enabled = true;
			userControlReadWriteDevice1.SetEnable( false );
			this.pipeSelectControl1.ExtraCloseAction( xGBCpu );
		}

		#endregion

		public override void SaveXmlParameter( XElement element )
		{
			this.pipeSelectControl1.SaveXmlParameter( element );
			element.SetAttributeValue( DemoDeviceList.XmlStation, textBox15.Text );

			this.userControlReadWriteDevice1.GetDataTable( element );
		}

		public override void LoadXmlParameter( XElement element )
		{
			base.LoadXmlParameter( element );
			this.pipeSelectControl1.LoadXmlParameter( element, SettingPipe.SerialPipe );
			textBox15.Text = element.Attribute( DemoDeviceList.XmlStation ).Value;

			if (this.userControlReadWriteDevice1.LoadDataTable( element ) > 0)
				this.userControlReadWriteDevice1.SelectTabDataTable( );
		}

		private void userControlHead1_SaveConnectEvent_1( object sender, EventArgs e )
		{
			userControlHead1_SaveConnectEvent( sender, e );
		}
	}
}
