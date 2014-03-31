﻿// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Constants;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// Block for administrating a tag
    /// </summary>
    [DisplayName( "Tag Detail" )]
    [Category( "Core" )]
    [Description( "Block for administrating a tag." )]
    public partial class TagDetail : Rock.Web.UI.RockBlock
    {
        #region Fields

        private bool _canConfigure = false;

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _canConfigure = IsUserAuthorized( Authorization.ADMINISTRATE );

            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", Group.FriendlyTypeName );

        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                string itemId = PageParameter( "tagId" );
                string entityTypeId = PageParameter( "entityTypeId" );

                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    if ( string.IsNullOrWhiteSpace( entityTypeId ) )
                    {
                        ShowDetail( "tagId", int.Parse( itemId ) );
                    }
                    else
                    {
                        ShowDetail( "tagId", int.Parse( itemId ), int.Parse( entityTypeId ) );
                    }
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
            else
            {
                nbEditError.Visible = false;
            }
        }

        public override List<Rock.Web.UI.BreadCrumb> GetBreadCrumbs( Rock.Web.PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();
            
            string pageTitle = "New Tag";
            
            int? tagId = PageParameter( "tagId" ).AsInteger( false );
            if (tagId.HasValue)
            {
                Tag tag = new TagService().Get( tagId.Value );
                if (tag != null)
                {
                    pageTitle = tag.Name;
                    breadCrumbs.Add( new BreadCrumb( tag.Name, pageReference ) );
                }
            }

            RockPage.Title = pageTitle;

            return breadCrumbs;
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            var tag = new TagService().Get( int.Parse( hfId.Value ) );
            ShowEditDetails( tag );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            var tagService = new Rock.Model.TagService();
            var tag = tagService.Get( int.Parse( hfId.Value ) );

            if ( tag != null )
            {
                string errorMessage;
                if ( !tagService.CanDelete( tag, out errorMessage ) )
                {
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                tagService.Delete( tag, CurrentPersonAlias );
                tagService.Save( tag, CurrentPersonAlias );

                NavigateToParentPage();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var tagService = new Rock.Model.TagService();
            Tag tag = null;

            int tagId = int.Parse( hfId.Value );

            if ( tagId != 0 )
            {
                tag = tagService.Get( tagId );
            }

            if ( tag == null )
            {
                tag = new Tag();
                tag.IsSystem = false;
                tagService.Add( tag, CurrentPersonAlias );
            }

            string name = tbName.Text;
            int? ownerId = ppOwner.PersonId;
            int entityTypeId = ddlEntityType.SelectedValueAsId().Value;
            string qualifierCol = tbEntityTypeQualifierColumn.Text;
            string qualifierVal = tbEntityTypeQualifierValue.Text;

            // Verify tag with same name does not already exist
            if (tagService.Queryable()
                    .Where( t =>
                        t.Id != tagId &&
                        t.Name == name &&
                        t.OwnerId.Equals( ownerId ) &&
                        t.EntityTypeId == entityTypeId &&
                        t.EntityTypeQualifierColumn == qualifierCol &&
                        t.EntityTypeQualifierValue == qualifierVal )
                    .Any())
            {
                nbEditError.Heading = "Tag Already Exists";
                nbEditError.Text = string.Format("A '{0}' tag already exists for the selected scope, owner, and entity type.", name);
                nbEditError.Visible = true;
            }
            else
            {
                tag.Name = name;
                tag.Description = tbDescription.Text;
                tag.OwnerId = ownerId;
                tag.EntityTypeId = entityTypeId;
                tag.EntityTypeQualifierColumn = qualifierCol;
                tag.EntityTypeQualifierValue = qualifierVal;
                tagService.Save( tag, CurrentPersonAlias );

                var qryParams = new Dictionary<string, string>();
                qryParams["tagId"] = tag.Id.ToString();

                NavigateToPage( RockPage.Guid, qryParams );
            }

        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfId.Value.Equals( "0" ) )
            {
                NavigateToParentPage();
            }
            else
            {
                var tag = new TagService().Get( int.Parse( hfId.Value ) );
                ShowReadonlyDetails( tag );
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblScope control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rblScope_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( rblScope.SelectedValue == "Personal" && CurrentPerson != null )
            {
                ppOwner.SetValue( CurrentPerson );
                ppOwner.Visible = _canConfigure;
            }
            else
            {
                ppOwner.SetValue( null );
                ppOwner.Visible = false;
            }

        }
        #endregion

        #region Methods

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            ShowDetail( itemKey, itemKeyValue, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The group id.</param>
        public void ShowDetail( string itemKey, int itemKeyValue, int? entityTypeId )
        {
            pnlDetails.Visible = false;
            if ( !itemKey.Equals( "tagId" ) )
            {
                return;
            }

            Tag tag = null;

            if ( !itemKeyValue.Equals( 0 ) )
            {
                tag = new TagService().Get( itemKeyValue );
            }
            else
            {
                tag = new Tag { Id = 0, OwnerId = CurrentPersonId };
                if ( entityTypeId.HasValue )
                {
                    tag.EntityTypeId = entityTypeId.Value;
                }
            }

            if ( tag == null )
            {
                return;
            }

            pnlDetails.Visible = true;
            hfId.Value = tag.Id.ToString();

            bool readOnly = false;

            if ( !_canConfigure && tag.OwnerId != CurrentPersonId )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Tag.FriendlyTypeName );
            }

            if ( tag.IsSystem )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlySystem( Group.FriendlyTypeName );
            }

            if ( readOnly )
            {
                btnEdit.Visible = false;
                btnDelete.Visible = false;
                ShowReadonlyDetails( tag );
            }
            else
            {
                btnEdit.Visible = true;
                btnDelete.Visible = true;
                if ( tag.Id > 0 )
                {
                    ShowReadonlyDetails( tag );
                }
                else
                {
                    ShowEditDetails( tag );
                }
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="tag">The tag.</param>
        private void ShowEditDetails( Tag tag )
        {

            if ( tag.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( Tag.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lReadOnlyTitle.Text = tag.Name.FormatAsHtmlTitle();
            }

            SetEditMode( true );

            tbName.Text = tag.Name;
            tbDescription.Text = tag.Description;
            if ( tag.OwnerId.HasValue )
            {
                rblScope.SelectedValue = "Personal";
            }
            else
            {
                rblScope.SelectedValue = "Organization";
            }
            ppOwner.SetValue( tag.Owner );

            ddlEntityType.Items.Clear();
            new EntityTypeService().GetEntityListItems().ForEach( l => ddlEntityType.Items.Add( l ) );
            ddlEntityType.SelectedValue = tag.EntityTypeId.ToString();
            tbEntityTypeQualifierColumn.Text = tag.EntityTypeQualifierColumn;
            tbEntityTypeQualifierValue.Text = tag.EntityTypeQualifierValue;

            rblScope.Visible = _canConfigure;
            ppOwner.Visible = _canConfigure;
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="group">The group.</param>
        private void ShowReadonlyDetails( Tag tag )
        {
            SetEditMode( false );
            lReadOnlyTitle.Text = tag.Name.FormatAsHtmlTitle();
            lDescription.Text = tag.Description;
            hlEntityType.Text = tag.EntityType.FriendlyName;
        }

        #endregion

    }
}