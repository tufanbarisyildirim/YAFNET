/* Yet Another Forum.net
 * Copyright (C) 2003 Bj�rnar Henden
 * http://www.yetanotherforum.net/
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 */

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace yaf.admin
{
	/// <summary>
	/// Summary description for main.
	/// </summary>
	public class main : AdminPage
	{
		protected System.Web.UI.WebControls.Repeater ActiveList, UserList;
		protected Label NumPosts,NumTopics,NumUsers,BoardStart,DayPosts,DayTopics,DayUsers;
		protected HtmlGenericControl UpgradeNotice;
	
		private void Page_Load(object sender, System.EventArgs e)
		{
			if(!IsPostBack) 
			{
				BindData();
				UpgradeNotice.Visible = install.GetCurrentVersion() < AppVersion;
			}
		}

		protected void Delete_Load(object sender, System.EventArgs e) 
		{
			((LinkButton)sender).Attributes["onclick"] = "return confirm('Delete this user?')";
		}

		private void BindData() 
		{
			ActiveList.DataSource = DB.active_list(true);
			UserList.DataSource = DB.user_list(null,false);
			DataBind();

			DataRow row = DB.board_stats();
			NumPosts.Text	= row["NumPosts"].ToString();
			NumTopics.Text	= row["NumTopics"].ToString();
			NumUsers.Text	= row["NumUsers"].ToString();

			TimeSpan span = DateTime.Now - (DateTime)row["BoardStart"];
			double days = span.Days;

			BoardStart.Text	= String.Format("{0:d} ({1:N0} days ago)",row["BoardStart"],days);

			if(days<1) days = 1;
			DayPosts.Text = String.Format("{0:N2}",(int)row["NumPosts"] / days);
			DayTopics.Text = String.Format("{0:N2}",(int)row["NumTopics"] / days);
			DayUsers.Text = String.Format("{0:N2}",(int)row["NumUsers"] / days);
		}

		private void UserList_ItemCommand(object source, System.Web.UI.WebControls.RepeaterCommandEventArgs e) 
		{
			switch(e.CommandName) 
			{
				case "edit":
					Response.Redirect(String.Format("edituser.aspx?u={0}",e.CommandArgument));
					break;
				case "delete":
					DB.user_delete(e.CommandArgument);
					AddLoadMessage("User deleted.");
					BindData();
					break;
			}
		}

		protected string FormatForumLink(object ForumID,object ForumName) 
		{
			if(ForumID.ToString()=="" || ForumName.ToString()=="")
				return "";

			return String.Format("<a target=\"_top\" href=\"../topics.aspx?f={0}\">{1}</a>",ForumID,ForumName);
		}

		protected string FormatTopicLink(object TopicID,object TopicName) {
			if(TopicID.ToString()=="" || TopicName.ToString()=="")
				return "";

			return String.Format("<a target=\"_top\" href=\"../posts.aspx?t={0}\">{1}</a>",TopicID,TopicName);
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			this.UserList.ItemCommand += new System.Web.UI.WebControls.RepeaterCommandEventHandler(this.UserList_ItemCommand);
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
			this.Load += new System.EventHandler(this.Page_Load);

		}
		#endregion
	}
}
